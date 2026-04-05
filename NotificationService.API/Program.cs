using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using NotificationService.API.Data;
using NotificationService.API.Messaging;
using NotificationService.API.Middleware;
using NotificationService.API.Services;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
        .Build())
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Service", "NotificationService")
    .WriteTo.Console()
    .WriteTo.File("logs/notification-service-.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "NotificationService API",
        Version = "v1",
        Description = "Handles email notifications and OTP verification via RabbitMQ"
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<INotificationService, NotificationService.API.Services.NotificationService>();
builder.Services.AddHostedService<RabbitMqConsumerService>();

var app = builder.Build();

app.UseExceptionHandlingMiddleware();

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "NotificationService API v1"));

app.UseAuthorization();
app.MapControllers();

app.Run();
