using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomaticBrewingCoffee.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddProductMakingListForOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CompletedProductIds",
                table: "Orders",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FailedProductIds",
                table: "Orders",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PreparingProductIds",
                table: "Orders",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "OrderDetails",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDate",
                table: "OrderDetails",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "OrderDetails",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "OrderDetails",
                type: "datetime2",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "IngredientTypes",
                keyColumn: "IngredientTypeId",
                keyValue: "CF",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 24, 10, 35, 26, 734, DateTimeKind.Utc).AddTicks(9246));

            migrationBuilder.UpdateData(
                table: "IngredientTypes",
                keyColumn: "IngredientTypeId",
                keyValue: "CMK",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 24, 10, 35, 26, 734, DateTimeKind.Utc).AddTicks(9260));

            migrationBuilder.UpdateData(
                table: "IngredientTypes",
                keyColumn: "IngredientTypeId",
                keyValue: "CUP",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 24, 10, 35, 26, 734, DateTimeKind.Utc).AddTicks(9261));

            migrationBuilder.UpdateData(
                table: "IngredientTypes",
                keyColumn: "IngredientTypeId",
                keyValue: "ICE",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 24, 10, 35, 26, 734, DateTimeKind.Utc).AddTicks(9254));

            migrationBuilder.UpdateData(
                table: "IngredientTypes",
                keyColumn: "IngredientTypeId",
                keyValue: "MLK",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 24, 10, 35, 26, 734, DateTimeKind.Utc).AddTicks(9250));

            migrationBuilder.UpdateData(
                table: "IngredientTypes",
                keyColumn: "IngredientTypeId",
                keyValue: "SUG",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 24, 10, 35, 26, 734, DateTimeKind.Utc).AddTicks(9251));

            migrationBuilder.UpdateData(
                table: "IngredientTypes",
                keyColumn: "IngredientTypeId",
                keyValue: "WTR",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 24, 10, 35, 26, 734, DateTimeKind.Utc).AddTicks(9253));

            migrationBuilder.UpdateData(
                table: "SystemConfigs",
                keyColumn: "SystemConfigId",
                keyValue: "VAT",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 24, 10, 35, 26, 734, DateTimeKind.Utc).AddTicks(9233));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompletedProductIds",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "FailedProductIds",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PreparingProductIds",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "OrderDetails");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                table: "OrderDetails");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "OrderDetails");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "OrderDetails");

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
    }
}
