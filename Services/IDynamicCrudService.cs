using GenericCrud.Metadata.Dynamic;
using Microsoft.AspNetCore.Http;

namespace GenericCrud.Services
{
    public interface IDynamicCrudService
    {
        Task<(List<Dictionary<string, object?>> Rows, int TotalCount)> GetAllAsync(
            DynamicEntityMetadata metadata, string? search, string? sortColumn, string? sortDirection, int page, int pageSize);

        Task<Dictionary<string, object?>?> GetByIdAsync(DynamicEntityMetadata metadata, string id);

        /// <summary>Returns field-name -> error-messages. Empty dictionary means success.</summary>
        Task<Dictionary<string, List<string>>> CreateAsync(DynamicEntityMetadata metadata, IFormCollection form);

        /// <summary>Returns field-name -> error-messages. Empty dictionary means success.</summary>
        Task<Dictionary<string, List<string>>> UpdateAsync(DynamicEntityMetadata metadata, string id, IFormCollection form);

        Task<bool> DeleteAsync(DynamicEntityMetadata metadata, string id);

        Task<List<(string Value, string Text)>> GetForeignKeyOptionsAsync(DynamicForeignKeyMetadata foreignKey);
    }
}
