using Slottet.Domain.Entities;

namespace Slottet.Application.Interfaces;

public interface IEmployeeRepository
{
    Task<IReadOnlyList<Employee>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Employee?> GetActiveByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<Employee?> GetByIdAsync(int employeeId, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
    Task<Employee> CreateAsync(Employee employee, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Employee employee, CancellationToken cancellationToken = default);
}
