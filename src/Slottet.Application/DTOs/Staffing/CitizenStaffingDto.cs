namespace Slottet.Application.DTOs.Staffing;

public sealed class CitizenStaffingDto
{
    public int ShiftId { get; set; }
    public int CitizenId { get; set; }
    public List<StaffMemberDto> Employees { get; set; } = [];
}
