using System;
using System.Threading;
using System.Threading.Tasks;
using CliWrap;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VaultService.Models.Options;
using VaultService.Security.Cryptography;

namespace VaultService.Services
{
    public class UnsealBackgroundService : CommandBackgroundService
    {
        private readonly ILogger<UnsealBackgroundService> _logger;
        private readonly IOptions<VaultOptions> _vaultOptions;

        public UnsealBackgroundService(ILogger<UnsealBackgroundService> logger, IOptions<VaultOptions> vaultOptions)
            : base(logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _vaultOptions = vaultOptions ?? throw new ArgumentNullException(nameof(vaultOptions));
        }

        /// <inheritdoc />
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var options = _vaultOptions.Value;

            await Task.Delay(3000, stoppingToken);
            stoppingToken.ThrowIfCancellationRequested();

            if (options.UnsealKeys is { Count: > 0 })
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    if (await IsVaultSealed(options, stoppingToken))
                    {
                        await UnsealVault(options, stoppingToken);
                    }
                    await Task.Delay(30000, stoppingToken);
                }
            }
        }

        private async Task<bool> IsVaultSealed(VaultOptions options, CancellationToken stoppingToken)
        {
            var isSealed = false;
            var statusCommand = NewStatusCommand(options);
            try
            {
                if (statusCommand != null)
                {
                    if (_logger.IsEnabled(LogLevel.Information)) _logger.LogInformation("Checking unseal status...");
                    var statusResult = await statusCommand.ExecuteAsync(stoppingToken);
                    isSealed = statusResult.ExitCode == 2;
                }
            }
            catch (Exception e)
            {
                _logger.LogWarning($"Getting status from Vault failed! {e.Message}");
            }

            return isSealed;
        }

        private async Task UnsealVault(VaultOptions options, CancellationToken stoppingToken)
        {
            //if (options.UnsealKeys is not { Count: > 0 }) return;
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("Unsealing Vault with {count} configured keys ...",
                    options.UnsealKeys!.Count);
            foreach (var key in options.UnsealKeys!)
            {
                stoppingToken.ThrowIfCancellationRequested();
                if (_logger.IsEnabled(LogLevel.Information)) _logger.LogInformation("Unsealing Vault ...");
                var unsealCommand = NewUnsealCommand(options, key);
                if (unsealCommand == null) continue;
                try
                {
                    await unsealCommand.ExecuteAsync(stoppingToken);
                }
                catch (Exception e)
                {
                    _logger.LogWarning($"Unsealing Vault failed! {e.Message}");
                    break;
                }
            }
        }

        private Command? NewUnsealCommand(VaultOptions options, string key)
        {
            if (options.Path == null) return null;
            return Cli.Wrap(options.Path)
                       .WithEnvironmentVariables(_ =>
                       {
                           _
                               .Set("VAULT_ADDR", options.Address);
                       })
                       .WithArguments(_ =>
                       {
                           _
                               .Add("operator")
                               .Add("unseal")
                               .Add(Cipher.DecryptText(key));
                       })
                       .WithValidation(CommandResultValidation.ZeroExitCode)
                   | (StdOutHandler, StdErrHandler);
        }

        private Command? NewStatusCommand(VaultOptions options)
        {
            if (options.Path == null) return null;
            return Cli.Wrap(options.Path)
                       .WithEnvironmentVariables(_ =>
                       {
                           _
                               .Set("VAULT_ADDR", options.Address);
                       })
                       .WithArguments(_ =>
                       {
                           _
                               .Add("status");
                       })
                       .WithValidation(CommandResultValidation.None)
                   | (StdOutHandler, StdErrHandler);
        }
    }
}
