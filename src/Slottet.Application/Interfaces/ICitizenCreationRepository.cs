using Slottet.Domain.Entities;

namespace Slottet.Application.Interfaces;

public interface ICitizenCreationRepository
{
    Task<Citizen?> GetCitizenByIdAsync(int citizenId, CancellationToken cancellationToken = default);
    Task<Department?> GetDepartmentByIdAsync(int departmentId, CancellationToken cancellationToken = default);
    Task<CitizenFixedMedication?> GetFixedMedicationByIdAsync(int fixedMedicationId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CitizenFixedMedication>> GetFixedMedicationsByCitizenIdAsync(int citizenId, CancellationToken cancellationToken = default);
    Task<Citizen> AddCitizenAsync(Citizen citizen, CancellationToken cancellationToken = default);
    Task<CitizenFixedMedication> AddFixedMedicationAsync(CitizenFixedMedication fixedMedication, CancellationToken cancellationToken = default);
    Task<CitizenFixedMedication> UpdateFixedMedicationAsync(CitizenFixedMedication fixedMedication, CancellationToken cancellationToken = default);
}
