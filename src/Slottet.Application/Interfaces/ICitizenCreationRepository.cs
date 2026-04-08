using Slottet.Domain.Entities;

namespace Slottet.Application.Interfaces;

public interface ICitizenCreationRepository
{
    Task<Department?> GetDepartmentByIdAsync(int departmentId, CancellationToken cancellationToken = default);
    Task<Citizen> AddCitizenAsync(Citizen citizen, CancellationToken cancellationToken = default);
}
