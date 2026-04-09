namespace Slottet.Application.DTOs.ShiftDefinitions;

public sealed class UpdateShiftDefinitionResultDto
{
    public bool IsSuccess { get; set; }
    public string? Error { get; set; }
    public ShiftDefinitionDto? ShiftDefinition { get; set; }
}
