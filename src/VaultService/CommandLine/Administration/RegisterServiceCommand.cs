using System;
using System.IO;
using System.Threading.Tasks;

namespace VaultService.CommandLine.Administration;

internal class RegisterServiceCommand
{
    public static async Task ExecuteAsync()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "VaultService.exe");
        await using var stdOut = Console.OpenStandardOutput();
        await using var stdErr = Console.OpenStandardError();
        var cmd = CliWrap.Cli.Wrap("sc.exe")
                      .WithArguments($"create VaultService binPath= \"{path}\" start= auto DisplayName= \"Service host for vault\"")
                  | (stdOut, stdErr);
        await cmd.ExecuteAsync();
    }
}