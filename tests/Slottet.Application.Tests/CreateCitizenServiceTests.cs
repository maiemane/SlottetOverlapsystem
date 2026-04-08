using Slottet.Application.DTOs.Citizens;
using Slottet.Application.Interfaces;
using Slottet.Application.Services.Citizens;
using Slottet.Domain.Entities;
using Slottet.Domain.Enums;

namespace Slottet.Application.Tests;

public class CreateCitizenServiceTests
{
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

    private sealed class FakeCitizenCreationRepository : ICitizenCreationRepository
    {
        public Task<Department?> GetDepartmentByIdAsync(int departmentId, CancellationToken cancellationToken = default)
        {
            Department? department = departmentId == 1
                ? new Department { Id = 1, Name = "Slottet" }
                : null;

            return Task.FromResult(department);
        }

        public Task<Citizen> AddCitizenAsync(Citizen citizen, CancellationToken cancellationToken = default)
        {
            citizen.Id = 10;
            return Task.FromResult(citizen);
        }
    }
}
