using Slottet.Domain.Enums;

namespace Slottet.Application.DTOs.Citizens;

public sealed class CreateCitizenFixedMedicationRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TimeOnly ScheduledTime { get; set; }
    public ShiftType ShiftType { get; set; }
}
