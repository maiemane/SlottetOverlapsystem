namespace Slottet.Application.DTOs.SpecialEvents;

public sealed class SpecialEventDto
{
    public int Id { get; set; }
    public int CitizenId { get; set; }
    public int ShiftId { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime EventTime { get; set; }
}
