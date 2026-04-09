using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Slottet.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddShiftDefinitions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ShiftDefinition",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ShiftType = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShiftDefinition", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShiftDefinition_ShiftType",
                table: "ShiftDefinition",
                column: "ShiftType",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShiftDefinition");
        }
    }
}
