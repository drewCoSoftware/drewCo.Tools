using drewCo.MathTools.Geometry;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public void CanDetectLineSegmentIntersectingWithRectangle()
    {

      var rect = new Rectangle(-1.0d, -1.0d, 2.0d, 2.0d);

      // Some line segments that should intersect with the rectangle.  Add as many as you need.
      var intersecting = new[] {
        // Overlapping right / left sides.
        new LineSegment(-2.0d, 0.0d, 0.0d, 0.0d),
        new LineSegment(0.0, 0.0d, 2.0d, 0.0d),
        new LineSegment(-2.0d, 0.0d, 2.0d, 0.0d),

        // Overlapping top / bottom, vertical
        new LineSegment(0.0d, -2.0d, 0.0d, 0.0d),
        new LineSegment(0.0d, 0.0d, 2.0d, 0.0d),
        new LineSegment(0.0d, -2.0d, 0.0d, 2.0d),

        // Oerlapping top / bottom, not vertical
        new LineSegment(0.0d, -2.0d, 0.5d, 0.0d),
        new LineSegment(0.0d, 0.0d, -0.5d, 2.0d),
        new LineSegment(-0.5d, -2.0d, 0.5d, 2.0d),

        // Totally contained:
        new LineSegment(-0.5d, -0.5d, 0.5d, 0.5d)
      };

      var notIntersecting = new[] {
        // Partial overlap in x-set.
        new LineSegment(-3.0d, -3.0d, 0.0d, -2.0d),
        new LineSegment(0.0d, -2.0d, -3.0d, -3.0d),

        // Contained x-set
        new LineSegment(-0.5d, -3.0d, 0.5d, -2.0d)
      };
    

      foreach (var line in intersecting)
      {
        Assert.IsTrue(line.Intersects(rect), $"The line segment ({line.P1.X}, {line.P1.Y})-({line.P2.X}, {line.P2.Y}) should intersect with our rectangle!");
      }
      foreach (var line in notIntersecting)
      {
        Assert.IsFalse(line.Intersects(rect), $"The line segment ({line.P1.X}, {line.P1.Y})-({line.P2.X}, {line.P2.Y}) should NOT intersect with our rectangle!");
      }


      // Try our other version.....
      foreach (var line in intersecting)
      {
        Assert.IsTrue(line.IntersectsEx(rect), $"The line segment ({line.P1.X}, {line.P1.Y})-({line.P2.X}, {line.P2.Y}) should intersect with our rectangle! [ex]");
      }
      foreach (var line in notIntersecting)
      {
        Assert.IsFalse(line.IntersectsEx(rect), $"The line segment ({line.P1.X}, {line.P1.Y})-({line.P2.X}, {line.P2.Y}) should NOT intersect with our rectangle! [ex]");
      }


      // Load test..
      return;

      const long MAX = (long)10e6;
      {
        long count = 0;
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < MAX; i++)
        {
          foreach (var line in intersecting)
          {
            bool res = line.Intersects(rect);
            ++count;
          }
        }
        Console.WriteLine($"V1 (intersecting) Execution took: {sw.Elapsed.TotalSeconds:f3} [{count}]");
      }

      {
        long count = 0;
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < MAX; i++)
        {
          foreach (var line in intersecting)
          {
            bool res = line.IntersectsEx(rect);
            ++count;
          }
        }
        Console.WriteLine($"V2 (intersecting) Execution took: {sw.Elapsed.TotalSeconds:f3} [{count}]");
      }

      {
        long count = 0;
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < MAX; i++)
        {
          foreach (var line in notIntersecting)
          {
            bool res = line.Intersects(rect);
            ++count;
          }
        }
        Console.WriteLine($"V1 (not intersecting) Execution took: {sw.Elapsed.TotalSeconds:f3} [{count}]");
      }
      {
        long count = 0;
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < MAX; i++)
        {
          foreach (var line in notIntersecting)
          {
            bool res = line.IntersectsEx(rect);
            ++count;
          }
        }
        Console.WriteLine($"V1 (not intersecting) Execution took: {sw.Elapsed.TotalSeconds:f3} [{count}]");
      }
    }


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
