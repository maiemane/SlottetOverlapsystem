using Slottet.Domain.Enums;

namespace Slottet.Application.DTOs.ShiftTasks;

public sealed class UpdateShiftTaskRequest
{
    public string Description { get; set; } = string.Empty;
    public ShiftTaskStatus Status { get; set; }
}
