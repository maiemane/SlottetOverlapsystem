using Slottet.Domain.Entities;

namespace Slottet.Application.Interfaces;

public interface IStaffAllocationRepository
{
    Task<Shift?> GetShiftByIdAsync(int shiftId, CancellationToken cancellationToken = default);
    Task<Citizen?> GetCitizenByIdAsync(int citizenId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Employee>> GetActiveEmployeesByIdsAsync(IReadOnlyCollection<int> employeeIds, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Employee>> GetEmployeesByShiftAsync(int shiftId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Employee>> GetEmployeesByCitizenAssignmentAsync(int shiftId, int citizenId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<int>> GetShiftEmployeeIdsAsync(int shiftId, CancellationToken cancellationToken = default);
    Task ReplaceShiftEmployeesAsync(int shiftId, IReadOnlyCollection<int> employeeIds, CancellationToken cancellationToken = default);
    Task ReplaceCitizenAssignmentsAsync(int shiftId, int citizenId, IReadOnlyCollection<int> employeeIds, CancellationToken cancellationToken = default);
}
