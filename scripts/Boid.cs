using Godot;
using System.Collections.Generic;
[GlobalClass]
public partial class Boid : Node3D
{
    // Weights and parameters
    float alignmentWeight = 0.3f;
    float cohesionWeight = 0.3f;
    float separationWeight = 1.3f;
    float separationRadius = 5.0f;
    public float searchRadius = 10.0f;
    public float maxSpeed = 50.0f;
    public float maxForce = 1.0f;
    float fieldOfView = 120.0f;

    // State
    public Vector3 velocity;
    Vector3 acceleration;
    public int ID;
    public float worldSize;

    // Dependencies
    Flock parentFlock;

    public Boid(int id, float worldSize,Flock parentFlock)
    {
        this.parentFlock = parentFlock;
		this.ID = id;
		this.worldSize = worldSize;
		this.velocity = RandomDirection() * maxSpeed;
    }

    Vector3 RandomDirection()
    {
        float angle = (float)GD.RandRange(0, Mathf.Tau);
        return new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)).Normalized();
    }

    public override void _Ready()
    {
        Helpers.CreatePrismMeshAsChild(this, new Vector3(2.0f, 4.0f, 2.0f));
        Position = new Vector3(
            (float)GD.RandRange(-worldSize, worldSize),
            0,
            (float)GD.RandRange(-worldSize, worldSize)
        );
    }

    public override void _PhysicsProcess(double delta)
    {
        UpdateRotation();
        UpdateMovement(delta);
        WrapPosition();
        acceleration = Vector3.Zero;
    }

    void UpdateRotation()
    {
        if (velocity.LengthSquared() > 0.01f)
        {
            LookAt(GlobalPosition + velocity.Normalized(), Vector3.Up);
        }
    }

    void UpdateMovement(double delta)
    {
        velocity += acceleration;
        velocity = velocity.LimitLength(maxSpeed);
        Position += velocity * (float)delta;
    }

    void WrapPosition()
    {
        var halfSize = worldSize / 2;
        Position = new Vector3(
            Wrap(Position.X, -halfSize, halfSize),
            Position.Y,
            Wrap(Position.Z, -halfSize, halfSize)
        );
    }

    float Wrap(float value, float min, float max)
    {
        float range = max - min;
        while (value < min) value += range;
        while (value > max) value -= range;
        return value;
    }

    public void Flock(QuadTree quadTree)
    {
        var nearby = GetNearbyBoids(quadTree);
        acceleration += Alignment(nearby) * alignmentWeight;
        acceleration += Cohesion(nearby) * cohesionWeight;
        acceleration += Separation(nearby) * separationWeight;
    }

    List<Boid> GetNearbyBoids(QuadTree quadTree)
    {
        var nearby = new List<Boid>();
        var myPoint = parentFlock.GetPoint(ID);
        if (myPoint == null) return nearby;

        var points = quadTree.QueryRadius(myPoint, searchRadius); // Pass float directly

        foreach (var p in points)
        {
            if (p.GetID() == ID) continue;

            var boid = parentFlock.GetBoid(p.GetID());
            if (boid != null && IsInFieldOfView(boid.Position))
            {
                nearby.Add(boid);
            }
        }
        return nearby;
    }

    bool IsInFieldOfView(Vector3 otherPos)
    {
        Vector3 toOther = (otherPos - Position).Normalized();
        float angle = Mathf.RadToDeg(Mathf.Acos(velocity.Normalized().Dot(toOther)));
        return angle <= fieldOfView * 0.5f;
    }

    Vector3 Alignment(List<Boid> nearby)
    {
        if (nearby.Count == 0) return Vector3.Zero;

        Vector3 avgVelocity = Vector3.Zero;
        foreach (var boid in nearby) avgVelocity += boid.velocity;

        avgVelocity = avgVelocity.Normalized() * maxSpeed;
        return (avgVelocity - velocity).LimitLength(maxForce);
    }

    Vector3 Cohesion(List<Boid> nearby)
    {
        if (nearby.Count == 0) return Vector3.Zero;

        Vector3 avgPosition = Vector3.Zero;
        foreach (var boid in nearby) avgPosition += boid.Position;

        return ((avgPosition / nearby.Count - Position).Normalized() * maxSpeed - velocity)
            .LimitLength(maxForce);
    }

    Vector3 Separation(List<Boid> nearby)
    {
        Vector3 steering = Vector3.Zero;

        foreach (var boid in nearby)
        {
            Vector3 diff = Position - boid.Position;
            float distance = diff.Length();

            if (distance > 0 && distance < separationRadius)
            {
                steering += diff.Normalized() / Mathf.Max(distance, 0.0001f);
            }
        }

        return nearby.Count > 0
            ? (steering / nearby.Count).LimitLength(maxForce)
            : Vector3.Zero;
    }
}
