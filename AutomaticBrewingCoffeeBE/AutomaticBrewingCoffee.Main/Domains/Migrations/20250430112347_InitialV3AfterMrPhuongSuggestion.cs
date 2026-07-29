using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomaticBrewingCoffee.Domain.Migrations
{
    /// <inheritdoc />
    public partial class InitialV3AfterMrPhuongSuggestion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    AccountId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Password = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    RoleName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ReferenceId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.AccountId);
                });

            migrationBuilder.CreateTable(
                name: "DeviceTypes",
                columns: table => new
                {
                    DeviceTypeId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceTypes", x => x.DeviceTypeId);
                });

            migrationBuilder.CreateTable(
                name: "Ingredients",
                columns: table => new
                {
                    IngredientId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ingredients", x => x.IngredientId);
                });

            migrationBuilder.CreateTable(
                name: "KioskTypes",
                columns: table => new
                {
                    KioskTypeId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KioskTypes", x => x.KioskTypeId);
                });

            migrationBuilder.CreateTable(
                name: "LocationTypes",
                columns: table => new
                {
                    LocationTypeId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationTypes", x => x.LocationTypeId);
                });

            migrationBuilder.CreateTable(
                name: "Menus",
                columns: table => new
                {
                    MenuId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    KioskId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Menus", x => x.MenuId);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    OrderId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Discount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    FeeAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FeeDescription = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FinalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    InvoiceId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    OrderType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PaymentGateway = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SessionId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    LastUpdateBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Vat = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    VatAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.OrderId);
                });

            migrationBuilder.CreateTable(
                name: "Organizations",
                columns: table => new
                {
                    OrganizationId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ContactPhone = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ContactEmail = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LogoUrl = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    TaxId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organizations", x => x.OrganizationId);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    ProductId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ParentId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Size = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Type = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
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
                name: "Role",
                columns: table => new
                {
                    RoleId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Role", x => x.RoleId);
                });

            migrationBuilder.CreateTable(
                name: "SyncEvents",
                columns: table => new
                {
                    SyncEventId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SyncEventType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SecondEntityId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyncEvents", x => x.SyncEventId);
                });

            migrationBuilder.CreateTable(
                name: "SystemConfigs",
                columns: table => new
                {
                    SystemConfigId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemConfigs", x => x.SystemConfigId);
                });

            migrationBuilder.CreateTable(
                name: "Webhooks",
                columns: table => new
                {
                    WebhookId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    KioskId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    WebhookUrl = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    WebhookType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Webhooks", x => x.WebhookId);
                });

            migrationBuilder.CreateTable(
                name: "DeviceModels",
                columns: table => new
                {
                    DeviceModelId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ModelName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Manufacturer = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    DeviceTypeId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceModels", x => x.DeviceModelId);
                    table.ForeignKey(
                        name: "FK_DeviceModels_DeviceTypes_DeviceTypeId",
                        column: x => x.DeviceTypeId,
                        principalTable: "DeviceTypes",
                        principalColumn: "DeviceTypeId");
                });

            migrationBuilder.CreateTable(
                name: "KioskVersions",
                columns: table => new
                {
                    KioskVersionId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    KioskTypeId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    VersionTitle = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    VersionNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KioskVersions", x => x.KioskVersionId);
                    table.ForeignKey(
                        name: "FK_KioskVersions_KioskTypes_KioskTypeId",
                        column: x => x.KioskTypeId,
                        principalTable: "KioskTypes",
                        principalColumn: "KioskTypeId");
                });

            migrationBuilder.CreateTable(
                name: "OrderDetails",
                columns: table => new
                {
                    OrderDetailId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OrderId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SellingPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ProductName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ProductDescription = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderDetails", x => x.OrderDetailId);
                    table.ForeignKey(
                        name: "FK_OrderDetails_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "OrderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    PaymentId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OrderId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PaymentContent = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    RequiredAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpiredDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReferenceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PaidAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PaymentStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreateBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdateBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LastUpdateAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.PaymentId);
                    table.ForeignKey(
                        name: "FK_Payments_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "OrderId");
                });

            migrationBuilder.CreateTable(
                name: "Stores",
                columns: table => new
                {
                    StoreId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OrganizationId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ContactPhone = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    LocationAddress = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    LocationTypeId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stores", x => x.StoreId);
                    table.ForeignKey(
                        name: "FK_Stores_LocationTypes_LocationTypeId",
                        column: x => x.LocationTypeId,
                        principalTable: "LocationTypes",
                        principalColumn: "LocationTypeId");
                    table.ForeignKey(
                        name: "FK_Stores_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "OrganizationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IngredientProductMappings",
                columns: table => new
                {
                    IngredientId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ProductId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Quantity = table.Column<double>(type: "float", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
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
                name: "MenuProductMappings",
                columns: table => new
                {
                    MenuId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ProductId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuProductMappings", x => new { x.MenuId, x.ProductId });
                    table.ForeignKey(
                        name: "FK_MenuProductMappings_Menus_MenuId",
                        column: x => x.MenuId,
                        principalTable: "Menus",
                        principalColumn: "MenuId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MenuProductMappings_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Workflows",
                columns: table => new
                {
                    WorkflowId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ProductId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
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
                name: "SyncTasks",
                columns: table => new
                {
                    SyncTaskId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SyncEventId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    KioskId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsSynced = table.Column<bool>(type: "bit", nullable: false),
                    SyncedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyncTasks", x => x.SyncTaskId);
                    table.ForeignKey(
                        name: "FK_SyncTasks_SyncEvents_SyncEventId",
                        column: x => x.SyncEventId,
                        principalTable: "SyncEvents",
                        principalColumn: "SyncEventId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Devices",
                columns: table => new
                {
                    DeviceId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DeviceModelId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SerialNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Devices", x => x.DeviceId);
                    table.ForeignKey(
                        name: "FK_Devices_DeviceModels_DeviceModelId",
                        column: x => x.DeviceModelId,
                        principalTable: "DeviceModels",
                        principalColumn: "DeviceModelId");
                });

            migrationBuilder.CreateTable(
                name: "KioskVersionDeviceModelMappings",
                columns: table => new
                {
                    KioskVersionId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DeviceModelId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KioskVersionDeviceModelMappings", x => new { x.KioskVersionId, x.DeviceModelId });
                    table.ForeignKey(
                        name: "FK_KioskVersionDeviceModelMappings_DeviceModels_DeviceModelId",
                        column: x => x.DeviceModelId,
                        principalTable: "DeviceModels",
                        principalColumn: "DeviceModelId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_KioskVersionDeviceModelMappings_KioskVersions_KioskVersionId",
                        column: x => x.KioskVersionId,
                        principalTable: "KioskVersions",
                        principalColumn: "KioskVersionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Kiosks",
                columns: table => new
                {
                    KioskId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    KioskVersionId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Position = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    WarrantyTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StoreId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Location = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    InstalledDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kiosks", x => x.KioskId);
                    table.ForeignKey(
                        name: "FK_Kiosks_KioskVersions_KioskVersionId",
                        column: x => x.KioskVersionId,
                        principalTable: "KioskVersions",
                        principalColumn: "KioskVersionId");
                    table.ForeignKey(
                        name: "FK_Kiosks_Stores_StoreId",
                        column: x => x.StoreId,
                        principalTable: "Stores",
                        principalColumn: "StoreId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Steps",
                columns: table => new
                {
                    StepId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    WorkflowId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DeviceType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    MaxRetries = table.Column<int>(type: "int", nullable: true),
                    CallbackWorkflowId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Parameters = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Steps", x => x.StepId);
                    table.ForeignKey(
                        name: "FK_Steps_Workflows_WorkflowId",
                        column: x => x.WorkflowId,
                        principalTable: "Workflows",
                        principalColumn: "WorkflowId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KioskDeviceMappings",
                columns: table => new
                {
                    KioskDeviceMappingId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DeviceId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    KioskId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KioskDeviceMappings", x => x.KioskDeviceMappingId);
                    table.ForeignKey(
                        name: "FK_KioskDeviceMappings_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "DeviceId");
                    table.ForeignKey(
                        name: "FK_KioskDeviceMappings_Kiosks_KioskId",
                        column: x => x.KioskId,
                        principalTable: "Kiosks",
                        principalColumn: "KioskId");
                });

            migrationBuilder.InsertData(
                table: "Accounts",
                columns: new[] { "AccountId", "CreatedDate", "DeletedDate", "Email", "FullName", "IsDeleted", "Password", "ReferenceId", "RoleName", "Status", "UpdatedDate" },
                values: new object[] { "herJ9VX5NEfyUWsDLc9MbbLH4603", new DateTime(2025, 4, 16, 11, 5, 56, 407, DateTimeKind.Utc).AddTicks(9733), null, "admin@gmail.com", "Administrator", false, "$2a$11$hXdz6sNcLtZ0Tr2x6gPqYuYPLVD9fgWBiQTIwkWzLeFmfzCtfqsHq", null, "Admin", "Active", null });

            migrationBuilder.InsertData(
                table: "Role",
                columns: new[] { "RoleId", "CreatedDate", "DeletedDate", "IsDeleted", "Name", "UpdatedDate" },
                values: new object[] { "A281950A-E25E-40A6-A773-C2FB4E12FA43", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, false, "Admin", null });

            migrationBuilder.InsertData(
                table: "SystemConfigs",
                columns: new[] { "SystemConfigId", "CreatedDate", "DeletedDate", "Description", "IsDeleted", "UpdatedDate", "Value" },
                values: new object[] { "VAT", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "VAT for order", false, null, "0.1" });

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Email",
                table: "Accounts",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeviceModels_DeviceTypeId",
                table: "DeviceModels",
                column: "DeviceTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Devices_DeviceModelId",
                table: "Devices",
                column: "DeviceModelId");

            migrationBuilder.CreateIndex(
                name: "IX_IngredientProductMappings_ProductId",
                table: "IngredientProductMappings",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_KioskDeviceMappings_DeviceId",
                table: "KioskDeviceMappings",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_KioskDeviceMappings_KioskId",
                table: "KioskDeviceMappings",
                column: "KioskId");

            migrationBuilder.CreateIndex(
                name: "IX_Kiosks_KioskVersionId",
                table: "Kiosks",
                column: "KioskVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_Kiosks_StoreId",
                table: "Kiosks",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_KioskVersionDeviceModelMappings_DeviceModelId",
                table: "KioskVersionDeviceModelMappings",
                column: "DeviceModelId");

            migrationBuilder.CreateIndex(
                name: "IX_KioskVersions_KioskTypeId",
                table: "KioskVersions",
                column: "KioskTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_MenuProductMappings_ProductId",
                table: "MenuProductMappings",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetails_OrderId",
                table: "OrderDetails",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_OrderId",
                table: "Payments",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_ParentId",
                table: "Products",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Steps_WorkflowId",
                table: "Steps",
                column: "WorkflowId");

            migrationBuilder.CreateIndex(
                name: "IX_Stores_LocationTypeId",
                table: "Stores",
                column: "LocationTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Stores_OrganizationId",
                table: "Stores",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_SyncTasks_SyncEventId",
                table: "SyncTasks",
                column: "SyncEventId");

            migrationBuilder.CreateIndex(
                name: "IX_Workflows_ProductId",
                table: "Workflows",
                column: "ProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Accounts");

            migrationBuilder.DropTable(
                name: "IngredientProductMappings");

            migrationBuilder.DropTable(
                name: "KioskDeviceMappings");

            migrationBuilder.DropTable(
                name: "KioskVersionDeviceModelMappings");

            migrationBuilder.DropTable(
                name: "MenuProductMappings");

            migrationBuilder.DropTable(
                name: "OrderDetails");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "Role");

            migrationBuilder.DropTable(
                name: "Steps");

            migrationBuilder.DropTable(
                name: "SyncTasks");

            migrationBuilder.DropTable(
                name: "SystemConfigs");

            migrationBuilder.DropTable(
                name: "Webhooks");

            migrationBuilder.DropTable(
                name: "Ingredients");

            migrationBuilder.DropTable(
                name: "Devices");

            migrationBuilder.DropTable(
                name: "Kiosks");

            migrationBuilder.DropTable(
                name: "Menus");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "Workflows");

            migrationBuilder.DropTable(
                name: "SyncEvents");

            migrationBuilder.DropTable(
                name: "DeviceModels");

            migrationBuilder.DropTable(
                name: "KioskVersions");

            migrationBuilder.DropTable(
                name: "Stores");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "DeviceTypes");

            migrationBuilder.DropTable(
                name: "KioskTypes");

            migrationBuilder.DropTable(
                name: "LocationTypes");

            migrationBuilder.DropTable(
                name: "Organizations");
        }
    }
}
