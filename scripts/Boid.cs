using Godot;
using System.Collections.Generic;
public partial class Boid : Node3D
{
    public float search_radius = 5.0f;
    public float max_speed = 20.0f;
    public float max_force = 1.0f; // Maximum steering force
    public int ID;
    public Vector3 velocity;
    public Vector3 acceleration;
    private QuadTree quadTree;
    private Flock parentFlock;
    private MeshInstance3D meshInstance;
	private Point point;
    public Vector2 world_size;

    public void Initialize(int ID,Vector2 world_size,Flock parentFlock)
    {
		this.parentFlock = parentFlock;
        this.ID = ID;
		this.world_size = world_size;
		point = new Point(ID,
			(int)GD.RandRange(-world_size.X, world_size.X) * 1000,
			(int)GD.RandRange(-world_size.Y, world_size.Y)
				);
        // Initialize random velocity
        velocity = new Vector3(GD.Randf() * 2 - 1, 0, GD.Randf() * 2 - 1).Normalized() * max_speed;
    }

    public override void _Ready()
    {
        meshInstance = Helpers.CreatePrismMeshAsChild(this, new Vector3(2.0f, 4.0f, 2.0f));
        // Point mesh in direction of movement
        meshInstance.RotationDegrees = new Vector3(-90, 0, 0); // Rotate to point forward
		Position = new Vector3(
			(float)GD.RandRange(-world_size.X, world_size.X),
			0,
			(float)GD.RandRange(-world_size.Y, world_size.Y)
		);
		parentFlock.AddToBoidID(this);
		parentFlock.AddToPointID(point);
    }

    private void WrapPosition()
    {
        var half_size = world_size / 2;

        // Wrap X coordinate
        Position = new Vector3(
            Wrap(Position.X, -half_size.X, half_size.X),
            Position.Y,
            Wrap(Position.Z, -half_size.Y, half_size.Y)
        );
    }

    private float Wrap(float value, float min_val, float max_val)
    {
        float range = max_val - min_val;
        if (value < min_val)
        {
            value += range;
        }
        else if (value > max_val)
        {
            value -= range;
        }
        return value;
    }

    public override void _Process(double delta)
    {
        if (velocity != Vector3.Zero)
        {
            var lookAt = GlobalPosition + velocity.Normalized();
            LookAt(lookAt);
            RotateObjectLocal(Vector3.Right, Mathf.Pi / 2); // Adjust to point forward
        }
        velocity += acceleration;
        velocity = velocity.Normalized() * Mathf.Min(velocity.Length(), max_speed);
        Position += velocity * (float)delta;
        WrapPosition();  // Add wrapping
        acceleration = Vector3.Zero; // Reset acceleration
    }

    public void Flock(QuadTree quadTree)
    {

        Point myPoint = parentFlock.GetPointFromID(ID);
        List<Point> nearbyPoints = quadTree.QueryRadius(myPoint, (int)search_radius);
        Vector3 alignment = Alignment(nearbyPoints);
        Vector3 cohesion = Cohesion(nearbyPoints);
        Vector3 separation = Separation(nearbyPoints);
        acceleration += alignment + cohesion + separation;
    }

    public Vector3 Alignment(List<Point> nearby)
    {
        Vector3 steering = Vector3.Zero;
        int total = 0;

        foreach (Point point in nearby)
        {
            if (point.GetID() != ID)
            { // Don't include self
                Boid other = parentFlock.GetBoidFromID(point.GetID());
                steering += other.velocity;
                total++;
            }
        }

        if (total > 0)
        {
            steering /= total;
            steering = steering.Normalized() * max_speed;
            steering -= velocity;
            steering = steering.LimitLength(max_force);
        }
        return steering;
    }

    public Vector3 Cohesion(List<Point> nearby)
    {
        Vector3 steering = Vector3.Zero;
        int total = 0;

        foreach (Point point in nearby)
        {
            if (point.GetID() != ID)
            {
                Boid other = parentFlock.GetBoidFromID(point.GetID());
                steering += other.Position;
                total++;
            }
        }

        if (total > 0)
        {
            steering /= total;
            steering = (steering - Position).Normalized() * max_speed;
            steering -= velocity;
            steering = steering.LimitLength(max_force);
        }
        return steering;
    }

    public Vector3 Separation(List<Point> nearby)
    {
        Vector3 steering = Vector3.Zero;
        int total = 0;

        foreach (Point point in nearby)
        {
            if (point.GetID() != ID)
            {
                Boid other = parentFlock.GetBoidFromID(point.GetID());
                Vector3 diff = Position - other.Position;
                float d = diff.Length();
                if (d < search_radius && d > 0)
                {
                    steering += diff.Normalized() / d; // Weight by distance
                    total++;
                }
            }
        }

        if (total > 0)
        {
            steering /= total;
            steering = steering.Normalized() * max_speed;
            steering -= velocity;
            steering = steering.LimitLength(max_force);
        }
        return steering;
    }

	private void Log(string message){
		DeveloperConsole.Log("[Boid] with id : " + ID + message);
	}
}
