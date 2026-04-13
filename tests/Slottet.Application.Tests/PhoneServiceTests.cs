using Slottet.Application.DTOs.Phones;
using Slottet.Application.Interfaces;
using Slottet.Application.Services.Phones;
using Slottet.Domain.Entities;

namespace Slottet.Application.Tests;

public class PhoneServiceTests
{
    [Fact]
    public async Task CreateAsync_CreatesPhone_WhenRequestIsValid()
    {
        var repository = new FakePhoneRepository();
        var sut = new PhoneService(repository);

        var result = await sut.CreateAsync(new CreatePhoneRequest
        {
            NameOrNumber = " Telefon 2 ",
            IsActive = true
        });

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Phone);
        Assert.Equal(2, result.Phone!.Id);
        Assert.Equal("Telefon 2", result.Phone.NameOrNumber);
    }

    [Fact]
    public async Task CreateAsync_ReturnsError_WhenPhoneAlreadyExists()
    {
        var repository = new FakePhoneRepository();
        var sut = new PhoneService(repository);

        var result = await sut.CreateAsync(new CreatePhoneRequest
        {
            NameOrNumber = "Telefon 1"
        });

        Assert.False(result.IsSuccess);
        Assert.Equal("PhoneAlreadyExists", result.Error);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsError_WhenPhoneIsInUse()
    {
        var repository = new FakePhoneRepository();
        var sut = new PhoneService(repository);

        var result = await sut.DeleteAsync(1);

        Assert.False(result.IsSuccess);
        Assert.Equal("PhoneInUse", result.Error);
    }

    private sealed class FakePhoneRepository : IPhoneRepository
    {
        public List<Phone> Phones { get; } =
        [
            new Phone
            {
                Id = 1,
                NameOrNumber = "Telefon 1",
                IsActive = true
            }
        ];

        public Task<IReadOnlyList<Phone>> GetPhonesAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<Phone>>(Phones);
        }

        public Task<Phone?> GetPhoneByIdAsync(int phoneId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Phones.FirstOrDefault(phone => phone.Id == phoneId));
        }

        public Task<Phone?> GetPhoneByNameOrNumberAsync(string nameOrNumber, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Phones.FirstOrDefault(phone => string.Equals(phone.NameOrNumber, nameOrNumber, StringComparison.OrdinalIgnoreCase)));
        }

        public Task<bool> HasAllocationsAsync(int phoneId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(phoneId == 1);
        }

        public Task<Phone> AddPhoneAsync(Phone phone, CancellationToken cancellationToken = default)
        {
            phone.Id = Phones.Max(existingPhone => existingPhone.Id) + 1;
            Phones.Add(phone);
            return Task.FromResult(phone);
        }

        public Task<Phone> UpdatePhoneAsync(Phone phone, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(phone);
        }

        public Task DeletePhoneAsync(Phone phone, CancellationToken cancellationToken = default)
        {
            Phones.Remove(phone);
            return Task.CompletedTask;
        }
    }
}
