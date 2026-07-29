using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomaticBrewingCoffee.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddUnitPropIntoDeviceIngredientState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Unit",
                table: "DeviceIngredientStates",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "SystemConfigs",
                keyColumn: "SystemConfigId",
                keyValue: "VAT",
                column: "CreatedDate",
                value: new DateTime(2025, 6, 29, 18, 47, 16, 646, DateTimeKind.Utc).AddTicks(3138));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Unit",
                table: "DeviceIngredientStates");

            migrationBuilder.UpdateData(
                table: "SystemConfigs",
                keyColumn: "SystemConfigId",
                keyValue: "VAT",
                column: "CreatedDate",
                value: new DateTime(2025, 6, 29, 18, 41, 40, 655, DateTimeKind.Utc).AddTicks(8832));
        }
    }
}
