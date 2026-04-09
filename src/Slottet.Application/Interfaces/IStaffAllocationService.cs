using Slottet.Application.DTOs.Staffing;

namespace Slottet.Application.Interfaces;

public interface IStaffAllocationService
{
    Task<CitizenAssignmentBoardDto?> GetCitizenAssignmentBoardAsync(int departmentId, DateTime date, CancellationToken cancellationToken = default);
    Task<ShiftStaffingDto?> GetShiftEmployeesAsync(int shiftId, CancellationToken cancellationToken = default);
    Task<CitizenStaffingDto?> GetCitizenEmployeesAsync(int shiftId, int citizenId, CancellationToken cancellationToken = default);
    Task<AssignEmployeesToShiftResult> AssignEmployeesToShiftAsync(int shiftId, AssignEmployeesToShiftRequest request, CancellationToken cancellationToken = default);
    Task<AssignEmployeesToCitizenResult> AssignEmployeesToCitizenAsync(int shiftId, int citizenId, AssignEmployeesToCitizenRequest request, CancellationToken cancellationToken = default);
}
