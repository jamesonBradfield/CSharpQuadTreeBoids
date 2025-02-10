using Godot;
using System.Collections.Generic;

public partial class Flock : Node3D
{
    [Export] float worldSize = 250f;
    [Export] int initialBoids = 10;

    QuadTree quadTree;
    readonly List<Boid> boids = new();
    readonly Dictionary<int, BoidPoint> boidPoints = new();

    public override void _Ready()
    {
        Helpers.CreateCameraToViewBounds(worldSize, this);
        quadTree = Helpers.CreateQuadTree(worldSize);

        for (int i = 0; i < initialBoids; i++)
        {
            AddBoid(new Boid());
        }
    }

    public override void _Process(double delta)
    {
        UpdateQuadTree();
        UpdateFlockBehavior();
    }

    void AddBoid(Boid boid)
    {
        AddChild(boid);
        boid.Initialize(boids.Count, worldSize, this);
        var point = new BoidPoint(boid);
        boidPoints[boid.ID] = point;
        boids.Add(boid);
    }

    void UpdateQuadTree()
    {
        quadTree.Clear();
        foreach (var boid in boids)
        {
            if (boidPoints.TryGetValue(boid.ID, out var point))
            {
                point.UpdateFromBoid(boid);
                quadTree.Insert(point);
            }
        }
    }

    void UpdateFlockBehavior()
    {
        foreach (var boid in boids)
        {
            boid.Flock(quadTree);
        }
    }

    public Boid GetBoid(int id) => boidPoints.TryGetValue(id, out var p) ? p.Boid : null;
    public Point GetPoint(int id) => boidPoints.TryGetValue(id, out var p) ? p : null;

    class BoidPoint : Point
    {
        public readonly Boid Boid;

        public BoidPoint(Boid boid) : base(boid.ID, boid.Position.X, boid.Position.Z)
        {
            Boid = boid;
        }

        public void UpdateFromBoid(Boid boid)
        {
            // Explicit scaling call for clarity
            UpdatePosition(boid.Position.X, boid.Position.Z);
        }
    }
}
