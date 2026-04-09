using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Slottet.Application.Interfaces;

namespace Slottet.Infrastructure.Auth;

public sealed class HttpCurrentUserContext : ICurrentUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpCurrentUserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int? EmployeeId
    {
        get
        {
            var employeeIdClaim = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

            return int.TryParse(employeeIdClaim, out var employeeId)
                ? employeeId
                : null;
        }
    }

    public string RequestPath => _httpContextAccessor.HttpContext?.Request.Path.Value ?? string.Empty;

    public string CorrelationId => _httpContextAccessor.HttpContext?.TraceIdentifier ?? string.Empty;
}
