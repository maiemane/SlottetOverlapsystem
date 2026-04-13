using Microsoft.EntityFrameworkCore;
using Slottet.Application.Interfaces;
using Slottet.Domain.Entities;
using Slottet.Infrastructure.Data;

namespace Slottet.Infrastructure.Repositories;

public sealed class PhoneRepository : IPhoneRepository
{
    private readonly ApplicationDbContext _dbContext;

    public PhoneRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Phone>> GetPhonesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Phones
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public Task<Phone?> GetPhoneByIdAsync(int phoneId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Phones
            .FirstOrDefaultAsync(phone => phone.Id == phoneId, cancellationToken);
    }

    public Task<Phone?> GetPhoneByNameOrNumberAsync(string nameOrNumber, CancellationToken cancellationToken = default)
    {
        return _dbContext.Phones
            .FirstOrDefaultAsync(phone => phone.NameOrNumber == nameOrNumber, cancellationToken);
    }

    public Task<bool> HasAllocationsAsync(int phoneId, CancellationToken cancellationToken = default)
    {
        return _dbContext.PhoneAllocations
            .AnyAsync(allocation => allocation.PhoneId == phoneId, cancellationToken);
    }

    public async Task<Phone> AddPhoneAsync(Phone phone, CancellationToken cancellationToken = default)
    {
        _dbContext.Phones.Add(phone);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return phone;
    }

    public async Task<Phone> UpdatePhoneAsync(Phone phone, CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
        return phone;
    }

    public async Task DeletePhoneAsync(Phone phone, CancellationToken cancellationToken = default)
    {
        _dbContext.Phones.Remove(phone);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
