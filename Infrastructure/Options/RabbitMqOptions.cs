namespace Infrastructure.Options;

public sealed class RabbitMqOptions
{
    public string HostName { get; set; } = "localhost";
    public string? UserName { get; set; }    // optional
    public string? Password { get; set; }    // optional
    public string InputQueueName { get; set; } = "InputDataQ";
    public string OutputQueueName { get; set; } = "OutputDataQ";
}