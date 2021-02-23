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
    /// <summary>
    /// This is meant to be a fater version of the 'intersects' code.
    /// The idea is that we consider the rectangle as a set of points, and the line segment as a set of points.
    /// If there is any overlap in those sets, then we have an intersection.
    /// </summary>
    public bool Intersects(LineSegment segment)
    {
      // Check to see if x overlaps left or right side, or is contained.
      //double otherX  = X + Width;
      double xIntersect = X; // 0.0d;



      // We will sort our points so that x1 <= x2
      // Make sure that we always represetn the segment as moving from LEFT to RIGHT.
      // NOTE: We may want to make this a condition of 'LineSegment'.
      Vector2 p1 = segment.P1;
      Vector2 p2 = segment.P2;
      
      // Sort so that x2 >= x1
      // Is there some way to skip this part?
      if (p2.X < p1.X) { p1 = segment.P2; p2 = segment.P1; }

      if (p1.X < X)
      {
        // P1 is left of the rectangle.
        if (p2.X < X)
        {
          // No x-overlap, no possible intersection.
          return false;
        }
      }
      else
      {
        if (p1.X > X + Width)
        {
          // No x-overlap.  no possible intersection.
          return false;
        }
        else
        {
          // p1x is inside of the rectangle.
          if (p2.X <= X + Width)
          {
            // X-Set is entirely contained.  If there is any y-set overlap we have it.
            double minY = Math.Min(p1.Y, p2.Y);
            double maxY = Math.Max(p1.Y, p2.Y);
            return SetsOverlap(Y, Y + Height, minY, maxY);
          }
          else
          {
            // Right side overlap in X.
            xIntersect = X + Width;
          }
        }
      }


      // We have an intersect on X, so we need to find out where the corresponding y-value is.
      if (p1.X == p2.X)
      {
        // Vertical line.  Check for y-set overlap.
        double minY = Math.Min(p1.Y, p2.Y);
        double maxY = Math.Max(p1.Y, p2.Y);
        return SetsOverlap(Y, Y + Height, minY, maxY);
      }
      else
      {
        double xPart = p2.X - p1.X;
        double slope = (p1.Y - p2.Y) / xPart;
        double xDiff = xIntersect - p1.X;
        double yIntersect = p1.Y + (xDiff * slope);
        bool res = yIntersect >= Y && yIntersect <= (Y + Height);
        return res;
      }



      // // Original Version below.  This is actually a bit faster than the version above...
      // // I like the above version as it is easier to follow IMO.
      //if (segment.P1.X >= X && segment.P1.X <= otherX && segment.P2.X >= X && segment.P2.X <= otherX)
      //{
      //  // Segment's x-set is totally contained in the rectangle's x-set.
      //  // We need only to check for y-set overlap.
      //  double y1 = segment.P1.Y;
      //  double y2 = segment.P2.Y;
      //  if (y2 < y1)
      //  {
      //    y1 = segment.P2.Y;
      //    y2 = segment.P1.Y;
      //  }

      //  bool res = SetsOverlap(Y, Y + Height, y1, y2);
      //  return res;
      //}


      //// Make sure that we always represetn the segment as moving from LEFT to RIGHT.
      //// NOTE: We may want to make this a condition of 'LineSegment'.
      //Vector2 p1 = segment.P1;
      //Vector2 p2 = segment.P2;
      //if (p2.X < p1.X) { p1 = segment.P2; p2 = segment.P1; }

      //double xIntersect = 0.0d;
      //if (p1.X <= this.X)
      //{
      //  // Left side check.
      //  if (p2.X >= this.X)
      //  {
      //    xIntersect = this.X;
      //  }
      //  else
      //  {
      //    // No intersection.
      //    return false;
      //  }
      //}
      //else
      //{
      //  // Right side check.
      //  if (p1.X <= this.X + this.Width)
      //  {
      //    xIntersect = p2.X;
      //  }
      //  else
      //  {
      //    // No intersection.
      //    return false;
      //  }
      //}

      //// We can now compute a y-coord for the intersect point.
      //// double yPart = p2.Y - p1.Y;
      //double xPart = p2.X - p1.X;
      //if (xPart == 0.0d)
      //{
      //  // Vertical line.
      //  // If any of the y-points are in range, we are good.
      //  // Sort values + check for subset of rectangle's Y + height.
      //  double y1 = p1.Y;
      //  double y2 = p2.Y;
      //  if (y2 < y1)
      //  {
      //    y1 = p2.Y;
      //    y2 = p1.Y;
      //  }

      //  bool res = SetsOverlap(Y, Y + Height, y1, y2);
      //  return res;
      //}
      //else
      //{
      //  double slope = (p1.Y - p2.Y) / xPart;
      //  double xDiff = xIntersect - p1.X;
      //  double yIntersect = p1.Y + (xDiff * slope);
      //  bool res = yIntersect >= Y && yIntersect <= (Y + Height);
      //  return res;
      //}
    }

    // --------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Given a min/max value for two sets, this will tell us if there is any overlap between them.
    /// </summary>
    private bool SetsOverlap(double min1, double max1, double min2, double max2)
    {
      bool res = (max1 > min2) && (max2 > min1);
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
}
