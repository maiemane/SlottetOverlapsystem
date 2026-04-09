namespace Slottet.Application.DTOs.ShiftTasks;

public sealed class CreateShiftTaskResult
{
    public bool IsSuccess { get; set; }
    public string? Error { get; set; }
    public ShiftTaskDto? ShiftTask { get; set; }
}
