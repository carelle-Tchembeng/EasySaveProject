// EasyLog/Helpers/LogFileNamer.cs

using System;
using System.IO;

namespace EasyLog.Helpers
{
    
    /// Static utility for generating log file names dynamically based on strategy.
    
    public static class LogFileNamer
    {
        
        /// Returns the log file name for the specified date and extension.
        
        public static string GetFileName(DateTime date, string extension, string dateFormat = "yyyy-MM-dd")
        {
            return date.ToString(dateFormat) + extension;
        }

        
        /// Returns the full absolute path for a log file.
        
        public static string GetFullPath(string logDirectory, DateTime date, string extension, string dateFormat = "yyyy-MM-dd")
        {
            string fileName = GetFileName(date, extension, dateFormat);
            return Path.Combine(logDirectory, fileName);
        }
    }
}