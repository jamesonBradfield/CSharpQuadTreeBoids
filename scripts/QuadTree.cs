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
