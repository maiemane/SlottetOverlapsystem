using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Slottet.Domain.Entities;

namespace Slottet.Api.Auth;

public sealed class JwtTokenGenerator
{
    private readonly JwtOptions _options;

    public JwtTokenGenerator(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public AuthTokenResult CreateToken(Employee employee)
    {
        var now = DateTime.UtcNow;
        var expiresAt = now.AddMinutes(_options.ExpirationMinutes);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, employee.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, employee.Email),
            new(JwtRegisteredClaimNames.UniqueName, employee.Name),
            new(ClaimTypes.NameIdentifier, employee.Id.ToString()),
            new(ClaimTypes.Name, employee.Name),
            new(ClaimTypes.Email, employee.Email),
            new(ClaimTypes.Role, employee.Role)
        };

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey)),
            SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: now,
            expires: expiresAt,
            signingCredentials: credentials);

        var token = new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);

        return new AuthTokenResult
        {
            AccessToken = token,
            ExpiresAtUtc = expiresAt
        };
    }
}
