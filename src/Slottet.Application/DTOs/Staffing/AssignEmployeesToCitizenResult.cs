namespace Slottet.Application.DTOs.Staffing;

public sealed class AssignEmployeesToCitizenResult
{
    public bool IsSuccess { get; set; }
    public string? Error { get; set; }
    public AssignEmployeesToCitizenResponse? Assignment { get; set; }
}
