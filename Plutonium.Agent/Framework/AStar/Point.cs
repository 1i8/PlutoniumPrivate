namespace Plutonium.Agent.Framework.AStar;

public struct Point
{
    // point X
    public int x;

    // point Y
    public int y;

    /// <summary>
    ///     Init the point with values.
    /// </summary>
    public Point(int iX, int iY)
    {
        x = iX;
        y = iY;
    }

    /// <summary>
    ///     Init the point with a single value.
    /// </summary>
    public Point(Point b)
    {
        x = b.x;
        y = b.y;
    }

    /// <summary>
    ///     Get point hash code.
    /// </summary>
    public override int GetHashCode()
    {
        return x ^ y;
    }

    /// <summary>
    ///     Compare points.
    /// </summary>
    public override bool Equals(object obj)
    {
        // check type
        if (!(obj.GetType() == typeof(Point)))
            return false;

        // check if other is null
        var p = (Point)obj;
        if (ReferenceEquals(null, p)) return false;

        // Return true if the fields match:
        return x == p.x && y == p.y;
    }

    /// <summary>
    ///     Compare points.
    /// </summary>
    public bool Equals(Point p)
    {
        // check if other is null
        if (ReferenceEquals(null, p)) return false;

        // Return true if the fields match:
        return x == p.x && y == p.y;
    }

    /// <summary>
    ///     Check if points are equal in value.
    /// </summary>
    public static bool operator ==(Point a, Point b)
    {
        // If both are null, or both are same instance, return true.
        if (ReferenceEquals(a, b)) return true;
        if (ReferenceEquals(null, a)) return false;
        if (ReferenceEquals(null, b)) return false;
        // Return true if the fields match:
        return a.x == b.x && a.y == b.y;
    }

    /// <summary>
    ///     Check if points are not equal in value.
    /// </summary>
    public static bool operator !=(Point a, Point b)
    {
        return !(a == b);
    }

    /// <summary>
    ///     Set point value.
    /// </summary>
    public Point Set(int iX, int iY)
    {
        x = iX;
        y = iY;
        return this;
    }
}