using Godot;
using System.Collections.Generic;

public partial class Flock : Node3D
{
    [Export] float worldSize = 250f;
    [Export] int initialBoids = 10;
    public QuadTree quadTree;

    public Flock()
    {
        quadTree = Helpers.CreateQuadTree(worldSize);
    }

    public readonly List<Boid> boids = new();
    readonly Dictionary<int, BoidPoint> boidPoints = new();



    public override void _Ready()
    {
        Helpers.CreateCameraToViewBounds(worldSize, this);
        AddChild(quadTree.GetDebugMesh());

        for (int i = 0; i < initialBoids; i++)
        {
            AddBoid(new Boid(boids.Count, worldSize, this));
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

    public Boid GetBoid(int id)
    {
      try{
		  return boidPoints[id].Boid;
	  }catch{
		  GD.PrintErr("Boid with id : " + id + " isn't in boidPoints");
		  return null;
	  }
    }

    public Point GetPoint(int id)
    {
      try{
		  return boidPoints[id];
	  }catch{
		  GD.PrintErr("Boid with id : " + id + " isn't in boidPoints");
		  return null;
	  }
    }

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
