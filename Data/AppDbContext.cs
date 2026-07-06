using GenericCrud.Models;
using Microsoft.EntityFrameworkCore;

namespace GenericCrud.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Add any new entity here — the CRUD engine will pick it up automatically,
        // no controller/view changes needed.
        public DbSet<Department> Departments => Set<Department>();
        public DbSet<Employee> Employees => Set<Employee>();
        public DbSet<Statuses> Statuses => Set<Statuses>();
    }
}
