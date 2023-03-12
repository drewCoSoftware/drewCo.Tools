using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace drewCo.Tools.Core.Testers
{

  // ============================================================================================
  public class JsonTesters
  {
    // ----------------------------------------------------------------------------------------
    /// <summary>
    /// This test case was provided to solve a problem where the json serializer emitted a BOM by default.
    /// for purposes of interoperability, we don't want or need a BOM.
    /// </summary>
    [Test]
    public void JsonSerializerDoesNotEmitBOM()
    {
      const string TEST_PATH = nameof(JsonSerializerDoesNotEmitBOM) + ".json";
      var data = new { Name = "Name", Number = 123 };
      FileTools.SaveJson(TEST_PATH, data);

      // Now we will read the data back in, byte-wise and make sure that there isn't a BOM.
      byte[] byteData = FileTools.ReadAllBytes(TEST_PATH);
      Assert.AreNotEqual(0xEF, byteData[0]);
      Assert.AreNotEqual(0xBB, byteData[1]);
      Assert.AreNotEqual(0xBF, byteData[2]);

    }
  }
}
