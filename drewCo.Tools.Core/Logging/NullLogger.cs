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
    public void WriteLine(ELogLevel level, string message) { }
    public void WriteLine(string level, string message) { }
    public void Verbose(string message) { }
    public void Info(string message) { }
    public void Warning(string message) { }
    public string? Exception(Exception? ex, string? introMessage = "An unhandled exception was encountered!") { return null; }
    public void Error(string message) { }
    public void Debug(string message) { }
  }
}
