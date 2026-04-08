using Slottet.Application.DTOs.Employees;
using Slottet.Application.Interfaces;
using Slottet.Domain.Entities;

namespace Slottet.Application.Services.Employees;

public sealed class EmployeeService : IEmployeeService
{
    private static readonly string[] AllowedRoles = ["Admin", "Medarbejder", "Vagtansvarlig"];
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IPasswordHashingService _passwordHashingService;

    public EmployeeService(IEmployeeRepository employeeRepository, IPasswordHashingService passwordHashingService)
    {
        _employeeRepository = employeeRepository;
        _passwordHashingService = passwordHashingService;
    }

    public async Task<IReadOnlyList<EmployeeDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var employees = await _employeeRepository.GetAllAsync(cancellationToken);

        return employees
            .OrderBy(employee => employee.Name)
            .Select(employee => new EmployeeDto
            {
                Id = employee.Id,
                Name = employee.Name,
                Email = employee.Email,
                Role = employee.Role,
                IsActive = employee.IsActive
            })
            .ToList();
    }

    public async Task<CreateEmployeeResultDto> CreateAsync(CreateEmployeeRequestDto request, CancellationToken cancellationToken = default)
    {
        var validation = ValidateEmployeeData(request.Name, request.Email, request.Role);

        if (!validation.IsSuccess)
        {
            return new CreateEmployeeResultDto
            {
                IsSuccess = false,
                Error = validation.Error
            };
        }

        if (request.Password.Length < 6)
        {
            return new CreateEmployeeResultDto
            {
                IsSuccess = false,
                Error = "PasswordTooShort"
            };
        }

        var emailInUse = await _employeeRepository.EmailExistsAsync(validation.Email!, cancellationToken: cancellationToken);

        if (emailInUse)
        {
            return new CreateEmployeeResultDto
            {
                IsSuccess = false,
                Error = "EmailAlreadyExists"
            };
        }

        var employee = new Employee
        {
            Name = validation.Name!,
            Email = validation.Email!,
            Role = validation.Role!,
            IsActive = request.IsActive
        };

        employee.PasswordHash = _passwordHashingService.Hash(employee, request.Password);

        var createdEmployee = await _employeeRepository.CreateAsync(employee, cancellationToken);

        return new CreateEmployeeResultDto
        {
            IsSuccess = true,
            Employee = MapEmployee(createdEmployee)
        };
    }

    public async Task<UpdateEmployeeResultDto> UpdateAsync(int employeeId, UpdateEmployeeRequestDto request, CancellationToken cancellationToken = default)
    {
        if (employeeId <= 0)
        {
            return new UpdateEmployeeResultDto
            {
                IsSuccess = false,
                Error = "NotFound"
            };
        }

        var employee = await _employeeRepository.GetByIdAsync(employeeId, cancellationToken);

        if (employee is null)
        {
            return new UpdateEmployeeResultDto
            {
                IsSuccess = false,
                Error = "NotFound"
            };
        }

        var validation = ValidateEmployeeData(request.Name, request.Email, request.Role);

        if (!validation.IsSuccess)
        {
            return new UpdateEmployeeResultDto
            {
                IsSuccess = false,
                Error = validation.Error
            };
        }

        var emailInUse = await _employeeRepository.EmailExistsAsync(validation.Email!, employeeId, cancellationToken);

        if (emailInUse)
        {
            return new UpdateEmployeeResultDto
            {
                IsSuccess = false,
                Error = "EmailAlreadyExists"
            };
        }

        employee.Name = validation.Name!;
        employee.Email = validation.Email!;
        employee.Role = validation.Role!;
        employee.IsActive = request.IsActive;

        var updatedEmployee = await _employeeRepository.UpdateAsync(employee, cancellationToken);

        return new UpdateEmployeeResultDto
        {
            IsSuccess = true,
            Employee = MapEmployee(updatedEmployee)
        };
    }

    public async Task<DeleteEmployeeResultDto> DeleteAsync(int employeeId, CancellationToken cancellationToken = default)
    {
        if (employeeId <= 0)
        {
            return new DeleteEmployeeResultDto
            {
                IsSuccess = false,
                Error = "NotFound"
            };
        }

        var employee = await _employeeRepository.GetByIdAsync(employeeId, cancellationToken);

        if (employee is null)
        {
            return new DeleteEmployeeResultDto
            {
                IsSuccess = false,
                Error = "NotFound"
            };
        }

        var deleted = await _employeeRepository.DeleteAsync(employee, cancellationToken);

        if (!deleted)
        {
            return new DeleteEmployeeResultDto
            {
                IsSuccess = false,
                Error = "HasRelations"
            };
        }

        return new DeleteEmployeeResultDto
        {
            IsSuccess = true
        };
    }

    private static EmployeeDto MapEmployee(Employee employee)
    {
        return new EmployeeDto
        {
            Id = employee.Id,
            Name = employee.Name,
            Email = employee.Email,
            Role = employee.Role,
            IsActive = employee.IsActive
        };
    }

    private static EmployeeValidationResult ValidateEmployeeData(string name, string email, string role)
    {
        var normalizedName = name.Trim();
        var normalizedEmail = email.Trim();
        var normalizedRole = role.Trim();

        if (string.IsNullOrWhiteSpace(normalizedName) ||
            string.IsNullOrWhiteSpace(normalizedEmail) ||
            string.IsNullOrWhiteSpace(normalizedRole))
        {
            return new EmployeeValidationResult
            {
                Error = "MissingFields"
            };
        }

        var resolvedRole = AllowedRoles.FirstOrDefault(candidate =>
            string.Equals(candidate, normalizedRole, StringComparison.OrdinalIgnoreCase));

        if (resolvedRole is null)
        {
            return new EmployeeValidationResult
            {
                Error = "InvalidRole"
            };
        }

        return new EmployeeValidationResult
        {
            IsSuccess = true,
            Name = normalizedName,
            Email = normalizedEmail,
            Role = resolvedRole
        };
    }

    private sealed class EmployeeValidationResult
    {
        public bool IsSuccess { get; init; }
        public string? Error { get; init; }
        public string? Name { get; init; }
        public string? Email { get; init; }
        public string? Role { get; init; }
    }
}
