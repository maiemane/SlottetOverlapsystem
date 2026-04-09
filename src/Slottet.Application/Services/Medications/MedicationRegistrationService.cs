using Slottet.Application.DTOs.Medications;
using Slottet.Application.Interfaces;
using Slottet.Domain.Entities;
using Slottet.Domain.Enums;

namespace Slottet.Application.Services.Medications;

public sealed class MedicationRegistrationService : IMedicationRegistrationService
{
    private readonly IMedicationRegistrationRepository _medicationRegistrationRepository;

    public MedicationRegistrationService(IMedicationRegistrationRepository medicationRegistrationRepository)
    {
        _medicationRegistrationRepository = medicationRegistrationRepository;
    }

    public async Task<IReadOnlyList<MedicationRegistrationDto>?> GetByCitizenAndShiftAsync(int shiftId, int citizenId, CancellationToken cancellationToken = default)
    {
        if (shiftId <= 0 || citizenId <= 0)
        {
            return null;
        }

        var shift = await _medicationRegistrationRepository.GetShiftByIdAsync(shiftId, cancellationToken);
        var citizen = await _medicationRegistrationRepository.GetCitizenByIdAsync(citizenId, cancellationToken);

        if (shift is null || citizen is null || !citizen.IsActive || citizen.DepartmentId != shift.DepartmentId)
        {
            return null;
        }

        var registrations = await _medicationRegistrationRepository.GetMedicationRegistrationsAsync(shiftId, citizenId, cancellationToken);

        return registrations
            .OrderBy(registration => registration.ScheduledTime)
            .ThenBy(registration => registration.Name)
            .Select(MapDto)
            .ToList();
    }

    public async Task<CreateMedicationRegistrationResult> CreateAsync(int shiftId, int citizenId, CreateMedicationRegistrationRequest request, CancellationToken cancellationToken = default)
    {
        if (shiftId <= 0 || citizenId <= 0)
        {
            return InvalidRequest();
        }

        var shift = await _medicationRegistrationRepository.GetShiftByIdAsync(shiftId, cancellationToken);
        var citizen = await _medicationRegistrationRepository.GetCitizenByIdAsync(citizenId, cancellationToken);

        if (shift is null)
        {
            return new CreateMedicationRegistrationResult
            {
                IsSuccess = false,
                Error = "ShiftNotFound"
            };
        }

        if (citizen is null || !citizen.IsActive)
        {
            return new CreateMedicationRegistrationResult
            {
                IsSuccess = false,
                Error = "CitizenNotFound"
            };
        }

        if (citizen.DepartmentId != shift.DepartmentId)
        {
            return new CreateMedicationRegistrationResult
            {
                IsSuccess = false,
                Error = "CitizenNotInShiftDepartment"
            };
        }

        if (!Enum.IsDefined(typeof(MedicinType), request.MedicinType))
        {
            return InvalidRequest();
        }

        if (request.MedicinType == MedicinType.Fast)
        {
            return await CreateFixedMedicationRegistrationAsync(shift, citizen, request, cancellationToken);
        }

        return await CreatePnMedicationRegistrationAsync(shift, citizen, request, cancellationToken);
    }

    public async Task<DeleteMedicationRegistrationResult> DeleteAsync(int shiftId, int citizenId, int medicationRegistrationId, CancellationToken cancellationToken = default)
    {
        if (shiftId <= 0 || citizenId <= 0 || medicationRegistrationId <= 0)
        {
            return new DeleteMedicationRegistrationResult
            {
                IsSuccess = false,
                Error = "InvalidRequest"
            };
        }

        var shift = await _medicationRegistrationRepository.GetShiftByIdAsync(shiftId, cancellationToken);
        var citizen = await _medicationRegistrationRepository.GetCitizenByIdAsync(citizenId, cancellationToken);

        if (shift is null)
        {
            return new DeleteMedicationRegistrationResult
            {
                IsSuccess = false,
                Error = "ShiftNotFound"
            };
        }

        if (citizen is null || !citizen.IsActive)
        {
            return new DeleteMedicationRegistrationResult
            {
                IsSuccess = false,
                Error = "CitizenNotFound"
            };
        }

        if (citizen.DepartmentId != shift.DepartmentId)
        {
            return new DeleteMedicationRegistrationResult
            {
                IsSuccess = false,
                Error = "CitizenNotInShiftDepartment"
            };
        }

        var registration = await _medicationRegistrationRepository.GetMedicationRegistrationByIdAsync(medicationRegistrationId, cancellationToken);

        if (registration is null || registration.ShiftId != shiftId || registration.CitizenId != citizenId)
        {
            return new DeleteMedicationRegistrationResult
            {
                IsSuccess = false,
                Error = "RegistrationNotFound"
            };
        }

        await _medicationRegistrationRepository.DeleteMedicationRegistrationAsync(registration, cancellationToken);

        return new DeleteMedicationRegistrationResult
        {
            IsSuccess = true
        };
    }

    private async Task<CreateMedicationRegistrationResult> CreateFixedMedicationRegistrationAsync(
        Shift shift,
        Citizen citizen,
        CreateMedicationRegistrationRequest request,
        CancellationToken cancellationToken)
    {
        if (!request.CitizenFixedMedicationId.HasValue)
        {
            return InvalidRequest();
        }

        var fixedMedication = await _medicationRegistrationRepository.GetFixedMedicationByIdAsync(request.CitizenFixedMedicationId.Value, cancellationToken);

        if (fixedMedication is null || !fixedMedication.IsActive || fixedMedication.CitizenId != citizen.Id || fixedMedication.ShiftType != shift.Type)
        {
            return new CreateMedicationRegistrationResult
            {
                IsSuccess = false,
                Error = "FixedMedicationNotFound"
            };
        }

        var existingRegistration = await _medicationRegistrationRepository.GetFixedMedicationRegistrationAsync(shift.Id, fixedMedication.Id, cancellationToken);

        if (existingRegistration is not null)
        {
            return new CreateMedicationRegistrationResult
            {
                IsSuccess = false,
                Error = "AlreadyRegistered"
            };
        }

        var registration = await _medicationRegistrationRepository.AddMedicationRegistrationAsync(new MedicinRegistration
        {
            CitizenId = citizen.Id,
            ShiftId = shift.Id,
            CitizenFixedMedicationId = fixedMedication.Id,
            MedicinType = MedicinType.Fast,
            Name = fixedMedication.Name,
            Description = fixedMedication.Description,
            ScheduledTime = shift.Date.Date.Add(fixedMedication.ScheduledTime.ToTimeSpan()),
            RegistrationTime = request.RegistrationTime
        }, cancellationToken);

        return Success(registration);
    }

    private async Task<CreateMedicationRegistrationResult> CreatePnMedicationRegistrationAsync(
        Shift shift,
        Citizen citizen,
        CreateMedicationRegistrationRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return InvalidRequest();
        }

        var scheduledTime = request.ScheduledTime ?? request.RegistrationTime;

        var registration = await _medicationRegistrationRepository.AddMedicationRegistrationAsync(new MedicinRegistration
        {
            CitizenId = citizen.Id,
            ShiftId = shift.Id,
            CitizenFixedMedicationId = null,
            MedicinType = MedicinType.PN,
            Name = request.Name.Trim(),
            Description = request.Description.Trim(),
            ScheduledTime = scheduledTime,
            RegistrationTime = request.RegistrationTime
        }, cancellationToken);

        return Success(registration);
    }

    private static CreateMedicationRegistrationResult Success(MedicinRegistration registration)
    {
        return new CreateMedicationRegistrationResult
        {
            IsSuccess = true,
            Registration = MapDto(registration)
        };
    }

    private static CreateMedicationRegistrationResult InvalidRequest()
    {
        return new CreateMedicationRegistrationResult
        {
            IsSuccess = false,
            Error = "InvalidRequest"
        };
    }

    private static MedicationRegistrationDto MapDto(MedicinRegistration registration)
    {
        return new MedicationRegistrationDto
        {
            Id = registration.Id,
            CitizenId = registration.CitizenId,
            ShiftId = registration.ShiftId,
            CitizenFixedMedicationId = registration.CitizenFixedMedicationId,
            MedicinType = registration.MedicinType,
            Name = registration.Name,
            Description = registration.Description,
            ScheduledTime = registration.ScheduledTime,
            RegistrationTime = registration.RegistrationTime
        };
    }
}
