using System.Collections.Generic;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace drewCo.Tools.Logging
{

  // ============================================================================================================================
  /// <summary>
  /// This is used for our logging purposes.
  /// REFACTOR: This logger is useful and should find its way to shared space!
  /// </summary>
  public class Logger : IDisposable
  {
    private const int DEFAULT_LEFT = -1;

    private bool LogToFile { get { return LogStream != null; } }

    public LoggerOptions Options { get; private set; } = null!;
    private FileStream? LogStream = null;
    private bool UseAllLogLevels = false;

    private HashSet<string> UseLevels = null!;

    private object ExceptionLock = new object();

    /// <summary>
    /// This event is fired when something is logged.
    /// </summary>
    public EventHandler<LogEventArgs> OnLogged = null;

    // --------------------------------------------------------------------------------------------------------------------------
    public Logger() : this(new LoggerOptions())
    { }

    // --------------------------------------------------------------------------------------------------------------------------
    public Logger(LoggerOptions options_)
    {
      Options = options_;
      if (Options.LogFilePath != null)
      {
        LogStream = new FileStream(Options.LogFilePath, FileMode.Create, FileAccess.Write);
      }
      FileTools.CreateDirectory(Options.ExceptionsDir);

      UseLevels = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
      foreach (string level in Options.LogLevels.Distinct())
      {
        UseLevels.Add(level);
      }
      UseAllLogLevels = Options.LogLevels.Contains(LoggerOptions.ALL_LEVELS);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public void Log(ELogLevel level, string message)
    {
      Log(level.ToString(), message);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <param name="level">The level of hte log to write (case insensitive).  If the level isn't set in the options
    /// it will be ignored.</param>
    public void Log(string level, string message)
    {
      // Only log the levels that we currently support.
      if (!UseAllLogLevels && !Options.LogLevels.Contains(level))
      {
        return;
      }

      try
      {
        string useMsg = FormatMessage(level, message);
        Write(useMsg);
      }
      catch (Exception ex)
      {
        // We have a catch-all here so that failure to write to the log will not crash the application.
        Debug.WriteLine("Could not write log!");
        Debug.WriteLine(ex.Message);
      }

      OnLogged?.Invoke(this, new LogEventArgs(level, message));
    }

    // --------------------------------------------------------------------------------------------------------------------------
    private string FormatMessage(string? level, string message)
    {
      // TODO: This is where we would check the options for the format...
      // For now we will just return everything.
      return message;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    private void Write(string message)
    {
      if (Options.LogToConsole)
      {
        Console.WriteLine(message);
      }
      if (LogToFile)
      {
        int useLines = message == Environment.NewLine ? 0 : 1;
        EZWriter.RawString(LogStream, message, useLines);

        // TODO: Flush every n-messages.
        LogStream!.Flush();
      }
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Adds a blank line to the log...
    /// </summary>
    public void NewLine()
    {
      Write(Environment.NewLine);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Alias for <see cref="NewLine"/>
    /// </summary>
    public void Info()
    {
      NewLine();
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public void Info(string message)
    {
      Log(ELogLevel.INFO, message);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public void Error(string message)
    {
      Log(ELogLevel.ERROR, message);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public void Warning(string message)
    {
      Log(ELogLevel.WARNING, message);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public void Verbose(string message)
    {
      Log(ELogLevel.VERBOSE, message);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public void Dispose()
    {
      CloseFileStream();
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// This is used at the end of the process so we can email the log, make copies, etc.
    /// </summary>
    public void StopFileLogging()
    {
      CloseFileStream();
    }

    // --------------------------------------------------------------------------------------------------------------------------
    private void CloseFileStream()
    {
      if (LogStream != null)
      {
        LogStream.Dispose();
      }
      LogStream = null;
    }


    // --------------------------------------------------------------------------------------------------------------------------
    /// <param name="introMessage">Introduction error message to print to the log. If null, no message will be used.</param>
    /// <returns>Path to the ExceptionDetail that was saved, if any.</returns>
    public string? LogException(Exception? ex, string? introMessage = "An unhandled exception was encountered!")
    {
      lock (ExceptionLock)
      {
        if (ex == null) { return null; }

        string exPath = FileTools.GetSequentialFileName(Options.ExceptionsDir, "ExceptionDetail", ".xml", 50000);

        var detail = new ExceptionDetail(ex);
        detail.ToXML().Save(exPath);

        string relPath = Path.GetRelativePath(FileTools.GetAppDir(), exPath);

        if (!string.IsNullOrWhiteSpace(introMessage))
        {
          Error(introMessage);
        }
        Error(ex.Message);
        Error($"An ExceptionDetail was written to: {relPath}!");

        return relPath;
      }
    }


    // --------------------------------------------------------------------------------------------------------------------------
    /// <inheritdoc path="WriteToConsole"/>     /*How does this work?*/
    public void WriteToConsole(string message)
    {
      WriteToConsole(message, DEFAULT_LEFT);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Write a message to the console only.
    /// This does not add a newline.
    /// </summary>
    public void WriteToConsole(string message, int left)
    {
      if (Options.LogToConsole)
      {
        if (left == DEFAULT_LEFT) { left = Console.CursorLeft; }

        int top = Console.CursorTop;
        WriteToConsole(message, left, top);
      }
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Write a message to the console only.
    /// This does not add a newline.
    /// </summary>
    public void WriteToConsole(string message, int left, int top, bool clearLineFirst = true)
    {
      if (Options.LogToConsole)
      {
        if (clearLineFirst)
        {
          int w = Console.BufferWidth;
          Console.Write(new string((char)0, w));
        }

        message = FormatMessage(null, message);
        Console.SetCursorPosition(left, top);
        Console.Write(message);
      }
    }
  }


  // ============================================================================================================================
  public class LoggerOptions
  {

    public const string ALL_LEVELS = "ALL";

    /// <summary>
    /// Should log messages go to the console?
    /// </summary>
    public bool LogToConsole { get; set; } = true;

    /// <summary>
    /// Optional path to file that should contain log messages.
    /// </summary>
    public string? LogFilePath { get; set; }

    /// <summary>
    /// If true, existing log files will be appended to, vs. truncating the whole file.
    /// </summary>
    public bool AppendLog { get; set; } = false;

    /// <summary>
    /// Which log levels should be used?  If set to 'ALL', all messages will be used.
    /// </summary>
    public string[] LogLevels { get; set; } = new[] { ALL_LEVELS };

    /// <summary>
    /// Directory that excception logs should be placed in.
    /// </summary>
    public string ExceptionsDir { get; set; } = "Exceptions";

    ///// <summary>
    ///// Instructs the logger how to format the messages. (Level, Timestamps, etc.)
    ///// This is optional, default will just show the message.
    ///// </summary>
    ///// <remarks>
    ///// Not really supported at this time. 
    ///// </remarks>
    //public string? LineFormat { get; set; } = null;
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
    DEBUG
  }


  // ============================================================================================================================
  public class LogEventArgs : EventArgs
  {

    // --------------------------------------------------------------------------------------------------------------------------
    public LogEventArgs(string level_, string message_)
    {
      Level = level_;
      Message = message_;
    }

    public readonly string Level;
    public readonly string Message;
  }

}
