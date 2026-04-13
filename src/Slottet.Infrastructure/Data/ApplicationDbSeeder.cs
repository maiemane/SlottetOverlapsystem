using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Slottet.Domain.Entities;
using Slottet.Domain.Enums;

namespace Slottet.Infrastructure.Data;

public static class ApplicationDbSeeder
{
    public static async Task EnsureDatabaseMigratedAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await MigrateWithRetryAsync(dbContext, cancellationToken);
    }

    public static async Task SeedAuthenticationDataAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await services.EnsureDatabaseMigratedAsync(cancellationToken);

        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger("Slottet.Infrastructure.Data.LogRetention");
        var retentionOptions = scope.ServiceProvider.GetRequiredService<IOptions<LogRetentionOptions>>().Value;

        await CleanupExpiredLogsAsync(dbContext, retentionOptions, logger, cancellationToken);

        var requiredDepartmentNames = new[]
        {
            "Slottet",
            "Skoven"
        };

        var existingDepartmentNames = await dbContext.Departments
            .AsNoTracking()
            .Select(department => department.Name)
            .ToListAsync(cancellationToken);

        var missingDepartments = requiredDepartmentNames
            .Where(requiredName => !existingDepartmentNames.Any(existingName =>
                string.Equals(existingName, requiredName, StringComparison.OrdinalIgnoreCase)))
            .Select(name => new Department { Name = name })
            .ToList();

        if (missingDepartments.Count > 0)
        {
            await dbContext.Departments.AddRangeAsync(missingDepartments, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        var existingShiftTypes = await dbContext.ShiftDefinitions
            .AsNoTracking()
            .Select(definition => definition.ShiftType)
            .ToListAsync(cancellationToken);

        var defaultShiftDefinitions = new[]
        {
            new ShiftDefinition { ShiftType = ShiftType.Dagvagt, StartTime = new TimeOnly(7, 0), EndTime = new TimeOnly(15, 0), IsActive = true },
            new ShiftDefinition { ShiftType = ShiftType.Aftenvagt, StartTime = new TimeOnly(15, 0), EndTime = new TimeOnly(23, 0), IsActive = true },
            new ShiftDefinition { ShiftType = ShiftType.Nattevagt, StartTime = new TimeOnly(23, 0), EndTime = new TimeOnly(7, 0), IsActive = true }
        };

        var missingShiftDefinitions = defaultShiftDefinitions
            .Where(requiredDefinition => !existingShiftTypes.Contains(requiredDefinition.ShiftType))
            .ToList();

        if (missingShiftDefinitions.Count > 0)
        {
            await dbContext.ShiftDefinitions.AddRangeAsync(missingShiftDefinitions, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        var requiredResponsibilityTypeNames = new[]
        {
            "Medicintovholder",
            "Omsorgsperson",
            "Aftensmad",
            "Hygiejne/afsprit",
            "Kaffe til næste hold",
            "Tøm skraldespand",
            "Søndag: Madplan"
        };

        var existingResponsibilityTypeNames = await dbContext.ResponsibilityTypes
            .AsNoTracking()
            .Select(responsibilityType => responsibilityType.Name)
            .ToListAsync(cancellationToken);

        var missingResponsibilityTypes = requiredResponsibilityTypeNames
            .Where(requiredName => !existingResponsibilityTypeNames.Any(existingName =>
                string.Equals(existingName, requiredName, StringComparison.OrdinalIgnoreCase)))
            .Select(name => new ResponsibilityType { Name = name })
            .ToList();

        if (missingResponsibilityTypes.Count > 0)
        {
            await dbContext.ResponsibilityTypes.AddRangeAsync(missingResponsibilityTypes, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        if (await dbContext.Employees.AnyAsync(cancellationToken))
        {
            return;
        }

        var passwordHasher = new PasswordHasher<Employee>();
        var employees = new[]
        {
            CreateEmployee(1, "Admin", "admin@slottet.dk", "Admin", "admin123", passwordHasher),
            CreateEmployee(2, "Medarbejder", "medarbejder@slottet.dk", "Medarbejder", "med123", passwordHasher)
        };

        await dbContext.Employees.AddRangeAsync(employees, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task MigrateWithRetryAsync(ApplicationDbContext dbContext, CancellationToken cancellationToken)
    {
        const int maxAttempts = 10;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                await dbContext.Database.MigrateAsync(cancellationToken);
                return;
            }
            catch (Exception) when (attempt < maxAttempts)
            {
                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
            }
        }

        await dbContext.Database.MigrateAsync(cancellationToken);
    }

    private static async Task CleanupExpiredLogsAsync(
        ApplicationDbContext dbContext,
        LogRetentionOptions options,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        if (!options.Enabled)
        {
            logger.LogInformation("Log retention cleanup is disabled.");
            return;
        }

        if (options.AuditLogRetentionDays > 0)
        {
            var auditThresholdUtc = DateTime.UtcNow.AddDays(-options.AuditLogRetentionDays);
            var deletedAuditLogs = await dbContext.AuditLogs
                .Where(log => log.OccurredAtUtc < auditThresholdUtc)
                .ExecuteDeleteAsync(cancellationToken);

            logger.LogInformation(
                "Deleted {DeletedAuditLogs} audit logs older than {AuditThresholdUtc}.",
                deletedAuditLogs,
                auditThresholdUtc);
        }

        if (options.AccessLogRetentionDays > 0)
        {
            var accessThresholdUtc = DateTime.UtcNow.AddDays(-options.AccessLogRetentionDays);
            var deletedAccessLogs = await dbContext.AccessLogs
                .Where(log => log.OccurredAtUtc < accessThresholdUtc)
                .ExecuteDeleteAsync(cancellationToken);

            logger.LogInformation(
                "Deleted {DeletedAccessLogs} access logs older than {AccessThresholdUtc}.",
                deletedAccessLogs,
                accessThresholdUtc);
        }
    }

    private static Employee CreateEmployee(
        int id,
        string name,
        string email,
        string role,
        string password,
        PasswordHasher<Employee> passwordHasher)
    {
        var employee = new Employee
        {
            Id = id,
            Name = name,
            Email = email,
            Role = role,
            IsActive = true
        };

        employee.PasswordHash = passwordHasher.HashPassword(employee, password);

        return employee;
    }
}
