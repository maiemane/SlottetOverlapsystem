using Slottet.Domain.Enums;

namespace Slottet.Application.DTOs.Medications;

public sealed class CreateMedicationRegistrationRequest
{
    public MedicinType MedicinType { get; set; }
    public int? CitizenFixedMedicationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime? ScheduledTime { get; set; }
    public DateTime RegistrationTime { get; set; }
}
