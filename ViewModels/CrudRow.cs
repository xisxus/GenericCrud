using GenericCrud.Metadata;
using GenericCrud.Services;

namespace GenericCrud.ViewModels
{
    public class CrudRow2
    {
        /// <summary>Primary key value as string.</summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>Column name -> display-ready value.</summary>
        public Dictionary<string, string> Values { get; set; } = new();
    }

    public class CrudListViewModel2
    {
        public string EntityName { get; set; } = string.Empty;
        public EntityMetadata Metadata { get; set; } = default!;
        public List<CrudRow2> Rows { get; set; } = new();

        /// <summary>
        /// Dropdown options for FK fields rendered in the left-side form on the master setup page.
        /// Keyed by the scalar FK property name (e.g. "DepartmentId").
        /// </summary>
        public Dictionary<string, List<DropdownItem>> FormDropdownOptions { get; set; } = new();
    }
}