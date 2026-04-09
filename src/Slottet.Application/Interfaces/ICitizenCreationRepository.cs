using Slottet.Domain.Entities;

namespace Slottet.Application.Interfaces;

public interface ICitizenCreationRepository
{
    Task<IReadOnlyList<Citizen>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Citizen?> GetByIdAsync(int citizenId, CancellationToken cancellationToken = default);
    Task<Department?> GetDepartmentByIdAsync(int departmentId, CancellationToken cancellationToken = default);
    Task<Citizen> AddCitizenAsync(Citizen citizen, CancellationToken cancellationToken = default);
    Task<Citizen> UpdateCitizenAsync(Citizen citizen, CancellationToken cancellationToken = default);
    Task<bool> DeleteCitizenAsync(Citizen citizen, CancellationToken cancellationToken = default);
}
