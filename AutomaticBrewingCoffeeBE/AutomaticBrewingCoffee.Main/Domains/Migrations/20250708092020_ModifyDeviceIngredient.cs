using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomaticBrewingCoffee.Domain.Migrations
{
    /// <inheritdoc />
    public partial class ModifyDeviceIngredient : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeviceFunctionName",
                table: "DeviceIngredients",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "IngredientSelectorParameter",
                table: "DeviceIngredients",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IngredientSelectorValue",
                table: "DeviceIngredients",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TargetOverrideParameter",
                table: "DeviceIngredients",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "SystemConfigs",
                keyColumn: "SystemConfigId",
                keyValue: "VAT",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 8, 9, 20, 20, 290, DateTimeKind.Utc).AddTicks(5310));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeviceFunctionName",
                table: "DeviceIngredients");

            migrationBuilder.DropColumn(
                name: "IngredientSelectorParameter",
                table: "DeviceIngredients");

            migrationBuilder.DropColumn(
                name: "IngredientSelectorValue",
                table: "DeviceIngredients");

            migrationBuilder.DropColumn(
                name: "TargetOverrideParameter",
                table: "DeviceIngredients");

            migrationBuilder.UpdateData(
                table: "SystemConfigs",
                keyColumn: "SystemConfigId",
                keyValue: "VAT",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 7, 10, 26, 21, 525, DateTimeKind.Utc).AddTicks(647));
        }
    }
}
