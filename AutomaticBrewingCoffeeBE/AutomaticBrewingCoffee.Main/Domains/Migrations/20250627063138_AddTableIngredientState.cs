using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomaticBrewingCoffee.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddTableIngredientState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeviceIngredientStates",
                columns: table => new
                {
                    DeviceIngredientStateId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DeviceId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DeviceIngredientId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CurrentCapacity = table.Column<double>(type: "float", nullable: false),
                    CapacityLevel = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    IsWarning = table.Column<bool>(type: "bit", nullable: false),
                    LastRefilled = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceIngredientStates", x => x.DeviceIngredientStateId);
                    table.ForeignKey(
                        name: "FK_DeviceIngredientStates_DeviceIngredients_DeviceIngredientId",
                        column: x => x.DeviceIngredientId,
                        principalTable: "DeviceIngredients",
                        principalColumn: "DeviceIngredientId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DeviceIngredientStates_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "DeviceId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "SystemConfigs",
                keyColumn: "SystemConfigId",
                keyValue: "VAT",
                column: "CreatedDate",
                value: new DateTime(2025, 6, 27, 6, 31, 37, 653, DateTimeKind.Utc).AddTicks(1994));

            migrationBuilder.CreateIndex(
                name: "IX_DeviceIngredientStates_DeviceId",
                table: "DeviceIngredientStates",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceIngredientStates_DeviceIngredientId",
                table: "DeviceIngredientStates",
                column: "DeviceIngredientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeviceIngredientStates");

            migrationBuilder.UpdateData(
                table: "SystemConfigs",
                keyColumn: "SystemConfigId",
                keyValue: "VAT",
                column: "CreatedDate",
                value: new DateTime(2025, 6, 27, 6, 5, 23, 170, DateTimeKind.Utc).AddTicks(9832));
        }
    }
}
