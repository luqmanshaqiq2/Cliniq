using Microsoft.Extensions.Logging;

namespace Cliniq.Services.Logging;

public sealed class AppLogging<T> : IAppLogging<T>
{
    private readonly ILogger<T> _logger;

    public AppLogging(ILogger<T> logger)
    {
        _logger = logger;
    }

    public void LogDebug(string message, params object?[] args)
        => _logger.LogDebug(message, args);

    public void LogInformation(string message, params object?[] args)
        => _logger.LogInformation(message, args);

    public void LogWarning(string message, params object?[] args)
        => _logger.LogWarning(message, args);

    public void LogError(Exception exception, string message, params object?[] args)
        => _logger.LogError(exception, message, args);

    public void LogCritical(Exception exception, string message, params object?[] args)
        => _logger.LogCritical(exception, message, args);
}
