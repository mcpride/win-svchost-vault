using System;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace VaultService.Services;

public abstract class CommandBackgroundService : BackgroundService
{
    private readonly ILogger _logger;

    protected CommandBackgroundService(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected void StdOutHandler(string? value)
    {
        value ??= string.Empty;
        if (_logger.IsEnabled(LogLevel.Information)) _logger.LogInformation("out: {value}", value);
    }

    protected void StdErrHandler(string? value)
    {
        value ??= string.Empty;
        if (_logger.IsEnabled(LogLevel.Information)) _logger.LogInformation("log: {value}", value);
    }
}