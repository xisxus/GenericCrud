using System.ComponentModel.DataAnnotations;

namespace GenericCrud.Models.Dynamic
{
    public class DynamicFileConfig
    {
        public int Id { get; set; }

        public int DynamicFieldId { get; set; }
        public DynamicField? DynamicField { get; set; }

        // Relative to wwwroot, e.g. "uploads/products"
        [Required, MaxLength(300)]
        public string SaveFolder { get; set; } = "uploads";

        // Comma-separated, e.g. ".jpg,.jpeg,.png,.pdf"
        [Required, MaxLength(300)]
        public string AllowedExtensions { get; set; } = ".jpg,.jpeg,.png,.pdf";

        public int MaxSizeKb { get; set; } = 2048;

        public bool RenameToGuid { get; set; } = true;
    }
}
