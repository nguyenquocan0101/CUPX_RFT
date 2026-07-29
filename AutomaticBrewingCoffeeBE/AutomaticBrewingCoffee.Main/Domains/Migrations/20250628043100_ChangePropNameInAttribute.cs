using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomaticBrewingCoffee.Domain.Migrations
{
    /// <inheritdoc />
    public partial class ChangePropNameInAttribute : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Target",
                table: "ProductAttributes",
                newName: "Label");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "ProductAttributes",
                newName: "IngredientType");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "DeviceIngredients",
                newName: "Label");

            migrationBuilder.AddColumn<string>(
                name: "Unit",
                table: "ProductAttributes",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "IngredientType",
                table: "DeviceIngredients",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "SystemConfigs",
                keyColumn: "SystemConfigId",
                keyValue: "VAT",
                column: "CreatedDate",
                value: new DateTime(2025, 6, 28, 4, 30, 59, 665, DateTimeKind.Utc).AddTicks(5267));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Unit",
                table: "ProductAttributes");

            migrationBuilder.DropColumn(
                name: "IngredientType",
                table: "DeviceIngredients");

            migrationBuilder.RenameColumn(
                name: "Label",
                table: "ProductAttributes",
                newName: "Target");

            migrationBuilder.RenameColumn(
                name: "IngredientType",
                table: "ProductAttributes",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "Label",
                table: "DeviceIngredients",
                newName: "Name");

            migrationBuilder.UpdateData(
                table: "SystemConfigs",
                keyColumn: "SystemConfigId",
                keyValue: "VAT",
                column: "CreatedDate",
                value: new DateTime(2025, 6, 28, 3, 8, 50, 73, DateTimeKind.Utc).AddTicks(2505));
        }
    }
}
