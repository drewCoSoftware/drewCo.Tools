//:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
// Copyright ©2009-2015 Andrew A. Ritz, All Rights Reserved
//:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::

using System;

namespace drewCo.Tools
{

  // ============================================================================================================================
  /// <summary>
  /// Acts as a simple re-entrancy sentinel.
  /// </summary>
#if IS_TOOLS_LIB
  public class Sentinel
#else
  internal class Sentinel
#endif
  {
    private object WorkLock = new object();

    private bool _IsWorking;
    public bool IsWorking
    {
      get { lock (WorkLock) { return _IsWorking; } }
      set { lock (WorkLock) { _IsWorking = value; } }
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public void DoWork(Action work)
    {
      if (IsWorking) { return; }
      IsWorking = true;
      try
      {
        work();
      }
      finally
      {
        IsWorking = false;
      }
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Sets the working flag to false.  This will make the work code reentrant again.  Can be used in scenarios where you want
    /// to limit reentrancy, without blocking it altogether.  Use sparingly.
    /// </summary>
    public void CancelWork()
    {
      IsWorking = false;
    }
  }
}
