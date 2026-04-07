using Slottet.Domain.Entities;

namespace Slottet.Application.Interfaces;

public interface IEmployeeRepository
{
    Task<Employee?> GetActiveByEmailAsync(string email, CancellationToken cancellationToken = default);
}
