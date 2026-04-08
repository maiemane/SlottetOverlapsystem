using Slottet.Application.DTOs.Staffing;

namespace Slottet.Application.Interfaces;

public interface IStaffAllocationService
{
    Task<AssignEmployeesToShiftResult> AssignEmployeesToShiftAsync(int shiftId, AssignEmployeesToShiftRequest request, CancellationToken cancellationToken = default);
    Task<AssignEmployeesToCitizenResult> AssignEmployeesToCitizenAsync(int shiftId, int citizenId, AssignEmployeesToCitizenRequest request, CancellationToken cancellationToken = default);
}
