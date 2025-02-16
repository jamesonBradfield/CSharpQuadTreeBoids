using Godot;
using System.Collections.Generic;

public partial class Flock : Node3D
{
	[ExportGroup("Boid Values")]
	[Export]
	public BoidSettings settings;
	[ExportGroup("Flock Settings")]
    [Export] public float worldSize = 250f;
    [Export] public int initialBoids = 10;
	public bool testing = false;
    private QuadTree quadTree;
	private HashSet<Boid> boids = new HashSet<Boid>();

  //   public Flock(float worldSize,int initialBoids,bool testing)
  //   {
		// this.worldSize = worldSize;
		// this.initialBoids = initialBoids;
		// this.testing = testing;
  //   }

	// NOTE: OLD BOID NODE Functions!
    // public override void _Ready()
    // {
    //     Helpers.CreatePrismMeshAsChild(this, new Vector3(2.0f, 4.0f, 2.0f));
    //     Position = new Vector3(
    //         (float)GD.RandRange(-worldSize, worldSize),
    //         0,
    //         (float)GD.RandRange(-worldSize, worldSize)
    //     );
    // }
    //
    // public override void _PhysicsProcess(double delta)
    // {
    //     UpdateRotation();
    //     UpdateMovement(delta);
    //     WrapPosition();
    //     acceleration = Vector3.Zero;
    // }
    // void UpdateRotation()
    // {
    //     if (velocity.LengthSquared() > 0.01f)
    //     {
    //         LookAt(GlobalPosition + velocity.Normalized(), Vector3.Up);
    //     }
    // }
    // void WrapPosition()
    // {
    //     var halfSize = worldSize / 2;
    //     Position = new Vector3(
    //         Wrap(Position.X, -halfSize, halfSize),
    //         Position.Y,
    //         Wrap(Position.Z, -halfSize, halfSize)
    //     );
    // }
    //
    // float Wrap(float value, float min, float max)
    // {
    //     float range = max - min;
    //     while (value < min) value += range;
    //     while (value > max) value -= range;
    //     return value;
    // }
  	//   public Boid(int id, float worldSize)
  	//   {
		// this.velocity = RandomDirection() * parentFlock.maxSpeed;
  	//   }
    // Vector3 RandomDirection()
    // {
    //     float angle = (float)GD.RandRange(0, Mathf.Tau);
    //     return new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)).Normalized();
    // }

    public override void _Ready()
    {
        quadTree = Helpers.CreateQuadTree(worldSize);
		if(!testing){
			Helpers.CreateCameraToViewBounds(worldSize, this);
		}
		  for (int i = 0; i < initialBoids; i++)
		{
			Vector3 randomPos = new Vector3(
				(float)GD.RandRange(-worldSize/2, worldSize/2),
				0,
				(float)GD.RandRange(-worldSize/2, worldSize/2)
			);
			boids.Add(new Boid(randomPos, settings));
		}
    }

    public override void _Process(double delta)
    {
        UpdateQuadTree();
        UpdateFlockBehavior();
    }

    public override void _PhysicsProcess(double delta)
    {
        foreach (Boid boid in boids)
        {
			// UpdateRotation();
			boid.UpdateMovement();
			boid.WrapPosition(worldSize);
		}
    }

    public void UpdateQuadTree()
    {
        quadTree.Clear();
        foreach (Boid boid in boids)
        {
			quadTree.Insert(boid);
        }
    }

    void UpdateFlockBehavior()
    {
        foreach (Boid boid in boids)
        {
			boid.Flock(quadTree);
        }
    }
}
