using Godot;
using System.Collections.Generic;
public partial class Boid : Node3D
{
    float alignmentWeight = 0.75f;
    float cohesionWeight = 0.4f;
    float separationWeight = 1.4f;
    public float search_radius = 25.0f;
    public float max_speed = 20.0f;
    public float max_force = .5f;
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

    public void Initialize(int ID, float world_size, Flock parentFlock, Material coneMat)
    {
        this.parentFlock = parentFlock;
        this.ID = ID;
        this.world_size = world_size;
        point = new Point(ID,
            (int)GD.RandRange(-world_size, world_size) * 1000,
            (int)GD.RandRange(-world_size, world_size)
                );
        velocity = new Vector3(GD.Randf() * 2 - 1, 0, GD.Randf() * 2 - 1).Normalized() * max_speed;
    }
    private bool IsInFieldOfView(Vector3 otherPos)
    {
        Vector3 directionToOther = (otherPos - Position).Normalized();
        Vector3 forward = -Transform.Basis.Z; // Assuming forward is -Z in Godot
        float angle = Mathf.RadToDeg(Mathf.Acos(forward.Dot(directionToOther)));
        return angle <= fieldOfView * 0.5f; // Half angle on each side
    }
    public override void _Ready()
    {
		float topRadius =  Mathf.DegToRad(Mathf.Tan(fieldOfView * 0.5f)) * search_radius;
        meshInstance = Helpers.CreatePrismMeshAsChild(this, new Vector3(2.0f, 4.0f, 2.0f));
        visionConeMeshInstance = Helpers.CreateConeMeshAsChild(this, topRadius, 0.0f, search_radius);
        visionConeMeshInstance.MaterialOverride = coneMat;
        // Point mesh in direction of movement
        meshInstance.RotationDegrees = new Vector3(0, 0, -180); // Rotate to point forward
        visionConeMeshInstance.RotationDegrees = new Vector3(0, 0, -180); // Rotate to point forward
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

            if (oldPos != Position)
            {
                Log($"Wrapped position from ({oldPos.X:F1}, {oldPos.Z:F1}) to ({Position.X:F1}, {Position.Z:F1})");
            }
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
        List<Point> nearbyPoints = quadTree.QueryRadius(myPoint, (int)(search_radius));
        Vector3 alignment = Alignment(nearbyPoints) * alignmentWeight;
        Vector3 cohesion = Cohesion(nearbyPoints) * cohesionWeight;
        Vector3 separation = Separation(nearbyPoints) * separationWeight;

        if (ID == 0)
        {
            Log($"Raw forces:");
            Log($"  Alignment ({alignmentWeight:F1}): {alignment.Length():F2}");
            Log($"  Cohesion ({cohesionWeight:F1}): {cohesion.Length():F2}");
            Log($"  Separation ({separationWeight:F1}): {separation.Length():F2}");
        }
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

                    // Use unscaled search radius for world-space comparison
                    if (d < search_radius && d > 0)
                    {
                        float epsilon = 0.0001f;
                        // Stronger inverse square falloff
                        steering += diff.Normalized() * (search_radius / (d * d + epsilon));
                        total++;
                    }
                }
            }
        }

        if (total > 0 && ID == 0)
        {
            Log($"Separation found {total} neighbors within range");
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

    private void Log(string message)
    {
        DeveloperConsole.Log("[Boid] with id : " + ID + message);
    }
}
