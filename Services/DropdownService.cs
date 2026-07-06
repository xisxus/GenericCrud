using System.Linq;
using GenericCrud.Data;
using GenericCrud.Metadata;
using Microsoft.EntityFrameworkCore;

namespace GenericCrud.Services
{
    public class DropdownService : IDropdownService
    {
        private readonly AppDbContext _context;

        public DropdownService(AppDbContext context)
        {
            _context = context;
        }

        public Task<List<DropdownItem>> GetOptionsAsync(ForeignKeyMetadata fk)
        {
            var keyProp = fk.PrincipalClrType.GetProperty(fk.PrincipalKeyName)!;
            var displayProp = fk.PrincipalClrType.GetProperty(fk.DisplayPropertyName)!;

            // Materializing this executes "SELECT * FROM <PrincipalTable>" for the principal entity.
            var query = GetQueryable(fk.PrincipalClrType);
            var rows = query.Cast<object>().ToList();

            var items = rows
                .Select(row => new DropdownItem
                {
                    Value = keyProp.GetValue(row)?.ToString() ?? string.Empty,
                    Text = displayProp.GetValue(row)?.ToString() ?? string.Empty
                })
                .OrderBy(i => i.Text)
                .ToList();

            return Task.FromResult(items);
        }

        public Task<string?> GetDisplayTextAsync(ForeignKeyMetadata fk, object? keyValue)
        {
            if (keyValue == null) return Task.FromResult<string?>(null);

            var convertedKey = ConvertKey(keyValue, fk.PrincipalClrType.GetProperty(fk.PrincipalKeyName)!.PropertyType);
            var entity = _context.Find(fk.PrincipalClrType, convertedKey);
            if (entity == null) return Task.FromResult<string?>(null);

            var displayProp = fk.PrincipalClrType.GetProperty(fk.DisplayPropertyName)!;
            return Task.FromResult(displayProp.GetValue(entity)?.ToString());
        }

        private static object ConvertKey(object value, Type targetType)
        {
            if (value.GetType() == targetType) return value;
            return Convert.ChangeType(value, targetType);
        }

        // DbContext only exposes Set<TEntity>() generically — invoke it via reflection for a runtime Type.
        private IQueryable GetQueryable(Type clrType)
        {
            var setMethod = typeof(DbContext)
                .GetMethod(nameof(DbContext.Set), 1, Type.EmptyTypes)!
                .MakeGenericMethod(clrType);

            return (IQueryable)setMethod.Invoke(_context, null)!;
        }
    }
}
