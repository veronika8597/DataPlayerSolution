using API.Workers;
using Core.Entities;
using Core.Interfaces;
using Core.Services;
using Infrastructure.Services;
using Infrastructure.Options;


var builder = WebApplication.CreateBuilder(args);

// OpenAPI (optional)
builder.Services.AddOpenApi();

//////////////////////////////////////
// Bind RabbitMQ settings from appSettings.json
builder.Services.Configure<RabbitMqOptions>(
    builder.Configuration.GetSection("RabbitMq"));

// Register your MQ service
builder.Services.AddSingleton<IMessageQueueService, MessageQueueService>();
builder.Services.AddTransient<ILoggerService, LoggerService>();
builder.Services.AddSingleton<IXorConfigurationService, XorConfigurationService>();

builder.Services.AddSingleton<DataProcessor>();
builder.Services.AddHostedService<DataPlayerWorker>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// ===== Test endpoints =====

app.MapPost("/mq/enqueue", async (string text, IMessageQueueService mq, CancellationToken ct) =>
{
    var msg = new InputMessage
    {
        TaskId = Guid.NewGuid(),
        Data = System.Text.Encoding.UTF8.GetBytes(text)
    };

    await mq.SendInputMessageAsync(msg, ct);
    return Results.Ok(new { msg.TaskId, bytes = msg.Data.Length });
});

// Wait for the next JSON InputMessage and return it
app.MapGet("/mq/receive", async (IMessageQueueService mq, CancellationToken ct) =>
{
    var msg = await mq.ReceiveAsync(ct);
    return msg is null ? Results.NoContent() : Results.Ok(msg);
});

app.Run();