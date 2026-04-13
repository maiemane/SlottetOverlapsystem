using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;
using Slottet.Application.Interfaces;
using Slottet.Domain.Entities;

namespace Slottet.Infrastructure.Data;

public class ApplicationDbContext : DbContext, IDataProtectionKeyContext
{
    private const string RedactedValue = "[REDACTED]";

    private static readonly HashSet<string> SensitivePropertyNames =
    [
        "Password",
        "PasswordHash"
    ];

    private static readonly Dictionary<string, HashSet<string>> SensitiveAuditPropertiesByEntity = new(StringComparer.Ordinal)
    {
        [nameof(Citizen)] = new HashSet<string>(StringComparer.Ordinal)
        {
            nameof(Citizen.Name),
            nameof(Citizen.ApartmentNumber)
        },
        [nameof(Employee)] = new HashSet<string>(StringComparer.Ordinal)
        {
            nameof(Employee.Name),
            nameof(Employee.Email),
            nameof(Employee.PasswordHash)
        },
        [nameof(CitizenFixedMedication)] = new HashSet<string>(StringComparer.Ordinal)
        {
            nameof(CitizenFixedMedication.Name),
            nameof(CitizenFixedMedication.Description)
        },
        [nameof(MedicinRegistration)] = new HashSet<string>(StringComparer.Ordinal)
        {
            nameof(MedicinRegistration.Name),
            nameof(MedicinRegistration.Description)
        },
        [nameof(SpecialEvent)] = new HashSet<string>(StringComparer.Ordinal)
        {
            nameof(SpecialEvent.Description)
        }
    };

    private readonly ICurrentUserContext? _currentUserContext;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ICurrentUserContext? currentUserContext = null)
        : base(options)
    {
        _currentUserContext = currentUserContext;
    }

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<AccessLog> AccessLogs => Set<AccessLog>();
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
    public DbSet<DataProtectionKey> DataProtectionKeys => Set<DataProtectionKey>();

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ChangeTracker.DetectChanges();

        var auditEntries = CreateAuditEntries();
        var result = await base.SaveChangesAsync(cancellationToken);

        if (auditEntries.Count > 0)
        {
            var auditLogs = auditEntries
                .Select(CreateAuditLog)
                .ToList();

            AuditLogs.AddRange(auditLogs);
            await base.SaveChangesAsync(cancellationToken);
        }

        return result;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("AuditLog");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Action).IsRequired().HasMaxLength(20);
            entity.Property(x => x.EntityName).IsRequired().HasMaxLength(100);
            entity.Property(x => x.EntityId).IsRequired().HasMaxLength(100);
            entity.Property(x => x.OldValuesJson).HasMaxLength(4000);
            entity.Property(x => x.NewValuesJson).HasMaxLength(4000);
            entity.Property(x => x.RequestPath).HasMaxLength(500);
            entity.Property(x => x.CorrelationId).HasMaxLength(100);
        });

        modelBuilder.Entity<AccessLog>(entity =>
        {
            entity.ToTable("AccessLog");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.HttpMethod).IsRequired().HasMaxLength(10);
            entity.Property(x => x.RequestPath).IsRequired().HasMaxLength(500);
            entity.Property(x => x.QueryString).HasMaxLength(1000);
            entity.Property(x => x.CorrelationId).HasMaxLength(100);
        });

        modelBuilder.Entity<DataProtectionKey>(entity =>
        {
            entity.ToTable("DataProtectionKey");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.FriendlyName).HasMaxLength(512);
            entity.Property(x => x.Xml).IsRequired();
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.ToTable("Department");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).IsRequired().HasMaxLength(100);
            entity.Property(x => x.Message).HasMaxLength(2000);
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

    private List<AuditEntry> CreateAuditEntries()
    {
        var auditEntries = new List<AuditEntry>();

        foreach (var entry in ChangeTracker.Entries()
                     .Where(candidate => candidate.State is EntityState.Added or EntityState.Modified or EntityState.Deleted))
        {
            if (entry.Entity is AuditLog or AccessLog)
            {
                continue;
            }

            var oldValues = new Dictionary<string, object?>();
            var newValues = new Dictionary<string, object?>();

            foreach (var property in entry.Properties)
            {
                if (property.Metadata.IsPrimaryKey() || SensitivePropertyNames.Contains(property.Metadata.Name))
                {
                    continue;
                }

                var propertyName = property.Metadata.Name;
                var redactValue = ShouldRedactAuditProperty(entry.Metadata.ClrType.Name, propertyName);

                if (entry.State == EntityState.Added)
                {
                    newValues[propertyName] = redactValue
                        ? RedactedValue
                        : property.CurrentValue;
                }
                else if (entry.State == EntityState.Deleted)
                {
                    oldValues[propertyName] = redactValue
                        ? RedactedValue
                        : property.OriginalValue;
                }
                else if (property.IsModified)
                {
                    oldValues[propertyName] = redactValue
                        ? RedactedValue
                        : property.OriginalValue;
                    newValues[propertyName] = redactValue
                        ? RedactedValue
                        : property.CurrentValue;
                }
            }

            if (entry.State == EntityState.Modified && oldValues.Count == 0 && newValues.Count == 0)
            {
                continue;
            }

            auditEntries.Add(new AuditEntry
            {
                Entry = entry,
                Action = entry.State switch
                {
                    EntityState.Added => "Create",
                    EntityState.Modified => "Update",
                    EntityState.Deleted => "Delete",
                    _ => "Unknown"
                },
                EntityName = entry.Metadata.ClrType.Name,
                OldValues = oldValues,
                NewValues = newValues
            });
        }

        return auditEntries;
    }

    private AuditLog CreateAuditLog(AuditEntry auditEntry)
    {
        return new AuditLog
        {
            OccurredAtUtc = DateTime.UtcNow,
            EmployeeId = _currentUserContext?.EmployeeId,
            Action = auditEntry.Action,
            EntityName = auditEntry.EntityName,
            EntityId = GetPrimaryKeyValue(auditEntry.Entry),
            OldValuesJson = Serialize(auditEntry.OldValues),
            NewValuesJson = Serialize(auditEntry.NewValues),
            RequestPath = _currentUserContext?.RequestPath ?? string.Empty,
            CorrelationId = _currentUserContext?.CorrelationId ?? string.Empty
        };
    }

    private static string GetPrimaryKeyValue(EntityEntry entry)
    {
        var keyValues = entry.Properties
            .Where(property => property.Metadata.IsPrimaryKey())
            .Select(property => property.CurrentValue?.ToString() ?? string.Empty)
            .ToList();

        return string.Join(",", keyValues);
    }

    private static string Serialize(Dictionary<string, object?> values)
    {
        return values.Count == 0
            ? string.Empty
            : JsonSerializer.Serialize(values);
    }

    private static bool ShouldRedactAuditProperty(string entityName, string propertyName)
    {
        return SensitiveAuditPropertiesByEntity.TryGetValue(entityName, out var properties)
               && properties.Contains(propertyName);
    }
}
