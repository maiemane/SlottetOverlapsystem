using System.Collections.Generic;
using Slottet.Domain.Enums;

namespace Slottet.Application.DTOs;

public class OverlapOverviewDto
{
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public ShiftType ShiftType { get; set; }
    public List<CitizenOverviewItemDto> Citizens { get; set; } = new();
    public List<SharedTaskDto> SharedTasks { get; set; } = new();
}