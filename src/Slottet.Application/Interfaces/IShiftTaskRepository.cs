using Slottet.Domain.Entities;

namespace Slottet.Application.Interfaces;

public interface IShiftTaskRepository
{
    Task<Shift?> GetShiftByIdAsync(int shiftId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ShiftTask>> GetShiftTasksAsync(int shiftId, CancellationToken cancellationToken = default);
    Task<ShiftTask?> GetShiftTaskByIdAsync(int shiftTaskId, CancellationToken cancellationToken = default);
    Task<ShiftTask> AddShiftTaskAsync(ShiftTask shiftTask, CancellationToken cancellationToken = default);
    Task<ShiftTask> UpdateShiftTaskAsync(ShiftTask shiftTask, CancellationToken cancellationToken = default);
    Task DeleteShiftTaskAsync(ShiftTask shiftTask, CancellationToken cancellationToken = default);
}
