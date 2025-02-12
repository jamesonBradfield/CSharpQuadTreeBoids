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

    // Debug visualization
    private MeshInstance3D debugMesh;
    private bool showDebug = false;
    private float debugOpacity = 0.5f;
    private bool showPoints = false;
	public bool insertFinished = false;

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
         debugMesh = new MeshInstance3D
        {
            Name = "QuadTreeDebug",
            CastShadow = GeometryInstance3D.ShadowCastingSetting.Off,
            MaterialOverride = new StandardMaterial3D
            {
                ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
                VertexColorUseAsAlbedo = true,
                Transparency = BaseMaterial3D.TransparencyEnum.Alpha
            }
        };
    }



    public void SetDebugVisibility(bool show, float opacity = 0.5f, bool showPoints = false)
    {
        showDebug = show;
        debugOpacity = opacity;
        this.showPoints = showPoints;
		UpdateDebugVisualization();
		if(divided){
			nw?.SetDebugVisibility(show,opacity,showPoints);
			ne?.SetDebugVisibility(show,opacity,showPoints);
			sw?.SetDebugVisibility(show,opacity,showPoints);
			se?.SetDebugVisibility(show,opacity,showPoints);
		}       
    }

    public MeshInstance3D GetDebugMesh()
    {
        return debugMesh;
    }

    public void UpdateDebugVisualization()
    {
        if (!showDebug)
        {
            debugMesh.Mesh = null;
            return;
        }

        var immediateMesh = new ImmediateMesh();
        debugMesh.Mesh = immediateMesh;

        // Start a single surface for both grid and points
        immediateMesh.SurfaceBegin(Mesh.PrimitiveType.Lines, null);

        // Draw grid and points in one pass
        DrawQuadTreeRecursive(immediateMesh);

        immediateMesh.SurfaceEnd();
    }

    private void DrawQuadTreeRecursive(ImmediateMesh mesh)
    {
        float scale = 1.0f / QuadTreeConstants.WORLD_TO_QUAD_SCALE;
        float x = boundary.X * scale;
        float z = boundary.Y * scale;
        float size = boundary.HalfSize * scale * 2;

        // Draw grid with green color
        var gridColor = new Color(0, 1, 0, debugOpacity);
        Vector3[] corners = {
            new(x - size/2, 0, z - size/2),
            new(x + size/2, 0, z - size/2),
            new(x + size/2, 0, z + size/2),
            new(x - size/2, 0, z + size/2)
        };

        for (int i = 0; i < 4; i++)
        {
            mesh.SurfaceSetColor(gridColor);
            mesh.SurfaceAddVertex(corners[i]);
            mesh.SurfaceSetColor(gridColor);
            mesh.SurfaceAddVertex(corners[(i + 1) % 4]);
        }

        // Draw points with red color if enabled
        if (showPoints)
        {
            var pointColor = new Color(1, 0, 0, debugOpacity);
            foreach (var point in points)
            {
                var worldPos = new Vector3(
                    point.GetX() * scale,
                    0,
                    point.GetY() * scale
                );
                // Draw a small cross for each point using lines
                var pointSize = size * 0.02f; // Adjust size as needed

                // Vertical line of the cross
                mesh.SurfaceSetColor(pointColor);
                mesh.SurfaceAddVertex(worldPos + new Vector3(0, 0, -pointSize));
                mesh.SurfaceSetColor(pointColor);
                mesh.SurfaceAddVertex(worldPos + new Vector3(0, 0, pointSize));

                // Horizontal line of the cross
                mesh.SurfaceSetColor(pointColor);
                mesh.SurfaceAddVertex(worldPos + new Vector3(-pointSize, 0, 0));
                mesh.SurfaceSetColor(pointColor);
                mesh.SurfaceAddVertex(worldPos + new Vector3(pointSize, 0, 0));
            }
        }

        // Recursively draw subdivisions
        if (Divided)
        {
            Nw?.DrawQuadTreeRecursive(mesh);
            Ne?.DrawQuadTreeRecursive(mesh);
            Sw?.DrawQuadTreeRecursive(mesh);
            Se?.DrawQuadTreeRecursive(mesh);
        }
    }

    public void Insert(Point point)
    {
		insertFinished = false;
		var insert_start_time = Time.GetTicksMsec();
        if (!Boundary.Contains(point)) return;

        if (Points.Count < Capacity)
        {
            Points.Add(point);
			insertFinished = true;
            return;
        }

        if (!Divided) Subdivide();

        Nw?.Insert(point);
        Ne?.Insert(point);
        Sw?.Insert(point);
        Se?.Insert(point);
		if(Nw.insertFinished && Ne.insertFinished && Sw.insertFinished && Se.insertFinished){
			insertFinished = true;
		}
        UpdateDebugVisualization();
    }

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

    private void Subdivide()
    {
        int half = Boundary.HalfSize / 2;
        int x = Boundary.X;
        int y = Boundary.Y;

        Nw = new QuadTree(new Square(x - half, y + half, half), Capacity);
        Ne = new QuadTree(new Square(x + half, y + half, half), Capacity);
        Sw = new QuadTree(new Square(x - half, y - half, half), Capacity);
        Se = new QuadTree(new Square(x + half, y - half, half), Capacity);

        Divided = true;
        UpdateDebugVisualization();
    }

    private bool QuadIntersectsCircle(Point center, int sqRadius)
    {
        int closestX = Math.Clamp(center.GetX(), Boundary.X - Boundary.HalfSize, Boundary.X + Boundary.HalfSize);
        int closestY = Math.Clamp(center.GetY(), Boundary.Y - Boundary.HalfSize, Boundary.Y + Boundary.HalfSize);

        int dx = center.GetX() - closestX;
        int dy = center.GetY() - closestY;
        return (long)dx * dx + (long)dy * dy <= sqRadius;
    }

    public void Clear()
    {
        Points.Clear();
        Nw = Ne = Sw = Se = null;
        Divided = false;
        UpdateDebugVisualization();
    }
}
