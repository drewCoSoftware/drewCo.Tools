using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace drewCo.Tools.Logging
{
  // ============================================================================================================================
  /// <summary>
  /// Yep, its a logger that doesn't do anything.
  /// </summary>
  public class NullLogger : ILogger
  {
    public void Log(ELogLevel level, string message) { }
    public void Log(string level, string message) { }
    public void Verbose(string message) { }
    public void Info(string message) { }
    public void Warning(string message) { }
    public string? LogException(Exception? ex, string? introMessage = "An unhandled exception was encountered!") { return null; }
    public void Error(string message) { } 
  }
}
