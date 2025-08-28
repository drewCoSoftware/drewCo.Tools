using drewCo.Tools.Logging;

namespace ConsoleLoggerTestbed
{
  // ==============================================================================================================================
  internal class Program
  {
    // --------------------------------------------------------------------------------------------------------------------------
    static void Main(string[] args)
    {
      Console.WriteLine("Hello, World!");

      // Having some way to print to the same line over and over.
      // This is for headers + banners + status messages.

      // Question is, should we have some kind of special logging call for ILogger,
      // or should we have a standalone tool?

      // 1. Pros: We can use normal logging calls, and don't have to implement those transient
      // messages for loggers that don't need to record the data.
      // --> This type of logging really only applies to the ConsoleLogger type.....  worth noting...

      // 2. Cons, probably hard to difficult to integrate with the other loggers, esp. console logger....

      // Another way would be to have a special logging level that acts as 'commands' for the temp
      // stuff.....  Yeah, probably some kind of override of console logger to handle the message.....

      var cl = new ConsoleLogger();
      cl.ShowCursor(false);

      cl.WriteTemp("Hello");
      cl.NewLine();
      const int MAX = 10;
      for (int i = 0; i < MAX; i++)
      {
        cl.WriteTemp("Number is: " + i);
        Thread.Sleep(50);
      }
      cl.NewLine();
      cl.NewLine();

      cl.Info("Multi-line demo.  Alternate between lines:");

      int curLine = cl.GetTop();
      int nextLine = curLine + 1;

      for (int i = 0;i < MAX;i++) {
        cl.WriteTemp("Line 1: " + (i + 1), curLine);
        Thread.Sleep(100);
        cl.WriteTemp("Line 2: " + (i + 1) * 10, nextLine);
        Thread.Sleep(100);
      }

      // ColoredMessages();
    }

    // --------------------------------------------------------------------------------------------------------------------------
    private static void ColoredMessages()
    {
      var cl = new ConsoleLogger();
      cl.Info("Normal Message");
      cl.Warning("This is a warning!");
      cl.Error("This is an error!");
      cl.Verbose("Very verbose!");
      cl.Debug("A debug message!");
    }
  }
}
