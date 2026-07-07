using GenericCrud.Metadata.Dynamic;
using GenericCrud.Models.Dynamic;

namespace GenericCrud.Services
{
    public interface IDynamicMetadataService
    {
        Task<DynamicEntityMetadata?> GetEntityMetadataAsync(string entityName);
        Task<List<DynamicEntity>> GetAllEntityConfigsAsync();

        void InvalidateCache(string entityName);
        void InvalidateAllCache();
    }
}
