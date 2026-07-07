using GenericCrud.Models.Dynamic;

namespace GenericCrud.Metadata.Dynamic
{
    // In-memory, cached shape built from the DynamicEntity/DynamicField* config tables.
    // This is what DynamicCrudController and DynamicCrudService actually work against.
    public class DynamicEntityMetadata
    {
        public int Id { get; set; }
        public string EntityName { get; set; } = string.Empty;
        public string PageTitle { get; set; } = string.Empty;
        public string PrimaryKeyColumn { get; set; } = "Id";
        public bool SoftDelete { get; set; }
        public string SoftDeleteColumn { get; set; } = "IsDeleted";
        public string? DefaultSortColumn { get; set; }
        public string DefaultSortDirection { get; set; } = "ASC";
        public int PageSize { get; set; } = 10;

        public List<DynamicFieldSectionMetadata> Sections { get; set; } = new();
        public List<DynamicFieldMetadata> Fields { get; set; } = new();

        public List<DynamicFieldMetadata> TableFields =>
            Fields.Where(f => f.ShowInTable).OrderBy(f => f.TableOrder).ToList();

        public List<DynamicFieldMetadata> FormFields =>
            Fields.Where(f => f.ShowInForm).OrderBy(f => f.FormOrder).ToList();
    }

    public class DynamicFieldSectionMetadata
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int SortOrder { get; set; }
    }

    public class DynamicFieldMetadata
    {
        public int Id { get; set; }
        public string FieldName { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public DynamicInputType InputType { get; set; }
        public int FormOrder { get; set; }
        public int TableOrder { get; set; }
        public bool ShowInForm { get; set; } = true;
        public bool ShowInTable { get; set; } = true;
        public bool IsRequired { get; set; }
        public string? DefaultValue { get; set; }
        public int? SectionId { get; set; }
        public string? ConditionalOnFieldName { get; set; }
        public string? ConditionalOnValue { get; set; }

        public DynamicFieldValidationMetadata? Validation { get; set; }
        public List<DynamicFieldOptionMetadata> Options { get; set; } = new();
        public DynamicForeignKeyMetadata? ForeignKey { get; set; }
        public DynamicFileConfigMetadata? FileConfig { get; set; }

        public bool IsFileField => InputType == DynamicInputType.File;
        public bool IsChoiceField => InputType == DynamicInputType.Dropdown || InputType == DynamicInputType.Radio;
        public bool HasCondition => !string.IsNullOrEmpty(ConditionalOnFieldName);
    }

    public class DynamicFieldValidationMetadata
    {
        public int? MinLength { get; set; }
        public int? MaxLength { get; set; }
        public decimal? MinValue { get; set; }
        public decimal? MaxValue { get; set; }
        public string? Pattern { get; set; }
        public int? MaxFileSizeKb { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class DynamicFieldOptionMetadata
    {
        public string Value { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
    }

    public class DynamicForeignKeyMetadata
    {
        public string ForeignTableName { get; set; } = string.Empty;
        public string ValueColumn { get; set; } = "Id";
        public string TextColumn { get; set; } = string.Empty;
        public string? OrderByColumn { get; set; }
    }

    public class DynamicFileConfigMetadata
    {
        public string SaveFolder { get; set; } = "uploads";
        public string AllowedExtensions { get; set; } = string.Empty;
        public int MaxSizeKb { get; set; } = 2048;
        public bool RenameToGuid { get; set; } = true;
    }
}
