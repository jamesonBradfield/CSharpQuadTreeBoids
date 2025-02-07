using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class MemoryVisualizerTab : CanvasConsoleTab
{
    private List<MemoryProfileModule.MemorySnapshot> snapshots = new();
    private Dictionary<string, Color> metricColors = new();
    private HashSet<string> enabledMetrics = new();
    private const float LABEL_PADDING = 40.0f;
    private const double TIME_WINDOW = 60.0;
    private TabContainer parentTabContainer;
    private double updateTimer = 0;
    private const double UPDATE_INTERVAL = 1.0 / 30.0;
    private string chartType = "stacked_area";  // Store as a property instead
    // Default colors for standard metrics
    private static readonly Color StaticMemoryColor = Colors.Blue;
    private static readonly Color DynamicMemoryColor = Colors.Red;
    private static readonly Color TotalMemoryColor = Colors.Green;
    private static readonly Color GridColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
    private static readonly Color BackgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.5f);

    // Grid configuration
    private const int VERTICAL_GRID_LINES = 6;
    private const int HORIZONTAL_GRID_LINES = 5;

    protected override void OnTabReady()
    {
        base.OnTabReady();
        parentTabContainer = GetParent() as TabContainer;

        // Initialize default enabled metrics
        enabledMetrics.Add("StaticMemory");
        enabledMetrics.Add("TotalMemory");

        // Setup default colors for standard metrics
        metricColors["StaticMemory"] = StaticMemoryColor;
        metricColors["DynamicMemory"] = DynamicMemoryColor;
        metricColors["TotalMemory"] = TotalMemoryColor;
    }

    public override void _Process(double delta)
    {
        if (parentTabContainer?.CurrentTab != GetIndex()) return;

        updateTimer += delta;
        if (updateTimer >= UPDATE_INTERVAL)
        {
            updateTimer = 0;
            QueueRedraw();
        }
    }

    public void UpdateData(List<MemoryProfileModule.MemorySnapshot> newSnapshots)
    {
        snapshots = new List<MemoryProfileModule.MemorySnapshot>(newSnapshots ?? new());

        // Update metric colors for any new custom metrics
        if (snapshots.Count > 0)
        {
            var lastSnapshot = snapshots[^1];
            foreach (var metric in lastSnapshot.CustomMetrics.Keys)
            {
                if (!metricColors.ContainsKey(metric))
                {
                    // Generate a new color for this metric
                    metricColors[metric] = new Color(
                        (float)GD.RandRange(0.3, 1.0),
                        (float)GD.RandRange(0.3, 1.0),
                        (float)GD.RandRange(0.3, 1.0)
                    );
                }
            }
        }

        if (parentTabContainer?.CurrentTab == GetIndex())
        {
            QueueRedraw();
        }
    }

    protected override void OnCanvasDraw()
    {
        if (snapshots.Count == 0) return;

        var graphRect = new Rect2(
            LABEL_PADDING,
            PADDING,
            canvas.Size.X - (LABEL_PADDING + PADDING),
            canvas.Size.Y - (2 * PADDING)
        );

        DrawGrid(graphRect);
        DrawMemoryGraph(graphRect);
        DrawLabels(graphRect);
        DrawLegend(graphRect);
    }

    private void DrawMemoryGraph(Rect2 graphRect)
    {
        if (snapshots.Count < 2) return;

        var currentTime = snapshots[^1].Timestamp;
        var startTime = currentTime - TIME_WINDOW;
        var visibleSnapshots = snapshots.Where(s => s.Timestamp >= startTime).ToList();

        if (visibleSnapshots.Count < 2) return;

        // Calculate max value including custom metrics
        double maxMemory = visibleSnapshots.Max(s =>
        {
            double max = s.StaticMemory + s.DynamicMemory;
            foreach (var metric in s.CustomMetrics.Values)
            {
                max = Math.Max(max, metric);
            }
            return max;
        }) * 1.1;

        // Draw standard metrics
        if (enabledMetrics.Contains("StaticMemory"))
        {
            DrawDataLine(graphRect, visibleSnapshots, s => s.StaticMemory, maxMemory, startTime, currentTime, StaticMemoryColor);
        }

        if (enabledMetrics.Contains("DynamicMemory"))
        {
            DrawDataLine(graphRect, visibleSnapshots, s => s.DynamicMemory, maxMemory, startTime, currentTime, DynamicMemoryColor);
        }

        if (enabledMetrics.Contains("TotalMemory"))
        {
            DrawDataLine(graphRect, visibleSnapshots, s => s.StaticMemory + s.DynamicMemory, maxMemory, startTime, currentTime, TotalMemoryColor);
        }

        // Draw custom metrics
        if (visibleSnapshots.Count > 0)
        {
            var lastSnapshot = visibleSnapshots[^1];
            foreach (var metric in lastSnapshot.CustomMetrics.Keys)
            {
                if (enabledMetrics.Contains(metric) && metricColors.ContainsKey(metric))
                {
                    DrawDataLine(graphRect, visibleSnapshots,
                        s => (ulong)(s.CustomMetrics.GetValueOrDefault(metric, 0)),
                        maxMemory, startTime, currentTime, metricColors[metric]);
                }
            }
        }
    }

    private void DrawGrid(Rect2 graphRect)
    {
        canvas.DrawRect(graphRect, BackgroundColor);

        for (int i = 0; i <= VERTICAL_GRID_LINES; i++)
        {
            float x = graphRect.Position.X + (i * graphRect.Size.X / VERTICAL_GRID_LINES);
            Vector2 start = new Vector2(x, graphRect.Position.Y);
            Vector2 end = new Vector2(x, graphRect.Position.Y + graphRect.Size.Y);
            canvas.DrawLine(start, end, GridColor);
        }

        for (int i = 0; i <= HORIZONTAL_GRID_LINES; i++)
        {
            float y = graphRect.Position.Y + (i * graphRect.Size.Y / HORIZONTAL_GRID_LINES);
            Vector2 start = new Vector2(graphRect.Position.X, y);
            Vector2 end = new Vector2(graphRect.Position.X + graphRect.Size.X, y);
            canvas.DrawLine(start, end, GridColor);
        }
    }

    private void DrawDataLine(Rect2 graphRect, List<MemoryProfileModule.MemorySnapshot> data,
        Func<MemoryProfileModule.MemorySnapshot, ulong> getValue, double maxValue, double startTime, double currentTime, Color color)
    {
        if (data.Count < 2) return;

        Vector2[] points = new Vector2[data.Count];
        double timeRange = currentTime - startTime;

        for (int i = 0; i < data.Count; i++)
        {
            var snapshot = data[i];
            float xPercent = (float)((snapshot.Timestamp - startTime) / timeRange);
            float x = graphRect.Position.X + (xPercent * graphRect.Size.X);

            float yValue = getValue(snapshot);
            float yPercent = maxValue > 0 ? (float)(yValue / maxValue) : 0;
            float y = graphRect.End.Y - (yPercent * graphRect.Size.Y);

            points[i] = new Vector2(x, y);
        }

        for (int i = 1; i < points.Length; i++)
        {
            canvas.DrawLine(points[i - 1], points[i], color);
        }
    }

    private void DrawLegend(Rect2 graphRect)
    {
        var font = ThemeDB.FallbackFont;
        float y = graphRect.Position.Y + 15;
        float x = graphRect.Position.X + 10;
        float lineLength = 20;
        float spacing = 5;

        // Draw standard metrics
        if (enabledMetrics.Contains("StaticMemory"))
        {
            DrawLegendItem("Static Memory", StaticMemoryColor, ref x, ref y, lineLength, spacing, font);
        }

        if (enabledMetrics.Contains("DynamicMemory"))
        {
            DrawLegendItem("Dynamic Memory", DynamicMemoryColor, ref x, ref y, lineLength, spacing, font);
        }

        if (enabledMetrics.Contains("TotalMemory"))
        {
            DrawLegendItem("Total Memory", TotalMemoryColor, ref x, ref y, lineLength, spacing, font);
        }

        // Draw custom metrics
        if (snapshots.Count > 0)
        {
            var lastSnapshot = snapshots[^1];
            foreach (var metric in lastSnapshot.CustomMetrics.Keys)
            {
                if (enabledMetrics.Contains(metric) && metricColors.ContainsKey(metric))
                {
                    DrawLegendItem(metric, metricColors[metric], ref x, ref y, lineLength, spacing, font);
                }
            }
        }
    }

    private void DrawLegendItem(string label, Color color, ref float x, ref float y, float lineLength, float spacing, Font font)
    {
        canvas.DrawLine(new Vector2(x, y), new Vector2(x + lineLength, y), color);
        canvas.DrawString(font, new Vector2(x + lineLength + spacing, y - 5), label, modulate: Colors.White);
        y += 20;
    }

    private void DrawLabels(Rect2 graphRect)
    {
        if (snapshots.Count == 0) return;

        var currentTime = snapshots[^1].Timestamp;
        var startTime = currentTime - TIME_WINDOW;
        var visibleSnapshots = snapshots.Where(s => s.Timestamp >= startTime).ToList();
        var maxMemory = visibleSnapshots.Max(s => (decimal)(s.StaticMemory + s.DynamicMemory));

        var font = ThemeDB.FallbackFont;

        // Time labels
        for (int i = 0; i <= VERTICAL_GRID_LINES; i++)
        {
            float x = graphRect.Position.X + (i * graphRect.Size.X / VERTICAL_GRID_LINES);
            float timeValue = (float)(TIME_WINDOW * (VERTICAL_GRID_LINES - i) / VERTICAL_GRID_LINES);
            canvas.DrawString(font,
                new Vector2(x - 15, graphRect.End.Y + 15),
                $"-{timeValue:F0}s",
                modulate: Colors.White);
        }

        // Memory labels
        for (int i = 0; i <= HORIZONTAL_GRID_LINES; i++)
        {
            float y = graphRect.Position.Y + (i * graphRect.Size.Y / HORIZONTAL_GRID_LINES);
            float memoryValue = (float)(maxMemory * (HORIZONTAL_GRID_LINES - i) / HORIZONTAL_GRID_LINES);
            canvas.DrawString(font,
                new Vector2(5, y + 5),
                FormatBytes((ulong)memoryValue),
                modulate: Colors.White);
        }
    }
    private void UpdateVisualization(List<MemoryProfileModule.MemorySnapshot> snapshots)
    {
        // Get all unique metric names across all snapshots
        HashSet<string> metricNames = new HashSet<string>();
        foreach (var snapshot in snapshots)
        {
            foreach (var metric in snapshot.CustomMetrics.Keys)
            {
                metricNames.Add(metric);
            }
        }

        // Update metrics without trying to set chart type as a metric
        foreach (var name in metricNames)
        {
            var values = snapshots.Select(s => s.CustomMetrics.GetValueOrDefault(name, 0)).ToList();
            MemoryProfiler.AddMetric("memory_visualization", name, values.Average());
        }
    }
    public void SetChartType(string type)
    {
        chartType = type;
        // Trigger redraw or update as needed
    }
    private string FormatBytes(ulong bytes)
    {
        string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
        int order = 0;
        double size = bytes;

        while (size >= 1024 && order < suffixes.Length - 1)
        {
            order++;
            size /= 1024;
        }

        return $"{size:F1}{suffixes[order]}";
    }

    public void ToggleMetric(string metricName)
    {
        if (enabledMetrics.Contains(metricName))
        {
            enabledMetrics.Remove(metricName);
        }
        else
        {
            enabledMetrics.Add(metricName);
        }
        QueueRedraw();
    }

    public override void Clear()
    {
        snapshots.Clear();
        QueueRedraw();
    }
}
