using System;
using System.Diagnostics;

namespace drewCo.Tools.Logging;

// ==================================================================================================================
/// <summary>
/// Convenient way to recycle console lines (vs. scrolling) when outputing to the console?
/// </summary>
public class ConsoleHelper
{
  private int Width = -1;
  private string ClearData = null!;
  private bool CanWrite = false;

  private char[] LineBuffer = null!;

  private Stopwatch Clock = Stopwatch.StartNew();
  private long LastWrite = long.MinValue;
  private long UpdateInterval = 0;

  // --------------------------------------------------------------------------------------
  /// <param name="updateInterval">How often will we update the message?  This helps to prevent 'blink' when writing messages to the same line often. </param>
  public ConsoleHelper(int updateInterval = 250)
  {
    UpdateInterval = updateInterval;
    LastWrite = -UpdateInterval;

    Width = Console.BufferWidth;
    if (Width > 0)
    {
      CanWrite = true;
      ClearData = new string(' ', Width);
      LineBuffer = new char[Width];
    }
  }

  // --------------------------------------------------------------------------------------
  // Just need a way to mark the current line....
  public void ToLine(string message, bool clearFirst = true)
  {
    if (!CanWrite) { return; }
    if (DelayWrite()) { return; }

    long now = Clock.ElapsedMilliseconds;
    long elapsed = now - LastWrite;
    if (elapsed > UpdateInterval)
    {
      LastWrite = now;

      Console.CursorVisible = false;

      while (message.EndsWith(Environment.NewLine))
      {
        message = message.Substring(0, message.Length - Environment.NewLine.Length);
      }

      if (clearFirst)
      {
        ClearLine();
      }
      if (message.Length > Width)
      {
        message = message.Substring(0, Width);
      }
      Console.Write(message);
    }
  }

  // --------------------------------------------------------------------------------------
  public void ClearLine()
  {
    if (!CanWrite) { return; }

    Console.CursorLeft = 0;
    Console.Write(ClearData);
    Console.CursorLeft = 0;
  }

  // --------------------------------------------------------------------------------------
  /// <summary>
  /// Used to move the console to the next line + optionally makes the cursor visible....
  /// </summary>
  public void Next(bool makeCursorVisible = true)
  {
    if (!CanWrite) { return; }

    Console.Write(Environment.NewLine);
    if (makeCursorVisible)
    {
      Console.CursorVisible = true;
    }
  }

  // --------------------------------------------------------------------------------------
  /// <summary>
  /// Print a progress message + bar to the console.  The bar fills as 'count' approaches 'max'.
  /// </summary>
  public void ProgressMsg(string msg, int count, int max)
  {
    if (!CanWrite) { return; }
    if (DelayWrite(count == max))
    {
      return;
    }
    Console.CursorVisible = false;

    // OPTIONS:
    const int BAR_SIZE = 30;
    const int MAX_SPACE = 20;

    msg += $" {count} of {max}";

    int copyLen = Math.Min(Width, msg.Length);
    int leftover = Width - msg.Length;
    int index = 0;
    for (int i = 0; i < copyLen; i++)
    {
      LineBuffer[i] = msg[i];
      ++index;
    }

    // We want the bar to always appear in the same place....
    // NOTE: Ideally we would have a situation where we have a designated max message length
    // and then the progress bar is printed at a fixed column.  So that way the message gets
    // truncated so that everything fits....  That will take many more options and we can look
    // into it at some other point in time...
    int idealPos = Width - (BAR_SIZE + 2);
    int useSpace = Width - (msg.Length + BAR_SIZE + 2);

    if (useSpace < 0)
    {
      throw new InvalidOperationException("deal with this case...");
    }

    int start = index;
    for (int i = start; i < start + useSpace; i++)
    {
      LineBuffer[i] = ' ';
      ++index;
    }


    float pct = ((float)count / (float)max);
    int toFill = (int)(pct * BAR_SIZE);

    LineBuffer[index] = '[';
    index++;
    start = index;
    for (int i = start; i < start + BAR_SIZE; i++)
    {
      LineBuffer[i] = (i - start) < toFill ? '*' : '.';
      index++;
    }
    LineBuffer[index] = ']';
    index++;

    int leftOver = Width - index;
    for (int i = 0; i < leftOver; i++)
    {
      LineBuffer[index + i] = ' ';
    }
    Console.CursorLeft = 0;

    // TODO: Write the buffer directly.....
    Console.Write(LineBuffer);

    //ToLine(new string(LineBuffer), false);
  }


  // --------------------------------------------------------------------------------------
  private bool DelayWrite(bool skipDelay = false)
  {
    if (skipDelay) { return false; }

    long now = Clock.ElapsedMilliseconds;
    long elapsed = now - LastWrite;
    if (elapsed > UpdateInterval)
    {
      LastWrite = now;
      return false;
    }
    return true;
  }

}


// ==================================================================================================================
public class ConsoleHelperLogger : ILogger
{
  public const string LOG_LEVEL = "ConsoleHelper_MSG";

  public void Debug(object message)
  {
    throw new NotImplementedException();
  }

  public void Error(object message)
  {
    throw new NotImplementedException();
  }

  public string? Exception(Exception? ex, string? introMessage = "An unhandled exception was encountered!")
  {
    throw new NotImplementedException();
  }

  public void Info()
  {
    throw new NotImplementedException();
  }

  public void Info(object message)
  {
    throw new NotImplementedException();
  }

  public void Verbose(object message)
  {
    throw new NotImplementedException();
  }

  public void Warning(object message)
  {
    throw new NotImplementedException();
  }

  public void WriteLine(ELogLevel level, object message)
  {
    throw new NotImplementedException();
  }

  public void WriteLine(string level, object message)
  {
    throw new NotImplementedException();
  }

  public void NewLine()
  {
    throw new NotImplementedException();
  }

}
