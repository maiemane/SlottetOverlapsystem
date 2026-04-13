using Slottet.Application.DTOs.Citizens;
using Slottet.Application.Interfaces;
using Slottet.Domain.Entities;
using Slottet.Domain.Enums;

namespace Slottet.Application.Services.Citizens;

public sealed class CreateCitizenService : ICreateCitizenService
{
    private const string AnonymizedApartmentNumber = "ANON";
    private const string AnonymizedMedicationName = "Anonymiseret medicin";
    private const string AnonymizedDescription = "Anonymiseret";

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

    public async Task<CitizenPersonalDataExportDto?> ExportPersonalDataAsync(int citizenId, CancellationToken cancellationToken = default)
    {
        if (citizenId <= 0)
        {
            return null;
        }

        var citizen = await _citizenCreationRepository.GetCitizenByIdAsync(citizenId, cancellationToken);

        if (citizen is null)
        {
            return null;
        }

        var fixedMedications = await _citizenCreationRepository.GetFixedMedicationsByCitizenIdAsync(citizenId, cancellationToken);
        var medicationRegistrations = await _citizenCreationRepository.GetMedicationRegistrationsByCitizenIdAsync(citizenId, cancellationToken);
        var specialEvents = await _citizenCreationRepository.GetSpecialEventsByCitizenIdAsync(citizenId, cancellationToken);

        return new CitizenPersonalDataExportDto
        {
            ExportedAtUtc = DateTime.UtcNow,
            Citizen = new CitizenPersonalDataDto
            {
                Id = citizen.Id,
                Name = citizen.Name,
                ApartmentNumber = citizen.ApartmentNumber,
                TrafficLight = citizen.TrafficLight,
                DepartmentId = citizen.DepartmentId,
                IsActive = citizen.IsActive
            },
            FixedMedications = fixedMedications
                .OrderBy(x => x.ShiftType)
                .ThenBy(x => x.ScheduledTime)
                .Select(x => new CitizenFixedMedicationPersonalDataDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    ScheduledTime = x.ScheduledTime,
                    ShiftType = x.ShiftType,
                    IsActive = x.IsActive
                })
                .ToList(),
            MedicationRegistrations = medicationRegistrations
                .OrderByDescending(x => x.RegistrationTime)
                .Select(x => new CitizenMedicationRegistrationPersonalDataDto
                {
                    Id = x.Id,
                    ShiftId = x.ShiftId,
                    CitizenFixedMedicationId = x.CitizenFixedMedicationId,
                    MedicinType = x.MedicinType,
                    Name = x.Name,
                    Description = x.Description,
                    ScheduledTime = x.ScheduledTime,
                    RegistrationTime = x.RegistrationTime
                })
                .ToList(),
            SpecialEvents = specialEvents
                .OrderByDescending(x => x.EventTime)
                .Select(x => new CitizenSpecialEventPersonalDataDto
                {
                    Id = x.Id,
                    ShiftId = x.ShiftId,
                    Description = x.Description,
                    EventTime = x.EventTime
                })
                .ToList()
        };
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

    public async Task<AnonymizeCitizenResult> AnonymizeAsync(int citizenId, CancellationToken cancellationToken = default)
    {
        if (citizenId <= 0)
        {
            return new AnonymizeCitizenResult
            {
                IsSuccess = false,
                Error = "NotFound"
            };
        }

        var citizen = await _citizenCreationRepository.GetByIdAsync(citizenId, cancellationToken);

        if (citizen is null)
        {
            return new AnonymizeCitizenResult
            {
                IsSuccess = false,
                Error = "NotFound"
            };
        }

        var fixedMedications = await _citizenCreationRepository.GetFixedMedicationsByCitizenIdAsync(citizenId, cancellationToken);
        var medicationRegistrations = await _citizenCreationRepository.GetMedicationRegistrationsByCitizenIdAsync(citizenId, cancellationToken);
        var specialEvents = await _citizenCreationRepository.GetSpecialEventsByCitizenIdAsync(citizenId, cancellationToken);

        citizen.Name = BuildAnonymizedCitizenName(citizen.Id);
        citizen.ApartmentNumber = AnonymizedApartmentNumber;
        citizen.TrafficLight = TrafficLight.Grøn;
        citizen.IsActive = false;

        foreach (var fixedMedication in fixedMedications)
        {
            fixedMedication.Name = AnonymizedMedicationName;
            fixedMedication.Description = AnonymizedDescription;
            fixedMedication.IsActive = false;
        }

        foreach (var medicationRegistration in medicationRegistrations)
        {
            medicationRegistration.Name = AnonymizedMedicationName;
            medicationRegistration.Description = AnonymizedDescription;
        }

        foreach (var specialEvent in specialEvents)
        {
            specialEvent.Description = AnonymizedDescription;
        }

        await _citizenCreationRepository.UpdateCitizenAsync(citizen, cancellationToken);

        return new AnonymizeCitizenResult
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

    private static string BuildAnonymizedCitizenName(int citizenId)
    {
        return $"Anonymiseret borger #{citizenId}";
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
