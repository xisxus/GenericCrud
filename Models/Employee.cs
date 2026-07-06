using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GenericCrud.Models
{
    public class Employee
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Full name is required")]
        [MaxLength(100)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [EmailAddress]
        [MaxLength(150)]
        public string? Email { get; set; }

        [Range(0, 10000000)]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Monthly Salary")]
        public decimal Salary { get; set; }

        [Display(Name = "Date of Joining")]
        [DataType(DataType.Date)]
        public DateTime JoiningDate { get; set; } = DateTime.Today;

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        // Foreign key — auto-detected by EntityMetadataService
        [Display(Name = "Department")]
        public int DepartmentId { get; set; }

        [ForeignKey(nameof(DepartmentId))]
        public Department? Department { get; set; }
    }
}
