using Slottet.Domain.Enums;

namespace Slottet.Application.DTOs.Citizens;

public sealed class CitizenPersonalDataExportDto
{
    public DateTime ExportedAtUtc { get; set; }
    public CitizenPersonalDataDto Citizen { get; set; } = new();
    public List<CitizenFixedMedicationPersonalDataDto> FixedMedications { get; set; } = [];
    public List<CitizenMedicationRegistrationPersonalDataDto> MedicationRegistrations { get; set; } = [];
    public List<CitizenSpecialEventPersonalDataDto> SpecialEvents { get; set; } = [];
}

public sealed class CitizenPersonalDataDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ApartmentNumber { get; set; } = string.Empty;
    public TrafficLight TrafficLight { get; set; }
    public int DepartmentId { get; set; }
    public bool IsActive { get; set; }
}

public sealed class CitizenFixedMedicationPersonalDataDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TimeOnly ScheduledTime { get; set; }
    public ShiftType ShiftType { get; set; }
    public bool IsActive { get; set; }
}

public sealed class CitizenMedicationRegistrationPersonalDataDto
{
    public int Id { get; set; }
    public int ShiftId { get; set; }
    public int? CitizenFixedMedicationId { get; set; }
    public MedicinType MedicinType { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime ScheduledTime { get; set; }
    public DateTime RegistrationTime { get; set; }
}

public sealed class CitizenSpecialEventPersonalDataDto
{
    public int Id { get; set; }
    public int ShiftId { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime EventTime { get; set; }
}
