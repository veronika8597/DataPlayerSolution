using Core.Entities;
using Core.Interfaces;
using Infrastructure.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services;

public class MessageQueueService : IMessageQueueService, IAsyncDisposable
{
    private readonly RabbitMqOptions _opts;
    private readonly IConnection _conn;
    private readonly IChannel _inChannel;
    private readonly IChannel _outChannel;
    
     public MessageQueueService(IOptions<RabbitMqOptions> optsAccessor)
    {
        _opts = optsAccessor.Value;

        var factory = new ConnectionFactory { HostName = _opts.HostName };

        _conn = factory.CreateConnectionAsync().GetAwaiter().GetResult();
        _inChannel = _conn.CreateChannelAsync().GetAwaiter().GetResult();
        _outChannel = _conn.CreateChannelAsync().GetAwaiter().GetResult();

        _inChannel.QueueDeclareAsync(_opts.InputQueueName, false, false, false, null)
                  .GetAwaiter().GetResult();
        _outChannel.QueueDeclareAsync(_opts.OutputQueueName, false, false, false, null)
                   .GetAwaiter().GetResult();
    }

    public async Task<InputMessage?> ReceiveAsync(CancellationToken ct)
    {
        while (true)
        {
            ct.ThrowIfCancellationRequested();

            var res = await _inChannel.BasicGetAsync(_opts.InputQueueName, autoAck: false);
            if (res is null)
            {
                await Task.Delay(150, ct);
                continue;
            }

            try
            {
                var msg = JsonSerializer.Deserialize<InputMessage>(res.Body.Span);
                await _inChannel.BasicAckAsync(res.DeliveryTag, multiple: false);
                return msg;
            }
            catch
            {
                await _inChannel.BasicRejectAsync(res.DeliveryTag, requeue: false);
                return null;
            }
        }
    }

    public async Task SendByteAsync(byte data, CancellationToken ct)
    {
        var props = new BasicProperties(); // optional
        await _outChannel.BasicPublishAsync(
            exchange: "",
            routingKey: _opts.OutputQueueName,
            mandatory: false,
            basicProperties: props,
            body: new[] { data },
            cancellationToken: ct
        );
    }

    public async ValueTask DisposeAsync()
    {
        try { await _inChannel.CloseAsync(); } catch { }
        try { await _outChannel.CloseAsync(); } catch { }
        try { await _conn.CloseAsync(); } catch { }

        try { await _inChannel.DisposeAsync(); } catch { }
        try { await _outChannel.DisposeAsync(); } catch { }
        try { await _conn.DisposeAsync(); } catch { }
    }
    
    public async Task SendOutputByteAsync(OutputByteMessage msg, CancellationToken ct)
    {
        var body = JsonSerializer.SerializeToUtf8Bytes(msg);
        var props = new BasicProperties();
        await _outChannel.BasicPublishAsync(
            exchange: "",
            routingKey: _opts.OutputQueueName,
            mandatory: false,
            basicProperties: props,
            body: body,
            cancellationToken: ct);
    }

    public async Task<OutputByteMessage?> ReceiveOutputByteAsync(CancellationToken ct)
    {
        while (true)
        {
            ct.ThrowIfCancellationRequested();

            var res = await _outChannel.BasicGetAsync(_opts.OutputQueueName, autoAck: false);
            if (res is null)
            {
                await Task.Delay(150, ct);
                continue;
            }

            try
            {
                var msg = JsonSerializer.Deserialize<OutputByteMessage>(res.Body.Span);
                await _outChannel.BasicAckAsync(res.DeliveryTag, multiple: false);
                return msg;
            }
            catch
            {
                await _outChannel.BasicRejectAsync(res.DeliveryTag, requeue: false);
                return null;
            }
        }
    }
    
    public async Task SendInputMessageAsync(InputMessage msg, CancellationToken ct)
    {
        var body = JsonSerializer.SerializeToUtf8Bytes(msg);
        var props = new BasicProperties();
        await _inChannel.BasicPublishAsync(
            exchange: "",
            routingKey: _opts.InputQueueName,
            mandatory: false,
            basicProperties: props,
            body: body,
            cancellationToken: ct);
    }
}
