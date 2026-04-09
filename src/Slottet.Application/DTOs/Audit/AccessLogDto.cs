namespace Slottet.Application.DTOs.Audit;

public sealed class AccessLogDto
{
    public int Id { get; set; }
    public DateTime OccurredAtUtc { get; set; }
    public int? EmployeeId { get; set; }
    public string HttpMethod { get; set; } = string.Empty;
    public string RequestPath { get; set; } = string.Empty;
    public string QueryString { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
}
