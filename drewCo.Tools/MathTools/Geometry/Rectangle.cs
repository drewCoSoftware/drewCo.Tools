using drewCo.MathTools;
using drewCo.MathTools.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
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
  public class Rectangle
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
    public bool Intersects(LineSegment segment)
    {
      if (this.Contains(segment.P1) || this.Contains(segment.P2))
      {
        return true;
      }


      // NOTE: Another way entirely is to determine where the segment overlaps the rectangle in the x-axis.  From there, it is a matter
      // of pluggin that x-value into the segment function to see if the resulting y-coordinate is in the y-range of the rectangle.

      // NOTE: This is the most basic way to do this.  I thint that there is a calculus way that will compute it even faster,
      // but I don't have the math chops to figure that out just yet.
      foreach (var s in Sides)
      {
        if (segment.Intersects(s))
        {
          return true;
        }
      }
      return false;

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
}
