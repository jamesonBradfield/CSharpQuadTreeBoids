using System.Collections.Generic;
using Godot;
[GlobalClass]
public partial class QuadTree : RefCounted
{
    public Rectangle boundary;
    public int capacity;
    private List<Point> points = new List<Point>();
    public QuadTree northwest;
    public QuadTree northeast;
    public QuadTree southwest;
    public QuadTree southeast;
    public bool divided = false;

    public QuadTree(Rectangle boundary, int n)
    {
        this.boundary = boundary;
        this.capacity = n;
    }

    public void Subdivide()
    {
        int x = boundary.GetX();
        int y = boundary.GetY();
        int w = boundary.GetW();
        int h = boundary.GetH();
		Rectangle nw = new Rectangle(x-(w>>1),y+(h>>1),w>>1,h>>1);
		Rectangle ne = new Rectangle(x+ (w>>1),y+(h>>1),w>>1,h>>1);
		Rectangle sw = new Rectangle(x-(w>>1),y-(h>>1),w>>1,h>>1);
		Rectangle se = new Rectangle(x+(w>>1),y-(h>>1),w>>1,h>>1);

        this.northwest = new QuadTree(nw, capacity);
        this.northeast = new QuadTree(ne, capacity);
        this.southwest = new QuadTree(sw, capacity);
        this.southeast = new QuadTree(se, capacity);
        divided = true;
    }
	private bool QuadIntersectsCircle(Rectangle quad, Point center, int squaredRadius) {
		// Find closest point on rectangle to circle center
		int closestX = Mathf.Clamp(center.GetX(), quad.GetX() - quad.GetW(), quad.GetX() + quad.GetW());
		int closestZ = Mathf.Clamp(center.GetY(), quad.GetY() - quad.GetH(), quad.GetY() + quad.GetH());
		
		// Calculate squared distance between closest point and circle center
		int dx = center.GetX() - closestX;
		int dy = center.GetY() - closestZ;
		int squaredDistance = dx * dx + dy * dy;
		
		// If squared distance is less than squared radius, they intersect
		return squaredDistance <= squaredRadius;
	}
	public void Clear() {
		points.Clear();
		if (divided) {
			northwest = null;
			northeast = null;
			southwest = null;
			southeast = null;
			divided = false;
		}
	}
	public List<Point> QueryRadius(Point center, int radius) {
		// Convert radius to our integer space
		int searchRadius = radius * 1000;
		// Use squared distance to avoid sqrt
		int squaredRadius = searchRadius * searchRadius;
		
		return QueryRadius(center, squaredRadius, new List<Point>());
	}

	private List<Point> QueryRadius(Point center, int squaredRadius, List<Point> found) {
		// First check if this quad is too far from search circle
		// We can still use rectangle for quick broad-phase rejection
		if (!QuadIntersectsCircle(boundary, center, squaredRadius)) {
			return found;
		}

		// Check points in this quad
		foreach(var p in points) {
			int dx = p.GetX() - center.GetX();
			int dy = p.GetY() - center.GetY();
			if (dx * dx + dy * dy <= squaredRadius) {
				found.Add(p);
			}
		}

		if (divided) {
			northwest.QueryRadius(center, squaredRadius, found);
			northeast.QueryRadius(center, squaredRadius, found);
			southwest.QueryRadius(center, squaredRadius, found);
			southeast.QueryRadius(center, squaredRadius, found);
		}

		return found;
	}
	public List<Point> Query(Rectangle range){
		return Query(range, new List<Point>());
	}

	private List<Point> Query(Rectangle range, List<Point> found){
		if (!this.boundary.intersects(range)){
			return found;
		}else{
			foreach( var p in this.points){
				if (range.contains(p)){
					found.Add(p);
				}
			}
			if (divided){
				northwest.Query(range,found);
				northeast.Query(range,found);
				southwest.Query(range,found);
				southeast.Query(range,found);
			}
			return found;
		}

	}

    public bool Insert(Point point)
    {
        if (!boundary.contains(point))
        {
            return false;
        }
        if (points.Count < capacity)
        {
            points.Add(point);
            return true;
        }
        if (!divided)
        {
            Subdivide();
        }
        if (northeast.Insert(point)) 
        {
            return true;
        }
        if (northwest.Insert(point)) 
        {
            return true;
        }
        if (southeast.Insert(point)) 
        {
            return true;
        }
        if (southwest.Insert(point)) 
        {
            return true;
        }

        return false;
    }

    public Rectangle GetBoundary() => boundary;
    public List<Point> GetPoints() => points;
    public bool IsDivided() => divided;
    public QuadTree GetNorthwest() => northwest;
    public QuadTree GetNortheast() => northeast;
    public QuadTree GetSouthwest() => southwest;
    public QuadTree GetSoutheast() => southeast;
}
