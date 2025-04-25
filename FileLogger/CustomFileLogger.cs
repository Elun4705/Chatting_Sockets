// Created by Andy Huo during a lab session in CS 3500.  This is not meant to be used outside of school.
using Microsoft.Extensions.Logging;
using static System.Net.Mime.MediaTypeNames;
using System.IO;

namespace FileLogger
{
    /// <summary>
    /// A class for a CustomFileLogger which implements the ILogger interface and is meant to be used in our Chat/communication project.
    /// </summary>
    public class CustomFileLogger : ILogger
    {
        private readonly string categoryName;
        string? _FileName;
        public CustomFileLogger(string categoryName)
        {
            this.categoryName = categoryName;
        }
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            throw new NotImplementedException();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This is a method for logging messages to a file. It takes in a LogLevel enum value, an EventId, a TState value 
        /// (which is a generic type parameter), an Exception (which can be null), and a Func that takes in a TState and an Exception 
        /// and returns a formatted string.

        /// Within the method, it first sets the _FileName variable to the path where the log file should be stored.It then appends the formatted 
        /// message to the log file using the File.AppendAllText method.This method writes the specified text to a file, creating the file if 
        /// \it does not already exist, and appending the text to the end of the file if it does exist.
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <param name="logLevel"></param>
        /// <param name="eventId"></param>
        /// <param name="state"></param>
        /// <param name="exception"></param>
        /// <param name="formatter"></param>
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            _FileName = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + 
                Path.DirectorySeparatorChar + 
                $"Chat History-{categoryName}.log";

            File.AppendAllText(_FileName, formatter(state, exception));
        }
    }

}