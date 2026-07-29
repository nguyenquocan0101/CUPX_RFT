using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomaticBrewingCoffee.Domain.Migrations
{
    /// <inheritdoc />
    public partial class PrecisionForSellingPrice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "SystemConfigs",
                keyColumn: "SystemConfigId",
                keyValue: "VAT",
                column: "CreatedDate",
                value: new DateTime(2025, 6, 19, 7, 42, 1, 865, DateTimeKind.Utc).AddTicks(5656));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "SystemConfigs",
                keyColumn: "SystemConfigId",
                keyValue: "VAT",
                column: "CreatedDate",
                value: new DateTime(2025, 6, 19, 7, 39, 33, 700, DateTimeKind.Utc).AddTicks(4626));
        }
    }
}
