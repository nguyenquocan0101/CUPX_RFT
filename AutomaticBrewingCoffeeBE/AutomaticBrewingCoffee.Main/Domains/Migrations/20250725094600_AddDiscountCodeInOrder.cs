using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomaticBrewingCoffee.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddDiscountCodeInOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DiscountCode",
                table: "Orders",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "IngredientTypes",
                keyColumn: "IngredientTypeId",
                keyValue: "CF",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 25, 9, 45, 59, 500, DateTimeKind.Utc).AddTicks(3434));

            migrationBuilder.UpdateData(
                table: "IngredientTypes",
                keyColumn: "IngredientTypeId",
                keyValue: "CMK",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 25, 9, 45, 59, 500, DateTimeKind.Utc).AddTicks(3445));

            migrationBuilder.UpdateData(
                table: "IngredientTypes",
                keyColumn: "IngredientTypeId",
                keyValue: "CUP",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 25, 9, 45, 59, 500, DateTimeKind.Utc).AddTicks(3456));

            migrationBuilder.UpdateData(
                table: "IngredientTypes",
                keyColumn: "IngredientTypeId",
                keyValue: "ICE",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 25, 9, 45, 59, 500, DateTimeKind.Utc).AddTicks(3443));

            migrationBuilder.UpdateData(
                table: "IngredientTypes",
                keyColumn: "IngredientTypeId",
                keyValue: "MLK",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 25, 9, 45, 59, 500, DateTimeKind.Utc).AddTicks(3438));

            migrationBuilder.UpdateData(
                table: "IngredientTypes",
                keyColumn: "IngredientTypeId",
                keyValue: "SUG",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 25, 9, 45, 59, 500, DateTimeKind.Utc).AddTicks(3440));

            migrationBuilder.UpdateData(
                table: "IngredientTypes",
                keyColumn: "IngredientTypeId",
                keyValue: "WTR",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 25, 9, 45, 59, 500, DateTimeKind.Utc).AddTicks(3441));

            migrationBuilder.UpdateData(
                table: "SystemConfigs",
                keyColumn: "SystemConfigId",
                keyValue: "VAT",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 25, 9, 45, 59, 500, DateTimeKind.Utc).AddTicks(3422));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiscountCode",
                table: "Orders");

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
    }
}
