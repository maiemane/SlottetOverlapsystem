using Slottet.Application.DTOs.Medications;
using Slottet.Application.Interfaces;
using Slottet.Application.Services.Medications;
using Slottet.Domain.Entities;
using Slottet.Domain.Enums;

namespace Slottet.Application.Tests;

public class MedicationRegistrationServiceTests
{
    [Fact]
    public async Task GetByCitizenAndShiftAsync_Returns_registrations_for_citizen_on_shift()
    {
        var repository = new FakeMedicationRegistrationRepository();
        var sut = new MedicationRegistrationService(repository);

        var result = await sut.GetByCitizenAndShiftAsync(10, 1, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Single(result!);
        Assert.Equal("Panodil", result[0].Name);
    }

    [Fact]
    public async Task CreateAsync_Creates_registration_for_fixed_medication()
    {
        var repository = new FakeMedicationRegistrationRepository();
        var sut = new MedicationRegistrationService(repository);

        var result = await sut.CreateAsync(10, 1, new CreateMedicationRegistrationRequest
        {
            MedicinType = MedicinType.Fast,
            CitizenFixedMedicationId = 50,
            RegistrationTime = new DateTime(2026, 4, 9, 8, 5, 0)
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Registration);
        Assert.Equal(MedicinType.Fast, result.Registration!.MedicinType);
        Assert.Equal(50, result.Registration.CitizenFixedMedicationId);
    }

    [Fact]
    public async Task CreateAsync_Creates_registration_for_pn_medication()
    {
        var repository = new FakeMedicationRegistrationRepository();
        var sut = new MedicationRegistrationService(repository);

        var result = await sut.CreateAsync(10, 1, new CreateMedicationRegistrationRequest
        {
            MedicinType = MedicinType.PN,
            Name = "Stesolid",
            Description = "Ved akut uro",
            RegistrationTime = new DateTime(2026, 4, 9, 11, 30, 0)
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Registration);
        Assert.Equal(MedicinType.PN, result.Registration!.MedicinType);
        Assert.Null(result.Registration.CitizenFixedMedicationId);
        Assert.Equal("Stesolid", result.Registration.Name);
    }

    [Fact]
    public async Task CreateAsync_Returns_failure_when_fixed_medication_is_already_registered()
    {
        var repository = new FakeMedicationRegistrationRepository
        {
            ExistingFixedMedicationRegistration = new MedicinRegistration
            {
                Id = 200,
                CitizenId = 1,
                ShiftId = 10,
                CitizenFixedMedicationId = 50,
                MedicinType = MedicinType.Fast,
                Name = "Panodil",
                Description = "Fast morgenmedicin",
                ScheduledTime = new DateTime(2026, 4, 9, 8, 0, 0),
                RegistrationTime = new DateTime(2026, 4, 9, 8, 5, 0)
            }
        };

        var sut = new MedicationRegistrationService(repository);

        var result = await sut.CreateAsync(10, 1, new CreateMedicationRegistrationRequest
        {
            MedicinType = MedicinType.Fast,
            CitizenFixedMedicationId = 50,
            RegistrationTime = new DateTime(2026, 4, 9, 8, 10, 0)
        }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("AlreadyRegistered", result.Error);
    }

    private sealed class FakeMedicationRegistrationRepository : IMedicationRegistrationRepository
    {
        public MedicinRegistration? ExistingFixedMedicationRegistration { get; set; }

        public Task<Shift?> GetShiftByIdAsync(int shiftId, CancellationToken cancellationToken = default)
        {
            Shift? shift = shiftId == 10
                ? new Shift
                {
                    Id = 10,
                    DepartmentId = 1,
                    Date = new DateTime(2026, 4, 9),
                    Type = ShiftType.Dagvagt
                }
                : null;

            return Task.FromResult(shift);
        }

        public Task<Citizen?> GetCitizenByIdAsync(int citizenId, CancellationToken cancellationToken = default)
        {
            Citizen? citizen = citizenId == 1
                ? new Citizen
                {
                    Id = 1,
                    Name = "Anna Jensen",
                    ApartmentNumber = "12A",
                    DepartmentId = 1,
                    TrafficLight = TrafficLight.Grøn,
                    IsActive = true
                }
                : null;

            return Task.FromResult(citizen);
        }

        public Task<CitizenFixedMedication?> GetFixedMedicationByIdAsync(int fixedMedicationId, CancellationToken cancellationToken = default)
        {
            CitizenFixedMedication? medication = fixedMedicationId == 50
                ? new CitizenFixedMedication
                {
                    Id = 50,
                    CitizenId = 1,
                    Name = "Panodil",
                    Description = "Fast morgenmedicin",
                    ScheduledTime = new TimeOnly(8, 0),
                    ShiftType = ShiftType.Dagvagt,
                    IsActive = true
                }
                : null;

            return Task.FromResult(medication);
        }

        public Task<IReadOnlyList<MedicinRegistration>> GetMedicationRegistrationsAsync(int shiftId, int citizenId, CancellationToken cancellationToken = default)
        {
            IReadOnlyList<MedicinRegistration> registrations =
            [
                new MedicinRegistration
                {
                    Id = 300,
                    CitizenId = 1,
                    ShiftId = 10,
                    CitizenFixedMedicationId = 50,
                    MedicinType = MedicinType.Fast,
                    Name = "Panodil",
                    Description = "Fast morgenmedicin",
                    ScheduledTime = new DateTime(2026, 4, 9, 8, 0, 0),
                    RegistrationTime = new DateTime(2026, 4, 9, 8, 5, 0)
                }
            ];

            return Task.FromResult(registrations);
        }

        public Task<MedicinRegistration?> GetFixedMedicationRegistrationAsync(int shiftId, int citizenFixedMedicationId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(ExistingFixedMedicationRegistration);
        }

        public Task<MedicinRegistration?> GetMedicationRegistrationByIdAsync(int medicationRegistrationId, CancellationToken cancellationToken = default)
        {
            MedicinRegistration? registration = medicationRegistrationId == ExistingFixedMedicationRegistration?.Id
                ? ExistingFixedMedicationRegistration
                : null;

            return Task.FromResult(registration);
        }

        public Task<MedicinRegistration> AddMedicationRegistrationAsync(MedicinRegistration registration, CancellationToken cancellationToken = default)
        {
            registration.Id = 300;
            return Task.FromResult(registration);
        }

        public Task DeleteMedicationRegistrationAsync(MedicinRegistration registration, CancellationToken cancellationToken = default)
        {
            if (ExistingFixedMedicationRegistration?.Id == registration.Id)
            {
                ExistingFixedMedicationRegistration = null;
            }

            return Task.CompletedTask;
        }
    }
}
