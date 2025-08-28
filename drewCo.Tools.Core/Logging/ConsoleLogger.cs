using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Principal;

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
  private Dictionary<string, string> LevelsToAnsiColors = new Dictionary<string, string>();

  private bool IsAnsiColorMode = false;
  private bool IsUsingColor = true;

  // --------------------------------------------------------------------------------------------------------------------------
  public ConsoleLogger(LoggerOptions options_ = null)
    : base(options_)
  {
    Console.OutputEncoding = System.Text.Encoding.UTF8;

    IsAnsiColorMode = IsAnsiSupportEnabled();

    LevelsToColors.Add(ELogLevel.INFO.ToString(), ConsoleColor.White);
    LevelsToColors.Add(ELogLevel.VERBOSE.ToString(), ConsoleColor.Blue);
    LevelsToColors.Add(ELogLevel.WARNING.ToString(), ConsoleColor.Yellow);
    LevelsToColors.Add(ELogLevel.ERROR.ToString(), ConsoleColor.Red);
    LevelsToColors.Add(ELogLevel.DEBUG.ToString(), ConsoleColor.Green);
    LevelsToColors.Add(ELogLevel.EXCEPTION.ToString(), ConsoleColor.Magenta);

    LevelsToAnsiColors.Add(ELogLevel.INFO.ToString(), "#FFFFFF");
    LevelsToAnsiColors.Add(ELogLevel.VERBOSE.ToString(), "#CD86F9");   // (205,134,249)
    LevelsToAnsiColors.Add(ELogLevel.WARNING.ToString(), "#EABE1E");   // (234,190,30)
    LevelsToAnsiColors.Add(ELogLevel.ERROR.ToString(), "#FF0000");   // (255,0,0)
    LevelsToAnsiColors.Add(ELogLevel.DEBUG.ToString(), "#61BFF9");   // (97,191,249)
    LevelsToAnsiColors.Add(ELogLevel.EXCEPTION.ToString(), "#FCA9E9");  // (252,169,233)

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

  public const int CURRENT_LINE = -1;

  // --------------------------------------------------------------------------------------------------------------------------
  public void ShowCursor(bool show)
  {
    Console.CursorVisible = show;
  }

  // --------------------------------------------------------------------------------------------------------------------------
  public int GetTop()
  {
    return Console.CursorTop;
  }

  // --------------------------------------------------------------------------------------------------------------------------
  /// <summary>
  /// Set the cursor to the top of the buffer.
  /// </summary>
  public void ToTop()
  {
    Console.CursorTop = 0;
    Console.CursorLeft = 0;
  }

  // --------------------------------------------------------------------------------------------------------------------------
  // TODO: Make this part of the interface: 
  /// <summary>
  /// Write a temporary message to the console.
  /// It will be overwritten by the next message that is written.
  /// NOTE: This does not break to a new line and will truncate output to fit on one line.
  /// This function is great for writing progress information and other things that don't need
  /// to scroll the console all over the place.
  /// </summary>
  public void WriteTemp(string msg, int line = CURRENT_LINE)
  {
    msg = StringTools.Truncate(msg, Console.BufferWidth);

    // Clear the current line:
    if (line == CURRENT_LINE)
    {
      line = Console.CursorTop;
    }
    if (line > Console.BufferHeight) {
      line = Console.BufferHeight - 1;
    }

    Console.CursorTop = line;
    Console.CursorLeft = 0;

    var blankLine = new string(' ', Console.BufferWidth);
    Console.Write(blankLine);

    Console.CursorLeft = 0;
    Console.Write(msg);

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
    if (IsUsingColor)
    {

      if (IsAnsiColorMode)
      {
        string useMessage = message;
        if (LevelsToAnsiColors.TryGetValue(level, out var color))
        {
          useMessage = Ansi.Style(message, color);
        }

        Console.Write(useMessage);
      }
      else
      {
        var startColor = Console.ForegroundColor;
        if (LevelsToColors.TryGetValue(level, out var color))
        {
          Console.ForegroundColor = color;
        }

        Console.Write(message);
        Console.ForegroundColor = startColor;
      }
    }
    else
    {
      Console.Write(message);
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


// ==============================================================================================================================
// CLANKER CODE!
public static class Ansi
{
  // ---------- Foreground ----------
  public static string Fg(string text, int r, int g, int b) =>
      $"\u001b[38;2;{r};{g};{b}m{text}\u001b[0m";

  public static string Fg(string text, uint rgbOrArgb) =>
      $"\u001b[38;2;{(rgbOrArgb >> 16) & 0xFF};{(rgbOrArgb >> 8) & 0xFF};{rgbOrArgb & 0xFF}m{text}\u001b[0m";

  public static string Fg(string text, string hex)
  {
    var (r, g, b) = FromHex(hex);
    return $"\u001b[38;2;{r};{g};{b}m{text}\u001b[0m";
  }

  // ---------- Background ----------
  public static string Bg(string text, int r, int g, int b) =>
      $"\u001b[48;2;{r};{g};{b}m{text}\u001b[0m";

  public static string Bg(string text, uint rgbOrArgb) =>
      $"\u001b[48;2;{(rgbOrArgb >> 16) & 0xFF};{(rgbOrArgb >> 8) & 0xFF};{rgbOrArgb & 0xFF}m{text}\u001b[0m";

  public static string Bg(string text, string hex)
  {
    var (r, g, b) = FromHex(hex);
    return $"\u001b[48;2;{r};{g};{b}m{text}\u001b[0m";
  }

  // ---------- Styles (can be combined) ----------
  public static string Bold(string text) => $"\u001b[1m{text}\u001b[22m";
  public static string Underline(string text) => $"\u001b[4m{text}\u001b[24m";
  public static string Reversed(string text) => $"\u001b[7m{text}\u001b[27m";
  public static string Dim(string text) => $"\u001b[2m{text}\u001b[22m";
  public static string Italic(string text) => $"\u001b[3m{text}\u001b[23m";

  // ---------- Compose manually ----------
  public static string Style(string text, string fgHex = null, string bgHex = null,
                             bool bold = false, bool underline = false, bool italic = false, bool reversed = false, bool dim = false)
  {
    string seq = "";
    if (bold) seq += "\u001b[1m";
    if (dim) seq += "\u001b[2m";
    if (italic) seq += "\u001b[3m";
    if (underline) seq += "\u001b[4m";
    if (reversed) seq += "\u001b[7m";
    if (!string.IsNullOrEmpty(fgHex))
    {
      var (r, g, b) = FromHex(fgHex);
      seq += $"\u001b[38;2;{r};{g};{b}m";
    }
    if (!string.IsNullOrEmpty(bgHex))
    {
      var (r, g, b) = FromHex(bgHex);
      seq += $"\u001b[48;2;{r};{g};{b}m";
    }

    return $"{seq}{text}\u001b[0m";
  }

  // ---------- Reset ----------
  public static string Reset() => "\u001b[0m";

  // ---------- Helpers ----------
  private static (int r, int g, int b) FromHex(string hex)
  {
    if (hex.StartsWith("#")) hex = hex[1..];
    if (hex.Length == 8) hex = hex[2..]; // strip alpha if #AARRGGBB
    int r = Convert.ToInt32(hex.Substring(0, 2), 16);
    int g = Convert.ToInt32(hex.Substring(2, 2), 16);
    int b = Convert.ToInt32(hex.Substring(4, 2), 16);
    return (r, g, b);
  }
}


