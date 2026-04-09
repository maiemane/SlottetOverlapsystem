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
        var sut = new CreateCitizenFixedMedicationService(repository);

        var result = await sut.CreateAsync(1, new CreateCitizenFixedMedicationRequest
        {
            Name = "Panodil",
            Description = "2 tabletter",
            ScheduledTime = new TimeOnly(8, 0),
            ShiftType = ShiftType.Dagvagt
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
        var sut = new CreateCitizenFixedMedicationService(repository);

        var result = await sut.CreateAsync(99, new CreateCitizenFixedMedicationRequest
        {
            Name = "Panodil",
            ScheduledTime = new TimeOnly(8, 0),
            ShiftType = ShiftType.Dagvagt
        }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("CitizenNotFound", result.Error);
        Assert.Null(result.FixedMedication);
    }

    [Fact]
    public async Task CreateAsync_Returns_failure_when_request_is_invalid()
    {
        var repository = new FakeCitizenCreationRepository();
        var sut = new CreateCitizenFixedMedicationService(repository);

        var result = await sut.CreateAsync(1, new CreateCitizenFixedMedicationRequest
        {
            Name = "",
            ScheduledTime = new TimeOnly(8, 0),
            ShiftType = (ShiftType)99
        }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("InvalidRequest", result.Error);
        Assert.Null(result.FixedMedication);
    }

    private sealed class FakeCitizenCreationRepository : ICitizenCreationRepository
    {
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

        public Task<Citizen> AddCitizenAsync(Citizen citizen, CancellationToken cancellationToken = default)
        {
            citizen.Id = 10;
            return Task.FromResult(citizen);
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
}
