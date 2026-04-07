using Microsoft.AspNetCore.Identity;
using Slottet.Domain.Entities;

namespace Slottet.Api.Auth;

public sealed class PasswordVerificationService
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
