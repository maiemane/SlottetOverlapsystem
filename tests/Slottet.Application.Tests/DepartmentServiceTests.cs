using Slottet.Application.DTOs.Departments;
using Slottet.Application.Interfaces;
using Slottet.Application.Services.Departments;
using Slottet.Domain.Entities;

namespace Slottet.Application.Tests;

public class DepartmentServiceTests
{
    [Fact]
    public async Task GetAllAsync_Returns_departments_with_messages()
    {
        var repository = new FakeDepartmentRepository();
        var sut = new DepartmentService(repository);

        var result = await sut.GetAllAsync(CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Equal("Skoven", result[0].Name);
        Assert.Equal("Husk teammøde kl. 14", result[0].Message);
    }

    [Fact]
    public async Task UpdateMessageAsync_Updates_department_message()
    {
        var repository = new FakeDepartmentRepository();
        var sut = new DepartmentService(repository);

        var result = await sut.UpdateMessageAsync(1, new UpdateDepartmentMessageRequest
        {
            Message = "Ny fælles besked"
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Department);
        Assert.Equal("Ny fælles besked", result.Department!.Message);
    }

    [Fact]
    public async Task UpdateMessageAsync_Returns_failure_when_message_is_too_long()
    {
        var repository = new FakeDepartmentRepository();
        var sut = new DepartmentService(repository);

        var result = await sut.UpdateMessageAsync(1, new UpdateDepartmentMessageRequest
        {
            Message = new string('a', 2001)
        }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("MessageTooLong", result.Error);
    }

    private sealed class FakeDepartmentRepository : IDepartmentRepository
    {
        private readonly List<Department> _departments =
        [
            new() { Id = 1, Name = "Slottet", Message = "Velkommen til dagvagt" },
            new() { Id = 2, Name = "Skoven", Message = "Husk teammøde kl. 14" }
        ];

        public Task<IReadOnlyList<Department>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<Department>>(_departments
                .Select(department => new Department
                {
                    Id = department.Id,
                    Name = department.Name,
                    Message = department.Message
                })
                .ToList());
        }

        public Task<Department?> GetByIdAsync(int departmentId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_departments.FirstOrDefault(department => department.Id == departmentId));
        }

        public Task<Department> UpdateAsync(Department department, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(department);
        }
    }
}
