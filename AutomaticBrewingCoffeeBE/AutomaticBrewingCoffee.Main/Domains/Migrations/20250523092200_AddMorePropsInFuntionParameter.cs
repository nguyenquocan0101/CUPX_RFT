using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomaticBrewingCoffee.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddMorePropsInFuntionParameter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Default",
                table: "FunctionParameters",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "FunctionParameters",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Max",
                table: "FunctionParameters",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Min",
                table: "FunctionParameters",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Options",
                table: "FunctionParameters",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "FunctionParameters");

            migrationBuilder.DropColumn(
                name: "Max",
                table: "FunctionParameters");

            migrationBuilder.DropColumn(
                name: "Min",
                table: "FunctionParameters");

            migrationBuilder.DropColumn(
                name: "Options",
                table: "FunctionParameters");

            migrationBuilder.AlterColumn<string>(
                name: "Default",
                table: "FunctionParameters",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450);
        }
    }
}
