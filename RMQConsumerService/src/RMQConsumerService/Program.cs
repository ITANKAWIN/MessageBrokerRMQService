using System.ComponentModel.DataAnnotations;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using RabbitMQ.Client;
using RMQConsumerService.Data;
using RMQConsumerService.Models;
using RMQConsumerService.Services;
using RMQConsumerService.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
    )
);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "RMQConsumer Service",
        Description = "An ASP.NET Core Web API for managing ToDo items",
        TermsOfService = new Uri("https://example.com/terms"),
        Contact = new OpenApiContact
        {
            Name = "Example Contact",
            Url = new Uri("https://example.com/contact")
        }
    });
});
builder.Services.AddHealthChecks();

// Configure bulk insert options
builder.Services.Configure<BulkInsertOptions>(builder.Configuration.GetSection(BulkInsertOptions.SectionName));

// Register bulk insert logging services
builder.Services.AddScoped<ILogApplication, LogApplication>();
// Alternative: Use enhanced version with retry logic and advanced features
// builder.Services.AddScoped<ILogApplication, EnhancedLogApplication>();

// Register message buffer service as singleton for shared buffer across consumers
builder.Services.AddSingleton<IMessageBufferService, MessageBufferService>();

builder.Services.Configure<RabbitMQOption>(builder.Configuration.GetSection("RabbitMQOption"));

builder.Services.AddMassTransit(x =>
{
    x.SetKebabCaseEndpointNameFormatter();
    x.AddConsumer<SmsService>();
    x.AddConsumer<NotiService>();

    x.UsingRabbitMq((context, cfg) =>
    {
        var rmqOption = context.GetRequiredService<IOptions<RabbitMQOption>>().Value;

        cfg.Host(rmqOption.IP, rmqOption.Port, rmqOption.VirtualHost, h =>
        {
            h.Username(rmqOption.Username);
            h.Password(rmqOption.PWD);
        });

        cfg.ReceiveEndpoint(rmqOption.SmsQueue.QueueName, e =>
        {
            e.ConfigureConsumeTopology = false;

            e.Bind(rmqOption.SmsQueue.ExchangeName, s =>
            {
                s.RoutingKey = rmqOption.RoutingKey;
                s.ExchangeType = ExchangeType.Topic;
            });

            e.ConfigureDefaultErrorTransport();
            e.ExchangeType = ExchangeType.Topic;
            e.ConfigureConsumer<SmsService>(context);

            // Optional: PrefetchCount, Concurrency
            //e.PrefetchCount = 16;
            //e.ConcurrentMessageLimit = 8;
        });

        cfg.ReceiveEndpoint(rmqOption.NotificationQueue.QueueName, e =>
        {
            e.ConfigureConsumeTopology = false;

            e.Bind(rmqOption.NotificationQueue.ExchangeName, s =>
            {
                s.RoutingKey = rmqOption.RoutingKey;
                s.ExchangeType = ExchangeType.Topic;
            });

            e.ConfigureDefaultErrorTransport();
            e.ExchangeType = ExchangeType.Topic;
            e.ConfigureConsumer<NotiService>(context);

            // Optional: PrefetchCount, Concurrency
            //e.PrefetchCount = 16;
            //e.ConcurrentMessageLimit = 8;
        });

        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

var app = builder.Build();

if (!app.Environment.IsProduction())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();
//app.MapHealthChecks("/healthz");

app.Run();
