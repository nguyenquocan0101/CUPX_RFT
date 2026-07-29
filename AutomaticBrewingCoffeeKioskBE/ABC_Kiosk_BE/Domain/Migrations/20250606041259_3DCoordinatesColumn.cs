using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class _3DCoordinatesColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "J1",
                table: "Devices",
                type: "numeric(7,3)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "J2",
                table: "Devices",
                type: "numeric(7,3)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "J3",
                table: "Devices",
                type: "numeric(7,3)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "J4",
                table: "Devices",
                type: "numeric(7,3)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "J5",
                table: "Devices",
                type: "numeric(7,3)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "J6",
                table: "Devices",
                type: "numeric(7,3)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "X",
                table: "Devices",
                type: "numeric(7,3)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Y",
                table: "Devices",
                type: "numeric(7,3)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Z",
                table: "Devices",
                type: "numeric(7,3)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "J1",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "J2",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "J3",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "J4",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "J5",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "J6",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "X",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "Y",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "Z",
                table: "Devices");
        }
    }
}
