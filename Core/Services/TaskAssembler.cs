namespace Core.Services;

public class TaskAssembler
{
    private sealed class Buffer
    {
        public readonly SortedDictionary<int, byte> Bytes = new();
        public bool LastSeen;
        public int LastIndex = -1;
    }

    private readonly Dictionary<Guid, Buffer> _byTask = new();

    public void Add(Guid taskId, int index, byte value, bool isLast)
    {
        if (!_byTask.TryGetValue(taskId, out var buf))
            _byTask[taskId] = buf = new Buffer();

        buf.Bytes[index] = value;
        if (isLast) { buf.LastSeen = true; buf.LastIndex = index; }
    }

    public bool TryAssemble(Guid taskId, out byte[] data)
    {
        data = Array.Empty<byte>();
        if (!_byTask.TryGetValue(taskId, out var buf) || !buf.LastSeen) return false;

        for (int i = 0; i <= buf.LastIndex; i++)
            if (!buf.Bytes.ContainsKey(i)) return false;

        data = buf.Bytes.OrderBy(kv => kv.Key).Select(kv => kv.Value).ToArray();
        _byTask.Remove(taskId);
        return true;
    }
}