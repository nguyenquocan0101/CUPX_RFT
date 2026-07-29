using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomaticBrewingCoffee.Domain.Migrations
{
    /// <inheritdoc />
    public partial class RelationModifyForTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ReferenceId",
                table: "Accounts",
                newName: "OrganizationId");

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

            migrationBuilder.CreateIndex(
                name: "IX_Webhooks_KioskId",
                table: "Webhooks",
                column: "KioskId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OrganizationId",
                table: "Orders",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_StoreId",
                table: "Orders",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_OrganizationId",
                table: "Accounts",
                column: "OrganizationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_Organizations_OrganizationId",
                table: "Accounts",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "OrganizationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Organizations_OrganizationId",
                table: "Orders",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "OrganizationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Stores_StoreId",
                table: "Orders",
                column: "StoreId",
                principalTable: "Stores",
                principalColumn: "StoreId");

            migrationBuilder.AddForeignKey(
                name: "FK_Webhooks_Kiosks_KioskId",
                table: "Webhooks",
                column: "KioskId",
                principalTable: "Kiosks",
                principalColumn: "KioskId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_Organizations_OrganizationId",
                table: "Accounts");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Organizations_OrganizationId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Stores_StoreId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Webhooks_Kiosks_KioskId",
                table: "Webhooks");

            migrationBuilder.DropIndex(
                name: "IX_Webhooks_KioskId",
                table: "Webhooks");

            migrationBuilder.DropIndex(
                name: "IX_Orders_OrganizationId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_StoreId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_OrganizationId",
                table: "Accounts");

            migrationBuilder.RenameColumn(
                name: "OrganizationId",
                table: "Accounts",
                newName: "ReferenceId");

            migrationBuilder.UpdateData(
                table: "IngredientTypes",
                keyColumn: "IngredientTypeId",
                keyValue: "CF",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 13, 7, 28, 14, 832, DateTimeKind.Utc).AddTicks(2108));

            migrationBuilder.UpdateData(
                table: "IngredientTypes",
                keyColumn: "IngredientTypeId",
                keyValue: "CMK",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 13, 7, 28, 14, 832, DateTimeKind.Utc).AddTicks(2125));

            migrationBuilder.UpdateData(
                table: "IngredientTypes",
                keyColumn: "IngredientTypeId",
                keyValue: "CUP",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 13, 7, 28, 14, 832, DateTimeKind.Utc).AddTicks(2126));

            migrationBuilder.UpdateData(
                table: "IngredientTypes",
                keyColumn: "IngredientTypeId",
                keyValue: "ICE",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 13, 7, 28, 14, 832, DateTimeKind.Utc).AddTicks(2122));

            migrationBuilder.UpdateData(
                table: "IngredientTypes",
                keyColumn: "IngredientTypeId",
                keyValue: "MLK",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 13, 7, 28, 14, 832, DateTimeKind.Utc).AddTicks(2111));

            migrationBuilder.UpdateData(
                table: "IngredientTypes",
                keyColumn: "IngredientTypeId",
                keyValue: "SUG",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 13, 7, 28, 14, 832, DateTimeKind.Utc).AddTicks(2113));

            migrationBuilder.UpdateData(
                table: "IngredientTypes",
                keyColumn: "IngredientTypeId",
                keyValue: "WTR",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 13, 7, 28, 14, 832, DateTimeKind.Utc).AddTicks(2114));

            migrationBuilder.UpdateData(
                table: "SystemConfigs",
                keyColumn: "SystemConfigId",
                keyValue: "VAT",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 13, 7, 28, 14, 832, DateTimeKind.Utc).AddTicks(2092));
        }
    }
}
