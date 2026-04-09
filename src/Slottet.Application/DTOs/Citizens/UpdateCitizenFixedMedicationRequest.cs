namespace Slottet.Application.DTOs.Citizens;

public sealed class UpdateCitizenFixedMedicationRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TimeOnly ScheduledTime { get; set; }
    public bool IsActive { get; set; } = true;
}
