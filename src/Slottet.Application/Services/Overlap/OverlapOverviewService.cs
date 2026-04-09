using Slottet.Application.DTOs.Overlap;
using Slottet.Application.Interfaces;
using Slottet.Domain.Enums;

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
        var citizenIds = citizens.Select(citizen => citizen.Id).ToList();
        var fixedMedications = await _overlapOverviewRepository.GetFixedMedicationsByCitizensAndShiftTypeAsync(citizenIds, shift.Type, cancellationToken);
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
                Medications = MapCitizenMedications(citizen.Id, shift.Date, fixedMedications, medications),
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

    private static List<CitizenMedicationDto> MapCitizenMedications(
        int citizenId,
        DateTime shiftDate,
        IReadOnlyList<Domain.Entities.CitizenFixedMedication> fixedMedications,
        IReadOnlyList<Domain.Entities.MedicinRegistration> registrations)
    {
        var citizenFixedMedications = fixedMedications
            .Where(medication => medication.CitizenId == citizenId)
            .ToList();

        var citizenRegistrations = registrations
            .Where(registration => registration.CitizenId == citizenId)
            .ToList();

        var medicationDtos = citizenFixedMedications
            .Select(fixedMedication =>
            {
                var matchedRegistration = citizenRegistrations
                    .Where(registration => registration.CitizenFixedMedicationId == fixedMedication.Id)
                    .OrderByDescending(registration => registration.RegistrationTime)
                    .FirstOrDefault();

                return new CitizenMedicationDto
                {
                    FixedMedicationId = fixedMedication.Id,
                    MedicationRegistrationId = matchedRegistration?.Id,
                    MedicinType = MedicinType.Fast,
                    Name = fixedMedication.Name,
                    Description = fixedMedication.Description,
                    ScheduledTime = shiftDate.Date.Add(fixedMedication.ScheduledTime.ToTimeSpan()),
                    RegistrationTime = matchedRegistration?.RegistrationTime,
                    IsRegistered = matchedRegistration is not null
                };
            })
            .ToList();

        var standaloneRegistrations = citizenRegistrations
            .Where(registration => registration.CitizenFixedMedicationId is null)
            .Select(registration => new CitizenMedicationDto
            {
                FixedMedicationId = null,
                MedicationRegistrationId = registration.Id,
                MedicinType = registration.MedicinType,
                Name = registration.Name,
                Description = registration.Description,
                ScheduledTime = registration.ScheduledTime,
                RegistrationTime = registration.RegistrationTime,
                IsRegistered = true
            });

        medicationDtos.AddRange(standaloneRegistrations);

        return medicationDtos
            .OrderBy(medication => medication.ScheduledTime)
            .ThenBy(medication => medication.Name)
            .ToList();
    }
}
