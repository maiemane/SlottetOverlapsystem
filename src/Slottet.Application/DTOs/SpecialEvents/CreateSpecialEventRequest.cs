namespace Slottet.Application.DTOs.SpecialEvents;

public sealed class CreateSpecialEventRequest
{
    public string Description { get; set; } = string.Empty;
    public DateTime EventTime { get; set; }
}
