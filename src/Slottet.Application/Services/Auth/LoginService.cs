using Slottet.Application.DTOs.Auth;
using Slottet.Application.Interfaces;

namespace Slottet.Application.Services.Auth;

public class LoginService : ILoginService
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IPasswordVerificationService _passwordVerificationService;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public LoginService(
        IEmployeeRepository employeeRepository,
        IPasswordVerificationService passwordVerificationService,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _employeeRepository = employeeRepository;
        _passwordVerificationService = passwordVerificationService;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<LoginResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return new LoginResult
            {
                IsSuccess = false,
                Error = "MissingCredentials"
            };
        }

        var employee = await _employeeRepository.GetActiveByEmailAsync(request.Email, cancellationToken);

        if (employee is null || !_passwordVerificationService.Verify(employee, request.Password))
        {
            return new LoginResult
            {
                IsSuccess = false,
                Error = "InvalidCredentials"
            };
        }

        var token = _jwtTokenGenerator.CreateToken(employee);

        return new LoginResult
        {
            IsSuccess = true,
            AccessToken = token.AccessToken,
            ExpiresAtUtc = token.ExpiresAtUtc,
            Name = employee.Name,
            Email = employee.Email,
            Role = employee.Role
        };
    }
}