using System;
using System.IO;

namespace WindowsGameAutomationTools.Files
{
    public static class SystemIOHelper
    {
        public static void SafeCreateDirectory(string directoryName)
        {
            if (string.IsNullOrWhiteSpace(directoryName))
                throw new ArgumentNullException(nameof(directoryName));

            if (Directory.Exists(directoryName) || File.Exists(directoryName))
                return;

            Directory.CreateDirectory(directoryName);
        }
    }
}
