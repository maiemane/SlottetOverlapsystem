using Slottet.Application.DTOs.Staffing;
using Slottet.Application.Interfaces;
using Slottet.Application.Services.Staffing;
using Slottet.Domain.Entities;
using Slottet.Domain.Enums;

namespace Slottet.Application.Tests;

public class StaffAllocationServiceTests
{
    [Fact]
    public async Task GetCitizenAssignmentBoardAsync_Returns_board_with_citizens_shifts_and_assignments()
    {
        var repository = new FakeStaffAllocationRepository();
        repository.StoredCitizenAssignmentEmployeeIds = [2, 1];
        var sut = new StaffAllocationService(repository);

        var result = await sut.GetCitizenAssignmentBoardAsync(1, new DateTime(2026, 4, 8), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(1, result!.DepartmentId);
        Assert.Equal(3, result.Employees.Count);
        var citizen = Assert.Single(result.Citizens);
        Assert.Equal("Anna Jensen", citizen.Name);
        Assert.Equal(3, citizen.Shifts.Count);
        Assert.Equal([1, 2], citizen.Shifts[0].AssignedEmployeeIds);
    }

    [Fact]
    public async Task GetShiftEmployeesAsync_Returns_staffing_for_shift()
    {
        var repository = new FakeStaffAllocationRepository();
        var sut = new StaffAllocationService(repository);

        var result = await sut.GetShiftEmployeesAsync(10, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(10, result!.ShiftId);
        Assert.Equal(3, result.Employees.Count);
        Assert.Equal("Employee 1", result.Employees[0].Name);
    }

    [Fact]
    public async Task GetCitizenEmployeesAsync_Returns_staffing_for_citizen_on_shift()
    {
        var repository = new FakeStaffAllocationRepository();
        repository.StoredCitizenAssignmentEmployeeIds = [2, 1];
        var sut = new StaffAllocationService(repository);

        var result = await sut.GetCitizenEmployeesAsync(10, 100, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(10, result!.ShiftId);
        Assert.Equal(100, result.CitizenId);
        Assert.Equal([1, 2], result.Employees.Select(employee => employee.EmployeeId).ToList());
    }

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
        public IReadOnlyList<Shift> StoredShifts { get; private set; } =
        [
            new Shift { Id = 10, DepartmentId = 1, Date = new DateTime(2026, 4, 8), Type = ShiftType.Dagvagt },
            new Shift { Id = 11, DepartmentId = 1, Date = new DateTime(2026, 4, 8), Type = ShiftType.Aftenvagt },
            new Shift { Id = 12, DepartmentId = 1, Date = new DateTime(2026, 4, 8), Type = ShiftType.Nattevagt }
        ];
        public List<int> StoredShiftEmployeeIds { get; private set; } = [1, 2, 3];
        public List<int> StoredCitizenAssignmentEmployeeIds { get; set; } = [];

        public Task<Department?> GetDepartmentByIdAsync(int departmentId, CancellationToken cancellationToken = default)
        {
            Department? department = departmentId == 1
                ? new Department { Id = 1, Name = "Slottet" }
                : null;

            return Task.FromResult(department);
        }

        public Task<Shift?> GetShiftByIdAsync(int shiftId, CancellationToken cancellationToken = default)
        {
            Shift? shift = StoredShifts.FirstOrDefault(candidate => candidate.Id == shiftId);

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

        public Task<IReadOnlyList<Citizen>> GetActiveCitizensByDepartmentAsync(int departmentId, CancellationToken cancellationToken = default)
        {
            IReadOnlyList<Citizen> citizens = departmentId == 1
                ?
                [
                    new Citizen
                    {
                        Id = 100,
                        Name = "Anna Jensen",
                        ApartmentNumber = "12A",
                        DepartmentId = 1,
                        TrafficLight = TrafficLight.Grøn,
                        IsActive = true
                    }
                ]
                : [];

            return Task.FromResult(citizens);
        }

        public Task<IReadOnlyList<Employee>> GetActiveEmployeesAsync(CancellationToken cancellationToken = default)
        {
            return GetActiveEmployeesByIdsAsync([1, 2, 3], cancellationToken);
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

        public Task<IReadOnlyList<Shift>> GetShiftsByDepartmentAndDateAsync(int departmentId, DateTime date, CancellationToken cancellationToken = default)
        {
            IReadOnlyList<Shift> shifts = StoredShifts
                .Where(shift => shift.DepartmentId == departmentId && shift.Date.Date == date.Date)
                .ToList();

            return Task.FromResult(shifts);
        }

        public Task<IReadOnlyList<Shift>> EnsureShiftsForDepartmentAndDateAsync(int departmentId, DateTime date, CancellationToken cancellationToken = default)
        {
            return GetShiftsByDepartmentAndDateAsync(departmentId, date, cancellationToken);
        }

        public Task<IReadOnlyList<CitizenAssignment>> GetCitizenAssignmentsByShiftIdsAsync(IReadOnlyCollection<int> shiftIds, CancellationToken cancellationToken = default)
        {
            IReadOnlyList<CitizenAssignment> assignments = shiftIds.Contains(10)
                ?
                [
                    new CitizenAssignment { CitizenId = 100, ShiftId = 10, EmployeeId = 2 },
                    new CitizenAssignment { CitizenId = 100, ShiftId = 10, EmployeeId = 1 }
                ]
                : [];

            return Task.FromResult(assignments);
        }

        public Task<IReadOnlyList<int>> GetShiftEmployeeIdsAsync(int shiftId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<int>>(StoredShiftEmployeeIds);
        }

        public Task<IReadOnlyList<Employee>> GetEmployeesByShiftAsync(int shiftId, CancellationToken cancellationToken = default)
        {
            return GetActiveEmployeesByIdsAsync(StoredShiftEmployeeIds, cancellationToken);
        }

        public Task<IReadOnlyList<Employee>> GetEmployeesByCitizenAssignmentAsync(int shiftId, int citizenId, CancellationToken cancellationToken = default)
        {
            return GetActiveEmployeesByIdsAsync(StoredCitizenAssignmentEmployeeIds, cancellationToken);
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
