using Godot;

[GlobalClass]
public partial class BoidController : Node
{
    // Flock Parameters
    [Export] public int initialBoids = 10;
    [Export] public float worldSize = 250f;

    // Boid Movement Parameters
    [Export] public float maxSpeed = 50.0f;
    [Export] public float maxForce = 1.0f;
    [Export] public float searchRadius = 10.0f;

    // Flocking Weights
    [ExportGroup("Flocking Weights")]
    [Export] public float alignmentWeight = 0.3f;
    [Export] public float cohesionWeight = 0.3f;
    [Export] public float separationWeight = 1.3f;

    // Visual Settings
    [ExportGroup("Visual Settings")]
    [Export] public float separationRadius = 5.0f;
    [Export] public float fieldOfView = 120.0f;

    // Debug Options
    [ExportGroup("Debug")]
    [Export] public bool showQuadTree = false;
    [Export] public bool showQuadTreeGrid = false;
    [Export] public bool showQuadTreePoints = false;
    [Export] public bool showSearchRadius = false;
    [Export] public bool showSeparationRadius = false;
    [Export(PropertyHint.Range, "0,1,0.1")] public float debugLineOpacity = 0.5f;

    // Control Buttons
    [Export] string export_button_addBoid = "Add Boid";
    [Export] string export_button_resetFlock = "Reset Flock";
    [Export] string export_comment_status = "Current Status: Running";

    private Flock flock;

    public override void _Ready()
    {

        flock = GetParent<Flock>();
        if (flock == null)
        {
            GD.PrintErr("BoidController must be a child of a Flock node!");
            return;
        }

        // Apply initial settings
        ApplySettings();
    }

    public void addBoid()
    {
        if (flock != null)
        {
            // Call your existing AddBoid method
            flock.Call("AddBoid", new Boid(flock.boids.Count,worldSize,flock));
        }
    }

    public void resetFlock()
    {
        if (flock != null)
        {
            // Remove all existing boids
            foreach (Node child in flock.GetChildren())
            {
                if (child is Boid)
                {
                    child.QueueFree();
                }
            }

            // Create new flock with initial settings
            for (int i = 0; i < initialBoids; i++)
            {
                addBoid();
            }
        }
    }

    private void ApplySettings()
    {
        if (flock == null) return;
        var quadTree = flock.quadTree as QuadTree;
        if (quadTree != null)
        {
            quadTree.SetDebugVisibility(
                showQuadTree,
                debugLineOpacity,
                showQuadTreePoints
            );
        }

        // Update all boids with new settings
        foreach (Node child in flock.GetChildren())
        {
            if (child is Boid boid)
            {
                // Update boid parameters
                boid.Set("maxSpeed", maxSpeed);
                boid.Set("maxForce", maxForce);
                boid.Set("searchRadius", searchRadius);
                boid.Set("alignmentWeight", alignmentWeight);
                boid.Set("cohesionWeight", cohesionWeight);
                boid.Set("separationWeight", separationWeight);
                boid.Set("separationRadius", separationRadius);
                boid.Set("fieldOfView", fieldOfView);
            }
        }
    }

    public override void _Process(double delta)
    {
        // Update settings in real-time
        ApplySettings();

        // Update status comment with current boid count
        int boidCount = 0;
        if (flock != null)
        {
            foreach (Node child in flock.GetChildren())
            {
                if (child is Boid)
                {
                    boidCount++;
                }
            }
        }
        Set("export_comment_status", $"Current Status: Running ({boidCount} boids)");
    }
}
