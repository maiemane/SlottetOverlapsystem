using Microsoft.EntityFrameworkCore;
using Slottet.Application.Interfaces;
using Slottet.Domain.Entities;
using Slottet.Infrastructure.Data;

namespace Slottet.Infrastructure.Repositories;

public sealed class CitizenCreationRepository : ICitizenCreationRepository
{
    private readonly ApplicationDbContext _dbContext;

    public CitizenCreationRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Citizen>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Citizens
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public Task<Citizen?> GetByIdAsync(int citizenId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Citizens
            .FirstOrDefaultAsync(citizen => citizen.Id == citizenId, cancellationToken);
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
            .FirstOrDefaultAsync(fixedMedication => fixedMedication.Id == fixedMedicationId, cancellationToken);
    }

    public async Task<IReadOnlyList<CitizenFixedMedication>> GetFixedMedicationsByCitizenIdAsync(int citizenId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.CitizenFixedMedications
            .AsNoTracking()
            .Where(fixedMedication => fixedMedication.CitizenId == citizenId)
            .ToListAsync(cancellationToken);
    }
    public Task<Department?> GetDepartmentByIdAsync(int departmentId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Departments
            .AsNoTracking()
            .FirstOrDefaultAsync(department => department.Id == departmentId, cancellationToken);
    }

    public async Task<Citizen> AddCitizenAsync(Citizen citizen, CancellationToken cancellationToken = default)
    {
        _dbContext.Citizens.Add(citizen);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return citizen;
    }

    public async Task<Citizen> UpdateCitizenAsync(Citizen citizen, CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
        return citizen;
    }

    public async Task<bool> DeleteCitizenAsync(Citizen citizen, CancellationToken cancellationToken = default)
    {
        _dbContext.Citizens.Remove(citizen);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException)
        {
            _dbContext.Entry(citizen).State = EntityState.Unchanged;
            return false;
        }
    }

    public async Task<CitizenFixedMedication> AddFixedMedicationAsync(CitizenFixedMedication fixedMedication, CancellationToken cancellationToken = default)
    {
        _dbContext.CitizenFixedMedications.Add(fixedMedication);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return fixedMedication;
    }

    public async Task<CitizenFixedMedication> UpdateFixedMedicationAsync(CitizenFixedMedication fixedMedication, CancellationToken cancellationToken = default)
    {
        _dbContext.CitizenFixedMedications.Update(fixedMedication);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return fixedMedication;
    }
}
