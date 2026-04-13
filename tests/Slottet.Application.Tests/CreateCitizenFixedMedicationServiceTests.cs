using Slottet.Application.DTOs.Citizens;
using Slottet.Application.Interfaces;
using Slottet.Application.Services.Citizens;
using Slottet.Domain.Entities;
using Slottet.Domain.Enums;

namespace Slottet.Application.Tests;

public class CreateCitizenFixedMedicationServiceTests
{
    [Fact]
    public async Task CreateAsync_Returns_success_when_request_is_valid()
    {
        var repository = new FakeCitizenCreationRepository();
        var shiftDefinitionService = new FakeShiftDefinitionService();
        var sut = new CreateCitizenFixedMedicationService(repository, shiftDefinitionService);

        var result = await sut.CreateAsync(1, new CreateCitizenFixedMedicationRequest
        {
            Name = "Panodil",
            Description = "2 tabletter",
            ScheduledTime = new TimeOnly(8, 0)
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.FixedMedication);
        Assert.Equal(1, result.FixedMedication!.CitizenId);
        Assert.Equal("Panodil", result.FixedMedication.Name);
        Assert.Equal(ShiftType.Dagvagt, result.FixedMedication.ShiftType);
        Assert.True(result.FixedMedication.IsActive);
    }

    [Fact]
    public async Task CreateAsync_Returns_failure_when_citizen_does_not_exist()
    {
        var repository = new FakeCitizenCreationRepository();
        var shiftDefinitionService = new FakeShiftDefinitionService();
        var sut = new CreateCitizenFixedMedicationService(repository, shiftDefinitionService);

        var result = await sut.CreateAsync(99, new CreateCitizenFixedMedicationRequest
        {
            Name = "Panodil",
            ScheduledTime = new TimeOnly(8, 0)
        }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("CitizenNotFound", result.Error);
        Assert.Null(result.FixedMedication);
    }

    [Fact]
    public async Task CreateAsync_Returns_failure_when_request_is_invalid()
    {
        var repository = new FakeCitizenCreationRepository();
        var shiftDefinitionService = new FakeShiftDefinitionService();
        var sut = new CreateCitizenFixedMedicationService(repository, shiftDefinitionService);

        var result = await sut.CreateAsync(1, new CreateCitizenFixedMedicationRequest
        {
            Name = "",
            ScheduledTime = new TimeOnly(8, 0)
        }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("InvalidRequest", result.Error);
        Assert.Null(result.FixedMedication);
    }

    [Fact]
    public async Task CreateAsync_Returns_failure_when_no_shift_definition_matches_time()
    {
        var repository = new FakeCitizenCreationRepository();
        var shiftDefinitionService = new FakeShiftDefinitionService
        {
            ResolvedShiftType = null
        };
        var sut = new CreateCitizenFixedMedicationService(repository, shiftDefinitionService);

        var result = await sut.CreateAsync(1, new CreateCitizenFixedMedicationRequest
        {
            Name = "Panodil",
            ScheduledTime = new TimeOnly(8, 0)
        }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("ShiftDefinitionNotFound", result.Error);
        Assert.Null(result.FixedMedication);
    }

    private sealed class FakeCitizenCreationRepository : ICitizenCreationRepository
    {
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
            return Task.FromResult<CitizenFixedMedication?>(null);
        }

        public Task<IReadOnlyList<CitizenFixedMedication>> GetFixedMedicationsByCitizenIdAsync(int citizenId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<CitizenFixedMedication>>([]);
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
            fixedMedication.Id = 20;
            return Task.FromResult(fixedMedication);
        }

        public Task<CitizenFixedMedication> UpdateFixedMedicationAsync(CitizenFixedMedication fixedMedication, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(fixedMedication);
        }
    }

    private sealed class FakeShiftDefinitionService : IShiftDefinitionService
    {
        public ShiftType? ResolvedShiftType { get; init; } = ShiftType.Dagvagt;

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
