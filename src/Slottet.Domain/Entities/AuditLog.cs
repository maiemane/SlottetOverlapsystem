namespace Slottet.Domain.Entities;

public class AuditLog
{
    public int Id { get; set; }
    public DateTime OccurredAtUtc { get; set; }
    public int? EmployeeId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string OldValuesJson { get; set; } = string.Empty;
    public string NewValuesJson { get; set; } = string.Empty;
    public string RequestPath { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = string.Empty;
}
