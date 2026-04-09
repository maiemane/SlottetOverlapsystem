using Slottet.Application.DTOs.ShiftTasks;

namespace Slottet.Application.Interfaces;

public interface IShiftTaskService
{
    Task<IReadOnlyList<ShiftTaskDto>?> GetByShiftAsync(int shiftId, CancellationToken cancellationToken = default);
    Task<ShiftTaskDto?> GetByIdAsync(int shiftId, int shiftTaskId, CancellationToken cancellationToken = default);
    Task<CreateShiftTaskResult> CreateAsync(int shiftId, CreateShiftTaskRequest request, CancellationToken cancellationToken = default);
    Task<UpdateShiftTaskResult> UpdateAsync(int shiftId, int shiftTaskId, UpdateShiftTaskRequest request, CancellationToken cancellationToken = default);
    Task<DeleteShiftTaskResult> DeleteAsync(int shiftId, int shiftTaskId, CancellationToken cancellationToken = default);
}
