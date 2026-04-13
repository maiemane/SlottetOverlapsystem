namespace Slottet.Application.DTOs.Responsibilities;

public sealed class UpdateResponsibilityTypeResult
{
    public bool IsSuccess { get; set; }
    public string? Error { get; set; }
    public ResponsibilityTypeDto? ResponsibilityType { get; set; }
}
