using GenericCrud.Models.Dynamic;

namespace GenericCrud.ViewModels.Dynamic
{
    // Flat model bound directly from the field config modal's <form> post.
    public class DynamicFieldInputModel
    {
        public int Id { get; set; }
        public int DynamicEntityId { get; set; }

        public string FieldName { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public DynamicInputType InputType { get; set; }
        public int FormOrder { get; set; }
        public int TableOrder { get; set; }
        public bool ShowInForm { get; set; }
        public bool ShowInTable { get; set; }
        public bool IsRequired { get; set; }
        public string? DefaultValue { get; set; }
        public int? DynamicFieldSectionId { get; set; }
        public string? ConditionalOnFieldName { get; set; }
        public string? ConditionalOnValue { get; set; }

        // validation
        public int? MinLength { get; set; }
        public int? MaxLength { get; set; }
        public decimal? MinValue { get; set; }
        public decimal? MaxValue { get; set; }
        public string? Pattern { get; set; }
        public int? MaxFileSizeKb { get; set; }
        public string? ErrorMessage { get; set; }

        // foreign key
        public string? ForeignTableName { get; set; }
        public string? ValueColumn { get; set; }
        public string? TextColumn { get; set; }
        public string? OrderByColumn { get; set; }

        // file upload
        public string? SaveFolder { get; set; }
        public string? AllowedExtensions { get; set; }
        public int? MaxSizeKb { get; set; }
        public bool RenameToGuid { get; set; } = true;

        // static dropdown/radio options (repeated inputs with the same name bind as a list)
        public List<string>? OptionValues { get; set; }
        public List<string>? OptionTexts { get; set; }
    }
}
