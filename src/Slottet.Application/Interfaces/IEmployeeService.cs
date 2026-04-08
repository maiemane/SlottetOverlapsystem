using Slottet.Application.DTOs.Employees;

namespace Slottet.Application.Interfaces;

public interface IEmployeeService
{
    Task<IReadOnlyList<EmployeeDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<CreateEmployeeResultDto> CreateAsync(CreateEmployeeRequestDto request, CancellationToken cancellationToken = default);
    Task<UpdateEmployeeResultDto> UpdateAsync(int employeeId, UpdateEmployeeRequestDto request, CancellationToken cancellationToken = default);
    Task<DeleteEmployeeResultDto> DeleteAsync(int employeeId, CancellationToken cancellationToken = default);
}
