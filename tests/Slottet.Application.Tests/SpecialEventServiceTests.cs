using Slottet.Application.DTOs.SpecialEvents;
using Slottet.Application.Interfaces;
using Slottet.Application.Services.SpecialEvents;
using Slottet.Domain.Entities;
using Slottet.Domain.Enums;

namespace Slottet.Application.Tests;

public class SpecialEventServiceTests
{
    [Fact]
    public async Task GetByCitizenAndShiftAsync_Returns_special_events_sorted_descending()
    {
        var repository = new FakeSpecialEventRepository();
        var sut = new SpecialEventService(repository);

        var result = await sut.GetByCitizenAndShiftAsync(10, 1, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(2, result!.Count);
        Assert.Equal(101, result[0].Id);
        Assert.Equal(100, result[1].Id);
    }

    [Fact]
    public async Task CreateAsync_Returns_success_when_request_is_valid()
    {
        var repository = new FakeSpecialEventRepository();
        var sut = new SpecialEventService(repository);

        var result = await sut.CreateAsync(10, 1, new CreateSpecialEventRequest
        {
            Description = "Borger faldt i stuen",
            EventTime = new DateTime(2026, 4, 9, 9, 15, 0)
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.SpecialEvent);
        Assert.Equal(102, result.SpecialEvent!.Id);
        Assert.Equal("Borger faldt i stuen", result.SpecialEvent.Description);
    }

    [Fact]
    public async Task UpdateAsync_Updates_existing_special_event()
    {
        var repository = new FakeSpecialEventRepository();
        var sut = new SpecialEventService(repository);

        var result = await sut.UpdateAsync(10, 1, 100, new UpdateSpecialEventRequest
        {
            Description = "Borger faldt i stuen og blev tilset",
            EventTime = new DateTime(2026, 4, 9, 8, 20, 0)
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.SpecialEvent);
        Assert.Equal("Borger faldt i stuen og blev tilset", result.SpecialEvent!.Description);
    }

    [Fact]
    public async Task DeleteAsync_Removes_existing_special_event()
    {
        var repository = new FakeSpecialEventRepository();
        var sut = new SpecialEventService(repository);

        var result = await sut.DeleteAsync(10, 1, 100, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.DoesNotContain(repository.StoredSpecialEvents, specialEvent => specialEvent.Id == 100);
    }

    [Fact]
    public async Task CreateAsync_Returns_failure_when_citizen_is_not_in_shift_department()
    {
        var repository = new FakeSpecialEventRepository();
        var sut = new SpecialEventService(repository);

        var result = await sut.CreateAsync(10, 2, new CreateSpecialEventRequest
        {
            Description = "Uro",
            EventTime = new DateTime(2026, 4, 9, 9, 15, 0)
        }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("CitizenNotInShiftDepartment", result.Error);
    }

    private sealed class FakeSpecialEventRepository : ISpecialEventRepository
    {
        public List<SpecialEvent> StoredSpecialEvents { get; } =
        [
            new()
            {
                Id = 100,
                CitizenId = 1,
                ShiftId = 10,
                Description = "Borger faldt i stuen",
                EventTime = new DateTime(2026, 4, 9, 8, 15, 0)
            },
            new()
            {
                Id = 101,
                CitizenId = 1,
                ShiftId = 10,
                Description = "Borger virkede urolig",
                EventTime = new DateTime(2026, 4, 9, 10, 00, 0)
            }
        ];

        public Task<Shift?> GetShiftByIdAsync(int shiftId, CancellationToken cancellationToken = default)
        {
            Shift? shift = shiftId == 10
                ? new Shift
                {
                    Id = 10,
                    DepartmentId = 1,
                    Date = new DateTime(2026, 4, 9),
                    Type = ShiftType.Dagvagt
                }
                : null;

            return Task.FromResult(shift);
        }

        public Task<Citizen?> GetCitizenByIdAsync(int citizenId, CancellationToken cancellationToken = default)
        {
            Citizen? citizen = citizenId switch
            {
                1 => new Citizen
                {
                    Id = 1,
                    Name = "Anna Jensen",
                    ApartmentNumber = "12A",
                    DepartmentId = 1,
                    TrafficLight = TrafficLight.Grøn,
                    IsActive = true
                },
                2 => new Citizen
                {
                    Id = 2,
                    Name = "Peter Hansen",
                    ApartmentNumber = "10B",
                    DepartmentId = 2,
                    TrafficLight = TrafficLight.Gul,
                    IsActive = true
                },
                _ => null
            };

            return Task.FromResult(citizen);
        }

        public Task<IReadOnlyList<SpecialEvent>> GetSpecialEventsAsync(int shiftId, int citizenId, CancellationToken cancellationToken = default)
        {
            IReadOnlyList<SpecialEvent> result = StoredSpecialEvents
                .Where(specialEvent => specialEvent.ShiftId == shiftId && specialEvent.CitizenId == citizenId)
                .ToList();

            return Task.FromResult(result);
        }

        public Task<SpecialEvent?> GetSpecialEventByIdAsync(int specialEventId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(StoredSpecialEvents.FirstOrDefault(specialEvent => specialEvent.Id == specialEventId));
        }

        public Task<SpecialEvent> AddSpecialEventAsync(SpecialEvent specialEvent, CancellationToken cancellationToken = default)
        {
            specialEvent.Id = 102;
            StoredSpecialEvents.Add(specialEvent);
            return Task.FromResult(specialEvent);
        }

        public Task<SpecialEvent> UpdateSpecialEventAsync(SpecialEvent specialEvent, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(specialEvent);
        }

        public Task DeleteSpecialEventAsync(SpecialEvent specialEvent, CancellationToken cancellationToken = default)
        {
            StoredSpecialEvents.Remove(specialEvent);
            return Task.CompletedTask;
        }
    }
}
