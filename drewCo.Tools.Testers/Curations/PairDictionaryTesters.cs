// ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
// Copyright (c)2013 Andrew A. Ritz, all rights reserved.
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
  public class PairDictionaryTesters
  {
    private const string ONE = "One";
    private const string TWO = "Two";
    private const string THREE = "Three";

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Shows that values can be set in pair dictionaries through their keys.
    /// </summary>
    [TestMethod]
    public void CanSetValuesViaKeys()
    {
      const string KEY = "x";
      const int VAL_1 = 100;
      const int VAL_2 = 1000;

      PairDictionary<string, int> test = new PairDictionary<string,int>();
      test.Add(KEY, VAL_1);

      // Set the value, and make sure that it took!
      test[KEY] = VAL_2;
      Assert.AreEqual(VAL_2, test[KEY]);
    }


    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Shows that <see cref="DefaultPairDictionary"/> types provide real defaults.
    /// </summary>
    [TestMethod]
    public void CanUseDefaultPairDictionary()
    {
      const string DEFAULT_KEY = "DEFAULT";
      const int DEFAULT_VALUE = int.MinValue;

      DefaultPairDictionary<string, int> test = new DefaultPairDictionary<string, int>(DEFAULT_KEY, DEFAULT_VALUE);

      string actualKey = test[12345];
      int actualValue = test["whatver"];

      Assert.AreEqual(DEFAULT_KEY, actualKey, "Invalid default key!");
      Assert.AreEqual(DEFAULT_VALUE, actualValue, "Invalid default value!");
    }



    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// SHows that it is possible for us to use a key comparer in a PairDictionary.
    /// </summary>
    [TestMethod]
    public void CanUseKeyComparer()
    {

      PairDictionary<string, int> test = new PairDictionary<string, int>(StringComparer.OrdinalIgnoreCase);
      test.Add(ONE, 1);
      test.Add(TWO, 2);
      test.Add(THREE, 3);

      // See, the formatting no longer matters.
      Assert.AreEqual(1, test[ONE.ToLower()]);
      Assert.AreEqual(2, test[TWO.ToUpper()]);
      Assert.AreEqual(3, test["tHreE"]);

      // TODO: Include a check to show that we will get an exception if we use odd formatting as well.
      // 'DrewCo.UnitTesting' ??
    }



    // --------------------------------------------------------------------------------------------------------------------------
    [TestMethod]
    public void CanGetDictionaryKeysAndValues()
    {
      int[] keys = new int[] { 1, 2, 3 };
      string[] vals = new string[] { "alpha", "beta", "gamma" };

      PairDictionary<int, string> test = new PairDictionary<int, string>();
      for (int i = 0; i < keys.Length; i++)
      {
        test.Add(keys[i], vals[i]);
      }

      Assert.AreEqual(keys.Length, test.Count);

      IList<int> testKeys = test.Keys;
      IList<string> testVals = test.Values;

      Assert.AreEqual(test.Count, testKeys.Count);
      Assert.AreEqual(test.Count, testVals.Count);

      // Make sure that the contained keys / values are correct.
      for (int i = 0; i < keys.Length; i++)
      {
        Assert.AreEqual(keys[i], testKeys[i]);
        Assert.AreEqual(vals[i], testVals[i]);
      }
    }


    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// This makes sure that is isn't possible to add the same key twice.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void CantAddDuplicateKeys()
    {
      PairDictionary<string, int> test = new PairDictionary<string, int>();

      const string DUPE = "ABC";
      test.Add(DUPE, 1);

      // KABOOM!
      test.Add(DUPE, 2);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// This makes sure that is isn't possible to add the same key twice.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void CantAddDuplicateValues()
    {
      PairDictionary<int, string> test = new PairDictionary<int, string>();

      const string DUPE = "ABC";
      test.Add(1, DUPE);

      // KABOOM!
      test.Add(2, DUPE);

    }



    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Shows that the same key and value type can be used on a pair dictionary, although the indexing won't work
    /// because the syntax will be ambiguous.
    /// </summary>
    [TestMethod]
    public void CanUseSameKeyAndValueTypes()
    {
      const string DOG = "Dog";
      const string HORSE = "Horse";
      const string PET = "Pet";
      const string FOOD = "Food";

      PairDictionary<string, string> test = new PairDictionary<string, string>()
      {
        {DOG, PET},
        {HORSE, FOOD}
      };


      // NOTE: This syntax won't work because of ambiguity.
      // Uncomment it and try it out.
      // string val = test[DOG];


      // Make sure that the value associations will work.
      Assert.AreEqual(PET, test.Value(DOG));
      Assert.AreEqual(FOOD, test.Value(HORSE));

      // Make sure the key associations works.
      Assert.AreEqual(DOG, test.Key(PET));
      Assert.AreEqual(HORSE, test.Key(FOOD));
    }


  }

}
