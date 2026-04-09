using Microsoft.AspNetCore.Http;
using Slottet.Application.Interfaces;
using Slottet.Domain.Entities;
using Slottet.Infrastructure.Data;

namespace Slottet.Infrastructure.Logging;

public sealed class AccessLogMiddleware
{
    private readonly RequestDelegate _next;

    public AccessLogMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ApplicationDbContext dbContext, ICurrentUserContext currentUserContext)
    {
        await _next(context);

        if (!context.Request.Path.StartsWithSegments("/api"))
        {
            return;
        }

        dbContext.AccessLogs.Add(new AccessLog
        {
            OccurredAtUtc = DateTime.UtcNow,
            EmployeeId = currentUserContext.EmployeeId,
            HttpMethod = context.Request.Method,
            RequestPath = context.Request.Path.Value ?? string.Empty,
            QueryString = context.Request.QueryString.Value ?? string.Empty,
            StatusCode = context.Response.StatusCode,
            CorrelationId = context.TraceIdentifier
        });

        await dbContext.SaveChangesAsync(context.RequestAborted);
    }
}
