using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Slottet.Infrastructure.Data;

internal sealed class AuditEntry
{
    public required EntityEntry Entry { get; init; }
    public required string Action { get; init; }
    public required string EntityName { get; init; }
    public required Dictionary<string, object?> OldValues { get; init; }
    public required Dictionary<string, object?> NewValues { get; init; }
}
