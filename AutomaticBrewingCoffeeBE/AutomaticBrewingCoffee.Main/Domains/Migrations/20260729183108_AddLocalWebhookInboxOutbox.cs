using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomaticBrewingCoffee.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddLocalWebhookInboxOutbox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LocalWebhookInboxes",
                columns: table => new
                {
                    InboxId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Source = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EventId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PayloadHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    IdempotencyKey = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    StatusCode = table.Column<int>(type: "int", nullable: true),
                    AttemptCount = table.Column<int>(type: "int", nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastAttemptAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LeaseUntil = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastError = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocalWebhookInboxes", x => x.InboxId);
                });

            migrationBuilder.CreateTable(
                name: "LocalWebhookOutboxes",
                columns: table => new
                {
                    OutboxId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    InboxId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    TargetPath = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    PayloadJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    LastStatusCode = table.Column<int>(type: "int", nullable: true),
                    AttemptCount = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LeaseUntil = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastError = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocalWebhookOutboxes", x => x.OutboxId);
                    table.ForeignKey(
                        name: "FK_LocalWebhookOutboxes_LocalWebhookInboxes_InboxId",
                        column: x => x.InboxId,
                        principalTable: "LocalWebhookInboxes",
                        principalColumn: "InboxId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LocalWebhookInboxes_Source_EventType_EventId",
                table: "LocalWebhookInboxes",
                columns: new[] { "Source", "EventType", "EventId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LocalWebhookOutboxes_InboxId",
                table: "LocalWebhookOutboxes",
                column: "InboxId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LocalWebhookOutboxes");

            migrationBuilder.DropTable(
                name: "LocalWebhookInboxes");
        }
    }
}
