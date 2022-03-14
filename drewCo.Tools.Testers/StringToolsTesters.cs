using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace drewCo.Tools.Testers
{
  // ============================================================================================================================
  [TestClass]
  public class StringToolsTesters
  {


    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Shows that we can de-camel case a number of strings.
    /// </summary>
    [TestMethod]
    public void CanDeCamelCaseString()
    {
      string[] tests = new[] {
        "Made In USA",
        "franklin Mints Grave"
      };


      foreach (var t in tests)
      {
        string joined = t.Replace(" ", "");
        string deCameled = StringTools.DeCamelCase(joined);

        Assert.AreEqual(t, deCameled);
      }

    }


    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Demonstrates that we can get a unique string, given a list of existing strings.
    /// </summary>
    [TestMethod]
    public void CanGetUniqueString()
    {

      List<string> current = new List<string>()
      {
        "First",
        "Second",
      };

      const string UNIQUE = "UNIQUE";
      Assert.IsFalse(current.Contains(UNIQUE), "The unique string should not exist in the list!");

      {
        string check = StringTools.GetUniqueString(UNIQUE, current);
        Assert.AreEqual(UNIQUE, check, "The check string should be the same as the input!");
        current.Add(check);
        Assert.IsTrue(current.Contains(UNIQUE), "The unique string should now exist in the list!");
      }

      {
        string check = StringTools.GetUniqueString(UNIQUE, current);
        Assert.AreNotEqual(UNIQUE, check, "The check string should NOT be the same as the input!");
        Assert.AreEqual(UNIQUE + "_1", check);      // NOTE: We can add other options for this later if we want...
        //current.Add(check);
      }

      //  Assert.Fail("Finish this test please...");
    }


    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// This test case is provided to show that our custom string compare function (for sorting) produces the same results as that
    /// of the built-in .NET version.
    /// </summary>
    [TestMethod]
    public void CanCompareStrings()
    {
      var testStrings = new List<string>()
      {
        "abc",
        "Hamster",
        "Hamster_Longer",
        "Apples & Oranges",
        "Apples & Carrots",
      };

      // Let's add some random ones....
      const int RND_COUNT = 50;
      for (int i = 0; i < RND_COUNT; i++)
      {
        string toAdd = RandomTools.GetAlphaNumericString(10);
        string toAdd2 = RandomTools.RandomizeCase(toAdd);

        testStrings.Add(toAdd);
        testStrings.Add(toAdd2);
      }

      // We need to permute these....
      var permuted = new List<Tuple<string, string>>();
      int len = testStrings.Count;
      for (int i = 0; i < len; i++)
      {
        for (int j = i + 1; j < len; j++)
        {
          permuted.Add(new Tuple<string, string>(testStrings[i], testStrings[j]));
        }
      }

      //var sorted = new List<string>(testStrings);
      //sorted.Sort(
      int pLen = permuted.Count;
      for (int i = 0; i < pLen; i++)
      {
        string l = permuted[i].Item1;
        string r = permuted[i].Item2;

        int legacy = string.Compare(l, r, true); //  l.CompareTo(r);
        int comp = StringTools.AlphaNumericStrCompare_NoCase(l, r);

        Assert.AreEqual(legacy, comp, "Non-matching comparison at index: {0} ({1} : {2})", i, l, r);

      }
    }




    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Demonstrates some of our string masking capabilities, and what we expect from them...
    /// </summary>
    [TestMethod]
    public void CanApplyStringMask()
    {
      const string MASK_1 = "###-###";

      string res = StringTools.ApplyMask("abc123", MASK_1);
      Assert.AreEqual("abc-123", res);

      // Show what happens when we have inputs that are long or short compared to the mask.
      const string MASK_2 = "### ## ###-#";
      string longInput = "12345abc999";
      string longRes = StringTools.ApplyMask(longInput, MASK_2);
      Assert.AreEqual("123 45 abc-9", longRes, "Invalid output for long string!");

      string shortInput = "12345";
      string shortRes = StringTools.ApplyMask(shortInput, MASK_2);
      Assert.AreEqual("123 45", shortRes, "Invalid output for short string!");

    }



    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// This shows that we can determine which strings are numeric, i.e. valid numbers.
    /// </summary>
    [TestMethod]
    public void CanDetectNumericString()
    {
      // This is a valid number!
      double val = 0;
      Assert.IsTrue(double.TryParse("1.", out val));
      Assert.AreEqual(1, val);

      // These are all numbers!
      string[] testPostive = new[]
      {
        ".1",
        "1.",
        "15898918891891911918489644698189",
        "123",
        "123189.189189",
        "-.12",
        "-1233.849",
      };
      foreach (var item in testPostive)
      {
        Assert.IsTrue(StringTools.IsNumeric(item), "The test string '{0}' should be marked as Numeric!", item);
      }

      // These are all not numbers!
      string[] testNegative = new[]
      {
        "abc",
        "1 1",
        "1.1.1",
        "4..2",
        "..1",
        "--254",
      };
      foreach (var item in testNegative)
      {
        Assert.IsFalse(StringTools.IsNumeric(item), "The test string '{0}' should be NOT marked as Numeric!", item);
      }

    }


    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Shows that we can get character counts within strings.
    /// </summary>
    [TestMethod]
    public void CanGetCharCounts()
    {
      string TEST_ME = "0010101101";

      int zeroCount = StringTools.GetCharCount(TEST_ME, 0, '0');
      Assert.AreEqual(5, zeroCount, "Invalid character count for '0'!");

      int oneCount = StringTools.GetCharCount(TEST_ME, 0, '1');
      Assert.AreEqual(5, oneCount, "Invalid character count for '1'!");
    }


    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Shows that nesting of parens, etc. doesn't make too big of a difference....
    /// </summary>
    [TestMethod]
    public void CanExtractNestedParens()
    {
      const string EXPECTED = "some data (xxx())";
      string testString = string.Format("this is ({0}) OK!", EXPECTED);

      string extract = StringTools.GetInnerString(testString, '(', ')');
      Assert.AreEqual(EXPECTED, extract);

    }


    //// --------------------------------------------------------------------------------------------------------------------------
    ///// <summary>
    ///// Shows that the 'MatchesFormat' function works correctly.  Add conditions, etc. as you go.
    ///// </summary>
    //[TestMethod, Ignore]
    //public void CanDetectMatchingFormats()
    //{
    //  TestGroup g1 = new TestGroup()
    //  {
    //    TestFormat = "ABC {0} DEF",
    //    TestCases = new Dictionary<string,bool>() {
    //      { "ABC 123 DEF", true },
    //      { "xyz 123 DEF", false }
    //    },
    //  };


    //  TestGroup g2 = new TestGroup()
    //  {
    //    TestFormat = "Order {0} Placed on {1:G}",
    //    TestCases = new Dictionary<string, bool>() {
    //      { "Order 123 Placed on x", true },
    //      { "Order 123 Placed never", false }
    //    },
    //  };


    //  RunMatchTest(g2);


    //}

    //// --------------------------------------------------------------------------------------------------------------------------
    //private void RunMatchTest(TestGroup g1)
    //{
    //  foreach (var item in g1.TestCases)
    //  {
    //    Assert.AreEqual(item.Value, StringTools.MatchesFormat(item.Key, g1.TestFormat), "Invalid result for matching function....");
    //  }
    //}

  }


  // ============================================================================================================================
  internal class TestGroup
  {
    public string TestFormat;
    public Dictionary<string, bool> TestCases;
  }


}
