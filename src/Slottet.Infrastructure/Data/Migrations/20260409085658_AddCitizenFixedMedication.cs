using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Slottet.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCitizenFixedMedication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CitizenFixedMedicationId",
                table: "MedicinRegistration",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ApartmentNumber",
                table: "Citizen",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateTable(
                name: "CitizenFixedMedication",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CitizenId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ScheduledTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    ShiftType = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CitizenFixedMedication", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CitizenFixedMedication_Citizen_CitizenId",
                        column: x => x.CitizenId,
                        principalTable: "Citizen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MedicinRegistration_CitizenFixedMedicationId",
                table: "MedicinRegistration",
                column: "CitizenFixedMedicationId");

            migrationBuilder.CreateIndex(
                name: "IX_CitizenFixedMedication_CitizenId_ShiftType",
                table: "CitizenFixedMedication",
                columns: new[] { "CitizenId", "ShiftType" });

            migrationBuilder.AddForeignKey(
                name: "FK_MedicinRegistration_CitizenFixedMedication_CitizenFixedMedicationId",
                table: "MedicinRegistration",
                column: "CitizenFixedMedicationId",
                principalTable: "CitizenFixedMedication",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MedicinRegistration_CitizenFixedMedication_CitizenFixedMedicationId",
                table: "MedicinRegistration");

            migrationBuilder.DropTable(
                name: "CitizenFixedMedication");

            migrationBuilder.DropIndex(
                name: "IX_MedicinRegistration_CitizenFixedMedicationId",
                table: "MedicinRegistration");

            migrationBuilder.DropColumn(
                name: "CitizenFixedMedicationId",
                table: "MedicinRegistration");

            migrationBuilder.AlterColumn<string>(
                name: "ApartmentNumber",
                table: "Citizen",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);
        }
    }
}
