using Slottet.Application.DTOs.Citizens;
using Slottet.Application.Interfaces;
using Slottet.Application.Services.Citizens;
using Slottet.Domain.Entities;
using Slottet.Domain.Enums;

namespace Slottet.Application.Tests;

public class CitizenFixedMedicationServiceTests
{
    [Fact]
    public async Task GetByCitizenAsync_Returns_fixed_medications_for_active_citizen()
    {
        var repository = new FakeCitizenCreationRepository();
        var shiftDefinitionService = new FakeShiftDefinitionService();
        var sut = new CitizenFixedMedicationService(repository, shiftDefinitionService);

        var result = await sut.GetByCitizenAsync(1, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(2, result!.Count);
    }

    [Fact]
    public async Task GetByIdAsync_Returns_fixed_medication_when_it_belongs_to_citizen()
    {
        var repository = new FakeCitizenCreationRepository();
        var shiftDefinitionService = new FakeShiftDefinitionService();
        var sut = new CitizenFixedMedicationService(repository, shiftDefinitionService);

        var result = await sut.GetByIdAsync(1, 20, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(20, result!.FixedMedicationId);
        Assert.Equal(1, result.CitizenId);
    }

    [Fact]
    public async Task UpdateAsync_Updates_fixed_medication_when_request_is_valid()
    {
        var repository = new FakeCitizenCreationRepository();
        var shiftDefinitionService = new FakeShiftDefinitionService();
        var sut = new CitizenFixedMedicationService(repository, shiftDefinitionService);

        var result = await sut.UpdateAsync(1, 20, new UpdateCitizenFixedMedicationRequest
        {
            Name = "Pamol",
            Description = "1 tablet",
            ScheduledTime = new TimeOnly(20, 0),
            IsActive = false
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.FixedMedication);
        Assert.Equal("Pamol", result.FixedMedication!.Name);
        Assert.Equal(ShiftType.Aftenvagt, result.FixedMedication.ShiftType);
        Assert.False(result.FixedMedication.IsActive);
    }

    [Fact]
    public async Task UpdateAsync_Returns_failure_when_fixed_medication_is_not_found()
    {
        var repository = new FakeCitizenCreationRepository();
        var shiftDefinitionService = new FakeShiftDefinitionService();
        var sut = new CitizenFixedMedicationService(repository, shiftDefinitionService);

        var result = await sut.UpdateAsync(1, 999, new UpdateCitizenFixedMedicationRequest
        {
            Name = "Pamol",
            ScheduledTime = new TimeOnly(20, 0)
        }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("FixedMedicationNotFound", result.Error);
    }

    [Fact]
    public async Task UpdateAsync_Returns_failure_when_no_shift_definition_matches_time()
    {
        var repository = new FakeCitizenCreationRepository();
        var shiftDefinitionService = new FakeShiftDefinitionService
        {
            ResolvedShiftType = null
        };
        var sut = new CitizenFixedMedicationService(repository, shiftDefinitionService);

        var result = await sut.UpdateAsync(1, 20, new UpdateCitizenFixedMedicationRequest
        {
            Name = "Pamol",
            ScheduledTime = new TimeOnly(20, 0),
            IsActive = true
        }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("ShiftDefinitionNotFound", result.Error);
    }

    private sealed class FakeCitizenCreationRepository : ICitizenCreationRepository
    {
        private readonly List<CitizenFixedMedication> _fixedMedications =
        [
            new()
            {
                Id = 20,
                CitizenId = 1,
                Name = "Panodil",
                Description = "2 tabletter",
                ScheduledTime = new TimeOnly(8, 0),
                ShiftType = ShiftType.Dagvagt,
                IsActive = true
            },
            new()
            {
                Id = 21,
                CitizenId = 1,
                Name = "Melatonin",
                Description = "Til nat",
                ScheduledTime = new TimeOnly(22, 0),
                ShiftType = ShiftType.Nattevagt,
                IsActive = true
            }
        ];

        public Task<IReadOnlyList<Citizen>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<Citizen>>([]);
        }

        public Task<Citizen?> GetByIdAsync(int citizenId, CancellationToken cancellationToken = default)
        {
            return GetCitizenByIdAsync(citizenId, cancellationToken);
        }

        public Task<Citizen?> GetCitizenByIdAsync(int citizenId, CancellationToken cancellationToken = default)
        {
            Citizen? citizen = citizenId == 1
                ? new Citizen
                {
                    Id = 1,
                    Name = "Anna Jensen",
                    ApartmentNumber = "12A",
                    DepartmentId = 1,
                    TrafficLight = TrafficLight.Grøn,
                    IsActive = true
                }
                : null;

            return Task.FromResult(citizen);
        }

        public Task<Department?> GetDepartmentByIdAsync(int departmentId, CancellationToken cancellationToken = default)
        {
            Department? department = departmentId == 1
                ? new Department { Id = 1, Name = "Slottet" }
                : null;

            return Task.FromResult(department);
        }

        public Task<CitizenFixedMedication?> GetFixedMedicationByIdAsync(int fixedMedicationId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_fixedMedications.FirstOrDefault(medication => medication.Id == fixedMedicationId));
        }

        public Task<IReadOnlyList<CitizenFixedMedication>> GetFixedMedicationsByCitizenIdAsync(int citizenId, CancellationToken cancellationToken = default)
        {
            IReadOnlyList<CitizenFixedMedication> medications = _fixedMedications
                .Where(medication => medication.CitizenId == citizenId)
                .ToList();

            return Task.FromResult(medications);
        }

        public Task<IReadOnlyList<MedicinRegistration>> GetMedicationRegistrationsByCitizenIdAsync(int citizenId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<MedicinRegistration>>([]);
        }

        public Task<IReadOnlyList<SpecialEvent>> GetSpecialEventsByCitizenIdAsync(int citizenId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<SpecialEvent>>([]);
        }

        public Task<Citizen> AddCitizenAsync(Citizen citizen, CancellationToken cancellationToken = default)
        {
            citizen.Id = 10;
            return Task.FromResult(citizen);
        }

        public Task<Citizen> UpdateCitizenAsync(Citizen citizen, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(citizen);
        }

        public Task<bool> DeleteCitizenAsync(Citizen citizen, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(true);
        }

        public Task<CitizenFixedMedication> AddFixedMedicationAsync(CitizenFixedMedication fixedMedication, CancellationToken cancellationToken = default)
        {
            fixedMedication.Id = 22;
            _fixedMedications.Add(fixedMedication);
            return Task.FromResult(fixedMedication);
        }

        public Task<CitizenFixedMedication> UpdateFixedMedicationAsync(CitizenFixedMedication fixedMedication, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(fixedMedication);
        }
    }

    private sealed class FakeShiftDefinitionService : IShiftDefinitionService
    {
        public ShiftType? ResolvedShiftType { get; init; } = ShiftType.Aftenvagt;

        public Task<IReadOnlyList<Slottet.Application.DTOs.ShiftDefinitions.ShiftDefinitionDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<Slottet.Application.DTOs.ShiftDefinitions.ShiftDefinitionDto>>([]);
        }

        public Task<Slottet.Application.DTOs.ShiftDefinitions.ShiftDefinitionDto?> GetByIdAsync(int shiftDefinitionId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<Slottet.Application.DTOs.ShiftDefinitions.ShiftDefinitionDto?>(null);
        }

        public Task<Slottet.Application.DTOs.ShiftDefinitions.UpdateShiftDefinitionResultDto> UpdateAsync(int shiftDefinitionId, Slottet.Application.DTOs.ShiftDefinitions.UpdateShiftDefinitionRequestDto request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new Slottet.Application.DTOs.ShiftDefinitions.UpdateShiftDefinitionResultDto());
        }

        public Task<Slottet.Application.DTOs.ShiftDefinitions.ResolvedShiftTypeDto?> ResolveByTimeAsync(TimeOnly time, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<Slottet.Application.DTOs.ShiftDefinitions.ResolvedShiftTypeDto?>(null);
        }

        public Task<ShiftType?> ResolveShiftTypeAsync(TimeOnly time, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(ResolvedShiftType);
        }
    }
}
