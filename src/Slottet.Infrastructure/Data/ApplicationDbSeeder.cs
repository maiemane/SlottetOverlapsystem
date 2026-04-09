using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Slottet.Domain.Entities;

namespace Slottet.Infrastructure.Data;

public static class ApplicationDbSeeder
{
    public static async Task SeedAuthenticationDataAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await dbContext.Database.MigrateAsync(cancellationToken);

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
