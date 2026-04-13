using Slottet.Application.DTOs.Responsibilities;

namespace Slottet.Application.Interfaces;

public interface IResponsibilityAssignmentService
{
    Task<IReadOnlyList<ResponsibilityAssignmentDto>?> GetByShiftAsync(int shiftId, CancellationToken cancellationToken = default);
    Task<ResponsibilityAssignmentDto?> GetByIdAsync(int shiftId, int responsibilityAssignmentId, CancellationToken cancellationToken = default);
    Task<CreateResponsibilityAssignmentResult> CreateAsync(int shiftId, CreateResponsibilityAssignmentRequest request, CancellationToken cancellationToken = default);
    Task<UpdateResponsibilityAssignmentResult> UpdateAsync(int shiftId, int responsibilityAssignmentId, UpdateResponsibilityAssignmentRequest request, CancellationToken cancellationToken = default);
    Task<DeleteResponsibilityAssignmentResult> DeleteAsync(int shiftId, int responsibilityAssignmentId, CancellationToken cancellationToken = default);
}
