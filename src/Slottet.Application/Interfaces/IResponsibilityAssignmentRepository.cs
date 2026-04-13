using Slottet.Domain.Entities;

namespace Slottet.Application.Interfaces;

public interface IResponsibilityAssignmentRepository
{
    Task<Shift?> GetShiftByIdAsync(int shiftId, CancellationToken cancellationToken = default);
    Task<Employee?> GetEmployeeByIdAsync(int employeeId, CancellationToken cancellationToken = default);
    Task<ResponsibilityType?> GetResponsibilityTypeByIdAsync(int responsibilityTypeId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<int>> GetShiftEmployeeIdsAsync(int shiftId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ResponsibilityAssignment>> GetResponsibilityAssignmentsAsync(int shiftId, CancellationToken cancellationToken = default);
    Task<ResponsibilityAssignment?> GetResponsibilityAssignmentByIdAsync(int responsibilityAssignmentId, CancellationToken cancellationToken = default);
    Task<ResponsibilityAssignment?> GetResponsibilityAssignmentByShiftAndTypeAsync(int shiftId, int responsibilityTypeId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Employee>> GetEmployeesByIdsAsync(IReadOnlyCollection<int> employeeIds, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ResponsibilityType>> GetResponsibilityTypesByIdsAsync(IReadOnlyCollection<int> responsibilityTypeIds, CancellationToken cancellationToken = default);
    Task<ResponsibilityAssignment> AddResponsibilityAssignmentAsync(ResponsibilityAssignment responsibilityAssignment, CancellationToken cancellationToken = default);
    Task<ResponsibilityAssignment> UpdateResponsibilityAssignmentAsync(ResponsibilityAssignment responsibilityAssignment, CancellationToken cancellationToken = default);
    Task DeleteResponsibilityAssignmentAsync(ResponsibilityAssignment responsibilityAssignment, CancellationToken cancellationToken = default);
}
