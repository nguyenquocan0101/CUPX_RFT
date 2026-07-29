using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace Domain.Models;

public partial class AutoBrewingKioskBeContext : DbContext
{
    public AutoBrewingKioskBeContext()
    {
    }

    public AutoBrewingKioskBeContext(DbContextOptions<AutoBrewingKioskBeContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Device> Devices { get; set; }

    public virtual DbSet<DeviceLog> DeviceLogs { get; set; }

    public virtual DbSet<Ingredient> Ingredients { get; set; }

    public virtual DbSet<IngredientProductMapping> IngredientProductMappings { get; set; }

    public virtual DbSet<LocalOrder> LocalOrders { get; set; }

    public virtual DbSet<LocalOrderDetail> LocalOrderDetails { get; set; }

    public virtual DbSet<Menu> Menus { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<Step> Steps { get; set; }

    public virtual DbSet<Workflow> Workflows { get; set; }

//     protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
// #warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
//         => optionsBuilder.UseSqlServer("Data Source=MSI\\MSSQLSERVER01;Initial Catalog=AutoBrewing_Kiosk_BE;Persist Security Info=True;User ID=sa;Password=12345;Encrypt=False");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        OnModelCreatingPartial(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
