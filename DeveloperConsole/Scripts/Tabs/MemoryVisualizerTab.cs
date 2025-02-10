using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class MemoryVisualizerTab : CanvasConsoleTab
{
    private List<MemoryProfileModule.MemorySnapshot> snapshots = new();
    private Dictionary<string, Color> metricColors = new() {
        ["StaticMemory"] = Colors.Blue,
        ["DynamicMemory"] = Colors.Red,
        ["TotalMemory"] = Colors.Green
    };
    private HashSet<string> enabledMetrics = new() { "StaticMemory", "TotalMemory" };
    
    private const double TIME_WINDOW = 60.0;
    private TabContainer parentTabContainer;
    private double updateTimer = 0;

    protected override void OnTabReady() => parentTabContainer = GetParent() as TabContainer;

    public override void _Process(double delta)
    {
        if (parentTabContainer?.CurrentTab == GetIndex() && (updateTimer += delta) >= 1.0/30.0)
        {
            updateTimer = 0;
            base.Clear(); // Use base class's QueueRedraw
        }
    }

    public void UpdateData(List<MemoryProfileModule.MemorySnapshot> newSnapshots)
    {
        snapshots = new(newSnapshots ?? new());
        if (snapshots.Count > 0)
        {
            foreach (var metric in snapshots[^1].CustomMetrics.Keys.Where(k => !metricColors.ContainsKey(k)))
            {
                metricColors[metric] = new Color((float)GD.RandRange(0.3, 1.0), (float)GD.RandRange(0.3, 1.0), (float)GD.RandRange(0.3, 1.0));
            }
            if (parentTabContainer?.CurrentTab == GetIndex()) base.Clear();
        }
    }

    protected override void OnCanvasDraw()
    {
        if (snapshots.Count < 2) return;

        var chartArea = GetChartArea();
        var currentTime = snapshots[^1].Timestamp;
        var visibleData = snapshots.Where(s => s.Timestamp >= currentTime - TIME_WINDOW).ToList();
        var maxMemory = visibleData.Max(s => Math.Max(
            s.StaticMemory + s.DynamicMemory, 
            s.CustomMetrics.Values.DefaultIfEmpty(0).Max()
        )) * 1.1;

        DrawChart(chartArea, visibleData, currentTime, maxMemory);
    }

    private Rect2 GetChartArea() => new(
        40, Padding,  // x, y
        Canvas.Size.X - (40 + Padding), // width
        Canvas.Size.Y - (2 * Padding)   // height
    );

    private void DrawChart(Rect2 area, List<MemoryProfileModule.MemorySnapshot> data, double currentTime, double maxMemory)
    {
        // Background and grid
        Canvas.DrawRect(area, new Color(0.1f, 0.1f, 0.1f, 0.5f));
        var gridColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
        
        for (int i = 0; i <= 5; i++)
        {
            float x = area.Position.X + (i * area.Size.X / 5);
            float y = area.Position.Y + (i * area.Size.Y / 5);
            
            Canvas.DrawLine(new Vector2(x, area.Position.Y), new Vector2(x, area.End.Y), gridColor);
            Canvas.DrawLine(new Vector2(area.Position.X, y), new Vector2(area.End.X, y), gridColor);
            
            // Labels
            Canvas.DrawString(ThemeDB.FallbackFont, 
                new Vector2(x - 15, area.End.Y + 15),
                $"-{TIME_WINDOW * (5 - i) / 5:F0}s",
                modulate: Colors.White);

            Canvas.DrawString(ThemeDB.FallbackFont,
                new Vector2(5, y + 5),
                FormatBytes((ulong)(maxMemory * (5 - i) / 5)),
                modulate: Colors.White);
        }

        // Metrics
        foreach (var (name, color) in metricColors.Where(m => enabledMetrics.Contains(m.Key)))
        {
            DrawMetricLine(area, data, currentTime, maxMemory, name, color);
            
            // Legend entry
            var legendPos = new Vector2(area.Position.X + 10 + (metricColors.Keys.ToList().IndexOf(name) * 120), area.Position.Y + 15);
            Canvas.DrawLine(legendPos, legendPos + new Vector2(20, 0), color);
            Canvas.DrawString(ThemeDB.FallbackFont, legendPos + new Vector2(25, -5), 
                name.Contains("Memory") ? name.Replace("Memory", " Memory") : name, 
                modulate: Colors.White);
        }
    }

    private void DrawMetricLine(Rect2 area, List<MemoryProfileModule.MemorySnapshot> data, double currentTime, double maxMemory, string metricName, Color color)
    {
        Func<MemoryProfileModule.MemorySnapshot, ulong> getValue = metricName switch
        {
            "StaticMemory" => s => s.StaticMemory,
            "DynamicMemory" => s => s.DynamicMemory,
            "TotalMemory" => s => s.StaticMemory + s.DynamicMemory,
            _ => s => (ulong)s.CustomMetrics.GetValueOrDefault(metricName, 0)
        };

        var points = data.Select(s => new Vector2(
            area.Position.X + (float)((s.Timestamp - (currentTime - TIME_WINDOW)) / TIME_WINDOW) * area.Size.X,
            area.End.Y - (float)(getValue(s) / maxMemory) * area.Size.Y
        )).ToArray();

        for (int i = 1; i < points.Length; i++)
            Canvas.DrawLine(points[i - 1], points[i], color);
    }

    private string FormatBytes(ulong bytes)
    {
        string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
        int order = 0;
        double size = bytes;
        while (size >= 1024 && order < suffixes.Length - 1) { order++; size /= 1024; }
        return $"{size:F1}{suffixes[order]}";
    }

    public void ToggleMetric(string metricName)
    {
        if (enabledMetrics.Contains(metricName)) enabledMetrics.Remove(metricName);
        else enabledMetrics.Add(metricName);
        base.Clear();
    }

    public override void Clear()
    {
        snapshots.Clear();
        base.Clear();
    }
}
