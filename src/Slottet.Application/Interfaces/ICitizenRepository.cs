using Slottet.Domain.Entities;

namespace Slottet.Application.Interfaces;

public interface ICitizenRepository
{
    Task<IEnumerable<Citizen>> GetAllAsync();
    Task<Citizen?> GetByIdAsync(int id);
}