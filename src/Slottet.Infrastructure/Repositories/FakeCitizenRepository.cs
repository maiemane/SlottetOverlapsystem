using Slottet.Application.Interfaces;
using Slottet.Domain.Entities;

namespace Slottet.Infrastructure.Repositories;

public class FakeCitizenRepository : ICitizenRepository
{
    private readonly List<Citizen> _citizens = new()
    {
        new Citizen { Id = 1, Name = "Anna Jensen", DepartmentId = 1, IsActive = true },
        new Citizen { Id = 2, Name = "Peter Hansen", DepartmentId = 1, IsActive = true },
        new Citizen { Id = 3, Name = "Lise Madsen", DepartmentId = 2, IsActive = true }
    };

    public Task<IEnumerable<Citizen>> GetAllAsync()
    {
        return Task.FromResult(_citizens.AsEnumerable());
    }

    public Task<Citizen?> GetByIdAsync(int id)
    {
        var citizen = _citizens.FirstOrDefault(c => c.Id == id);
        return Task.FromResult(citizen);
    }
}