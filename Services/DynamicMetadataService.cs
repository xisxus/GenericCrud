using GenericCrud.Data;
using GenericCrud.Metadata.Dynamic;
using GenericCrud.Models.Dynamic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace GenericCrud.Services
{
    public class DynamicMetadataService : IDynamicMetadataService
    {
        private const string CachePrefix = "dyn_meta_";
        private const string AllEntitiesCacheKey = "dyn_meta_all_entities";
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30);

        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;

        public DynamicMetadataService(AppDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<DynamicEntityMetadata?> GetEntityMetadataAsync(string entityName)
        {
            var cacheKey = CachePrefix + entityName.Trim().ToLowerInvariant();

            if (_cache.TryGetValue(cacheKey, out DynamicEntityMetadata? cached))
                return cached;

            var entity = await _context.DynamicEntities
                .Include(e => e.Sections)
                .Include(e => e.Fields).ThenInclude(f => f.Validation)
                .Include(e => e.Fields).ThenInclude(f => f.Options)
                .Include(e => e.Fields).ThenInclude(f => f.ForeignKey)
                .Include(e => e.Fields).ThenInclude(f => f.FileConfig)
                .AsSplitQuery()
                .FirstOrDefaultAsync(e => e.IsActive && e.EntityName == entityName);

            if (entity == null)
            {
                // don't cache misses forever, but avoid hammering the DB either
                _cache.Set(cacheKey, (DynamicEntityMetadata?)null, TimeSpan.FromSeconds(30));
                return null;
            }

            var metadata = BuildMetadata(entity);
            _cache.Set(cacheKey, metadata, CacheDuration);
            return metadata;
        }

        public async Task<List<DynamicEntity>> GetAllEntityConfigsAsync()
        {
            if (_cache.TryGetValue(AllEntitiesCacheKey, out List<DynamicEntity>? cached) && cached != null)
                return cached;

            var entities = await _context.DynamicEntities
                .OrderBy(e => e.PageTitle)
                .ToListAsync();

            _cache.Set(AllEntitiesCacheKey, entities, CacheDuration);
            return entities;
        }

        public void InvalidateCache(string entityName)
        {
            _cache.Remove(CachePrefix + entityName.Trim().ToLowerInvariant());
            _cache.Remove(AllEntitiesCacheKey);
        }

        public void InvalidateAllCache()
        {
            // Individual keys aren't tracked centrally by IMemoryCache, so callers should
            // pair broad config changes with an explicit InvalidateCache(entityName) call too.
            _cache.Remove(AllEntitiesCacheKey);
        }

        private static DynamicEntityMetadata BuildMetadata(DynamicEntity entity)
        {
            var metadata = new DynamicEntityMetadata
            {
                Id = entity.Id,
                EntityName = entity.EntityName,
                PageTitle = entity.PageTitle,
                PrimaryKeyColumn = entity.PrimaryKeyColumn,
                SoftDelete = entity.SoftDelete,
                SoftDeleteColumn = entity.SoftDeleteColumn,
                DefaultSortColumn = entity.DefaultSortColumn,
                DefaultSortDirection = entity.DefaultSortDirection,
                PageSize = entity.PageSize <= 0 ? 10 : entity.PageSize,
                Sections = entity.Sections
                    .OrderBy(s => s.SortOrder)
                    .Select(s => new DynamicFieldSectionMetadata { Id = s.Id, Title = s.Title, SortOrder = s.SortOrder })
                    .ToList()
            };

            metadata.Fields = entity.Fields
                .Where(f => f.IsActive)
                .Select(f => new DynamicFieldMetadata
                {
                    Id = f.Id,
                    FieldName = f.FieldName,
                    Label = f.Label,
                    InputType = f.InputType,
                    FormOrder = f.FormOrder,
                    TableOrder = f.TableOrder,
                    ShowInForm = f.ShowInForm,
                    ShowInTable = f.ShowInTable,
                    IsRequired = f.IsRequired,
                    DefaultValue = f.DefaultValue,
                    SectionId = f.DynamicFieldSectionId,
                    ConditionalOnFieldName = f.ConditionalOnFieldName,
                    ConditionalOnValue = f.ConditionalOnValue,
                    Validation = f.Validation == null ? null : new DynamicFieldValidationMetadata
                    {
                        MinLength = f.Validation.MinLength,
                        MaxLength = f.Validation.MaxLength,
                        MinValue = f.Validation.MinValue,
                        MaxValue = f.Validation.MaxValue,
                        Pattern = f.Validation.Pattern,
                        MaxFileSizeKb = f.Validation.MaxFileSizeKb,
                        ErrorMessage = f.Validation.ErrorMessage
                    },
                    Options = f.Options
                        .OrderBy(o => o.SortOrder)
                        .Select(o => new DynamicFieldOptionMetadata { Value = o.OptionValue, Text = o.OptionText })
                        .ToList(),
                    ForeignKey = f.ForeignKey == null ? null : new DynamicForeignKeyMetadata
                    {
                        ForeignTableName = f.ForeignKey.ForeignTableName,
                        ValueColumn = f.ForeignKey.ValueColumn,
                        TextColumn = f.ForeignKey.TextColumn,
                        OrderByColumn = f.ForeignKey.OrderByColumn
                    },
                    FileConfig = f.FileConfig == null ? null : new DynamicFileConfigMetadata
                    {
                        SaveFolder = f.FileConfig.SaveFolder,
                        AllowedExtensions = f.FileConfig.AllowedExtensions,
                        MaxSizeKb = f.FileConfig.MaxSizeKb,
                        RenameToGuid = f.FileConfig.RenameToGuid
                    }
                })
                .ToList();

            return metadata;
        }
    }
}
