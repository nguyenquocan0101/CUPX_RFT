using Kiosk.ApiService.Extensions;
using Kiosk.ApiService.Filters;
using Kiosk.ApiService.Middleware;
using Serilog;
using Services.Implements;
using Services.Interfaces;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = Directory.GetCurrentDirectory(),
    EnvironmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"
});

var configuration = builder.Configuration;


//Add support to logging with SERILOG
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

// Add services to the container.
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy => { policy.WithOrigins("*").AllowAnyHeader().AllowAnyMethod(); });
});

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
});



//Register StartupInitializer
builder.Services.AddRedisCache(configuration);
builder.Services.AddScoped<IRuntimeStateService, RedisRuntimeStateService>();
builder.Services.AddScoped<IStartupInitializer, StartupInitializer>();
builder.Services.AddSingleton<IStartupResourceProvisioner, StartupResourceProvisioner>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<ApiKeyAuthenticationMiddleware>();
builder.Services.AddHealthChecks();
builder.Services.AddSwaggerGen();
builder.Services.AddConfigSwagger();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddMapperConfig();
builder.Services.AddDatabase(configuration);
//builder.Services.AddUnitOfWork();
builder.Services.AddAppServices(configuration);
//builder.Services.AddIotHub(configuration);
//builder.Services.AddWorkflowEngineConfig(configuration);
//builder.Services.AddWebSocketServices();
if (KioskRuntimeSettings.AreWorkflowWorkersEnabled(configuration))
{
    builder.Services.AddWorkflowWorker();
}
builder.Services.AddRabbitMQ(configuration).AddRabbitMQConsumers();

//Add Controller Filter
builder.Services.AddTransient<MaintenanceFilter>();


var app = builder.Build();

// Auto-apply migrations at runtime
//using (var scope = app.Services.CreateScope())
//{
//    var dbContext = scope.ServiceProvider.GetRequiredService<AutoBrewingKioskBeContext>();
//    //dbContext.Database.Migrate();
//}

app.UseSerilogRequestLogging();
// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
app.UseSwagger();
    app.UseSwaggerUI();
//}

// Enable Websocket
//app.UseWebSocketConfiguration();

app.UseCors();

if (!KioskRuntimeSettings.IsLocalMode(configuration))
{
    app.UseHttpsRedirection();
}
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<ApiKeyAuthenticationMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<IStartupInitializer>();
    await initializer.InitializeAsync();
}

//app.ApplyMigration();

app.Run();
