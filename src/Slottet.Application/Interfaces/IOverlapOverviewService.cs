using Slottet.Application.DTOs.Overlap;

namespace Slottet.Application.Interfaces;

public interface IOverlapOverviewService
{
    Task<CitizenOverlapOverviewDto?> GetCitizenOverviewAsync(int departmentId, int shiftId, CancellationToken cancellationToken = default);
}
