using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomaticBrewingCoffee.Domain.Migrations
{
    /// <inheritdoc />
    public partial class RelativeMenuWithOrganization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProductId",
                table: "OrderDetails",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Menus_OrganizationId",
                table: "Menus",
                column: "OrganizationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Menus_Organizations_OrganizationId",
                table: "Menus",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "OrganizationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Menus_Organizations_OrganizationId",
                table: "Menus");

            migrationBuilder.DropIndex(
                name: "IX_Menus_OrganizationId",
                table: "Menus");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "OrderDetails");
        }
    }
}
