using Microsoft.AspNetCore.Identity;
using Slottet.Application.Interfaces;
using Slottet.Domain.Entities;

namespace Slottet.Infrastructure.Auth;

public sealed class PasswordVerificationService : IPasswordVerificationService
{
    private readonly PasswordHasher<Employee> _passwordHasher = new();

    public bool Verify(Employee employee, string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            return false;
        }

        var result = _passwordHasher.VerifyHashedPassword(employee, employee.PasswordHash, password);

        return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
    }
}
