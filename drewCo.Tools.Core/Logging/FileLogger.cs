using System;
using System.IO;
using System.Text;

namespace drewCo.Tools.Logging
{
  // ============================================================================================================================== 
  /// <summary>
  /// Logs messages to disk.
  /// </summary>
  public class FileLogger : ConsoleLogger, ILogger, IDisposable
  {
    private object WriteLock = new object();
    private object ExceptionLock = new object();

    private FileLoggerOptions FileOptions = null;

    private FileStream? LogStream = null;

    /// <summary>
    /// Path to where the logs are currently being written to.
    /// </summary>
    public string FilePath { get; private set; }

    // --------------------------------------------------------------------------------------------------------------------------
    public FileLogger(FileLoggerOptions options_)
      : base(new LoggerOptions(options_.LogLevels))
    {
      FileOptions = options_;
      FilePath = OpenLogStream(FileOptions);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Open the log stream + return the path of the file where it is being written to.
    /// </summary>
    private string OpenLogStream(FileLoggerOptions options)
    {
      FileTools.CreateDirectory(options.LogsDir);
      FileTools.CreateDirectory(options.ExceptionsDir);

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

      var fileMode = FileMode.Create;
      LogStream = new FileStream(res, fileMode, FileAccess.ReadWrite, FileShare.Read);

      return res;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public override void WriteToLog(string message)
    {
      var data = Encoding.UTF8.GetBytes(message);
      LogStream.Write(data, 0, data.Length);
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

        string exPath = FileTools.GetSequentialFileName(FileOptions.ExceptionsDir, "ExceptionDetail", ".xml");

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
    private void CloseFileStream()
    {
      if (LogStream != null)
      {
        LogStream.Dispose();
      }
      LogStream = null;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public void Dispose()
    {
      CloseFileStream();
    }


  }

}
