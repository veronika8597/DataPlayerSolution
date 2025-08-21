using Core.Interfaces;
using Core.Services;

namespace API.Workers;

public sealed class DataPlayerWorker : BackgroundService
{
    private readonly ILogger<DataPlayerWorker> _logger;
    private readonly IMessageQueueService _queueService;
    private readonly DataProcessor _processor;

    public DataPlayerWorker(
        ILogger<DataPlayerWorker> logger,
        IMessageQueueService queueService,
        DataProcessor processor)
    {
        _logger = logger;
        _queueService = queueService;
        _processor = processor;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("DataPlayerWorker started");
        while (!stoppingToken.IsCancellationRequested)
        {
            var input = await _queueService.ReceiveAsync(stoppingToken); // InputMessage from InputDataQ
            if (input is null) continue;

            await _processor.ProcessMessageAsync(input, stoppingToken);
        }
    }
}