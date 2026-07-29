using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomaticBrewingCoffee.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganizationToMenu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "KioskId",
                table: "Menus",
                newName: "OrganizationId");

            migrationBuilder.AddColumn<bool>(
                name: "IsMobileDevice",
                table: "DeviceTypes",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsMobileDevice",
                table: "DeviceTypes");

            migrationBuilder.RenameColumn(
                name: "OrganizationId",
                table: "Menus",
                newName: "KioskId");
        }
    }
}
