using Slottet.Domain.Enums;

namespace Slottet.Application.DTOs.Citizens;

public sealed class CreateCitizenFixedMedicationResponse
{
    public int FixedMedicationId { get; set; }
    public int CitizenId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TimeOnly ScheduledTime { get; set; }
    public ShiftType ShiftType { get; set; }
    public bool IsActive { get; set; }
}
