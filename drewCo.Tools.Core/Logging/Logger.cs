using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;

namespace drewCo.Tools.Logging
{

    // ============================================================================================================================
    public enum EFileLoggerMode
    {
        Invalid = 0,

        /// <summary>
        /// The currently existing log file will be appended to.
        /// </summary>
        Append,

        /// <summary>
        /// The currently existing log file will be overwritten.
        /// </summary>
        Overwrite,

        /// <summary>
        /// A new, sequential file name will be used each time the logger is initialized.
        /// </summary>
        Sequential
    }

    // ============================================================================================================================
    public class FileLoggerOptions : LoggerOptions
    {
        public EFileLoggerMode Mode { get; private set; } = EFileLoggerMode.Invalid;
        public string LogsDir { get; set; }
        public string FileName { get; set; }
        public string ExceptionsDir { get; set; }

        // --------------------------------------------------------------------------------------------------------------------------
        public FileLoggerOptions(IEnumerable<string> levels_, string logsDir_, string logFileName_, EFileLoggerMode mode_)
          : this(levels_, logsDir_, logFileName_, Path.Combine(logsDir_, "exceptions"), mode_)
        { }

        // --------------------------------------------------------------------------------------------------------------------------
        public FileLoggerOptions(IEnumerable<string> levels_, string logsDir_, string logFileName_, string exceptionsDir_, EFileLoggerMode mode_)
          : base(levels_)
        {
            LogsDir = logsDir_;
            FileName = logFileName_;
            ExceptionsDir = exceptionsDir_;
            Mode = mode_;
        }
    }


    // ============================================================================================================================
    public class LoggerOptions
    {
        public HashSet<string> LogLevels { get; private set; } = default!;

        // --------------------------------------------------------------------------------------------------------------------------
        /// <param name="logLevels_">The log levels that should be included.  This can be omitted to use all log levels.</param>
        public LoggerOptions(IEnumerable<string> logLevels_ = null)
        {
            LogLevels = (logLevels_ ?? new HashSet<string>()).ToHashSet();
        }

        // --------------------------------------------------------------------------------------------------------------------------
        public bool HasLevel(ELogLevel level)
        {
            return HasLevel(level.ToString());
        }

        // --------------------------------------------------------------------------------------------------------------------------
        public bool HasLevel(string level)
        {
            bool res = LogLevels.Count == 0 || LogLevels.Contains(level);
            return res;
        }


    }

    // ============================================================================================================================
    /// <summary>
    /// Standard log levels.  Note that users may use other levels as they see fit.
    /// </summary>
    public enum ELogLevel
    {
        /// <summary>
        /// General information.
        /// </summary>
        INFO,

        /// <summary>
        /// Something is not quite right, but not critical.
        /// </summary>
        WARNING,

        /// <summary>
        /// There was an error.
        /// </summary>
        ERROR,

        /// <summary>
        /// Extra wordy logs, ususally for diagnostics or extra descriptions.
        /// </summary>
        VERBOSE,

        /// <summary>
        /// Messages for debug builds of your application.  Kind of like VERBOSE, but maybe more to the point.
        /// </summary>
        DEBUG,

        /// <summary>
        /// Used when exceptions are encountered so they can be summarized, etc.
        /// </summary>
        EXCEPTION
    }


    // ============================================================================================================================
    public class LogEventArgs : EventArgs
    {

        // --------------------------------------------------------------------------------------------------------------------------
        public LogEventArgs(string level_, object message_)
        {
            Level = level_;
            Message = message_;
        }

        public readonly string Level;
        public readonly object Message;
    }

}
