using System.Globalization;
using System.Linq;
using GenericCrud.Data;
using GenericCrud.Metadata;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace GenericCrud.Services
{
    public class CrudService : ICrudService
    {
        private readonly AppDbContext _context;
        private readonly IReflectionService _reflectionService;

        public CrudService(AppDbContext context, IReflectionService reflectionService)
        {
            _context = context;
            _reflectionService = reflectionService;
        }

        public Task<List<object>> GetAllAsync(EntityMetadata metadata)
        {
            var query = GetQueryable(metadata.ClrType);
            var rows = query.Cast<object>().ToList();
            return Task.FromResult(rows);
        }

        public Task<object?> GetByIdAsync(EntityMetadata metadata, object id)
        {
            var entity = _context.Find(metadata.ClrType, id);
            return Task.FromResult(entity);
        }

        public async Task<object> CreateAsync(EntityMetadata metadata, IFormCollection form)
        {
            var entity = _reflectionService.CreateInstance(metadata, form);
            _context.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> UpdateAsync(EntityMetadata metadata, object id, IFormCollection form)
        {
            var entity = _context.Find(metadata.ClrType, id);
            if (entity == null) return false;

            _reflectionService.ApplyFormValues(metadata, entity, form);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(EntityMetadata metadata, object id)
        {
            var entity = _context.Find(metadata.ClrType, id);
            if (entity == null) return false;

            _context.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public object ConvertKey(EntityMetadata metadata, string id)
        {
            var keyType = metadata.PrimaryKey.UnderlyingType;

            if (keyType == typeof(int)) return int.Parse(id, CultureInfo.InvariantCulture);
            if (keyType == typeof(long)) return long.Parse(id, CultureInfo.InvariantCulture);
            if (keyType == typeof(short)) return short.Parse(id, CultureInfo.InvariantCulture);
            if (keyType == typeof(Guid)) return Guid.Parse(id);
            if (keyType == typeof(string)) return id;

            return Convert.ChangeType(id, keyType, CultureInfo.InvariantCulture);
        }

        // DbContext only exposes Set<TEntity>() generically — there is no Set(Type) overload.
        // Since the entity type is only known at runtime here, invoke the generic method via reflection.
        private IQueryable GetQueryable(Type clrType)
        {
            var setMethod = typeof(DbContext)
                .GetMethod(nameof(DbContext.Set), 1, Type.EmptyTypes)!
                .MakeGenericMethod(clrType);

            return (IQueryable)setMethod.Invoke(_context, null)!;
        }
    }
}
