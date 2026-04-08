using Slottet.Application.DTOs.Citizens;
using Slottet.Application.Interfaces;
using Slottet.Domain.Entities;
using Slottet.Domain.Enums;

namespace Slottet.Application.Services.Citizens;

public sealed class CreateCitizenService : ICreateCitizenService
{
    private readonly ICitizenCreationRepository _citizenCreationRepository;

    public CreateCitizenService(ICitizenCreationRepository citizenCreationRepository)
    {
        _citizenCreationRepository = citizenCreationRepository;
    }

    public async Task<CreateCitizenResult> CreateAsync(CreateCitizenRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name) ||
            string.IsNullOrWhiteSpace(request.ApartmentNumber) ||
            request.DepartmentId <= 0 ||
            !Enum.IsDefined(typeof(TrafficLight), request.TrafficLight))
        {
            return new CreateCitizenResult
            {
                IsSuccess = false,
                Error = "InvalidRequest"
            };
        }

        var department = await _citizenCreationRepository.GetDepartmentByIdAsync(request.DepartmentId, cancellationToken);

        if (department is null)
        {
            return new CreateCitizenResult
            {
                IsSuccess = false,
                Error = "DepartmentNotFound"
            };
        }

        var citizen = await _citizenCreationRepository.AddCitizenAsync(new Citizen
        {
            Name = request.Name.Trim(),
            ApartmentNumber = request.ApartmentNumber.Trim(),
            TrafficLight = request.TrafficLight,
            DepartmentId = request.DepartmentId,
            IsActive = true
        }, cancellationToken);

        return new CreateCitizenResult
        {
            IsSuccess = true,
            Citizen = new CreateCitizenResponse
            {
                CitizenId = citizen.Id,
                Name = citizen.Name,
                ApartmentNumber = citizen.ApartmentNumber,
                TrafficLight = citizen.TrafficLight,
                DepartmentId = citizen.DepartmentId,
                IsActive = citizen.IsActive
            }
        };
    }
}
