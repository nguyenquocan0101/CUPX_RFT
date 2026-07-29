using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomaticBrewingCoffee.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddUnbanReasoneForAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UnbannedReason",
                table: "Accounts",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Accounts",
                keyColumn: "AccountId",
                keyValue: "herJ9VX5NEfyUWsDLc9MbbLH4603",
                column: "UnbannedReason",
                value: null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UnbannedReason",
                table: "Accounts");
        }
    }
}
