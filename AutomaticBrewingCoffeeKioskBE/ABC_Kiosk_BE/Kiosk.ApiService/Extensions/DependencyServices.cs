using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Repositories.Implement;
using Repositories.Interfaces;
using Services.Implements;
using Services.Interfaces;
using MassTransit;
using Kiosk.ApiService.Saga.StateMachines;
using Kiosk.ApiService.Saga.StateMachineInstances;
using Services.WorkflowEngine;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Kiosk.ApiService.Saga.Contracts;
using Services.ExternalClients;
using Serilog;
using Kiosk.ApiService.Middleware;
using McMaster.NETCore.Plugins;
using System.Data;
using Services.Background;
using Services.Dtos.DeviceMonitoring;
using Services.WebSockets;
using Domain.CouchDbModels;
using Services.StrategyPattern.Sync;
using Kiosk.ApiService.Consumers;
using RabbitMQ.Client;
using Shared.MessageStore;
using StackExchange.Redis;
using Repositories.CouchDbRepository;
using CouchDB.Driver.DependencyInjection;


namespace Kiosk.ApiService.Extensions
{
    public static class DependencyServices
    {
        public static IServiceCollection AddUnitOfWork(this IServiceCollection services)
        {
            services.AddScoped<IUnitOfWork, UnitOfWork<AutoBrewingKioskBeContext>>();
            return services;
        }

        public static IServiceCollection AddMapperConfig(this IServiceCollection services)
        {
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            return services;
        }

        private static void AddCouchRepositories(this IServiceCollection services)
        {
            services.AddScoped<IWorkflowDataRepository, WorkflowDataRepository>();
            services.AddScoped<IDeviceDocumentRepository, DeviceDocumentRepository>();
            services.AddScoped<IDeviceStatusRepository, DeviceStatusRepository>();

        }

        public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            //services.AddDbContext<AutoBrewingKioskBeContext>(options =>
            //    options.UseNpgsql(CreateConnectionString(configuration)));


            services.AddCouchContext<KioskDbContext>(builder => builder
                .UseEndpoint(configuration["CouchDB:Url"]!)
                .UseBasicAuthentication(username: configuration["CouchDB:Username"]!, password: configuration["CouchDB:Pwd"]!));
            services.AddCouchRepositories();
            return services;
        }

        private static string CreateConnectionString(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("Db")!;
            return connectionString;
        }

        public static IServiceCollection AddAppServices(this IServiceCollection services, IConfiguration config)
        {
            services.AddHttpClient<CloudClient>(httpClient =>
            {
                httpClient.BaseAddress = new Uri(config["CloudConfig:BaseUrl"]!);

            });
            //services.AddScoped<IDeviceService, DeviceService>();
            //services.AddScoped<IOrderService, OrderService>();
            //services.AddScoped<IProductService, ProductService>();
            //services.AddScoped<IWorkflowService, WorkflowService>();
            //services.AddScoped<IMenuService, MenuService>();
            services.AddScoped<IKioskSyncService, KioskSyncService>();
            services.AddScoped<IWorkflowService2, WorkflowService2>();
            services.AddScoped<IDeviceService2, DeviceService2>();
            services.AddScoped<IOrderCacheService, OrderCacheService>();

            services.AddTransient<IApiKeyValidatorService, ApiKeyValidatorService>();

            services.AddKeyedScoped<ISyncStrategy<DeviceDocument>, CouchDbSyncStrategy<DeviceDocument>>(typeof(DeviceDocument).Name);
            services.AddKeyedScoped<ISyncStrategy<DeviceStatusDocument>, CouchDbSyncStrategy<DeviceStatusDocument>>(typeof(DeviceStatusDocument).Name);
            services.AddKeyedScoped<ISyncStrategy<Workflow>, FileSyncStrategy<Workflow>>(typeof(Workflow).Name);

            return services;
        }

        public static IServiceCollection AddConfigSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo() { Title = "Kiosk System", Version = "v1" });
                options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme()
                {
                    In = ParameterLocation.Header,
                    Name = "X-API-KEY",
                    Type = SecuritySchemeType.ApiKey,
                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "ApiKey" //phải trùng với name ở security definition
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });
            return services;
        }

        //private static IServiceCollection AddDevices(this IServiceCollection services, IConfiguration configuration)
        //{

        //    services.AddSingleton(sp =>
        //    {
        //        var portName = configuration["SerialPorts:CupDroppingPort"]!;
        //        var baudRate = int.Parse(configuration["SerialPorts:BaudRate"]!);
        //        return new CupDroppingMachine(portName, baudRate);
        //    });
        //    services.AddSingleton(sp =>
        //    {
        //        var portName = configuration["SerialPorts:CoffeePort"]!;
        //        var baudRate = int.Parse(configuration["SerialPorts:BaudRate"]!);
        //        return new CoffeeMachine(portName, baudRate);
        //    });
        //    services.AddSingleton(sp =>
        //    {
        //        var portName = configuration["SerialPorts:IcePort"]!;
        //        var baudRate = int.Parse(configuration["SerialPorts:BaudRate"]!);
        //        return new IceMachine(portName, baudRate);
        //    });
        //    services.AddSingleton(sp =>
        //    {
        //        var portName = configuration["SerialPorts:ArmPort"]!;
        //        var baudRate = int.Parse(configuration["SerialPorts:BaudRate"]!);
        //        return new RoboticArm(portName, baudRate);
        //    });
        //    return services;
        //}

        #region WorkflowEngine 
        public static IServiceCollection AddWorkflowEngineConfig(this IServiceCollection services, IConfiguration configuration)
        {
            //services.AddDevices(configuration)
            //    .AddMassTransitConfig(configuration)
            //    .AddStepCommandExecutor();
            services.AddMassTransitConfig(configuration).AddStepCommandExecutor();
            services.LoadOutsideDLLCommands(configuration);
            return services;
        }

        private static void LoadOutsideDLLCommands(this IServiceCollection services, IConfiguration configuration)
        {
            var pluginDirectoryPath = configuration["DDLSourceFolder"]!;
            var loaders = new List<PluginLoader>();
            var baseDir = AppContext.BaseDirectory;
            var pluginDir = Path.Combine(baseDir, pluginDirectoryPath);

            if (!Directory.Exists(pluginDir))
            {
                return;
            }

            foreach (var dir in Directory.GetFiles(pluginDir))
            {
                var dirName = Path.GetFileName(dir);
                if (File.Exists(dir))
                {
                    var loader = PluginLoader.CreateFromAssemblyFile(
                        dir,
                        sharedTypes: new[] { typeof(IStepCommand), typeof(StepTypeAttribute) } //same type with host
                        );
                    loaders.Add(loader);
                }
            }

            //create instance of plugin types
            if (loaders.Count != 0)
            {
                foreach (var loader in loaders)
                {
                    var assembly = loader.LoadDefaultAssembly();
                    Log.Information($"Import DDL Command.Executing {0}", assembly.GetName());
                    var cmdTypes = assembly.GetTypes().Where(t => typeof(IStepCommand).IsAssignableFrom(t) && !t.IsAbstract);
                    //add scope for commands in plugin dll
                    foreach (var cmdType in cmdTypes)
                    {
                        services.AddScoped(cmdType);
                    }

                }
            }
        }

        private static IServiceCollection AddStepCommandExecutor(this IServiceCollection services)
        {
            services.AddScoped<StepCommandFactory>();
            services.AddScoped<WorkflowExecutor>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            var commandTypes = assemblies
                .SelectMany(a => a.GetTypes())
                .Where(t => typeof(IStepCommand).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

            foreach (var cmdType in commandTypes)
            {
                services.AddScoped(cmdType);
            }

            return services;
        }

        private static IServiceCollection AddMassTransitConfig(this IServiceCollection services, IConfiguration configuration)
        {
            services.TryAddSingleton(KebabCaseEndpointNameFormatter.Instance);

            services.AddMassTransit(x =>
            {
                x.SetEndpointNameFormatter(KebabCaseEndpointNameFormatter.Instance);

                //DO NOT REMOVE THIS LINE
                x.AddConsumers(typeof(Program).Assembly);

                x.AddSagaStateMachine<OrderPreparingStateMachine, OrderPreparingState>()
                    .InMemoryRepository();
                x.AddSagaStateMachine<CoffeeMakingStateMachine, CoffeeMakingState>()
                    .InMemoryRepository();

                //register client
                x.AddRequestClient<PrepareOrder>(new Uri("queue:order-prepare"));
                x.AddRequestClient<CreateOrder>(new Uri("queue:order-prepare"));
                x.AddRequestClient<CreatePayment>(new Uri("queue:order-prepare"));
                x.AddRequestClient<DoWorkflow>(new Uri("queue:workflow"), timeout: TimeSpan.FromMinutes(1));
                x.AddRequestClient<CompleteOrder>(new Uri("queue:workflow"));
                x.AddRequestClient<FailOrder>(new Uri("queue:workflow"));
                //x.UsingRabbitMq((context, cfg) =>
                //{

                //    var host = configuration["RabbitMQ:Host"] ?? "localhost";
                //    var port = configuration["RabbitMQ:Port"] ?? "5672";
                //    var username = configuration["RabbitMQ:Username"] ?? "guest";
                //    var password = configuration["RabbitMQ:Password"] ?? "guest";

                //    cfg.Host(host, "/", h =>
                //    {
                //        h.Username(username);
                //        h.Password(password);
                //    });

                //    cfg.ReceiveEndpoint("order-prepare", e =>
                //    {
                //        e.PrefetchCount = 2;
                //        e.PurgeOnStartup = true;
                //        e.AutoDelete = false;
                //        e.Durable = true;
                //        e.ConfigureConsumer<CreateOrderConsumer>(context);
                //        e.ConfigureConsumer<CreatePaymentConsumer>(context);
                //        e.ConfigureSaga<OrderPreparingState>(context);
                //        e.SetExchangeArgument("x-max-length", 2);
                //        e.SetExchangeArgument("x-overflow", "reject-publish");
                //    });
                //    cfg.ReceiveEndpoint("workflow", e =>
                //    {
                //        e.PrefetchCount = 1;
                //        e.PurgeOnStartup = true;
                //        e.AutoDelete = false;
                //        e.Durable = true;
                //        e.ConfigureConsumer<DoWorkflowConsumer>(context);
                //        e.ConfigureConsumer<CompleteOrderConsumer>(context);
                //        e.ConfigureConsumer<FailOrderConsumer>(context);
                //        e.ConfigureSaga<CoffeeMakingState>(context);
                //    });

                //    cfg.ReceiveEndpoint("order-paid", e =>
                //    {
                //        e.PurgeOnStartup = true;
                //        e.AutoDelete = false;
                //        e.Durable = true;
                //        e.ConfigureConsumer<OrderCalledBackConsumer>(context);

                //    });
                //    cfg.ReceiveEndpoint("order-paid_error", e =>
                //    {
                //        e.ConfigureConsumer<FaultOrderPaidHandler>(context);
                //    });

                //    cfg.ReceiveEndpoint("order-pending", e =>
                //    {
                //        e.PrefetchCount = 1;
                //        e.UseRateLimit(2);
                //        e.PurgeOnStartup = true;
                //        e.AutoDelete = false;
                //        e.Durable = true;
                //        e.ConfigureConsumer<QueueOrderConsumer>(context);
                //        e.SetExchangeArgument("x-max-length", 2);
                //        e.SetExchangeArgument("x-overflow", "reject-publish");
                //    });
                //});
            });

            return services;
        }
        #endregion

        public static WebApplication ApplyMigration(this WebApplication app)
        {
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AutoBrewingKioskBeContext>();

                if (context.Database.GetPendingMigrations().Any())
                {
                    context.Database.Migrate();
                }
            }

            return app;
        }

        public static IServiceCollection AddIotHub(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<IoTHubSettings>(
               configuration.GetSection("IoTHub"));
            services.AddScoped<IIoTHubService, IotHubService>();
            //services.AddHostedService<DeviceMonitoringService>();
            //services.AddHostedService<D2CMsgReceivingService>();
            return services;
        }

        public static IServiceCollection AddWebSocketServices(this IServiceCollection services)
        {
            services.AddSingleton<WebSocketConnectionHandler>();
            services.AddSingleton<IWebSocketManager, Services.WebSockets.WebSocketManager>();
            return services;
        }

        //public static WebApplication UseWebSocketConfiguration(this WebApplication app)
        //{
        //    app.UseWebSockets();

        //    app.Use(async (context, next) =>
        //    {
        //        if (context.Request.Path == "/ws")
        //        {
        //            if (context.WebSockets.IsWebSocketRequest)
        //            {
        //                var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        //                var connectionId = Guid.NewGuid().ToString();

        //                var handler = context.RequestServices.GetRequiredService<WebSocketConnectionHandler>();
        //                await handler.HandleAsync(webSocket, connectionId);
        //            }
        //            else
        //            {
        //                context.Response.StatusCode = StatusCodes.Status400BadRequest;
        //            }
        //        }
        //        else
        //        {
        //            await next();
        //        }
        //    });

        //    return app;
        //}

        #region Workflow Worker
        public static IServiceCollection AddWorkflowWorker(this IServiceCollection services)
        {
            services.AddHostedService<WorkflowObserverWorker>();
            services.AddHostedService<StepObserverWorker>();
            services.AddHostedService<CallbackStepObserverWorker>();
            services.AddHostedService<CleanWorkflowObserverWorker>();
            return services;
        }
        #endregion

        #region RabbitMQ
        public static IServiceCollection AddRabbitMQ(this IServiceCollection services, IConfiguration configuration)
        {
            var rbmqSettings = configuration.GetSection("RabbitMQ");
            services.Configure<RabbitMQSetting>(rbmqSettings);
            services.AddSingleton<IConnection>(sp =>
            {
                var factory = new ConnectionFactory
                {
                    HostName = rbmqSettings["HostName"],
                    UserName = rbmqSettings["UserName"],
                    Password = rbmqSettings["Password"],
                };

                return factory.CreateConnection();
            });
            services.AddKeyedSingleton<IModel>(QueueConstants.QUEUE_WORKFLOW_EXECUTE, (sp, _) =>
            {
                var conn = sp.GetRequiredService<IConnection>();
                var channel = conn.CreateModel();
                channel.ExchangeDeclare(QueueConstants.EXCHANGE_NAME, ExchangeType.Direct, durable: true, autoDelete: false);
                channel.QueueDeclare(queue: QueueConstants.QUEUE_WORKFLOW_EXECUTE, durable: true, exclusive: false, autoDelete: false, arguments: null);
                channel.QueueBind(QueueConstants.QUEUE_WORKFLOW_EXECUTE, QueueConstants.EXCHANGE_NAME, QueueConstants.QUEUE_WORKFLOW_EXECUTE_ROUTING_KEY);

                channel.BasicQos(
                    prefetchSize: 0,     
                    prefetchCount: 1,
                    global: false       
                );

                return channel;
            });

            services.AddKeyedSingleton<IModel>(QueueConstants.QUEUE_STEP_UPDATE, (sp, _) =>
            {
                var conn = sp.GetRequiredService<IConnection>();
                var channel = conn.CreateModel();
                channel.ExchangeDeclare(QueueConstants.EXCHANGE_NAME, ExchangeType.Direct, durable: true, autoDelete: false);

                channel.QueueDeclare(queue: QueueConstants.QUEUE_STEP_UPDATE, durable: true, exclusive: false, autoDelete: false, arguments: null);  
                channel.QueueBind(QueueConstants.QUEUE_STEP_UPDATE, QueueConstants.EXCHANGE_NAME, QueueConstants.QUEUE_STEP_UPDATE_ROUTING_KEY);
                channel.BasicQos(
                    prefetchSize: 0,
                    prefetchCount: 1,
                    global: false
                );

                return channel;
            });

            services.AddKeyedSingleton<IModel>(QueueConstants.QUEUE_DEVICE_UPDATE, (sp, _) =>
            {
                var conn = sp.GetRequiredService<IConnection>();
                var channel = conn.CreateModel();
                channel.ExchangeDeclare(QueueConstants.EXCHANGE_NAME, ExchangeType.Direct, durable: true, autoDelete: false);

                channel.QueueDeclare(queue: QueueConstants.QUEUE_DEVICE_UPDATE, durable: true, exclusive: false, autoDelete: false, arguments: null);  //thông tin status sẽ không cần lưu lâu dài
                channel.QueueBind(QueueConstants.QUEUE_DEVICE_UPDATE, QueueConstants.EXCHANGE_NAME, QueueConstants.QUEUE_DEVICE_UPDATE_ROUTING_KEY);
                channel.BasicQos(
                    prefetchSize: 0,     
                    prefetchCount: 1,    
                    global: false        
                );

                return channel;
            });


            services.AddKeyedSingleton<IModel>(QueueConstants.QUEUE_ORDER, (sp, _) =>
            {
                var conn = sp.GetRequiredService<IConnection>();
                var channel = conn.CreateModel();
                channel.ExchangeDeclare(QueueConstants.EXCHANGE_NAME, ExchangeType.Direct, durable: true, autoDelete: false);

                channel.QueueDeclare(queue: QueueConstants.QUEUE_ORDER, durable: true, exclusive: false, autoDelete: false, arguments: null);  //thông tin status sẽ không cần lưu lâu dài
                channel.QueueBind(QueueConstants.QUEUE_ORDER, QueueConstants.EXCHANGE_NAME, QueueConstants.QUEUE_ORDER_ROUTING_KEY_UPDATE);
                channel.BasicQos(
                    prefetchSize: 0,
                    prefetchCount: 1,
                    global: false
                );

                return channel;
            });

            return services;
        }

        public static void AddRabbitMQConsumers(this IServiceCollection services)
        {
            services.AddHostedService<UpdateDeviceStatusDocConsumer>();
            services.AddHostedService<ExecuteWorkflowConsumer>();
            services.AddHostedService<UpdateWorkflowConsumer>();
            services.AddHostedService<OrderEventConsumer>();
        }
        #endregion

        #region StackExchange.Redis
        public static void AddRedisCache(this IServiceCollection services, IConfiguration configuration)
        {
            // Đăng ký Redis connection multiplexer (singleton)
            services.AddSingleton<IConnectionMultiplexer>(sp =>
            { 
                return ConnectionMultiplexer.Connect(configuration.GetConnectionString("Redis") ?? "localhost:6379");
            });

            services.AddScoped<IDatabase>(sp =>
            {
                var multiplexer = sp.GetRequiredService<IConnectionMultiplexer>();
                return multiplexer.GetDatabase();
            });

        }
        #endregion
    }
}