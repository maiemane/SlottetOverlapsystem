namespace Slottet.Application.DTOs.Overlap;

public sealed class CitizenSpecialEventDto
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime EventTime { get; set; }
}
