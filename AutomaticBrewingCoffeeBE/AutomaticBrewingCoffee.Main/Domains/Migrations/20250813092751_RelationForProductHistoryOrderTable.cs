using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomaticBrewingCoffee.Domain.Migrations
{
    /// <inheritdoc />
    public partial class RelationForProductHistoryOrderTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OrderId",
                table: "DeviceIngredientHistories",
                type: "nvarchar(50)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetails_ProductId",
                table: "OrderDetails",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceIngredientHistories_OrderId",
                table: "DeviceIngredientHistories",
                column: "OrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceIngredientHistories_Orders_OrderId",
                table: "DeviceIngredientHistories",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "OrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderDetails_Products_ProductId",
                table: "OrderDetails",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "ProductId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeviceIngredientHistories_Orders_OrderId",
                table: "DeviceIngredientHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderDetails_Products_ProductId",
                table: "OrderDetails");

            migrationBuilder.DropIndex(
                name: "IX_OrderDetails_ProductId",
                table: "OrderDetails");

            migrationBuilder.DropIndex(
                name: "IX_DeviceIngredientHistories_OrderId",
                table: "DeviceIngredientHistories");

            migrationBuilder.DropColumn(
                name: "OrderId",
                table: "DeviceIngredientHistories");
        }
    }
}
