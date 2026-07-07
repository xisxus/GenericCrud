using System.ComponentModel.DataAnnotations;

namespace GenericCrud.Models.Dynamic
{
    public class DynamicFieldValidation
    {
        public int Id { get; set; }

        public int DynamicFieldId { get; set; }
        public DynamicField? DynamicField { get; set; }

        public int? MinLength { get; set; }
        public int? MaxLength { get; set; }

        public decimal? MinValue { get; set; }
        public decimal? MaxValue { get; set; }

        [MaxLength(500)]
        public string? Pattern { get; set; } // regex, e.g. for custom text formats

        public int? MaxFileSizeKb { get; set; }

        [MaxLength(300)]
        public string? ErrorMessage { get; set; } // custom message overriding the default
    }
}
