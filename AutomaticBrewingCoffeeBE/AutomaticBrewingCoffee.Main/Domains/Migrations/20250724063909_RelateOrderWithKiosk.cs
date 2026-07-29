using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomaticBrewingCoffee.Domain.Migrations
{
    /// <inheritdoc />
    public partial class RelateOrderWithKiosk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Stores_StoreId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_StoreId",
                table: "Orders");

            migrationBuilder.RenameColumn(
                name: "LastUpdateBy",
                table: "Orders",
                newName: "UpdateBy");

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
                name: "IX_Orders_KioskId",
                table: "Orders",
                column: "KioskId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Kiosks_KioskId",
                table: "Orders",
                column: "KioskId",
                principalTable: "Kiosks",
                principalColumn: "KioskId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Kiosks_KioskId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_KioskId",
                table: "Orders");

            migrationBuilder.RenameColumn(
                name: "UpdateBy",
                table: "Orders",
                newName: "LastUpdateBy");

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

            migrationBuilder.CreateIndex(
                name: "IX_Orders_StoreId",
                table: "Orders",
                column: "StoreId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Stores_StoreId",
                table: "Orders",
                column: "StoreId",
                principalTable: "Stores",
                principalColumn: "StoreId");
        }
    }
}
