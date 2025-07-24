using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace drewCo.Tools.Core.Testers
{

  // ==============================================================================================================================
  public class FileToolTesters
  {

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// This test case shows that we can actually compute sequential file names and resolve collisions
    /// in them correctly.  Unlike before, this no longer relies on some arbitrary count of files....
    /// </summary>
    [Test]
    public void CanComputeSequentialFileNameInCrowdedDirectory()
    {

      const string TEST_DIR = "test-sequence-1";
      FileTools.EmptyDirectory(TEST_DIR);
      FileTools.CreateDirectory(TEST_DIR);

      var files = Directory.GetFiles(TEST_DIR);
      Assert.That(files.Count, Is.EqualTo(0), "There should be no files in the directory!");

      // This is designed so that when the system finds the newest file, its number is 9998.
      // It should then be able to determine that the file with the largest sequence is 9999,
      // and ultimately return the path: file-10000.txt.
      var file3 = Path.Combine(TEST_DIR, "file-444.txt");
      File.WriteAllText(file3, "test-444");

      Thread.Sleep(1000);
      var file2 = Path.Combine(TEST_DIR, "file-9999.txt");
      File.WriteAllText(file2, "test-9999");

      Thread.Sleep(1000);
      const string OLDEST_FILE_NAME = "file-9998.txt";
      var file1 = Path.Combine(TEST_DIR, OLDEST_FILE_NAME);
      File.WriteAllText(file1, "test-9998");

      // Make sure the write times are what we expect.
      var inDir = FileTools.GetFileInfosByDate(TEST_DIR, "*.txt", SearchOption.TopDirectoryOnly);
      Assert.That(inDir[0].Name, Is.EqualTo(OLDEST_FILE_NAME));


      // Now get the next sequence...
      string nextPath = FileTools.GetSequentialFileName(TEST_DIR, "file", ".txt");
      string nextName = Path.GetFileName(nextPath);

      Assert.That(nextName, Is.EqualTo("file-10000.txt"));
    }

    // --------------------------------------------------------------------------------------------------------------------------
    [Test]
    public void CanComputeSequentialFileNameFromExistingName()
    {

      string testDir = FileTools.GetLocalDir("test-sequence-2");
      FileTools.EmptyDirectory(testDir);
      FileTools.CreateDirectory(testDir);

      string testPath = Path.Combine(testDir, "my-file.json");

      string outputPath = FileTools.GetSequentialFileName(testPath);

      // The file names should be the same after the first call!.
      Assert.AreEqual(testPath, outputPath);
      File.WriteAllText(outputPath, "test-data");

      // Let's get another one, just to make sure we are working as expected....
      string outputPath2 = FileTools.GetSequentialFileName(testPath);
      string fileName = Path.GetFileName(outputPath2);
      Assert.AreEqual("my-file-1.json", fileName, "Invalid file name after getting sequential name!");
    }

  }
}
