using GenericCrud.Metadata;
using Microsoft.AspNetCore.Http;

namespace GenericCrud.Services
{
    public interface ICrudService
    {
        Task<List<object>> GetAllAsync(EntityMetadata metadata);
        Task<object?> GetByIdAsync(EntityMetadata metadata, object id);
        Task<object> CreateAsync(EntityMetadata metadata, IFormCollection form);
        Task<bool> UpdateAsync(EntityMetadata metadata, object id, IFormCollection form);
        Task<bool> DeleteAsync(EntityMetadata metadata, object id);

        /// <summary>Converts a route/query string id into the entity's actual primary key CLR type.</summary>
        object ConvertKey(EntityMetadata metadata, string id);
    }
}
