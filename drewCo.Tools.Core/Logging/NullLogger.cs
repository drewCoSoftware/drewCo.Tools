using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace drewCo.Tools.Logging
{
  // ============================================================================================================================
  /// <summary>
  /// Yep, its a logger that doesn't do anything.
  /// </summary>
  [Obsolete("Using the new 'Log' approach obviates the need for NullLogger.  It will be deleted!")]
  public class NullLogger : ILogger
  {
    public void WriteLine(ELogLevel level, object message) { }
    public void WriteLine(string level, object message) { }
    public void Verbose(object message) { }
    public void Info(object message) { }
    public void Warning(object message) { }
    public string? Exception(Exception? ex, string? introMessage = "An unhandled exception was encountered!") { return null; }
    public void Error(object message) { }
    public void Debug(object message) { }
  }
}
