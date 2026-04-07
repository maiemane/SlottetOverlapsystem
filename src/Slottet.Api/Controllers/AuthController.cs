using Microsoft.AspNetCore.Mvc;
using Slottet.Api.Auth;
using Slottet.Api.Contracts.Auth;
using Slottet.Application.Interfaces;

namespace Slottet.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly JwtTokenGenerator _jwtTokenGenerator;
    private readonly PasswordVerificationService _passwordVerificationService;

    public AuthController(
        IEmployeeRepository employeeRepository,
        JwtTokenGenerator jwtTokenGenerator,
        PasswordVerificationService passwordVerificationService)
    {
        _employeeRepository = employeeRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
        _passwordVerificationService = passwordVerificationService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest("Email og password er påkrævet.");
        }

        var employee = await _employeeRepository.GetActiveByEmailAsync(request.Email, cancellationToken);

        if (employee is null || !_passwordVerificationService.Verify(employee, request.Password))
        {
            return Unauthorized();
        }

        var token = _jwtTokenGenerator.CreateToken(employee);

        return Ok(new LoginResponse
        {
            AccessToken = token.AccessToken,
            ExpiresAtUtc = token.ExpiresAtUtc,
            Name = employee.Name,
            Email = employee.Email,
            Role = employee.Role
        });
    }
}
