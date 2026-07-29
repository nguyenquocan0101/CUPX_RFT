using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomaticBrewingCoffee.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddTableNotification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    NotificationId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    NotificationType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ReferenceId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReferenceType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Severity = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsGlobal = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.NotificationId);
                });

            migrationBuilder.CreateTable(
                name: "NotificationRecipients",
                columns: table => new
                {
                    NotificationRecipientId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NotificationId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AccountRole = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AccountId = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationRecipients", x => x.NotificationRecipientId);
                    table.ForeignKey(
                        name: "FK_NotificationRecipients_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NotificationRecipients_Notifications_NotificationId",
                        column: x => x.NotificationId,
                        principalTable: "Notifications",
                        principalColumn: "NotificationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Accounts",
                keyColumn: "AccountId",
                keyValue: "herJ9VX5NEfyUWsDLc9MbbLH4603",
                column: "Email",
                value: "datsung.dev@gmail.com");

            migrationBuilder.UpdateData(
                table: "IngredientTypes",
                keyColumn: "IngredientTypeId",
                keyValue: "CF",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 25, 22, 38, 21, 885, DateTimeKind.Utc).AddTicks(4050));

            migrationBuilder.UpdateData(
                table: "IngredientTypes",
                keyColumn: "IngredientTypeId",
                keyValue: "CMK",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 25, 22, 38, 21, 885, DateTimeKind.Utc).AddTicks(4059));

            migrationBuilder.UpdateData(
                table: "IngredientTypes",
                keyColumn: "IngredientTypeId",
                keyValue: "CUP",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 25, 22, 38, 21, 885, DateTimeKind.Utc).AddTicks(4070));

            migrationBuilder.UpdateData(
                table: "IngredientTypes",
                keyColumn: "IngredientTypeId",
                keyValue: "ICE",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 25, 22, 38, 21, 885, DateTimeKind.Utc).AddTicks(4057));

            migrationBuilder.UpdateData(
                table: "IngredientTypes",
                keyColumn: "IngredientTypeId",
                keyValue: "MLK",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 25, 22, 38, 21, 885, DateTimeKind.Utc).AddTicks(4053));

            migrationBuilder.UpdateData(
                table: "IngredientTypes",
                keyColumn: "IngredientTypeId",
                keyValue: "SUG",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 25, 22, 38, 21, 885, DateTimeKind.Utc).AddTicks(4054));

            migrationBuilder.UpdateData(
                table: "IngredientTypes",
                keyColumn: "IngredientTypeId",
                keyValue: "WTR",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 25, 22, 38, 21, 885, DateTimeKind.Utc).AddTicks(4056));

            migrationBuilder.UpdateData(
                table: "SystemConfigs",
                keyColumn: "SystemConfigId",
                keyValue: "VAT",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 25, 22, 38, 21, 885, DateTimeKind.Utc).AddTicks(4032));

            migrationBuilder.CreateIndex(
                name: "IX_NotificationRecipients_AccountId",
                table: "NotificationRecipients",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationRecipients_NotificationId",
                table: "NotificationRecipients",
                column: "NotificationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NotificationRecipients");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.UpdateData(
                table: "Accounts",
                keyColumn: "AccountId",
                keyValue: "herJ9VX5NEfyUWsDLc9MbbLH4603",
                column: "Email",
                value: "admin@gmail.com");

            migrationBuilder.UpdateData(
                table: "IngredientTypes",
                keyColumn: "IngredientTypeId",
                keyValue: "CF",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 25, 9, 45, 59, 500, DateTimeKind.Utc).AddTicks(3434));

            migrationBuilder.UpdateData(
                table: "IngredientTypes",
                keyColumn: "IngredientTypeId",
                keyValue: "CMK",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 25, 9, 45, 59, 500, DateTimeKind.Utc).AddTicks(3445));

            migrationBuilder.UpdateData(
                table: "IngredientTypes",
                keyColumn: "IngredientTypeId",
                keyValue: "CUP",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 25, 9, 45, 59, 500, DateTimeKind.Utc).AddTicks(3456));

            migrationBuilder.UpdateData(
                table: "IngredientTypes",
                keyColumn: "IngredientTypeId",
                keyValue: "ICE",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 25, 9, 45, 59, 500, DateTimeKind.Utc).AddTicks(3443));

            migrationBuilder.UpdateData(
                table: "IngredientTypes",
                keyColumn: "IngredientTypeId",
                keyValue: "MLK",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 25, 9, 45, 59, 500, DateTimeKind.Utc).AddTicks(3438));

            migrationBuilder.UpdateData(
                table: "IngredientTypes",
                keyColumn: "IngredientTypeId",
                keyValue: "SUG",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 25, 9, 45, 59, 500, DateTimeKind.Utc).AddTicks(3440));

            migrationBuilder.UpdateData(
                table: "IngredientTypes",
                keyColumn: "IngredientTypeId",
                keyValue: "WTR",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 25, 9, 45, 59, 500, DateTimeKind.Utc).AddTicks(3441));

            migrationBuilder.UpdateData(
                table: "SystemConfigs",
                keyColumn: "SystemConfigId",
                keyValue: "VAT",
                column: "CreatedDate",
                value: new DateTime(2025, 7, 25, 9, 45, 59, 500, DateTimeKind.Utc).AddTicks(3422));
        }
    }
}
