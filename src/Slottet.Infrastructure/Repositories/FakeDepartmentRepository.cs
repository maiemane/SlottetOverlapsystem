using Slottet.Application.Interfaces;
using Slottet.Domain.Entities;

namespace Slottet.Infrastructure.Repositories;

public class FakeDepartmentRepository : IDepartmentRepository
{
    private readonly List<Department> _departments = new()
    {
        new Department { Id = 1, Name = "Slottet" },
        new Department { Id = 2, Name = "Skoven" }
    };

    public Task<IEnumerable<Department>> GetAllAsync()
    {
        return Task.FromResult(_departments.AsEnumerable());
    }

    public Task<Department?> GetByIdAsync(int id)
    {
        var department = _departments.FirstOrDefault(d => d.Id == id);
        return Task.FromResult(department);
    }
}