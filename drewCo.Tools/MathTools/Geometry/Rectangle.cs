using drewCo.MathTools;
using drewCo.MathTools.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace drewCo.MathTools.Geometry
{

  // ============================================================================================================================
  /// <summary>
  /// It is a rectangle!
  /// </summary>
  /// <remarks>
  /// Rectangles are assumed to be axis aligned.
  /// </remarks>
  public class Rectangle : IIntersectsRect
  {
    // --------------------------------------------------------------------------------------------------------------------------
    /// <param name="x">The center, X coordinate</param>
    /// <param name="y">The center, Y coordinate</param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    public Rectangle(double x, double y, double width, double height)
    {
      X = x;
      Y = y;
      Width = width;
      Height = height;

      Sides = new[]  {
        new LineSegment(X, Y, x + Width, Y),
        new LineSegment(X + Width, y, X+Width, Y + Height),
        new LineSegment(x + Width, Y + Height, X, Y + Height),
        new LineSegment(X, Y + Height, x , Y)
      };


    }

    // --------------------------------------------------------------------------------------------------------------------------
    public Polygon ToPolygon()
    {
      var res = new Polygon(new[] {
        new Vector2(X, Y),
        new Vector2(X + Width, Y),
        new Vector2(X + Width, Y + Height),
        new Vector2(X, Y + Height)
      });
      return res;
    }

    /// <summary>
    /// Center-X
    /// </summary>
    public double X { get; private set; }

    /// <summary>
    /// Center-Y
    /// </summary>
    public double Y { get; private set; }

    public double Width { get; private set; }
    public double Height { get; private set; }

    public Vector2 Center { get { return new Vector2(X + Width * 0.5d, Y + Height * 0.5d); } }

    public bool IsSquare { get { return Width == Height; } }


    public LineSegment[] Sides { get; private set; }


    // ------------------------------------------------------------------------------------------------    
    public bool Intersects(Rectangle rect)
    {
      double myRight = this.X + this.Width;
      double right = rect.X + rect.Width;

      // X-Bounds.
      if (rect.X > myRight) { return false; }
      if (right < this.X) { return false; }


      double myTop = this.Y + this.Height;
      double top = rect.Y + this.Height;

      // Y-Bounds
      if (rect.Y > myTop) { return false; }
      if (top < this.Y) { return false; }

      // The boundaries are OK, so this rect must be intersecting.
      // We could also use the 'SetsOverlap' function on the x and y axes to determine this result.
      return true;

    }

    // ------------------------------------------------------------------------------------------------    
    // Special thanks:
    // https://stackoverflow.com/questions/401847/circle-rectangle-collision-detection-intersection#402010
    // It is the AABB related answer.
    // --> This code was modified quite a bit to make it work with our stuff.
    public bool Intersects(Circle circle)
    {
      double circleX = circle.X;
      double circleY = circle.Y;

      Vector2 center = this.Center;
      double rectX = center.X;
      double rectY = center.Y;

      // Vector2 circleDistance = new Vector2();
      double xDist = Math.Abs(circleX - rectX);
      double yDist = Math.Abs(circleY - rectY);

      double rw2 = this.Width * 0.5d;
      if (xDist > rw2 + circle.Radius) { return false; }

      double rh2 = this.Height * 0.5d;
      if (yDist > rh2 + circle.Radius) { return false; }

      if (xDist <= rw2) { return true; }
      if (yDist <= rh2) { return true; }

      double distX = xDist - rw2;
      double distY = yDist - rh2;
      double cornerDist = distX * distX + distY * distY;

      double circleRadius = circle.Radius * circle.Radius;
      return cornerDist <= circleRadius;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Does the given point intersect (is it inside of) this rectangle?
    /// </summary>
    public bool Intersects(Vector2 v)
    {
      bool res = v.X >= this.X &&
                 v.X <= this.X + this.Width &&
                 v.Y >= this.Y &&
                 v.Y <= this.Y + this.Height;
      return res;

    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// This is meant to be a faster version of the 'intersects' code.
    /// The idea is that we consider the rectangle as a set of points, and the line segment as a set of points.
    /// If there is any overlap in those sets, then we have an intersection.
    /// </summary>
    public bool Intersects(LineSegment segment)
    {
      // Check for containment of the points.
      if (this.Intersects(segment.P1) || this.Intersects(segment.P2)) { return true; }

      // Slow, but reliable.  Check each of our segments to the given segment.
      foreach (var s in this.Sides)
      {
        if (s.Intersects(segment)) { return true; }
      }
      return false;


      // We attempted to optimize this function at some point.
      // It was a good effort, but ultimately a failure.  We can investigate optimizations again in the future.....
      // The big idea here is to make sure that the convex hull of the rectangle contains one or more set
      // items from the line segment.
      // Check to see if x overlaps left or right side, or is contained.
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Given a min/max value for two sets, this will tell us if there is any overlap between them.
    /// </summary>
    private bool SetsOverlap(double min1, double max1, double min2, double max2)
    {
      bool res = (max1 >= min2) && (max2 >= min1);
      return res;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Is the given line segment entirely inside of the rectangle?
    /// NOTE: 'Intersects' will also be true when this function is true.
    /// </summary>
    public bool Contains(LineSegment segment)
    {
      bool res = Contains(segment.P1) && Contains(segment.P2);
      return res;
    }

    // --------------------------------------------------------------------------------------------------------------------------
    public bool Contains(Vector2 point)
    {
      bool res = point.X >= X && point.X <= X + Width && point.Y >= Y && point.Y <= Y + Height;
      return res;
    }


    // --------------------------------------------------------------------------------------------------------------------------
    public bool Intersects(Polygon poly)
    {
      foreach (var segment in poly.Sides)
      {
        if (Intersects(segment)) { return true; }
      }
      return false;

    }

  }


  // ============================================================================================================================
  /// <summary>
  /// Simple interface to test rectangle intersection.
  /// </summary>
  public interface IIntersectsRect
  {
    bool Intersects(Rectangle rect);
  }

}
