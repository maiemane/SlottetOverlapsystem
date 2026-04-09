using Slottet.Application.DTOs.ShiftTasks;
using Slottet.Application.Interfaces;
using Slottet.Domain.Entities;
using Slottet.Domain.Enums;

namespace Slottet.Application.Services.ShiftTasks;

public sealed class ShiftTaskService : IShiftTaskService
{
    private readonly IShiftTaskRepository _shiftTaskRepository;

    public ShiftTaskService(IShiftTaskRepository shiftTaskRepository)
    {
        _shiftTaskRepository = shiftTaskRepository;
    }

    public async Task<IReadOnlyList<ShiftTaskDto>?> GetByShiftAsync(int shiftId, CancellationToken cancellationToken = default)
    {
        if (shiftId <= 0)
        {
            return null;
        }

        var shift = await _shiftTaskRepository.GetShiftByIdAsync(shiftId, cancellationToken);

        if (shift is null)
        {
            return null;
        }

        var tasks = await _shiftTaskRepository.GetShiftTasksAsync(shiftId, cancellationToken);
        return tasks
            .OrderBy(task => task.Status)
            .ThenBy(task => task.Id)
            .Select(MapTask)
            .ToList();
    }

    public async Task<ShiftTaskDto?> GetByIdAsync(int shiftId, int shiftTaskId, CancellationToken cancellationToken = default)
    {
        if (shiftId <= 0 || shiftTaskId <= 0)
        {
            return null;
        }

        var shift = await _shiftTaskRepository.GetShiftByIdAsync(shiftId, cancellationToken);

        if (shift is null)
        {
            return null;
        }

        var shiftTask = await _shiftTaskRepository.GetShiftTaskByIdAsync(shiftTaskId, cancellationToken);

        if (shiftTask is null || shiftTask.ShiftId != shiftId)
        {
            return null;
        }

        return MapTask(shiftTask);
    }

    public async Task<CreateShiftTaskResult> CreateAsync(int shiftId, CreateShiftTaskRequest request, CancellationToken cancellationToken = default)
    {
        if (shiftId <= 0 || string.IsNullOrWhiteSpace(request.Description))
        {
            return InvalidCreate();
        }

        var shift = await _shiftTaskRepository.GetShiftByIdAsync(shiftId, cancellationToken);

        if (shift is null)
        {
            return CreateFailure("ShiftNotFound");
        }

        var shiftTask = await _shiftTaskRepository.AddShiftTaskAsync(new ShiftTask
        {
            ShiftId = shiftId,
            Description = request.Description.Trim(),
            Status = ShiftTaskStatus.Open
        }, cancellationToken);

        return new CreateShiftTaskResult
        {
            IsSuccess = true,
            ShiftTask = MapTask(shiftTask)
        };
    }

    public async Task<UpdateShiftTaskResult> UpdateAsync(int shiftId, int shiftTaskId, UpdateShiftTaskRequest request, CancellationToken cancellationToken = default)
    {
        if (shiftId <= 0 ||
            shiftTaskId <= 0 ||
            string.IsNullOrWhiteSpace(request.Description) ||
            !Enum.IsDefined(typeof(ShiftTaskStatus), request.Status))
        {
            return InvalidUpdate();
        }

        var shift = await _shiftTaskRepository.GetShiftByIdAsync(shiftId, cancellationToken);

        if (shift is null)
        {
            return UpdateFailure("ShiftNotFound");
        }

        var shiftTask = await _shiftTaskRepository.GetShiftTaskByIdAsync(shiftTaskId, cancellationToken);

        if (shiftTask is null || shiftTask.ShiftId != shiftId)
        {
            return UpdateFailure("ShiftTaskNotFound");
        }

        shiftTask.Description = request.Description.Trim();
        shiftTask.Status = request.Status;

        var updatedTask = await _shiftTaskRepository.UpdateShiftTaskAsync(shiftTask, cancellationToken);

        return new UpdateShiftTaskResult
        {
            IsSuccess = true,
            ShiftTask = MapTask(updatedTask)
        };
    }

    public async Task<DeleteShiftTaskResult> DeleteAsync(int shiftId, int shiftTaskId, CancellationToken cancellationToken = default)
    {
        if (shiftId <= 0 || shiftTaskId <= 0)
        {
            return new DeleteShiftTaskResult
            {
                IsSuccess = false,
                Error = "ShiftTaskNotFound"
            };
        }

        var shift = await _shiftTaskRepository.GetShiftByIdAsync(shiftId, cancellationToken);

        if (shift is null)
        {
            return new DeleteShiftTaskResult
            {
                IsSuccess = false,
                Error = "ShiftNotFound"
            };
        }

        var shiftTask = await _shiftTaskRepository.GetShiftTaskByIdAsync(shiftTaskId, cancellationToken);

        if (shiftTask is null || shiftTask.ShiftId != shiftId)
        {
            return new DeleteShiftTaskResult
            {
                IsSuccess = false,
                Error = "ShiftTaskNotFound"
            };
        }

        await _shiftTaskRepository.DeleteShiftTaskAsync(shiftTask, cancellationToken);

        return new DeleteShiftTaskResult
        {
            IsSuccess = true
        };
    }

    private static ShiftTaskDto MapTask(ShiftTask task)
    {
        return new ShiftTaskDto
        {
            Id = task.Id,
            ShiftId = task.ShiftId,
            Description = task.Description,
            Status = task.Status
        };
    }

    private static CreateShiftTaskResult InvalidCreate()
    {
        return CreateFailure("InvalidRequest");
    }

    private static CreateShiftTaskResult CreateFailure(string error)
    {
        return new CreateShiftTaskResult
        {
            IsSuccess = false,
            Error = error
        };
    }

    private static UpdateShiftTaskResult InvalidUpdate()
    {
        return UpdateFailure("InvalidRequest");
    }

    private static UpdateShiftTaskResult UpdateFailure(string error)
    {
        return new UpdateShiftTaskResult
        {
            IsSuccess = false,
            Error = error
        };
    }
}
