using Slottet.Application.DTOs.Departments;

namespace Slottet.Application.Interfaces;

public interface IDepartmentService
{
    Task<IReadOnlyList<DepartmentDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<DepartmentDto?> GetByIdAsync(int departmentId, CancellationToken cancellationToken = default);
    Task<UpdateDepartmentMessageResult> UpdateMessageAsync(int departmentId, UpdateDepartmentMessageRequest request, CancellationToken cancellationToken = default);
}
