using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;
using VaultService.Models.Options;
using VaultService.Services;

namespace VaultService.CommandLine;

internal class RunCommand
{
    public static void Execute(string[]? args)
    {
        var codePath = AppContext.BaseDirectory;
        Directory.SetCurrentDirectory(codePath);
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);

        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File(
                path: Path.Combine(appDataPath, "VaultService", "logs", "servicelog.txt"),
                fileSizeLimitBytes: 1000000,
                rollOnFileSizeLimit: true)
            .CreateLogger();

        Host
            .CreateDefaultBuilder(args)
            .UseWindowsService(o =>
            {
                o.ServiceName = "Service host for vault";
            })
            .ConfigureServices((ctx, services) =>
            {
                services
                    .AddHostedService<HostBackgroundService>()
                    .AddSingleton(
                        sp =>
                            new Lazy<UnsealBackgroundService>(
                                    new UnsealBackgroundService(
                                        sp.GetRequiredService<ILogger<UnsealBackgroundService>>(),
                                        sp.GetRequiredService<IOptions<VaultOptions>>())))
                    .AddOptions<VaultOptions>()
                    .Bind(ctx.Configuration.GetSection("Vault"));
            })
            .ConfigureAppConfiguration((_, builder) =>
            {
                builder.AddJsonFile(Path.Combine(codePath, "appsettings.json"), true, true);
            })
            .UseSerilog()
            .Build()
            .Run();
    }
}