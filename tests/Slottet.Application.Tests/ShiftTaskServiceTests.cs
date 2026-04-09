using Slottet.Application.DTOs.ShiftTasks;
using Slottet.Application.Interfaces;
using Slottet.Application.Services.ShiftTasks;
using Slottet.Domain.Entities;
using Slottet.Domain.Enums;

namespace Slottet.Application.Tests;

public class ShiftTaskServiceTests
{
    [Fact]
    public async Task GetByShiftAsync_Returns_tasks_for_shift()
    {
        var repository = new FakeShiftTaskRepository();
        var sut = new ShiftTaskService(repository);

        var result = await sut.GetByShiftAsync(10, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(2, result!.Count);
        Assert.Equal("Fyld depotrummet op", result[0].Description);
    }

    [Fact]
    public async Task CreateAsync_Creates_open_task()
    {
        var repository = new FakeShiftTaskRepository();
        var sut = new ShiftTaskService(repository);

        var result = await sut.CreateAsync(10, new CreateShiftTaskRequest
        {
            Description = "Husk at fylde depotrummet op"
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.ShiftTask);
        Assert.Equal(3, result.ShiftTask!.Id);
        Assert.Equal(ShiftTaskStatus.Open, result.ShiftTask.Status);
    }

    [Fact]
    public async Task UpdateAsync_Updates_task_description_and_status()
    {
        var repository = new FakeShiftTaskRepository();
        var sut = new ShiftTaskService(repository);

        var result = await sut.UpdateAsync(10, 1, new UpdateShiftTaskRequest
        {
            Description = "Depotrummet er fyldt op",
            Status = ShiftTaskStatus.Done
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.ShiftTask);
        Assert.Equal("Depotrummet er fyldt op", result.ShiftTask!.Description);
        Assert.Equal(ShiftTaskStatus.Done, result.ShiftTask.Status);
    }

    [Fact]
    public async Task DeleteAsync_Removes_existing_task()
    {
        var repository = new FakeShiftTaskRepository();
        var sut = new ShiftTaskService(repository);

        var result = await sut.DeleteAsync(10, 1, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.DoesNotContain(repository.StoredTasks, task => task.Id == 1);
    }

    [Fact]
    public async Task CreateAsync_Returns_failure_when_shift_is_not_found()
    {
        var repository = new FakeShiftTaskRepository();
        var sut = new ShiftTaskService(repository);

        var result = await sut.CreateAsync(99, new CreateShiftTaskRequest
        {
            Description = "Husk kaffe"
        }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("ShiftNotFound", result.Error);
    }

    private sealed class FakeShiftTaskRepository : IShiftTaskRepository
    {
        public List<ShiftTask> StoredTasks { get; } =
        [
            new()
            {
                Id = 1,
                ShiftId = 10,
                Description = "Fyld depotrummet op",
                Status = ShiftTaskStatus.Open
            },
            new()
            {
                Id = 2,
                ShiftId = 10,
                Description = "Husk at bestille varer",
                Status = ShiftTaskStatus.Done
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

        public Task<IReadOnlyList<ShiftTask>> GetShiftTasksAsync(int shiftId, CancellationToken cancellationToken = default)
        {
            IReadOnlyList<ShiftTask> tasks = StoredTasks
                .Where(task => task.ShiftId == shiftId)
                .ToList();

            return Task.FromResult(tasks);
        }

        public Task<ShiftTask?> GetShiftTaskByIdAsync(int shiftTaskId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(StoredTasks.FirstOrDefault(task => task.Id == shiftTaskId));
        }

        public Task<ShiftTask> AddShiftTaskAsync(ShiftTask shiftTask, CancellationToken cancellationToken = default)
        {
            shiftTask.Id = 3;
            StoredTasks.Add(shiftTask);
            return Task.FromResult(shiftTask);
        }

        public Task<ShiftTask> UpdateShiftTaskAsync(ShiftTask shiftTask, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(shiftTask);
        }

        public Task DeleteShiftTaskAsync(ShiftTask shiftTask, CancellationToken cancellationToken = default)
        {
            StoredTasks.Remove(shiftTask);
            return Task.CompletedTask;
        }
    }
}
