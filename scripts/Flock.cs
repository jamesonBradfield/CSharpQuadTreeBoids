using Godot;
using System.Collections.Generic;

public partial class Flock : Node3D
{
    [Export] public float worldSize = 250f;
    [Export] public int initialBoids = 10;
	public bool testing = false;
    public QuadTree quadTree;

    public Flock(float worldSize,int initialBoids,bool testing)
    {
		this.worldSize = worldSize;
		this.initialBoids = initialBoids;
		this.testing = testing;
        quadTree = Helpers.CreateQuadTree(worldSize);
    }

    public readonly List<Boid> boids = new();
    public readonly Dictionary<int, BoidPoint> boidPoints = new();



    public override void _Ready()
    {
		if(!testing){
			Helpers.CreateCameraToViewBounds(worldSize, this);
		}
        // AddChild(quadTree.GetDebugMesh());

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

    public void AddBoid(Boid boid)
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

    public Boid? GetBoid(int id)
    {
      try{
		  return boidPoints[id].Boid;
	  }catch{
		  GD.PrintErr("Boid with id : " + id + " isn't in boidPoints");
		  return null;
	  }
    }

    public Point? GetPoint(int id)
    {
      try{
		  return boidPoints[id];
	  }catch{
		  GD.PrintErr("Boid with id : " + id + " isn't in boidPoints");
		  return null;
	  }
    }
}
