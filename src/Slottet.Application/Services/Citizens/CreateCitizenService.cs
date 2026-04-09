using Slottet.Application.DTOs.Citizens;
using Slottet.Application.Interfaces;
using Slottet.Domain.Entities;
using Slottet.Domain.Enums;

namespace Slottet.Application.Services.Citizens;

public sealed class CreateCitizenService : ICreateCitizenService
{
    private readonly ICitizenCreationRepository _citizenCreationRepository;

    public CreateCitizenService(ICitizenCreationRepository citizenCreationRepository)
    {
        _citizenCreationRepository = citizenCreationRepository;
    }

    public async Task<IReadOnlyList<CitizenDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var citizens = await _citizenCreationRepository.GetAllAsync(cancellationToken);

        return citizens
            .OrderBy(citizen => citizen.Name)
            .Select(MapCitizen)
            .ToList();
    }

    public async Task<CreateCitizenResult> CreateAsync(CreateCitizenRequest request, CancellationToken cancellationToken = default)
    {
        var validation = ValidateCitizenData(request.Name, request.ApartmentNumber, request.DepartmentId, request.TrafficLight);

        if (!validation.IsSuccess)
        {
            return new CreateCitizenResult
            {
                IsSuccess = false,
                Error = validation.Error
            };
        }

        var department = await _citizenCreationRepository.GetDepartmentByIdAsync(validation.DepartmentId!.Value, cancellationToken);

        if (department is null)
        {
            return new CreateCitizenResult
            {
                IsSuccess = false,
                Error = "DepartmentNotFound"
            };
        }

        var citizen = await _citizenCreationRepository.AddCitizenAsync(new Citizen
        {
            Name = validation.Name!,
            ApartmentNumber = validation.ApartmentNumber!,
            TrafficLight = validation.TrafficLight!.Value,
            DepartmentId = validation.DepartmentId.Value,
            IsActive = true
        }, cancellationToken);

        return new CreateCitizenResult
        {
            IsSuccess = true,
            Citizen = new CreateCitizenResponse
            {
                Id = citizen.Id,
                Name = citizen.Name,
                ApartmentNumber = citizen.ApartmentNumber,
                TrafficLight = citizen.TrafficLight,
                DepartmentId = citizen.DepartmentId,
                IsActive = citizen.IsActive
            }
        };
    }

    public async Task<UpdateCitizenResult> UpdateAsync(int citizenId, UpdateCitizenRequest request, CancellationToken cancellationToken = default)
    {
        if (citizenId <= 0)
        {
            return new UpdateCitizenResult
            {
                IsSuccess = false,
                Error = "NotFound"
            };
        }

        var citizen = await _citizenCreationRepository.GetByIdAsync(citizenId, cancellationToken);

        if (citizen is null)
        {
            return new UpdateCitizenResult
            {
                IsSuccess = false,
                Error = "NotFound"
            };
        }

        var validation = ValidateCitizenData(request.Name, request.ApartmentNumber, request.DepartmentId, request.TrafficLight);

        if (!validation.IsSuccess)
        {
            return new UpdateCitizenResult
            {
                IsSuccess = false,
                Error = validation.Error
            };
        }

        var department = await _citizenCreationRepository.GetDepartmentByIdAsync(validation.DepartmentId!.Value, cancellationToken);

        if (department is null)
        {
            return new UpdateCitizenResult
            {
                IsSuccess = false,
                Error = "DepartmentNotFound"
            };
        }

        citizen.Name = validation.Name!;
        citizen.ApartmentNumber = validation.ApartmentNumber!;
        citizen.TrafficLight = validation.TrafficLight!.Value;
        citizen.DepartmentId = validation.DepartmentId.Value;
        citizen.IsActive = request.IsActive;

        var updatedCitizen = await _citizenCreationRepository.UpdateCitizenAsync(citizen, cancellationToken);

        return new UpdateCitizenResult
        {
            IsSuccess = true,
            Citizen = MapCitizen(updatedCitizen)
        };
    }

    public async Task<DeleteCitizenResult> DeleteAsync(int citizenId, CancellationToken cancellationToken = default)
    {
        if (citizenId <= 0)
        {
            return new DeleteCitizenResult
            {
                IsSuccess = false,
                Error = "NotFound"
            };
        }

        var citizen = await _citizenCreationRepository.GetByIdAsync(citizenId, cancellationToken);

        if (citizen is null)
        {
            return new DeleteCitizenResult
            {
                IsSuccess = false,
                Error = "NotFound"
            };
        }

        var deleted = await _citizenCreationRepository.DeleteCitizenAsync(citizen, cancellationToken);

        if (!deleted)
        {
            return new DeleteCitizenResult
            {
                IsSuccess = false,
                Error = "HasRelations"
            };
        }

        return new DeleteCitizenResult
        {
            IsSuccess = true
        };
    }

    private static CitizenDto MapCitizen(Citizen citizen)
    {
        return new CitizenDto
        {
            Id = citizen.Id,
            Name = citizen.Name,
            ApartmentNumber = citizen.ApartmentNumber,
            TrafficLight = citizen.TrafficLight,
            DepartmentId = citizen.DepartmentId,
            IsActive = citizen.IsActive
        };
    }

    private static CitizenValidationResult ValidateCitizenData(string name, string apartmentNumber, int departmentId, TrafficLight trafficLight)
    {
        var normalizedName = name.Trim();
        var normalizedApartmentNumber = apartmentNumber.Trim();

        if (string.IsNullOrWhiteSpace(normalizedName) ||
            string.IsNullOrWhiteSpace(normalizedApartmentNumber) ||
            departmentId <= 0 ||
            !Enum.IsDefined(typeof(TrafficLight), trafficLight))
        {
            return new CitizenValidationResult
            {
                Error = "InvalidRequest"
            };
        }

        return new CitizenValidationResult
        {
            IsSuccess = true,
            Name = normalizedName,
            ApartmentNumber = normalizedApartmentNumber,
            DepartmentId = departmentId,
            TrafficLight = trafficLight
        };
    }

    private sealed class CitizenValidationResult
    {
        public bool IsSuccess { get; init; }
        public string? Error { get; init; }
        public string? Name { get; init; }
        public string? ApartmentNumber { get; init; }
        public int? DepartmentId { get; init; }
        public TrafficLight? TrafficLight { get; init; }
    }
}
