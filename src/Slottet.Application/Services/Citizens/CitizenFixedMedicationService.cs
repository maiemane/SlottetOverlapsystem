using Slottet.Application.DTOs.Citizens;
using Slottet.Application.Interfaces;
using Slottet.Domain.Entities;
using Slottet.Domain.Enums;

namespace Slottet.Application.Services.Citizens;

public sealed class CitizenFixedMedicationService : ICitizenFixedMedicationService
{
    private readonly ICitizenCreationRepository _citizenCreationRepository;

    public CitizenFixedMedicationService(ICitizenCreationRepository citizenCreationRepository)
    {
        _citizenCreationRepository = citizenCreationRepository;
    }

    public async Task<IReadOnlyList<CitizenFixedMedicationDto>?> GetByCitizenAsync(int citizenId, CancellationToken cancellationToken = default)
    {
        var citizen = await _citizenCreationRepository.GetCitizenByIdAsync(citizenId, cancellationToken);

        if (citizen is null || !citizen.IsActive)
        {
            return null;
        }

        var medications = await _citizenCreationRepository.GetFixedMedicationsByCitizenIdAsync(citizenId, cancellationToken);

        return medications
            .OrderBy(medication => medication.ShiftType)
            .ThenBy(medication => medication.ScheduledTime)
            .ThenBy(medication => medication.Name)
            .Select(MapDto)
            .ToList();
    }

    public async Task<CitizenFixedMedicationDto?> GetByIdAsync(int citizenId, int fixedMedicationId, CancellationToken cancellationToken = default)
    {
        if (citizenId <= 0 || fixedMedicationId <= 0)
        {
            return null;
        }

        var citizen = await _citizenCreationRepository.GetCitizenByIdAsync(citizenId, cancellationToken);
        var fixedMedication = await _citizenCreationRepository.GetFixedMedicationByIdAsync(fixedMedicationId, cancellationToken);

        if (citizen is null || !citizen.IsActive || fixedMedication is null || fixedMedication.CitizenId != citizenId)
        {
            return null;
        }

        return MapDto(fixedMedication);
    }

    public async Task<UpdateCitizenFixedMedicationResult> UpdateAsync(int citizenId, int fixedMedicationId, UpdateCitizenFixedMedicationRequest request, CancellationToken cancellationToken = default)
    {
        if (citizenId <= 0 ||
            fixedMedicationId <= 0 ||
            string.IsNullOrWhiteSpace(request.Name) ||
            !Enum.IsDefined(typeof(ShiftType), request.ShiftType))
        {
            return new UpdateCitizenFixedMedicationResult
            {
                IsSuccess = false,
                Error = "InvalidRequest"
            };
        }

        var citizen = await _citizenCreationRepository.GetCitizenByIdAsync(citizenId, cancellationToken);
        var fixedMedication = await _citizenCreationRepository.GetFixedMedicationByIdAsync(fixedMedicationId, cancellationToken);

        if (citizen is null || !citizen.IsActive || fixedMedication is null || fixedMedication.CitizenId != citizenId)
        {
            return new UpdateCitizenFixedMedicationResult
            {
                IsSuccess = false,
                Error = "FixedMedicationNotFound"
            };
        }

        fixedMedication.Name = request.Name.Trim();
        fixedMedication.Description = request.Description.Trim();
        fixedMedication.ScheduledTime = request.ScheduledTime;
        fixedMedication.ShiftType = request.ShiftType;
        fixedMedication.IsActive = request.IsActive;

        var updatedMedication = await _citizenCreationRepository.UpdateFixedMedicationAsync(fixedMedication, cancellationToken);

        return new UpdateCitizenFixedMedicationResult
        {
            IsSuccess = true,
            FixedMedication = MapDto(updatedMedication)
        };
    }

    private static CitizenFixedMedicationDto MapDto(CitizenFixedMedication medication)
    {
        return new CitizenFixedMedicationDto
        {
            FixedMedicationId = medication.Id,
            CitizenId = medication.CitizenId,
            Name = medication.Name,
            Description = medication.Description,
            ScheduledTime = medication.ScheduledTime,
            ShiftType = medication.ShiftType,
            IsActive = medication.IsActive
        };
    }
}
