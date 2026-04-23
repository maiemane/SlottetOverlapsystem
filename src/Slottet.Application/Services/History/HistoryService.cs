using Slottet.Application.DTOs.History;
using Slottet.Application.Interfaces;

namespace Slottet.Application.Services.History;

public sealed class HistoryService : IHistoryService
{
    private const int DefaultTake = 100;
    private const int MaxTake = 500;

    private readonly IHistoryRepository _historyRepository;

    public HistoryService(IHistoryRepository historyRepository)
    {
        _historyRepository = historyRepository;
    }

    public async Task<IReadOnlyList<HistoryEventDto>> GetHistoryAsync(
        DateTime fromUtc,
        DateTime toUtc,
        int? citizenId,
        string? eventType,
        int take,
        CancellationToken cancellationToken = default)
    {
        var resolvedTake = ResolveTake(take);

        bool includeMedication = string.IsNullOrWhiteSpace(eventType) || eventType == "Medication";
        bool includeSpecialEvent = string.IsNullOrWhiteSpace(eventType) || eventType == "SpecialEvent";

        // Intermediate list keeps CitizenId so we can resolve names in one query
        var pending = new List<(int CitizenId, HistoryEventDto Dto)>();

        if (includeMedication)
        {
            var regs = await _historyRepository.GetMedicinRegistrationsAsync(
                fromUtc, toUtc, citizenId, resolvedTake, cancellationToken);

            foreach (var reg in regs)
            {
                var dto = new HistoryEventDto
                {
                    Id = reg.Id,
                    OccurredAtUtc = reg.RegistrationTime,
                    EventType = "Medication",
                    Description = string.IsNullOrWhiteSpace(reg.Description)
                                        ? reg.Name
                                        : $"{reg.Name} – {reg.Description}"
                };

                pending.Add((reg.CitizenId, dto));
            }
        }

        if (includeSpecialEvent)
        {
            var events = await _historyRepository.GetSpecialEventsAsync(
                fromUtc, toUtc, citizenId, resolvedTake, cancellationToken);

            foreach (var evt in events)
            {
                var dto = new HistoryEventDto
                {
                    Id = evt.Id,
                    OccurredAtUtc = evt.EventTime,
                    EventType = "SpecialEvent",
                    Description = evt.Description
                };

                pending.Add((evt.CitizenId, dto));
            }
        }

        // Resolve all citizen names in a single query
        var allCitizenIds = pending.Select(p => p.CitizenId).Distinct();
        var citizens = await _historyRepository.GetCitizensByIdsAsync(allCitizenIds, cancellationToken);
        var citizenNameMap = citizens.ToDictionary(c => c.Id, c => c.Name);

        foreach (var (cid, dto) in pending)
        {
            dto.CitizenName = citizenNameMap.TryGetValue(cid, out var name) ? name : string.Empty;
        }

        return pending
            .Select(p => p.Dto)
            .OrderByDescending(d => d.OccurredAtUtc)
            .Take(resolvedTake)
            .ToList();
    }

    private static int ResolveTake(int take)
    {
        if (take <= 0)
        {
            return DefaultTake;
        }

        return Math.Min(take, MaxTake);
    }
}