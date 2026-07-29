using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomaticBrewingCoffee.Domain.Migrations
{
    /// <inheritdoc />
    public partial class RefactorOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FeeAmount",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "FeeDescription",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "InvoiceId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "SessionId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Vat",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "VatAmount",
                table: "Orders");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "FeeAmount",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "FeeDescription",
                table: "Orders",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InvoiceId",
                table: "Orders",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SessionId",
                table: "Orders",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Vat",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "VatAmount",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: true);
        }
    }
}
