using Slottet.Application.DTOs.Employees;
using Slottet.Application.Interfaces;
using Slottet.Domain.Entities;

namespace Slottet.Application.Services.Employees;

public sealed class EmployeeService : IEmployeeService
{
    private static readonly string[] AllowedRoles = ["Admin", "Medarbejder"];
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
        var normalizedName = request.Name.Trim();
        var normalizedEmail = request.Email.Trim();
        var normalizedRole = request.Role.Trim();

        if (string.IsNullOrWhiteSpace(normalizedName) ||
            string.IsNullOrWhiteSpace(normalizedEmail) ||
            string.IsNullOrWhiteSpace(request.Password) ||
            string.IsNullOrWhiteSpace(normalizedRole))
        {
            return new CreateEmployeeResultDto
            {
                IsSuccess = false,
                Error = "MissingFields"
            };
        }

        var resolvedRole = AllowedRoles.FirstOrDefault(role =>
            string.Equals(role, normalizedRole, StringComparison.OrdinalIgnoreCase));

        if (resolvedRole is null)
        {
            return new CreateEmployeeResultDto
            {
                IsSuccess = false,
                Error = "InvalidRole"
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

        var emailInUse = await _employeeRepository.EmailExistsAsync(normalizedEmail, cancellationToken);

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
            Name = normalizedName,
            Email = normalizedEmail,
            Role = resolvedRole,
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
}
