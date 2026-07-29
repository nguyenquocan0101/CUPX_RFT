using System.Text.Json.Serialization;
using AutomaticBrewingCoffee.API.Extensions;
using Hangfire;
using Microsoft.IO;
using Services.Local;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = Directory.GetCurrentDirectory(),
    EnvironmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"
});


var configuration = builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();

var localMode = LocalRuntimeDependency.IsLocalMode(
    builder.Environment.EnvironmentName,
    configuration);
var backgroundJobsEnabled =
    LocalRuntimeDependency.AreBackgroundJobsEnabled(localMode, configuration);

var assemblies = AppDomain.CurrentDomain.GetAssemblies();

if (!localMode)
{
    builder.WebHost.AddSentry(configuration);
}

builder.Services.AddSingleton<RecyclableMemoryStreamManager>();
builder.Services.AddRabbitMQCap(configuration);
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();
builder.Services.AddEmail(configuration);
builder.Services.AddRedis(configuration);
builder.Services.AddAutoMapper(assemblies);
builder.Services.AddDatabase(configuration);
builder.Services.AddUnitOfWork();
builder.Services.AddServices();

if (localMode)
{
    builder.Services.AddLocalRuntime(configuration);
    builder.Services.AddLocalProviders(configuration);
}
else
{
    builder.Services.AddCloudflareConfig(configuration);
    builder.Services.AddMPOSConfig(configuration);
    builder.Services.AddFirebase(configuration);
    builder.Services.AddSupabase(configuration);
    builder.Services.AddVNPay(configuration);
    builder.Services.AddAzureHub(configuration);
}

if (backgroundJobsEnabled)
{
    builder.Services.AddAppHangFire(configuration);
}

builder.Services.AddAuthentication(configuration);
builder.Services.AddConfigSwagger();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();
builder.Services.AddSignalR();
builder.Services.AddControllers().AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    }
);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.WithOrigins(configuration["WebApp:Domain"]!)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

if (localMode)
{
    app.UseDeveloperExceptionPage();
}

if (localMode && configuration.GetValue<bool>("LocalSeed:Enabled"))
{
    using var seedScope = app.Services.CreateScope();
    await seedScope.ServiceProvider
        .GetRequiredService<LocalDevelopmentSeeder>()
        .SeedAsync();
}

if (localMode && args.Contains("--seed-only", StringComparer.OrdinalIgnoreCase))
{
    return;
}

// Configure the HTTP request pipeline.

// app.UseMiddleware<ExceptionHandlingMiddleware>();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None); // Collapse all by default
    });
}

if (!localMode)
{
    app.UseSentryTracing();
}

app.UseRouting();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapHealthChecks("/health");
app.MapControllers();
app.MapSignalRHub();

if (!localMode)
{
    app.ApplyMigration();
}

if (backgroundJobsEnabled)
{
    app.UseHangfireDashboard();
    ServicesDependency.RegisterRecurringJob();
}

app.Run();
