using Godot;
using System.Collections.Generic;
public partial struct Boid
{
    public  Vector3i velocity;
    private Vector3i acceleration;
    public Vector3i Position;
	private BoidSettings settings;
    public Boid(Vector3 Position,BoidSettings settings)
    {
		this.Position = (Vector3i)(Position) * QuadTreeConstants.WORLD_TO_QUAD_SCALE;
		this.velocity = (Vector3i)(RandomDirection() * settings.Maxspeed);
		this.settings = settings;
		this.acceleration = new(0,0,0);
    }
    Vector3 RandomDirection()
    {
        float angle = (float)GD.RandRange(0, Mathf.Tau);
        return new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)).Normalized();
    }

    public void UpdateMovement()
    {
        velocity += acceleration;
        velocity = velocity.LimitLength(settings.Maxspeed);
        Position += velocity;
		GD.Print("Boid Position " + Position);
    }

    public void Flock(QuadTree quadTree)
    {
        var nearby = GetNearbyBoids(quadTree);
        acceleration += Alignment(nearby) * settings.Alignmentweight;
        acceleration += Cohesion(nearby) * settings.Cohesionweight;
        acceleration += Separation(nearby) * settings.Separationweight;
    }

    public List<Boid> GetNearbyBoids(QuadTree quadTree)
    {
        var nearby = new List<Boid>();

        List<Boid> boids = quadTree.QueryRadius(this.Position, (int)(settings.Searchradius * QuadTreeConstants.WORLD_TO_QUAD_SCALE));

        foreach (Boid b in boids)
        {
            if (IsInFieldOfView(b.Position))
            {
                nearby.Add(b);
            }
        }
        return nearby;
    }

    public bool IsInFieldOfView(Vector3i otherPos)
    {
        if (velocity.LengthSquared() < 1) return true;
        
        Vector3i toOther = otherPos - Position;
        float currentLength = velocity.Length();
        float dotProduct = velocity.x * toOther.x + velocity.y * toOther.y + velocity.z * toOther.z;
        float otherLength = toOther.Length();
        
        if (currentLength == 0 || otherLength == 0) return true;
        
        float angle = Mathf.RadToDeg(Mathf.Acos(dotProduct / (currentLength * otherLength)));
        return angle <= settings.Fieldofview * 0.5f;
    }


    public Vector3i Alignment(List<Boid> nearby)
    {
        if (nearby.Count == 0) return new Vector3i(0, 0, 0);

        Vector3i avgVelocity = new Vector3i(0, 0, 0);
        foreach (var boid in nearby)
        {
            avgVelocity += boid.velocity;
        }
        avgVelocity = new Vector3i(
            avgVelocity.x / nearby.Count,
            avgVelocity.y / nearby.Count,
            avgVelocity.z / nearby.Count
        );

        avgVelocity = avgVelocity.LimitLength(settings.Maxspeed);
        return (avgVelocity - velocity).LimitLength(settings.Maxforce);
    }

    public Vector3i Cohesion(List<Boid> nearby)
    {
        if (nearby.Count == 0) return new Vector3i(0, 0, 0);

        Vector3i center = new Vector3i(0, 0, 0);
        foreach (var boid in nearby)
        {
            center += boid.Position;
        }
        center = new Vector3i(
            center.x / nearby.Count,
            center.y / nearby.Count,
            center.z / nearby.Count
        );

        Vector3i desired = center - Position;
        desired = desired.LimitLength(settings.Maxspeed);
        return (desired - velocity).LimitLength(settings.Maxforce);
    }

    public Vector3i Separation(List<Boid> nearby)
    {
        if (nearby.Count == 0) return new Vector3i(0, 0, 0);

        Vector3i steering = new Vector3i(0, 0, 0);
        foreach (var boid in nearby)
        {
            Vector3i diff = Position - boid.Position;
            float distance = diff.Length();
            if (distance > 0 && distance < settings.Separationradius)
            {
                steering += new Vector3i(
                    (int)(diff.x / distance),
                    (int)(diff.y / distance),
                    (int)(diff.z / distance)
                );
            }
        }

		steering = new Vector3i(
			steering.x / nearby.Count,
			steering.y / nearby.Count,
			steering.z / nearby.Count
		);

        return steering.LimitLength(settings.Maxforce);
    }
public void WrapPosition(float worldSize)
{
    float scale = QuadTreeConstants.WORLD_TO_QUAD_SCALE;
    int halfSize = (int)(worldSize * scale / 2);
    
    Position = new Vector3i(
        Wrap(Position.x, -halfSize, halfSize),
        Position.y,
        Wrap(Position.z, -halfSize, halfSize)
    );
}

private int Wrap(int value, int min, int max)
{
    int range = max - min;
    while (value < min) value += range;
    while (value > max) value -= range;
    return value;
}
}
