using drewCo.Tools.Logging;

namespace ConsoleLoggerTestbed
{
  internal class Program
  {
    static void Main(string[] args)
    {
      Console.WriteLine("Hello, World!");

      var cl = new ConsoleLogger();

      cl.Info("Normal Message");
      cl.Warning("This is a warning!");
      cl.Error("This is an error!");
      cl.Verbose("Very verbose!");
      cl.Debug("A debug message!");

    }
  }
}
