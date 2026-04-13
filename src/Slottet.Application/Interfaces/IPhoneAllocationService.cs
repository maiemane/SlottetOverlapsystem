using Slottet.Application.DTOs.PhoneAllocations;

namespace Slottet.Application.Interfaces;

public interface IPhoneAllocationService
{
    Task<IReadOnlyList<PhoneAllocationDto>?> GetByShiftAsync(int shiftId, CancellationToken cancellationToken = default);
    Task<PhoneAllocationDto?> GetByIdAsync(int shiftId, int phoneAllocationId, CancellationToken cancellationToken = default);
    Task<CreatePhoneAllocationResult> CreateAsync(int shiftId, CreatePhoneAllocationRequest request, CancellationToken cancellationToken = default);
    Task<UpdatePhoneAllocationResult> UpdateAsync(int shiftId, int phoneAllocationId, UpdatePhoneAllocationRequest request, CancellationToken cancellationToken = default);
    Task<DeletePhoneAllocationResult> DeleteAsync(int shiftId, int phoneAllocationId, CancellationToken cancellationToken = default);
}
