using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomaticBrewingCoffee.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddTableDeviceIngredient : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeviceIngredients",
                columns: table => new
                {
                    DeviceIngredientId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DeviceModelId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    MaxCapacity = table.Column<double>(type: "float", nullable: false),
                    MinCapacity = table.Column<double>(type: "float", nullable: false),
                    WarningPercent = table.Column<double>(type: "float", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsRenewable = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceIngredients", x => x.DeviceIngredientId);
                    table.ForeignKey(
                        name: "FK_DeviceIngredients_DeviceModels_DeviceModelId",
                        column: x => x.DeviceModelId,
                        principalTable: "DeviceModels",
                        principalColumn: "DeviceModelId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "SystemConfigs",
                keyColumn: "SystemConfigId",
                keyValue: "VAT",
                column: "CreatedDate",
                value: new DateTime(2025, 6, 27, 6, 5, 23, 170, DateTimeKind.Utc).AddTicks(9832));

            migrationBuilder.CreateIndex(
                name: "IX_DeviceIngredients_DeviceModelId",
                table: "DeviceIngredients",
                column: "DeviceModelId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeviceIngredients");

            migrationBuilder.UpdateData(
                table: "SystemConfigs",
                keyColumn: "SystemConfigId",
                keyValue: "VAT",
                column: "CreatedDate",
                value: new DateTime(2025, 6, 27, 5, 36, 23, 64, DateTimeKind.Utc).AddTicks(2518));
        }
    }
}
