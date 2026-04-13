namespace Slottet.Application.DTOs.Responsibilities;

public sealed class CreateResponsibilityAssignmentResult
{
    public bool IsSuccess { get; set; }
    public string? Error { get; set; }
    public ResponsibilityAssignmentDto? ResponsibilityAssignment { get; set; }
}
