using drewCo.Tools;
using drewCo.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace drewCo.Tools.Testers
{
  // ============================================================================================================================
  [TestClass]
  public class ReadWriteTesters
  {
    public const int MAX_TEST = 5;


    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Shows that the DTOMapper can copy data from fields.
    /// </summary>
    [TestMethod]
    public void CanCopyFields()
    {

      const string TEST_STRING = "MyTestString";
      const int TEST_NUMBER = 10;

      TypeWithFields src = new TypeWithFields()
      {
      Description = TEST_STRING,
      Number = TEST_NUMBER,
      };

      TypeWithFields comp = DTOMapper.CreateCopy(src);
      Assert.AreEqual(TEST_STRING , comp.Description, "Invalid description on copy!");
      Assert.AreEqual(TEST_NUMBER , comp.Number, "Invalid number on copy!");

    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// This test case was provided to demonstrate that the DTOMapper can map information from a struct to a class and vice versa.
    /// </summary>
    [TestMethod]
    public void CanMapClassToStruct()
    {

      const string TEST_NAME = "abcdef";
      const int TEST_NUMBER = 551;
      const double TEST_WEIGHT = 3.14159;

      var t1 = new Classo.MyType()
      {
          Name = TEST_NAME,
          Number = TEST_NUMBER,
          Weight = TEST_WEIGHT,
      };
      Structo.MyType t2 = new Structo.MyType();

      DTOMapper.CopyMembers<Classo.MyType, Structo.MyType>(t1, t2);

      Assert.AreEqual(TEST_NAME, t2.Name);
      Assert.AreEqual(TEST_NUMBER, t2.Number);
      Assert.AreEqual(TEST_WEIGHT, t2.Weight);
    }




    // --------------------------------------------------------------------------------------------------------------------------
    [TestMethod]
    public void CanReadAndWriteDecimalType()
    {
      const decimal TEST = 12.3654m;
      ReadWriteTest(TEST);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// This test case was provided to solve a bug where a null char inside of a string would cause the wrong number of bytes to
    /// be read back in.  To solve this, we are going to just start encoding length information...
    /// </summary>
    [TestMethod]
    public void CanReadAndWriteStringWithNullChar()
    {
      const int STR_LEN = 32;

      int rnd = RandomTools.RNG.Next(STR_LEN);
      string p1 = RandomTools.GetAlphaString(rnd);

      rnd = RandomTools.RNG.Next(STR_LEN);
      string p2 = RandomTools.GetAlphaString(rnd);

      string test = p1 + (char)0 + p2;


      using (MemoryStream ms = new MemoryStream())
      {
        EZWriter.Write(ms, test);

        ms.Seek(0, SeekOrigin.Begin);


        string comp = EZReader.ReadString(ms);

        Assert.AreEqual(test, comp, "Strings don't match!");
      }

    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// This test case was provided to sole potentials issues where reading strings can potentially exceed the file size, or
    /// even allocate too much memory.  Typicaly a string large enough to go out of memory will be past EOF.
    /// </summary>
    [TestMethod]
    public void ReadingStringWontEOF()
    {
      Assert.Fail("Finish this test please...");
    }



    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Shows that boolean data can be read and written correctly.
    /// </summary>
    [TestMethod]
    public void CanReadAndWriteBools()
    {

      const int MAX = 5;
      bool[] myBoools = new bool[MAX];

      for (int i = 0; i < MAX; i++)
      {
        myBoools[i] = RandomTools.FiftyFifty();
      }

      // Write the data, then read it back in.
      using (MemoryStream ms = new MemoryStream())
      {
        for (int i = 0; i < MAX; i++)
        {
          EZWriter.Write(ms, myBoools[i]);
        }

        ms.Seek(0, SeekOrigin.Begin);

        for (int i = 0; i < MAX; i++)
        {
          bool nextBool = EZReader.ReadBool(ms);
          Assert.AreEqual(myBoools[i], nextBool, "Invalid boolean at index #{0}", i);
        }
      }

    }


    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Shows that the generic version of EZReader works indeed.
    /// </summary>
    [TestMethod]
    public void CanUseGenericEZReaderMethod()
    {

      // A more difficult one...
      const int MAX = 10;
      Int32[] data = new Int32[MAX];
      for (int i = 0; i < MAX; i++)
      {
        data[i] = i;
      }

      MemoryStream m = new MemoryStream();
      EZWriter.Write(m, data);

      m.Seek(0, SeekOrigin.Begin);

      Int32[] comp = EZReader.Read<Int32[]>(m);

      ObjectInspector oi = new ObjectInspector();
      var result = oi.CompareArrays<int>(data, comp);

      // Make sure that it matches!
      Assert.IsTrue(result.Item1, result.Item2);


      // Let's do a string to make sure!
      string testString = RandomTools.GetRandomCharString(MAX);
      m = new MemoryStream();
      EZWriter.Write(m, testString);

      m.Seek(0, SeekOrigin.Begin);

      string compString = EZReader.Read<string>(m);

      Assert.AreEqual(testString, compString);

    }


    // --------------------------------------------------------------------------------------------------------------------------
    [TestMethod]
    public void CanSerializeFloats()
    {
      float[] testFloats = new[] { 1.25f, 0.546f, 21.581f };


      using (var ms = new MemoryStream())
      {
        foreach (var item in testFloats)
        {
          EZWriter.Write(ms, item);
        }
        ms.Seek(0, SeekOrigin.Begin);

        float[] others = new float[testFloats.Length];
        for (int i = 0; i < others.Length; i++)
        {
          others[i] = EZReader.ReadFloat(ms);
        }

        ObjectInspector inspector = new ObjectInspector();
        inspector.CompareObjects(testFloats, others, true);

      }

    }


    // --------------------------------------------------------------------------------------------------------------------------
    [TestMethod]
    public void CanSerializeDoubles()
    {
      double[] testDoubles = new[] { 1.25d, 0.546d, 21.581d };


      using (var ms = new MemoryStream())
      {
        foreach (var item in testDoubles)
        {
          EZWriter.Write(ms, item);
        }
        ms.Seek(0, SeekOrigin.Begin);

        double[] others = new double[testDoubles.Length];
        for (int i = 0; i < others.Length; i++)
        {
          others[i] = EZReader.ReadDouble(ms);
        }

        ObjectInspector inspector = new ObjectInspector();
        inspector.CompareObjects<double[]>(testDoubles, others, true);

      }

    }


    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Shows that we can read and write lists.
    /// </summary>
    [TestMethod]
    public void CanReadWriteList()
    {

      // MethodInfo writer = typeof(EZWriter).GetMethod("Write", new Type[] { typeof(Stream), typeof(T) });
      // writer.Invoke(null, new object[] { ms, data });

      // TODO: when it works, use 'ReadWriteTest(x)'

      List<string> strList = new List<string>() { "ABC", "DEF", "GHI" };
      using (var ms = new MemoryStream())
      {
        EZWriter.Write(ms, strList);
        ms.Seek(0, SeekOrigin.Begin);

        List<string> comp = EZReader.ReadList<string>(ms);

        ObjectInspector inspector = new ObjectInspector();
        inspector.CompareObjects(strList, comp, true);
      }


      List<int> intList = new List<int>() { 1, 2, 3, 4, 5 };
      using (var ms = new MemoryStream())
      {
        EZWriter.Write(ms, intList);
        ms.Seek(0, SeekOrigin.Begin);

        List<int> comp = EZReader.ReadList<int>(ms);

        ObjectInspector inspector = new ObjectInspector();
        inspector.CompareObjects(intList, comp, true);
      }

    }

    // --------------------------------------------------------------------------------------------------------------------------
    // TODO: This won't work for lists or dictionaries!
    private static void ReadWriteTest<T>(T data)
    {
      using (var ms = new MemoryStream())
      {
        MethodInfo writer = typeof(EZWriter).GetMethod("Write", new Type[] { typeof(Stream), typeof(T) });
        if (writer == null)
        {
          throw new InvalidOperationException(string.Format("Could not find write method for type {0}!", typeof(T)));
        }
        writer.Invoke(null, new object[] { ms, data });
        ms.Seek(0, SeekOrigin.Begin);

        T comp = EZReader.Read<T>(ms);

        //List<string> comp = EZReader.ReadList<string>(ms);

        ObjectInspector inspector = new ObjectInspector();
        inspector.CompareObjects((object)data, (object)comp, true);

      }
    }

    // --------------------------------------------------------------------------------------------------------------------------
    [TestMethod]
    public void CanReadWriteInt64()
    {

      const int MAX = 100;
      for (int i = 0; i < MAX; i++)
      {
        byte[] data = new byte[sizeof(Int64)];
        RandomTools.RNG.NextBytes(data);

        Int64 next = BitConverter.ToInt64(data, 0);

        using (var ms = new MemoryStream())
        {
          EZWriter.Write(ms, next);
          ms.Seek(0, SeekOrigin.Begin);
          Int64 comp = EZReader.ReadInt64(ms);

          Assert.AreEqual<Int64>(next, comp);
        }

      }

    }


    // --------------------------------------------------------------------------------------------------------------------------
    [TestMethod]
    public void CanReadWriteInt32()
    {
      const int MAX = 100;
      for (int i = 0; i < MAX; i++)
      {
        Int32 next = RandomTools.RNG.Next(int.MinValue, int.MaxValue);

        using (var ms = new MemoryStream())
        {
          EZWriter.Write(ms, next);
          ms.Seek(0, SeekOrigin.Begin);

          int comp = EZReader.ReadInt32(ms);

          Assert.AreEqual(next, comp);
        }

      }
    }


    // --------------------------------------------------------------------------------------------------------------------------
    [TestMethod]
    public void CanReadAndWriteString()
    {

      const int SIZE = 100;
      // The first string is null, because we need to be able to support this concept.
      List<string> testStrings = new List<string>() { null };
      for (int i = 0; i < MAX_TEST; i++)
      {
        string test = RandomTools.GetAlphaString(SIZE);
        testStrings.Add(test);
      }
      foreach (var test in testStrings)
      {
        using (var ms = new MemoryStream())
        {
          EZWriter.Write(ms, test);
          ms.Seek(0, SeekOrigin.Begin);

          string comp = EZReader.ReadString(ms);

          Assert.AreEqual(test, comp, "The strings should be the same!");
        }
      }

    }




    // --------------------------------------------------------------------------------------------------------------------------
    [TestMethod]
    public void CanReadAndWriteUint()
    {
      // const uint test = 0xF00010FF;

      for (int i = 0; i < MAX_TEST; i++)
      {
        // WOW!  The C# preference for int over everything sure is annoying!
        uint test = (uint)RandomTools.RNG.Next((int)uint.MinValue, int.MaxValue);

        using (var ms = new MemoryStream())
        {
          EZWriter.Write(ms, test);
          ms.Seek(0, SeekOrigin.Begin);

          uint comp = EZReader.ReadUInt32(ms);

          Assert.AreEqual(test, comp, "The uints should be the same!");
        }
      }

    }

  }

}
