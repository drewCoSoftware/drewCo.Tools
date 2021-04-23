using drewCo.MathTools;
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
    const long MAX_TEST_COUNT = (long)10e6;
    private const double EPSILON = 0.0000001;


    // --------------------------------------------------------------------------------------------------------------------------
    [TestMethod]
    public void CanComputePolygonAngles()
    {

      // Let's try for a triangle.  Any old triangle should do....
      var triangleVerts = new[] {
        new Vector2(0,0),
        new Vector2(0,10),
        new Vector2(10,0)
      };
      var trianglePoly = new Polygon(triangleVerts);

      // This should add up to 180 degrees, or pi.
      var verts = trianglePoly.VerticesEx;
      double angleSum = (from x in verts select x.Angle).Sum();
      Assert.AreEqual(Math.PI, angleSum, EPSILON);


      var r = new Rectangle(0, 0, 10, 10);
      Polygon p = r.ToPolygon();

      const double HALF_PI = Math.PI * 0.5d;

      // Each should be 90 degrees!
      foreach (var v in p.VerticesEx)
      {
        Assert.AreEqual(v.Angle, HALF_PI, EPSILON);
      }


    }


    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// This solves a case where a 
    /// </summary>
    [TestMethod]
    public void CanDetectPolygonThatContainsRectangle()
    {
      // This is a pretty trivial case, but gets the point across.  A small rectangle is inside of a large one.
      // Both can ultimately be represented as polygons, so we use that code path.
      const double SIZE = 50.0d;
      var polygon = new Rectangle(-SIZE, -SIZE, SIZE * 2.0d, SIZE * 2.0d).ToPolygon();

      const double SIZE_2 = SIZE * 0.5d;
      var rect = new Rectangle(-SIZE, -SIZE, SIZE_2 * 2.0d, SIZE_2 * 2.0d);

      Assert.IsTrue(polygon.Contains(rect));
    }


    // --------------------------------------------------------------------------------------------------------------------------
    [TestMethod]
    public void CanDetectRectangleToSegmentIntersection()
    {

      // Some other trouble cases...
      {
        Rectangle r = new Rectangle(112.5, 37.5, 37.5, 37.5);
        LineSegment s = new LineSegment(100, 21, 213, 86);
        Assert.IsTrue(r.Intersects(s), "There should be an intersect!");
      }

      {
        Rectangle r = new Rectangle(0.0d, 0.0d, 100.0d, 100.0d);
        LineSegment s = new LineSegment(-20.0d, 9.0d, 5.0d, 34.0d);
        Assert.IsTrue(r.Intersects(s), "There should be an intersect!");
      }

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



      foreach (var line in intersecting)
      {
        Assert.IsTrue(line.Intersects(rect), $"The line segment ({line.P1.X}, {line.P1.Y})-({line.P2.X}, {line.P2.Y}) should intersect with our rectangle!");
      }



      // Load test...
      return;

      long count = 0;
      var sw = Stopwatch.StartNew();
      for (int i = 0; i < MAX_TEST_COUNT; i++)
      {
        foreach (var line in intersecting)
        {
          bool res = line.Intersects(rect);
          ++count;
        }
      }
      Console.WriteLine($"V1 (intersecting) Execution took: {sw.Elapsed.TotalSeconds:f3} [{count}]");



    }

    // --------------------------------------------------------------------------------------------------------------------------
    [TestMethod]
    public void CanDetectNonRectangleToSegmentIntersection()
    {
      var rect = new Rectangle(-1.0d, -1.0d, 2.0d, 2.0d);

      var notIntersecting = new[] {
        // Partial overlap in x-set.
        new LineSegment(-3.0d, -3.0d, 0.0d, -2.0d),
        new LineSegment(0.0d, -2.0d, -3.0d, -3.0d),

        // Contained x-set
        new LineSegment(-0.5d, -3.0d, 0.5d, -2.0d)
      };
      foreach (var line in notIntersecting)
      {
        Assert.IsFalse(line.Intersects(rect), $"The line segment ({line.P1.X}, {line.P1.Y})-({line.P2.X}, {line.P2.Y}) should NOT intersect with our rectangle!");
      }

      return;

      // Load Test.
      long count = 0;
      var sw = Stopwatch.StartNew();
      for (int i = 0; i < MAX_TEST_COUNT; i++)
      {
        foreach (var line in notIntersecting)
        {
          bool res = line.Intersects(rect);
          ++count;
        }
      }
      Console.WriteLine($"V1 (not intersecting) Execution took: {sw.Elapsed.TotalSeconds:f3} [{count}]");

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
