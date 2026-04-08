using Slottet.Domain.Entities;

namespace Slottet.Application.Interfaces;

public interface IPasswordHashingService
{
    string Hash(Employee employee, string password);
}
