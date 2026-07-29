using System.Data.Common;
using AutomaticBrewingCoffee.API.Mappers;
using AutomaticBrewingCoffee.Domain.Context;
using AutomaticBrewingCoffee.Repository.Implement;
using AutomaticBrewingCoffee.Repository.Interfaces;
using DotNet.Testcontainers.Builders;
using DotNetCore.CAP;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Respawn;
using Services.AzureIotHub;
using Services.Firebase;
using Services.Implements;
using Services.Interfaces;
using Services.Local;
using Services.Redis;
using StackExchange.Redis;
using Testcontainers.MsSql;
using Testcontainers.Redis;

namespace Services.Tests.TestBase;

// ReSharper disable once ClassNeverInstantiated.Global
public class StartUpTestBase : IAsyncLifetime
{
    public AutoBrewingBeContext DbContext { get; private set; } = null!;
    public IServiceProvider ServiceProvider { get; private set; } = null!;

    private readonly MsSqlContainer _msSqlContainer;
    private readonly RedisContainer _redisContainer;
    private DbConnection _dbConnection = null!;
    private Respawner _respawner = null!;

    public StartUpTestBase()
    {
        _redisContainer = new RedisBuilder()
            .WithImage("redis:7.0")
            .WithPortBinding(6379, true)
            .WithCleanUp(true) // Clear the container after test
            .Build();

        _msSqlContainer = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2019-latest")
            .WithPassword("yourStrong(!)Password")
            .WithPortBinding(1433, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(1433))
            .WithCleanUp(true) // Clear the container after test
            .Build();

        // Configure Hangfire to use in-memory storage
        GlobalConfiguration.Configuration.UseMemoryStorage();
    }

    public async Task InitializeAsync()
    {
        await _redisContainer.StartAsync();

        await _msSqlContainer.StartAsync();

        var options = new DbContextOptionsBuilder<AutoBrewingBeContext>()
            .UseSqlServer(_msSqlContainer.GetConnectionString())
            .EnableSensitiveDataLogging()
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
            .Options;

        DbContext = new AutoBrewingBeContext(options);
        await DbContext.Database.MigrateAsync();

        var services = new ServiceCollection();

        // Register your services here
        IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();
        var localMode = configuration.GetValue<bool>("LOCAL_MODE")
            || string.Equals(configuration["ASPNETCORE_ENVIRONMENT"], "Local", StringComparison.OrdinalIgnoreCase);
        if (localMode && string.IsNullOrWhiteSpace(configuration["Jwt:Key"]))
        {
            configuration = new ConfigurationBuilder()
                .AddConfiguration(configuration)
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:Key"] = "cupx-local-test-jwt-key-2026-0001",
                    ["Jwt:Issuer"] = "AutoBrewing",
                    ["Jwt:Audience"] = "AutoBrewingUsers"
                })
                .Build();
        }
        var assemblies = new[]
        {
            typeof(DeviceMapper).Assembly,
            typeof(StoreMapper).Assembly,
            typeof(KioskDeviceMapper).Assembly,
            typeof(KioskMapper).Assembly,
            typeof(OrderDetailMapper).Assembly,
            typeof(OrderMapper).Assembly,
            typeof(PaginationMapper).Assembly,
            typeof(PaymentMapper).Assembly,
            typeof(ProductMapper).Assembly,
            typeof(WorkflowMapper).Assembly,
        };
        var firebaseOptions = configuration.GetSection("Firebase").Get<FirebaseOptions>()!;
        if (!localMode && FirebaseApp.DefaultInstance == null)
        {
            var credentialPath = Path.Combine(Directory.GetCurrentDirectory(), firebaseOptions.Credential);
            FirebaseApp.Create(new AppOptions
            {
                Credential = GoogleCredential.FromFile(credentialPath),
                ProjectId = firebaseOptions.ProjectId,
            });
        }
        // Register firebase service

        services.AddHttpContextAccessor();
        services.AddHttpClient();
        services.AddLogging();
        services.AddSingleton(DbContext);
        services.AddSingleton(firebaseOptions);
        services.AddSingleton<IConfiguration>(configuration);
        services.AddSingleton<ICapPublisher>(_ => new Mock<ICapPublisher>().Object);
        services.AddSingleton<DeviceManager>();
        services.AddSingleton<IConnectionMultiplexer>(sp =>
            ConnectionMultiplexer.Connect(_redisContainer.GetConnectionString()));
        services.AddAutoMapper(assemblies);


        services.AddScoped(typeof(BaseService<>));
        services.AddScoped<IUnitOfWork, UnitOfWork<AutoBrewingBeContext>>();
        services.AddScoped<IUnitOfWork<AutoBrewingBeContext>, UnitOfWork<AutoBrewingBeContext>>();
        services.AddScoped<IRedisService, RedisService>();
        if (localMode)
        {
            services.AddScoped<IFirebaseAuthService, DisabledFirebaseAuthService>();
        }
        else
        {
            services.AddSingleton(FirebaseAuth.DefaultInstance);
            services.AddScoped<IFirebaseAuthService, FirebaseAuthService>();
        }
        services.AddScoped<IAuthService, Implements.AuthService>();
        services.AddScoped<IDeviceService, Implements.DeviceService>();
        services.AddScoped<IStoreService, Implements.StoreService>();

        ServiceProvider = services.BuildServiceProvider();

        await InitializeRespawner();
    }

    private async Task InitializeRespawner()
    {
        _dbConnection = DbContext.Database.GetDbConnection();

        if (_dbConnection.State != System.Data.ConnectionState.Open)
        {
            await _dbConnection.OpenAsync();
        }

        _respawner = await Respawner.CreateAsync(
            _dbConnection,
            new RespawnerOptions
            {
                DbAdapter = DbAdapter.SqlServer,
                SchemasToInclude = ["dbo"]
            });
    }

    public async Task ResetDatabaseAsync()
    {
        await _respawner.ResetAsync(_dbConnection);
        DbContext.ChangeTracker.Clear();
    }

    public async Task DisposeAsync()
    {
        await _redisContainer.DisposeAsync();
        await _dbConnection.DisposeAsync();
        await DbContext.DisposeAsync();
        await _msSqlContainer.DisposeAsync();
    }
}
