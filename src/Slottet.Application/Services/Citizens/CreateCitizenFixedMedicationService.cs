using Slottet.Application.DTOs.Citizens;
using Slottet.Application.Interfaces;
using Slottet.Domain.Entities;
using Slottet.Domain.Enums;

namespace Slottet.Application.Services.Citizens;

public sealed class CreateCitizenFixedMedicationService : ICreateCitizenFixedMedicationService
{
    private readonly ICitizenCreationRepository _citizenCreationRepository;

    public CreateCitizenFixedMedicationService(ICitizenCreationRepository citizenCreationRepository)
    {
        _citizenCreationRepository = citizenCreationRepository;
    }

    public async Task<CreateCitizenFixedMedicationResult> CreateAsync(int citizenId, CreateCitizenFixedMedicationRequest request, CancellationToken cancellationToken = default)
    {
        if (citizenId <= 0 ||
            string.IsNullOrWhiteSpace(request.Name) ||
            !Enum.IsDefined(typeof(ShiftType), request.ShiftType))
        {
            return new CreateCitizenFixedMedicationResult
            {
                IsSuccess = false,
                Error = "InvalidRequest"
            };
        }

        var citizen = await _citizenCreationRepository.GetCitizenByIdAsync(citizenId, cancellationToken);

        if (citizen is null || !citizen.IsActive)
        {
            return new CreateCitizenFixedMedicationResult
            {
                IsSuccess = false,
                Error = "CitizenNotFound"
            };
        }

        var fixedMedication = await _citizenCreationRepository.AddFixedMedicationAsync(new CitizenFixedMedication
        {
            CitizenId = citizenId,
            Name = request.Name.Trim(),
            Description = request.Description.Trim(),
            ScheduledTime = request.ScheduledTime,
            ShiftType = request.ShiftType,
            IsActive = true
        }, cancellationToken);

        return new CreateCitizenFixedMedicationResult
        {
            IsSuccess = true,
            FixedMedication = new CreateCitizenFixedMedicationResponse
            {
                FixedMedicationId = fixedMedication.Id,
                CitizenId = fixedMedication.CitizenId,
                Name = fixedMedication.Name,
                Description = fixedMedication.Description,
                ScheduledTime = fixedMedication.ScheduledTime,
                ShiftType = fixedMedication.ShiftType,
                IsActive = fixedMedication.IsActive
            }
        };
    }
}
