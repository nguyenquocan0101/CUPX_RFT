using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomaticBrewingCoffee.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddBaseModelForFunctionParameter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OriginServer",
                table: "Kiosks",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "FunctionParameters",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDate",
                table: "FunctionParameters",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "FunctionParameters",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "FunctionParameters",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OriginServer",
                table: "Kiosks");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "FunctionParameters");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                table: "FunctionParameters");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "FunctionParameters");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "FunctionParameters");
        }
    }
}
