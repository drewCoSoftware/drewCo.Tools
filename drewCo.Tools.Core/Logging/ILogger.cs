using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace drewCo.Tools.Logging
{

  // ============================================================================================================================
  /// <summary>
  /// Interface for the things that log.
  /// </summary>
  public interface ILogger
  {
    void WriteLine(ELogLevel level, object message);
    void WriteLine(string level, object message);
    void Verbose(object message);
    void Info(object message);
    void Warning(object message);
    string? Exception(Exception? ex, string? introMessage = "An unhandled exception was encountered!");
    void Error(object message);
    void Debug(object message);
  }

}
