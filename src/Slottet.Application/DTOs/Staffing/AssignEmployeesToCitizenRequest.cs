namespace Slottet.Application.DTOs.Staffing;

public sealed class AssignEmployeesToCitizenRequest
{
    public List<int> EmployeeIds { get; set; } = [];
}
