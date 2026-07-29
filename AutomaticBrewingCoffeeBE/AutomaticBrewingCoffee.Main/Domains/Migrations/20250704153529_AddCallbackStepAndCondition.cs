using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomaticBrewingCoffee.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddCallbackStepAndCondition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CallbackStepId",
                table: "Steps",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Conditions",
                table: "Steps",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "SystemConfigs",
                keyColumn: "SystemConfigId",
                keyValue: "VAT",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 4, 15, 35, 28, 211, DateTimeKind.Utc).AddTicks(640));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CallbackStepId",
                table: "Steps");

            migrationBuilder.DropColumn(
                name: "Conditions",
                table: "Steps");

            migrationBuilder.UpdateData(
                table: "SystemConfigs",
                keyColumn: "SystemConfigId",
                keyValue: "VAT",
                column: "CreatedDate",
                value: new DateTime(2025, 6, 29, 18, 47, 16, 646, DateTimeKind.Utc).AddTicks(3138));
        }
    }
}
