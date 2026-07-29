using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Domain.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Devices",
                columns: table => new
                {
                    DeviceId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DeviceModelId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SerialNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Status = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Devices", x => x.DeviceId);
                });

            migrationBuilder.CreateTable(
                name: "Ingredients",
                columns: table => new
                {
                    IngredientId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Unit = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    LastSync = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ingredients", x => x.IngredientId);
                });

            migrationBuilder.CreateTable(
                name: "LocalOrders",
                columns: table => new
                {
                    OrderId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    OrderData = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsSynced = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocalOrders", x => x.OrderId);
                });

            migrationBuilder.CreateTable(
                name: "Menus",
                columns: table => new
                {
                    MenuId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Status = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Menus", x => x.MenuId);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    ProductId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ParentId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Price = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    ImageUrl = table.Column<string>(type: "text", nullable: false),
                    Size = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    Type = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    LastSync = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.ProductId);
                    table.ForeignKey(
                        name: "FK_Products_Products_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Products",
                        principalColumn: "ProductId");
                });

            migrationBuilder.CreateTable(
                name: "DeviceLogs",
                columns: table => new
                {
                    LogId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DeviceId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LogKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LogValue = table.Column<string>(type: "text", nullable: false),
                    LogType = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Response = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceLogs", x => x.LogId);
                    table.ForeignKey(
                        name: "FK_DeviceLogs_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "DeviceId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LocalOrderDetails",
                columns: table => new
                {
                    OrderDetailId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    OrderId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ProductName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Size = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    SellingPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    DetailData = table.Column<string>(type: "text", nullable: false),
                    IsSynced = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocalOrderDetails", x => x.OrderDetailId);
                    table.ForeignKey(
                        name: "FK_LocalOrderDetails_LocalOrders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "LocalOrders",
                        principalColumn: "OrderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IngredientProductMappings",
                columns: table => new
                {
                    IngredientId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ProductId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Quantity = table.Column<double>(type: "double precision", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IngredientProductMappings", x => new { x.IngredientId, x.ProductId });
                    table.ForeignKey(
                        name: "FK_IngredientProductMappings_Ingredients_IngredientId",
                        column: x => x.IngredientId,
                        principalTable: "Ingredients",
                        principalColumn: "IngredientId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IngredientProductMappings_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MenuProductMapping",
                columns: table => new
                {
                    MenuId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ProductId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: true),
                    Status = table.Column<int>(type: "integer", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuProductMapping", x => new { x.MenuId, x.ProductId });
                    table.ForeignKey(
                        name: "FK_MenuProductMapping_Menus_MenuId",
                        column: x => x.MenuId,
                        principalTable: "Menus",
                        principalColumn: "MenuId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MenuProductMapping_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Workflows",
                columns: table => new
                {
                    WorkflowId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ProductId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Workflows", x => x.WorkflowId);
                    table.ForeignKey(
                        name: "FK_Workflows_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "ProductId");
                });

            migrationBuilder.CreateTable(
                name: "Steps",
                columns: table => new
                {
                    StepId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    WorkflowId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Function = table.Column<string>(type: "text", nullable: false),
                    DeviceModelId = table.Column<string>(type: "text", nullable: false),
                    Sequence = table.Column<int>(type: "integer", nullable: false),
                    MaxRetries = table.Column<int>(type: "integer", nullable: true),
                    CallbackWorkflowId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Parameters = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Steps", x => x.StepId);
                    table.ForeignKey(
                        name: "FK_Steps_Workflows_WorkflowId",
                        column: x => x.WorkflowId,
                        principalTable: "Workflows",
                        principalColumn: "WorkflowId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceLogs_DeviceId",
                table: "DeviceLogs",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_IngredientProductMappings_ProductId",
                table: "IngredientProductMappings",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalOrderDetails_OrderId",
                table: "LocalOrderDetails",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_MenuProductMapping_ProductId",
                table: "MenuProductMapping",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_ParentId",
                table: "Products",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Steps_WorkflowId",
                table: "Steps",
                column: "WorkflowId");

            migrationBuilder.CreateIndex(
                name: "IX_Workflows_ProductId",
                table: "Workflows",
                column: "ProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeviceLogs");

            migrationBuilder.DropTable(
                name: "IngredientProductMappings");

            migrationBuilder.DropTable(
                name: "LocalOrderDetails");

            migrationBuilder.DropTable(
                name: "MenuProductMapping");

            migrationBuilder.DropTable(
                name: "Steps");

            migrationBuilder.DropTable(
                name: "Devices");

            migrationBuilder.DropTable(
                name: "Ingredients");

            migrationBuilder.DropTable(
                name: "LocalOrders");

            migrationBuilder.DropTable(
                name: "Menus");

            migrationBuilder.DropTable(
                name: "Workflows");

            migrationBuilder.DropTable(
                name: "Products");
        }
    }
}
