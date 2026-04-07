using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Slottet.Api.Auth;
using Slottet.Api.Contracts.Auth;
using Slottet.Infrastructure.Data;

namespace Slottet.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    private readonly JwtTokenGenerator _jwtTokenGenerator;
    private readonly PasswordVerificationService _passwordVerificationService;

    public AuthController(
        ApplicationDbContext dbContext,
        JwtTokenGenerator jwtTokenGenerator,
        PasswordVerificationService passwordVerificationService)
    {
        _dbContext = dbContext;
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

        var employee = await _dbContext.Employees
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Email == request.Email.Trim() && x.IsActive, cancellationToken);

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
