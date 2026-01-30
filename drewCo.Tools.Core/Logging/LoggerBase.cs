using System;
using System.Collections.Generic;
using System.Linq;

namespace drewCo.Tools.Logging;

// ==============================================================================================================================
/// <summary>
/// Base functionality for ILoggers
/// </summary>
public abstract class LoggerBase
{
  public LoggerOptions Options { get; private set; } = null!;

  private HashSet<string> UseLevels = null!;

  /// <summary>
  /// This event is fired when something is logged.
  /// </summary>
  public EventHandler<LogEventArgs> OnLogged = null;


  // ------------------------------------------------------------------------------------------------------
  public LoggerBase(LoggerOptions options_)
  {
    Options = options_ ?? new LoggerOptions();
    Options.LogLevels = Options.LogLevels ?? new HashSet<string>();

    UseLevels = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    foreach (string level in Options.LogLevels.Distinct())
    {
      UseLevels.Add(level);
    }

  }

  // ------------------------------------------------------------------------------------------------------
  public string ObjectToString(object e)
  {
    return e?.ToString();
  }

  // ------------------------------------------------------------------------------------------------------
  public bool HasLogLevel(string level)
  {
    bool res = Options.LogLevels.Count == 0 || Options.LogLevels.Contains(level);
    return res;
  }

  // --------------------------------------------------------------------------------------------------------------------------
  // NOTE: Message formatters could be added later, as needed, and they will be needed.
  protected virtual string FormatMessage(string level, object input, bool addNewline)
  {
    string msg = ObjectToString(input);
    string res = msg + (addNewline ? Environment.NewLine : null);
    return res;
  }

  /// <summary>
  /// This function is used to write the formatted message to the log.
  /// </summary>
  public abstract void WriteToLog(string level, string message);
}

