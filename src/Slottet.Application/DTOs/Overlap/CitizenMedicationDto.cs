using Slottet.Domain.Enums;

namespace Slottet.Application.DTOs.Overlap;

public sealed class CitizenMedicationDto
{
    public int? FixedMedicationId { get; set; }
    public int? MedicationRegistrationId { get; set; }
    public MedicinType MedicinType { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime ScheduledTime { get; set; }
    public DateTime? RegistrationTime { get; set; }
    public bool IsRegistered { get; set; }
}
