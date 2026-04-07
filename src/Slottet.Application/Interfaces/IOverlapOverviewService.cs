using Slottet.Application.DTOs;
using Slottet.Domain.Enums;

namespace Slottet.Application.Interfaces;

public interface IOverlapOverviewService
{
    Task<OverlapOverviewDto?> GetOverviewAsync(int departmentId, ShiftType shiftType);
}