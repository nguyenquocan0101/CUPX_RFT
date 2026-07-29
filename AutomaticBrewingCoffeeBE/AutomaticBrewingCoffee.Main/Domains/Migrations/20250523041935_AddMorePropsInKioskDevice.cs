using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomaticBrewingCoffee.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddMorePropsInKioskDevice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AuthenticationInHub",
                table: "KioskDeviceMappings",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CloudToDeviceMessageCountInHub",
                table: "KioskDeviceMappings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ConnectionStateInHub",
                table: "KioskDeviceMappings",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ConnectionStateUpdatedTimeInHub",
                table: "KioskDeviceMappings",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastActivityTimeInHub",
                table: "KioskDeviceMappings",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StatusInHub",
                table: "KioskDeviceMappings",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StatusUpdatedTimeInHub",
                table: "KioskDeviceMappings",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AuthenticationInHub",
                table: "KioskDeviceMappings");

            migrationBuilder.DropColumn(
                name: "CloudToDeviceMessageCountInHub",
                table: "KioskDeviceMappings");

            migrationBuilder.DropColumn(
                name: "ConnectionStateInHub",
                table: "KioskDeviceMappings");

            migrationBuilder.DropColumn(
                name: "ConnectionStateUpdatedTimeInHub",
                table: "KioskDeviceMappings");

            migrationBuilder.DropColumn(
                name: "LastActivityTimeInHub",
                table: "KioskDeviceMappings");

            migrationBuilder.DropColumn(
                name: "StatusInHub",
                table: "KioskDeviceMappings");

            migrationBuilder.DropColumn(
                name: "StatusUpdatedTimeInHub",
                table: "KioskDeviceMappings");
        }
    }
}
