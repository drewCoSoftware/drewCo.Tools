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

      string testName = Path.Combine(testDir, "my-file.json");

      string output = FileTools.GetSequentialFileName(testName);

      // The file names should be the same after the first call!.
      Assert.AreEqual(testDir, testName);
    }

  }
}
