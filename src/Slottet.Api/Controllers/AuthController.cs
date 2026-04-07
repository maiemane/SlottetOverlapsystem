using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Slottet.Application.DTOs.Auth;
using Slottet.Application.Interfaces;

namespace Slottet.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ILoginService _loginService;

    public AuthController(ILoginService loginService)
    {
        _loginService = loginService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginResult>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _loginService.LoginAsync(new global::Slottet.Application.DTOs.Auth.LoginRequest
        {
            Email = request.Email,
            Password = request.Password
        }, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error switch
            {
                "MissingCredentials" => BadRequest("Email og password er påkrævet."),
                "InvalidCredentials" => Unauthorized(),
                _ => StatusCode(StatusCodes.Status500InternalServerError)
            };
        }

        return Ok(new LoginResult
        {
            AccessToken = result.AccessToken!,
            ExpiresAtUtc = result.ExpiresAtUtc!.Value,
            Name = result.Name!,
            Email = result.Email!,
            Role = result.Role!
        });
    }
}
