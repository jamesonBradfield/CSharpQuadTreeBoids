using Godot;
using System.Collections.Generic;
public partial class Flock : Node3D
{
    #region profiling_data
    const string BOID_PROFILE_NAME = "boid_performance";
    bool isProfilingEnabled = true;
    double totalProcessingTime = 0;
    int processingFrames = 0;
    #endregion
    #region flock_data
    [Export] Material coneMat;
    float world_size = 500.0f;
    int num_boids = 200;
    QuadTree quad_tree;
    public List<Boid> flock = new List<Boid>();
    Dictionary<int, Point> idToPoint = new Dictionary<int, Point>();
    Dictionary<int, Boid> idToBoid = new Dictionary<int, Boid>();
    #endregion

    public override void _Ready()
    {
        Helpers.CreateCameraToViewBounds(world_size, this);
        quad_tree = Helpers.CreateQuadTree(world_size);
        idToPoint.Clear();

        // Initialize the boid performance profile
        MemoryProfiler.CreateProfile(BOID_PROFILE_NAME);

        for (int i = 0; i < num_boids; i++)
        {
            Boid boid = new Boid();
            boid.Initialize(i, world_size, this, coneMat);
            AddChild(boid);
            flock.Add(boid);
        }
        StartProfiling();
    }

    public override void _Process(double delta)
    {
        if (isProfilingEnabled)
        {
            var startTime = Time.GetTicksUsec();
            quad_tree.Clear();
            ProcessBoids(delta);
            var endTime = Time.GetTicksUsec();
            double frameProcessingTime = (endTime - startTime) / 1000.0;
            totalProcessingTime += frameProcessingTime;
            processingFrames++;
            MemoryProfiler.AddMetric(BOID_PROFILE_NAME, "avg_frame_time", totalProcessingTime / processingFrames);
            MemoryProfiler.AddMetric(BOID_PROFILE_NAME, "last_frame_time", frameProcessingTime);
            MemoryProfiler.AddMetric(BOID_PROFILE_NAME, "active_boids", flock.Count);
            MemoryProfiler.AddMetric(BOID_PROFILE_NAME, "points_in_quadtree", idToPoint.Count);
        }
        else
        {
            ProcessBoids(delta);
        }
    }

    private void ProcessBoids(double delta)
    {
        for (int i = 0; i < flock.Count; i++)
        {
            Boid boid = flock[i];
            Point point = idToPoint[boid.ID];
            var pos = boid.Position;
            point.UpdatePosition(
                (int)(pos.X * 1000),
                (int)(pos.Z * 1000)
            );
            quad_tree.Insert(point);
        }

        foreach (Boid boid in flock)
        {
            boid.Flock(quad_tree);
        }
    }
    public void StartProfiling()
    {
        isProfilingEnabled = true;
        totalProcessingTime = 0;
        processingFrames = 0;
        MemoryProfiler.StartProfile(BOID_PROFILE_NAME);
    }
    public void StopProfiling()
    {
        isProfilingEnabled = false;
        MemoryProfiler.StopProfile(BOID_PROFILE_NAME);
    }
    public Boid GetBoidFromID(int id)
    {
        if (idToBoid.TryGetValue(id, out Boid boid))
        {
            return boid;
        }
        return null;  // or throw exception if you prefer
    }
    public Point GetPointFromID(int id)
    {
        if (idToPoint.TryGetValue(id, out Point point))
        {
            return point;
        }
        throw new System.Exception("couldn't find point with id " + id);
    }
    public void AddToPointID(Point point)
    {
        idToPoint[point.GetID()] = point;
    }
    public void AddToBoidID(Boid boid)
    {
        idToBoid[boid.ID] = boid;
    }
    private void Log(string message)
    {
        DeveloperConsole.Log("[Flock] " + message);
    }
}
