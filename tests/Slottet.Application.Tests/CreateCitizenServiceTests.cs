using Slottet.Application.DTOs.Citizens;
using Slottet.Application.Interfaces;
using Slottet.Application.Services.Citizens;
using Slottet.Domain.Entities;
using Slottet.Domain.Enums;

namespace Slottet.Application.Tests;

public class CreateCitizenServiceTests
{
    [Fact]
    public async Task GetAllAsync_Returns_sorted_citizen_list()
    {
        var repository = new FakeCitizenCreationRepository
        {
            Citizens =
            [
                new Citizen { Id = 2, Name = "Maise", ApartmentNumber = "10", TrafficLight = TrafficLight.Grøn, DepartmentId = 1, IsActive = true },
                new Citizen { Id = 1, Name = "Anna Jensen", ApartmentNumber = "5", TrafficLight = TrafficLight.Gul, DepartmentId = 2, IsActive = false }
            ]
        };
        var sut = new CreateCitizenService(repository);

        var result = await sut.GetAllAsync(CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Equal("Anna Jensen", result[0].Name);
        Assert.Equal("Maise", result[1].Name);
    }

    [Fact]
    public async Task CreateAsync_Returns_success_when_request_is_valid()
    {
        var repository = new FakeCitizenCreationRepository();
        var sut = new CreateCitizenService(repository);

        var result = await sut.CreateAsync(new CreateCitizenRequest
        {
            Name = "Anna Jensen",
            ApartmentNumber = "12A",
            TrafficLight = TrafficLight.Grøn,
            DepartmentId = 1
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Citizen);
        Assert.Equal("Anna Jensen", result.Citizen!.Name);
        Assert.Equal("12A", result.Citizen.ApartmentNumber);
        Assert.Equal(TrafficLight.Grøn, result.Citizen.TrafficLight);
        Assert.Equal(1, result.Citizen.DepartmentId);
        Assert.True(result.Citizen.IsActive);
        Assert.Equal(10, result.Citizen.Id);
    }

    [Fact]
    public async Task CreateAsync_Returns_failure_when_department_does_not_exist()
    {
        var repository = new FakeCitizenCreationRepository();
        var sut = new CreateCitizenService(repository);

        var result = await sut.CreateAsync(new CreateCitizenRequest
        {
            Name = "Anna Jensen",
            ApartmentNumber = "12A",
            TrafficLight = TrafficLight.Grøn,
            DepartmentId = 99
        }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("DepartmentNotFound", result.Error);
        Assert.Null(result.Citizen);
    }

    [Fact]
    public async Task CreateAsync_Returns_failure_when_request_is_invalid()
    {
        var repository = new FakeCitizenCreationRepository();
        var sut = new CreateCitizenService(repository);

        var result = await sut.CreateAsync(new CreateCitizenRequest
        {
            Name = "",
            ApartmentNumber = "",
            DepartmentId = 0,
            TrafficLight = (TrafficLight)99
        }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("InvalidRequest", result.Error);
        Assert.Null(result.Citizen);
    }

    [Fact]
    public async Task UpdateAsync_Updates_citizen_when_request_is_valid()
    {
        var repository = new FakeCitizenCreationRepository
        {
            CitizenById = new Citizen
            {
                Id = 10,
                Name = "Anna Jensen",
                ApartmentNumber = "12A",
                TrafficLight = TrafficLight.Grøn,
                DepartmentId = 1,
                IsActive = true
            }
        };
        var sut = new CreateCitizenService(repository);

        var result = await sut.UpdateAsync(10, new UpdateCitizenRequest
        {
            Name = "Anna Holm",
            ApartmentNumber = "14B",
            TrafficLight = TrafficLight.Gul,
            DepartmentId = 2,
            IsActive = false
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Citizen);
        Assert.Equal("Anna Holm", repository.UpdatedCitizen!.Name);
        Assert.Equal("14B", repository.UpdatedCitizen.ApartmentNumber);
        Assert.Equal(TrafficLight.Gul, repository.UpdatedCitizen.TrafficLight);
        Assert.Equal(2, repository.UpdatedCitizen.DepartmentId);
        Assert.False(repository.UpdatedCitizen.IsActive);
    }

    [Fact]
    public async Task DeleteAsync_Returns_success_when_citizen_is_deleted()
    {
        var repository = new FakeCitizenCreationRepository
        {
            CitizenById = new Citizen { Id = 10, Name = "Anna Jensen" }
        };
        var sut = new CreateCitizenService(repository);

        var result = await sut.DeleteAsync(10, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(10, repository.DeletedCitizenId);
    }

    [Fact]
    public async Task DeleteAsync_Returns_failure_when_citizen_has_relations()
    {
        var repository = new FakeCitizenCreationRepository
        {
            CitizenById = new Citizen { Id = 10, Name = "Anna Jensen" },
            DeleteSucceeds = false
        };
        var sut = new CreateCitizenService(repository);

        var result = await sut.DeleteAsync(10, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("HasRelations", result.Error);
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
        public IReadOnlyList<Citizen> Citizens { get; init; } = [];
        public Citizen? CitizenById { get; init; }
        public Citizen? UpdatedCitizen { get; private set; }
        public int? DeletedCitizenId { get; private set; }
        public bool DeleteSucceeds { get; init; } = true;

        public Task<IReadOnlyList<Citizen>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Citizens);
        }

        public Task<Citizen?> GetByIdAsync(int citizenId, CancellationToken cancellationToken = default)
        {
            if (CitizenById?.Id == citizenId)
            {
                return Task.FromResult<Citizen?>(CitizenById);
            }

            return Task.FromResult<Citizen?>(null);
        }

        public Task<Department?> GetDepartmentByIdAsync(int departmentId, CancellationToken cancellationToken = default)
        {
            Department? department = departmentId switch
            {
                1 => new Department { Id = 1, Name = "Slottet" },
                2 => new Department { Id = 2, Name = "Skoven" },
                _ => null
            };

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
        public Task<Citizen> UpdateCitizenAsync(Citizen citizen, CancellationToken cancellationToken = default)
        {
            UpdatedCitizen = citizen;
            return Task.FromResult(citizen);
        }

        public Task<bool> DeleteCitizenAsync(Citizen citizen, CancellationToken cancellationToken = default)
        {
            DeletedCitizenId = citizen.Id;
            return Task.FromResult(DeleteSucceeds);
        }
    }
}
