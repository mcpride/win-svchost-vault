using System;
using VaultService.Security.Cryptography;

namespace VaultService.CommandLine.Administration;

internal class EncryptCommand
{
    public static void Execute(string? thumbprint, string? text)
    {
        if (string.IsNullOrWhiteSpace(thumbprint))
        {
            thumbprint = Console.In.ReadLine();
        }
        while (string.IsNullOrWhiteSpace(thumbprint))
        {
            Console.Error.WriteLine("Please enter the thumbprint of the encipherment certificate in the certificate store:");
            thumbprint = Console.ReadLine();
        }
        if (string.IsNullOrWhiteSpace(text))
        {
            text = Console.In.ReadLine();
        }
        while (string.IsNullOrWhiteSpace(text))
        {
            Console.Error.WriteLine("Please enter text to encrypt:");
            text = Console.ReadLine();
        }
        try
        {
            var s = Cipher.EncryptText(text, thumbprint);
            Console.Error.WriteLine("Encrypted text:");
            Console.Out.WriteLine(s);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.Message);
        }
    }
}