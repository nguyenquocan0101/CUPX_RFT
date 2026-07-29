using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomaticBrewingCoffee.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddDeviceFunctionParameter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeviceFunctions",
                columns: table => new
                {
                    DeviceFunctionId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DeviceModelId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceFunctions", x => x.DeviceFunctionId);
                    table.ForeignKey(
                        name: "FK_DeviceFunctions_DeviceModels_DeviceModelId",
                        column: x => x.DeviceModelId,
                        principalTable: "DeviceModels",
                        principalColumn: "DeviceModelId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FunctionParameters",
                columns: table => new
                {
                    FunctionParameterId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DeviceFunctionId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Default = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FunctionParameters", x => x.FunctionParameterId);
                    table.ForeignKey(
                        name: "FK_FunctionParameters_DeviceFunctions_DeviceFunctionId",
                        column: x => x.DeviceFunctionId,
                        principalTable: "DeviceFunctions",
                        principalColumn: "DeviceFunctionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceFunctions_DeviceModelId",
                table: "DeviceFunctions",
                column: "DeviceModelId");

            migrationBuilder.CreateIndex(
                name: "IX_FunctionParameters_DeviceFunctionId",
                table: "FunctionParameters",
                column: "DeviceFunctionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FunctionParameters");

            migrationBuilder.DropTable(
                name: "DeviceFunctions");
        }
    }
}
