using System;
using System.IO;
using System.Text;

namespace drewCo.Tools.Logging
{
  // ============================================================================================================================== 
  /// <summary>
  /// Logs messages to disk.
  /// </summary>
  public class FileLogger : ILogger, IDisposable
  {

    /// <summary>
    /// This event is fired when something is logged.
    /// TODO: Move this to the interface? / Log class?
    /// </summary>
    public EventHandler<LogEventArgs> OnLogged = null;

    private object WriteLock = new object();
    private object ExceptionLock = new object();

    private FileLoggerOptions Options = null;

    private FileStream? LogStream = null;

    ///// <summary>
    ///// If file logging is active, this is the path where the file will be written to.
    ///// </summary>
    //public string LogFilePath { get { return LogStream == null ? null : Options.LogFilePath; } }

    /// <summary>
    /// Where are exceptions logged to?
    /// </summary>
    // public string ExceptionsDir { get; private set; }

    public string FilePath { get; private set; }

    // --------------------------------------------------------------------------------------------------------------------------
    public FileLogger(FileLoggerOptions options_)
    {
      Options = options_;
      FilePath = OpenLogStream(Options);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Open the log stream + return the path of the file where it is being written to.
    /// </summary>
    private string OpenLogStream(FileLoggerOptions options)
    {
      string res = Path.Combine(options.LogsDir, options.FileName);
      switch (options.Mode)
      {
        case EFileLoggerMode.Overwrite:
        case EFileLoggerMode.Append:
          break;

        case EFileLoggerMode.Sequential:
          res = FileTools.GetSequentialFileName(res);
          break;

        default:
          throw new ArgumentOutOfRangeException();
      }

      if (options.Mode == EFileLoggerMode.Overwrite)
      {
      }

      var fileMode = FileMode.Create;
      if (options.Mode == EFileLoggerMode.Overwrite)
      {
        fileMode = FileMode.Truncate;
      }
      LogStream = new FileStream(res, fileMode, FileAccess.ReadWrite, FileShare.Read);


      return res;
    }


    // --------------------------------------------------------------------------------------------------------------------------
    public void WriteLine(ELogLevel level, object message)
    {
      WriteLine(level.ToString(), message);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <param name="level">The level of the log to write (case insensitive).  If the level isn't set in the options
    /// it will be ignored.</param>
    public void WriteLine(string level, object message)
    {
      // Only log the levels that we currently support.
      if (!Options.HasLevel(level))
      {
        return;
      }

      try
      {
        string useMsg = Options.FormatMessage(level, message, true);
        Write(useMsg);
      }
      catch (Exception ex)
      {
        // We have a catch-all here so that failure to write to the log will not crash the application.
        System.Diagnostics.Debug.WriteLine("Could not write log!");
        System.Diagnostics.Debug.WriteLine(ex.Message);
      }

      OnLogged?.Invoke(this, new LogEventArgs(level, message));
    }

    // --------------------------------------------------------------------------------------------------------------------------
    private void Write(object message)
    {
      string useMsg = Log.ObjectToString(message);  

      var data = Encoding.UTF8.GetBytes(useMsg);
      LogStream.Write(data, 0, data.Length);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public void Debug(object message)
    {
      WriteLine(ELogLevel.DEBUG, message);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public void Info(object message)
    {
      WriteLine(ELogLevel.INFO, message);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public void Error(object message)
    {
      WriteLine(ELogLevel.ERROR, message);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public void Warning(object message)
    {
      WriteLine(ELogLevel.WARNING, message);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public void Verbose(object message)
    {
      WriteLine(ELogLevel.VERBOSE, message);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public void Dispose()
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
    public string GetLogFileContent()
    {
      lock (WriteLock)
      {
        var curPos = LogStream.Position;
        LogStream.Seek(0, SeekOrigin.Begin);

        using (var reader = new StreamReader(LogStream, leaveOpen: true))
        {
          string res = reader.ReadToEnd();

          LogStream.Seek(curPos, SeekOrigin.Begin);
          return res;
        }
      }
    }
    // --------------------------------------------------------------------------------------------------------------------------
    /// <param name="introMessage">Introduction error message to print to the log. If null, no message will be used.</param>
    /// <returns>Path to the ExceptionDetail that was saved, if any.</returns>
    public string? Exception(Exception? ex, string? introMessage = "An unhandled exception was encountered!")
    {
      lock (ExceptionLock)
      {
        if (ex == null) { return null; }

        string exPath = FileTools.GetSequentialFileName(Options.ExceptionsDir, "ExceptionDetail", ".xml");

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

  }

}
