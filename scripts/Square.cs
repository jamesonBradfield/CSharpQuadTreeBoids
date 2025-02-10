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

    public bool Contains(Point point)
    {
        int dx = point.GetX() - X;
        int dy = point.GetY() - Y;
        return Math.Abs(dx) <= HalfSize && Math.Abs(dy) <= HalfSize;
    }

    public bool IntersectsCircle(Point center, int squaredRadius)
    {
        // Find closest point in square to the circle center
        int closestX = Math.Clamp(center.GetX(), X - HalfSize, X + HalfSize);
        int closestY = Math.Clamp(center.GetY(), Y - HalfSize, Y + HalfSize);

        // Calculate squared distance to circle center
        int dx = center.GetX() - closestX;
        int dy = center.GetY() - closestY;
        return (long)dx * dx + (long)dy * dy <= squaredRadius;
    }
}
