using Slottet.Application.Interfaces;
using Slottet.Application.Services.Overlap;
using Slottet.Domain.Entities;
using Slottet.Domain.Enums;

namespace Slottet.Application.Tests;

public class OverlapOverviewServiceTests
{
    [Fact]
    public async Task GetCitizenOverviewAsync_Returns_citizen_overview_for_department_and_shift()
    {
        var repository = new FakeOverlapOverviewRepository();
        var sut = new OverlapOverviewService(repository);

        var result = await sut.GetCitizenOverviewAsync(1, 10, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(1, result!.DepartmentId);
        Assert.Equal("Slottet", result.DepartmentName);
        Assert.Equal(10, result.ShiftId);
        Assert.Equal(ShiftType.Dagvagt, result.ShiftType);
        Assert.Equal(2, result.Citizens.Count);

        var anna = Assert.Single(result.Citizens, citizen => citizen.CitizenId == 1);
        Assert.Equal("Anna Jensen", anna.CitizenName);
        Assert.Equal(TrafficLight.Grøn, anna.TrafficLight);
        Assert.Single(anna.Medications);
        Assert.Single(anna.SpecialEvents);
        Assert.Equal([5], anna.AssignedEmployeeIds);

        var peter = Assert.Single(result.Citizens, citizen => citizen.CitizenId == 2);
        Assert.Equal("Peter Hansen", peter.CitizenName);
        Assert.Equal(TrafficLight.Gul, peter.TrafficLight);
        Assert.Empty(peter.Medications);
        Assert.Empty(peter.SpecialEvents);
        Assert.Equal([6], peter.AssignedEmployeeIds);
    }

    [Fact]
    public async Task GetCitizenOverviewAsync_Returns_null_when_shift_does_not_belong_to_department()
    {
        var repository = new FakeOverlapOverviewRepository();
        var sut = new OverlapOverviewService(repository);

        var result = await sut.GetCitizenOverviewAsync(2, 10, CancellationToken.None);

        Assert.Null(result);
    }

    private sealed class FakeOverlapOverviewRepository : IOverlapOverviewRepository
    {
        public Task<Department?> GetDepartmentByIdAsync(int departmentId, CancellationToken cancellationToken = default)
        {
            Department? department = departmentId switch
            {
                1 => new Department { Id = 1, Name = "Slottet" },
                2 => new Department { Id = 2, Name = "Skoven" },
                _ => null
            };

            return Task.FromResult(department);
        }

        public Task<Shift?> GetShiftByIdAsync(int shiftId, CancellationToken cancellationToken = default)
        {
            Shift? shift = shiftId switch
            {
                10 => new Shift
                {
                    Id = 10,
                    DepartmentId = 1,
                    Date = new DateTime(2026, 4, 8),
                    Type = ShiftType.Dagvagt
                },
                _ => null
            };

            return Task.FromResult(shift);
        }

        public Task<IReadOnlyList<Citizen>> GetActiveCitizensByDepartmentAsync(int departmentId, CancellationToken cancellationToken = default)
        {
            IReadOnlyList<Citizen> citizens =
            [
                new Citizen { Id = 1, Name = "Anna Jensen", DepartmentId = 1, TrafficLight = TrafficLight.Grøn, IsActive = true },
                new Citizen { Id = 2, Name = "Peter Hansen", DepartmentId = 1, TrafficLight = TrafficLight.Gul, IsActive = true }
            ];

            return Task.FromResult(citizens);
        }

        public Task<IReadOnlyList<MedicinRegistration>> GetMedicationsByShiftAsync(int shiftId, CancellationToken cancellationToken = default)
        {
            IReadOnlyList<MedicinRegistration> medications =
            [
                new MedicinRegistration
                {
                    Id = 100,
                    CitizenId = 1,
                    ShiftId = 10,
                    MedicinType = MedicinType.Fast,
                    Name = "Paracetamol",
                    Description = "Givet efter morgenmad",
                    ScheduledTime = new DateTime(2026, 4, 8, 8, 0, 0),
                    RegistrationTime = new DateTime(2026, 4, 8, 8, 5, 0)
                }
            ];

            return Task.FromResult(medications);
        }

        public Task<IReadOnlyList<SpecialEvent>> GetSpecialEventsByShiftAsync(int shiftId, CancellationToken cancellationToken = default)
        {
            IReadOnlyList<SpecialEvent> specialEvents =
            [
                new SpecialEvent
                {
                    Id = 200,
                    CitizenId = 1,
                    ShiftId = 10,
                    Description = "Urolig formiddag",
                    EventTime = new DateTime(2026, 4, 8, 10, 30, 0)
                }
            ];

            return Task.FromResult(specialEvents);
        }

        public Task<IReadOnlyList<CitizenAssignment>> GetCitizenAssignmentsByShiftAsync(int shiftId, CancellationToken cancellationToken = default)
        {
            IReadOnlyList<CitizenAssignment> assignments =
            [
                new CitizenAssignment { Id = 300, CitizenId = 1, EmployeeId = 5, ShiftId = 10 },
                new CitizenAssignment { Id = 301, CitizenId = 2, EmployeeId = 6, ShiftId = 10 }
            ];

            return Task.FromResult(assignments);
        }
    }
}
