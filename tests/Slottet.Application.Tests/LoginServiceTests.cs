using Slottet.Application.DTOs.Auth;
using Slottet.Application.Interfaces;
using Slottet.Application.Services.Auth;
using Slottet.Domain.Entities;

namespace Slottet.Application.Tests;

public class LoginServiceTests
{
    [Fact]
    public async Task LoginAsync_Returns_success_when_credentials_are_valid()
    {
        // Arrange
        var employee = new Employee
        {
            Id = 1,
            Name = "Anna Jensen",
            Email = "anna@slottet.dk",
            Role = "Admin",
            IsActive = true
        };

        var employeeRepository = new FakeEmployeeRepository(employee);
        var passwordVerificationService = new FakePasswordVerificationService(shouldVerify: true);
        var jwtTokenGenerator = new FakeJwtTokenGenerator(
            accessToken: "fake-jwt-token",
            expiresAtUtc: new DateTime(2030, 1, 1, 0, 0, 0, DateTimeKind.Utc));

        var sut = new LoginService(
            employeeRepository,
            passwordVerificationService,
            jwtTokenGenerator);

        var request = new LoginRequest
        {
            Email = "anna@slottet.dk",
            Password = "Password123!"
        };

        // Act
        var result = await sut.LoginAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("fake-jwt-token", result.AccessToken);
        Assert.Equal(employee.Name, result.Name);
        Assert.Equal(employee.Email, result.Email);
        Assert.Equal(employee.Role, result.Role);
        Assert.Equal(new DateTime(2030, 1, 1, 0, 0, 0, DateTimeKind.Utc), result.ExpiresAtUtc);
        Assert.Null(result.Error);
    }

    [Fact]
    public async Task LoginAsync_Returns_failure_when_employee_is_not_found()
    {
        // Arrange
        var employeeRepository = new FakeEmployeeRepository(null);
        var passwordVerificationService = new FakePasswordVerificationService(shouldVerify: true);
        var jwtTokenGenerator = new FakeJwtTokenGenerator("fake-jwt-token", DateTime.UtcNow.AddHours(1));

        var sut = new LoginService(
            employeeRepository,
            passwordVerificationService,
            jwtTokenGenerator);

        var request = new LoginRequest
        {
            Email = "ukendt@slottet.dk",
            Password = "Password123!"
        };

        // Act
        var result = await sut.LoginAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("InvalidCredentials", result.Error);
        Assert.Null(result.AccessToken);
    }

    [Fact]
    public async Task LoginAsync_Returns_failure_when_password_is_invalid()
    {
        // Arrange
        var employee = new Employee
        {
            Id = 1,
            Name = "Anna Jensen",
            Email = "anna@slottet.dk",
            Role = "Admin",
            IsActive = true
        };

        var employeeRepository = new FakeEmployeeRepository(employee);
        var passwordVerificationService = new FakePasswordVerificationService(shouldVerify: false);
        var jwtTokenGenerator = new FakeJwtTokenGenerator("fake-jwt-token", DateTime.UtcNow.AddHours(1));

        var sut = new LoginService(
            employeeRepository,
            passwordVerificationService,
            jwtTokenGenerator);

        var request = new LoginRequest
        {
            Email = "anna@slottet.dk",
            Password = "ForkertKode"
        };

        // Act
        var result = await sut.LoginAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("InvalidCredentials", result.Error);
        Assert.Null(result.AccessToken);
    }

    [Fact]
    public async Task LoginAsync_Returns_failure_when_email_is_empty()
    {
        // Arrange
        var employeeRepository = new FakeEmployeeRepository(null);
        var passwordVerificationService = new FakePasswordVerificationService(shouldVerify: false);
        var jwtTokenGenerator = new FakeJwtTokenGenerator("fake-jwt-token", DateTime.UtcNow.AddHours(1));

        var sut = new LoginService(
            employeeRepository,
            passwordVerificationService,
            jwtTokenGenerator);

        var request = new LoginRequest
        {
            Email = "",
            Password = "Password123!"
        };

        // Act
        var result = await sut.LoginAsync(request, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("MissingCredentials", result.Error);
        Assert.Null(result.AccessToken);
    }

    private sealed class FakeEmployeeRepository : IEmployeeRepository
    {
        private readonly Employee? _employee;

        public FakeEmployeeRepository(Employee? employee)
        {
            _employee = employee;
        }

        public Task<Employee?> GetActiveByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_employee);
        }
    }

    private sealed class FakePasswordVerificationService : IPasswordVerificationService
    {
        private readonly bool _shouldVerify;

        public FakePasswordVerificationService(bool shouldVerify)
        {
            _shouldVerify = shouldVerify;
        }

        public bool Verify(Employee employee, string password)
        {
            return _shouldVerify;
        }
    }

    private sealed class FakeJwtTokenGenerator : IJwtTokenGenerator
    {
        private readonly string _accessToken;
        private readonly DateTime _expiresAtUtc;

        public FakeJwtTokenGenerator(string accessToken, DateTime expiresAtUtc)
        {
            _accessToken = accessToken;
            _expiresAtUtc = expiresAtUtc;
        }

        public JwtTokenResult CreateToken(Employee employee)
        {
            return new JwtTokenResult
            {
                AccessToken = _accessToken,
                ExpiresAtUtc = _expiresAtUtc
            };
        }
    }
}