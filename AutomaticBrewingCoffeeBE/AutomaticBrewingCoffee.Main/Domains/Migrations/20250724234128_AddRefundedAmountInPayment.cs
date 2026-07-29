using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomaticBrewingCoffee.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddRefundedAmountInPayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "RefundedAmount",
                table: "Payments",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "IngredientTypes",
                keyColumn: "IngredientTypeId",
                keyValue: "CF",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 24, 23, 41, 27, 59, DateTimeKind.Utc).AddTicks(864));

            migrationBuilder.UpdateData(
                table: "IngredientTypes",
                keyColumn: "IngredientTypeId",
                keyValue: "CMK",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 24, 23, 41, 27, 59, DateTimeKind.Utc).AddTicks(872));

            migrationBuilder.UpdateData(
                table: "IngredientTypes",
                keyColumn: "IngredientTypeId",
                keyValue: "CUP",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 24, 23, 41, 27, 59, DateTimeKind.Utc).AddTicks(882));

            migrationBuilder.UpdateData(
                table: "IngredientTypes",
                keyColumn: "IngredientTypeId",
                keyValue: "ICE",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 24, 23, 41, 27, 59, DateTimeKind.Utc).AddTicks(870));

            migrationBuilder.UpdateData(
                table: "IngredientTypes",
                keyColumn: "IngredientTypeId",
                keyValue: "MLK",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 24, 23, 41, 27, 59, DateTimeKind.Utc).AddTicks(866));

            migrationBuilder.UpdateData(
                table: "IngredientTypes",
                keyColumn: "IngredientTypeId",
                keyValue: "SUG",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 24, 23, 41, 27, 59, DateTimeKind.Utc).AddTicks(868));

            migrationBuilder.UpdateData(
                table: "IngredientTypes",
                keyColumn: "IngredientTypeId",
                keyValue: "WTR",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 24, 23, 41, 27, 59, DateTimeKind.Utc).AddTicks(869));

            migrationBuilder.UpdateData(
                table: "SystemConfigs",
                keyColumn: "SystemConfigId",
                keyValue: "VAT",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 24, 23, 41, 27, 59, DateTimeKind.Utc).AddTicks(844));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RefundedAmount",
                table: "Payments");

            migrationBuilder.UpdateData(
                table: "IngredientTypes",
                keyColumn: "IngredientTypeId",
                keyValue: "CF",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 24, 10, 57, 3, 209, DateTimeKind.Utc).AddTicks(6921));

            migrationBuilder.UpdateData(
                table: "IngredientTypes",
                keyColumn: "IngredientTypeId",
                keyValue: "CMK",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 24, 10, 57, 3, 209, DateTimeKind.Utc).AddTicks(6931));

            migrationBuilder.UpdateData(
                table: "IngredientTypes",
                keyColumn: "IngredientTypeId",
                keyValue: "CUP",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 24, 10, 57, 3, 209, DateTimeKind.Utc).AddTicks(6940));

            migrationBuilder.UpdateData(
                table: "IngredientTypes",
                keyColumn: "IngredientTypeId",
                keyValue: "ICE",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 24, 10, 57, 3, 209, DateTimeKind.Utc).AddTicks(6929));

            migrationBuilder.UpdateData(
                table: "IngredientTypes",
                keyColumn: "IngredientTypeId",
                keyValue: "MLK",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 24, 10, 57, 3, 209, DateTimeKind.Utc).AddTicks(6925));

            migrationBuilder.UpdateData(
                table: "IngredientTypes",
                keyColumn: "IngredientTypeId",
                keyValue: "SUG",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 24, 10, 57, 3, 209, DateTimeKind.Utc).AddTicks(6927));

            migrationBuilder.UpdateData(
                table: "IngredientTypes",
                keyColumn: "IngredientTypeId",
                keyValue: "WTR",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 24, 10, 57, 3, 209, DateTimeKind.Utc).AddTicks(6928));

            migrationBuilder.UpdateData(
                table: "SystemConfigs",
                keyColumn: "SystemConfigId",
                keyValue: "VAT",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 24, 10, 57, 3, 209, DateTimeKind.Utc).AddTicks(6909));
        }
    }
}
