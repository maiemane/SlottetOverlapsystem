using Slottet.Domain.Entities;

namespace Slottet.Application.Interfaces;

public interface IPhoneRepository
{
    Task<IReadOnlyList<Phone>> GetPhonesAsync(CancellationToken cancellationToken = default);
    Task<Phone?> GetPhoneByIdAsync(int phoneId, CancellationToken cancellationToken = default);
    Task<Phone?> GetPhoneByNameOrNumberAsync(string nameOrNumber, CancellationToken cancellationToken = default);
    Task<bool> HasAllocationsAsync(int phoneId, CancellationToken cancellationToken = default);
    Task<Phone> AddPhoneAsync(Phone phone, CancellationToken cancellationToken = default);
    Task<Phone> UpdatePhoneAsync(Phone phone, CancellationToken cancellationToken = default);
    Task DeletePhoneAsync(Phone phone, CancellationToken cancellationToken = default);
}
