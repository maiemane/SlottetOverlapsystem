using Slottet.Application.DTOs.Citizens;

namespace Slottet.Application.Interfaces;

public interface ICreateCitizenService
{
    Task<IReadOnlyList<CitizenDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<CreateCitizenResult> CreateAsync(CreateCitizenRequest request, CancellationToken cancellationToken = default);
    Task<UpdateCitizenResult> UpdateAsync(int citizenId, UpdateCitizenRequest request, CancellationToken cancellationToken = default);
    Task<DeleteCitizenResult> DeleteAsync(int citizenId, CancellationToken cancellationToken = default);
}
