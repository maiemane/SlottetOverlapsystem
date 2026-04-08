using Microsoft.AspNetCore.Identity;
using Slottet.Application.Interfaces;
using Slottet.Domain.Entities;

namespace Slottet.Infrastructure.Auth;

public sealed class PasswordHashingService : IPasswordHashingService
{
    private readonly PasswordHasher<Employee> _passwordHasher = new();

    public string Hash(Employee employee, string password)
    {
        return _passwordHasher.HashPassword(employee, password);
    }
}
