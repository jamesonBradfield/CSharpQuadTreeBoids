using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

[GlobalClass]
public partial class MemoryProfileModule : ConsoleModule
{
    private bool _isRecording = false;
    private double _recordInterval = 1.0;
    private double _elapsedTime = 0.0;
    private Dictionary<string, List<MemorySnapshot>> _profileSnapshots = new();
    private int _maxSnapshots = QuadTreeConstants.WORLD_TO_QUAD_SCALE;
    public string _currentProfile = "default";
    private Dictionary<string, double> startTimes = new Dictionary<string, double>();
    private Dictionary<string, double> memoryUsages = new Dictionary<string, double>();
    public class MemorySnapshot
    {
        public double Timestamp { get; set; }
        public ulong StaticMemory { get; set; }
        public ulong DynamicMemory { get; set; }
        public double Objects { get; set; }
        public double Resources { get; set; }
        public double Nodes { get; set; }
        public Dictionary<string, double> CustomMetrics { get; set; } = new();
    }

    public override void Initialize(DeveloperConsole console)
    {
        MemoryProfiler.Initialize(this);
        base.Initialize(console);
        ModuleTab = console.AddTab<MemoryVisualizerTab>("Memory Graph", "memgraph");
        CreateProfile(new[] { "default" });  // Fix: Pass string array
    }

    protected override void RegisterCommands()
    {
        base.RegisterCommands();
        RegisterCommand("mem.start", StartRecording);
        RegisterCommand("mem.stop", StopRecording);
        RegisterCommand("mem.snapshot", TakeSnapshot);
        RegisterCommand("mem.stats", ShowCurrentStats);
        RegisterCommand("mem.interval", SetInterval);
        RegisterCommand("mem.export", ExportStats);
        RegisterCommand("mem.clear", ClearStats);
        RegisterCommand("mem.create", CreateProfile);
        RegisterCommand("mem.switch", SwitchProfile);
        RegisterCommand("mem.list", ListProfiles);
        RegisterCommand("mem.compare", CompareProfiles);
        RegisterCommand("mem.delete", DeleteProfile);
    }

    protected override void DisplayHelp()
    {
        Log("Memory Profiling Commands:");
        Log("  mem.start - Start recording memory stats");
        Log("  mem.stop - Stop recording memory stats");
        Log("  mem.snapshot - Take a single memory snapshot");
        Log("  mem.stats - Display current memory statistics");
        Log("  mem.interval [seconds] - Set recording interval (default: 1.0)");
        Log("  mem.export [filename] - Export memory stats to CSV");
        Log("  mem.clear - Clear recorded stats");
        Log("  mem.create [name] - Create a new memory profile");
        Log("  mem.switch [name] - Switch to a different profile");
        Log("  mem.list - List all available profiles");
        Log("  mem.compare [profile1] [profile2] - Compare two profiles");
        Log("  mem.delete [name] - Delete a profile");
    }

    public void CreateProfile(string[] args)
    {
        if (args.Length < 1)
        {
            LogError("Usage: mem.create [profile_name]");
            return;
        }

        string profileName = args[0];
        if (_profileSnapshots.ContainsKey(profileName))
        {
            LogWarning($"Profile '{profileName}' already exists");
            return;
        }

        _profileSnapshots[profileName] = new List<MemorySnapshot>();
        LogSuccess($"Created new memory profile: {profileName}");
    }

    public void SwitchProfile(string[] args)
    {
        if (args.Length < 1)
        {
            LogError("Usage: mem.switch [profile_name]");
            return;
        }

        string profileName = args[0];
        if (!_profileSnapshots.ContainsKey(profileName))
        {
            LogError($"Profile '{profileName}' does not exist");
            return;
        }

        _currentProfile = profileName;
        LogSuccess($"Switched to profile: {profileName}");

        if (ModuleTab is MemoryVisualizerTab visualizer)
        {
            visualizer.UpdateData(_profileSnapshots[_currentProfile]);
        }
    }

    public void ListProfiles(string[] args)
    {
        Log("\nAvailable Memory Profiles:");
        foreach (var profile in _profileSnapshots.Keys)
        {
            var snapCount = _profileSnapshots[profile].Count;
            var indicator = profile == _currentProfile ? "*" : " ";
            Log($"{indicator} {profile} ({snapCount} snapshots)");
            foreach (var metrics in _profileSnapshots[profile])
            {
                Log($"{metrics.CustomMetrics}");
            }
        }
    }

    public void DeleteProfile(string[] args)
    {
        if (args.Length < 1)
        {
            LogError("Usage: mem.delete [profile_name]");
            return;
        }

        string profileName = args[0];
        if (profileName == "default")
        {
            LogError("Cannot delete the default profile");
            return;
        }

        if (!_profileSnapshots.ContainsKey(profileName))
        {
            LogError($"Profile '{profileName}' does not exist");
            return;
        }

        if (_currentProfile == profileName)
        {
            _currentProfile = "default";
        }

        _profileSnapshots.Remove(profileName);
        LogSuccess($"Deleted profile: {profileName}");
    }

    public void CompareProfiles(string[] args)
    {
        if (args.Length < 2)
        {
            LogError("Usage: mem.compare [profile1] [profile2]");
            return;
        }

        string profile1 = args[0];
        string profile2 = args[1];

        if (!_profileSnapshots.ContainsKey(profile1) || !_profileSnapshots.ContainsKey(profile2))
        {
            LogError("One or both profiles do not exist");
            return;
        }

        var snapshots1 = _profileSnapshots[profile1];
        var snapshots2 = _profileSnapshots[profile2];

        if (snapshots1.Count == 0 || snapshots2.Count == 0)
        {
            LogError("One or both profiles have no snapshots");
            return;
        }

        Log($"\nComparing profiles: {profile1} vs {profile2}");

        var last1 = snapshots1[^1];
        var last2 = snapshots2[^1];

        Log("\nCurrent Memory Comparison:");
        Log($"Static Memory:   {profile1}: {FormatBytes(last1.StaticMemory)}   {profile2}: {FormatBytes(last2.StaticMemory)}");
        Log($"Dynamic Memory:  {profile1}: {FormatBytes(last1.DynamicMemory)}   {profile2}: {FormatBytes(last2.DynamicMemory)}");
        Log($"Objects:         {profile1}: {last1.Objects:F0}   {profile2}: {last2.Objects:F0}");
        Log($"Resources:       {profile1}: {last1.Resources:F0}   {profile2}: {last2.Resources:F0}");
        Log($"Nodes:           {profile1}: {last1.Nodes:F0}   {profile2}: {last2.Nodes:F0}");

        // Compare custom metrics if they exist
        var allMetrics = last1.CustomMetrics.Keys.Union(last2.CustomMetrics.Keys);
        if (allMetrics.Any())
        {
            Log("\nCustom Metrics:");
            foreach (var metric in allMetrics)
            {
                var value1 = last1.CustomMetrics.GetValueOrDefault(metric, 0);
                var value2 = last2.CustomMetrics.GetValueOrDefault(metric, 0);
                Log($"{metric,-15} {profile1}: {value1:F2}   {profile2}: {value2:F2}");
            }
        }
    }

    public void AddCustomMetric(string metricName, double value)
    {
        if (!_profileSnapshots.ContainsKey(_currentProfile) ||
            _profileSnapshots[_currentProfile].Count == 0)
        {
            return;
        }

        var currentSnapshot = _profileSnapshots[_currentProfile][^1];
        currentSnapshot.CustomMetrics[metricName] = value;
    }

    public void TakeSnapshot(string[] args)
    {
        var snapshot = new MemorySnapshot
        {
            Timestamp = Time.GetTicksMsec() / 1000.0,
            StaticMemory = OS.GetStaticMemoryUsage(),
            DynamicMemory = (ulong)Math.Max(0, GC.GetTotalMemory(false)), // Cast with safety check
            Objects = Performance.GetMonitor(Performance.Monitor.ObjectCount),
            Resources = Performance.GetMonitor(Performance.Monitor.ObjectCount), // Alternative to ResourceCount
            Nodes = Performance.GetMonitor(Performance.Monitor.ObjectNodeCount)  // Fixed NodeCount enum
        };

        if (!_profileSnapshots.ContainsKey(_currentProfile))
        {
            _profileSnapshots[_currentProfile] = new List<MemorySnapshot>();
        }

        var snapshots = _profileSnapshots[_currentProfile];
        snapshots.Add(snapshot);

        if (snapshots.Count > _maxSnapshots)
        {
            snapshots.RemoveAt(0);
        }

        if (ModuleTab is MemoryVisualizerTab visualizer)
        {
            visualizer.UpdateData(new List<MemorySnapshot>(snapshots));
        }

        if (args.Length > 0 && args[0] == "verbose")
        {
            ShowCurrentStats(args);
        }
    }

    public void StartRecording(string[] args)
    {
        if (_isRecording)
        {
            LogWarning("Memory profiling is already running");
            return;
        }

        _isRecording = true;
        _elapsedTime = 0.0;
        TakeSnapshot(args);
        LogSuccess("Memory profiling started");
    }

    public void StopRecording(string[] args)
    {
        if (!_isRecording)
        {
            LogWarning("Memory profiling is not running");
            return;
        }

        _isRecording = false;
        TakeSnapshot(args);
        LogSuccess("Memory profiling stopped");
        ShowStats();
    }

    private void SetInterval(string[] args)
    {
        if (args.Length < 1)
        {
            LogError("Usage: mem.interval [seconds]");
            return;
        }

        if (double.TryParse(args[0], out double interval) && interval > 0)
        {
            _recordInterval = interval;
            LogSuccess($"Recording interval set to {interval} seconds");
        }
        else
        {
            LogError("Invalid interval value. Must be a positive number.");
        }
    }

    private void ShowCurrentStats(string[] args)
    {
        var snapshots = _profileSnapshots[_currentProfile];
        if (snapshots.Count == 0)
        {
            LogWarning("No memory snapshots available");
            return;
        }
        var latest = snapshots[^1];

        Log("\nCurrent Memory Stats:");
        Log($"Static Memory: {FormatBytes(latest.StaticMemory)}");
        Log($"Dynamic Memory: {FormatBytes(latest.DynamicMemory)}");
        Log($"Total Memory: {FormatBytes(latest.StaticMemory + latest.DynamicMemory)}");
        Log($"Objects: {latest.Objects:F0}");
        Log($"Resources: {latest.Resources:F0}");
        Log($"Nodes: {latest.Nodes:F0}");

        if (latest.CustomMetrics.Count > 0)
        {
            Log("\nCustom Metrics:");
            foreach (var metric in latest.CustomMetrics.OrderBy(m => m.Key))
            {
                Log($"{metric.Key}: {metric.Value:F2}");
            }
        }
    }

    private void ShowStats()
    {
        var snapshots = _profileSnapshots[_currentProfile];
        if (snapshots.Count < 2)
        {
            ShowCurrentStats(Array.Empty<string>());
            return;
        }

        var first = snapshots[0];
        var last = snapshots[^1];
        var duration = last.Timestamp - first.Timestamp;

        Log("\nMemory Profile Summary:");
        Log($"Duration: {duration:F1} seconds");
        Log($"Snapshots: {snapshots.Count}");

        // Standard metrics changes
        var staticChange = last.StaticMemory - first.StaticMemory;
        var dynamicChange = last.DynamicMemory - first.DynamicMemory;
        var objectChange = last.Objects - first.Objects;
        var resourceChange = last.Resources - first.Resources;
        var nodeChange = last.Nodes - first.Nodes;

        Log("\nMemory Changes:");
        Log($"Static Memory: {FormatBytes(staticChange)} ({GetChangePercentage(first.StaticMemory, last.StaticMemory)}%)");
        Log($"Dynamic Memory: {FormatBytes(dynamicChange)} ({GetChangePercentage(first.DynamicMemory, last.DynamicMemory)}%)");
        Log($"Objects: {objectChange:+#;-#;0} ({GetChangePercentage(first.Objects, last.Objects)}%)");
        Log($"Resources: {resourceChange:+#;-#;0} ({GetChangePercentage(first.Resources, last.Resources)}%)");
        Log($"Nodes: {nodeChange:+#;-#;0} ({GetChangePercentage(first.Nodes, last.Nodes)}%)");

        // Custom metrics changes
        if (last.CustomMetrics.Count > 0)
        {
            Log("\nCustom Metrics Changes:");
            var allMetricKeys = first.CustomMetrics.Keys.Union(last.CustomMetrics.Keys).OrderBy(k => k);

            foreach (var key in allMetricKeys)
            {
                var firstValue = first.CustomMetrics.GetValueOrDefault(key, 0.0);
                var lastValue = last.CustomMetrics.GetValueOrDefault(key, 0.0);
                var change = lastValue - firstValue;
                Log($"{key}: {change:+#.##;-#.##;0.##} ({GetChangePercentage(firstValue, lastValue)}%)");
            }
        }

        // Peak values for standard metrics
        var peakStatic = 0UL;
        var peakDynamic = 0UL;
        var peakObjects = 0.0;
        var peakResources = 0.0;
        var peakNodes = 0.0;

        // Dictionary to track peak values for custom metrics
        var peakCustomMetrics = new Dictionary<string, double>();

        foreach (var snapshot in snapshots)
        {
            peakStatic = Math.Max(peakStatic, snapshot.StaticMemory);
            peakDynamic = Math.Max(peakDynamic, snapshot.DynamicMemory);
            peakObjects = Math.Max(peakObjects, snapshot.Objects);
            peakResources = Math.Max(peakResources, snapshot.Resources);
            peakNodes = Math.Max(peakNodes, snapshot.Nodes);

            // Track peak values for custom metrics
            foreach (var metric in snapshot.CustomMetrics)
            {
                if (!peakCustomMetrics.ContainsKey(metric.Key) ||
                    metric.Value > peakCustomMetrics[metric.Key])
                {
                    peakCustomMetrics[metric.Key] = metric.Value;
                }
            }
        }

        Log("\nPeak Values:");
        Log($"Peak Static Memory: {FormatBytes(peakStatic)}");
        Log($"Peak Dynamic Memory: {FormatBytes(peakDynamic)}");
        Log($"Peak Objects: {peakObjects:F0}");
        Log($"Peak Resources: {peakResources:F0}");
        Log($"Peak Nodes: {peakNodes:F0}");

        if (peakCustomMetrics.Count > 0)
        {
            Log("\nPeak Custom Metrics:");
            foreach (var metric in peakCustomMetrics.OrderBy(m => m.Key))
            {
                Log($"Peak {metric.Key}: {metric.Value:F2}");
            }
        }
    }

    private void ExportStats(string[] args)
    {
        var snapshots = _profileSnapshots[_currentProfile];
        if (snapshots.Count == 0)
        {
            LogWarning("No memory snapshots to export");
            return;
        }

        string filename = args.Length > 0 ? args[0] : "memory_stats.csv";
        if (!filename.EndsWith(".csv"))
        {
            filename += ".csv";
        }

        using var file = FileAccess.Open($"user://{filename}", FileAccess.ModeFlags.Write);
        if (file == null)
        {
            LogError($"Failed to open file for writing: {filename}");
            return;
        }

        file.StoreLine("timestamp,static_memory,dynamic_memory,objects,resources,nodes");

        foreach (var snapshot in snapshots)
        {
            file.StoreLine($"{snapshot.Timestamp},{snapshot.StaticMemory}," +
                          $"{snapshot.DynamicMemory},{snapshot.Objects}," +
                          $"{snapshot.Resources},{snapshot.Nodes}");
        }

        LogSuccess($"Memory stats exported to user://{filename}");
    }

    private void ClearStats(string[] args)
    {
        if (_profileSnapshots.ContainsKey(_currentProfile))
        {
            _profileSnapshots[_currentProfile].Clear();
            if (ModuleTab is MemoryVisualizerTab visualizer)
            {
                visualizer.UpdateData(_profileSnapshots[_currentProfile]);
            }
            LogSuccess("Memory stats cleared");
        }
    }

    private static string FormatBytes(ulong bytes)
    {
        string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
        int order = 0;
        double size = bytes;

        while (size >= 1024 && order < suffixes.Length - 1)
        {
            order++;
            size /= 1024;
        }

        return $"{size:F2} {suffixes[order]}";
    }

    private static float GetChangePercentage(double initial, double final)
    {
        if (initial == 0) return 0;
        return (float)((final - initial) / initial * 100);
    }

    public void Process(double delta)
    {
        if (!_isRecording) return;

        _elapsedTime += delta;
        if (_elapsedTime >= _recordInterval)
        {
            _elapsedTime = 0;
            TakeSnapshot(Array.Empty<string>());
        }
    }
}
