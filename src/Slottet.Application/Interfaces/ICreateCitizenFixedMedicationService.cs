using Slottet.Application.DTOs.Citizens;

namespace Slottet.Application.Interfaces;

public interface ICreateCitizenFixedMedicationService
{
    Task<CreateCitizenFixedMedicationResult> CreateAsync(int citizenId, CreateCitizenFixedMedicationRequest request, CancellationToken cancellationToken = default);
}
