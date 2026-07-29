using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomaticBrewingCoffee.Domain.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRelativeFromDeviceIngredient : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeviceIngredientStates_DeviceIngredients_DeviceIngredientId",
                table: "DeviceIngredientStates");

            migrationBuilder.DropIndex(
                name: "IX_DeviceIngredientStates_DeviceIngredientId",
                table: "DeviceIngredientStates");

            migrationBuilder.DropColumn(
                name: "DeviceIngredientId",
                table: "DeviceIngredientStates");

            migrationBuilder.AddColumn<string>(
                name: "IngredientType",
                table: "DeviceIngredientStates",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsPrimary",
                table: "DeviceIngredientStates",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsRenewable",
                table: "DeviceIngredientStates",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<double>(
                name: "MaxCapacity",
                table: "DeviceIngredientStates",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MinCapacity",
                table: "DeviceIngredientStates",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "WarningPercent",
                table: "DeviceIngredientStates",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.UpdateData(
                table: "SystemConfigs",
                keyColumn: "SystemConfigId",
                keyValue: "VAT",
                column: "CreatedDate",
                value: new DateTime(2025, 6, 29, 18, 41, 40, 655, DateTimeKind.Utc).AddTicks(8832));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IngredientType",
                table: "DeviceIngredientStates");

            migrationBuilder.DropColumn(
                name: "IsPrimary",
                table: "DeviceIngredientStates");

            migrationBuilder.DropColumn(
                name: "IsRenewable",
                table: "DeviceIngredientStates");

            migrationBuilder.DropColumn(
                name: "MaxCapacity",
                table: "DeviceIngredientStates");

            migrationBuilder.DropColumn(
                name: "MinCapacity",
                table: "DeviceIngredientStates");

            migrationBuilder.DropColumn(
                name: "WarningPercent",
                table: "DeviceIngredientStates");

            migrationBuilder.AddColumn<string>(
                name: "DeviceIngredientId",
                table: "DeviceIngredientStates",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "SystemConfigs",
                keyColumn: "SystemConfigId",
                keyValue: "VAT",
                column: "CreatedDate",
                value: new DateTime(2025, 6, 28, 7, 1, 33, 562, DateTimeKind.Utc).AddTicks(4899));

            migrationBuilder.CreateIndex(
                name: "IX_DeviceIngredientStates_DeviceIngredientId",
                table: "DeviceIngredientStates",
                column: "DeviceIngredientId");

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceIngredientStates_DeviceIngredients_DeviceIngredientId",
                table: "DeviceIngredientStates",
                column: "DeviceIngredientId",
                principalTable: "DeviceIngredients",
                principalColumn: "DeviceIngredientId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
