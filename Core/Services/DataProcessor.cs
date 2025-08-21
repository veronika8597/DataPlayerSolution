using Core.Entities;
using Core.Interfaces;

namespace Core.Services;

public class DataProcessor
{
    private readonly IXorConfigurationService _config;
    private readonly ILoggerService _logger;
    private readonly IMessageQueueService _queueService;

    public DataProcessor(
        IXorConfigurationService config,
        ILoggerService logger,
        IMessageQueueService queueService)
    {
        _config = config;
        _logger = logger;
        _queueService = queueService;
    }

    public async Task ProcessMessageAsync(InputMessage message, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        byte xorKey = _config.GetXorKey();
        
        for (int i = 0; i < message.Data.Length; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var originalByte = message.Data[i];
            var result = (byte)(originalByte ^ xorKey);

            _logger.Log($"Task {message.TaskId}: {originalByte} XOR {xorKey} = {result}");

            var outMsg = new OutputByteMessage(
                TaskId: message.TaskId,
                Index: i,
                Data: result,                  // the XOR'ed byte
                IsLast: i == message.Data.Length - 1
            );

            await _queueService.SendOutputByteAsync(outMsg, cancellationToken);
        }
    }
}