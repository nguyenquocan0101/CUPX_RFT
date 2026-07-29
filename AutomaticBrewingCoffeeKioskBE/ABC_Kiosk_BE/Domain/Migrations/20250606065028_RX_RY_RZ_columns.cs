using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class RX_RY_RZ_columns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "RX",
                table: "Devices",
                type: "numeric(7,3)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "RY",
                table: "Devices",
                type: "numeric(7,3)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "RZ",
                table: "Devices",
                type: "numeric(7,3)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RX",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "RY",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "RZ",
                table: "Devices");
        }
    }
}
