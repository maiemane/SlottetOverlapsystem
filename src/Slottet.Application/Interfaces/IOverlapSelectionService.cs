using Slottet.Application.DTOs;

namespace Slottet.Application.Interfaces;

public interface IOverlapSelectionService
{
    Task<IEnumerable<ShiftOptionDto>?> GetAvailableShiftsAsync(int departmentId);
    Task<SelectOverlapResponseDto?> SelectOverlapAsync(SelectOverlapRequestDto request);
}