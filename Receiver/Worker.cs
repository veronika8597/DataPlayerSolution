using Core.Interfaces;
using Core.Services;

namespace Receiver;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IXorConfigurationService _config;
    private readonly IMessageQueueService _queueService;
    private readonly TaskAssembler _assembler;

    public Worker(
        ILogger<Worker> logger,
        IXorConfigurationService config,
        IMessageQueueService queueService,
        TaskAssembler assembler)
    {
        _logger = logger;
        _config = config;
        _queueService = queueService;
        _assembler = assembler;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var key = _config.GetXorKey();
        
        while (!stoppingToken.IsCancellationRequested)
        {
            // 1. Receive one OutputByteMessage
            var message = await _queueService.ReceiveOutputByteAsync(stoppingToken);
            if (message is null) continue;
            
            // 2. Reverse XOR → get original byte
            byte unprocessedOriginalByte = (byte)(message.Data ^ key);
            _assembler.Add(message.TaskId, message.Index, unprocessedOriginalByte, message.IsLast);

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            }

            await Task.Delay(1000, stoppingToken);
        }
    }
}