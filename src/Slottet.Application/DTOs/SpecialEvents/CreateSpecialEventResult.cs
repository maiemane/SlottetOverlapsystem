namespace Slottet.Application.DTOs.SpecialEvents;

public sealed class CreateSpecialEventResult
{
    public bool IsSuccess { get; set; }
    public string? Error { get; set; }
    public SpecialEventDto? SpecialEvent { get; set; }
}
