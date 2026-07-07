using System.ComponentModel.DataAnnotations;

namespace GenericCrud.Models.Dynamic
{
    // One row = one generated entity. EntityName doubles as the physical table name
    // AND the /DynamicCrud/{action}/{entity} URL segment.
    public class DynamicEntity
    {
        public int Id { get; set; }

        [Required, MaxLength(128)]
        public string EntityName { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string PageTitle { get; set; } = string.Empty;

        [Required, MaxLength(128)]
        public string PrimaryKeyColumn { get; set; } = "Id";

        public bool SoftDelete { get; set; }

        [MaxLength(128)]
        public string SoftDeleteColumn { get; set; } = "IsDeleted";

        [MaxLength(128)]
        public string? DefaultSortColumn { get; set; }

        // "ASC" or "DESC"
        [MaxLength(4)]
        public string DefaultSortDirection { get; set; } = "ASC";

        [MaxLength(50)]
        public string? PageCode { get; set; }

        public int PageSize { get; set; } = 10;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public List<DynamicField> Fields { get; set; } = new();
        public List<DynamicFieldSection> Sections { get; set; } = new();
    }
}
