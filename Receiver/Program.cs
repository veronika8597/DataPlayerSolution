using Receiver;
using Core.Interfaces;
using Core.Services;
using Infrastructure.Options;
using Infrastructure.Services;

var builder = Host.CreateApplicationBuilder(args);

// read RabbitMQ section
builder.Services.Configure<RabbitMqOptions>(
    builder.Configuration.GetSection("RabbitMq"));

// register services
builder.Services.AddSingleton<IMessageQueueService, MessageQueueService>();
builder.Services.AddSingleton<IXorConfigurationService, XorConfigurationService>();
builder.Services.AddSingleton<ILoggerService, LoggerService>();
builder.Services.AddSingleton<TaskAssembler>();

// add our background worker
builder.Services.AddHostedService<Worker>();

await builder.Build().RunAsync();
