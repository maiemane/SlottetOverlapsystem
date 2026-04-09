using Slottet.Domain.Entities;

namespace Slottet.Application.Interfaces;

public interface IDepartmentRepository
{
    Task<IReadOnlyList<Department>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Department?> GetByIdAsync(int departmentId, CancellationToken cancellationToken = default);
    Task<Department> UpdateAsync(Department department, CancellationToken cancellationToken = default);
}
