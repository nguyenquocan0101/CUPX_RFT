using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomaticBrewingCoffee.Domain.Migrations
{
    /// <inheritdoc />
    public partial class RelationWithKioskInsideSyncTask : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_SyncTasks_KioskId",
                table: "SyncTasks",
                column: "KioskId");

            migrationBuilder.AddForeignKey(
                name: "FK_SyncTasks_Kiosks_KioskId",
                table: "SyncTasks",
                column: "KioskId",
                principalTable: "Kiosks",
                principalColumn: "KioskId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SyncTasks_Kiosks_KioskId",
                table: "SyncTasks");

            migrationBuilder.DropIndex(
                name: "IX_SyncTasks_KioskId",
                table: "SyncTasks");
        }
    }
}
