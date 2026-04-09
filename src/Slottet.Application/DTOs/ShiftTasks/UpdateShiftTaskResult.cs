namespace Slottet.Application.DTOs.ShiftTasks;

public sealed class UpdateShiftTaskResult
{
    public bool IsSuccess { get; set; }
    public string? Error { get; set; }
    public ShiftTaskDto? ShiftTask { get; set; }
}
