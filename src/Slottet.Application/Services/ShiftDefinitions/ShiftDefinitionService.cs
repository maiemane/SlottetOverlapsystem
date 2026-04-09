using Slottet.Application.DTOs.ShiftDefinitions;
using Slottet.Application.Interfaces;
using Slottet.Domain.Entities;
using Slottet.Domain.Enums;

namespace Slottet.Application.Services.ShiftDefinitions;

public sealed class ShiftDefinitionService : IShiftDefinitionService
{
    private readonly IShiftDefinitionRepository _shiftDefinitionRepository;

    public ShiftDefinitionService(IShiftDefinitionRepository shiftDefinitionRepository)
    {
        _shiftDefinitionRepository = shiftDefinitionRepository;
    }

    public async Task<IReadOnlyList<ShiftDefinitionDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var definitions = await _shiftDefinitionRepository.GetAllAsync(cancellationToken);

        return definitions
            .OrderBy(definition => definition.ShiftType)
            .Select(MapDto)
            .ToList();
    }

    public async Task<ShiftDefinitionDto?> GetByIdAsync(int shiftDefinitionId, CancellationToken cancellationToken = default)
    {
        if (shiftDefinitionId <= 0)
        {
            return null;
        }

        var definition = await _shiftDefinitionRepository.GetByIdAsync(shiftDefinitionId, cancellationToken);
        return definition is null ? null : MapDto(definition);
    }

    public async Task<UpdateShiftDefinitionResultDto> UpdateAsync(int shiftDefinitionId, UpdateShiftDefinitionRequestDto request, CancellationToken cancellationToken = default)
    {
        if (shiftDefinitionId <= 0 ||
            !Enum.IsDefined(typeof(ShiftType), request.ShiftType) ||
            request.StartTime == request.EndTime)
        {
            return InvalidResult();
        }

        var definition = await _shiftDefinitionRepository.GetByIdAsync(shiftDefinitionId, cancellationToken);

        if (definition is null)
        {
            return new UpdateShiftDefinitionResultDto
            {
                IsSuccess = false,
                Error = "NotFound"
            };
        }

        if (definition.ShiftType != request.ShiftType)
        {
            return new UpdateShiftDefinitionResultDto
            {
                IsSuccess = false,
                Error = "ShiftTypeMismatch"
            };
        }

        var allDefinitions = await _shiftDefinitionRepository.GetAllAsync(cancellationToken);
        var candidateDefinitions = allDefinitions
            .Select(existing => existing.Id == shiftDefinitionId
                ? new ShiftDefinition
                {
                    Id = existing.Id,
                    ShiftType = request.ShiftType,
                    StartTime = request.StartTime,
                    EndTime = request.EndTime,
                    IsActive = request.IsActive
                }
                : new ShiftDefinition
                {
                    Id = existing.Id,
                    ShiftType = existing.ShiftType,
                    StartTime = existing.StartTime,
                    EndTime = existing.EndTime,
                    IsActive = existing.IsActive
                })
            .ToList();

        if (!HasValidSchedule(candidateDefinitions))
        {
            return new UpdateShiftDefinitionResultDto
            {
                IsSuccess = false,
                Error = "InvalidSchedule"
            };
        }

        definition.StartTime = request.StartTime;
        definition.EndTime = request.EndTime;
        definition.IsActive = request.IsActive;

        var updatedDefinition = await _shiftDefinitionRepository.UpdateAsync(definition, cancellationToken);

        return new UpdateShiftDefinitionResultDto
        {
            IsSuccess = true,
            ShiftDefinition = MapDto(updatedDefinition)
        };
    }

    public async Task<ResolvedShiftTypeDto?> ResolveByTimeAsync(TimeOnly time, CancellationToken cancellationToken = default)
    {
        var definitions = await _shiftDefinitionRepository.GetAllAsync(cancellationToken);
        var definition = definitions
            .Where(candidate => candidate.IsActive)
            .OrderBy(candidate => candidate.ShiftType)
            .FirstOrDefault(candidate => ContainsTime(candidate, time));

        if (definition is null)
        {
            return null;
        }

        return new ResolvedShiftTypeDto
        {
            ShiftDefinitionId = definition.Id,
            ShiftType = definition.ShiftType,
            StartTime = definition.StartTime,
            EndTime = definition.EndTime,
            Time = time
        };
    }

    public async Task<ShiftType?> ResolveShiftTypeAsync(TimeOnly time, CancellationToken cancellationToken = default)
    {
        var resolved = await ResolveByTimeAsync(time, cancellationToken);
        return resolved?.ShiftType;
    }

    private static bool HasValidSchedule(IReadOnlyList<ShiftDefinition> definitions)
    {
        var activeDefinitions = definitions
            .Where(definition => definition.IsActive)
            .ToList();

        if (activeDefinitions.Count == 0)
        {
            return false;
        }

        foreach (var minute in Enumerable.Range(0, 24 * 60))
        {
            var time = TimeOnly.MinValue.AddMinutes(minute);
            var matches = activeDefinitions.Count(definition => ContainsTime(definition, time));

            if (matches > 1)
            {
                return false;
            }
        }

        return true;
    }

    private static bool ContainsTime(ShiftDefinition definition, TimeOnly time)
    {
        if (definition.StartTime < definition.EndTime)
        {
            return time >= definition.StartTime && time < definition.EndTime;
        }

        return time >= definition.StartTime || time < definition.EndTime;
    }

    private static ShiftDefinitionDto MapDto(ShiftDefinition definition)
    {
        return new ShiftDefinitionDto
        {
            Id = definition.Id,
            ShiftType = definition.ShiftType,
            StartTime = definition.StartTime,
            EndTime = definition.EndTime,
            IsActive = definition.IsActive
        };
    }

    private static UpdateShiftDefinitionResultDto InvalidResult()
    {
        return new UpdateShiftDefinitionResultDto
        {
            IsSuccess = false,
            Error = "InvalidRequest"
        };
    }
}
