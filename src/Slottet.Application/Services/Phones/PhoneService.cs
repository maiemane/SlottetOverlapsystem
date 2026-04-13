using Slottet.Application.DTOs.Phones;
using Slottet.Application.Interfaces;
using Slottet.Domain.Entities;

namespace Slottet.Application.Services.Phones;

public sealed class PhoneService : IPhoneService
{
    private readonly IPhoneRepository _phoneRepository;

    public PhoneService(IPhoneRepository phoneRepository)
    {
        _phoneRepository = phoneRepository;
    }

    public async Task<IReadOnlyList<PhoneDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var phones = await _phoneRepository.GetPhonesAsync(cancellationToken);
        return phones
            .OrderBy(phone => phone.NameOrNumber)
            .Select(MapPhone)
            .ToList();
    }

    public async Task<PhoneDto?> GetByIdAsync(int phoneId, CancellationToken cancellationToken = default)
    {
        if (phoneId <= 0)
        {
            return null;
        }

        var phone = await _phoneRepository.GetPhoneByIdAsync(phoneId, cancellationToken);
        return phone is null ? null : MapPhone(phone);
    }

    public async Task<CreatePhoneResult> CreateAsync(CreatePhoneRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedNameOrNumber = NormalizeNameOrNumber(request.NameOrNumber);

        if (normalizedNameOrNumber is null)
        {
            return CreateFailure("InvalidRequest");
        }

        var existingPhone = await _phoneRepository.GetPhoneByNameOrNumberAsync(normalizedNameOrNumber, cancellationToken);

        if (existingPhone is not null)
        {
            return CreateFailure("PhoneAlreadyExists");
        }

        var createdPhone = await _phoneRepository.AddPhoneAsync(new Phone
        {
            NameOrNumber = normalizedNameOrNumber,
            IsActive = request.IsActive
        }, cancellationToken);

        return new CreatePhoneResult
        {
            IsSuccess = true,
            Phone = MapPhone(createdPhone)
        };
    }

    public async Task<UpdatePhoneResult> UpdateAsync(int phoneId, UpdatePhoneRequest request, CancellationToken cancellationToken = default)
    {
        if (phoneId <= 0)
        {
            return UpdateFailure("PhoneNotFound");
        }

        var normalizedNameOrNumber = NormalizeNameOrNumber(request.NameOrNumber);

        if (normalizedNameOrNumber is null)
        {
            return UpdateFailure("InvalidRequest");
        }

        var phone = await _phoneRepository.GetPhoneByIdAsync(phoneId, cancellationToken);

        if (phone is null)
        {
            return UpdateFailure("PhoneNotFound");
        }

        var existingPhone = await _phoneRepository.GetPhoneByNameOrNumberAsync(normalizedNameOrNumber, cancellationToken);

        if (existingPhone is not null && existingPhone.Id != phoneId)
        {
            return UpdateFailure("PhoneAlreadyExists");
        }

        phone.NameOrNumber = normalizedNameOrNumber;
        phone.IsActive = request.IsActive;

        var updatedPhone = await _phoneRepository.UpdatePhoneAsync(phone, cancellationToken);

        return new UpdatePhoneResult
        {
            IsSuccess = true,
            Phone = MapPhone(updatedPhone)
        };
    }

    public async Task<DeletePhoneResult> DeleteAsync(int phoneId, CancellationToken cancellationToken = default)
    {
        if (phoneId <= 0)
        {
            return new DeletePhoneResult
            {
                IsSuccess = false,
                Error = "PhoneNotFound"
            };
        }

        var phone = await _phoneRepository.GetPhoneByIdAsync(phoneId, cancellationToken);

        if (phone is null)
        {
            return new DeletePhoneResult
            {
                IsSuccess = false,
                Error = "PhoneNotFound"
            };
        }

        if (await _phoneRepository.HasAllocationsAsync(phoneId, cancellationToken))
        {
            return new DeletePhoneResult
            {
                IsSuccess = false,
                Error = "PhoneInUse"
            };
        }

        await _phoneRepository.DeletePhoneAsync(phone, cancellationToken);

        return new DeletePhoneResult
        {
            IsSuccess = true
        };
    }

    private static string? NormalizeNameOrNumber(string value)
    {
        var normalized = value?.Trim();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }

    private static PhoneDto MapPhone(Phone phone)
    {
        return new PhoneDto
        {
            Id = phone.Id,
            NameOrNumber = phone.NameOrNumber,
            IsActive = phone.IsActive
        };
    }

    private static CreatePhoneResult CreateFailure(string error)
    {
        return new CreatePhoneResult
        {
            IsSuccess = false,
            Error = error
        };
    }

    private static UpdatePhoneResult UpdateFailure(string error)
    {
        return new UpdatePhoneResult
        {
            IsSuccess = false,
            Error = error
        };
    }
}
