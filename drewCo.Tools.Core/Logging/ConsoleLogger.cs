using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace drewCo.Tools.Logging;


// ============================================================================================================================== 
public class ConsoleLogger : LoggerBase, ILogger, IDisposable
{
  //ANSI MODE DETECTION:
  [DllImport("kernel32.dll", SetLastError = true)]
  static extern IntPtr GetStdHandle(int nStdHandle);

  [DllImport("kernel32.dll", SetLastError = true)]
  static extern bool GetConsoleMode(IntPtr hConsoleHandle, out int mode);

  [DllImport("kernel32.dll", SetLastError = true)]
  static extern bool SetConsoleMode(IntPtr hConsoleHandle, int mode);

  const int STD_OUTPUT_HANDLE = -11;
  const int ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;


  private Dictionary<string, ConsoleColor> LevelsToColors = new Dictionary<string, ConsoleColor>();
  private bool IsUsingAnsiColor = false;
  private bool IsUsingColor = true;

  // --------------------------------------------------------------------------------------------------------------------------
  public ConsoleLogger(LoggerOptions options_ = null)
    : base(options_)
  {
    Console.OutputEncoding = System.Text.Encoding.UTF8;

    IsUsingAnsiColor = IsAnsiSupportEnabled() & false;

    LevelsToColors.Add(ELogLevel.INFO.ToString(), ConsoleColor.White);
    LevelsToColors.Add(ELogLevel.VERBOSE.ToString(), ConsoleColor.Blue);
    LevelsToColors.Add(ELogLevel.WARNING.ToString(), ConsoleColor.Yellow);
    LevelsToColors.Add(ELogLevel.ERROR.ToString(), ConsoleColor.Red);
    LevelsToColors.Add(ELogLevel.DEBUG.ToString(), ConsoleColor.Green);
    LevelsToColors.Add(ELogLevel.EXCEPTION.ToString(), ConsoleColor.Magenta);
  }

  // --------------------------------------------------------------------------------------------------------------------------
  private bool IsAnsiSupportEnabled()
  {
    var handle = GetStdHandle(STD_OUTPUT_HANDLE);
    if (GetConsoleMode(handle, out int mode))
    {
      SetConsoleMode(handle, mode | ENABLE_VIRTUAL_TERMINAL_PROCESSING);
      return true;
    }
    else
    {
      return false;
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
      WriteToLog(level, useMsg);
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
  public override void WriteToLog(string level, string message)
  {
    var startColor = Console.ForegroundColor;
    if (!IsUsingAnsiColor) {
      if (LevelsToColors.TryGetValue(level, out var color)) {
        Console.ForegroundColor = color;
      }
    }

    Console.Write(message);

    if (!IsUsingAnsiColor) { 
      Console.ForegroundColor = startColor;
    }
  }

  // --------------------------------------------------------------------------------------------------------------------------
  /// <summary>
  /// Adds a blank line to the log...
  /// </summary>
  public void NewLine()
  {
    WriteToLog(string.Empty, Environment.NewLine);
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

