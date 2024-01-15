using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace drewCo.Tools
{
  // ============================================================================================================================
  public static class ProcessTools
  {
    // --------------------------------------------------------------------------------------------------------------------------
    public static void RunProcess(string fileName, string arguments)
    {
      var processStartInfo = new ProcessStartInfo(fileName, arguments)
      {
        RedirectStandardOutput = true
      };
      var process = Process.Start(processStartInfo);
      if (!process.WaitForExit(5000)) // wait for 5 seconds
      {
        process.Kill();
        throw new TimeoutException("The process for " + fileName + " timed out.");
      }
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static string RunProcessAndGetOutput(string fileName, string arguments)
    {
      var processStartInfo = new ProcessStartInfo(fileName, arguments)
      {
        RedirectStandardOutput = true
      };
      var process = Process.Start(processStartInfo);
      var output = process.StandardOutput.ReadToEnd();
      if (!process.WaitForExit(5000)) // wait for 5 seconds
      {
        process.Kill();
        throw new TimeoutException("The process for " + fileName + " timed out.");
      }
      return output;
    }
  }
}
