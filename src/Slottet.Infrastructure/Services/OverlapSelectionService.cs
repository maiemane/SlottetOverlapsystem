using Slottet.Application.DTOs;
using Slottet.Application.Interfaces;
using Slottet.Domain.Enums;

namespace Slottet.Infrastructure.Services;

public class OverlapSelectionService : IOverlapSelectionService
{
    private readonly IDepartmentRepository _departmentRepository;

    public OverlapSelectionService(IDepartmentRepository departmentRepository)
    {
        _departmentRepository = departmentRepository;
    }

    public async Task<IEnumerable<ShiftOptionDto>?> GetAvailableShiftsAsync(int departmentId)
    {
        var department = await _departmentRepository.GetByIdAsync(departmentId);

        if (department is null)
        {
            return null;
        }

        var shifts = Enum.GetValues<ShiftType>()
            .Select(shift => new ShiftOptionDto
            {
                Value = (int)shift,
                Name = shift.ToString()
            });

        return shifts;
    }

    public async Task<SelectOverlapResponseDto?> SelectOverlapAsync(SelectOverlapRequestDto request)
    {
        var department = await _departmentRepository.GetByIdAsync(request.DepartmentId);

        if (department is null)
        {
            return null;
        }

        if (!Enum.IsDefined(typeof(ShiftType), request.ShiftType))
        {
            return null;
        }

        return new SelectOverlapResponseDto
        {
            DepartmentId = department.Id,
            DepartmentName = department.Name,
            ShiftType = request.ShiftType,
            Message = $"Afdeling '{department.Name}' og vagt '{request.ShiftType}' er valgt."
        };
    }
}