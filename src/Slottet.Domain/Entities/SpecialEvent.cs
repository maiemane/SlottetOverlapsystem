namespace Slottet.Domain.Entities;

public class SpecialEvent
{
    public int Id { get; set; }
    public string Description { get; set; }
    public DateTime EventTime { get; set; }
    public int CitizenId { get; set; } //foreign key
    public int ShiftId { get; set; } //foreign key
}