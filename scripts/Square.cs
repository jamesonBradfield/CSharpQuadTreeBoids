using System;

public struct Square
{
    public int X { get; }
    public int Y { get; }
    public int HalfSize { get; }

    public Square(int x, int y, int halfSize)
    {
        X = x;
        Y = y;
        HalfSize = halfSize;
    }

    public bool Contains(Vector3i point)
    {
        int dx = point.x - X;
        int dy = point.y - Y;
        return Math.Abs(dx) <= HalfSize && Math.Abs(dy) <= HalfSize;
    }

    public bool IntersectsCircle(Vector3i center, int squaredRadius)
    {
        // Find closest point in square to the circle center
        int closestX = Math.Clamp(center.x, X - HalfSize, X + HalfSize);
        int closestY = Math.Clamp(center.y, Y - HalfSize, Y + HalfSize);

        // Calculate squared distance to circle center
        int dx = center.x - closestX;
        int dy = center.y - closestY;
        return (long)dx * dx + (long)dy * dy <= squaredRadius;
    }
}
