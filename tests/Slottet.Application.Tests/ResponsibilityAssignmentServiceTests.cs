using Slottet.Application.DTOs.Responsibilities;
using Slottet.Application.Interfaces;
using Slottet.Application.Services.Responsibilities;
using Slottet.Domain.Entities;
using Slottet.Domain.Enums;

namespace Slottet.Application.Tests;

public class ResponsibilityAssignmentServiceTests
{
    [Fact]
    public async Task CreateAsync_CreatesResponsibilityAssignment_WhenEmployeeIsOnShift()
    {
        var repository = new FakeResponsibilityAssignmentRepository();
        var sut = new ResponsibilityAssignmentService(repository);

        var result = await sut.CreateAsync(10, new CreateResponsibilityAssignmentRequest
        {
            ResponsibilityTypeId = 2,
            EmployeeId = 1
        });

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.ResponsibilityAssignment);
        Assert.Equal(2, result.ResponsibilityAssignment!.Id);
        Assert.Equal("Hygiejne", result.ResponsibilityAssignment.ResponsibilityTypeName);
        Assert.Equal("Anna", result.ResponsibilityAssignment.EmployeeName);
    }

    [Fact]
    public async Task CreateAsync_ReturnsError_WhenEmployeeIsNotOnShift()
    {
        var repository = new FakeResponsibilityAssignmentRepository();
        var sut = new ResponsibilityAssignmentService(repository);

        var result = await sut.CreateAsync(10, new CreateResponsibilityAssignmentRequest
        {
            ResponsibilityTypeId = 2,
            EmployeeId = 2
        });

        Assert.False(result.IsSuccess);
        Assert.Equal("EmployeeNotOnShift", result.Error);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsError_WhenResponsibilityAlreadyAssignedToSomeoneElse()
    {
        var repository = new FakeResponsibilityAssignmentRepository();
        repository.StoredResponsibilityAssignments.Add(new ResponsibilityAssignment
        {
            Id = 2,
            ShiftId = 10,
            ResponsibilityTypeId = 2,
            EmployeeId = 1
        });

        var sut = new ResponsibilityAssignmentService(repository);

        var result = await sut.UpdateAsync(10, 1, new UpdateResponsibilityAssignmentRequest
        {
            ResponsibilityTypeId = 2,
            EmployeeId = 1
        });

        Assert.False(result.IsSuccess);
        Assert.Equal("ResponsibilityAlreadyAssigned", result.Error);
    }

    private sealed class FakeResponsibilityAssignmentRepository : IResponsibilityAssignmentRepository
    {
        public List<ResponsibilityAssignment> StoredResponsibilityAssignments { get; } =
        [
            new ResponsibilityAssignment
            {
                Id = 1,
                ShiftId = 10,
                ResponsibilityTypeId = 1,
                EmployeeId = 1
            }
        ];

        public List<Employee> Employees { get; } =
        [
            new Employee
            {
                Id = 1,
                Name = "Anna",
                Email = "anna@slottet.dk",
                PasswordHash = "hash",
                Role = "Medarbejder",
                IsActive = true
            },
            new Employee
            {
                Id = 2,
                Name = "Peter",
                Email = "peter@slottet.dk",
                PasswordHash = "hash",
                Role = "Medarbejder",
                IsActive = true
            }
        ];

        public List<ResponsibilityType> ResponsibilityTypes { get; } =
        [
            new ResponsibilityType { Id = 1, Name = "Medicinansvarlig" },
            new ResponsibilityType { Id = 2, Name = "Hygiejne" }
        ];

        public Task<Shift?> GetShiftByIdAsync(int shiftId, CancellationToken cancellationToken = default)
        {
            Shift? shift = shiftId == 10
                ? new Shift { Id = 10, DepartmentId = 1, Date = DateTime.Today, Type = ShiftType.Dagvagt }
                : null;

            return Task.FromResult(shift);
        }

        public Task<Employee?> GetEmployeeByIdAsync(int employeeId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Employees.FirstOrDefault(employee => employee.Id == employeeId));
        }

        public Task<ResponsibilityType?> GetResponsibilityTypeByIdAsync(int responsibilityTypeId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(ResponsibilityTypes.FirstOrDefault(type => type.Id == responsibilityTypeId));
        }

        public Task<IReadOnlyList<int>> GetShiftEmployeeIdsAsync(int shiftId, CancellationToken cancellationToken = default)
        {
            IReadOnlyList<int> shiftEmployeeIds = shiftId == 10 ? [1] : [];
            return Task.FromResult(shiftEmployeeIds);
        }

        public Task<IReadOnlyList<ResponsibilityAssignment>> GetResponsibilityAssignmentsAsync(int shiftId, CancellationToken cancellationToken = default)
        {
            IReadOnlyList<ResponsibilityAssignment> result = StoredResponsibilityAssignments.Where(assignment => assignment.ShiftId == shiftId).ToList();
            return Task.FromResult(result);
        }

        public Task<ResponsibilityAssignment?> GetResponsibilityAssignmentByIdAsync(int responsibilityAssignmentId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(StoredResponsibilityAssignments.FirstOrDefault(assignment => assignment.Id == responsibilityAssignmentId));
        }

        public Task<ResponsibilityAssignment?> GetResponsibilityAssignmentByShiftAndTypeAsync(int shiftId, int responsibilityTypeId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(StoredResponsibilityAssignments.FirstOrDefault(assignment => assignment.ShiftId == shiftId && assignment.ResponsibilityTypeId == responsibilityTypeId));
        }

        public Task<IReadOnlyList<Employee>> GetEmployeesByIdsAsync(IReadOnlyCollection<int> employeeIds, CancellationToken cancellationToken = default)
        {
            IReadOnlyList<Employee> result = Employees.Where(employee => employeeIds.Contains(employee.Id)).ToList();
            return Task.FromResult(result);
        }

        public Task<IReadOnlyList<ResponsibilityType>> GetResponsibilityTypesByIdsAsync(IReadOnlyCollection<int> responsibilityTypeIds, CancellationToken cancellationToken = default)
        {
            IReadOnlyList<ResponsibilityType> result = ResponsibilityTypes.Where(type => responsibilityTypeIds.Contains(type.Id)).ToList();
            return Task.FromResult(result);
        }

        public Task<ResponsibilityAssignment> AddResponsibilityAssignmentAsync(ResponsibilityAssignment responsibilityAssignment, CancellationToken cancellationToken = default)
        {
            responsibilityAssignment.Id = StoredResponsibilityAssignments.Max(assignment => assignment.Id) + 1;
            StoredResponsibilityAssignments.Add(responsibilityAssignment);
            return Task.FromResult(responsibilityAssignment);
        }

        public Task<ResponsibilityAssignment> UpdateResponsibilityAssignmentAsync(ResponsibilityAssignment responsibilityAssignment, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(responsibilityAssignment);
        }

        public Task DeleteResponsibilityAssignmentAsync(ResponsibilityAssignment responsibilityAssignment, CancellationToken cancellationToken = default)
        {
            StoredResponsibilityAssignments.Remove(responsibilityAssignment);
            return Task.CompletedTask;
        }
    }
}
