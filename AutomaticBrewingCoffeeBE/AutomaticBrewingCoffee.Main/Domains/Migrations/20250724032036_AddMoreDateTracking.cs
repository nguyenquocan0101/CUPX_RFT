using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomaticBrewingCoffee.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddMoreDateTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PaymentDate",
                table: "Payments",
                newName: "UpdatedDate");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "Payments",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CancelledDate",
                table: "Payments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDate",
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

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Payments",
                type: "bit",
                nullable: false,
                defaultValue: false);

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

            migrationBuilder.AddColumn<DateTime>(
                name: "SuccessDate",
                table: "Payments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CancelledDate",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedDate",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FailedDate",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PendingDate",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PreparingDate",
                table: "Orders",
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CancelledDate",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "ErrorDate",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "FailedDate",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
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

            migrationBuilder.DropColumn(
                name: "SuccessDate",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "CancelledDate",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CompletedDate",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "FailedDate",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PendingDate",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PreparingDate",
                table: "Orders");

            migrationBuilder.RenameColumn(
                name: "UpdatedDate",
                table: "Payments",
                newName: "PaymentDate");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "Payments",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.UpdateData(
                table: "IngredientTypes",
                keyColumn: "IngredientTypeId",
                keyValue: "CF",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 23, 16, 54, 3, 153, DateTimeKind.Utc).AddTicks(6552));

            migrationBuilder.UpdateData(
                table: "IngredientTypes",
                keyColumn: "IngredientTypeId",
                keyValue: "CMK",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 23, 16, 54, 3, 153, DateTimeKind.Utc).AddTicks(6571));

            migrationBuilder.UpdateData(
                table: "IngredientTypes",
                keyColumn: "IngredientTypeId",
                keyValue: "CUP",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 23, 16, 54, 3, 153, DateTimeKind.Utc).AddTicks(6573));

            migrationBuilder.UpdateData(
                table: "IngredientTypes",
                keyColumn: "IngredientTypeId",
                keyValue: "ICE",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 23, 16, 54, 3, 153, DateTimeKind.Utc).AddTicks(6560));

            migrationBuilder.UpdateData(
                table: "IngredientTypes",
                keyColumn: "IngredientTypeId",
                keyValue: "MLK",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 23, 16, 54, 3, 153, DateTimeKind.Utc).AddTicks(6555));

            migrationBuilder.UpdateData(
                table: "IngredientTypes",
                keyColumn: "IngredientTypeId",
                keyValue: "SUG",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 23, 16, 54, 3, 153, DateTimeKind.Utc).AddTicks(6557));

            migrationBuilder.UpdateData(
                table: "IngredientTypes",
                keyColumn: "IngredientTypeId",
                keyValue: "WTR",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 23, 16, 54, 3, 153, DateTimeKind.Utc).AddTicks(6559));

            migrationBuilder.UpdateData(
                table: "SystemConfigs",
                keyColumn: "SystemConfigId",
                keyValue: "VAT",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 23, 16, 54, 3, 153, DateTimeKind.Utc).AddTicks(6527));
        }
    }
}
