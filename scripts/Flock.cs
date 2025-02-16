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

    public readonly Dictionary<int, Point> points = new();
    public readonly Dictionary<int, Boid> boids = new();




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
        points[boid.ID] = point;
		boids[boid.ID] = boid;
    }

    public void UpdateQuadTree()
    {
        quadTree.Clear();
        for (int id = 0; id < boids.Count;id++)
        {
			points[id].UpdatePosition(boids[id].Position.X,boids[id].Position.Z);
			quadTree.Insert(points[id]);
        }
    }

    void UpdateFlockBehavior()
    {
        for (int id = 0; id < boids.Count;id++)
        {
            boids[id].Flock(quadTree);
        }
    }

    public Boid? GetBoid(int id)
    {
      try{
		  return boids[id];
	  }catch{
		  GD.PrintErr("Boid with id : " + id + " isn't in boidPoints");
		  return null;
	  }
    }

    public Point? GetPoint(int id)
    {
      try{
		  return points[id];
	  }catch{
		  GD.PrintErr("Boid with id : " + id + " isn't in boidPoints");
		  return null;
	  }
    }
}
