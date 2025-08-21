namespace Core.Entities;

public class InputMessage
{
    public Guid TaskId { get; set; }
    public byte[] Data { get; set; } = Array.Empty<byte>();
}