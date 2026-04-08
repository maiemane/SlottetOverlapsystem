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

        if (!await dbContext.Departments.AnyAsync(cancellationToken))
        {
            var departments = new[]
            {
                new Department { Id = 1, Name = "Slottet" },
                new Department { Id = 2, Name = "Skoven" }
            };

            await dbContext.Departments.AddRangeAsync(departments, cancellationToken);
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
