using System.ComponentModel.DataAnnotations;

namespace GenericCrud.Models.Dynamic
{
    // One row = one field/column on the generated entity.
    public class DynamicField
    {
        public int Id { get; set; }

        public int DynamicEntityId { get; set; }
        public DynamicEntity? DynamicEntity { get; set; }

        [Required, MaxLength(128)]
        public string FieldName { get; set; } = string.Empty; // physical column name

        [Required, MaxLength(200)]
        public string Label { get; set; } = string.Empty;

        public DynamicInputType InputType { get; set; } = DynamicInputType.Text;

        public int FormOrder { get; set; }
        public int TableOrder { get; set; }

        public bool ShowInForm { get; set; } = true;
        public bool ShowInTable { get; set; } = true;

        // Force required in the generated form even if the DB column is nullable.
        public bool IsRequired { get; set; }

        [MaxLength(500)]
        public string? DefaultValue { get; set; }

        public int? DynamicFieldSectionId { get; set; }
        public DynamicFieldSection? Section { get; set; }

        // Conditional display: only show this field when the field named
        // ConditionalOnFieldName currently holds ConditionalOnValue.
        [MaxLength(128)]
        public string? ConditionalOnFieldName { get; set; }

        [MaxLength(200)]
        public string? ConditionalOnValue { get; set; }

        public bool IsActive { get; set; } = true;

        public DynamicFieldValidation? Validation { get; set; }
        public List<DynamicFieldOption> Options { get; set; } = new();
        public DynamicForeignKey? ForeignKey { get; set; }
        public DynamicFileConfig? FileConfig { get; set; }
    }
}
