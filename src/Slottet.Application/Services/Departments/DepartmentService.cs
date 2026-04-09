using Slottet.Application.DTOs.Departments;
using Slottet.Application.Interfaces;
using Slottet.Domain.Entities;

namespace Slottet.Application.Services.Departments;

public sealed class DepartmentService : IDepartmentService
{
    private readonly IDepartmentRepository _departmentRepository;

    public DepartmentService(IDepartmentRepository departmentRepository)
    {
        _departmentRepository = departmentRepository;
    }

    public async Task<IReadOnlyList<DepartmentDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var departments = await _departmentRepository.GetAllAsync(cancellationToken);

        return departments
            .OrderBy(department => department.Name)
            .Select(MapDto)
            .ToList();
    }

    public async Task<DepartmentDto?> GetByIdAsync(int departmentId, CancellationToken cancellationToken = default)
    {
        if (departmentId <= 0)
        {
            return null;
        }

        var department = await _departmentRepository.GetByIdAsync(departmentId, cancellationToken);
        return department is null ? null : MapDto(department);
    }

    public async Task<UpdateDepartmentMessageResult> UpdateMessageAsync(int departmentId, UpdateDepartmentMessageRequest request, CancellationToken cancellationToken = default)
    {
        if (departmentId <= 0)
        {
            return new UpdateDepartmentMessageResult
            {
                IsSuccess = false,
                Error = "NotFound"
            };
        }

        var department = await _departmentRepository.GetByIdAsync(departmentId, cancellationToken);

        if (department is null)
        {
            return new UpdateDepartmentMessageResult
            {
                IsSuccess = false,
                Error = "NotFound"
            };
        }

        var message = request.Message.Trim();

        if (message.Length > 2000)
        {
            return new UpdateDepartmentMessageResult
            {
                IsSuccess = false,
                Error = "MessageTooLong"
            };
        }

        department.Message = message;

        var updatedDepartment = await _departmentRepository.UpdateAsync(department, cancellationToken);

        return new UpdateDepartmentMessageResult
        {
            IsSuccess = true,
            Department = MapDto(updatedDepartment)
        };
    }

    private static DepartmentDto MapDto(Department department)
    {
        return new DepartmentDto
        {
            Id = department.Id,
            Name = department.Name,
            Message = department.Message
        };
    }
}
