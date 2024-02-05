using System.CommandLine;
using System.Threading.Tasks;
using VaultService.CommandLine;
using VaultService.CommandLine.Administration;

namespace VaultService
{
    internal class Program
    {
        public static string[]? Args;
        static async Task Main(string[] args)
        {
            Args = args;
            await BuildCommandLine().InvokeAsync(args);
        }

        private static RootCommand BuildCommandLine()
        {
            var certFileOption = new Option<string>(
                new[] { "--file", "-f" }, 
                () => string.Empty,
                "The path and filename of the certificate (pfx) to be written");

            var certPwdOption = new Option<string?>(
                new[] { "--password", "-pw" },
                "The required password for the certificate file (pfx)");

            var certSubjectOption = new Option<string>(
                new[] { "--subject", "-s" }, 
                () => "cn=encipherment",
                "The subject of certificate file (default value: `cn=encipherment`)");

            var newEncCertCommand = new Command(
                "newcrt", 
                "Create a new encipherment certificate (pfx)")
            {
                certFileOption,
                certPwdOption,
                certSubjectOption
            };

            newEncCertCommand.SetHandler(NewEncCertCommand.Execute, certFileOption, certPwdOption, certSubjectOption);

            var thumbprintOption = new Option<string>(
                new[] { "--thumbprint", "-tp" },
                "The thumbprint of the encipherment certificate in the certificate store used for encryption");

            var textOption = new Option<string>(
                new[] { "--text", "-tx" },
                "The text to encrypt");


            var encryptCommand = new Command("encrypt", "Execute a text")
            {
                thumbprintOption, 
                textOption
            };
            encryptCommand.SetHandler(EncryptCommand.Execute, thumbprintOption, textOption);

            var text2Option = new Option<string>(
                new[] { "--text", "-tx" },
                "The text to decrypt");

            var decryptCommand = new Command("decrypt", "Execute a text")
            {
                text2Option
            };
            decryptCommand.SetHandler(DecryptCommand.Execute, text2Option);

            var registerServiceCommand = new Command("register", "Register as windows service");
            registerServiceCommand.SetHandler(RegisterServiceCommand.ExecuteAsync);

            var unregisterServiceCommand = new Command("unregister", "unregister windows service");
            unregisterServiceCommand.SetHandler(UnregisterServiceCommand.ExecuteAsync);

            var serviceCommand = new Command("service", "Windows service related commands")
            {
                registerServiceCommand,
                unregisterServiceCommand
            };

            var adminCommand = new Command("admin", "Administrative commands")
            {
                newEncCertCommand,
                encryptCommand,
                decryptCommand,
                serviceCommand

            };

            var root = new RootCommand("VaultService cmd: Service host for vault")
            {
                adminCommand
            };
            root.SetHandler(() => RunCommand.Execute(Args));

            return root;
        }
    }
}