using drewCo.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if NETFX_CORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

namespace drewCo.Tools.Testers
{
  // ============================================================================================================================
  [TestClass]
  public class FileToolsTesters
  {
    public const string TEST_DEFAULT_VALUE = "This is a test!";

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// This shows that we can find files using certain date criteria.
    /// </summary>
    [TestMethod]
    public void CanFindFilesByDate()
    {
      // Create a test dir + populate it with some files.

      string testDir = $"{nameof(CanFindFilesByDate)}_test-files";
      FileTools.EmptyDirectory(testDir);
      FileTools.CreateDirectory(testDir);
      DateTime startDate = new DateTime(2000, 1, 1);

      // Set each write time like 1 hour apart.
      const int FILE_COUNT = 10;
      for (int i = 0; i < FILE_COUNT; i++)
      {
        //  make the files....
        string path = Path.Combine(testDir, $"test-file_{i}");
        using (var fs = File.Create(path))
        {
          EZWriter.RawString(fs, "Hello " + i);
        }

        var fi = new FileInfo(path);
        fi.LastWriteTime = startDate + (TimeSpan.FromHours(i));
      }

      // Show we can find files before and after some certain date.
      // Show that no files outside of a certain bound will be located.
      DateTime cutoff = startDate + (TimeSpan.FromHours(FILE_COUNT / 2));

      // BEFORE:
      {
        var files = FileTools.FindFiles(testDir, new FindFilesOptions()
        {
          Cutoff = cutoff,
          DateCompareType = EDateComparisonType.Before
        });

        foreach (var file in files)
        {
          var fi = new FileInfo(file);
          Assert.IsTrue(fi.LastWriteTime < cutoff, "Incorrect write time!");
        }
      }

      // AFTER:
      {
        var files = FileTools.FindFiles(testDir, new FindFilesOptions()
        {
          Cutoff = cutoff,
          DateCompareType = EDateComparisonType.After
        });

        foreach (var file in files)
        {
          var fi = new FileInfo(file);
          Assert.IsTrue(fi.LastWriteTime > cutoff, "Incorrect write time!");
        }
      }

    }


    // --------------------------------------------------------------------------------------------------------------------------
    [TestMethod]
    public void CanCreateBackupFile()
    {
      string myDir = Path.Combine(FileTools.GetAppDir(), "Sequential");
      FileTools.CreateDirectory(myDir);
      FileTools.EmptyDirectory(myDir);

      Assert.IsTrue(Directory.GetFiles(myDir, "*.*", SearchOption.AllDirectories).Length == 0, "The test directory should be empty!");

      string srcPath = Path.Combine(myDir, "TestSrcFile.txt");
      File.WriteAllText(srcPath, "test data!");

      string newPath = FileTools.CreateBackup(srcPath);
      Assert.IsTrue(File.Exists(newPath));
    }

    // --------------------------------------------------------------------------------------------------------------------------
    [TestMethod]
    public void CanGetSequentialFileNameFromExistingPath()
    {
      string myDir = Path.Combine(FileTools.GetAppDir(), "Sequential");
      FileTools.CreateDirectory(myDir);
      FileTools.EmptyDirectory(myDir);

      string srcPath = Path.Combine(myDir, "TestSrcFile.txt");
      File.WriteAllText(srcPath, "test data!");

      string nextPath = FileTools.GetSequentialFileName(srcPath);
      string nextDir = Path.GetDirectoryName(nextPath);
      string nextName = Path.GetFileName(nextPath);

      Assert.AreEqual(myDir, nextDir);
      Assert.AreEqual(nextName, "TestSrcFile-0.txt");
    }

    // --------------------------------------------------------------------------------------------------------------------------
    [TestMethod]
    public void CanAppendFileName()
    {
      {
        const string BASE = "c:\\someDir\\MyFile.txt";
        string appended = FileTools.AppendToFileName(BASE, "_thing");
        Assert.AreEqual("c:\\someDir\\MyFile_thing.txt", appended, "Incorrect appended path! [1]");
      }

      {
        const string BASE = "MyFile.txt";
        string appended = FileTools.AppendToFileName(BASE, "_thing");
        Assert.AreEqual("MyFile_thing.txt", appended, "Incorrect appended path! [2]");
      }

      {
        const string BASE = "MyFile";
        string appended = FileTools.AppendToFileName(BASE, "_thing");
        Assert.AreEqual("MyFile_thing", appended, "Incorrect appended path! [3]");
      }

    }


    // --------------------------------------------------------------------------------------------------------------------------
    [TestMethod]
    public void CanGetFileParts()
    {
      {
        const string PATH = "MyPath\\somefile.txt";
        FileTools.GetFilePathParts(PATH, out string dir, out string name, out string ext);
        Assert.AreEqual("MyPath", dir, "The directory is incorrect! [1]");
        Assert.AreEqual("somefile", name, "The file name is incorrect! [1]");
        Assert.AreEqual(".txt", ext, "The extension is incorrect! [1]");
      }

      {
        const string PATH = "somefile.abc";
        FileTools.GetFilePathParts(PATH, out string dir, out string name, out string ext);
        Assert.AreEqual(null, dir, "The directory is incorrect! [2]");
        Assert.AreEqual("somefile", name, "The file name is incorrect! [2]");
        Assert.AreEqual(".abc", ext, "The extension is incorrect! [2]");
      }

      {
        const string PATH = "c:\\MyDir\\AndStuff\\otherthing.html";
        FileTools.GetFilePathParts(PATH, out string dir, out string name, out string ext);
        Assert.AreEqual("c:\\MyDir\\AndStuff", dir, "The directory is incorrect! [3]");
        Assert.AreEqual("otherthing", name, "The file name is incorrect! [3]");
        Assert.AreEqual(".html", ext, "The extension is incorrect! [3]");
      }

      {
        const string PATH = "c:\\MyDir\\AndStuff\\otherthing";
        FileTools.GetFilePathParts(PATH, out string dir, out string name, out string ext);
        Assert.AreEqual("c:\\MyDir\\AndStuff", dir, "The directory is incorrect! [4]");
        Assert.AreEqual("otherthing", name, "The file name is incorrect! [4]");
        Assert.AreEqual(null, ext, "The extension is incorrect! [4]");
      }

      {
        const string PATH = "\\yourStuff.x";
        FileTools.GetFilePathParts(PATH, out string dir, out string name, out string ext);
        Assert.AreEqual(string.Empty, dir, "The directory is incorrect! [5]");
        Assert.AreEqual("yourStuff", name, "The file name is incorrect! [5]");
        Assert.AreEqual(".x", ext, "The extension is incorrect! [5]");
      }

    }



    // --------------------------------------------------------------------------------------------------------------------------
    [TestMethod]
    public void CanChangeExtension()
    {
      const string PATH = "myPath.txt";
      const string PATH2 = "otherPath";

      // Show that we can change the extension.
      string change1 = FileTools.ChangeExtension(PATH, ".thingy");
      Assert.AreEqual("myPath.thingy", change1);

      string change2 = FileTools.ChangeExtension(PATH2, ".thingy");
      Assert.AreEqual("otherPath.thingy", change2);

      // Show that we can strip the extension.
      string change3 = FileTools.ChangeExtension(PATH, "");
      Assert.AreEqual("myPath", change3);

      string change4 = FileTools.ChangeExtension(PATH2, "");
      Assert.AreEqual("otherPath", change4);


    }


    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Show that we can increment the number at the end of a file name, according to our spec.
    /// </summary>
    [TestMethod]
    public void CanIncrementFileNames()
    {
      string path1 = "c:\\SomeDir\\MyFile.abc";
      string path2 = "c:\\SomeDir\\MyFile_123.def";

      Assert.AreEqual("c:\\SomeDir\\MyFile_124.def", FileTools.IncrementFileName(path2));
      Assert.AreEqual("c:\\SomeDir\\MyFile_1.abc", FileTools.IncrementFileName(path1));
    }


    // --------------------------------------------------------------------------------------------------------------------------
    [TestMethod]
    public void CanComputeRelativePath()
    {
      string basePath = @"c:\something\or\rather";
      string filePath = @"\otherdir\mytestfile.txt";
      string myFile = basePath + filePath;

      string relative = FileTools.ComputeRelativePath(basePath, myFile);
      Assert.AreEqual(filePath, relative, "Invalid relative path!");
    }


    // TODO: We should split these test files, maybe?
#if NETFX_CORE
    // --------------------------------------------------------------------------------------------------------------------------
    [TestMethod]
    public async Task CanCreateDirectory()
    {
      // Normal directory, one level deep.
      {
        const string TEST_DIR_NAME = "MyTestDir";
        string myDir = FileTools.GetAppDir();
        string testDir = Path.Combine(myDir, TEST_DIR_NAME);

        FileTools.DeleteExistingDirectory(testDir);
        Assert.IsFalse(FileTools.FolderExists(testDir).Result, "The test directory should not exist!");

        await FileTools.CreateDirectory(testDir);
        Assert.IsTrue(FileTools.FolderExists(testDir).Result, "The test directory is missing!");

      }

    }

#endif

#if !NETFX_CORE

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Shows that the 'DefaultValue' attribute will actually work on our XMLFile instances.
    /// </summary>
    [TestMethod]
    public void CanGetDefaultValuesOnXMLFile()
    {

      string testPath1 = FileTools.GetAppDir() + "\\Test-ExampleFile-1.xml";
      string testPath2 = FileTools.GetAppDir() + "\\Test-ExampleFile-1.xml";

      FileTools.DeleteExistingFile(testPath1);
      FileTools.DeleteExistingFile(testPath2);

      Assert.IsFalse(File.Exists(testPath1), "The test file should not exist! [1]");
      Assert.IsFalse(File.Exists(testPath2), "The test file should not exist! [2]");

      ExampleFile file1 = ExampleFile.Load(testPath1, true);
      ExampleFile file2 = ExampleFile.Load(testPath2, true);

      Assert.IsNotNull(file1, "We should have a default instance of our test file. [1]");
      Assert.IsNotNull(file2, "We should have a default instance of our test file. [2]");


      Assert.AreEqual(TEST_DEFAULT_VALUE, file1.TheString, "Bad default value for the string! [1]");
      Assert.AreEqual(TEST_DEFAULT_VALUE, file2.TheString, "Bad default value for the string! [2]");

      // Make sure that there are number lists, and that they are different references. (why we are testing two files)
      Assert.IsNotNull(file1.NumberList, "The number list should not be null! [1]");
      Assert.IsNotNull(file2.NumberList, "The number list should not be null! [2]");

      Assert.IsFalse(object.ReferenceEquals(file1.NumberList, file2.NumberList), "The default lists should be different objects!");

    }

#endif
  }




  // ============================================================================================================================
  public class ExampleFile : XMLFile<ExampleFile>
  {
    [DefaultValue(FileToolsTesters.TEST_DEFAULT_VALUE)]
    public string TheString { get; set; }

    [DefaultValue(UseNewInstace = true)]
    public List<int> NumberList { get; set; }
  }

}
