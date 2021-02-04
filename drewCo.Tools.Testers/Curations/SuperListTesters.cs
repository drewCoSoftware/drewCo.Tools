// ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
// Copyright (c)2010-2014 Andrew A. Ritz, all rights reserved.
//
// This code is released under the ms-pl (Microsoft Public License)
// ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using drewCo.Curations;

namespace CurationsTester
{

  // ============================================================================================================================
  [TestClass]
  public class SuperListTesters
  {

    // --------------------------------------------------------------------------------------------------------------------------
    [TestMethod]
    public void CanGetAddEvent()
    {
      string testItem = "abc";
      bool eventCaught = false;

      SuperList<string> testList = new SuperList<string>();
      testList.ItemAdded += (s, e) =>
      {
        eventCaught = true;
        Assert.AreEqual(testItem, e.Item);
        Assert.IsTrue(object.ReferenceEquals(testItem, e.Item));
      };
      testList.Add(testItem);
      Assert.IsTrue(eventCaught);

    }


    // --------------------------------------------------------------------------------------------------------------------------
    [TestMethod]
    public void CanGetRemoveEvent()
    {
      int testItem = 1001;
      bool eventCaught = false;

      SuperList<int> testList = new SuperList<int>();
      testList.ItemRemoved += (s, e) =>
      {
        eventCaught = true;
        Assert.AreEqual(testItem, e.Item);
      };
      testList.Add(testItem);
      testList.Remove(testItem);

      Assert.IsTrue(eventCaught);
    }


    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Shows that we get the proper events when the list is cleared.
    /// @@AAR - 9.22.2011 -- The feature is not supported at this time.
    /// </summary>
    [TestMethod, Ignore]
    public void ClearingListRaisesEvents()
    {
      SuperList<double> testList = new SuperList<double>();
      for (int i = 0; i < 100; i++)
      {
        testList.Add((double)i);
      }

      testList.Clear();
    }


  }

}
