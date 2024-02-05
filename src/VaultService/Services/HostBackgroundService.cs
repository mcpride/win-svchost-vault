using System;
using System.Threading;
using System.Threading.Tasks;
using CliWrap;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VaultService.Models.Options;

namespace VaultService.Services
{
    public class HostBackgroundService : CommandBackgroundService
    {
        private readonly ILogger<HostBackgroundService> _logger;
        private readonly IOptions<VaultOptions> _vaultOptions;
        private readonly Lazy<UnsealBackgroundService> _unsealBackgroundService;
        private CancellationTokenSource? _forceFulCts;

        public HostBackgroundService(
            ILogger<HostBackgroundService> logger,
            IOptions<VaultOptions> vaultOptions,
            Lazy<UnsealBackgroundService> unsealBackgroundService)
        : base(logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _vaultOptions = vaultOptions ?? throw new ArgumentNullException(nameof(vaultOptions));
            _unsealBackgroundService = unsealBackgroundService ?? throw new ArgumentNullException(nameof(unsealBackgroundService));
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            if (_logger.IsEnabled(LogLevel.Information)) _logger.LogInformation("Starting vault service...");

            _forceFulCts?.Dispose();
            _forceFulCts = new CancellationTokenSource();

            await base.StartAsync(cancellationToken);
            await _unsealBackgroundService.Value.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var options = _vaultOptions.Value;
            var startServerCommand = NewStartServerCommand(options);
            if (startServerCommand == null) return;
            var startServerCommandTask = startServerCommand.ExecuteAsync(_forceFulCts!.Token, stoppingToken);

            if (_logger.IsEnabled(LogLevel.Information)) _logger.LogInformation("Vault service started!");

            await startServerCommandTask;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            if (_logger.IsEnabled(LogLevel.Information)) _logger.LogInformation("Stopping vault service...");
            _unsealBackgroundService.Value.StopAsync(cancellationToken);
            _forceFulCts!.CancelAfter(TimeSpan.FromSeconds(10));
            var result = base.StopAsync(cancellationToken);
            return result;
        }

        public override void Dispose()
        {
            base.Dispose();
            _forceFulCts?.Dispose();
        }

        private Command? NewStartServerCommand(VaultOptions options)
        {
            if (options.Path == null) return null;
            return Cli.Wrap(options.Path)
                       .WithArguments(string.Empty + options.StartParameters)
                       .WithValidation(CommandResultValidation.ZeroExitCode)
                   | (StdOutHandler, StdErrHandler);
        }
    }
}
