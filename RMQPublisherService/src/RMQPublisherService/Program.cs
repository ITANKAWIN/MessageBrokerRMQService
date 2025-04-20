using Microsoft.OpenApi.Models;
using RMQPublisherService.Services.Interfaces;
using RMQPublisherService.Services;
using MassTransit;
using RMQPublisherService.Models;
using RabbitMQ.Client;
using Microsoft.Extensions.Options;
using RabbitMQ.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "RMQPublisher Service",
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

builder.Services.Configure<RabbitMQOption>(builder.Configuration.GetSection("RabbitMQOption"));

// MassTransit Setup
builder.Services.AddMassTransit(x =>
{
    x.SetKebabCaseEndpointNameFormatter();
    x.UsingRabbitMq((context, cfg) =>
    {
        var rmqOption = context.GetRequiredService<IOptions<RabbitMQOption>>().Value;

        cfg.Host(rmqOption.IP, rmqOption.Port, rmqOption.VirtualHost, h =>
        {
            h.Username(rmqOption.Username);
            h.Password(rmqOption.PWD);
        });

        cfg.Message<ISendSMSMessage>(m =>
        {
            m.SetEntityName(rmqOption.SmsQueue.ExchangeName);
        });

        cfg.Publish<ISendSMSMessage>(p =>
        {
            p.BindQueue(rmqOption.SmsQueue.ExchangeName, rmqOption.SmsQueue.QueueName, cc =>
            {
                cc.RoutingKey = rmqOption.RoutingKey;
                cc.ExchangeType = ExchangeType.Topic;
            });

            p.ExchangeType = ExchangeType.Topic;
        });

        cfg.Message<ISendNotiMessage>(m =>
        {
            m.SetEntityName(rmqOption.NotificationQueue.ExchangeName);
        });

        cfg.Publish<ISendNotiMessage>(p =>
        {
            p.BindQueue(rmqOption.NotificationQueue.ExchangeName, rmqOption.NotificationQueue.QueueName, cc =>
            {
                cc.RoutingKey = rmqOption.RoutingKey;
                cc.ExchangeType = ExchangeType.Topic;
            });

            p.ExchangeType = ExchangeType.Topic;
        });

        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddScoped<IPublisherService, PublisherService>();

var app = builder.Build();

if (!app.Environment.IsProduction())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/healthz");

app.Run();
