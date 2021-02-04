// ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
// Copyright (c)2010-2014 Andrew A. Ritz, all rights reserved.
//
// This code is released under the ms-pl (Microsoft Public License)
// ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::

using drewCo.Curations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CurationsTester
{
  // ============================================================================================================================
  [TestClass]
  public class DisposingListTester
  {

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Shows that the 'Dispose' method on a disposing list actually works.
    /// </summary>
    [TestMethod]
    public void ListClearsOnDisposal()
    {

      DisposingList<DisposalbleItem> test = new DisposingList<DisposalbleItem>();
      
      const int MAX = 10;
      for (int i = 0; i < MAX; i++)
      {
        test.Add(new DisposalbleItem());
      }

      // We pass the references off somewhere else so we can keep track of them.
      List<DisposalbleItem> check = new List<DisposalbleItem>();
      for (int i = 0; i < test.Count; i++)
      {
      check.Add(test[i]);
        Assert.IsFalse(test[i].IsDisposed);
      }


      // Now dispose it.
      test.Dispose();
      Assert.AreEqual(0, test.Count, "There should be no more items in the list!");

      for (int i = 0; i < check.Count; i++)
      {
        Assert.IsTrue(check[i].IsDisposed, "The item at index {0} should be disposed!", i);
      }
    }


  }




  // ============================================================================================================================
  /// <summary>
  /// Probably the simplest implementation of a disposable class.
  /// </summary>
  public class DisposalbleItem : IDisposable
  {
    private bool _IsDisposed;

    public bool IsDisposed
    {
      get { return _IsDisposed; }
      private set { _IsDisposed = value; }
    }


    // --------------------------------------------------------------------------------------------------------------------------
    public void Dispose()
    {
      _IsDisposed = true;
    }
  }

}
