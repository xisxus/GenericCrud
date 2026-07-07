using GenericCrud.Models;
using GenericCrud.Models.Dynamic;
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

        // Config schema for the DynamicCrud engine — see DynamicMetadataService/DynamicCrudService.
        public DbSet<DynamicEntity> DynamicEntities => Set<DynamicEntity>();
        public DbSet<DynamicField> DynamicFields => Set<DynamicField>();
        public DbSet<DynamicFieldValidation> DynamicFieldValidations => Set<DynamicFieldValidation>();
        public DbSet<DynamicFieldOption> DynamicFieldOptions => Set<DynamicFieldOption>();
        public DbSet<DynamicForeignKey> DynamicForeignKeys => Set<DynamicForeignKey>();
        public DbSet<DynamicFileConfig> DynamicFileConfigs => Set<DynamicFileConfig>();
        public DbSet<DynamicFieldSection> DynamicFieldSections => Set<DynamicFieldSection>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<DynamicEntity>()
                .HasIndex(e => e.EntityName)
                .IsUnique();

            modelBuilder.Entity<DynamicEntity>()
                .HasMany(e => e.Fields)
                .WithOne(f => f.DynamicEntity)
                .HasForeignKey(f => f.DynamicEntityId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DynamicEntity>()
                .HasMany(e => e.Sections)
                .WithOne(s => s.DynamicEntity)
                .HasForeignKey(s => s.DynamicEntityId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DynamicField>()
                .HasIndex(f => new { f.DynamicEntityId, f.FieldName })
                .IsUnique();

            modelBuilder.Entity<DynamicField>()
                .HasOne(f => f.Section)
                .WithMany(s => s.Fields)
                .HasForeignKey(f => f.DynamicFieldSectionId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            modelBuilder.Entity<DynamicField>()
                .HasOne(f => f.Validation)
                .WithOne(v => v.DynamicField)
                .HasForeignKey<DynamicFieldValidation>(v => v.DynamicFieldId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DynamicField>()
                .HasOne(f => f.ForeignKey)
                .WithOne(fk => fk.DynamicField)
                .HasForeignKey<DynamicForeignKey>(fk => fk.DynamicFieldId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DynamicField>()
                .HasOne(f => f.FileConfig)
                .WithOne(fc => fc.DynamicField)
                .HasForeignKey<DynamicFileConfig>(fc => fc.DynamicFieldId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DynamicField>()
                .HasMany(f => f.Options)
                .WithOne(o => o.DynamicField)
                .HasForeignKey(o => o.DynamicFieldId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DynamicField>()
                .Property(f => f.InputType)
                .HasConversion<string>()
                .HasMaxLength(20);
        }
    }
}
