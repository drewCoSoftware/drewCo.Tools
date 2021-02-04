using drewCo.MathTools;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace drewCo.Tools.Testers
{
  // ============================================================================================================================
  [TestClass]
  public class VectorTesters
  {

    // --------------------------------------------------------------------------------------------------------------------------
    [TestMethod]
    public void CanComputePerpendicularVectorInEitherDirection()
    {
      const int SEED = 1234;
      const int MAX_TESTS = 100;
      var rng = new Random(SEED);

      for (int i = 0; i < MAX_TESTS; i++)
      {
        // Generate a random vector.
        var src = new Vector2(rng.NextDouble(), rng.NextDouble()) * rng.Next(1, 10);

        // Generate + check the left/right perpendicular vectors.
        Vector2 perpLeft = src.Perpendicular(1.0d);
        double crossLeft = System.Math.Sign(src.Cross(perpLeft));
        Assert.AreEqual(1.0d, crossLeft, "Incorrect cross product for left vector!");

        Vector2 perpRight = src.Perpendicular(-1.0d);
        double crossRight = System.Math.Sign(src.Cross(perpRight));
        Assert.AreEqual(-1.0d, crossRight, "Incorrect cross product for right vector!");
      }

    }


  }
}
