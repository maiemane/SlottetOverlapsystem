using Slottet.Domain.Entities;

namespace Slottet.Application.Interfaces;

public interface IResponsibilityTypeRepository
{
    Task<IReadOnlyList<ResponsibilityType>> GetResponsibilityTypesAsync(CancellationToken cancellationToken = default);
    Task<ResponsibilityType?> GetResponsibilityTypeByIdAsync(int responsibilityTypeId, CancellationToken cancellationToken = default);
    Task<ResponsibilityType?> GetResponsibilityTypeByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<bool> HasAssignmentsAsync(int responsibilityTypeId, CancellationToken cancellationToken = default);
    Task<ResponsibilityType> AddResponsibilityTypeAsync(ResponsibilityType responsibilityType, CancellationToken cancellationToken = default);
    Task<ResponsibilityType> UpdateResponsibilityTypeAsync(ResponsibilityType responsibilityType, CancellationToken cancellationToken = default);
    Task DeleteResponsibilityTypeAsync(ResponsibilityType responsibilityType, CancellationToken cancellationToken = default);
}
