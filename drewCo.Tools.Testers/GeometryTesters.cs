using drewCo.MathTools.Geometry;
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
  public class GeometryTesters
  {

    // --------------------------------------------------------------------------------------------------------------------------
    [TestMethod]
    public void CanDetecctSegmentIntersectingCircles()
    {
      {
        var segment = new LineSegment(-10.0d, 0.0d, 10.0d, 0.0d);
        var circle = new Circle(0.0d, 0.0d, 5.0d);

        Assert.IsTrue(segment.Intersects(circle), "The segment should intersect the circle!");
      }
    }


  }

}
