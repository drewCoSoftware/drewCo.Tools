using drewCo.Tools;


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if !NETFX_CORE
using Microsoft.VisualStudio.TestTools.UnitTesting;
#else
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#endif


namespace ReflectionToolsTesters
{

  // ============================================================================================================================
  [TestClass]
  public class ConverterTesters
  {

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// This test was provided to solve a bug where trying to convert a DateTime? value with a null input would cause a crash.
    /// This actually solves the problems of converting nullable(T) types with null inputs.
    /// </summary>
    [TestMethod]
    public void CanConvertNullableDateTime()
    {

      DateTime? val = ReflectionTools.ConvertEx<DateTime?>(null);
      Assert.IsNull(val, "The converted value should be null!");

    }


    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Demonstrates a bug fix where a call to ConvertEx would fail when using a guid.
    /// </summary>
    [TestMethod]
    public void CanConvertGuidToString()
    {
      Guid g = Guid.NewGuid();
      string s = g.ToString();

      string comp = ReflectionTools.ConvertEx<string>(g);
      Assert.AreEqual(comp, s, "Invalid string value for GUID");
    }

  }
}
