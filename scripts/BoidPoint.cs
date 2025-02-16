
public partial class BoidPoint : Point
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

