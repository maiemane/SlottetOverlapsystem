using Slottet.Application.DTOs.Citizens;
using Slottet.Application.Interfaces;
using Slottet.Domain.Entities;

namespace Slottet.Application.Services.Citizens;

public sealed class CreateCitizenFixedMedicationService : ICreateCitizenFixedMedicationService
{
    private readonly ICitizenCreationRepository _citizenCreationRepository;
    private readonly IShiftDefinitionService _shiftDefinitionService;

    public CreateCitizenFixedMedicationService(
        ICitizenCreationRepository citizenCreationRepository,
        IShiftDefinitionService shiftDefinitionService)
    {
        _citizenCreationRepository = citizenCreationRepository;
        _shiftDefinitionService = shiftDefinitionService;
    }

    public async Task<CreateCitizenFixedMedicationResult> CreateAsync(int citizenId, CreateCitizenFixedMedicationRequest request, CancellationToken cancellationToken = default)
    {
        if (citizenId <= 0 || string.IsNullOrWhiteSpace(request.Name))
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

        var shiftType = await _shiftDefinitionService.ResolveShiftTypeAsync(request.ScheduledTime, cancellationToken);

        if (shiftType is null)
        {
            return new CreateCitizenFixedMedicationResult
            {
                IsSuccess = false,
                Error = "ShiftDefinitionNotFound"
            };
        }

        var fixedMedication = await _citizenCreationRepository.AddFixedMedicationAsync(new CitizenFixedMedication
        {
            CitizenId = citizenId,
            Name = request.Name.Trim(),
            Description = request.Description.Trim(),
            ScheduledTime = request.ScheduledTime,
            ShiftType = shiftType.Value,
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
