using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomaticBrewingCoffee.Domain.Migrations
{
    /// <inheritdoc />
    public partial class RollbackToPaymentDateUsing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CancelledDate",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "ErrorDate",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "FailedDate",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "LastUpdateAt",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "PendingDate",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "RefundFailedDate",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "RefundedDate",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "RefundingDate",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "ReversedDate",
                table: "Payments");

            migrationBuilder.RenameColumn(
                name: "SuccessDate",
                table: "Payments",
                newName: "PaymentDate");

            migrationBuilder.RenameColumn(
                name: "LastUpdateBy",
                table: "Payments",
                newName: "UpdateBy");

            migrationBuilder.UpdateData(
                table: "IngredientTypes",
                keyColumn: "IngredientTypeId",
                keyValue: "CF",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 24, 6, 6, 56, 787, DateTimeKind.Utc).AddTicks(6968));

            migrationBuilder.UpdateData(
                table: "IngredientTypes",
                keyColumn: "IngredientTypeId",
                keyValue: "CMK",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 24, 6, 6, 56, 787, DateTimeKind.Utc).AddTicks(6985));

            migrationBuilder.UpdateData(
                table: "IngredientTypes",
                keyColumn: "IngredientTypeId",
                keyValue: "CUP",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 24, 6, 6, 56, 787, DateTimeKind.Utc).AddTicks(6986));

            migrationBuilder.UpdateData(
                table: "IngredientTypes",
                keyColumn: "IngredientTypeId",
                keyValue: "ICE",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 24, 6, 6, 56, 787, DateTimeKind.Utc).AddTicks(6983));

            migrationBuilder.UpdateData(
                table: "IngredientTypes",
                keyColumn: "IngredientTypeId",
                keyValue: "MLK",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 24, 6, 6, 56, 787, DateTimeKind.Utc).AddTicks(6971));

            migrationBuilder.UpdateData(
                table: "IngredientTypes",
                keyColumn: "IngredientTypeId",
                keyValue: "SUG",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 24, 6, 6, 56, 787, DateTimeKind.Utc).AddTicks(6972));

            migrationBuilder.UpdateData(
                table: "IngredientTypes",
                keyColumn: "IngredientTypeId",
                keyValue: "WTR",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 24, 6, 6, 56, 787, DateTimeKind.Utc).AddTicks(6982));

            migrationBuilder.UpdateData(
                table: "SystemConfigs",
                keyColumn: "SystemConfigId",
                keyValue: "VAT",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 24, 6, 6, 56, 787, DateTimeKind.Utc).AddTicks(6959));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UpdateBy",
                table: "Payments",
                newName: "LastUpdateBy");

            migrationBuilder.RenameColumn(
                name: "PaymentDate",
                table: "Payments",
                newName: "SuccessDate");

            migrationBuilder.AddColumn<DateTime>(
                name: "CancelledDate",
                table: "Payments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ErrorDate",
                table: "Payments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FailedDate",
                table: "Payments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUpdateAt",
                table: "Payments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PendingDate",
                table: "Payments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RefundFailedDate",
                table: "Payments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RefundedDate",
                table: "Payments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RefundingDate",
                table: "Payments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReversedDate",
                table: "Payments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "IngredientTypes",
                keyColumn: "IngredientTypeId",
                keyValue: "CF",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 24, 3, 20, 35, 45, DateTimeKind.Utc).AddTicks(9026));

            migrationBuilder.UpdateData(
                table: "IngredientTypes",
                keyColumn: "IngredientTypeId",
                keyValue: "CMK",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 24, 3, 20, 35, 45, DateTimeKind.Utc).AddTicks(9039));

            migrationBuilder.UpdateData(
                table: "IngredientTypes",
                keyColumn: "IngredientTypeId",
                keyValue: "CUP",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 24, 3, 20, 35, 45, DateTimeKind.Utc).AddTicks(9040));

            migrationBuilder.UpdateData(
                table: "IngredientTypes",
                keyColumn: "IngredientTypeId",
                keyValue: "ICE",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 24, 3, 20, 35, 45, DateTimeKind.Utc).AddTicks(9037));

            migrationBuilder.UpdateData(
                table: "IngredientTypes",
                keyColumn: "IngredientTypeId",
                keyValue: "MLK",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 24, 3, 20, 35, 45, DateTimeKind.Utc).AddTicks(9028));

            migrationBuilder.UpdateData(
                table: "IngredientTypes",
                keyColumn: "IngredientTypeId",
                keyValue: "SUG",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 24, 3, 20, 35, 45, DateTimeKind.Utc).AddTicks(9029));

            migrationBuilder.UpdateData(
                table: "IngredientTypes",
                keyColumn: "IngredientTypeId",
                keyValue: "WTR",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 24, 3, 20, 35, 45, DateTimeKind.Utc).AddTicks(9030));

            migrationBuilder.UpdateData(
                table: "SystemConfigs",
                keyColumn: "SystemConfigId",
                keyValue: "VAT",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 24, 3, 20, 35, 45, DateTimeKind.Utc).AddTicks(9010));
        }
    }
}
