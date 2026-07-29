using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomaticBrewingCoffee.Domain.Migrations
{
    /// <inheritdoc />
    public partial class MaxLengthForProductMaking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PreparingProductIds",
                table: "Orders",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FailedProductIds",
                table: "Orders",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CompletedProductIds",
                table: "Orders",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450,
                oldNullable: true);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PreparingProductIds",
                table: "Orders",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FailedProductIds",
                table: "Orders",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CompletedProductIds",
                table: "Orders",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "IngredientTypes",
                keyColumn: "IngredientTypeId",
                keyValue: "CF",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 24, 10, 41, 1, 366, DateTimeKind.Utc).AddTicks(8786));

            migrationBuilder.UpdateData(
                table: "IngredientTypes",
                keyColumn: "IngredientTypeId",
                keyValue: "CMK",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 24, 10, 41, 1, 366, DateTimeKind.Utc).AddTicks(8795));

            migrationBuilder.UpdateData(
                table: "IngredientTypes",
                keyColumn: "IngredientTypeId",
                keyValue: "CUP",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 24, 10, 41, 1, 366, DateTimeKind.Utc).AddTicks(8803));

            migrationBuilder.UpdateData(
                table: "IngredientTypes",
                keyColumn: "IngredientTypeId",
                keyValue: "ICE",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 24, 10, 41, 1, 366, DateTimeKind.Utc).AddTicks(8793));

            migrationBuilder.UpdateData(
                table: "IngredientTypes",
                keyColumn: "IngredientTypeId",
                keyValue: "MLK",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 24, 10, 41, 1, 366, DateTimeKind.Utc).AddTicks(8789));

            migrationBuilder.UpdateData(
                table: "IngredientTypes",
                keyColumn: "IngredientTypeId",
                keyValue: "SUG",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 24, 10, 41, 1, 366, DateTimeKind.Utc).AddTicks(8791));

            migrationBuilder.UpdateData(
                table: "IngredientTypes",
                keyColumn: "IngredientTypeId",
                keyValue: "WTR",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 24, 10, 41, 1, 366, DateTimeKind.Utc).AddTicks(8792));

            migrationBuilder.UpdateData(
                table: "SystemConfigs",
                keyColumn: "SystemConfigId",
                keyValue: "VAT",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 24, 10, 41, 1, 366, DateTimeKind.Utc).AddTicks(8776));
        }
    }
}
