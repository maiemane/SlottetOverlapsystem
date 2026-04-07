using Slottet.Application.DTOs;
using Slottet.Application.Interfaces;
using Slottet.Domain.Enums;

namespace Slottet.Infrastructure.Repositories;

public class FakeOverlapOverviewRepository : IOverlapOverviewRepository
{
    private readonly ICitizenRepository _citizenRepository;
    private readonly IDepartmentRepository _departmentRepository;

    public FakeOverlapOverviewRepository(
        ICitizenRepository citizenRepository,
        IDepartmentRepository departmentRepository)
    {
        _citizenRepository = citizenRepository;
        _departmentRepository = departmentRepository;
    }

    public async Task<OverlapOverviewDto?> GetByDepartmentAndShiftAsync(int departmentId, ShiftType shiftType)
    {
        var department = await _departmentRepository.GetByIdAsync(departmentId);

        if (department is null)
        {
            return null;
        }

        var citizens = (await _citizenRepository.GetAllAsync())
            .Where(c => c.DepartmentId == departmentId && c.IsActive)
            .ToList();

        var citizenOverviewItems = citizens
            .Select(c => new CitizenOverviewItemDto
            {
                CitizenId = c.Id,
                CitizenName = c.Name,
                TrafficLightStatus = GetTrafficLightStatus(c.Id, shiftType),
                MedicationStatus = GetMedicationStatus(c.Id, shiftType),
                AssignedEmployees = GetAssignedEmployees(c.Id, shiftType),
                SpecialIncidentSummary = GetSpecialIncidentSummary(c.Id, shiftType)
            })
            .ToList();

        var sharedTasks = GetSharedTasks(departmentId, shiftType);

        return new OverlapOverviewDto
        {
            DepartmentId = department.Id,
            DepartmentName = department.Name,
            ShiftType = shiftType,
            Citizens = citizenOverviewItems,
            SharedTasks = sharedTasks
        };
    }

    private static string GetTrafficLightStatus(int citizenId, ShiftType shiftType)
    {
        return (citizenId, shiftType) switch
        {
            (1, ShiftType.Dagvagt) => "Grøn",
            (2, ShiftType.Dagvagt) => "Gul",
            (3, ShiftType.Dagvagt) => "Rød",
            (1, ShiftType.Aftenvagt) => "Gul",
            (2, ShiftType.Aftenvagt) => "Grøn",
            (3, ShiftType.Aftenvagt) => "Gul",
            (1, ShiftType.Nattevagt) => "Grøn",
            (2, ShiftType.Nattevagt) => "Grøn",
            (3, ShiftType.Nattevagt) => "Gul",
            _ => "Grøn"
        };
    }

    private static string GetMedicationStatus(int citizenId, ShiftType shiftType)
    {
        return (citizenId, shiftType) switch
        {
            (1, ShiftType.Dagvagt) => "Morgenmedicin givet kl. 08:00",
            (2, ShiftType.Dagvagt) => "Ikke givet endnu",
            (3, ShiftType.Dagvagt) => "Morgenmedicin givet kl. 08:15",
            (1, ShiftType.Aftenvagt) => "Aftenmedicin givet kl. 18:00",
            (2, ShiftType.Aftenvagt) => "Aftenmedicin givet kl. 18:10",
            (3, ShiftType.Aftenvagt) => "PN-medicin givet kl. 19:30",
            (1, ShiftType.Nattevagt) => "Ingen medicin denne vagt",
            (2, ShiftType.Nattevagt) => "Natmedicin givet kl. 22:00",
            (3, ShiftType.Nattevagt) => "Observation af behov for PN",
            _ => "Ukendt"
        };
    }

    private static List<string> GetAssignedEmployees(int citizenId, ShiftType shiftType)
    {
        return (citizenId, shiftType) switch
        {
            (1, ShiftType.Dagvagt) => new List<string> { "Hanne", "Ulla" },
            (2, ShiftType.Dagvagt) => new List<string> { "Mads" },
            (3, ShiftType.Dagvagt) => new List<string> { "Lene" },

            (1, ShiftType.Aftenvagt) => new List<string> { "Ulla" },
            (2, ShiftType.Aftenvagt) => new List<string> { "Hanne", "Mads" },
            (3, ShiftType.Aftenvagt) => new List<string> { "Lene" },

            (1, ShiftType.Nattevagt) => new List<string> { "Nattevagt 1" },
            (2, ShiftType.Nattevagt) => new List<string> { "Nattevagt 2" },
            (3, ShiftType.Nattevagt) => new List<string> { "Nattevagt 1" },

            _ => new List<string>()
        };
    }

    private static string GetSpecialIncidentSummary(int citizenId, ShiftType shiftType)
    {
        return (citizenId, shiftType) switch
        {
            (2, ShiftType.Dagvagt) => "Borger virker urolig efter morgenmad.",
            (3, ShiftType.Aftenvagt) => "Højlydt konflikt med medborger.",
            (2, ShiftType.Nattevagt) => "Sov uroligt mellem kl. 02 og 03.",
            _ => string.Empty
        };
    }

    private static List<SharedTaskDto> GetSharedTasks(int departmentId, ShiftType shiftType)
    {
        return (departmentId, shiftType) switch
        {
            (1, ShiftType.Dagvagt) => new List<SharedTaskDto>
            {
                new() { Description = "Husk at tørre bord af i fællesrum." },
                new() { Description = "FMK skal tjekkes inden vagtskifte." }
            },
            (1, ShiftType.Aftenvagt) => new List<SharedTaskDto>
            {
                new() { Description = "Kaffe skal gøres klar til næste hold." }
            },
            (2, ShiftType.Dagvagt) => new List<SharedTaskDto>
            {
                new() { Description = "Omsorgsperson skal følge op på borger 2." }
            },
            _ => new List<SharedTaskDto>()
        };
    }
}