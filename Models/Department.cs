using System.ComponentModel.DataAnnotations;

namespace GenericCrud.Models
{
    public class Department
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Department name is required")]
        [MaxLength(100)]
        [Display(Name = "Department Name")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(250)]
        public string? Description { get; set; }

        public ICollection<Employee>? Employees { get; set; }
    }
}
