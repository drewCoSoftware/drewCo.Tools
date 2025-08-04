using System;
namespace drewCo.Tools.Logging;

// ============================================================================================================================== 
public class ConsoleLogger : LoggerBase, ILogger, IDisposable
{
  // --------------------------------------------------------------------------------------------------------------------------
  public ConsoleLogger(LoggerOptions options_ = null)
    : base(options_)
  { }

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
    string content = ObjectToString(message);

    // Only log the levels that we currently support.
    bool logIt = HasLogLevel(level);
    if (!logIt)
    {
      return;
    }

    try
    {
      string useMsg = FormatMessage(level, content, true);
      WriteToLog(useMsg);
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
  public override void WriteToLog(string message)
  {
    Console.WriteLine(message);
  }

  // --------------------------------------------------------------------------------------------------------------------------
  /// <summary>
  /// Adds a blank line to the log...
  /// </summary>
  public void NewLine()
  {
    WriteToLog(Environment.NewLine);
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
  }


}

