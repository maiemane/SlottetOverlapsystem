using Slottet.Application.DTOs.Phones;

namespace Slottet.Application.Interfaces;

public interface IPhoneService
{
    Task<IReadOnlyList<PhoneDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PhoneDto?> GetByIdAsync(int phoneId, CancellationToken cancellationToken = default);
    Task<CreatePhoneResult> CreateAsync(CreatePhoneRequest request, CancellationToken cancellationToken = default);
    Task<UpdatePhoneResult> UpdateAsync(int phoneId, UpdatePhoneRequest request, CancellationToken cancellationToken = default);
    Task<DeletePhoneResult> DeleteAsync(int phoneId, CancellationToken cancellationToken = default);
}
