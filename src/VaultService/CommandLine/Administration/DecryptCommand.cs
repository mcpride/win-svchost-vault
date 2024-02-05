using System;
using VaultService.Security.Cryptography;

namespace VaultService.CommandLine.Administration;

internal class DecryptCommand
{
    public static void Execute(string? text)
    {

        if (string.IsNullOrWhiteSpace(text))
        {
            text = Console.In.ReadLine();
        }
        while (string.IsNullOrWhiteSpace(text))
        {
            Console.Error.WriteLine("Please enter text to decrypt:");
            text = Console.ReadLine();
        }
        try
        {
            var s = Cipher.DecryptText(text);
            Console.Error.WriteLine("Decrypted text:");
            Console.Out.WriteLine(s);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.Message);
        }
    }
}