using Slottet.Application.DTOs;
using Slottet.Domain.Enums;

namespace Slottet.Application.Interfaces;

public interface IOverlapOverviewRepository
{
    Task<OverlapOverviewDto?> GetByDepartmentAndShiftAsync(int departmentId, ShiftType shiftType);
}