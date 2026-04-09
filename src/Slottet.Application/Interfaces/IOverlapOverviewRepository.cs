using Slottet.Domain.Entities;
using Slottet.Domain.Enums;

namespace Slottet.Application.Interfaces;

public interface IOverlapOverviewRepository
{
    Task<Department?> GetDepartmentByIdAsync(int departmentId, CancellationToken cancellationToken = default);
    Task<Shift?> GetShiftByIdAsync(int shiftId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Citizen>> GetActiveCitizensByDepartmentAsync(int departmentId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CitizenFixedMedication>> GetFixedMedicationsByCitizensAndShiftTypeAsync(
        IReadOnlyCollection<int> citizenIds,
        ShiftType shiftType,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MedicinRegistration>> GetMedicationsByShiftAsync(int shiftId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SpecialEvent>> GetSpecialEventsByShiftAsync(int shiftId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CitizenAssignment>> GetCitizenAssignmentsByShiftAsync(int shiftId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Employee>> GetEmployeesByIdsAsync(IReadOnlyCollection<int> employeeIds, CancellationToken cancellationToken = default);
}
