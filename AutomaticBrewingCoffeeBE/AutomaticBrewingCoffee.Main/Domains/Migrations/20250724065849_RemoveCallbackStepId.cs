using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomaticBrewingCoffee.Domain.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCallbackStepId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Organizations_OrganizationId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_OrganizationId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CallbackStepId",
                table: "Steps");

            migrationBuilder.UpdateData(
                table: "IngredientTypes",
                keyColumn: "IngredientTypeId",
                keyValue: "CF",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 24, 6, 58, 48, 746, DateTimeKind.Utc).AddTicks(9616));

            migrationBuilder.UpdateData(
                table: "IngredientTypes",
                keyColumn: "IngredientTypeId",
                keyValue: "CMK",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 24, 6, 58, 48, 746, DateTimeKind.Utc).AddTicks(9625));

            migrationBuilder.UpdateData(
                table: "IngredientTypes",
                keyColumn: "IngredientTypeId",
                keyValue: "CUP",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 24, 6, 58, 48, 746, DateTimeKind.Utc).AddTicks(9677));

            migrationBuilder.UpdateData(
                table: "IngredientTypes",
                keyColumn: "IngredientTypeId",
                keyValue: "ICE",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 24, 6, 58, 48, 746, DateTimeKind.Utc).AddTicks(9623));

            migrationBuilder.UpdateData(
                table: "IngredientTypes",
                keyColumn: "IngredientTypeId",
                keyValue: "MLK",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 24, 6, 58, 48, 746, DateTimeKind.Utc).AddTicks(9620));

            migrationBuilder.UpdateData(
                table: "IngredientTypes",
                keyColumn: "IngredientTypeId",
                keyValue: "SUG",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 24, 6, 58, 48, 746, DateTimeKind.Utc).AddTicks(9621));

            migrationBuilder.UpdateData(
                table: "IngredientTypes",
                keyColumn: "IngredientTypeId",
                keyValue: "WTR",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 24, 6, 58, 48, 746, DateTimeKind.Utc).AddTicks(9622));

            migrationBuilder.UpdateData(
                table: "SystemConfigs",
                keyColumn: "SystemConfigId",
                keyValue: "VAT",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 24, 6, 58, 48, 746, DateTimeKind.Utc).AddTicks(9605));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CallbackStepId",
                table: "Steps",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "IngredientTypes",
                keyColumn: "IngredientTypeId",
                keyValue: "CF",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 24, 6, 39, 9, 18, DateTimeKind.Utc).AddTicks(5164));

            migrationBuilder.UpdateData(
                table: "IngredientTypes",
                keyColumn: "IngredientTypeId",
                keyValue: "CMK",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 24, 6, 39, 9, 18, DateTimeKind.Utc).AddTicks(5183));

            migrationBuilder.UpdateData(
                table: "IngredientTypes",
                keyColumn: "IngredientTypeId",
                keyValue: "CUP",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 24, 6, 39, 9, 18, DateTimeKind.Utc).AddTicks(5184));

            migrationBuilder.UpdateData(
                table: "IngredientTypes",
                keyColumn: "IngredientTypeId",
                keyValue: "ICE",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 24, 6, 39, 9, 18, DateTimeKind.Utc).AddTicks(5181));

            migrationBuilder.UpdateData(
                table: "IngredientTypes",
                keyColumn: "IngredientTypeId",
                keyValue: "MLK",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 24, 6, 39, 9, 18, DateTimeKind.Utc).AddTicks(5169));

            migrationBuilder.UpdateData(
                table: "IngredientTypes",
                keyColumn: "IngredientTypeId",
                keyValue: "SUG",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 24, 6, 39, 9, 18, DateTimeKind.Utc).AddTicks(5170));

            migrationBuilder.UpdateData(
                table: "IngredientTypes",
                keyColumn: "IngredientTypeId",
                keyValue: "WTR",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 24, 6, 39, 9, 18, DateTimeKind.Utc).AddTicks(5179));

            migrationBuilder.UpdateData(
                table: "SystemConfigs",
                keyColumn: "SystemConfigId",
                keyValue: "VAT",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 24, 6, 39, 9, 18, DateTimeKind.Utc).AddTicks(5149));

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OrganizationId",
                table: "Orders",
                column: "OrganizationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Organizations_OrganizationId",
                table: "Orders",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "OrganizationId");
        }
    }
}
