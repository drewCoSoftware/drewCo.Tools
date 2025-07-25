﻿using System;
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
    void WriteLine(ELogLevel level, string message);
    void WriteLine(string level, string message);
    void Verbose(string message);
    void Info(string message);
    void Warning(string message);
    string? Exception(Exception? ex, string? introMessage = "An unhandled exception was encountered!");
    void Error(string message);
    void Debug(string message);

  }

}
