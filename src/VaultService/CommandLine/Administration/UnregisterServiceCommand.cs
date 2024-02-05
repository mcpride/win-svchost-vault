using System;
using System.Threading.Tasks;

namespace VaultService.CommandLine.Administration;

internal class UnregisterServiceCommand
{
    public static async Task ExecuteAsync()
    {
        await using var stdOut = Console.OpenStandardOutput();
        await using var stdErr = Console.OpenStandardError();
        var cmd = CliWrap.Cli.Wrap("sc.exe")
                      .WithArguments("delete VaultService")
                  | (stdOut, stdErr);
        await cmd.ExecuteAsync();
    }
}