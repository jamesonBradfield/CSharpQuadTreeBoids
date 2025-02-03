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
    private List<MemorySnapshot> _snapshots = new();
    private int _maxSnapshots = 1000;

    public class MemorySnapshot
    {
        public double Timestamp { get; set; }
        public ulong StaticMemory { get; set; }
        public ulong DynamicMemory { get; set; }
        public double Objects { get; set; }
        public double Resources { get; set; }
        public double Nodes { get; set; }
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
    }

    private void StartRecording(string[] args)
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
	public override void Initialize(DeveloperConsole console)
	{
		base.Initialize(console);
		ModuleTab = console.AddTab<MemoryVisualizerTab>("Memory Graph", "memgraph");
		
		// Take initial snapshot and update visualizer
		TakeSnapshot(Array.Empty<string>());
	}
    private void StopRecording(string[] args)
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

	private void TakeSnapshot(string[] args)
	{
		var snapshot = new MemorySnapshot
		{
			Timestamp = Time.GetTicksMsec() / 1000.0,
			StaticMemory = OS.GetStaticMemoryUsage(),
			DynamicMemory = OS.GetStaticMemoryUsage(),
			Objects = Performance.GetMonitor(Performance.Monitor.ObjectCount),
			Resources = Performance.GetMonitor(Performance.Monitor.ObjectCount),
			Nodes = Performance.GetMonitor(Performance.Monitor.ObjectCount)
		};

		_snapshots.Add(snapshot);
		
		if (_snapshots.Count > _maxSnapshots)
		{
			_snapshots.RemoveAt(0);
		}

		// Update visualizer once
		if (ModuleTab is MemoryVisualizerTab visualizer)
		{
			visualizer.UpdateData(_snapshots);
		}

		if (args.Length > 0 && args[0] == "verbose")
		{
			ShowCurrentStats(args);
		}
	}

    private void ShowCurrentStats(string[] args)
    {
        if (_snapshots.Count == 0)
        {
            LogWarning("No memory snapshots available");
            return;
        }

        var latest = _snapshots[_snapshots.Count - 1];
        Log("\nCurrent Memory Stats:");
        Log($"Static Memory: {FormatBytes(latest.StaticMemory)}");
        Log($"Dynamic Memory: {FormatBytes(latest.DynamicMemory)}");
        Log($"Total Memory: {FormatBytes(latest.StaticMemory + latest.DynamicMemory)}");
        Log($"Objects: {latest.Objects:F0}");
        Log($"Resources: {latest.Resources:F0}");
        Log($"Nodes: {latest.Nodes:F0}");
    }

    private void ShowStats()
    {
        if (_snapshots.Count < 2)
        {
            ShowCurrentStats(Array.Empty<string>());
            return;
        }

        var first = _snapshots[0];
        var last = _snapshots[_snapshots.Count - 1];
        var duration = last.Timestamp - first.Timestamp;

        Log("\nMemory Profile Summary:");
        Log($"Duration: {duration:F1} seconds");
        Log($"Snapshots: {_snapshots.Count}");

        // Calculate changes
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

        // Show peaks
        var peakStatic = 0UL;
        var peakDynamic = 0UL;
        var peakObjects = 0.0;
        var peakResources = 0.0;
        var peakNodes = 0.0;

        foreach (var snapshot in _snapshots)
        {
            peakStatic = Math.Max(peakStatic, snapshot.StaticMemory);
            peakDynamic = Math.Max(peakDynamic, snapshot.DynamicMemory);
            peakObjects = Math.Max(peakObjects, snapshot.Objects);
            peakResources = Math.Max(peakResources, snapshot.Resources);
            peakNodes = Math.Max(peakNodes, snapshot.Nodes);
        }

        Log("\nPeak Values:");
        Log($"Peak Static Memory: {FormatBytes(peakStatic)}");
        Log($"Peak Dynamic Memory: {FormatBytes(peakDynamic)}");
        Log($"Peak Objects: {peakObjects:F0}");
        Log($"Peak Resources: {peakResources:F0}");
        Log($"Peak Nodes: {peakNodes:F0}");
    }

    private void ExportStats(string[] args)
    {
        if (_snapshots.Count == 0)
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

        // Write header
        file.StoreLine("timestamp,static_memory,dynamic_memory,objects,resources,nodes");

        // Write data
        foreach (var snapshot in _snapshots)
        {
            file.StoreLine($"{snapshot.Timestamp},{snapshot.StaticMemory}," +
                          $"{snapshot.DynamicMemory},{snapshot.Objects}," +
                          $"{snapshot.Resources},{snapshot.Nodes}");
        }

        LogSuccess($"Memory stats exported to user://{filename}");
    }

    private void ClearStats(string[] args)
    {
        _snapshots.Clear();
        LogSuccess("Memory stats cleared");
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
        if (initial == 0)
            return 0;
        return (float)((final - initial) / initial) * 100;
    }

	public void Process(double delta)
    {
        if (!_isRecording)
            return;

        _elapsedTime += delta;
        if (_elapsedTime >= _recordInterval)
        {
            _elapsedTime = 0;
            TakeSnapshot(Array.Empty<string>());
        }
    }
}
