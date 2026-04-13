using Slottet.Application.DTOs.Responsibilities;
using Slottet.Application.Interfaces;
using Slottet.Domain.Entities;

namespace Slottet.Application.Services.Responsibilities;

public sealed class ResponsibilityTypeService : IResponsibilityTypeService
{
    private readonly IResponsibilityTypeRepository _responsibilityTypeRepository;

    public ResponsibilityTypeService(IResponsibilityTypeRepository responsibilityTypeRepository)
    {
        _responsibilityTypeRepository = responsibilityTypeRepository;
    }

    public async Task<IReadOnlyList<ResponsibilityTypeDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var responsibilityTypes = await _responsibilityTypeRepository.GetResponsibilityTypesAsync(cancellationToken);
        return responsibilityTypes
            .OrderBy(type => type.Name)
            .Select(MapResponsibilityType)
            .ToList();
    }

    public async Task<ResponsibilityTypeDto?> GetByIdAsync(int responsibilityTypeId, CancellationToken cancellationToken = default)
    {
        if (responsibilityTypeId <= 0)
        {
            return null;
        }

        var responsibilityType = await _responsibilityTypeRepository.GetResponsibilityTypeByIdAsync(responsibilityTypeId, cancellationToken);
        return responsibilityType is null ? null : MapResponsibilityType(responsibilityType);
    }

    public async Task<CreateResponsibilityTypeResult> CreateAsync(CreateResponsibilityTypeRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedName = NormalizeName(request.Name);

        if (normalizedName is null)
        {
            return CreateFailure("InvalidRequest");
        }

        var existingType = await _responsibilityTypeRepository.GetResponsibilityTypeByNameAsync(normalizedName, cancellationToken);

        if (existingType is not null)
        {
            return CreateFailure("ResponsibilityTypeAlreadyExists");
        }

        var createdType = await _responsibilityTypeRepository.AddResponsibilityTypeAsync(new ResponsibilityType
        {
            Name = normalizedName
        }, cancellationToken);

        return new CreateResponsibilityTypeResult
        {
            IsSuccess = true,
            ResponsibilityType = MapResponsibilityType(createdType)
        };
    }

    public async Task<UpdateResponsibilityTypeResult> UpdateAsync(int responsibilityTypeId, UpdateResponsibilityTypeRequest request, CancellationToken cancellationToken = default)
    {
        if (responsibilityTypeId <= 0)
        {
            return UpdateFailure("ResponsibilityTypeNotFound");
        }

        var normalizedName = NormalizeName(request.Name);

        if (normalizedName is null)
        {
            return UpdateFailure("InvalidRequest");
        }

        var responsibilityType = await _responsibilityTypeRepository.GetResponsibilityTypeByIdAsync(responsibilityTypeId, cancellationToken);

        if (responsibilityType is null)
        {
            return UpdateFailure("ResponsibilityTypeNotFound");
        }

        var existingType = await _responsibilityTypeRepository.GetResponsibilityTypeByNameAsync(normalizedName, cancellationToken);

        if (existingType is not null && existingType.Id != responsibilityTypeId)
        {
            return UpdateFailure("ResponsibilityTypeAlreadyExists");
        }

        responsibilityType.Name = normalizedName;

        var updatedType = await _responsibilityTypeRepository.UpdateResponsibilityTypeAsync(responsibilityType, cancellationToken);

        return new UpdateResponsibilityTypeResult
        {
            IsSuccess = true,
            ResponsibilityType = MapResponsibilityType(updatedType)
        };
    }

    public async Task<DeleteResponsibilityTypeResult> DeleteAsync(int responsibilityTypeId, CancellationToken cancellationToken = default)
    {
        if (responsibilityTypeId <= 0)
        {
            return Failure("ResponsibilityTypeNotFound");
        }

        var responsibilityType = await _responsibilityTypeRepository.GetResponsibilityTypeByIdAsync(responsibilityTypeId, cancellationToken);

        if (responsibilityType is null)
        {
            return Failure("ResponsibilityTypeNotFound");
        }

        if (await _responsibilityTypeRepository.HasAssignmentsAsync(responsibilityTypeId, cancellationToken))
        {
            return Failure("ResponsibilityTypeInUse");
        }

        await _responsibilityTypeRepository.DeleteResponsibilityTypeAsync(responsibilityType, cancellationToken);

        return new DeleteResponsibilityTypeResult
        {
            IsSuccess = true
        };
    }

    private static string? NormalizeName(string value)
    {
        var normalized = value?.Trim();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }

    private static ResponsibilityTypeDto MapResponsibilityType(ResponsibilityType responsibilityType)
    {
        return new ResponsibilityTypeDto
        {
            Id = responsibilityType.Id,
            Name = responsibilityType.Name
        };
    }

    private static CreateResponsibilityTypeResult CreateFailure(string error)
    {
        return new CreateResponsibilityTypeResult
        {
            IsSuccess = false,
            Error = error
        };
    }

    private static UpdateResponsibilityTypeResult UpdateFailure(string error)
    {
        return new UpdateResponsibilityTypeResult
        {
            IsSuccess = false,
            Error = error
        };
    }

    private static DeleteResponsibilityTypeResult Failure(string error)
    {
        return new DeleteResponsibilityTypeResult
        {
            IsSuccess = false,
            Error = error
        };
    }
}
