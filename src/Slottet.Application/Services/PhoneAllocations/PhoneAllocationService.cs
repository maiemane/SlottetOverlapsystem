using Slottet.Application.DTOs.PhoneAllocations;
using Slottet.Application.Interfaces;
using Slottet.Domain.Entities;

namespace Slottet.Application.Services.PhoneAllocations;

public sealed class PhoneAllocationService : IPhoneAllocationService
{
    private readonly IPhoneAllocationRepository _phoneAllocationRepository;

    public PhoneAllocationService(IPhoneAllocationRepository phoneAllocationRepository)
    {
        _phoneAllocationRepository = phoneAllocationRepository;
    }

    public async Task<IReadOnlyList<PhoneAllocationDto>?> GetByShiftAsync(int shiftId, CancellationToken cancellationToken = default)
    {
        if (shiftId <= 0)
        {
            return null;
        }

        var shift = await _phoneAllocationRepository.GetShiftByIdAsync(shiftId, cancellationToken);

        if (shift is null)
        {
            return null;
        }

        var phoneAllocations = await _phoneAllocationRepository.GetPhoneAllocationsAsync(shiftId, cancellationToken);
        return await MapDtosAsync(phoneAllocations, cancellationToken);
    }

    public async Task<PhoneAllocationDto?> GetByIdAsync(int shiftId, int phoneAllocationId, CancellationToken cancellationToken = default)
    {
        if (shiftId <= 0 || phoneAllocationId <= 0)
        {
            return null;
        }

        var shift = await _phoneAllocationRepository.GetShiftByIdAsync(shiftId, cancellationToken);

        if (shift is null)
        {
            return null;
        }

        var phoneAllocation = await _phoneAllocationRepository.GetPhoneAllocationByIdAsync(phoneAllocationId, cancellationToken);

        if (phoneAllocation is null || phoneAllocation.ShiftId != shiftId)
        {
            return null;
        }

        return await MapDtoAsync(phoneAllocation, cancellationToken);
    }

    public async Task<CreatePhoneAllocationResult> CreateAsync(int shiftId, CreatePhoneAllocationRequest request, CancellationToken cancellationToken = default)
    {
        var validation = await ValidateAsync(shiftId, request.PhoneId, request.EmployeeId, cancellationToken);

        if (!validation.IsValid)
        {
            return CreateFailure(validation.Error!);
        }

        var existingAllocation = await _phoneAllocationRepository.GetPhoneAllocationByShiftAndPhoneAsync(shiftId, request.PhoneId, cancellationToken);

        if (existingAllocation is not null)
        {
            return CreateFailure("PhoneAlreadyAssigned");
        }

        var phoneAllocation = await _phoneAllocationRepository.AddPhoneAllocationAsync(new PhoneAllocation
        {
            ShiftId = shiftId,
            PhoneId = request.PhoneId,
            EmployeeId = request.EmployeeId
        }, cancellationToken);

        return new CreatePhoneAllocationResult
        {
            IsSuccess = true,
            PhoneAllocation = await MapDtoAsync(phoneAllocation, cancellationToken)
        };
    }

    public async Task<UpdatePhoneAllocationResult> UpdateAsync(int shiftId, int phoneAllocationId, UpdatePhoneAllocationRequest request, CancellationToken cancellationToken = default)
    {
        if (phoneAllocationId <= 0)
        {
            return UpdateFailure("PhoneAllocationNotFound");
        }

        var validation = await ValidateAsync(shiftId, request.PhoneId, request.EmployeeId, cancellationToken);

        if (!validation.IsValid)
        {
            return UpdateFailure(validation.Error!);
        }

        var phoneAllocation = await _phoneAllocationRepository.GetPhoneAllocationByIdAsync(phoneAllocationId, cancellationToken);

        if (phoneAllocation is null || phoneAllocation.ShiftId != shiftId)
        {
            return UpdateFailure("PhoneAllocationNotFound");
        }

        var existingAllocation = await _phoneAllocationRepository.GetPhoneAllocationByShiftAndPhoneAsync(shiftId, request.PhoneId, cancellationToken);

        if (existingAllocation is not null && existingAllocation.Id != phoneAllocationId)
        {
            return UpdateFailure("PhoneAlreadyAssigned");
        }

        phoneAllocation.PhoneId = request.PhoneId;
        phoneAllocation.EmployeeId = request.EmployeeId;

        var updatedAllocation = await _phoneAllocationRepository.UpdatePhoneAllocationAsync(phoneAllocation, cancellationToken);

        return new UpdatePhoneAllocationResult
        {
            IsSuccess = true,
            PhoneAllocation = await MapDtoAsync(updatedAllocation, cancellationToken)
        };
    }

    public async Task<DeletePhoneAllocationResult> DeleteAsync(int shiftId, int phoneAllocationId, CancellationToken cancellationToken = default)
    {
        if (shiftId <= 0)
        {
            return Failure("ShiftNotFound");
        }

        if (phoneAllocationId <= 0)
        {
            return Failure("PhoneAllocationNotFound");
        }

        var shift = await _phoneAllocationRepository.GetShiftByIdAsync(shiftId, cancellationToken);

        if (shift is null)
        {
            return Failure("ShiftNotFound");
        }

        var phoneAllocation = await _phoneAllocationRepository.GetPhoneAllocationByIdAsync(phoneAllocationId, cancellationToken);

        if (phoneAllocation is null || phoneAllocation.ShiftId != shiftId)
        {
            return Failure("PhoneAllocationNotFound");
        }

        await _phoneAllocationRepository.DeletePhoneAllocationAsync(phoneAllocation, cancellationToken);

        return new DeletePhoneAllocationResult
        {
            IsSuccess = true
        };
    }

    private async Task<ValidationResult> ValidateAsync(int shiftId, int phoneId, int employeeId, CancellationToken cancellationToken)
    {
        if (shiftId <= 0)
        {
            return new ValidationResult("ShiftNotFound");
        }

        if (phoneId <= 0 || employeeId <= 0)
        {
            return new ValidationResult("InvalidRequest");
        }

        var shift = await _phoneAllocationRepository.GetShiftByIdAsync(shiftId, cancellationToken);

        if (shift is null)
        {
            return new ValidationResult("ShiftNotFound");
        }

        var phone = await _phoneAllocationRepository.GetPhoneByIdAsync(phoneId, cancellationToken);

        if (phone is null || !phone.IsActive)
        {
            return new ValidationResult("PhoneNotFound");
        }

        var employee = await _phoneAllocationRepository.GetEmployeeByIdAsync(employeeId, cancellationToken);

        if (employee is null || !employee.IsActive)
        {
            return new ValidationResult("EmployeeNotFound");
        }

        var shiftEmployeeIds = await _phoneAllocationRepository.GetShiftEmployeeIdsAsync(shiftId, cancellationToken);

        if (!shiftEmployeeIds.Contains(employeeId))
        {
            return new ValidationResult("EmployeeNotOnShift");
        }

        return ValidationResult.Success;
    }

    private async Task<IReadOnlyList<PhoneAllocationDto>> MapDtosAsync(IReadOnlyList<PhoneAllocation> phoneAllocations, CancellationToken cancellationToken)
    {
        var employeeIds = phoneAllocations.Select(allocation => allocation.EmployeeId).Distinct().ToList();
        var phoneIds = phoneAllocations.Select(allocation => allocation.PhoneId).Distinct().ToList();

        var employees = await _phoneAllocationRepository.GetEmployeesByIdsAsync(employeeIds, cancellationToken);
        var phones = await _phoneAllocationRepository.GetPhonesByIdsAsync(phoneIds, cancellationToken);

        var employeeLookup = employees.ToDictionary(employee => employee.Id);
        var phoneLookup = phones.ToDictionary(phone => phone.Id);

        return phoneAllocations
            .OrderBy(allocation => phoneLookup.TryGetValue(allocation.PhoneId, out var phone) ? phone.NameOrNumber : string.Empty)
            .Select(allocation => MapDto(allocation, employeeLookup, phoneLookup))
            .ToList();
    }

    private async Task<PhoneAllocationDto> MapDtoAsync(PhoneAllocation phoneAllocation, CancellationToken cancellationToken)
    {
        var employee = await _phoneAllocationRepository.GetEmployeeByIdAsync(phoneAllocation.EmployeeId, cancellationToken);
        var phone = await _phoneAllocationRepository.GetPhoneByIdAsync(phoneAllocation.PhoneId, cancellationToken);

        return new PhoneAllocationDto
        {
            Id = phoneAllocation.Id,
            ShiftId = phoneAllocation.ShiftId,
            PhoneId = phoneAllocation.PhoneId,
            PhoneNameOrNumber = phone?.NameOrNumber ?? string.Empty,
            EmployeeId = phoneAllocation.EmployeeId,
            EmployeeName = employee?.Name ?? string.Empty
        };
    }

    private static PhoneAllocationDto MapDto(
        PhoneAllocation phoneAllocation,
        IReadOnlyDictionary<int, Employee> employeeLookup,
        IReadOnlyDictionary<int, Phone> phoneLookup)
    {
        return new PhoneAllocationDto
        {
            Id = phoneAllocation.Id,
            ShiftId = phoneAllocation.ShiftId,
            PhoneId = phoneAllocation.PhoneId,
            PhoneNameOrNumber = phoneLookup.TryGetValue(phoneAllocation.PhoneId, out var phone) ? phone.NameOrNumber : string.Empty,
            EmployeeId = phoneAllocation.EmployeeId,
            EmployeeName = employeeLookup.TryGetValue(phoneAllocation.EmployeeId, out var employee) ? employee.Name : string.Empty
        };
    }

    private static CreatePhoneAllocationResult CreateFailure(string error)
    {
        return new CreatePhoneAllocationResult
        {
            IsSuccess = false,
            Error = error
        };
    }

    private static UpdatePhoneAllocationResult UpdateFailure(string error)
    {
        return new UpdatePhoneAllocationResult
        {
            IsSuccess = false,
            Error = error
        };
    }

    private static DeletePhoneAllocationResult Failure(string error)
    {
        return new DeletePhoneAllocationResult
        {
            IsSuccess = false,
            Error = error
        };
    }

    private sealed class ValidationResult
    {
        public static ValidationResult Success => new(null) { IsValid = true };

        public ValidationResult(string? error)
        {
            Error = error;
        }

        public bool IsValid { get; init; }
        public string? Error { get; init; }
    }
}
