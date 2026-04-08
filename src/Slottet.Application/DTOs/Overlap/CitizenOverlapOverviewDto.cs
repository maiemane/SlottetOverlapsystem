using Slottet.Domain.Enums;

namespace Slottet.Application.DTOs.Overlap;

public sealed class CitizenOverlapOverviewDto
{
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public int ShiftId { get; set; }
    public DateTime ShiftDate { get; set; }
    public ShiftType ShiftType { get; set; }
    public List<CitizenOverlapItemDto> Citizens { get; set; } = [];
}
