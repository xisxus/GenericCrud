using GenericCrud.Metadata;
using GenericCrud.Services;

namespace GenericCrud.ViewModels
{
    public class CrudFormViewModel
    {
        public string EntityName { get; set; } = string.Empty;
        public EntityMetadata Metadata { get; set; } = default!;
        public bool IsEdit { get; set; }
        public string? Id { get; set; }

        /// <summary>Current value for each property, keyed by property name (empty for Create).</summary>
        public Dictionary<string, string?> Values { get; set; } = new();

        /// <summary>Dropdown options for each FK property, keyed by the FK's scalar property name.</summary>
        public Dictionary<string, List<DropdownItem>> DropdownOptions { get; set; } = new();

        public List<string> Errors { get; set; } = new();
    }
}
