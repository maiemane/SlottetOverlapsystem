using Slottet.Application.DTOs.Staffing;
using Slottet.Application.Interfaces;
using Slottet.Application.Services.Staffing;
using Slottet.Domain.Entities;
using Slottet.Domain.Enums;

namespace Slottet.Application.Tests;

public class StaffAllocationServiceTests
{
    [Fact]
    public async Task AssignEmployeesToShiftAsync_Replaces_shift_employees_when_request_is_valid()
    {
        var repository = new FakeStaffAllocationRepository();
        var sut = new StaffAllocationService(repository);

        var result = await sut.AssignEmployeesToShiftAsync(10, new AssignEmployeesToShiftRequest
        {
            EmployeeIds = [2, 1, 2]
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Assignment);
        Assert.Equal([1, 2], result.Assignment!.EmployeeIds);
        Assert.Equal([1, 2], repository.StoredShiftEmployeeIds);
    }

    [Fact]
    public async Task AssignEmployeesToCitizenAsync_Assigns_up_to_two_shift_employees_to_citizen()
    {
        var repository = new FakeStaffAllocationRepository();
        var sut = new StaffAllocationService(repository);

        var result = await sut.AssignEmployeesToCitizenAsync(10, 100, new AssignEmployeesToCitizenRequest
        {
            EmployeeIds = [2, 1]
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Assignment);
        Assert.Equal([1, 2], result.Assignment!.EmployeeIds);
        Assert.Equal([1, 2], repository.StoredCitizenAssignmentEmployeeIds);
    }

    [Fact]
    public async Task AssignEmployeesToCitizenAsync_Returns_failure_when_more_than_two_employees_are_assigned()
    {
        var repository = new FakeStaffAllocationRepository();
        var sut = new StaffAllocationService(repository);

        var result = await sut.AssignEmployeesToCitizenAsync(10, 100, new AssignEmployeesToCitizenRequest
        {
            EmployeeIds = [1, 2, 3]
        }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("TooManyEmployees", result.Error);
    }

    [Fact]
    public async Task AssignEmployeesToCitizenAsync_Returns_failure_when_employee_is_not_on_shift()
    {
        var repository = new FakeStaffAllocationRepository();
        var sut = new StaffAllocationService(repository);

        var result = await sut.AssignEmployeesToCitizenAsync(10, 100, new AssignEmployeesToCitizenRequest
        {
            EmployeeIds = [1, 4]
        }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("EmployeeNotOnShift", result.Error);
    }

    private sealed class FakeStaffAllocationRepository : IStaffAllocationRepository
    {
        public List<int> StoredShiftEmployeeIds { get; private set; } = [1, 2, 3];
        public List<int> StoredCitizenAssignmentEmployeeIds { get; private set; } = [];

        public Task<Shift?> GetShiftByIdAsync(int shiftId, CancellationToken cancellationToken = default)
        {
            Shift? shift = shiftId == 10
                ? new Shift
                {
                    Id = 10,
                    DepartmentId = 1,
                    Date = new DateTime(2026, 4, 8),
                    Type = ShiftType.Dagvagt
                }
                : null;

            return Task.FromResult(shift);
        }

        public Task<Citizen?> GetCitizenByIdAsync(int citizenId, CancellationToken cancellationToken = default)
        {
            Citizen? citizen = citizenId == 100
                ? new Citizen
                {
                    Id = 100,
                    Name = "Anna Jensen",
                    ApartmentNumber = "12A",
                    DepartmentId = 1,
                    TrafficLight = TrafficLight.Grøn,
                    IsActive = true
                }
                : null;

            return Task.FromResult(citizen);
        }

        public Task<IReadOnlyList<Employee>> GetActiveEmployeesByIdsAsync(IReadOnlyCollection<int> employeeIds, CancellationToken cancellationToken = default)
        {
            IReadOnlyList<Employee> employees = employeeIds
                .Where(employeeId => employeeId is 1 or 2 or 3)
                .Select(employeeId => new Employee
                {
                    Id = employeeId,
                    Name = $"Employee {employeeId}",
                    Email = $"employee{employeeId}@slottet.dk",
                    Role = "Medarbejder",
                    PasswordHash = "hash",
                    IsActive = true
                })
                .ToList();

            return Task.FromResult(employees);
        }

        public Task<IReadOnlyList<int>> GetShiftEmployeeIdsAsync(int shiftId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<int>>(StoredShiftEmployeeIds);
        }

        public Task ReplaceShiftEmployeesAsync(int shiftId, IReadOnlyCollection<int> employeeIds, CancellationToken cancellationToken = default)
        {
            StoredShiftEmployeeIds = employeeIds.ToList();
            return Task.CompletedTask;
        }

        public Task ReplaceCitizenAssignmentsAsync(int shiftId, int citizenId, IReadOnlyCollection<int> employeeIds, CancellationToken cancellationToken = default)
        {
            StoredCitizenAssignmentEmployeeIds = employeeIds.ToList();
            return Task.CompletedTask;
        }
    }
}
