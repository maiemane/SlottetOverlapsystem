namespace Slottet.Application.DTOs.Staffing;

public sealed class ShiftStaffingDto
{
    public int ShiftId { get; set; }
    public List<StaffMemberDto> Employees { get; set; } = [];
}
