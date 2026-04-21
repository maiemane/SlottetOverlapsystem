using Microsoft.EntityFrameworkCore;
using Slottet.Application.DTOs.History;
using Slottet.Application.Interfaces;
using Slottet.Infrastructure.Data;

namespace Slottet.Application.Services;

/// <summary>
/// Aggregates history events from MedicationRegistrations and SpecialEvents.
/// Follows the same repository-less, DbContext-injected pattern used elsewhere
/// in the infrastructure layer (e.g. AuditLogService).
/// </summary>
public sealed class HistoryService : IHistoryService
{
    private readonly ApplicationDbContext _db;

    public HistoryService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<HistoryEventDto>> GetHistoryAsync(
        DateTime fromUtc,
        DateTime toUtc,
        int? citizenId,
        string? eventType,
        int take)
    {
        var results = new List<HistoryEventDto>();

        bool includeMedication = string.IsNullOrWhiteSpace(eventType) || eventType == "Medication";
        bool includeSpecialEvent = string.IsNullOrWhiteSpace(eventType) || eventType == "SpecialEvent";

        // --- Medication registrations ---
        if (includeMedication)
        {
            var medQuery = _db.MedicationRegistrations
                .AsNoTracking()
                .Include(r => r.Citizen)
                .Where(r => r.RegisteredAtUtc >= fromUtc && r.RegisteredAtUtc <= toUtc);

            if (citizenId.HasValue)
            {
                medQuery = medQuery.Where(r => r.CitizenId == citizenId.Value);
            }

            var meds = await medQuery
                .OrderByDescending(r => r.RegisteredAtUtc)
                .Take(take)
                .Select(r => new HistoryEventDto
                {
                    Id = r.Id,
                    OccurredAtUtc = r.RegisteredAtUtc,
                    CitizenName = r.Citizen.Name,
                    EventType = "Medication",
                    Description = r.Notes ?? string.Empty,
                    EmployeeId = r.EmployeeId
                })
                .ToListAsync();

            results.AddRange(meds);
        }

        // --- Special events ---
        if (includeSpecialEvent)
        {
            var evtQuery = _db.SpecialEvents
                .AsNoTracking()
                .Include(e => e.Citizen)
                .Where(e => e.OccurredAtUtc >= fromUtc && e.OccurredAtUtc <= toUtc);

            if (citizenId.HasValue)
            {
                evtQuery = evtQuery.Where(e => e.CitizenId == citizenId.Value);
            }

            var evts = await evtQuery
                .OrderByDescending(e => e.OccurredAtUtc)
                .Take(take)
                .Select(e => new HistoryEventDto
                {
                    Id = e.Id,
                    OccurredAtUtc = e.OccurredAtUtc,
                    CitizenName = e.Citizen.Name,
                    EventType = "SpecialEvent",
                    Description = e.Description,
                    EmployeeId = e.EmployeeId
                })
                .ToListAsync();

            results.AddRange(evts);
        }

        // Merge, sort and cap at requested take
        return results
            .OrderByDescending(r => r.OccurredAtUtc)
            .Take(take)
            .ToList();
    }
}