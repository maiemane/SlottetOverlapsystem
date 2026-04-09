using Slottet.Application.DTOs.ShiftDefinitions;
using Slottet.Application.Interfaces;
using Slottet.Application.Services.ShiftDefinitions;
using Slottet.Domain.Entities;
using Slottet.Domain.Enums;

namespace Slottet.Application.Tests;

public class ShiftDefinitionServiceTests
{
    [Fact]
    public async Task GetAllAsync_Returns_definitions_sorted_by_shift_type()
    {
        var repository = new FakeShiftDefinitionRepository();
        var sut = new ShiftDefinitionService(repository);

        var result = await sut.GetAllAsync(CancellationToken.None);

        Assert.Equal(3, result.Count);
        Assert.Equal(ShiftType.Dagvagt, result[0].ShiftType);
        Assert.Equal(ShiftType.Aftenvagt, result[1].ShiftType);
        Assert.Equal(ShiftType.Nattevagt, result[2].ShiftType);
    }

    [Fact]
    public async Task ResolveByTimeAsync_Returns_matching_shift_definition()
    {
        var repository = new FakeShiftDefinitionRepository();
        var sut = new ShiftDefinitionService(repository);

        var result = await sut.ResolveByTimeAsync(new TimeOnly(23, 30), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(ShiftType.Nattevagt, result!.ShiftType);
    }

    [Fact]
    public async Task UpdateAsync_Updates_definition_when_schedule_is_valid()
    {
        var repository = new FakeShiftDefinitionRepository();
        var sut = new ShiftDefinitionService(repository);

        var result = await sut.UpdateAsync(2, new UpdateShiftDefinitionRequestDto
        {
            ShiftType = ShiftType.Aftenvagt,
            StartTime = new TimeOnly(15, 30),
            EndTime = new TimeOnly(22, 30),
            IsActive = true
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.ShiftDefinition);
        Assert.Equal(new TimeOnly(15, 30), result.ShiftDefinition!.StartTime);
        Assert.Equal(new TimeOnly(22, 30), result.ShiftDefinition.EndTime);
    }

    [Fact]
    public async Task UpdateAsync_Returns_failure_when_schedule_overlaps()
    {
        var repository = new FakeShiftDefinitionRepository();
        var sut = new ShiftDefinitionService(repository);

        var result = await sut.UpdateAsync(2, new UpdateShiftDefinitionRequestDto
        {
            ShiftType = ShiftType.Aftenvagt,
            StartTime = new TimeOnly(10, 0),
            EndTime = new TimeOnly(18, 0),
            IsActive = true
        }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("InvalidSchedule", result.Error);
    }

    private sealed class FakeShiftDefinitionRepository : IShiftDefinitionRepository
    {
        private readonly List<ShiftDefinition> _definitions =
        [
            new() { Id = 1, ShiftType = ShiftType.Dagvagt, StartTime = new TimeOnly(7, 0), EndTime = new TimeOnly(15, 0), IsActive = true },
            new() { Id = 2, ShiftType = ShiftType.Aftenvagt, StartTime = new TimeOnly(15, 0), EndTime = new TimeOnly(23, 0), IsActive = true },
            new() { Id = 3, ShiftType = ShiftType.Nattevagt, StartTime = new TimeOnly(23, 0), EndTime = new TimeOnly(7, 0), IsActive = true }
        ];

        public Task<IReadOnlyList<ShiftDefinition>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<ShiftDefinition>>(_definitions
                .Select(definition => Clone(definition))
                .ToList());
        }

        public Task<ShiftDefinition?> GetByIdAsync(int shiftDefinitionId, CancellationToken cancellationToken = default)
        {
            var definition = _definitions.FirstOrDefault(candidate => candidate.Id == shiftDefinitionId);
            return Task.FromResult(definition);
        }

        public Task<ShiftDefinition> UpdateAsync(ShiftDefinition shiftDefinition, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(shiftDefinition);
        }

        private static ShiftDefinition Clone(ShiftDefinition definition)
        {
            return new ShiftDefinition
            {
                Id = definition.Id,
                ShiftType = definition.ShiftType,
                StartTime = definition.StartTime,
                EndTime = definition.EndTime,
                IsActive = definition.IsActive
            };
        }
    }
}
