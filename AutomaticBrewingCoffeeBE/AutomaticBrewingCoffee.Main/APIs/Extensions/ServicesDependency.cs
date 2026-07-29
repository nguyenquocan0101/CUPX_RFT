using System.Text;
using AutomaticBrewingCoffee.Domain.Context;
using AutomaticBrewingCoffee.Repository.Implement;
using AutomaticBrewingCoffee.Repository.Interfaces;
using EFCoreSecondLevelCacheInterceptor;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;
using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Services.AzureIotHub;
using Services.BackgroundJobs;
using Services.CapRabbitMQ.Subscribers;
using Services.Cludflare;
using Services.Cludflare.OptionConfig;
using Services.Email;
using Services.Email.Base;
using Services.Firebase;
using Services.Implements;
using Services.Interceptors;
using Services.Interfaces;
using Services.MPOS;
using Services.Redis;
using Services.SignalR;
using Services.SignalR.Services;
using Services.Supabase;
using Services.Supabase.Base;
using Services.VNPay;
using StackExchange.Redis;
using Supabase;
using Supabase.Gotrue;
using Supabase.Interfaces;
using Supabase.Realtime;
using Supabase.Storage;
using Swashbuckle.AspNetCore.SwaggerGen;
using VNPAY.NET;

namespace AutomaticBrewingCoffee.API.Extensions;

public static class ServicesDependency
{
    #region AddUnitOfWork

    public static IServiceCollection AddUnitOfWork(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork<AutoBrewingBeContext>>();
        services.AddScoped<IUnitOfWork<AutoBrewingBeContext>, UnitOfWork<AutoBrewingBeContext>>();
        return services;
    }

    #endregion


    #region AddDatabase

    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var endpoint = configuration["Redis:DatabaseCache"]!;

        var redisConfig = new ConfigurationOptions()
        {
            EndPoints = { endpoint },
            AllowAdmin = true,
            ConnectTimeout = 10000,
            AbortOnConnectFail = false,
        };

        var redisAvailable = false;
        try
        {
            var muxer = ConnectionMultiplexer.Connect(redisConfig);
            redisAvailable = muxer.IsConnected;
            muxer.Dispose();
        }
        catch
        {
            redisAvailable = false;
        }

        services.AddEFSecondLevelCache(options =>
        {
            if (redisAvailable)
            {
                options.UseStackExchangeRedisCacheProvider(redisConfig, TimeSpan.FromMinutes(5));
            }
            else
            {
                options.UseMemoryCacheProvider();
            }

            options.ConfigureLogging(true)
                .CacheAllQueries(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(30));
        });

        services.AddScoped<AuditableEntitiesInterceptor>();
        services.AddDbContext<AutoBrewingBeContext>((sp, options) =>
            options
                .UseSqlServer(
                    CreateConnectionString(configuration),
                    sqlOptions => sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
                )
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                .AddInterceptors(
                    sp.GetRequiredService<AuditableEntitiesInterceptor>(),
                    sp.GetRequiredService<SecondLevelCacheInterceptor>()
                )
        );

        return services;
    }


    private static string CreateConnectionString(IConfiguration configuration)
    {
        var connectionString = configuration.GetValue<string>("ConnectionStrings:Db")!;
        return connectionString;
    }

    public static WebApplication ApplyMigration(this WebApplication app)
    {
        using (var scope = app.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AutoBrewingBeContext>();

            if (context.Database.GetPendingMigrations().Any())
            {
                context.Database.Migrate();
            }
        }

        return app;
    }

    #endregion


    #region AddServices

    public static void AddServices(this IServiceCollection services)
    {
        services.AddScoped(typeof(BaseService<>));
        services.AddSingleton<IRedisService, RedisService>();
        services.AddScoped<IFirebaseAuthService, FirebaseAuthService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IStoreService, StoreService>();
        services.AddScoped<IDeviceService, DeviceService>();
        services.AddScoped<IKioskService, KioskService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<ISupabaseStorageService, SupabaseStorageService>();
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IMenuService, MenuService>();
        services.AddScoped<IWorkflowService, WorkflowService>();
        services.AddScoped<ISyncService, SyncService>();
        services.AddScoped<IWebhookService, WebhookService>();
        services.AddScoped<IMasterDataService, MasterDataService>();
        services.AddScoped<IDeviceModelService, DeviceModelService>();
        services.AddScoped<IDeviceTypeService, DeviceTypeService>();
        services.AddScoped<IKioskTypeService, KioskTypeService>();
        services.AddScoped<IKioskVersionService, KioskVersionService>();
        services.AddScoped<IOrganizationService, OrganizationService>();
        services.AddScoped<ILocationTypeService, LocationTypeService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IProductCategoryService, ProductCategoryService>();
        services.AddScoped<IIngredientTypeService, IngredientTypeService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<TabletSignalService>();
        services.AddScoped<WebAppSignalService>();
    }

    #endregion


    #region AddJwtValidation

    public static IServiceCollection AddAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters()
            {
                ValidIssuer = configuration["JWT:Issuer"],
                ValidAudience = configuration["JWT:Audience"],
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(configuration["JWT:Key"]!)),
                RequireExpirationTime = true,
                ValidateLifetime = true
            };

            options.Events = new JwtBearerEvents()
            {
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];

                    // If the request is for our hub...
                    var path = context.HttpContext.Request.Path;
                    if (!string.IsNullOrEmpty(accessToken) &&
                        (path.StartsWithSegments("/hubs")))
                    {
                        // Read the token out of the query string
                        context.Token = accessToken;
                    }

                    return Task.CompletedTask;
                }
            };
            // options.Events = new JwtBearerEvents
            // {
            //     OnMessageReceived = async context =>
            //     {
            //         var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            //         if (string.IsNullOrEmpty(token))
            //         {
            //             context.Fail("UnAuthenticated");
            //             return;
            //         }
            //
            //         try
            //         {
            //             var firebaseAuth = context.HttpContext.RequestServices.GetRequiredService<FirebaseAuth>();
            //             var decodedToken = await firebaseAuth.VerifyIdTokenAsync(token);
            //
            //             if (decodedToken == null)
            //             {
            //                 context.Fail("UnAuthenticated");
            //                 return;
            //             }
            //
            //             var firebaseUser = await firebaseAuth.GetUserAsync(decodedToken.Uid);
            //             if (firebaseUser == null)
            //             {
            //                 context.Fail("UnAuthenticated");
            //                 return;
            //             }
            //
            //             // Get AccountService from DI Container
            //             var userService = context.HttpContext.RequestServices.GetRequiredService<IAccountService>();
            //
            //             //Check account
            //             var account = await userService.GetFirebaseUserAsync(firebaseUser.Uid);
            //             if (account is null)
            //             {
            //                 // create account 
            //                 // account = await userService.CreateViaFirebase(firebaseUser);
            //                 // throw new UnauthorizedAccessException();
            //             }
            //
            //             // Get claims
            //             var claims = decodedToken.Claims
            //                 .Select(c => new Claim(c.Key, c.Value.ToString()!))
            //                 .ToList();
            //
            //             // Add role to claims
            //             claims.Add(new Claim(ClaimTypes.Role, account!.RoleName));
            //
            //             context.Principal = new ClaimsPrincipal(
            //                 new ClaimsIdentity(claims, "Firebase"));
            //
            //             context.Success();
            //         }
            //         catch (UnauthorizedAccessException ex)
            //         {
            //             context.Fail($"Firebase authentication failed: {ex.Messages}");
            //         }
            //         catch
            //         {
            //             context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            //         }
            //     }
            // };
        });
        return services;
    }

    #endregion


    #region AddConfigSwagger

    public static IServiceCollection AddConfigSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo() { Title = "Automatic Brewing Coffee", Version = "v1" });
            options.EnableAnnotations();
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
            {
                In = ParameterLocation.Header,
                Description = "Please enter a valid token",
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                BearerFormat = "JWT",
                Scheme = "Bearer"
            });
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new string[] { }
                }
            });
            options.MapType<TimeOnly>(() => new OpenApiSchema
            {
                Type = "string",
                Format = "time",
                Example = OpenApiAnyFactory.CreateFromJson("\"13:45:42.0000000\"")
            });
        });
        return services;
    }

    #endregion


    #region AddRedis

    public static IServiceCollection AddRedis(this IServiceCollection services, IConfiguration configuration)
    {
        string connectionString =
            configuration.GetSection("Redis")["ConnectionString"]!;
        services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(connectionString));
        return services;
    }

    #endregion


    #region AddVNPay

    public static IServiceCollection AddVNPay(this IServiceCollection services, IConfiguration configuration)
    {
        var vnPay = new Vnpay();
        var vnPayOptions = configuration.GetSection("VNPay").Get<VNPayOptions>()!;
        vnPay.Initialize(
            tmnCode: vnPayOptions.TmnCode,
            hashSecret: vnPayOptions.HashSecret,
            baseUrl: vnPayOptions.BaseUrl,
            callbackUrl: vnPayOptions.CallbackUrl,
            version: vnPayOptions.Version,
            orderType: vnPayOptions.OrderType
        );

        services.AddSingleton<IVnpay>(vnPay);
        services.AddScoped<VNPayClient>();
        return services;
    }

    #endregion


    #region AddSupabase

    public static IServiceCollection AddSupabase(this IServiceCollection services, IConfiguration configuration)
    {
        var supabaseConfigure = configuration.GetSection("Supabase").Get<SupabaseConfigure>();

        var options = new SupabaseOptions
        {
            AutoConnectRealtime = true,
        };

        if (supabaseConfigure is null)
        {
            throw new NullReferenceException("SupabaseConfigure is null");
        }

        services.Configure<SupabaseConfigure>(opts =>
        {
            opts.Key = supabaseConfigure.Key;
            opts.Url = supabaseConfigure.Url;
        });

        var supabase = new Supabase.Client(supabaseConfigure.Url, supabaseConfigure.Key, options);
        var client = supabase.InitializeAsync().GetAwaiter().GetResult();

        services.AddSingleton<ISupabaseClient<User, Session, RealtimeSocket, RealtimeChannel, Bucket, FileObject>>(
            client);

        return services;
    }

    #endregion


    #region AddHangFire

    public static IServiceCollection AddAppHangFire(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHangfire(config =>
            config.UseSqlServerStorage(CreateConnectionString(configuration))
        );

        services.AddHangfireServer();

        // Register the job service
        services.AddScoped<PaymentExpiredJob>();
        services.AddScoped<SyncTaskSyncedJob>();
        services.AddScoped<SyncEventSyncedJob>();
        return services;
    }

    public static void RegisterRecurringJob()
    {
        RecurringJob.AddOrUpdate<PaymentExpiredJob>(
            "expire-payment-automatically",
            job => job.ExpirePaymentAutomatically(),
            Cron.Hourly
        );

        RecurringJob.AddOrUpdate<SyncTaskSyncedJob>(
            "sync-task-synced-automatically",
            job => job.SyncTaskSyncedAutomatically(),
            Cron.Hourly
        );

        RecurringJob.AddOrUpdate<SyncEventSyncedJob>(
            "sync-event-synced-automatically",
            job => job.SyncEventSyncedAutomatically(),
            Cron.Hourly
        );

        // RecurringJob.AddOrUpdate<KioskSynchronizedDataJob>(
        //     "sync-kiosk-menu-automatically",
        //     job => job.SyncKioskMenuAutomatically(),
        //     Cron.Daily
        // );
    }

    #endregion


    #region AddFirebase

    public static IServiceCollection AddFirebase(this IServiceCollection services, IConfiguration configuration)
    {
        var firebaseOptions = configuration.GetSection("Firebase").Get<FirebaseOptions>()!;
        var credentialPath = Path.Combine(Directory.GetCurrentDirectory(), firebaseOptions.Credential);

        if (FirebaseApp.DefaultInstance == null)
        {
            FirebaseApp.Create(new AppOptions
            {
                Credential = GoogleCredential.FromFile(credentialPath),
                ProjectId = firebaseOptions.ProjectId,
            });
        }

        // Register firebase service
        services.AddSingleton(firebaseOptions);
        services.AddSingleton(FirebaseAuth.DefaultInstance);

        return services;
    }

    #endregion


    #region AddMPOS

    public static IServiceCollection AddMPOSConfig(this IServiceCollection service, IConfiguration configuration)
    {
        service.Configure<MPOSMerchant>(configuration.GetSection("MPOS"));
        service.AddHttpClient<MPOSClient>(client =>
        {
            var url = configuration["MPOS:DevDomain"]!;
            client.BaseAddress = new Uri(url);
        });
        return service;
    }

    #endregion


    #region MapSignalRHub

    public static WebApplication MapSignalRHub(this WebApplication app)
    {
        app.MapHub<NotificationHub>("hubs/notification");
        app.MapHub<OrderHub>("hubs/order");
        return app;
    }

    #endregion


    #region Cloudflare

    public static void AddCloudflareConfig(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<CloudflareInfo>(configuration.GetSection("Cloudflare"));
        services.AddHttpClient<CloudflareApi>(client =>
        {
            var url = configuration["Cloudflare:BaseUrl"]!;
            client.BaseAddress = new Uri(url);
        });
    }

    #endregion


    #region Azure IOT HUB

    public static void AddAzureHub(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<DeviceManager>();
        services.AddSingleton<HostSender>();
    }

    #endregion


    #region Email

    public static void AddEmail(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(configuration.GetSection("SmtpSettings").Get<SmtpSettings>()!);
        services.AddSingleton<EmailTemplateHandler>();
        services.AddSingleton<EmailSender>();
    }

    #endregion Email


    #region AddSentry

    public static ConfigureWebHostBuilder AddSentry(this ConfigureWebHostBuilder webHostBuilder,
        IConfiguration configuration)
    {
        webHostBuilder.UseSentry(o =>
        {
            o.Dsn = configuration["Sentry:Dsn"] ?? "";
            o.MaxBreadcrumbs = int.TryParse(configuration["Sentry:MaxBreadcrumbs"], out var mb) ? mb : 100;
            o.Debug = bool.TryParse(configuration["Sentry:Debug"], out var dbg) && dbg;
            o.SendDefaultPii = bool.TryParse(configuration["Sentry:SendDefaultPii"], out var pii) && pii;
            o.TracesSampleRate = double.TryParse(configuration["Sentry:TracesSampleRate"], out var rate) ? rate : 0.1;
        });

        return webHostBuilder;
    }

    #endregion


    #region AddRabbitMQCap

    public static IServiceCollection AddRabbitMQCap(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCap(c =>
        {
            c.UseEntityFramework<AutoBrewingBeContext>();

            var rabbitSection = configuration.GetSection("RabbitMQ");
            c.UseRabbitMQ(opt =>
            {
                opt.HostName = rabbitSection["HostName"]!;
                opt.ExchangeName = rabbitSection["ExchangeName"]!;
                opt.UserName = rabbitSection["UserName"]!;
                opt.Password = rabbitSection["Password"]!;
                opt.Port = int.TryParse(rabbitSection["Port"], out var port) ? port : 5672;
                // Optional: opt.VirtualHost = rabbitSection["VirtualHost"];
            });
            // c.UseDashboard();
        });

        services.AddScoped<EmailCapSubscriber>();
        services.AddScoped<NotificationCapSubscriber>();
        services.AddScoped<PaymentCapSubscriber>();
        services.AddScoped<OrderCapSubscriber>();

        return services;
    }

    #endregion AddRabbitMQCap
}