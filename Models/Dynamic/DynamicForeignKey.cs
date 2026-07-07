using System.ComponentModel.DataAnnotations;

namespace GenericCrud.Models.Dynamic
{
    // Attached to a Dropdown/Radio field to source its options from another table.
    public class DynamicForeignKey
    {
        public int Id { get; set; }

        public int DynamicFieldId { get; set; }
        public DynamicField? DynamicField { get; set; }

        [Required, MaxLength(128)]
        public string ForeignTableName { get; set; } = string.Empty;

        [Required, MaxLength(128)]
        public string ValueColumn { get; set; } = "Id";

        [Required, MaxLength(128)]
        public string TextColumn { get; set; } = string.Empty;

        [MaxLength(128)]
        public string? OrderByColumn { get; set; }
    }
}
