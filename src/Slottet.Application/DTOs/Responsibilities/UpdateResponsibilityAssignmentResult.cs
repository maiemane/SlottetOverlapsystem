namespace Slottet.Application.DTOs.Responsibilities;

public sealed class UpdateResponsibilityAssignmentResult
{
    public bool IsSuccess { get; set; }
    public string? Error { get; set; }
    public ResponsibilityAssignmentDto? ResponsibilityAssignment { get; set; }
}
