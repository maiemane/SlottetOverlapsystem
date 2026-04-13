using Slottet.Domain.Entities;

namespace Slottet.Application.Interfaces;

public interface IPhoneAllocationRepository
{
    Task<Shift?> GetShiftByIdAsync(int shiftId, CancellationToken cancellationToken = default);
    Task<Employee?> GetEmployeeByIdAsync(int employeeId, CancellationToken cancellationToken = default);
    Task<Phone?> GetPhoneByIdAsync(int phoneId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<int>> GetShiftEmployeeIdsAsync(int shiftId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PhoneAllocation>> GetPhoneAllocationsAsync(int shiftId, CancellationToken cancellationToken = default);
    Task<PhoneAllocation?> GetPhoneAllocationByIdAsync(int phoneAllocationId, CancellationToken cancellationToken = default);
    Task<PhoneAllocation?> GetPhoneAllocationByShiftAndPhoneAsync(int shiftId, int phoneId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Employee>> GetEmployeesByIdsAsync(IReadOnlyCollection<int> employeeIds, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Phone>> GetPhonesByIdsAsync(IReadOnlyCollection<int> phoneIds, CancellationToken cancellationToken = default);
    Task<PhoneAllocation> AddPhoneAllocationAsync(PhoneAllocation phoneAllocation, CancellationToken cancellationToken = default);
    Task<PhoneAllocation> UpdatePhoneAllocationAsync(PhoneAllocation phoneAllocation, CancellationToken cancellationToken = default);
    Task DeletePhoneAllocationAsync(PhoneAllocation phoneAllocation, CancellationToken cancellationToken = default);
}
