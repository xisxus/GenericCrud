using GenericCrud.Metadata.Dynamic;

namespace GenericCrud.ViewModels.Dynamic
{
    public class DynamicListViewModel
    {
        public DynamicEntityMetadata Metadata { get; set; } = null!;
        public List<Dictionary<string, object?>> Rows { get; set; } = new();

        // FieldName -> (raw key -> display text), used to render FK columns as text instead of ids.
        public Dictionary<string, Dictionary<string, string>> FkDisplay { get; set; } = new();

        public string? Search { get; set; }
        public string? SortColumn { get; set; }
        public string? SortDirection { get; set; }
        public int Page { get; set; } = 1;
        public int TotalCount { get; set; }

        public int TotalPages => Metadata.PageSize > 0 ? Math.Max(1, (int)Math.Ceiling(TotalCount / (double)Metadata.PageSize)) : 1;

        public string DisplayValue(Dictionary<string, object?> row, DynamicFieldMetadata field)
        {
            row.TryGetValue(field.FieldName, out var raw);

            if (field.ForeignKey != null)
            {
                var key = raw?.ToString() ?? "";
                if (FkDisplay.TryGetValue(field.FieldName, out var map) && map.TryGetValue(key, out var text))
                    return text;
                return key.Length > 0 ? key : "(none)";
            }

            if (field.IsChoiceField && field.Options.Count > 0)
            {
                var key = raw?.ToString() ?? "";
                var match = field.Options.FirstOrDefault(o => o.Value == key);
                if (match != null) return match.Text;
            }

            return raw switch
            {
                null => "",
                DateTime dt => dt.ToString("yyyy-MM-dd"),
                TimeSpan ts => ts.ToString(@"hh\:mm"),
                bool b => b ? "Yes" : "No",
                decimal dec => dec.ToString("N2"),
                _ => raw.ToString() ?? ""
            };
        }
    }
}
