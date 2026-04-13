namespace Slottet.Application.DTOs.Responsibilities;

public sealed class CreateResponsibilityTypeResult
{
    public bool IsSuccess { get; set; }
    public string? Error { get; set; }
    public ResponsibilityTypeDto? ResponsibilityType { get; set; }
}
