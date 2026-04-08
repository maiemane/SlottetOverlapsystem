using Slottet.Application.DTOs.Citizens;

namespace Slottet.Application.Interfaces;

public interface ICreateCitizenService
{
    Task<CreateCitizenResult> CreateAsync(CreateCitizenRequest request, CancellationToken cancellationToken = default);
}
