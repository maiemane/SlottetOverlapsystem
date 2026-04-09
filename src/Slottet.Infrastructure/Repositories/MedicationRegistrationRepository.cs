using Microsoft.EntityFrameworkCore;
using Slottet.Application.Interfaces;
using Slottet.Domain.Entities;
using Slottet.Infrastructure.Data;

namespace Slottet.Infrastructure.Repositories;

public sealed class MedicationRegistrationRepository : IMedicationRegistrationRepository
{
    private readonly ApplicationDbContext _dbContext;

    public MedicationRegistrationRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Shift?> GetShiftByIdAsync(int shiftId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Shifts
            .AsNoTracking()
            .FirstOrDefaultAsync(shift => shift.Id == shiftId, cancellationToken);
    }

    public Task<Citizen?> GetCitizenByIdAsync(int citizenId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Citizens
            .AsNoTracking()
            .FirstOrDefaultAsync(citizen => citizen.Id == citizenId, cancellationToken);
    }

    public Task<CitizenFixedMedication?> GetFixedMedicationByIdAsync(int fixedMedicationId, CancellationToken cancellationToken = default)
    {
        return _dbContext.CitizenFixedMedications
            .AsNoTracking()
            .FirstOrDefaultAsync(fixedMedication => fixedMedication.Id == fixedMedicationId, cancellationToken);
    }

    public async Task<IReadOnlyList<MedicinRegistration>> GetMedicationRegistrationsAsync(int shiftId, int citizenId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.MedicinRegistrations
            .AsNoTracking()
            .Where(registration => registration.ShiftId == shiftId && registration.CitizenId == citizenId)
            .ToListAsync(cancellationToken);
    }

    public Task<MedicinRegistration?> GetMedicationRegistrationByIdAsync(int medicationRegistrationId, CancellationToken cancellationToken = default)
    {
        return _dbContext.MedicinRegistrations
            .FirstOrDefaultAsync(registration => registration.Id == medicationRegistrationId, cancellationToken);
    }

    public Task<MedicinRegistration?> GetFixedMedicationRegistrationAsync(int shiftId, int citizenFixedMedicationId, CancellationToken cancellationToken = default)
    {
        return _dbContext.MedicinRegistrations
            .AsNoTracking()
            .FirstOrDefaultAsync(
                registration => registration.ShiftId == shiftId && registration.CitizenFixedMedicationId == citizenFixedMedicationId,
                cancellationToken);
    }

    public async Task<MedicinRegistration> AddMedicationRegistrationAsync(MedicinRegistration registration, CancellationToken cancellationToken = default)
    {
        _dbContext.MedicinRegistrations.Add(registration);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return registration;
    }

    public async Task DeleteMedicationRegistrationAsync(MedicinRegistration registration, CancellationToken cancellationToken = default)
    {
        _dbContext.MedicinRegistrations.Remove(registration);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
