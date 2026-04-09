using Slottet.Domain.Enums;

namespace Slottet.Application.DTOs.ShiftTasks;

public sealed class ShiftTaskDto
{
    public int Id { get; set; }
    public int ShiftId { get; set; }
    public string Description { get; set; } = string.Empty;
    public ShiftTaskStatus Status { get; set; }
}
