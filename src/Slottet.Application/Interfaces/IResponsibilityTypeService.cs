using Slottet.Application.DTOs.Responsibilities;

namespace Slottet.Application.Interfaces;

public interface IResponsibilityTypeService
{
    Task<IReadOnlyList<ResponsibilityTypeDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ResponsibilityTypeDto?> GetByIdAsync(int responsibilityTypeId, CancellationToken cancellationToken = default);
    Task<CreateResponsibilityTypeResult> CreateAsync(CreateResponsibilityTypeRequest request, CancellationToken cancellationToken = default);
    Task<UpdateResponsibilityTypeResult> UpdateAsync(int responsibilityTypeId, UpdateResponsibilityTypeRequest request, CancellationToken cancellationToken = default);
    Task<DeleteResponsibilityTypeResult> DeleteAsync(int responsibilityTypeId, CancellationToken cancellationToken = default);
}
