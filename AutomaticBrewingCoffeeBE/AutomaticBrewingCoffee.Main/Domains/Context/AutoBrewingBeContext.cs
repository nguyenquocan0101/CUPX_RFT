using AutomaticBrewingCoffee.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace AutomaticBrewingCoffee.Domain.Context;

public partial class AutoBrewingBeContext : DbContext
{
    public AutoBrewingBeContext()
    {
    }

    public AutoBrewingBeContext(DbContextOptions<AutoBrewingBeContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Account> Accounts { get; set; }

    public virtual DbSet<Store> Stores { get; set; }

    public virtual DbSet<Organization> Organizations { get; set; }

    public virtual DbSet<Menu> Menus { get; set; }

    public virtual DbSet<MenuProductMapping> MenuProductMappings { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderDetail> OrderDetails { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductAttribute> ProductAttributes { get; set; }

    public virtual DbSet<AttributeOption> AttributeOptions { get; set; }

    public virtual DbSet<Step> Steps { get; set; }

    public virtual DbSet<Workflow> Workflows { get; set; }

    public virtual DbSet<SystemConfig> SystemConfigs { get; set; }

    public virtual DbSet<Webhook> Webhooks { get; set; }

    public virtual DbSet<SyncEvent> SyncEvents { get; set; }

    public virtual DbSet<SyncTask> SyncTasks { get; set; }

    public virtual DbSet<LocationType> LocationTypes { get; set; }

    public virtual DbSet<Kiosk> Kiosks { get; set; }

    public virtual DbSet<KioskType> KioskTypes { get; set; }

    public virtual DbSet<KioskVersion> KioskVersions { get; set; }

    public virtual DbSet<KioskDeviceMapping> KioskDeviceMappings { get; set; }

    public virtual DbSet<KioskVersionDeviceModelMapping> KioskVersionDeviceModelMappings { get; set; }

    public virtual DbSet<Device> Devices { get; set; }

    public virtual DbSet<DeviceModel> DeviceModels { get; set; }

    public virtual DbSet<DeviceType> DeviceTypes { get; set; }

    public virtual DbSet<KioskVersionProductMapping> KioskVersionProductMappings { get; set; }

    public virtual DbSet<DeviceFunction> DeviceFunctions { get; set; }

    public virtual DbSet<FunctionParameter> FunctionParameters { get; set; }

    public virtual DbSet<DeviceIngredient> DeviceIngredients { get; set; }

    public virtual DbSet<DeviceIngredientState> DeviceIngredientStates { get; set; }

    public virtual DbSet<IngredientType> IngredientTypes { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<NotificationRecipient> NotificationRecipients { get; set; }

    public virtual DbSet<DeviceIngredientHistory> DeviceIngredientHistories { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        OnModelCreatingPartial(modelBuilder);

        modelBuilder.Entity<Account>().HasQueryFilter(u => !u.IsDeleted);
        modelBuilder.Entity<Store>().HasQueryFilter(u => !u.IsDeleted);
        modelBuilder.Entity<Organization>().HasQueryFilter(u => !u.IsDeleted);
        modelBuilder.Entity<Menu>().HasQueryFilter(u => !u.IsDeleted);
        modelBuilder.Entity<MenuProductMapping>().HasQueryFilter(u => !u.IsDeleted);
        modelBuilder.Entity<Product>().HasQueryFilter(u => !u.IsDeleted);
        modelBuilder.Entity<SystemConfig>().HasQueryFilter(u => !u.IsDeleted);
        modelBuilder.Entity<Webhook>().HasQueryFilter(u => !u.IsDeleted);
        modelBuilder.Entity<SyncEvent>().HasQueryFilter(u => !u.IsDeleted);
        modelBuilder.Entity<SyncTask>().HasQueryFilter(u => !u.IsDeleted);
        modelBuilder.Entity<LocationType>().HasQueryFilter(u => !u.IsDeleted);
        modelBuilder.Entity<Kiosk>().HasQueryFilter(u => !u.IsDeleted);
        modelBuilder.Entity<KioskType>().HasQueryFilter(u => !u.IsDeleted);
        modelBuilder.Entity<KioskVersion>().HasQueryFilter(u => !u.IsDeleted);
        modelBuilder.Entity<KioskDeviceMapping>().HasQueryFilter(u => !u.IsDeleted);
        modelBuilder.Entity<KioskVersionDeviceModelMapping>().HasQueryFilter(u => !u.IsDeleted);
        modelBuilder.Entity<Device>().HasQueryFilter(u => !u.IsDeleted);
        modelBuilder.Entity<DeviceModel>().HasQueryFilter(u => !u.IsDeleted);
        modelBuilder.Entity<DeviceType>().HasQueryFilter(u => !u.IsDeleted);
        modelBuilder.Entity<KioskVersionProductMapping>().HasQueryFilter(u => !u.IsDeleted);
        modelBuilder.Entity<DeviceFunction>().HasQueryFilter(u => !u.IsDeleted);
        modelBuilder.Entity<FunctionParameter>().HasQueryFilter(u => !u.IsDeleted);
        modelBuilder.Entity<ProductAttribute>().HasQueryFilter(u => !u.IsDeleted);
        modelBuilder.Entity<AttributeOption>().HasQueryFilter(u => !u.IsDeleted);
        modelBuilder.Entity<ProductCategory>().HasQueryFilter(u => !u.IsDeleted);
        modelBuilder.Entity<DeviceIngredient>().HasQueryFilter(u => !u.IsDeleted);
        modelBuilder.Entity<DeviceIngredientState>().HasQueryFilter(u => !u.IsDeleted);
        modelBuilder.Entity<IngredientType>().HasQueryFilter(u => !u.IsDeleted);
        modelBuilder.Entity<Notification>().HasQueryFilter(u => !u.IsDeleted);
        modelBuilder.Entity<NotificationRecipient>().HasQueryFilter(u => !u.IsDeleted);
        modelBuilder.Entity<Order>().HasQueryFilter(u => !u.IsDeleted);
        modelBuilder.Entity<OrderDetail>().HasQueryFilter(u => !u.IsDeleted);
        modelBuilder.Entity<DeviceIngredientHistory>().HasQueryFilter(u => !u.IsDeleted);

        SeedUser(modelBuilder);
        SeedSystemConfig(modelBuilder);
        SeedIngredientType(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

    private static void SeedUser(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>().HasData(new Account()
            {
                AccountId = "herJ9VX5NEfyUWsDLc9MbbLH4603",
                Email = "datsung.dev@gmail.com",
                Password = "$2a$11$hXdz6sNcLtZ0Tr2x6gPqYuYPLVD9fgWBiQTIwkWzLeFmfzCtfqsHq",
                FullName = "Administrator",
                CreatedDate = new DateTime(2025, 4, 16, 11, 5, 56, 407, DateTimeKind.Utc).AddTicks(9733),
                RoleName = "Admin",
                Status = "Active"
            }
        );
    }

    private static void SeedSystemConfig(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SystemConfig>().HasData(new SystemConfig()
            {
                SystemConfigId = "VAT",
                Value = "0.1",
                Description = "VAT for order",
                CreatedDate = new DateTime(2025, 7, 25, 22, 38, 21, 885, DateTimeKind.Utc).AddTicks(4054)
            }
        );
    }

    private static void SeedIngredientType(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<IngredientType>().HasData(new List<IngredientType>
        {
            new IngredientType
            {
                IngredientTypeId = "CF",
                Name = "Cà phê",
                Description = "Nguyên liệu chính để pha chế đồ uống cà phê.",
                Status = "Active",
                CreatedDate = new DateTime(2025, 7, 25, 22, 38, 21, 885, DateTimeKind.Utc).AddTicks(4054)
            },
            new IngredientType
            {
                IngredientTypeId = "MLK",
                Name = "Sữa",
                Description = "Sữa tươi hoặc sữa pha để tăng vị béo.",
                Status = "Active",
                CreatedDate = new DateTime(2025, 7, 25, 22, 38, 21, 885, DateTimeKind.Utc).AddTicks(4054)
            },
            new IngredientType
            {
                IngredientTypeId = "SUG",
                Name = "Đường",
                Description = "Đường trắng dùng để tạo vị ngọt.",
                Status = "Active",
                CreatedDate = new DateTime(2025, 7, 25, 22, 38, 21, 885, DateTimeKind.Utc).AddTicks(4054)
            },
            new IngredientType
            {
                IngredientTypeId = "WTR",
                Name = "Nước",
                Description = "Nước lọc dùng để pha chế.",
                Status = "Active",
                CreatedDate = new DateTime(2025, 7, 25, 22, 38, 21, 885, DateTimeKind.Utc).AddTicks(4054)
            },
            new IngredientType
            {
                IngredientTypeId = "ICE",
                Name = "Đá lạnh",
                Description = "Đá viên để làm lạnh đồ uống.",
                Status = "Active",
                CreatedDate = new DateTime(2025, 7, 25, 22, 38, 21, 885, DateTimeKind.Utc).AddTicks(4054)
            },
            new IngredientType
            {
                IngredientTypeId = "CMK",
                Name = "Sữa đặc",
                Description = "Sữa đặc có đường, tạo độ ngọt và béo.",
                Status = "Active",
                CreatedDate = new DateTime(2025, 7, 25, 22, 38, 21, 885, DateTimeKind.Utc).AddTicks(4054)
            },
            new IngredientType
            {
                IngredientTypeId = "CUP",
                Name = "Cốc",
                Description = "Cốc đựng đồ uống phục vụ cho khách.",
                Status = "Active",
                CreatedDate = new DateTime(2025, 7, 25, 22, 38, 21, 885, DateTimeKind.Utc).AddTicks(4054)
            }
        });
    }
}