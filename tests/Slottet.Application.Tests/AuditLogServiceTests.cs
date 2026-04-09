using Slottet.Application.Interfaces;
using Slottet.Application.Services.Audit;
using Slottet.Domain.Entities;

namespace Slottet.Application.Tests;

public class AuditLogServiceTests
{
    [Fact]
    public async Task GetAuditLogsAsync_Returns_logs_sorted_descending_and_caps_take()
    {
        var repository = new FakeAuditLogRepository();
        var sut = new AuditLogService(repository);

        var result = await sut.GetAuditLogsAsync(1000, CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Equal(2, result[0].Id);
        Assert.Equal(500, repository.LastTake);
    }

    [Fact]
    public async Task GetAccessLogsAsync_Uses_default_take_when_invalid_value_is_passed()
    {
        var repository = new FakeAuditLogRepository();
        var sut = new AuditLogService(repository);

        var result = await sut.GetAccessLogsAsync(0, CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Equal(100, repository.LastTake);
        Assert.Equal(11, result[0].Id);
    }

    private sealed class FakeAuditLogRepository : IAuditLogRepository
    {
        public int LastTake { get; private set; }

        public Task<IReadOnlyList<AuditLog>> GetAuditLogsAsync(int take, CancellationToken cancellationToken = default)
        {
            LastTake = take;

            IReadOnlyList<AuditLog> logs =
            [
                new AuditLog
                {
                    Id = 1,
                    OccurredAtUtc = new DateTime(2026, 4, 9, 8, 0, 0, DateTimeKind.Utc),
                    Action = "Create",
                    EntityName = "Citizen",
                    EntityId = "10"
                },
                new AuditLog
                {
                    Id = 2,
                    OccurredAtUtc = new DateTime(2026, 4, 9, 9, 0, 0, DateTimeKind.Utc),
                    Action = "Update",
                    EntityName = "Citizen",
                    EntityId = "10"
                }
            ];

            return Task.FromResult(logs);
        }

        public Task<IReadOnlyList<AccessLog>> GetAccessLogsAsync(int take, CancellationToken cancellationToken = default)
        {
            LastTake = take;

            IReadOnlyList<AccessLog> logs =
            [
                new AccessLog
                {
                    Id = 10,
                    OccurredAtUtc = new DateTime(2026, 4, 9, 8, 0, 0, DateTimeKind.Utc),
                    HttpMethod = "GET",
                    RequestPath = "/api/overlap"
                },
                new AccessLog
                {
                    Id = 11,
                    OccurredAtUtc = new DateTime(2026, 4, 9, 9, 0, 0, DateTimeKind.Utc),
                    HttpMethod = "POST",
                    RequestPath = "/api/citizens"
                }
            ];

            return Task.FromResult(logs);
        }
    }
}
