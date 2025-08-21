namespace Core.Entities;

public record OutputByteMessage(Guid TaskId, int Index, byte Data, bool IsLast);