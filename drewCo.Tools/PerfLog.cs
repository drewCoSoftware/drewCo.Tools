//:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
// Copyright ©2012-2014 Andrew A. Ritz, All Rights Reserved
//:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::

using System;
using System.Diagnostics;
namespace drewCo.Tools
{

  // ============================================================================================================================
  /// <summary>
  /// A simple tool that can be used to make inline performance measurements.
  /// It is intended to be used inside of a using block.  Make a surrounds snippet for even easier usage!
  /// </summary>
  public class PerfLog : IDisposable
  {
    Stopwatch Clock;
    private string OperationName;

    // --------------------------------------------------------------------------------------------------------------------------
    public PerfLog(string opName_ = null)
    {
      OperationName = opName_;
      Clock = Stopwatch.StartNew();
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public void Dispose()
    {
      Console.WriteLine("Operation {0} :: \t {1:f4}", OperationName, (double)Clock.ElapsedTicks / (double)Stopwatch.Frequency);
    }
  }


}
