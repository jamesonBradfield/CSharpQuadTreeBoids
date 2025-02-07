using System.Collections.Generic;
using Godot;
// NOTE: I can't help but think storing the x,y of our root node and calculating every other node on the fly might be more performant, IE doing away with all data structures and storing one x and y and maybe a isRoot bool and a depth int with maybe an int for which quadrant of its parents a node is in.
[GlobalClass]
public partial class QuadTree : RefCounted
{
    public Square boundary;
    public int capacity;
    private List<Point> points = new List<Point>();
    public QuadTree northwest;
    public QuadTree northeast;
    public QuadTree southwest;
    public QuadTree southeast;
    public bool divided = false;

    public QuadTree(Square boundary, int n)
    {
        this.boundary = boundary;
        this.capacity = n;
    }

    public void Subdivide()
    {
        int x = boundary.GetX();
        int y = boundary.GetY();
        int halfSize = boundary.GetS() >> 1;

        // Since we're working with squares, we can simplify these calculations
        northwest = new QuadTree(new Square(x - halfSize, y + halfSize, halfSize), capacity);
        northeast = new QuadTree(new Square(x + halfSize, y + halfSize, halfSize), capacity);
        southwest = new QuadTree(new Square(x - halfSize, y - halfSize, halfSize), capacity);
        southeast = new QuadTree(new Square(x + halfSize, y - halfSize, halfSize), capacity);

        divided = true;
    }

    private bool QuadIntersectsCircle(Square quad, Point center, int squaredRadius)
    {
        int s = quad.GetS();
        int qx = quad.GetX();
        int qy = quad.GetY();

        // For squares, we can optimize the clamping
        int closestX = Mathf.Clamp(center.GetX(), qx - s, qx + s);
        int closestZ = Mathf.Clamp(center.GetY(), qy - s, qy + s);

        // Use squared distance comparison
        int dx = center.GetX() - closestX;
        int dy = center.GetY() - closestZ;
        return (dx * dx + dy * dy) <= squaredRadius;
    }

    public List<Point> QueryRadius(Point center, int radius)
    {
        int searchRadius = radius * 1000;
        return QueryRadius(center, searchRadius * searchRadius, new List<Point>());
    }

    private List<Point> QueryRadius(Point center, int squaredRadius, List<Point> found)
    {
        if (!QuadIntersectsCircle(boundary, center, squaredRadius))
        {
            return found;
        }

        foreach (var p in points)
        {
            int dx = p.GetX() - center.GetX();
            int dy = p.GetY() - center.GetY();
            if ((dx * dx + dy * dy) <= squaredRadius)
            {
                found.Add(p);
            }
        }

        if (divided)
        {
            northwest.QueryRadius(center, squaredRadius, found);
            northeast.QueryRadius(center, squaredRadius, found);
            southwest.QueryRadius(center, squaredRadius, found);
            southeast.QueryRadius(center, squaredRadius, found);
        }

        return found;
    }

    public bool Insert(Point point)
    {
        // Check if point is within boundary
        if (!boundary.contains(point))
        {
            return false;
        }

        // If there's room here and we haven't subdivided, add the point
        if (points.Count < capacity)
        {
            points.Add(point);
            return true;
        }

        // Subdivide if we haven't yet
        if (!divided)
        {
            Subdivide();
        }

        // Try to insert into appropriate quadrant
        return northeast.Insert(point) ||
               northwest.Insert(point) ||
               southeast.Insert(point) ||
               southwest.Insert(point);
    }

    public Square GetBoundary() => boundary;
    public List<Point> GetPoints() => points;
    public bool IsDivided() => divided;
    public QuadTree GetNorthwest() => northwest;
    public QuadTree GetNortheast() => northeast;
    public QuadTree GetSouthwest() => southwest;
    public QuadTree GetSoutheast() => southeast;
}
