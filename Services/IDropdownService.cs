using GenericCrud.Metadata;

namespace GenericCrud.Services
{
    public class DropdownItem
    {
        public string Value { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
    }

    public interface IDropdownService
    {
        /// <summary>Loads Id/DisplayName pairs for a foreign key's principal entity, ordered by display name.</summary>
        Task<List<DropdownItem>> GetOptionsAsync(ForeignKeyMetadata fk);

        /// <summary>Loads a single principal record's display text, e.g. for showing FK value in list/details views.</summary>
        Task<string?> GetDisplayTextAsync(ForeignKeyMetadata fk, object? keyValue);
    }
}
