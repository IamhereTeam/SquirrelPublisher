using System;
using Microsoft.VisualStudio.Shell;

namespace SquirrelPublisher
{
    public static class Settings
    {
        private static IServiceProvider _provider;
        private static string _name;

        public static void Initialize(Package provider, string name)
        {
            _provider = provider;
            _name = name;
        }

        public static string Username { get; set; } = "clickonce";
        public static string Password { get; set; } = "1111";
        public static string FtpUrl { get; set; } = "ftp://127.0.0.1/CSTEST/";
        public static string LockalDir { get; set; } = @"D:\q\CSTEST";
        public static Version CurrentVersion { get; set; } = new Version(3, 0, 17);
        public static string ProjectUniqueName { get; set; } = "ConsoleApp1\\ConsoleApp1.csproj";

        public static string ConfigurationName { get; set; } = "Release";
        public static string PlatformTarget { get; set; } = "Any CPU";
    }
}
