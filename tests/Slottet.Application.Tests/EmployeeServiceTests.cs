using Slottet.Application.DTOs.Employees;
using Slottet.Application.Interfaces;
using Slottet.Application.Services.Employees;
using Slottet.Domain.Entities;

namespace Slottet.Application.Tests;

public class EmployeeServiceTests
{
    [Fact]
    public async Task GetAllAsync_Returns_sorted_employee_list()
    {
        var repository = new FakeEmployeeRepository(emailExists: false)
        {
            Employees =
            [
                new Employee { Id = 2, Name = "Peter Hansen", Email = "peter@slottet.dk", Role = "Medarbejder", IsActive = true },
                new Employee { Id = 1, Name = "Anna Jensen", Email = "anna@slottet.dk", Role = "Admin", IsActive = false }
            ]
        };

        var sut = new EmployeeService(repository, new FakePasswordHashingService());

        var result = await sut.GetAllAsync(CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Equal("Anna Jensen", result[0].Name);
        Assert.Equal("Peter Hansen", result[1].Name);
    }

    [Fact]
    public async Task CreateAsync_Creates_employee_with_hashed_password()
    {
        var repository = new FakeEmployeeRepository(emailExists: false);
        var passwordHashingService = new FakePasswordHashingService();
        var sut = new EmployeeService(repository, passwordHashingService);

        var result = await sut.CreateAsync(new CreateEmployeeRequestDto
        {
            Name = "Anna Jensen",
            Email = "anna@slottet.dk",
            Password = "Password123",
            Role = "Admin",
            IsActive = true
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Employee);
        Assert.Equal("hashed::Password123", repository.CreatedEmployee!.PasswordHash);
        Assert.Equal("anna@slottet.dk", repository.CreatedEmployee.Email);
        Assert.Equal("Anna Jensen", result.Employee!.Name);
        Assert.Equal(42, result.Employee.Id);
    }

    [Fact]
    public async Task CreateAsync_Allows_vagtansvarlig_role()
    {
        var repository = new FakeEmployeeRepository(emailExists: false);
        var sut = new EmployeeService(repository, new FakePasswordHashingService());

        var result = await sut.CreateAsync(new CreateEmployeeRequestDto
        {
            Name = "Anna Jensen",
            Email = "anna@slottet.dk",
            Password = "Password123",
            Role = "Vagtansvarlig"
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Vagtansvarlig", repository.CreatedEmployee!.Role);
    }

    [Fact]
    public async Task UpdateAsync_Updates_employee()
    {
        var repository = new FakeEmployeeRepository(emailExists: false)
        {
            EmployeeById = new Employee
            {
                Id = 42,
                Name = "Anna Jensen",
                Email = "anna@slottet.dk",
                Role = "Medarbejder",
                IsActive = true
            }
        };

        var sut = new EmployeeService(repository, new FakePasswordHashingService());

        var result = await sut.UpdateAsync(42, new UpdateEmployeeRequestDto
        {
            Name = "Anna Holm",
            Email = "anna.holm@slottet.dk",
            Role = "Vagtansvarlig",
            IsActive = false
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Employee);
        Assert.Equal("Anna Holm", repository.UpdatedEmployee!.Name);
        Assert.Equal("anna.holm@slottet.dk", repository.UpdatedEmployee.Email);
        Assert.Equal("Vagtansvarlig", repository.UpdatedEmployee.Role);
        Assert.False(repository.UpdatedEmployee.IsActive);
    }

    [Fact]
    public async Task UpdateAsync_Returns_failure_when_email_already_exists()
    {
        var repository = new FakeEmployeeRepository(emailExists: true)
        {
            EmployeeById = new Employee
            {
                Id = 42,
                Name = "Anna Jensen",
                Email = "anna@slottet.dk",
                Role = "Medarbejder",
                IsActive = true
            }
        };

        var sut = new EmployeeService(repository, new FakePasswordHashingService());

        var result = await sut.UpdateAsync(42, new UpdateEmployeeRequestDto
        {
            Name = "Anna Jensen",
            Email = "anna.holm@slottet.dk",
            Role = "Admin",
            IsActive = true
        }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("EmailAlreadyExists", result.Error);
    }

    [Fact]
    public async Task DeleteAsync_Returns_success_when_employee_is_deleted()
    {
        var repository = new FakeEmployeeRepository(emailExists: false)
        {
            EmployeeById = new Employee { Id = 42, Name = "Anna Jensen" },
            DeleteResult = true
        };

        var sut = new EmployeeService(repository, new FakePasswordHashingService());

        var result = await sut.DeleteAsync(42, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(42, repository.DeletedEmployeeId);
    }

    [Fact]
    public async Task DeleteAsync_Returns_failure_when_employee_has_relations()
    {
        var repository = new FakeEmployeeRepository(emailExists: false)
        {
            EmployeeById = new Employee { Id = 42, Name = "Anna Jensen" },
            DeleteResult = false
        };

        var sut = new EmployeeService(repository, new FakePasswordHashingService());

        var result = await sut.DeleteAsync(42, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("HasRelations", result.Error);
    }

    [Fact]
    public async Task CreateAsync_Returns_failure_when_email_already_exists()
    {
        var repository = new FakeEmployeeRepository(emailExists: true);
        var sut = new EmployeeService(repository, new FakePasswordHashingService());

        var result = await sut.CreateAsync(new CreateEmployeeRequestDto
        {
            Name = "Anna Jensen",
            Email = "anna@slottet.dk",
            Password = "Password123",
            Role = "Admin"
        }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("EmailAlreadyExists", result.Error);
        Assert.Null(repository.CreatedEmployee);
    }

    [Fact]
    public async Task CreateAsync_Returns_failure_when_role_is_invalid()
    {
        var repository = new FakeEmployeeRepository(emailExists: false);
        var sut = new EmployeeService(repository, new FakePasswordHashingService());

        var result = await sut.CreateAsync(new CreateEmployeeRequestDto
        {
            Name = "Anna Jensen",
            Email = "anna@slottet.dk",
            Password = "Password123",
            Role = "Leder"
        }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("InvalidRole", result.Error);
    }

    private sealed class FakeEmployeeRepository : IEmployeeRepository
    {
        private readonly bool _emailExists;

        public FakeEmployeeRepository(bool emailExists)
        {
            _emailExists = emailExists;
        }

        public Employee? CreatedEmployee { get; private set; }
        public Employee? UpdatedEmployee { get; private set; }
        public IReadOnlyList<Employee> Employees { get; init; } = [];
        public Employee? EmployeeById { get; init; }
        public bool DeleteResult { get; init; } = true;
        public int? DeletedEmployeeId { get; private set; }

        public Task<IReadOnlyList<Employee>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Employees);
        }

        public Task<Employee?> GetActiveByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<Employee?>(null);
        }

        public Task<Employee?> GetByIdAsync(int employeeId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(EmployeeById);
        }

        public Task<bool> EmailExistsAsync(string email, int? excludeEmployeeId = null, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_emailExists);
        }

        public Task<Employee> CreateAsync(Employee employee, CancellationToken cancellationToken = default)
        {
            employee.Id = 42;
            CreatedEmployee = employee;
            return Task.FromResult(employee);
        }

        public Task<Employee> UpdateAsync(Employee employee, CancellationToken cancellationToken = default)
        {
            UpdatedEmployee = employee;
            return Task.FromResult(employee);
        }

        public Task<bool> DeleteAsync(Employee employee, CancellationToken cancellationToken = default)
        {
            DeletedEmployeeId = employee.Id;
            return Task.FromResult(DeleteResult);
        }
    }

    private sealed class FakePasswordHashingService : IPasswordHashingService
    {
        public string Hash(Employee employee, string password)
        {
            return $"hashed::{password}";
        }
    }
}
