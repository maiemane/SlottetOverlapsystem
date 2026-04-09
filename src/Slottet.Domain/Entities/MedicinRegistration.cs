namespace Slottet.Domain.Entities;
using Slottet.Domain.Enums;

public class MedicinRegistration
{
    public int Id { get; set; }
    public int CitizenId { get; set; } //foreign key
    public int ShiftId { get; set; } //foreign key
    public int? CitizenFixedMedicationId { get; set; } //nullable fk for fast medicin-plan
    public required MedicinType MedicinType { get; set; } //fast eller PN medicin
    public required string Name { get; set; } //medicin navn
    public string Description { get; set; } = string.Empty; // vgitigt ved PN medicin
    public DateTime ScheduledTime { get; set; }
    public DateTime RegistrationTime { get; set; }
}
