using System.ComponentModel.DataAnnotations;

namespace GenericCrud.Models.Dynamic
{
    public class DynamicFieldSection
    {
        public int Id { get; set; }

        public int DynamicEntityId { get; set; }
        public DynamicEntity? DynamicEntity { get; set; }

        [Required, MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        public int SortOrder { get; set; }

        public List<DynamicField> Fields { get; set; } = new();
    }
}
