using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AutomaticBrewingCoffee.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddTableIngredientType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IngredientTypes",
                columns: table => new
                {
                    IngredientTypeId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IngredientTypes", x => x.IngredientTypeId);
                });

            migrationBuilder.InsertData(
                table: "IngredientTypes",
                columns: new[] { "IngredientTypeId", "CreatedDate", "DeletedDate", "Description", "IsDeleted", "Name", "Status", "UpdatedDate" },
                values: new object[,]
                {
                    { "CF", new DateTime(2025, 7, 10, 4, 40, 35, 461, DateTimeKind.Utc).AddTicks(7935), null, "Nguyên liệu chính để pha chế đồ uống cà phê.", false, "Cà phê", "Active", null },
                    { "CMK", new DateTime(2025, 7, 10, 4, 40, 35, 461, DateTimeKind.Utc).AddTicks(8015), null, "Sữa đặc có đường, tạo độ ngọt và béo.", false, "Sữa đặc", "Active", null },
                    { "CUP", new DateTime(2025, 7, 10, 4, 40, 35, 461, DateTimeKind.Utc).AddTicks(8016), null, "Cốc đựng đồ uống phục vụ cho khách.", false, "Cốc", "Active", null },
                    { "ICE", new DateTime(2025, 7, 10, 4, 40, 35, 461, DateTimeKind.Utc).AddTicks(8007), null, "Đá viên để làm lạnh đồ uống.", false, "Đá lạnh", "Active", null },
                    { "MLK", new DateTime(2025, 7, 10, 4, 40, 35, 461, DateTimeKind.Utc).AddTicks(8003), null, "Sữa tươi hoặc sữa pha để tăng vị béo.", false, "Sữa", "Active", null },
                    { "SUG", new DateTime(2025, 7, 10, 4, 40, 35, 461, DateTimeKind.Utc).AddTicks(8005), null, "Đường trắng dùng để tạo vị ngọt.", false, "Đường", "Active", null },
                    { "WTR", new DateTime(2025, 7, 10, 4, 40, 35, 461, DateTimeKind.Utc).AddTicks(8006), null, "Nước lọc dùng để pha chế.", false, "Nước", "Active", null }
                });

            migrationBuilder.UpdateData(
                table: "SystemConfigs",
                keyColumn: "SystemConfigId",
                keyValue: "VAT",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 10, 4, 40, 35, 461, DateTimeKind.Utc).AddTicks(7921));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IngredientTypes");

            migrationBuilder.UpdateData(
                table: "SystemConfigs",
                keyColumn: "SystemConfigId",
                keyValue: "VAT",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 8, 11, 5, 57, 136, DateTimeKind.Utc).AddTicks(1408));
        }
    }
}
