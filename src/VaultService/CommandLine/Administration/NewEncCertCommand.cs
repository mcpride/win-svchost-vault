using System;
using System.IO;
using System.Text;
using VaultService.Security.Cryptography;

namespace VaultService.CommandLine.Administration;

internal class NewEncCertCommand
{
    public static void Execute(string file, string? password, string subject)
    {
        while (password == null)
        {
            Console.Error.WriteLine("Please enter a secure password for the PFX certificate file:");
            password = Console.ReadLine();
            Console.Clear();
        }

        try
        {
            var (certBytes, thumbprint) = Pki.CreateEnciphermentCertificate(subject, password);
            if (certBytes == null)
            {
                Console.Error.WriteLine("Failed to create certificate!");
                return;
            }
            Console.Error.WriteLine(thumbprint);
            if (string.IsNullOrEmpty(file))
            {
                using (var stdout = Console.OpenStandardOutput())
                {
                    stdout.Write(certBytes);
                }
                Console.Out.Write(Encoding.ASCII.GetChars(certBytes));
            }
            else
            {
                File.WriteAllBytes(file, certBytes);
                Console.Error.WriteLine($"New encipherment certificate with thumbprint {thumbprint} written to file `{file}`!");
            }
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.Message);
            throw;
        }

    }
}