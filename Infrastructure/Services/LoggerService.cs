using Core.Interfaces;

namespace Infrastructure.Services;

public class LoggerService : ILoggerService
{ 
    public void Log(string message)
    {
        Console.WriteLine($"[LOG] {message}");
    }
}