using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace drewCo.Tools.Core.Testers
{

  public class FileToolTesters
  {

    [Test]
    public void CanComputeSequentialFileNameFromExistingName()
    {

      string testDir = FileTools.GetLocalDir("test-sequential");
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
