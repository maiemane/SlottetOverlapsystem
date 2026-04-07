using Slottet.Domain.Entities;

namespace Slottet.Application.Interfaces;

public interface IPasswordVerificationService
{
    bool Verify(Employee employee, string password);
}