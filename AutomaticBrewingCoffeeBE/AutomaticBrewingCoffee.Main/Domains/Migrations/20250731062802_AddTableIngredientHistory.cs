using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomaticBrewingCoffee.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddTableIngredientHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeviceIngredientHistories",
                columns: table => new
                {
                    DeviceIngredientHistoryId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DeviceIngredientStateId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DeviceId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DeltaAmount = table.Column<double>(type: "float", nullable: false),
                    OldCapacity = table.Column<double>(type: "float", nullable: false),
                    NewCapacity = table.Column<double>(type: "float", nullable: false),
                    PerformedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Action = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceIngredientHistories", x => x.DeviceIngredientHistoryId);
                    table.ForeignKey(
                        name: "FK_DeviceIngredientHistories_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "DeviceId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceIngredientHistories_DeviceId",
                table: "DeviceIngredientHistories",
                column: "DeviceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeviceIngredientHistories");
        }
    }
}
