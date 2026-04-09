using Slottet.Domain.Enums;

namespace Slottet.Application.DTOs.Staffing;

public sealed class ShiftLookupDto
{
    public int ShiftId { get; set; }
    public int DepartmentId { get; set; }
    public DateTime Date { get; set; }
    public ShiftType ShiftType { get; set; }
}
