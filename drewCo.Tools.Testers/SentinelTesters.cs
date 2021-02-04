using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace drewCo.Tools.Testers
{
  // ============================================================================================================================
  [TestClass]
  public class SentinelTesters
  {
    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// This test case was provided to solve a bug where an exception in the work routine of a sentinel would leave it in
    /// the 'working' state, making it impossible to do work with it again.
    /// This is possible on web servers / anything that doesn't also crash / cleanup the sentinel.  Like on a web server for
    /// example.
    /// </summary>
    [TestMethod]
    public void SentinelStopsWorkingOnException()
    {
        
        Sentinel testSentinel = new Sentinel();
        try
        {
          testSentinel.DoWork(() =>
          { 
            throw new Exception("Kaboom!");
          });
        }
        catch (Exception)
        { }

        Assert.IsFalse(testSentinel.IsWorking, "The sentinel should not be working after an exception in its work routine!");
    }

  }
}
