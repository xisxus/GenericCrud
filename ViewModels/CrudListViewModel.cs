using GenericCrud.Metadata;

namespace GenericCrud.ViewModels
{
    public class CrudRow
    {
        /// <summary>Primary key value as string (used for building Edit/Delete/Details links).</summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>Column name -> display-ready value (FK columns already resolved to their display text).</summary>
        public Dictionary<string, string> Values { get; set; } = new();
    }

    public class CrudListViewModel
    {
        public string EntityName { get; set; } = string.Empty;
        public EntityMetadata Metadata { get; set; } = default!;
        public List<CrudRow> Rows { get; set; } = new();
    }
}
