namespace Slottet.Application.DTOs.SpecialEvents;

public sealed class UpdateSpecialEventRequest
{
    public string Description { get; set; } = string.Empty;
    public DateTime EventTime { get; set; }
}
