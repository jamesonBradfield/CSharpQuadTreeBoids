using System.Collections.Generic;
using Godot;
using System;
// TODO: we aren't redistributing our points on quadtree split, I believe that could lead the way to caching, and more performance gains along with others.
public partial class QuadTree : RefCounted
{
    private readonly Square boundary;
    private readonly int capacity;
    private readonly List<Point> points = new();
    private QuadTree? nw, ne, sw, se;
    private bool divided;



    public Square Boundary => boundary;
    public int Capacity => capacity;
    public List<Point> Points => points;
    public bool Divided { get => divided; set => divided = value; }
    public QuadTree? Nw { get => nw; set => nw = value; }
    public QuadTree? Ne { get => ne; set => ne = value; }
    public QuadTree? Sw { get => sw; set => sw = value; }
    public QuadTree? Se { get => se; set => se = value; }

    public QuadTree(Square boundary, int capacity)
    {
        this.boundary = boundary;
        this.capacity = capacity;
 
    }
	// TODO: experiment with quadrant checking code to "guess" or "smart-subdivide" our quads, this way we aren't wasting memory.
    public void Insert(Point point)
    {
        if (!Boundary.Contains(point)) return; // NOTE: we can look at this math function to implement a rough quadrant check.
		// NOTE: we might also want to experiment with our lightweight Square idea (we don't really need data there we can just on the fly compute our quadtree bounds with bitshifting to divide our root quadtrees size to calculate our x y and half-size)
		// NOTE: we could store all the data we need in a depth variable that will dictate how far down we need to calculate.
        if (Points.Count < Capacity)
        {
            Points.Add(point);
            return;
        }

        if (!Divided) Subdivide();

        Nw?.Insert(point);
        Ne?.Insert(point);
        Sw?.Insert(point);
        Se?.Insert(point);
    }

    private void Subdivide()
    {
		GD.Print("Subdividing");
        int half = Boundary.HalfSize / 2;
        int x = Boundary.X;
        int y = Boundary.Y;

        Nw = new QuadTree(new Square(x - half, y + half, half), Capacity);
        Ne = new QuadTree(new Square(x + half, y + half, half), Capacity);
        Sw = new QuadTree(new Square(x - half, y - half, half), Capacity);
        Se = new QuadTree(new Square(x + half, y - half, half), Capacity);

        Divided = true;
    }
    public void Clear()
    {
        Points.Clear();
        Nw = Ne = Sw = Se = null;
        Divided = false;
    }

	#region queries
    public List<Point> QueryRadius(Point center, float worldRadius)
    {
        int scaledRadius = (int)(worldRadius * QuadTreeConstants.WORLD_TO_QUAD_SCALE);
        int sqRadius = scaledRadius * scaledRadius;
        return QueryRadius(center, sqRadius, new List<Point>());
    }

    private List<Point> QueryRadius(Point center, int sqRadius, List<Point> results)
    {
        if (!QuadIntersectsCircle(center, sqRadius)) return results;

        foreach (var p in Points)
        {
            int dx = p.GetX() - center.GetX();
            int dy = p.GetY() - center.GetY();
            if (dx * dx + dy * dy <= sqRadius) results.Add(p);
        }

        if (Divided)
        {
            Nw?.QueryRadius(center, sqRadius, results);
            Ne?.QueryRadius(center, sqRadius, results);
            Sw?.QueryRadius(center, sqRadius, results);
            Se?.QueryRadius(center, sqRadius, results);
        }

        return results;
    }

    private bool QuadIntersectsCircle(Point center, int sqRadius)
    {
        int closestX = Math.Clamp(center.GetX(), Boundary.X - Boundary.HalfSize, Boundary.X + Boundary.HalfSize);
        int closestY = Math.Clamp(center.GetY(), Boundary.Y - Boundary.HalfSize, Boundary.Y + Boundary.HalfSize);

        int dx = center.GetX() - closestX;
        int dy = center.GetY() - closestY;
        return (long)dx * dx + (long)dy * dy <= sqRadius;
    }
	#endregion
}
