using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomaticBrewingCoffee.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddSellingPriceInProductMenu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "SellingPrice",
                table: "MenuProductMappings",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SellingPrice",
                table: "MenuProductMappings");
        }
    }
}
