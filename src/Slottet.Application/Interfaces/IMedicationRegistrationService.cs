using Slottet.Application.DTOs.Medications;

namespace Slottet.Application.Interfaces;

public interface IMedicationRegistrationService
{
    Task<IReadOnlyList<MedicationRegistrationDto>?> GetByCitizenAndShiftAsync(int shiftId, int citizenId, CancellationToken cancellationToken = default);
    Task<CreateMedicationRegistrationResult> CreateAsync(int shiftId, int citizenId, CreateMedicationRegistrationRequest request, CancellationToken cancellationToken = default);
    Task<DeleteMedicationRegistrationResult> DeleteAsync(int shiftId, int citizenId, int medicationRegistrationId, CancellationToken cancellationToken = default);
}
