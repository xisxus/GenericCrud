using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using GenericCrud.Data;
using GenericCrud.Metadata;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace GenericCrud.Services
{
    public class EntityMetadataService : IEntityMetadataService
    {
        private readonly AppDbContext _context;

        // Columns that should never be shown/edited even if present on an entity.
        private static readonly string[] ExcludedColumnNames =
        {
            "Password", "PasswordHash", "ConcurrencyStamp", "SecurityStamp", "RowVersion"
        };

        // Candidate property names used as the "label" for a foreign key dropdown, in priority order.
        private static readonly string[] DisplayPropertyCandidates =
        {
            "Name", "Title", "Description", "Code"
        };

        // Metadata is expensive to build (reflection + EF walk) — cache per entity name for the app lifetime.
        private static readonly ConcurrentDictionary<string, EntityMetadata?> Cache = new(StringComparer.OrdinalIgnoreCase);

        public EntityMetadataService(AppDbContext context)
        {
            _context = context;
        }

        public List<string> GetAllEntityNames() =>
            _context.Model.GetEntityTypes()
                .Select(e => e.ClrType.Name)
                .OrderBy(n => n)
                .ToList();

        public EntityMetadata? GetEntityMetadata(string entityName)
        {
            if (string.IsNullOrWhiteSpace(entityName)) return null;

            return Cache.GetOrAdd(entityName, BuildMetadata);
        }

        private EntityMetadata? BuildMetadata(string entityName)
        {
            var efEntityType = _context.Model.GetEntityTypes()
                .FirstOrDefault(e => e.ClrType.Name.Equals(entityName, StringComparison.OrdinalIgnoreCase));

            if (efEntityType == null) return null;

            var clrType = efEntityType.ClrType;
            var pkEf = efEntityType.FindPrimaryKey()?.Properties.FirstOrDefault()
                ?? throw new InvalidOperationException($"Entity '{entityName}' has no primary key.");

            var metadata = new EntityMetadata
            {
                EntityName = clrType.Name,
                ClrType = clrType,
                TableName = efEntityType.GetTableName() ?? clrType.Name
            };

            // Foreign keys first, so scalar property building below can flag IsForeignKey.
            var foreignKeys = BuildForeignKeys(efEntityType);
            metadata.ForeignKeys = foreignKeys;

            foreach (var efProp in efEntityType.GetProperties())
            {
                if (ExcludedColumnNames.Any(x => efProp.Name.Equals(x, StringComparison.OrdinalIgnoreCase)))
                    continue;

                // Skip shadow properties with no CLR backing (rare, but defensive).
                var clrProp = clrType.GetProperty(efProp.Name);
                if (clrProp == null) continue;

                var fk = foreignKeys.FirstOrDefault(f => f.PropertyName.Equals(efProp.Name, StringComparison.OrdinalIgnoreCase));

                var propMeta = new PropertyMetadata
                {
                    Name = efProp.Name,
                    DisplayName = GetDisplayName(clrProp, efProp.Name),
                    ClrType = efProp.ClrType,
                    IsPrimaryKey = efProp.IsPrimaryKey(),
                    IsIdentity = efProp.ValueGenerated == ValueGenerated.OnAdd,
                    IsForeignKey = fk != null,
                    ForeignKey = fk,
                    IsNullable = efProp.IsNullable,
                    IsRequired = !efProp.IsNullable && !efProp.IsPrimaryKey(),
                    MaxLength = efProp.GetMaxLength()
                };

                ApplyRangeAnnotation(clrProp, propMeta);
                propMeta.InputType = DetermineInputType(propMeta);

                metadata.Properties.Add(propMeta);
            }

            metadata.PrimaryKey = metadata.Properties.First(p => p.IsPrimaryKey);

            return metadata;
        }

        private List<ForeignKeyMetadata> BuildForeignKeys(IEntityType efEntityType)
        {
            var result = new List<ForeignKeyMetadata>();

            foreach (var fk in efEntityType.GetForeignKeys())
            {
                var fkProperty = fk.Properties.FirstOrDefault();
                if (fkProperty == null) continue;

                var principalType = fk.PrincipalEntityType;
                var principalKeyProp = fk.PrincipalKey.Properties.FirstOrDefault();
                if (principalKeyProp == null) continue;

                var displayProperty = DisplayPropertyCandidates
                    .Select(candidate => principalType.ClrType.GetProperty(candidate))
                    .FirstOrDefault(p => p != null)?.Name;

                // Fallback: first string property that isn't the PK.
                if (displayProperty == null)
                {
                    displayProperty = principalType.GetProperties()
                        .Where(p => p.ClrType == typeof(string) && !p.IsPrimaryKey())
                        .Select(p => p.Name)
                        .FirstOrDefault() ?? principalKeyProp.Name;
                }

                result.Add(new ForeignKeyMetadata
                {
                    PropertyName = fkProperty.Name,
                    PrincipalEntityName = principalType.ClrType.Name,
                    PrincipalClrType = principalType.ClrType,
                    PrincipalKeyName = principalKeyProp.Name,
                    DisplayPropertyName = displayProperty,
                    IsNullable = fkProperty.IsNullable
                });
            }

            return result;
        }

        private static string GetDisplayName(System.Reflection.PropertyInfo clrProp, string fallback)
        {
            var display = clrProp.GetCustomAttributes(typeof(DisplayAttribute), true)
                .OfType<DisplayAttribute>()
                .FirstOrDefault();

            if (display?.Name != null) return display.Name;

            // Split "DepartmentId" -> "Department Id" as a readable fallback.
            return System.Text.RegularExpressions.Regex.Replace(fallback, "([a-z])([A-Z])", "$1 $2");
        }

        private static void ApplyRangeAnnotation(System.Reflection.PropertyInfo clrProp, PropertyMetadata propMeta)
        {
            var range = clrProp.GetCustomAttributes(typeof(RangeAttribute), true)
                .OfType<RangeAttribute>()
                .FirstOrDefault();

            if (range == null) return;

            if (double.TryParse(range.Minimum.ToString(), out var min)) propMeta.RangeMin = min;
            if (double.TryParse(range.Maximum.ToString(), out var max)) propMeta.RangeMax = max;
        }

        private static CrudInputType DetermineInputType(PropertyMetadata prop)
        {
            if (prop.IsForeignKey) return CrudInputType.Dropdown;

            var t = prop.UnderlyingType;

            if (t == typeof(bool)) return CrudInputType.Checkbox;
            if (t == typeof(byte[])) return CrudInputType.File;
            if (t.IsEnum) return CrudInputType.Dropdown;
            if (t == typeof(DateTime)) return CrudInputType.Date;
            if (t == typeof(DateOnly)) return CrudInputType.Date;
            if (t == typeof(TimeOnly)) return CrudInputType.Time;
            if (t == typeof(decimal) || t == typeof(double) || t == typeof(float)) return CrudInputType.Decimal;
            if (t == typeof(int) || t == typeof(long) || t == typeof(short)) return CrudInputType.Number;
            if (t == typeof(Guid)) return CrudInputType.Text;

            if (t == typeof(string) && (prop.MaxLength == null || prop.MaxLength > 300))
                return CrudInputType.TextArea;

            return CrudInputType.Text;
        }
    }
}
