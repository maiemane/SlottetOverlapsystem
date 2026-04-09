namespace Slottet.Application.DTOs.Staffing;

public sealed class CitizenAssignmentBoardDto
{
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public List<StaffMemberDto> Employees { get; set; } = [];
    public List<CitizenAssignmentBoardCitizenDto> Citizens { get; set; } = [];
}
