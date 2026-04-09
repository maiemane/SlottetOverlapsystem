using Microsoft.EntityFrameworkCore;
using Slottet.Domain.Entities;

namespace Slottet.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Citizen> Citizens => Set<Citizen>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Shift> Shifts => Set<Shift>();
    public DbSet<CitizenAssignment> CitizenAssignments => Set<CitizenAssignment>();
    public DbSet<CitizenFixedMedication> CitizenFixedMedications => Set<CitizenFixedMedication>();
    public DbSet<MedicinRegistration> MedicinRegistrations => Set<MedicinRegistration>();
    public DbSet<Phone> Phones => Set<Phone>();
    public DbSet<PhoneAllocation> PhoneAllocations => Set<PhoneAllocation>();
    public DbSet<ResponsibilityType> ResponsibilityTypes => Set<ResponsibilityType>();
    public DbSet<ResponsibilityAssignment> ResponsibilityAssignments => Set<ResponsibilityAssignment>();
    public DbSet<ShiftTask> ShiftTasks => Set<ShiftTask>();
    public DbSet<SpecialEvent> SpecialEvents => Set<SpecialEvent>();
    public DbSet<ShiftEmployee> ShiftEmployees => Set<ShiftEmployee>();
    public DbSet<ShiftDefinition> ShiftDefinitions => Set<ShiftDefinition>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Department>(entity =>
        {
            entity.ToTable("Department");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).IsRequired().HasMaxLength(100);
        });

        modelBuilder.Entity<Citizen>(entity =>
        {
            entity.ToTable("Citizen");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).IsRequired().HasMaxLength(100);
            entity.Property(x => x.ApartmentNumber).IsRequired().HasMaxLength(50);

            entity.HasOne<Department>()
                .WithMany()
                .HasForeignKey(x => x.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CitizenFixedMedication>(entity =>
        {
            entity.ToTable("CitizenFixedMedication");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).IsRequired().HasMaxLength(200);
            entity.Property(x => x.Description).HasMaxLength(1000);
            entity.Property(x => x.ScheduledTime).IsRequired();
            entity.HasIndex(x => new { x.CitizenId, x.ShiftType });

            entity.HasOne<Citizen>()
                .WithMany()
                .HasForeignKey(x => x.CitizenId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.ToTable("Employee");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).IsRequired().HasMaxLength(100);
            entity.Property(x => x.Email).IsRequired().HasMaxLength(255);
            entity.Property(x => x.PasswordHash).IsRequired().HasMaxLength(500);
            entity.Property(x => x.Role).IsRequired().HasMaxLength(50);
        });

        modelBuilder.Entity<Shift>(entity =>
        {
            entity.ToTable("Shift");
            entity.HasKey(x => x.Id);

            entity.HasOne<Department>()
                .WithMany()
                .HasForeignKey(x => x.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ShiftDefinition>(entity =>
        {
            entity.ToTable("ShiftDefinition");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.StartTime).IsRequired();
            entity.Property(x => x.EndTime).IsRequired();
            entity.HasIndex(x => x.ShiftType).IsUnique();
        });

        modelBuilder.Entity<CitizenAssignment>(entity =>
        {
            entity.ToTable("CitizenAssignment");
            entity.HasKey(x => x.Id);

            entity.HasOne<Employee>()
                .WithMany()
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Citizen>()
                .WithMany()
                .HasForeignKey(x => x.CitizenId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<Shift>()
                .WithMany()
                .HasForeignKey(x => x.ShiftId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MedicinRegistration>(entity =>
        {
            entity.ToTable("MedicinRegistration");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).IsRequired().HasMaxLength(200);
            entity.Property(x => x.Description).HasMaxLength(1000);

            entity.HasOne<Citizen>()
                .WithMany()
                .HasForeignKey(x => x.CitizenId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<Shift>()
                .WithMany()
                .HasForeignKey(x => x.ShiftId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<CitizenFixedMedication>()
                .WithMany()
                .HasForeignKey(x => x.CitizenFixedMedicationId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<Phone>(entity =>
        {
            entity.ToTable("Phone");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.NameOrNumber).IsRequired().HasMaxLength(100);
        });

        modelBuilder.Entity<PhoneAllocation>(entity =>
        {
            entity.ToTable("PhoneAllocation");
            entity.HasKey(x => x.Id);

            entity.HasOne<Employee>()
                .WithMany()
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Shift>()
                .WithMany()
                .HasForeignKey(x => x.ShiftId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<Phone>()
                .WithMany()
                .HasForeignKey(x => x.PhoneId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ResponsibilityType>(entity =>
        {
            entity.ToTable("ResponsibilityType");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).IsRequired().HasMaxLength(100);
        });

        modelBuilder.Entity<ResponsibilityAssignment>(entity =>
        {
            entity.ToTable("ResponsibilityAssignment");
            entity.HasKey(x => x.Id);

            entity.HasOne<Employee>()
                .WithMany()
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Shift>()
                .WithMany()
                .HasForeignKey(x => x.ShiftId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<ResponsibilityType>()
                .WithMany()
                .HasForeignKey(x => x.ResponsibilityTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ShiftTask>(entity =>
        {
            entity.ToTable("ShiftTask");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Description).IsRequired().HasMaxLength(1000);

            entity.HasOne<Shift>()
                .WithMany()
                .HasForeignKey(x => x.ShiftId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SpecialEvent>(entity =>
        {
            entity.ToTable("SpecialEvent");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Description).IsRequired().HasMaxLength(1000);

            entity.HasOne<Citizen>()
                .WithMany()
                .HasForeignKey(x => x.CitizenId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<Shift>()
                .WithMany()
                .HasForeignKey(x => x.ShiftId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        modelBuilder.Entity<ShiftEmployee>(entity =>
        {
            entity.ToTable("Shift_Employee");
            entity.HasKey(x => x.ShiftEmployeeId);

            entity.HasOne<Shift>()
                .WithMany()
                .HasForeignKey(x => x.ShiftId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<Employee>()
                .WithMany()
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
