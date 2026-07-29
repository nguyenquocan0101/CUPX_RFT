using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomaticBrewingCoffee.Domain.Migrations
{
    /// <inheritdoc />
    public partial class ModifyAttributeOptionForProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "ProductAttributes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Target",
                table: "ProductAttributes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "AttributeOptions",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "SystemConfigs",
                keyColumn: "SystemConfigId",
                keyValue: "VAT",
                column: "CreatedDate",
                value: new DateTime(2025, 6, 26, 7, 17, 35, 76, DateTimeKind.Utc).AddTicks(2416));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "ProductAttributes");

            migrationBuilder.DropColumn(
                name: "Target",
                table: "ProductAttributes");

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "AttributeOptions");

            migrationBuilder.UpdateData(
                table: "SystemConfigs",
                keyColumn: "SystemConfigId",
                keyValue: "VAT",
                column: "CreatedDate",
                value: new DateTime(2025, 6, 26, 4, 57, 19, 564, DateTimeKind.Utc).AddTicks(7076));
        }
    }
}
