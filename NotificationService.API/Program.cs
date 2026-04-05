using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using NotificationService.API.Data;
using NotificationService.API.Messaging;
using NotificationService.API.Services;

var builder = WebApplication.CreateBuilder(args);

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

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "NotificationService API v1"));

app.UseAuthorization();
app.MapControllers();

app.Run();
