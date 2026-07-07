using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;
using GenericCrud.Data;
using System.Linq;
using GenericCrud.Models.Dynamic;
using GenericCrud.Metadata.Dynamic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace GenericCrud.Services
{
    // Executes CRUD directly against whatever physical table DynamicEntity.EntityName points at,
    // using raw parameterized ADO.NET — there is no C# POCO for these entities, only config rows.
    public class DynamicCrudService : IDynamicCrudService
    {
        private static readonly Regex IdentifierRegex = new("^[A-Za-z_][A-Za-z0-9_]*$", RegexOptions.Compiled);

        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public DynamicCrudService(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<(List<Dictionary<string, object?>> Rows, int TotalCount)> GetAllAsync(
            DynamicEntityMetadata metadata, string? search, string? sortColumn, string? sortDirection, int page, int pageSize)
        {
            var conn = await GetOpenConnectionAsync();
            var table = Quote(metadata.EntityName);

            var conditions = new List<string>();
            var parms = new List<(string Name, object Value)>();

            if (metadata.SoftDelete)
                conditions.Add($"{Quote(metadata.SoftDeleteColumn)} = 0");

            var searchable = metadata.Fields
                .Where(f => f.ShowInTable && f.InputType is DynamicInputType.Text
                    or DynamicInputType.TextArea
                    or DynamicInputType.Email
                    or DynamicInputType.Phone)
                .ToList();

            if (!string.IsNullOrWhiteSpace(search) && searchable.Count > 0)
            {
                var ors = new List<string>();
                for (var i = 0; i < searchable.Count; i++)
                {
                    ors.Add($"{Quote(searchable[i].FieldName)} LIKE @s{i}");
                    parms.Add(($"@s{i}", $"%{search}%"));
                }
                conditions.Add("(" + string.Join(" OR ", ors) + ")");
            }

            var whereSql = conditions.Count > 0 ? "WHERE " + string.Join(" AND ", conditions) : "";

            var sortCol = !string.IsNullOrWhiteSpace(sortColumn) && metadata.Fields.Any(f => f.FieldName == sortColumn)
                ? sortColumn!
                : (metadata.DefaultSortColumn ?? metadata.PrimaryKeyColumn);
            var direction = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase) ? "DESC" : "ASC";

            var total = Convert.ToInt32(await ExecuteScalarAsync(conn, $"SELECT COUNT(*) FROM {table} {whereSql}", parms));

            page = page < 1 ? 1 : page;
            pageSize = pageSize <= 0 ? 10 : pageSize;
            var offset = (page - 1) * pageSize;

            var dataSql = $"SELECT * FROM {table} {whereSql} ORDER BY {Quote(sortCol)} {direction} " +
                           $"OFFSET {offset} ROWS FETCH NEXT {pageSize} ROWS ONLY";

            var rows = await ExecuteRowsAsync(conn, dataSql, parms);
            return (rows, total);
        }

        public async Task<Dictionary<string, object?>?> GetByIdAsync(DynamicEntityMetadata metadata, string id)
        {
            var conn = await GetOpenConnectionAsync();
            var sql = $"SELECT * FROM {Quote(metadata.EntityName)} WHERE {Quote(metadata.PrimaryKeyColumn)} = @id";
            var rows = await ExecuteRowsAsync(conn, sql, new List<(string, object)> { ("@id", ConvertKeyValue(id)) });
            return rows.FirstOrDefault();
        }

        public async Task<Dictionary<string, List<string>>> CreateAsync(DynamicEntityMetadata metadata, IFormCollection form)
        {
            var errors = new Dictionary<string, List<string>>();
            var columnValues = new Dictionary<string, object?>();

            foreach (var field in metadata.FormFields)
            {
                if (field.InputType == DynamicInputType.ReadOnly) continue;
                if (!IsConditionMet(field, form)) continue;

                if (field.InputType == DynamicInputType.File)
                {
                    var file = form.Files[field.FieldName];
                    ValidateField(field, null, file, errors);
                    if (file != null && !errors.ContainsKey(field.FieldName))
                        columnValues[field.FieldName] = await SaveFileAsync(field, file);
                    continue;
                }

                var raw = form[field.FieldName].FirstOrDefault();
                ValidateField(field, raw, null, errors);
                columnValues[field.FieldName] = ConvertValue(field, raw);
            }

            if (errors.Count > 0) return errors;

            var conn = await GetOpenConnectionAsync();
            var cols = columnValues.Keys.ToList();
            var colList = string.Join(", ", cols.Select(Quote));
            var paramList = string.Join(", ", cols.Select((_, i) => $"@p{i}"));
            var sql = $"INSERT INTO {Quote(metadata.EntityName)} ({colList}) VALUES ({paramList});";

            await using var cmd = new SqlCommand(sql, conn);
            for (var i = 0; i < cols.Count; i++)
                cmd.Parameters.AddWithValue($"@p{i}", columnValues[cols[i]] ?? (object)DBNull.Value);

            await cmd.ExecuteNonQueryAsync();
            return errors;
        }

        public async Task<Dictionary<string, List<string>>> UpdateAsync(DynamicEntityMetadata metadata, string id, IFormCollection form)
        {
            var errors = new Dictionary<string, List<string>>();
            var columnValues = new Dictionary<string, object?>();

            foreach (var field in metadata.FormFields)
            {
                if (field.InputType == DynamicInputType.ReadOnly) continue;
                if (!IsConditionMet(field, form)) continue;

                if (field.InputType == DynamicInputType.File)
                {
                    var file = form.Files[field.FieldName];
                    if (file == null) continue; // no new file chosen -> keep existing value
                    ValidateField(field, null, file, errors);
                    if (!errors.ContainsKey(field.FieldName))
                        columnValues[field.FieldName] = await SaveFileAsync(field, file);
                    continue;
                }

                var raw = form[field.FieldName].FirstOrDefault();
                ValidateField(field, raw, null, errors);
                columnValues[field.FieldName] = ConvertValue(field, raw);
            }

            if (errors.Count > 0 || columnValues.Count == 0) return errors;

            var conn = await GetOpenConnectionAsync();
            var cols = columnValues.Keys.ToList();
            var setSql = string.Join(", ", cols.Select((c, i) => $"{Quote(c)} = @p{i}"));
            var sql = $"UPDATE {Quote(metadata.EntityName)} SET {setSql} WHERE {Quote(metadata.PrimaryKeyColumn)} = @id";

            await using var cmd = new SqlCommand(sql, conn);
            for (var i = 0; i < cols.Count; i++)
                cmd.Parameters.AddWithValue($"@p{i}", columnValues[cols[i]] ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@id", ConvertKeyValue(id));

            await cmd.ExecuteNonQueryAsync();
            return errors;
        }

        public async Task<bool> DeleteAsync(DynamicEntityMetadata metadata, string id)
        {
            var conn = await GetOpenConnectionAsync();
            var sql = metadata.SoftDelete
                ? $"UPDATE {Quote(metadata.EntityName)} SET {Quote(metadata.SoftDeleteColumn)} = 1 WHERE {Quote(metadata.PrimaryKeyColumn)} = @id"
                : $"DELETE FROM {Quote(metadata.EntityName)} WHERE {Quote(metadata.PrimaryKeyColumn)} = @id";

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", ConvertKeyValue(id));
            var affected = await cmd.ExecuteNonQueryAsync();
            return affected > 0;
        }

        public async Task<List<(string Value, string Text)>> GetForeignKeyOptionsAsync(DynamicForeignKeyMetadata foreignKey)
        {
            var conn = await GetOpenConnectionAsync();
            var orderBy = !string.IsNullOrWhiteSpace(foreignKey.OrderByColumn) ? foreignKey.OrderByColumn! : foreignKey.TextColumn;
            var sql = $"SELECT {Quote(foreignKey.ValueColumn)} AS OptValue, {Quote(foreignKey.TextColumn)} AS OptText " +
                      $"FROM {Quote(foreignKey.ForeignTableName)} ORDER BY {Quote(orderBy)}";

            var rows = await ExecuteRowsAsync(conn, sql, new List<(string, object)>());
            return rows.Select(r => (r["OptValue"]?.ToString() ?? "", r["OptText"]?.ToString() ?? "")).ToList();
        }

        // ---------- validation ----------

        private static void ValidateField(DynamicFieldMetadata field, string? raw, IFormFile? file, Dictionary<string, List<string>> errors)
        {
            var validation = field.Validation;

            bool isEmpty = field.InputType switch
            {
                DynamicInputType.File => file == null,
                DynamicInputType.Checkbox => !(raw == "true" || raw == "on" || raw == "1"),
                _ => string.IsNullOrWhiteSpace(raw)
            };

            if (field.IsRequired && isEmpty)
            {
                AddError(errors, field.FieldName, validation?.ErrorMessage ?? $"{field.Label} is required.");
                return;
            }
            if (isEmpty) return;

            switch (field.InputType)
            {
                case DynamicInputType.Text:
                case DynamicInputType.TextArea:
                case DynamicInputType.Email:
                case DynamicInputType.Phone:
                case DynamicInputType.Password:
                    if (validation?.MinLength is int minL && raw!.Length < minL)
                        AddError(errors, field.FieldName, validation.ErrorMessage ?? $"{field.Label} must be at least {minL} characters.");
                    if (validation?.MaxLength is int maxL && raw!.Length > maxL)
                        AddError(errors, field.FieldName, validation.ErrorMessage ?? $"{field.Label} must be at most {maxL} characters.");
                    if (!string.IsNullOrEmpty(validation?.Pattern) && !Regex.IsMatch(raw!, validation.Pattern))
                        AddError(errors, field.FieldName, validation.ErrorMessage ?? $"{field.Label} format is invalid.");
                    if (field.InputType == DynamicInputType.Email && !Regex.IsMatch(raw!, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                        AddError(errors, field.FieldName, $"{field.Label} must be a valid email address.");
                    break;

                case DynamicInputType.Number:
                    if (!long.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var lv))
                        AddError(errors, field.FieldName, $"{field.Label} must be a whole number.");
                    else
                    {
                        if (validation?.MinValue is decimal mn && lv < mn) AddError(errors, field.FieldName, validation.ErrorMessage ?? $"{field.Label} must be >= {mn}.");
                        if (validation?.MaxValue is decimal mx && lv > mx) AddError(errors, field.FieldName, validation.ErrorMessage ?? $"{field.Label} must be <= {mx}.");
                    }
                    break;

                case DynamicInputType.Decimal:
                    if (!decimal.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out var dv))
                        AddError(errors, field.FieldName, $"{field.Label} must be a number.");
                    else
                    {
                        if (validation?.MinValue is decimal dmn && dv < dmn) AddError(errors, field.FieldName, validation.ErrorMessage ?? $"{field.Label} must be >= {dmn}.");
                        if (validation?.MaxValue is decimal dmx && dv > dmx) AddError(errors, field.FieldName, validation.ErrorMessage ?? $"{field.Label} must be <= {dmx}.");
                    }
                    break;

                case DynamicInputType.Date:
                    if (!DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                        AddError(errors, field.FieldName, $"{field.Label} must be a valid date.");
                    break;

                case DynamicInputType.Time:
                    if (!TimeSpan.TryParse(raw, CultureInfo.InvariantCulture, out _))
                        AddError(errors, field.FieldName, $"{field.Label} must be a valid time.");
                    break;

                case DynamicInputType.File:
                    if (file != null)
                    {
                        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                        var allowedExtensions = (field.FileConfig?.AllowedExtensions ?? string.Empty)
                            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                            .Select(e => e.ToLowerInvariant())
                            .ToList();

                        if (allowedExtensions.Count > 0 && !allowedExtensions.Contains(ext))
                            AddError(errors, field.FieldName, $"{field.Label}: file type '{ext}' is not allowed.");

                        var maxKb = field.Validation?.MaxFileSizeKb ?? field.FileConfig?.MaxSizeKb ?? 2048;
                        if (file.Length > maxKb * 1024L)
                            AddError(errors, field.FieldName, $"{field.Label}: file exceeds the {maxKb} KB limit.");
                    }
                    break;
            }
        }

        private static void AddError(Dictionary<string, List<string>> errors, string field, string message)
        {
            if (!errors.TryGetValue(field, out var list))
            {
                list = new List<string>();
                errors[field] = list;
            }
            list.Add(message);
        }

        private static bool IsConditionMet(DynamicFieldMetadata field, IFormCollection form)
        {
            if (string.IsNullOrEmpty(field.ConditionalOnFieldName)) return true;
            var triggerValue = form[field.ConditionalOnFieldName].ToString();
            return string.Equals(triggerValue, field.ConditionalOnValue, StringComparison.OrdinalIgnoreCase);
        }

        private static object? ConvertValue(DynamicFieldMetadata field, string? raw)
        {
            switch (field.InputType)
            {
                case DynamicInputType.Number:
                    return string.IsNullOrWhiteSpace(raw) ? null : long.Parse(raw, CultureInfo.InvariantCulture);
                case DynamicInputType.Decimal:
                    return string.IsNullOrWhiteSpace(raw) ? null : decimal.Parse(raw, NumberStyles.Any, CultureInfo.InvariantCulture);
                case DynamicInputType.Date:
                    return string.IsNullOrWhiteSpace(raw) ? null : DateTime.Parse(raw, CultureInfo.InvariantCulture).Date;
                case DynamicInputType.Time:
                    return string.IsNullOrWhiteSpace(raw) ? null : TimeSpan.Parse(raw, CultureInfo.InvariantCulture);
                case DynamicInputType.Checkbox:
                    return raw == "true" || raw == "on" || raw == "1";
                case DynamicInputType.Dropdown:
                case DynamicInputType.Radio:
                    if (field.ForeignKey != null && int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var fkInt))
                        return fkInt;
                    return string.IsNullOrEmpty(raw) ? null : raw;
                default:
                    return string.IsNullOrEmpty(raw) ? null : raw;
            }
        }

        private async Task<string> SaveFileAsync(DynamicFieldMetadata field, IFormFile file)
        {
            var folder = field.FileConfig?.SaveFolder ?? "uploads";
            var physicalFolder = Path.Combine(_env.WebRootPath, folder);
            Directory.CreateDirectory(physicalFolder);

            var ext = Path.GetExtension(file.FileName);
            var fileName = (field.FileConfig?.RenameToGuid ?? true)
                ? $"{Guid.NewGuid():N}{ext}"
                : Path.GetFileName(file.FileName);

            var fullPath = Path.Combine(physicalFolder, fileName);
            await using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return "/" + string.Join("/", folder.Replace("\\", "/").Trim('/'), fileName);
        }

        // ---------- low-level ADO.NET helpers ----------

        private async Task<SqlConnection> GetOpenConnectionAsync()
        {
            var conn = (SqlConnection)_context.Database.GetDbConnection();
            if (conn.State != ConnectionState.Open) await conn.OpenAsync();
            return conn;
        }

        private static async Task<object?> ExecuteScalarAsync(SqlConnection conn, string sql, List<(string Name, object Value)> parms)
        {
            await using var cmd = new SqlCommand(sql, conn);
            foreach (var p in parms) cmd.Parameters.AddWithValue(p.Name, p.Value ?? DBNull.Value);
            return await cmd.ExecuteScalarAsync();
        }

        private static async Task<List<Dictionary<string, object?>>> ExecuteRowsAsync(SqlConnection conn, string sql, List<(string Name, object Value)> parms)
        {
            var results = new List<Dictionary<string, object?>>();
            await using var cmd = new SqlCommand(sql, conn);
            foreach (var p in parms) cmd.Parameters.AddWithValue(p.Name, p.Value ?? DBNull.Value);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object?>();
                for (var i = 0; i < reader.FieldCount; i++)
                {
                    var value = reader.GetValue(i);
                    row[reader.GetName(i)] = value == DBNull.Value ? null : value;
                }
                results.Add(row);
            }
            return results;
        }

        private static object ConvertKeyValue(string id) => int.TryParse(id, out var i) ? i : id;

        private static string Quote(string identifier)
        {
            if (string.IsNullOrWhiteSpace(identifier) || !IdentifierRegex.IsMatch(identifier))
                throw new InvalidOperationException($"Invalid identifier in dynamic CRUD config: '{identifier}'.");
            return "[" + identifier + "]";
        }
    }
}
