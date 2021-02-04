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
  public class DefaultDictionaryTester
  {

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Demonstrates that a default generator function will work as expected.
    /// </summary>
    [TestMethod]
    public void CanUseGeneratorWithDefaultDictionary()
    {

      // This will generate the values that we want.
      Func<int, string> gen = (x) =>
      {
        return x.ToString() + " is the number!";
      };

      DefaultDictionary<int, string> test = new DefaultDictionary<int, string>(gen);
      Assert.AreEqual(0, test.Values.Count, "There should be no values in the dictionary at this time!");

      const int MAX = 10;
      for (int i = 0; i < MAX; i++)
      {
        Assert.AreEqual(gen(i), test[i], "The value for key '{0}' does not match!", i);
      }

      Assert.AreEqual(MAX, test.Values.Count, "Invalid value count!");
    }


  }
}
