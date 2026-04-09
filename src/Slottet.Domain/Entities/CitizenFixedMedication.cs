using Slottet.Domain.Enums;

namespace Slottet.Domain.Entities;

public class CitizenFixedMedication
{
    public int Id { get; set; }
    public int CitizenId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TimeOnly ScheduledTime { get; set; }
    public ShiftType ShiftType { get; set; }
    public bool IsActive { get; set; } = true;
}
