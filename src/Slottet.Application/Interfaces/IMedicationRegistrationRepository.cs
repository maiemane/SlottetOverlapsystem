using Slottet.Domain.Entities;

namespace Slottet.Application.Interfaces;

public interface IMedicationRegistrationRepository
{
    Task<Shift?> GetShiftByIdAsync(int shiftId, CancellationToken cancellationToken = default);
    Task<Citizen?> GetCitizenByIdAsync(int citizenId, CancellationToken cancellationToken = default);
    Task<CitizenFixedMedication?> GetFixedMedicationByIdAsync(int fixedMedicationId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MedicinRegistration>> GetMedicationRegistrationsAsync(int shiftId, int citizenId, CancellationToken cancellationToken = default);
    Task<MedicinRegistration?> GetFixedMedicationRegistrationAsync(int shiftId, int citizenFixedMedicationId, CancellationToken cancellationToken = default);
    Task<MedicinRegistration> AddMedicationRegistrationAsync(MedicinRegistration registration, CancellationToken cancellationToken = default);
}
