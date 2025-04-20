using System.ComponentModel.DataAnnotations;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using RabbitMQ.Client;
using RMQConsumerService.Models;
using RMQConsumerService.Services;

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.Configure<RabbitMQOption>(builder.Configuration.GetSection("RabbitMQOption"));

// MassTransit Setup
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

        //cfg.ReceiveEndpoint("example-queue", e =>
        //{
        //    e.ConfigureConsumeTopology = false;

        //    e.Bind("example-ex", s =>
        //    {
        //        s.RoutingKey = "update.*";
        //        s.ExchangeType = ExchangeType.Topic;
        //    });

        //    e.ConfigureDefaultErrorTransport();
        //    e.ExchangeType = ExchangeType.Topic;
        //    e.ConfigureConsumer<SmsService>(context);

        //    e.UseMessageRetry(error =>
        //    {
        //        error.Handle<TimeoutException>(); // Retry only for TimeoutException
        //        error.Handle<ValidationException>(); // Retry only for ValidationException
        //        error.Ignore<UnauthorizedAccessException>(); // Skip retries for UnauthorizedAccessException

        //        error.Interval(3, TimeSpan.FromSeconds(2));
        //    });
        //    // Optional: PrefetchCount, Concurrency
        //    //e.PrefetchCount = 16;
        //    //e.ConcurrentMessageLimit = 8;
        //});

        //cfg.ReceiveEndpoint("example-queue_error", q =>
        //{
        //    q.ConfigureConsumeTopology = false;

        //    q.Bind("example-queue_error", s =>
        //    {
        //        s.RoutingKey = "update.*";
        //        s.ExchangeType = ExchangeType.Topic;
        //    });

        //    q.ExchangeType = ExchangeType.Topic;
        //    q.ConfigureConsumer<ErrorService>(context);
        //});

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
