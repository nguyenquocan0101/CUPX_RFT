using System.Text.Json.Serialization;
using AutomaticBrewingCoffee.API.Extensions;
using Hangfire;
using Microsoft.IO;

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

builder.WebHost.AddSentry(configuration);

var assemblies = AppDomain.CurrentDomain.GetAssemblies();

builder.Services.AddSingleton<RecyclableMemoryStreamManager>();
builder.Services.AddRabbitMQCap(configuration);
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();
builder.Services.AddEmail(configuration);
builder.Services.AddCloudflareConfig(configuration);
builder.Services.AddRedis(configuration);
builder.Services.AddMPOSConfig(builder.Configuration);
builder.Services.AddAppHangFire(configuration);
builder.Services.AddAutoMapper(assemblies);
builder.Services.AddDatabase(configuration);
builder.Services.AddFirebase(configuration);
builder.Services.AddSupabase(configuration);
builder.Services.AddVNPay(configuration);
builder.Services.AddUnitOfWork();
builder.Services.AddServices();
builder.Services.AddAzureHub(configuration);
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

app.UseSentryTracing();
app.UseRouting();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapHealthChecks("/health");
app.MapControllers();
app.ApplyMigration();
app.UseHangfireDashboard();
app.MapSignalRHub();

ServicesDependency.RegisterRecurringJob();

app.Run();
