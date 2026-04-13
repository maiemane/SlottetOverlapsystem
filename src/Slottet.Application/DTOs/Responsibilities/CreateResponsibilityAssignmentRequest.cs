namespace Slottet.Application.DTOs.Responsibilities;

public sealed class CreateResponsibilityAssignmentRequest
{
    public int ResponsibilityTypeId { get; set; }
    public int EmployeeId { get; set; }
}
