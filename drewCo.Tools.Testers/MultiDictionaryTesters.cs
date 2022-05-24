using drewCo.Curations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace drewCo.Tools.Testers
{
  // ============================================================================================================================
  [TestClass]
  public class MultiDictionaryTesters
  {

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// This test case shows that the 'TryGetValue' function of MultiDictionary works as expected.
    /// </summary>
    [TestMethod]
    public void CanUseTryGetValue()
    {

      const int KEY_1 = 100;
      const string KEY_2 = "One Hundred";
      const string TEST_VALUE = "I am a test value!";

      var dict = new MultiDictionary<int, string, string>();
      dict.Add(KEY_1, KEY_2, TEST_VALUE);

      {
        bool hasVal = dict.TryGetValue(KEY_1, KEY_2, out string checkVal);
        Assert.IsTrue(hasVal);
        Assert.AreEqual(checkVal, TEST_VALUE, "The test value does not match!");
      }


      // Show that it will come up empty for keys that don't exist.
      {
        const string KEY_3 = KEY_2 + "_notakey";
        bool hasVal = dict.TryGetValue(KEY_1, KEY_3, out string checkVal);
        Assert.IsFalse(hasVal);
      }

    }


  }
}
