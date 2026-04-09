using Slottet.Application.DTOs.Citizens;

namespace Slottet.Application.Interfaces;

public interface ICitizenFixedMedicationService
{
    Task<IReadOnlyList<CitizenFixedMedicationDto>?> GetByCitizenAsync(int citizenId, CancellationToken cancellationToken = default);
    Task<CitizenFixedMedicationDto?> GetByIdAsync(int citizenId, int fixedMedicationId, CancellationToken cancellationToken = default);
    Task<UpdateCitizenFixedMedicationResult> UpdateAsync(int citizenId, int fixedMedicationId, UpdateCitizenFixedMedicationRequest request, CancellationToken cancellationToken = default);
}
