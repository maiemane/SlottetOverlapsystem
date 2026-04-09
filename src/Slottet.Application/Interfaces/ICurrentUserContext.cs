namespace Slottet.Application.Interfaces;

public interface ICurrentUserContext
{
    int? EmployeeId { get; }
    string RequestPath { get; }
    string CorrelationId { get; }
}
