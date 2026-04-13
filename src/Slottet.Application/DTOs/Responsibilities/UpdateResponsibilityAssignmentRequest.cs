namespace Slottet.Application.DTOs.Responsibilities;

public sealed class UpdateResponsibilityAssignmentRequest
{
    public int ResponsibilityTypeId { get; set; }
    public int EmployeeId { get; set; }
}
