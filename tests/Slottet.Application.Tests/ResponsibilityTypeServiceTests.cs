using Slottet.Application.DTOs.Responsibilities;
using Slottet.Application.Interfaces;
using Slottet.Application.Services.Responsibilities;
using Slottet.Domain.Entities;

namespace Slottet.Application.Tests;

public class ResponsibilityTypeServiceTests
{
    [Fact]
    public async Task CreateAsync_CreatesResponsibilityType_WhenRequestIsValid()
    {
        var repository = new FakeResponsibilityTypeRepository();
        var sut = new ResponsibilityTypeService(repository);

        var result = await sut.CreateAsync(new CreateResponsibilityTypeRequest
        {
            Name = " Nattevagtstelefon "
        });

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.ResponsibilityType);
        Assert.Equal(2, result.ResponsibilityType!.Id);
        Assert.Equal("Nattevagtstelefon", result.ResponsibilityType.Name);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsError_WhenTypeIsInUse()
    {
        var repository = new FakeResponsibilityTypeRepository();
        var sut = new ResponsibilityTypeService(repository);

        var result = await sut.DeleteAsync(1);

        Assert.False(result.IsSuccess);
        Assert.Equal("ResponsibilityTypeInUse", result.Error);
    }

    private sealed class FakeResponsibilityTypeRepository : IResponsibilityTypeRepository
    {
        public List<ResponsibilityType> Types { get; } =
        [
            new ResponsibilityType
            {
                Id = 1,
                Name = "Medicinansvarlig"
            }
        ];

        public Task<IReadOnlyList<ResponsibilityType>> GetResponsibilityTypesAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<ResponsibilityType>>(Types);
        }

        public Task<ResponsibilityType?> GetResponsibilityTypeByIdAsync(int responsibilityTypeId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Types.FirstOrDefault(type => type.Id == responsibilityTypeId));
        }

        public Task<ResponsibilityType?> GetResponsibilityTypeByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Types.FirstOrDefault(type => string.Equals(type.Name, name, StringComparison.OrdinalIgnoreCase)));
        }

        public Task<bool> HasAssignmentsAsync(int responsibilityTypeId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(responsibilityTypeId == 1);
        }

        public Task<ResponsibilityType> AddResponsibilityTypeAsync(ResponsibilityType responsibilityType, CancellationToken cancellationToken = default)
        {
            responsibilityType.Id = Types.Max(type => type.Id) + 1;
            Types.Add(responsibilityType);
            return Task.FromResult(responsibilityType);
        }

        public Task<ResponsibilityType> UpdateResponsibilityTypeAsync(ResponsibilityType responsibilityType, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(responsibilityType);
        }

        public Task DeleteResponsibilityTypeAsync(ResponsibilityType responsibilityType, CancellationToken cancellationToken = default)
        {
            Types.Remove(responsibilityType);
            return Task.CompletedTask;
        }
    }
}
