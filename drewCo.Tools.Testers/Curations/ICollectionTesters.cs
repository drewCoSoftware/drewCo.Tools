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
  public class ICollectionTesters
  {

    // --------------------------------------------------------------------------------------------------------------------------
    [TestMethod]
    public void CanAddUniqueItems()
    {
      List<int> evenList = new List<int>() { 2, 4, 6, 8 };
      int expectedCount = evenList.Count;

      // Add something that is already in the list.
      ICollectionHelper.AddUnique(evenList, 2);
      Assert.AreEqual(expectedCount, evenList.Count);

      // Now add something that isn't in the list.
      ICollectionHelper.AddUnique(evenList, 10);
      Assert.AreEqual(expectedCount + 1, evenList.Count);

    }



  }
}
