using System.ComponentModel.DataAnnotations;

namespace GenericCrud.Models.Dynamic
{
    public class DynamicFieldOption
    {
        public int Id { get; set; }

        public int DynamicFieldId { get; set; }
        public DynamicField? DynamicField { get; set; }

        [Required, MaxLength(200)]
        public string OptionValue { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string OptionText { get; set; } = string.Empty;

        public int SortOrder { get; set; }
    }
}
