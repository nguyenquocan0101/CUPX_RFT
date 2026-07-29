using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomaticBrewingCoffee.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddLabelAndDisplayOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "ProductCategory",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Label",
                table: "DeviceFunctions",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "SystemConfigs",
                keyColumn: "SystemConfigId",
                keyValue: "VAT",
                column: "CreatedDate",
                value: new DateTime(2025, 6, 21, 10, 22, 57, 355, DateTimeKind.Utc).AddTicks(3194));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "ProductCategory");

            migrationBuilder.DropColumn(
                name: "Label",
                table: "DeviceFunctions");

            migrationBuilder.UpdateData(
                table: "SystemConfigs",
                keyColumn: "SystemConfigId",
                keyValue: "VAT",
                column: "CreatedDate",
                value: new DateTime(2025, 6, 21, 4, 5, 30, 732, DateTimeKind.Utc).AddTicks(8941));
        }
    }
}
