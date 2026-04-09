using Slottet.Application.DTOs.SpecialEvents;
using Slottet.Application.Interfaces;
using Slottet.Domain.Entities;

namespace Slottet.Application.Services.SpecialEvents;

public sealed class SpecialEventService : ISpecialEventService
{
    private readonly ISpecialEventRepository _specialEventRepository;

    public SpecialEventService(ISpecialEventRepository specialEventRepository)
    {
        _specialEventRepository = specialEventRepository;
    }

    public async Task<IReadOnlyList<SpecialEventDto>?> GetByCitizenAndShiftAsync(int shiftId, int citizenId, CancellationToken cancellationToken = default)
    {
        var validation = await ValidateShiftAndCitizenAsync(shiftId, citizenId, cancellationToken);

        if (!validation.IsValid)
        {
            return null;
        }

        var specialEvents = await _specialEventRepository.GetSpecialEventsAsync(shiftId, citizenId, cancellationToken);

        return specialEvents
            .OrderByDescending(specialEvent => specialEvent.EventTime)
            .Select(MapDto)
            .ToList();
    }

    public async Task<SpecialEventDto?> GetByIdAsync(int shiftId, int citizenId, int specialEventId, CancellationToken cancellationToken = default)
    {
        if (specialEventId <= 0)
        {
            return null;
        }

        var validation = await ValidateShiftAndCitizenAsync(shiftId, citizenId, cancellationToken);

        if (!validation.IsValid)
        {
            return null;
        }

        var specialEvent = await _specialEventRepository.GetSpecialEventByIdAsync(specialEventId, cancellationToken);

        if (specialEvent is null || specialEvent.ShiftId != shiftId || specialEvent.CitizenId != citizenId)
        {
            return null;
        }

        return MapDto(specialEvent);
    }

    public async Task<CreateSpecialEventResult> CreateAsync(int shiftId, int citizenId, CreateSpecialEventRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Description) || request.EventTime == default)
        {
            return InvalidRequest();
        }

        var validation = await ValidateShiftAndCitizenAsync(shiftId, citizenId, cancellationToken);

        if (!validation.IsValid)
        {
            return validation.Error switch
            {
                "ShiftNotFound" => Failure("ShiftNotFound"),
                "CitizenNotFound" => Failure("CitizenNotFound"),
                "CitizenNotInShiftDepartment" => Failure("CitizenNotInShiftDepartment"),
                _ => Failure("InvalidRequest")
            };
        }

        var specialEvent = await _specialEventRepository.AddSpecialEventAsync(new SpecialEvent
        {
            ShiftId = shiftId,
            CitizenId = citizenId,
            Description = request.Description.Trim(),
            EventTime = request.EventTime
        }, cancellationToken);

        return new CreateSpecialEventResult
        {
            IsSuccess = true,
            SpecialEvent = MapDto(specialEvent)
        };
    }

    public async Task<UpdateSpecialEventResult> UpdateAsync(int shiftId, int citizenId, int specialEventId, UpdateSpecialEventRequest request, CancellationToken cancellationToken = default)
    {
        if (specialEventId <= 0 || string.IsNullOrWhiteSpace(request.Description) || request.EventTime == default)
        {
            return InvalidUpdateRequest();
        }

        var validation = await ValidateShiftAndCitizenAsync(shiftId, citizenId, cancellationToken);

        if (!validation.IsValid)
        {
            return validation.Error switch
            {
                "ShiftNotFound" => UpdateFailure("ShiftNotFound"),
                "CitizenNotFound" => UpdateFailure("CitizenNotFound"),
                "CitizenNotInShiftDepartment" => UpdateFailure("CitizenNotInShiftDepartment"),
                _ => InvalidUpdateRequest()
            };
        }

        var specialEvent = await _specialEventRepository.GetSpecialEventByIdAsync(specialEventId, cancellationToken);

        if (specialEvent is null || specialEvent.ShiftId != shiftId || specialEvent.CitizenId != citizenId)
        {
            return UpdateFailure("SpecialEventNotFound");
        }

        specialEvent.Description = request.Description.Trim();
        specialEvent.EventTime = request.EventTime;

        var updatedSpecialEvent = await _specialEventRepository.UpdateSpecialEventAsync(specialEvent, cancellationToken);

        return new UpdateSpecialEventResult
        {
            IsSuccess = true,
            SpecialEvent = MapDto(updatedSpecialEvent)
        };
    }

    public async Task<DeleteSpecialEventResult> DeleteAsync(int shiftId, int citizenId, int specialEventId, CancellationToken cancellationToken = default)
    {
        if (specialEventId <= 0)
        {
            return new DeleteSpecialEventResult
            {
                IsSuccess = false,
                Error = "SpecialEventNotFound"
            };
        }

        var validation = await ValidateShiftAndCitizenAsync(shiftId, citizenId, cancellationToken);

        if (!validation.IsValid)
        {
            return new DeleteSpecialEventResult
            {
                IsSuccess = false,
                Error = validation.Error
            };
        }

        var specialEvent = await _specialEventRepository.GetSpecialEventByIdAsync(specialEventId, cancellationToken);

        if (specialEvent is null || specialEvent.ShiftId != shiftId || specialEvent.CitizenId != citizenId)
        {
            return new DeleteSpecialEventResult
            {
                IsSuccess = false,
                Error = "SpecialEventNotFound"
            };
        }

        await _specialEventRepository.DeleteSpecialEventAsync(specialEvent, cancellationToken);

        return new DeleteSpecialEventResult
        {
            IsSuccess = true
        };
    }

    private async Task<ValidationResult> ValidateShiftAndCitizenAsync(int shiftId, int citizenId, CancellationToken cancellationToken)
    {
        if (shiftId <= 0)
        {
            return new ValidationResult { Error = "ShiftNotFound" };
        }

        if (citizenId <= 0)
        {
            return new ValidationResult { Error = "CitizenNotFound" };
        }

        var shift = await _specialEventRepository.GetShiftByIdAsync(shiftId, cancellationToken);

        if (shift is null)
        {
            return new ValidationResult { Error = "ShiftNotFound" };
        }

        var citizen = await _specialEventRepository.GetCitizenByIdAsync(citizenId, cancellationToken);

        if (citizen is null || !citizen.IsActive)
        {
            return new ValidationResult { Error = "CitizenNotFound" };
        }

        if (citizen.DepartmentId != shift.DepartmentId)
        {
            return new ValidationResult { Error = "CitizenNotInShiftDepartment" };
        }

        return new ValidationResult { IsValid = true };
    }

    private static SpecialEventDto MapDto(SpecialEvent specialEvent)
    {
        return new SpecialEventDto
        {
            Id = specialEvent.Id,
            CitizenId = specialEvent.CitizenId,
            ShiftId = specialEvent.ShiftId,
            Description = specialEvent.Description,
            EventTime = specialEvent.EventTime
        };
    }

    private static CreateSpecialEventResult InvalidRequest()
    {
        return Failure("InvalidRequest");
    }

    private static CreateSpecialEventResult Failure(string error)
    {
        return new CreateSpecialEventResult
        {
            IsSuccess = false,
            Error = error
        };
    }

    private static UpdateSpecialEventResult InvalidUpdateRequest()
    {
        return UpdateFailure("InvalidRequest");
    }

    private static UpdateSpecialEventResult UpdateFailure(string error)
    {
        return new UpdateSpecialEventResult
        {
            IsSuccess = false,
            Error = error
        };
    }

    private sealed class ValidationResult
    {
        public bool IsValid { get; init; }
        public string? Error { get; init; }
    }
}
