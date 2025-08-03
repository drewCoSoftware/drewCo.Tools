using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace drewCo.Tools.Logging;

// ============================================================================================================================== 
public class ConsoleLogger : ILogger, IDisposable
{
  private const int DEFAULT_LEFT = -1;

  public LoggerOptions Options { get; private set; } = null!;

  private HashSet<string> UseLevels = null!;

  /// <summary>
  /// This event is fired when something is logged.
  /// TODO: Move this to the interface? / Log class?
  /// </summary>
  public EventHandler<LogEventArgs> OnLogged = null;

  /// <summary>
  /// Lock that makes sure that this logger is threadsafe!
  /// </summary>
  private object WriteLock = new object();


  // --------------------------------------------------------------------------------------------------------------------------
  public ConsoleLogger() : this(new LoggerOptions())
  { }

  // --------------------------------------------------------------------------------------------------------------------------
  public ConsoleLogger(LoggerOptions options_)
  {
    Options = options_;

    UseLevels = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    foreach (string level in Options.LogLevels.Distinct())
    {
      UseLevels.Add(level);
    }
  }

  // --------------------------------------------------------------------------------------------------------------------------
  public void WriteLine(ELogLevel level, object message)
  {
    WriteLine(level.ToString(), message);
  }

  // --------------------------------------------------------------------------------------------------------------------------
  /// <param name="level">The level of hte log to write (case insensitive).  If the level isn't set in the options
  /// it will be ignored.</param>
  public void WriteLine(string level, object message)
  {
    string content = Log.ObjectToString(message);
    // Only log the levels that we currently support.
    if (!Options.LogLevels.Contains(level))
    {
      return;
    }

    try
    {
      string useMsg = Options.FormatMessage(level, content, true);
      Write(useMsg);
    }
    catch (Exception ex)
    {
      // We have a catch-all here so that failure to write to the log will not crash the application.
      System.Diagnostics.Debug.WriteLine("Could not write log!");
      System.Diagnostics.Debug.WriteLine(ex.Message);
    }

    OnLogged?.Invoke(this, new LogEventArgs(level, content));
  }


  // --------------------------------------------------------------------------------------------------------------------------
  private void Write(object message)
  {
    Console.WriteLine(message);
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
  { }

  // --------------------------------------------------------------------------------------------------------------------------
  /// <param name="introMessage">Introduction error message to print to the log. If null, no message will be used.</param>
  /// <returns>Path to the ExceptionDetail that was saved, if any.</returns>
  public string? Exception(Exception? ex, string? introMessage = "An unhandled exception was encountered!")
  {
    // NOTE: I think that this might be more of the domain of 'ExceptionLogger' or something like that....
    // I have added an 'Exception' level log type too.
    string msg = introMessage + Environment.NewLine;
    msg += ex.Message;

    Console.WriteLine(msg);

    return null;
    //lock (ExceptionLock)
    //{
    //  if (ex == null) { return null; }

    //  string exPath = FileTools.GetSequentialFileName(Options.ExceptionsDir, "ExceptionDetail", ".xml");

    //  var detail = new ExceptionDetail(ex);
    //  detail.ToXML().Save(exPath);

    //  string relPath = Path.GetRelativePath(FileTools.GetAppDir(), exPath);

    //  if (!string.IsNullOrWhiteSpace(introMessage))
    //  {
    //    Error(introMessage);
    //  }
    //  Error(ex.Message);
    //  Error($"An ExceptionDetail was written to: {relPath}!");

    //  return relPath;
    //}
  }


  //// --------------------------------------------------------------------------------------------------------------------------
  ///// <inheritdoc path="WriteToConsole"/>     /*How does this work?*/
  //public void WriteToConsole(object message)
  //{
  //  WriteToConsole(message, DEFAULT_LEFT);
  //}

  //// --------------------------------------------------------------------------------------------------------------------------
  ///// <summary>
  ///// Write a message to the console only.
  ///// This does not add a newline.
  ///// </summary>
  //public void WriteToConsole(object message, int left)
  //{
  //  if (Options.LogToConsole)
  //  {
  //    if (left == DEFAULT_LEFT) { left = Console.CursorLeft; }

  //    int top = Console.CursorTop;
  //    WriteToConsole(message, left, top);
  //  }
  //}

  //// --------------------------------------------------------------------------------------------------------------------------
  ///// <summary>
  ///// Write a message to the console only.
  ///// This does not add a newline.
  ///// </summary>
  //public void WriteToConsole(object message, int left, int top, bool clearLineFirst = true)
  //{
  //  if (Options.LogToConsole)
  //  {
  //    if (clearLineFirst)
  //    {
  //      int w = Console.BufferWidth;
  //      Console.Write(new string((char)0, w));
  //    }

  //    message = FormatMessage(null, message);
  //    Console.SetCursorPosition(left, top);
  //    Console.Write(message);
  //  }
  //}
}

