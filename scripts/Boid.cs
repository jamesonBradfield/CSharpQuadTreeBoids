using Godot;
using System.Collections.Generic;
public partial class Boid : Node3D
{
    float alignmentWeight = 0.3f;
    float cohesionWeight = 0.3f;
    float separationWeight = 1.3f;
	float separation_radius = 5.0f;
    public float search_radius = 10.0f;
    public float max_speed = 50.0f;
    public float max_force = 1.0f;
    public int ID;
    float fieldOfView = 120.0f;
    QuadTree quadTree;
    Flock parentFlock;
    MeshInstance3D meshInstance;
    MeshInstance3D visionConeMeshInstance;
    Point point;
    Material coneMat;
    public float world_size;
    public Vector3 velocity;
    public Vector3 acceleration;

    public void Initialize(int ID, float world_size, Flock parentFlock)
    {
        this.parentFlock = parentFlock;
        this.ID = ID;
        this.world_size = world_size;
        point = new Point(ID,
            (float)GD.RandRange(-world_size, world_size),
            (float)GD.RandRange(-world_size, world_size)
        );

        // Initialize with a more similar direction
        float angle = (float)GD.RandRange(0, Mathf.Pi * 2);
        velocity = new Vector3(
            Mathf.Cos(angle),
            0,
            Mathf.Sin(angle)
        ).Normalized() * max_speed;
    }
    private bool IsInFieldOfView(Vector3 otherPos)
    {
        Vector3 directionToOther = (otherPos - Position).Normalized();
        Vector3 forward = velocity.Normalized(); // Use velocity direction instead of transform
        float angle = Mathf.RadToDeg(Mathf.Acos(forward.Dot(directionToOther)));
        return angle <= fieldOfView * 0.5f; // Half angle on each side
    }
    public override void _Ready()
    {
        // Create boid mesh
        meshInstance = Helpers.CreatePrismMeshAsChild(this, new Vector3(2.0f, 4.0f, 2.0f));
        meshInstance.Rotation = new Vector3(-90, 0, 0);
        Position = new Vector3(
            (float)GD.RandRange(-world_size, world_size),
            0,
            (float)GD.RandRange(-world_size, world_size)
        );

        parentFlock.AddToBoidID(this);
        parentFlock.AddToPointID(point);
    }

    private void WrapPosition()
    {
        var half_size = world_size / 2;

        if (ID == 0) // Log when wrapping occurs
        {
            var oldPos = Position;
            Position = new Vector3(
                Wrap(Position.X, -half_size, half_size),
                Position.Y,
                Wrap(Position.Z, -half_size, half_size)
            );
        }
        else
        {
            Position = new Vector3(
                Wrap(Position.X, -half_size, half_size),
                Position.Y,
                Wrap(Position.Z, -half_size, half_size)
            );
        }
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
            var lookAt = GlobalPosition + velocity.Normalized() * 10;
            LookAt(lookAt);
        }
        velocity += acceleration;
        velocity = velocity.Normalized() * Mathf.Min(velocity.Length(), max_speed);
        Position += velocity * (float)delta;
        point.UpdatePosition(Position.X, Position.Z);
        WrapPosition();  // Add wrapping
        acceleration = Vector3.Zero; // Reset acceleration
    }

    public void Flock(QuadTree quadTree)
    {
        Point myPoint = parentFlock.GetPointFromID(ID);
        List<Point> nearbyPoints = quadTree.QueryRadius(myPoint, (int)(search_radius));
        Vector3 alignment = Alignment(nearbyPoints) * alignmentWeight;
        Vector3 cohesion = Cohesion(nearbyPoints) * cohesionWeight;
        Vector3 separation = Separation(nearbyPoints) * separationWeight;
        acceleration += alignment;
		acceleration += cohesion;
		acceleration += separation;
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
                if (IsInFieldOfView(other.Position))
                {
                    steering += other.velocity;
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

    public Vector3 Cohesion(List<Point> nearby)
    {
        Vector3 steering = Vector3.Zero;
        int total = 0;

        foreach (Point point in nearby)
        {
            if (point.GetID() != ID)
            {
                Boid other = parentFlock.GetBoidFromID(point.GetID());
                if (IsInFieldOfView(other.Position))
                {
                    steering += other.Position;
                    total++;
                }
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
                if (IsInFieldOfView(other.Position))
                {
                    Vector3 diff = Position - other.Position;
                    float d = diff.Length();

                    if (d < search_radius && d > 0 && d > separation_radius)
                    {
                        float epsilon = 0.0001f;
                        steering += diff.Normalized() * (1.0f / (d + epsilon));
                        total++;
                    }
                }
            }
        }

        if (total > 0)
        {
            steering /= total;
            steering = steering.LimitLength(max_force);
        }
        return steering;
    }

    private void Log(string message)
    {
        DeveloperConsole.Log("[Boid] with id : " + ID + message);
    }
}
