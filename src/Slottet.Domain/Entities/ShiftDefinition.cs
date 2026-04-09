using Slottet.Domain.Enums;

namespace Slottet.Domain.Entities;

public class ShiftDefinition
{
    public int Id { get; set; }
    public ShiftType ShiftType { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public bool IsActive { get; set; } = true;
}
