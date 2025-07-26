using System.Collections.Generic;
using System;

namespace drewCo.Tools.Logging
{
  // ========================================================================================================
  /// <summary>
  /// Used for static calls to logging anywhere in your application.
  /// No need to pass ILogger instances around to your components.
  /// This was inspired by the convenience of logging in python.
  /// </summary>
  public static class Log
  {
    // ------------------------------------------------------------------------------------------------------
    /// <summary>
    /// The loggers that are currently attatched...
    /// </summary>
    private static List<ILogger> _Loggers = new List<ILogger>();


    // ------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Add a logger to the set of loggers.
    /// </summary>
    public static void AddLogger(ILogger logger_)
    {
      if (_Loggers.Contains(logger_))
      {
        throw new InvalidOperationException("This logger has already been added!");
      }
      _Loggers.Add(logger_);
    }

    // ------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Add a new logger that will be invoked with each call.
    /// </summary>
    public static void WriteLine(ILogger logger_)
    {
      _Loggers.Add(logger_);
    }


    // ------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Log a message with a standard logging level.
    /// NOTE: Use Log.Info/Debug/Warning for convenience.
    /// </summary>
    public static void WriteLine(ELogLevel level, string message)
    {
      foreach (var item in _Loggers)
      {
        item.WriteLine(level, message);
      }
    }

    // ------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Log a message with a custom logging level.
    /// </summary>
    public static void AddMessage(string level, string message)
    {
      foreach (var item in _Loggers)
      {
        item.WriteLine(level, message);
      }
    }

    // ------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Log an info level message.
    /// </summary>
    public static void Info(string message)
    {
      foreach (var item in _Loggers)
      {
        item.Info(message);
      }
    }

    // ------------------------------------------------------------------------------------------------------
    public static void Verbose(string message)
    {
      foreach (var item in _Loggers)
      {
        item.Verbose(message);
      }
    }

    // ------------------------------------------------------------------------------------------------------
    public static void Warning(string message)
    {
      foreach (var item in _Loggers)
      {
        item.Verbose(message);
      }
    }

    // ------------------------------------------------------------------------------------------------------
    public static void Error(string message)
    {
      foreach (var item in _Loggers)
      {
        item.Debug(message);
      }
    }

    // ------------------------------------------------------------------------------------------------------
    public static void Debug(string message)
    {
      foreach (var item in _Loggers)
      {
        item.Debug(message);
      }
    }

    // ------------------------------------------------------------------------------------------------------
    public static void Exception(Exception ex)
    {
      foreach (var item in _Loggers)
      {
        item.Exception(ex);
      }
    }

  }

}
