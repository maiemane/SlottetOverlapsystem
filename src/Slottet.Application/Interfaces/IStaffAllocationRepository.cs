using Slottet.Domain.Entities;

namespace Slottet.Application.Interfaces;

public interface IStaffAllocationRepository
{
    Task<Department?> GetDepartmentByIdAsync(int departmentId, CancellationToken cancellationToken = default);
    Task<Shift?> GetShiftByIdAsync(int shiftId, CancellationToken cancellationToken = default);
    Task<Citizen?> GetCitizenByIdAsync(int citizenId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Citizen>> GetActiveCitizensByDepartmentAsync(int departmentId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Employee>> GetActiveEmployeesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Employee>> GetActiveEmployeesByIdsAsync(IReadOnlyCollection<int> employeeIds, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Shift>> GetShiftsByDepartmentAndDateAsync(int departmentId, DateTime date, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Shift>> EnsureShiftsForDepartmentAndDateAsync(int departmentId, DateTime date, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CitizenAssignment>> GetCitizenAssignmentsByShiftIdsAsync(IReadOnlyCollection<int> shiftIds, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Employee>> GetEmployeesByShiftAsync(int shiftId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Employee>> GetEmployeesByCitizenAssignmentAsync(int shiftId, int citizenId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<int>> GetShiftEmployeeIdsAsync(int shiftId, CancellationToken cancellationToken = default);
    Task ReplaceShiftEmployeesAsync(int shiftId, IReadOnlyCollection<int> employeeIds, CancellationToken cancellationToken = default);
    Task ReplaceCitizenAssignmentsAsync(int shiftId, int citizenId, IReadOnlyCollection<int> employeeIds, CancellationToken cancellationToken = default);
}
