using NUnit.Framework;
using System.Text;

namespace drewCo.Tools.Core.Testers
{
  // ============================================================================================================================
  public class Tests
  {

    // --------------------------------------------------------------------------------------------------------------------------
    [Test]
    public void CanReadAndWriteXMLFileFromString()
    {
      var file = new SomeXMLFile();
      file.NameContent = "my-file";
      file.NumberContent = 123;


      // Write + Read.....
      string xmlData = file.ToString();
      var check = SomeXMLFile.FromString(xmlData);

      Assert.AreEqual("my-file", check.NameContent, "Incorrect name content!");
      Assert.AreEqual(123, check.NumberContent, "Incorrect number content!");

      Assert.Pass();
    }

  }

  // ============================================================================================================================
  public class SomeXMLFile : XMLFile<SomeXMLFile>
  {
    public string NameContent { get; set; }
    public int NumberContent { get; set; }
  }


}