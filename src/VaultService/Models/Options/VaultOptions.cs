using System.Collections.Generic;

namespace VaultService.Models.Options
{
    public class VaultOptions
    {
        public string? Path { get; set; }
        public string? Address { get; set; }
        public string? StartParameters { get; set; }
        public IList<string>? UnsealKeys { get; set;}
        public string? RootToken { get; set; }
    }
}
