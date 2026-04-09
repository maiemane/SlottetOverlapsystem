using Slottet.Application.DTOs.Staffing;
using Slottet.Application.Interfaces;
using Slottet.Domain.Enums;

namespace Slottet.Application.Services.Staffing;

public sealed class StaffAllocationService : IStaffAllocationService
{
    private readonly IStaffAllocationRepository _staffAllocationRepository;

    public StaffAllocationService(IStaffAllocationRepository staffAllocationRepository)
    {
        _staffAllocationRepository = staffAllocationRepository;
    }

    public async Task<CitizenAssignmentBoardDto?> GetCitizenAssignmentBoardAsync(int departmentId, DateTime date, CancellationToken cancellationToken = default)
    {
        if (departmentId <= 0)
        {
            return null;
        }

        var department = await _staffAllocationRepository.GetDepartmentByIdAsync(departmentId, cancellationToken);

        if (department is null)
        {
            return null;
        }

        var targetDate = date.Date;
        var shifts = await _staffAllocationRepository.EnsureShiftsForDepartmentAndDateAsync(departmentId, targetDate, cancellationToken);
        var citizens = await _staffAllocationRepository.GetActiveCitizensByDepartmentAsync(departmentId, cancellationToken);
        var employees = await _staffAllocationRepository.GetActiveEmployeesAsync(cancellationToken);
        var shiftIds = shifts.Select(shift => shift.Id).ToList();
        var assignments = await _staffAllocationRepository.GetCitizenAssignmentsByShiftIdsAsync(shiftIds, cancellationToken);

        return new CitizenAssignmentBoardDto
        {
            DepartmentId = department.Id,
            DepartmentName = department.Name,
            Date = targetDate,
            Employees = employees
                .OrderBy(employee => employee.Name)
                .Select(MapStaffMember)
                .ToList(),
            Citizens = citizens
                .OrderBy(citizen => citizen.ApartmentNumber)
                .ThenBy(citizen => citizen.Name)
                .Select(citizen => new CitizenAssignmentBoardCitizenDto
                {
                    CitizenId = citizen.Id,
                    Name = citizen.Name,
                    ApartmentNumber = citizen.ApartmentNumber,
                    Shifts = shifts
                        .OrderBy(shift => shift.Type)
                        .Select(shift => new CitizenAssignmentBoardShiftDto
                        {
                            ShiftId = shift.Id,
                            ShiftType = shift.Type,
                            AssignedEmployeeIds = assignments
                                .Where(assignment => assignment.CitizenId == citizen.Id && assignment.ShiftId == shift.Id)
                                .OrderBy(assignment => assignment.EmployeeId)
                                .Select(assignment => assignment.EmployeeId)
                                .ToList()
                        })
                        .ToList()
                })
                .ToList()
        };
    }

    public async Task<ShiftLookupDto?> GetShiftByDepartmentDateAndTypeAsync(int departmentId, DateTime date, ShiftType shiftType, CancellationToken cancellationToken = default)
    {
        if (departmentId <= 0 || !Enum.IsDefined(typeof(ShiftType), shiftType))
        {
            return null;
        }

        var department = await _staffAllocationRepository.GetDepartmentByIdAsync(departmentId, cancellationToken);

        if (department is null)
        {
            return null;
        }

        var targetDate = date.Date;
        var shifts = await _staffAllocationRepository.EnsureShiftsForDepartmentAndDateAsync(departmentId, targetDate, cancellationToken);
        var shift = shifts.FirstOrDefault(candidate => candidate.Type == shiftType);

        if (shift is null)
        {
            return null;
        }

        return new ShiftLookupDto
        {
            ShiftId = shift.Id,
            DepartmentId = shift.DepartmentId,
            Date = shift.Date,
            ShiftType = shift.Type
        };
    }

    public async Task<ShiftStaffingDto?> GetShiftEmployeesAsync(int shiftId, CancellationToken cancellationToken = default)
    {
        if (shiftId <= 0)
        {
            return null;
        }

        var shift = await _staffAllocationRepository.GetShiftByIdAsync(shiftId, cancellationToken);

        if (shift is null)
        {
            return null;
        }

        var employees = await _staffAllocationRepository.GetEmployeesByShiftAsync(shiftId, cancellationToken);

        return new ShiftStaffingDto
        {
            ShiftId = shiftId,
            Employees = employees
                .OrderBy(employee => employee.Name)
                .Select(MapStaffMember)
                .ToList()
        };
    }

    public async Task<CitizenStaffingDto?> GetCitizenEmployeesAsync(int shiftId, int citizenId, CancellationToken cancellationToken = default)
    {
        if (shiftId <= 0 || citizenId <= 0)
        {
            return null;
        }

        var shift = await _staffAllocationRepository.GetShiftByIdAsync(shiftId, cancellationToken);
        var citizen = await _staffAllocationRepository.GetCitizenByIdAsync(citizenId, cancellationToken);

        if (shift is null || citizen is null || !citizen.IsActive || citizen.DepartmentId != shift.DepartmentId)
        {
            return null;
        }

        var employees = await _staffAllocationRepository.GetEmployeesByCitizenAssignmentAsync(shiftId, citizenId, cancellationToken);

        return new CitizenStaffingDto
        {
            ShiftId = shiftId,
            CitizenId = citizenId,
            Employees = employees
                .OrderBy(employee => employee.Name)
                .Select(MapStaffMember)
                .ToList()
        };
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

    private static StaffMemberDto MapStaffMember(Domain.Entities.Employee employee)
    {
        return new StaffMemberDto
        {
            EmployeeId = employee.Id,
            Name = employee.Name,
            Email = employee.Email,
            Role = employee.Role
        };
    }
}
