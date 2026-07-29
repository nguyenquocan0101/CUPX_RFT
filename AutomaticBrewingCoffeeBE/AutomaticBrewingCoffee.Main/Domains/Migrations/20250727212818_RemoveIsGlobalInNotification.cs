using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomaticBrewingCoffee.Domain.Migrations
{
    /// <inheritdoc />
    public partial class RemoveIsGlobalInNotification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsGlobal",
                table: "Notifications");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsGlobal",
                table: "Notifications",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
