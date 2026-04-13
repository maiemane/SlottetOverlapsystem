namespace Slottet.Application.DTOs.Responsibilities;

public sealed class ResponsibilityAssignmentDto
{
    public int Id { get; set; }
    public int ShiftId { get; set; }
    public int ResponsibilityTypeId { get; set; }
    public string ResponsibilityTypeName { get; set; } = string.Empty;
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
}
