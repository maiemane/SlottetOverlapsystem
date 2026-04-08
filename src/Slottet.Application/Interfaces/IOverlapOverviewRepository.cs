using Slottet.Domain.Entities;

namespace Slottet.Application.Interfaces;

public interface IOverlapOverviewRepository
{
    Task<Department?> GetDepartmentByIdAsync(int departmentId, CancellationToken cancellationToken = default);
    Task<Shift?> GetShiftByIdAsync(int shiftId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Citizen>> GetActiveCitizensByDepartmentAsync(int departmentId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MedicinRegistration>> GetMedicationsByShiftAsync(int shiftId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SpecialEvent>> GetSpecialEventsByShiftAsync(int shiftId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CitizenAssignment>> GetCitizenAssignmentsByShiftAsync(int shiftId, CancellationToken cancellationToken = default);
}
