namespace drewCo.Tools.UnitTesting
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using System.IO;
  using System.Linq.Expressions;
  using drewCo.Tools;
  using drewCo.UnitTesting;

#if NETFX_CORE
//  using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
  using Windows.Storage;
  using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
#else
  using Microsoft.VisualStudio.TestTools.UnitTesting;
  using System.Threading.Tasks;
  using drewCo.Curations;
#endif



  // ============================================================================================================================
  /// <summary>
  /// Provides an additional series of condition check, like that of <see cref="Assert"/> in the MSTest framework.
  /// </summary>
  /// <remarks>This will be moved to a unit testing library at some point in the future!</remarks>
  public partial class Check
  {

    // --------------------------------------------------------------------------------------------------------------------------
    public static void ObjectValuesDoNotMatch<T>(T src, T comp)
    {
      InspectionReport ir = CompareObjectsInternal(src, comp);
      if (ir.ObjectsMatch.Value)
      {
        throw new AssertFailedException("The objects should not match!");
      }
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static void ObjectValuesMatch<T>(T src, T comp)
    {
      InspectionReport ir = CompareObjectsInternal(src, comp);
      if (!ir.ObjectsMatch.Value)
      {
        // TODO: We could include more information....
        throw new AssertFailedException("The objects do not match!" + Environment.NewLine + ir.Message);
      }
    }

    // --------------------------------------------------------------------------------------------------------------------------
    private static InspectionReport CompareObjectsInternal<T>(T src, T comp)
    {
      var oi = new ObjectInspector();
      InspectionReport ir = oi.CompareObjects<T>(src, comp);
      return ir;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Tells us if the given list contains an item based on the predicate.
    /// </summary>
    public static void Contains<T>(IList<T> src, Func<T, bool> predicate)
    {
      bool contains = IEnumHelper.Contains(src, predicate);
      if (!contains)
      {
        AssertFail("The list does not contain any members that match the given predicate!");
      }
    }

    // --------------------------------------------------------------------------------------------------------------------------
    private static void AssertFail(string msg, params object[] args)
    {
      throw new AssertFailedException(string.Format(msg, args));
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static void AreEqual<TSrc, TVal>(TSrc target, Expression<Func<TSrc, TVal>> prop, TVal expected)
    {
      TVal actual = (TVal)ReflectionTools.GetNestedPropertyValue(target, prop);
      if (!expected.Equals(actual))
      {
        throw new AssertFailedException(string.Format("Test Failed. {0} is invalid!", ReflectionTools.GetNestedPropertyName(prop)));
      }
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Make sure that the two object graphs are equivalent.
    /// </summary>
    public static void AreEquivalent(object src, object comp)
    {
      ObjectInspector inspector = new ObjectInspector();
      var result = inspector.CompareObjects(src, comp, false);
      if (!result.Success)
      {
        throw new AssertFailedException(string.Format("Object graphs are not equivalent! Message: {0}", result.Message));
      }
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static void IsNull<TSrc>(TSrc target, Expression<Func<TSrc, object>> prop)
    {
      object actual = ReflectionTools.GetNestedPropertyValue(target, prop);
      if (actual != null)
      {
        throw new AssertFailedException(string.Format("Test Failed. {0} should be null!", ReflectionTools.GetNestedPropertyName(prop)));
      }
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Make sure that the given target is of type 'T'
    /// </summary>
    public static void IsType<T>(object target)
    {
      if (target == null)
      {
        throw new AssertFailedException("target may not be null!");
      }
      Type t = target.GetType();
      Type expected = typeof(T);
      if (t != expected)
      {
        string errMsg = string.Format("target object is of type '{0}' when type '{1}' was expected!", t, expected);
        throw new AssertFailedException(errMsg);
      }
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static void IsNotNull<TSrc>(TSrc target, Expression<Func<TSrc, object>> prop)
    {
      object actual = ReflectionTools.GetNestedPropertyValue(target, prop);
      if (actual == null)
      {
        throw new AssertFailedException(string.Format("Test Failed. {0} should not be null!", ReflectionTools.GetNestedPropertyName(prop)));
      }
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// This is kind of a quick and dirty way to compare a bunch of properties on two instances of an object.
    /// </summary>
    public static void AreEqual<T>(T src, T comp, Func<T, T, bool> predicate)
    {
      bool res = predicate.Invoke(src, comp);
      if (!res)
      {
        throw new AssertFailedException("The objects aren't equal according to the predicate!");
      }
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Makes sure that the given file exists.
    /// </summary>
#if NETFX_CORE
    public async static Task FileExists(string path)
    {
      bool exists = await FileTools.FileExists(path);
#else
    public static void FileExists(string path)
    {
      bool exists = File.Exists(path);
#endif

      if (!exists)
      {
        throw new AssertFailedException(string.Format("Test Failed. " + "The file at '{0}' does not exist!", path));
      }
    }

    // --------------------------------------------------------------------------------------------------------------------------
#if !NETFX_CORE
    public const string ALL_FILES_FILTER = "*.*";
    /// <summary>
    /// Checks to make sure that the correct number of files exist in the given directory that also match the filter!
    /// </summary>
    /// <param name="expectedCount"></param>
    /// <param name="directory"></param>
    public static void FileCount(int expectedCount, string directory)
    {
      // TODO: Make this a parameter.
      string filter = ALL_FILES_FILTER;
      var files = Directory.GetFiles(directory, filter);
      if (files.Length != expectedCount)
      {
        throw new AssertFailedException($"Expected {expectedCount} files, and found {files.Length} instead!  Directory={directory}, filter={filter}");
      }
    }
#else
#endif

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Makes sure that the file does not exist.
    /// </summary>
#if NETFX_CORE
    public async static Task FileDoesNotExist(string path)
    {
      bool exists = await FileTools.FileExists(path);
#else
    public static void FileDoesNotExist(string path)
    {
      bool exists = File.Exists(path);
#endif

      if (exists)
      {
        throw new AssertFailedException(string.Format("Test Failed. " + "The file at '{0}' exists!", path));
      }
    }




    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Makes sure that the given directory exists.
    /// </summary>
#if NETFX_CORE
    public async static Task DirectoryExists(string path)
    {
      bool exists = await FileTools.FolderExists(path);
#else
    public static void DirectoryExists(string path)
    {
      bool exists = Directory.Exists(path);
#endif

      if (!exists)
      {
        string msg = "The given directory '{0}' does not exist!";
        throw new AssertFailedException(string.Format(msg, path));
      }
    }


    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Makes sure that the given directory exists.
    /// </summary>
#if NETFX_CORE
    public async static Task DirectoryDoesNotExist(string path)
    {
      bool exists = await FileTools.FolderExists(path);
#else
    public static void DirectoryDoesNotExist(string path)
    {
      bool exists = Directory.Exists(path);
#endif

      if (exists)
      {
        string msg = "The given directory '{0}' should not exist!";
        throw new AssertFailedException(string.Format(msg, path));
      }
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Makes sure that the given directory contains no files.
    // _TEST:
    /// </summary>
    public static void DirectoryIsEmpty(string path)
    {
      DirectoryExists(path);

#if NETFX_CORE
      StorageFolder folder = StorageFolder.GetFolderFromPathAsync(path).GetResults();
      var files = folder.GetFilesAsync().GetResults();
      var dirs = folder.GetFoldersAsync().GetResults();
#else
      string[] files = Directory.GetFiles(path);
      string[] dirs = Directory.GetDirectories(path);
#endif
#if NETFX_CORE
      if (files.Count > 0 || dirs.Count > 0)
#else
      if (files.Length > 0 || dirs.Length > 0)
#endif
      {
        string msg = "The given directory '{0}' is not empty!";
        throw new AssertFailedException(string.Format(msg, path));
      }

    }


    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Ensures that all objects are equal to the <paramref name="expected"/> object.
    /// _TEST:
    /// </summary>
    public static void AllAreEqualTo(object expected, object[] actuals)
    {
      try
      {
        foreach (var item in actuals)
        {
          Assert.AreEqual(expected, item);
        }
      }
      catch (AssertFailedException) { throw; }
      catch (Exception) { throw; }
    }


    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Ensures that all objects in <paramref name="items"/> are not equal to each other  For example:
    /// A,B,C -> A != B, B != C, C != A
    /// </summary>
    public static void AllAreNotEqual(object[] items)
    {
      for (int i = 0; i < items.Length; i++)
      {
        for (int j = i + 1; j < items.Length; j++)
        {
          if (items[i].Equals(items[j]))
          {
            string errMsg = "The item at index {0} should not be equal to the item at index {1}";
            throw new AssertFailedException(string.Format(errMsg, i, j));
          }
        }
      }
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public static void AllAreEqual(object[] items)
    {
      for (int i = 0; i < items.Length; i++)
      {
        for (int j = i + 1; j < items.Length; j++)
        {
          if (!items[i].Equals(items[j]))
          {
            string errMsg = "The item at index {0} should be equal to the item at index {1}";
            throw new AssertFailedException(string.Format(errMsg, i, j));
          }
        }
      }
    }

#if NETFX_CORE
    // --------------------------------------------------------------------------------------------------------------------------
    public async static Task ThrowsException<TEx>(Task operation)
    {
      await ThrowsException(typeof(TEx), operation);
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Makes sure that the given chunk of code, '<paramref name="operation"/>' throws the expected exception.
    /// This is useful in that you don't have to write multiple test cases w/ the 'ExpectedException' attribute.
    /// </summary>
    public async static Task ThrowsException(Type exceptionType, Task operation)
    {
      try
      {
        await operation;
      }
      catch (Exception ex)
      {
        if (ex.GetType() == exceptionType) { return; }
      }

      throw new AssertFailedException(string.Format("Operation did not throw expected exception '{0}'", exceptionType));
    }
#endif


    // --------------------------------------------------------------------------------------------------------------------------
    public static void ThrowsException<TEx>(Action operation)
    {
      ThrowsException(typeof(TEx), operation);
    }


    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Makes sure that the given chunk of code, '<paramref name="operation"/>' throws the expected exception.
    /// This is useful in that you don't have to write multiple test cases w/ the 'ExpectedException' attribute.
    /// </summary>
    public static void ThrowsException(Type exceptionType, Action operation)
    {
      try
      {
        operation.Invoke();
      }
      catch (Exception ex)
      {
        if (ex.GetType() == exceptionType) { return; }
      }

      throw new AssertFailedException(string.Format("Operation did not throw expected exception '{0}'", exceptionType));
    }
  }


}
