using Slottet.Application.DTOs.Overlap;
using Slottet.Application.Interfaces;

namespace Slottet.Application.Services.Overlap;

public sealed class OverlapOverviewService : IOverlapOverviewService
{
    private readonly IOverlapOverviewRepository _overlapOverviewRepository;

    public OverlapOverviewService(IOverlapOverviewRepository overlapOverviewRepository)
    {
        _overlapOverviewRepository = overlapOverviewRepository;
    }

    public async Task<CitizenOverlapOverviewDto?> GetCitizenOverviewAsync(int departmentId, int shiftId, CancellationToken cancellationToken = default)
    {
        if (departmentId <= 0 || shiftId <= 0)
        {
            return null;
        }

        var department = await _overlapOverviewRepository.GetDepartmentByIdAsync(departmentId, cancellationToken);
        var shift = await _overlapOverviewRepository.GetShiftByIdAsync(shiftId, cancellationToken);

        if (department is null || shift is null || shift.DepartmentId != departmentId)
        {
            return null;
        }

        var citizens = await _overlapOverviewRepository.GetActiveCitizensByDepartmentAsync(departmentId, cancellationToken);
        var medications = await _overlapOverviewRepository.GetMedicationsByShiftAsync(shiftId, cancellationToken);
        var specialEvents = await _overlapOverviewRepository.GetSpecialEventsByShiftAsync(shiftId, cancellationToken);
        var assignments = await _overlapOverviewRepository.GetCitizenAssignmentsByShiftAsync(shiftId, cancellationToken);

        var citizenItems = citizens
            .OrderBy(citizen => citizen.Name)
            .Select(citizen => new CitizenOverlapItemDto
            {
                CitizenId = citizen.Id,
                CitizenName = citizen.Name,
                TrafficLight = citizen.TrafficLight,
                ApartmentNumber = citizen.ApartmentNumber,
                Medications = medications
                    .Where(medication => medication.CitizenId == citizen.Id)
                    .OrderBy(medication => medication.ScheduledTime)
                    .Select(medication => new CitizenMedicationDto
                    {
                        Id = medication.Id,
                        MedicinType = medication.MedicinType,
                        Name = medication.Name,
                        Description = medication.Description,
                        ScheduledTime = medication.ScheduledTime,
                        RegistrationTime = medication.RegistrationTime
                    })
                    .ToList(),
                SpecialEvents = specialEvents
                    .Where(specialEvent => specialEvent.CitizenId == citizen.Id)
                    .OrderByDescending(specialEvent => specialEvent.EventTime)
                    .Select(specialEvent => new CitizenSpecialEventDto
                    {
                        Id = specialEvent.Id,
                        Description = specialEvent.Description,
                        EventTime = specialEvent.EventTime
                    })
                    .ToList(),
                AssignedEmployeeIds = assignments
                    .Where(assignment => assignment.CitizenId == citizen.Id)
                    .Select(assignment => assignment.EmployeeId)
                    .Distinct()
                    .OrderBy(employeeId => employeeId)
                    .ToList()
            })
            .ToList();

        return new CitizenOverlapOverviewDto
        {
            DepartmentId = department.Id,
            DepartmentName = department.Name,
            ShiftId = shift.Id,
            ShiftDate = shift.Date,
            ShiftType = shift.Type,
            Citizens = citizenItems
        };
    }
}
