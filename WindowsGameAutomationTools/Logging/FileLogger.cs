using System;
using System.IO;
using System.Text;

namespace WindowsGameAutomationTools.Logging
{
    public class FileLogger
    {
        public const string LOG_FILE_EXTENSION = ".log";
        public string LoggerFileName => $"{FileNamePrefix}_{CreationTime.Ticks}{LOG_FILE_EXTENSION}";

        private string FileNamePrefix { get; set; }
        private DateTime CreationTime { get; set; }
        private StreamWriter LogStream { get; set; }

        private string LogPrefix { get; set; }

        private StringBuilder LogContentsStringBuilder { get; set; }
        public string LogContents => LogContentsStringBuilder.ToString();

        public FileLogger(DateTime creationTime, string fileNamePrefix, string logPrefix)
        {
            FileNamePrefix = fileNamePrefix;
            LogPrefix = logPrefix;
            CreationTime = creationTime;

            LogStream = new StreamWriter(LoggerFileName);
            LogStream.AutoFlush = true;

            LogContentsStringBuilder = new StringBuilder();
        }

        ~FileLogger()
        {
            if (LogStream != null && LogStream.BaseStream.CanWrite)
            {
                LogStream.Close();
            }
        }

        public void Log(string contentsToLog)
        {
            string logString = $"{LogPrefix}: {contentsToLog}";
            LogStream.WriteLine(logString);
            Console.WriteLine(logString);
            LogContentsStringBuilder.AppendLine(logString);
        }
    }
}
