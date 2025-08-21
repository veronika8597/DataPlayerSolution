using Core.Entities;

namespace Core.Interfaces;

public interface IMessageQueueService
{
     Task<InputMessage?> ReceiveAsync(CancellationToken cancellationToken);
     Task SendByteAsync(byte data, CancellationToken cancellationToken);
     
     //send one processed byte with metadata to OutputDataQ
     Task SendOutputByteAsync(OutputByteMessage msg, CancellationToken cancellationToken);

     //receive one processed byte with metadata from OutputDataQ
     Task<OutputByteMessage?> ReceiveOutputByteAsync(CancellationToken cancellationToken);
     
     //publish a full InputMessage to InputDataQ (for testing)
     Task SendInputMessageAsync(InputMessage msg, CancellationToken ct);
}