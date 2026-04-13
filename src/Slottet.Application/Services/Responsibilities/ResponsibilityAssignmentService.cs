using Slottet.Application.DTOs.Responsibilities;
using Slottet.Application.Interfaces;
using Slottet.Domain.Entities;

namespace Slottet.Application.Services.Responsibilities;

public sealed class ResponsibilityAssignmentService : IResponsibilityAssignmentService
{
    private readonly IResponsibilityAssignmentRepository _responsibilityAssignmentRepository;

    public ResponsibilityAssignmentService(IResponsibilityAssignmentRepository responsibilityAssignmentRepository)
    {
        _responsibilityAssignmentRepository = responsibilityAssignmentRepository;
    }

    public async Task<IReadOnlyList<ResponsibilityAssignmentDto>?> GetByShiftAsync(int shiftId, CancellationToken cancellationToken = default)
    {
        if (shiftId <= 0)
        {
            return null;
        }

        var shift = await _responsibilityAssignmentRepository.GetShiftByIdAsync(shiftId, cancellationToken);

        if (shift is null)
        {
            return null;
        }

        var responsibilityAssignments = await _responsibilityAssignmentRepository.GetResponsibilityAssignmentsAsync(shiftId, cancellationToken);
        return await MapDtosAsync(responsibilityAssignments, cancellationToken);
    }

    public async Task<ResponsibilityAssignmentDto?> GetByIdAsync(int shiftId, int responsibilityAssignmentId, CancellationToken cancellationToken = default)
    {
        if (shiftId <= 0 || responsibilityAssignmentId <= 0)
        {
            return null;
        }

        var shift = await _responsibilityAssignmentRepository.GetShiftByIdAsync(shiftId, cancellationToken);

        if (shift is null)
        {
            return null;
        }

        var responsibilityAssignment = await _responsibilityAssignmentRepository.GetResponsibilityAssignmentByIdAsync(responsibilityAssignmentId, cancellationToken);

        if (responsibilityAssignment is null || responsibilityAssignment.ShiftId != shiftId)
        {
            return null;
        }

        return await MapDtoAsync(responsibilityAssignment, cancellationToken);
    }

    public async Task<CreateResponsibilityAssignmentResult> CreateAsync(int shiftId, CreateResponsibilityAssignmentRequest request, CancellationToken cancellationToken = default)
    {
        var validation = await ValidateAsync(shiftId, request.ResponsibilityTypeId, request.EmployeeId, cancellationToken);

        if (!validation.IsValid)
        {
            return CreateFailure(validation.Error!);
        }

        var existingAssignment = await _responsibilityAssignmentRepository.GetResponsibilityAssignmentByShiftAndTypeAsync(shiftId, request.ResponsibilityTypeId, cancellationToken);

        if (existingAssignment is not null)
        {
            return CreateFailure("ResponsibilityAlreadyAssigned");
        }

        var responsibilityAssignment = await _responsibilityAssignmentRepository.AddResponsibilityAssignmentAsync(new ResponsibilityAssignment
        {
            ShiftId = shiftId,
            ResponsibilityTypeId = request.ResponsibilityTypeId,
            EmployeeId = request.EmployeeId
        }, cancellationToken);

        return new CreateResponsibilityAssignmentResult
        {
            IsSuccess = true,
            ResponsibilityAssignment = await MapDtoAsync(responsibilityAssignment, cancellationToken)
        };
    }

    public async Task<UpdateResponsibilityAssignmentResult> UpdateAsync(int shiftId, int responsibilityAssignmentId, UpdateResponsibilityAssignmentRequest request, CancellationToken cancellationToken = default)
    {
        if (responsibilityAssignmentId <= 0)
        {
            return UpdateFailure("ResponsibilityAssignmentNotFound");
        }

        var validation = await ValidateAsync(shiftId, request.ResponsibilityTypeId, request.EmployeeId, cancellationToken);

        if (!validation.IsValid)
        {
            return UpdateFailure(validation.Error!);
        }

        var responsibilityAssignment = await _responsibilityAssignmentRepository.GetResponsibilityAssignmentByIdAsync(responsibilityAssignmentId, cancellationToken);

        if (responsibilityAssignment is null || responsibilityAssignment.ShiftId != shiftId)
        {
            return UpdateFailure("ResponsibilityAssignmentNotFound");
        }

        var existingAssignment = await _responsibilityAssignmentRepository.GetResponsibilityAssignmentByShiftAndTypeAsync(shiftId, request.ResponsibilityTypeId, cancellationToken);

        if (existingAssignment is not null && existingAssignment.Id != responsibilityAssignmentId)
        {
            return UpdateFailure("ResponsibilityAlreadyAssigned");
        }

        responsibilityAssignment.ResponsibilityTypeId = request.ResponsibilityTypeId;
        responsibilityAssignment.EmployeeId = request.EmployeeId;

        var updatedAssignment = await _responsibilityAssignmentRepository.UpdateResponsibilityAssignmentAsync(responsibilityAssignment, cancellationToken);

        return new UpdateResponsibilityAssignmentResult
        {
            IsSuccess = true,
            ResponsibilityAssignment = await MapDtoAsync(updatedAssignment, cancellationToken)
        };
    }

    public async Task<DeleteResponsibilityAssignmentResult> DeleteAsync(int shiftId, int responsibilityAssignmentId, CancellationToken cancellationToken = default)
    {
        if (shiftId <= 0)
        {
            return Failure("ShiftNotFound");
        }

        if (responsibilityAssignmentId <= 0)
        {
            return Failure("ResponsibilityAssignmentNotFound");
        }

        var shift = await _responsibilityAssignmentRepository.GetShiftByIdAsync(shiftId, cancellationToken);

        if (shift is null)
        {
            return Failure("ShiftNotFound");
        }

        var responsibilityAssignment = await _responsibilityAssignmentRepository.GetResponsibilityAssignmentByIdAsync(responsibilityAssignmentId, cancellationToken);

        if (responsibilityAssignment is null || responsibilityAssignment.ShiftId != shiftId)
        {
            return Failure("ResponsibilityAssignmentNotFound");
        }

        await _responsibilityAssignmentRepository.DeleteResponsibilityAssignmentAsync(responsibilityAssignment, cancellationToken);

        return new DeleteResponsibilityAssignmentResult
        {
            IsSuccess = true
        };
    }

    private async Task<ValidationResult> ValidateAsync(int shiftId, int responsibilityTypeId, int employeeId, CancellationToken cancellationToken)
    {
        if (shiftId <= 0)
        {
            return new ValidationResult("ShiftNotFound");
        }

        if (responsibilityTypeId <= 0 || employeeId <= 0)
        {
            return new ValidationResult("InvalidRequest");
        }

        var shift = await _responsibilityAssignmentRepository.GetShiftByIdAsync(shiftId, cancellationToken);

        if (shift is null)
        {
            return new ValidationResult("ShiftNotFound");
        }

        var responsibilityType = await _responsibilityAssignmentRepository.GetResponsibilityTypeByIdAsync(responsibilityTypeId, cancellationToken);

        if (responsibilityType is null)
        {
            return new ValidationResult("ResponsibilityTypeNotFound");
        }

        var employee = await _responsibilityAssignmentRepository.GetEmployeeByIdAsync(employeeId, cancellationToken);

        if (employee is null || !employee.IsActive)
        {
            return new ValidationResult("EmployeeNotFound");
        }

        var shiftEmployeeIds = await _responsibilityAssignmentRepository.GetShiftEmployeeIdsAsync(shiftId, cancellationToken);

        if (!shiftEmployeeIds.Contains(employeeId))
        {
            return new ValidationResult("EmployeeNotOnShift");
        }

        return ValidationResult.Success;
    }

    private async Task<IReadOnlyList<ResponsibilityAssignmentDto>> MapDtosAsync(IReadOnlyList<ResponsibilityAssignment> responsibilityAssignments, CancellationToken cancellationToken)
    {
        var employeeIds = responsibilityAssignments.Select(assignment => assignment.EmployeeId).Distinct().ToList();
        var responsibilityTypeIds = responsibilityAssignments.Select(assignment => assignment.ResponsibilityTypeId).Distinct().ToList();

        var employees = await _responsibilityAssignmentRepository.GetEmployeesByIdsAsync(employeeIds, cancellationToken);
        var responsibilityTypes = await _responsibilityAssignmentRepository.GetResponsibilityTypesByIdsAsync(responsibilityTypeIds, cancellationToken);

        var employeeLookup = employees.ToDictionary(employee => employee.Id);
        var responsibilityTypeLookup = responsibilityTypes.ToDictionary(type => type.Id);

        return responsibilityAssignments
            .OrderBy(assignment => responsibilityTypeLookup.TryGetValue(assignment.ResponsibilityTypeId, out var type) ? type.Name : string.Empty)
            .Select(assignment => MapDto(assignment, employeeLookup, responsibilityTypeLookup))
            .ToList();
    }

    private async Task<ResponsibilityAssignmentDto> MapDtoAsync(ResponsibilityAssignment responsibilityAssignment, CancellationToken cancellationToken)
    {
        var employee = await _responsibilityAssignmentRepository.GetEmployeeByIdAsync(responsibilityAssignment.EmployeeId, cancellationToken);
        var responsibilityType = await _responsibilityAssignmentRepository.GetResponsibilityTypeByIdAsync(responsibilityAssignment.ResponsibilityTypeId, cancellationToken);

        return new ResponsibilityAssignmentDto
        {
            Id = responsibilityAssignment.Id,
            ShiftId = responsibilityAssignment.ShiftId,
            ResponsibilityTypeId = responsibilityAssignment.ResponsibilityTypeId,
            ResponsibilityTypeName = responsibilityType?.Name ?? string.Empty,
            EmployeeId = responsibilityAssignment.EmployeeId,
            EmployeeName = employee?.Name ?? string.Empty
        };
    }

    private static ResponsibilityAssignmentDto MapDto(
        ResponsibilityAssignment responsibilityAssignment,
        IReadOnlyDictionary<int, Employee> employeeLookup,
        IReadOnlyDictionary<int, ResponsibilityType> responsibilityTypeLookup)
    {
        return new ResponsibilityAssignmentDto
        {
            Id = responsibilityAssignment.Id,
            ShiftId = responsibilityAssignment.ShiftId,
            ResponsibilityTypeId = responsibilityAssignment.ResponsibilityTypeId,
            ResponsibilityTypeName = responsibilityTypeLookup.TryGetValue(responsibilityAssignment.ResponsibilityTypeId, out var type) ? type.Name : string.Empty,
            EmployeeId = responsibilityAssignment.EmployeeId,
            EmployeeName = employeeLookup.TryGetValue(responsibilityAssignment.EmployeeId, out var employee) ? employee.Name : string.Empty
        };
    }

    private static CreateResponsibilityAssignmentResult CreateFailure(string error)
    {
        return new CreateResponsibilityAssignmentResult
        {
            IsSuccess = false,
            Error = error
        };
    }

    private static UpdateResponsibilityAssignmentResult UpdateFailure(string error)
    {
        return new UpdateResponsibilityAssignmentResult
        {
            IsSuccess = false,
            Error = error
        };
    }

    private static DeleteResponsibilityAssignmentResult Failure(string error)
    {
        return new DeleteResponsibilityAssignmentResult
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
