using Slottet.Application.DTOs;
using Slottet.Application.Interfaces;
using Slottet.Domain.Enums;

namespace Slottet.Infrastructure.Services;

public class OverlapOverviewService : IOverlapOverviewService
{
    private readonly IOverlapOverviewRepository _overlapOverviewRepository;

    public OverlapOverviewService(IOverlapOverviewRepository overlapOverviewRepository)
    {
        _overlapOverviewRepository = overlapOverviewRepository;
    }

    public async Task<OverlapOverviewDto?> GetOverviewAsync(int departmentId, ShiftType shiftType)
    {
        if (!Enum.IsDefined(typeof(ShiftType), shiftType))
        {
            return null;
        }

        return await _overlapOverviewRepository.GetByDepartmentAndShiftAsync(departmentId, shiftType);
    }
}