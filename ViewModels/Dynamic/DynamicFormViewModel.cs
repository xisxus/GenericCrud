using System.Globalization;
using GenericCrud.Metadata.Dynamic;

namespace GenericCrud.ViewModels.Dynamic
{
    public class DynamicFormViewModel
    {
        public DynamicEntityMetadata Metadata { get; set; } = null!;
        public string? Id { get; set; }
        public bool IsEdit => Id != null;

        // Existing row values, keyed by column name (Create -> empty, Edit -> loaded row).
        public Dictionary<string, object?> Values { get; set; } = new();

        // Values re-submitted after a failed validation, so the form doesn't lose user input.
        public Dictionary<string, string> SubmittedValues { get; set; } = new();

        public Dictionary<string, List<(string Value, string Text)>> FkOptions { get; set; } = new();
        public Dictionary<string, List<string>> Errors { get; set; } = new();

        public string? GetValue(string fieldName)
        {
            if (SubmittedValues.TryGetValue(fieldName, out var sv)) return sv;
            if (Values.TryGetValue(fieldName, out var v) && v != null)
            {
                return v switch
                {
                    DateTime dt => dt.ToString("yyyy-MM-dd"),
                    TimeSpan ts => ts.ToString(@"hh\:mm"),
                    bool b => b ? "true" : "false",
                    _ => Convert.ToString(v, CultureInfo.InvariantCulture)
                };
            }
            return null;
        }

        public bool HasError(string fieldName) => Errors.ContainsKey(fieldName);
        public string? FirstError(string fieldName) => Errors.TryGetValue(fieldName, out var list) ? list.FirstOrDefault() : null;
    }
}
