using Slottet.Application.DTOs.Staffing;
using Slottet.Application.Interfaces;

namespace Slottet.Application.Services.Staffing;

public sealed class StaffAllocationService : IStaffAllocationService
{
    private readonly IStaffAllocationRepository _staffAllocationRepository;

    public StaffAllocationService(IStaffAllocationRepository staffAllocationRepository)
    {
        _staffAllocationRepository = staffAllocationRepository;
    }

    public async Task<AssignEmployeesToShiftResult> AssignEmployeesToShiftAsync(int shiftId, AssignEmployeesToShiftRequest request, CancellationToken cancellationToken = default)
    {
        if (shiftId <= 0 || request.EmployeeIds is null)
        {
            return InvalidShiftResult();
        }

        var shift = await _staffAllocationRepository.GetShiftByIdAsync(shiftId, cancellationToken);

        if (shift is null)
        {
            return new AssignEmployeesToShiftResult
            {
                IsSuccess = false,
                Error = "ShiftNotFound"
            };
        }

        var employeeIds = request.EmployeeIds
            .Where(employeeId => employeeId > 0)
            .Distinct()
            .OrderBy(employeeId => employeeId)
            .ToList();

        var employees = await _staffAllocationRepository.GetActiveEmployeesByIdsAsync(employeeIds, cancellationToken);

        if (employees.Count != employeeIds.Count)
        {
            return new AssignEmployeesToShiftResult
            {
                IsSuccess = false,
                Error = "EmployeeNotFound"
            };
        }

        await _staffAllocationRepository.ReplaceShiftEmployeesAsync(shiftId, employeeIds, cancellationToken);

        return new AssignEmployeesToShiftResult
        {
            IsSuccess = true,
            Assignment = new AssignEmployeesToShiftResponse
            {
                ShiftId = shiftId,
                EmployeeIds = employeeIds
            }
        };
    }

    public async Task<AssignEmployeesToCitizenResult> AssignEmployeesToCitizenAsync(int shiftId, int citizenId, AssignEmployeesToCitizenRequest request, CancellationToken cancellationToken = default)
    {
        if (shiftId <= 0 || citizenId <= 0 || request.EmployeeIds is null)
        {
            return InvalidCitizenAssignmentResult();
        }

        var shift = await _staffAllocationRepository.GetShiftByIdAsync(shiftId, cancellationToken);

        if (shift is null)
        {
            return new AssignEmployeesToCitizenResult
            {
                IsSuccess = false,
                Error = "ShiftNotFound"
            };
        }

        var citizen = await _staffAllocationRepository.GetCitizenByIdAsync(citizenId, cancellationToken);

        if (citizen is null || !citizen.IsActive)
        {
            return new AssignEmployeesToCitizenResult
            {
                IsSuccess = false,
                Error = "CitizenNotFound"
            };
        }

        if (citizen.DepartmentId != shift.DepartmentId)
        {
            return new AssignEmployeesToCitizenResult
            {
                IsSuccess = false,
                Error = "CitizenNotInShiftDepartment"
            };
        }

        var employeeIds = request.EmployeeIds
            .Where(employeeId => employeeId > 0)
            .Distinct()
            .OrderBy(employeeId => employeeId)
            .ToList();

        if (employeeIds.Count > 2)
        {
            return new AssignEmployeesToCitizenResult
            {
                IsSuccess = false,
                Error = "TooManyEmployees"
            };
        }

        var shiftEmployeeIds = await _staffAllocationRepository.GetShiftEmployeeIdsAsync(shiftId, cancellationToken);

        if (employeeIds.Except(shiftEmployeeIds).Any())
        {
            return new AssignEmployeesToCitizenResult
            {
                IsSuccess = false,
                Error = "EmployeeNotOnShift"
            };
        }

        await _staffAllocationRepository.ReplaceCitizenAssignmentsAsync(shiftId, citizenId, employeeIds, cancellationToken);

        return new AssignEmployeesToCitizenResult
        {
            IsSuccess = true,
            Assignment = new AssignEmployeesToCitizenResponse
            {
                ShiftId = shiftId,
                CitizenId = citizenId,
                EmployeeIds = employeeIds
            }
        };
    }

    private static AssignEmployeesToShiftResult InvalidShiftResult()
    {
        return new AssignEmployeesToShiftResult
        {
            IsSuccess = false,
            Error = "InvalidRequest"
        };
    }

    private static AssignEmployeesToCitizenResult InvalidCitizenAssignmentResult()
    {
        return new AssignEmployeesToCitizenResult
        {
            IsSuccess = false,
            Error = "InvalidRequest"
        };
    }
}
