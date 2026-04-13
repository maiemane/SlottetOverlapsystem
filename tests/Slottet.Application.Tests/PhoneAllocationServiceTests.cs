using Slottet.Application.DTOs.PhoneAllocations;
using Slottet.Application.Interfaces;
using Slottet.Application.Services.PhoneAllocations;
using Slottet.Domain.Entities;
using Slottet.Domain.Enums;

namespace Slottet.Application.Tests;

public class PhoneAllocationServiceTests
{
    [Fact]
    public async Task CreateAsync_CreatesPhoneAllocation_WhenEmployeeIsOnShift()
    {
        var repository = new FakePhoneAllocationRepository();
        var sut = new PhoneAllocationService(repository);

        var result = await sut.CreateAsync(10, new CreatePhoneAllocationRequest
        {
            PhoneId = 2,
            EmployeeId = 1
        });

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.PhoneAllocation);
        Assert.Equal(2, result.PhoneAllocation!.Id);
        Assert.Equal("Telefon 2", result.PhoneAllocation.PhoneNameOrNumber);
        Assert.Equal("Anna", result.PhoneAllocation.EmployeeName);
    }

    [Fact]
    public async Task CreateAsync_ReturnsError_WhenEmployeeIsNotOnShift()
    {
        var repository = new FakePhoneAllocationRepository();
        var sut = new PhoneAllocationService(repository);

        var result = await sut.CreateAsync(10, new CreatePhoneAllocationRequest
        {
            PhoneId = 2,
            EmployeeId = 2
        });

        Assert.False(result.IsSuccess);
        Assert.Equal("EmployeeNotOnShift", result.Error);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsError_WhenPhoneAlreadyAssignedToSomeoneElse()
    {
        var repository = new FakePhoneAllocationRepository();
        repository.StoredPhoneAllocations.Add(new PhoneAllocation
        {
            Id = 2,
            ShiftId = 10,
            PhoneId = 2,
            EmployeeId = 1
        });

        var sut = new PhoneAllocationService(repository);

        var result = await sut.UpdateAsync(10, 1, new UpdatePhoneAllocationRequest
        {
            PhoneId = 2,
            EmployeeId = 1
        });

        Assert.False(result.IsSuccess);
        Assert.Equal("PhoneAlreadyAssigned", result.Error);
    }

    private sealed class FakePhoneAllocationRepository : IPhoneAllocationRepository
    {
        public List<PhoneAllocation> StoredPhoneAllocations { get; } =
        [
            new PhoneAllocation
            {
                Id = 1,
                ShiftId = 10,
                PhoneId = 1,
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

        public List<Phone> Phones { get; } =
        [
            new Phone { Id = 1, NameOrNumber = "Telefon 1", IsActive = true },
            new Phone { Id = 2, NameOrNumber = "Telefon 2", IsActive = true }
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

        public Task<Phone?> GetPhoneByIdAsync(int phoneId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Phones.FirstOrDefault(phone => phone.Id == phoneId));
        }

        public Task<IReadOnlyList<int>> GetShiftEmployeeIdsAsync(int shiftId, CancellationToken cancellationToken = default)
        {
            IReadOnlyList<int> shiftEmployeeIds = shiftId == 10 ? [1] : [];
            return Task.FromResult(shiftEmployeeIds);
        }

        public Task<IReadOnlyList<PhoneAllocation>> GetPhoneAllocationsAsync(int shiftId, CancellationToken cancellationToken = default)
        {
            IReadOnlyList<PhoneAllocation> result = StoredPhoneAllocations.Where(allocation => allocation.ShiftId == shiftId).ToList();
            return Task.FromResult(result);
        }

        public Task<PhoneAllocation?> GetPhoneAllocationByIdAsync(int phoneAllocationId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(StoredPhoneAllocations.FirstOrDefault(allocation => allocation.Id == phoneAllocationId));
        }

        public Task<PhoneAllocation?> GetPhoneAllocationByShiftAndPhoneAsync(int shiftId, int phoneId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(StoredPhoneAllocations.FirstOrDefault(allocation => allocation.ShiftId == shiftId && allocation.PhoneId == phoneId));
        }

        public Task<IReadOnlyList<Employee>> GetEmployeesByIdsAsync(IReadOnlyCollection<int> employeeIds, CancellationToken cancellationToken = default)
        {
            IReadOnlyList<Employee> result = Employees.Where(employee => employeeIds.Contains(employee.Id)).ToList();
            return Task.FromResult(result);
        }

        public Task<IReadOnlyList<Phone>> GetPhonesByIdsAsync(IReadOnlyCollection<int> phoneIds, CancellationToken cancellationToken = default)
        {
            IReadOnlyList<Phone> result = Phones.Where(phone => phoneIds.Contains(phone.Id)).ToList();
            return Task.FromResult(result);
        }

        public Task<PhoneAllocation> AddPhoneAllocationAsync(PhoneAllocation phoneAllocation, CancellationToken cancellationToken = default)
        {
            phoneAllocation.Id = StoredPhoneAllocations.Max(allocation => allocation.Id) + 1;
            StoredPhoneAllocations.Add(phoneAllocation);
            return Task.FromResult(phoneAllocation);
        }

        public Task<PhoneAllocation> UpdatePhoneAllocationAsync(PhoneAllocation phoneAllocation, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(phoneAllocation);
        }

        public Task DeletePhoneAllocationAsync(PhoneAllocation phoneAllocation, CancellationToken cancellationToken = default)
        {
            StoredPhoneAllocations.Remove(phoneAllocation);
            return Task.CompletedTask;
        }
    }
}
