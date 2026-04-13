namespace Slottet.Infrastructure.Data;

public sealed class LogRetentionOptions
{
    public const string SectionName = "LogRetention";

    public bool Enabled { get; set; } = true;
    public int AuditLogRetentionDays { get; set; } = 180;
    public int AccessLogRetentionDays { get; set; } = 90;
}
