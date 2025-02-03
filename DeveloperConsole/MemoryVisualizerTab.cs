using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class MemoryVisualizerTab : ConsoleTab
{
    private Control canvas;
    private List<MemoryProfileModule.MemorySnapshot> snapshots = new();
    private const float PADDING = 20.0f;
    private const float LABEL_PADDING = 40.0f;
    private const double TIME_WINDOW = 60.0;
    private TabContainer parentTabContainer;
    
    // Add color constants for consistency
    private static readonly Color StaticMemoryColor = Colors.Blue;
    private static readonly Color TotalMemoryColor = Colors.Green;
    private static readonly Color GridColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
    private static readonly Color BackgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.5f);
    private double updateTimer = 0;
    private const double UPDATE_INTERVAL = 1.0/30.0; // 30 FPS update rate

    // Add vertical grid line settings
    private const int VERTICAL_GRID_LINES = 6; // One line every 10 seconds for 60s window
    private const int HORIZONTAL_GRID_LINES = 5;

    public override void _Ready()
    {
        base._Ready();
        SetupCanvas();
        parentTabContainer = GetParent() as TabContainer;
    }

    private void SetupCanvas()
    {
        canvas = new Control();
        canvas.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        AddChild(canvas);
        canvas.Draw += OnCanvasDraw;
    }
    public override void _Process(double delta)
    {
        if (parentTabContainer == null) return;

        // Only update when our tab is visible
        bool isVisible = parentTabContainer.CurrentTab == GetIndex();
        if (!isVisible) return;

        updateTimer += delta;
        if (updateTimer >= UPDATE_INTERVAL)
        {
            updateTimer = 0;
            QueueRedraw();
        }
    }
    private void DrawGrid(Rect2 graphRect)
    {
        // Draw background
        canvas.DrawRect(graphRect, BackgroundColor);
        
        // Draw vertical grid lines (time divisions)
        for (int i = 0; i <= VERTICAL_GRID_LINES; i++)
        {
            float x = graphRect.Position.X + (i * graphRect.Size.X / VERTICAL_GRID_LINES);
            Vector2 start = new Vector2(x, graphRect.Position.Y);
            Vector2 end = new Vector2(x, graphRect.Position.Y + graphRect.Size.Y);
            canvas.DrawLine(start, end, GridColor);
        }

        // Draw horizontal grid lines (memory divisions)
        for (int i = 0; i <= HORIZONTAL_GRID_LINES; i++)
        {
            float y = graphRect.Position.Y + (i * graphRect.Size.Y / HORIZONTAL_GRID_LINES);
            Vector2 start = new Vector2(graphRect.Position.X, y);
            Vector2 end = new Vector2(graphRect.Position.X + graphRect.Size.X, y);
            canvas.DrawLine(start, end, GridColor);
        }
    }

    private void DrawLegend(Rect2 graphRect)
    {
        var font = ThemeDB.FallbackFont;
        float y = graphRect.Position.Y + 15;
        float x = graphRect.Position.X + 10;
        float lineLength = 20;
        float spacing = 5;

        // Draw Static Memory legend
        canvas.DrawLine(
            new Vector2(x, y),
            new Vector2(x + lineLength, y),
            StaticMemoryColor);
        canvas.DrawString(font,
            new Vector2(x + lineLength + spacing, y - 5),
            "Static Memory",
            modulate: Colors.White);

        // Draw Total Memory legend
        y += 20;
        canvas.DrawLine(
            new Vector2(x, y),
            new Vector2(x + lineLength, y),
            TotalMemoryColor);
        canvas.DrawString(font,
            new Vector2(x + lineLength + spacing, y - 5),
            "Total Memory",
            modulate: Colors.White);
    }

    private void DrawMemoryGraph(Rect2 graphRect)
    {
        if (snapshots.Count < 2) return;

        var currentTime = snapshots[^1].Timestamp;
        var startTime = currentTime - TIME_WINDOW;
        
        // Filter snapshots to only show those within our time window
        var visibleSnapshots = snapshots
            .Where(s => s.Timestamp >= startTime && s.Timestamp <= currentTime)
            .ToList();

        if (visibleSnapshots.Count < 2) return;
        
        var maxMemory = visibleSnapshots.Max(s => (double)(s.StaticMemory + s.DynamicMemory));
        // Add 10% padding to max memory for better visualization
        maxMemory = maxMemory * 1.1;

        // Draw static memory line
        DrawDataLine(graphRect, visibleSnapshots,
            s => s.StaticMemory,
            maxMemory,
            startTime,
            currentTime,
            StaticMemoryColor);

        // Draw total memory line
        DrawDataLine(graphRect, visibleSnapshots,
            s => s.StaticMemory + s.DynamicMemory,
            maxMemory,
            startTime,
            currentTime,
            TotalMemoryColor);
    }

    private void OnCanvasDraw()
    {
        if (snapshots == null || snapshots.Count == 0) return;
        
        var canvasSize = canvas.Size;
        var graphRect = new Rect2(
            LABEL_PADDING,
            PADDING,
            canvasSize.X - (LABEL_PADDING + PADDING),
            canvasSize.Y - (2 * PADDING)
        );

        DrawGrid(graphRect);
        DrawMemoryGraph(graphRect);
        DrawLabels(graphRect);
        DrawLegend(graphRect);
    }
	public void UpdateData(List<MemoryProfileModule.MemorySnapshot> newSnapshots)
	{
		if (newSnapshots == null) return;
		snapshots = new List<MemoryProfileModule.MemorySnapshot>(newSnapshots);
		QueueRedraw();  // This will only work if the tab is visible due to _Process check
		
		// Force a redraw if this is from a new snapshot
		if (parentTabContainer != null && parentTabContainer.CurrentTab == GetIndex())
		{
			canvas?.QueueRedraw();
		}
	}

private void DrawDataLine(Rect2 graphRect, List<MemoryProfileModule.MemorySnapshot> data, 
    Func<MemoryProfileModule.MemorySnapshot, ulong> getValue, double maxValue, 
    double startTime, double currentTime, Color color)
{
    if (data.Count < 2) return;

    Vector2[] points = new Vector2[data.Count];
    double timeRange = currentTime - startTime;
    
    for (int i = 0; i < data.Count; i++)
    {
        var snapshot = data[i];
        // Calculate x position relative to the current time window
        float xPercent = (float)((snapshot.Timestamp - startTime) / timeRange);
        float x = graphRect.Position.X + (xPercent * graphRect.Size.X);
        
        // Calculate y position
        float yValue = getValue(snapshot);
        float yPercent = maxValue > 0 ? (float)(yValue / maxValue) : 0;
        float y = graphRect.End.Y - (yPercent * graphRect.Size.Y);
        
        points[i] = new Vector2(x, y);
    }

    // Draw the lines
    for (int i = 1; i < points.Length; i++)
    {
        canvas.DrawLine(points[i - 1], points[i], color);
    }
}

    private void DrawLabels(Rect2 graphRect)
    {
        if (snapshots.Count == 0) return;

        var currentTime = snapshots[^1].Timestamp;
        var startTime = Math.Max(snapshots[0].Timestamp, currentTime - TIME_WINDOW);
        var endTime = currentTime;

        // Filter snapshots for max memory calculation
        var visibleSnapshots = snapshots.Where(s => s.Timestamp >= startTime).ToList();
        var maxMemory = visibleSnapshots.Max(s => s.StaticMemory + s.DynamicMemory);

        // Use the default font
        var font = ThemeDB.FallbackFont;
        var fontSize = ThemeDB.FallbackFontSize;

        // Draw time labels
        canvas.DrawString(font,
            new Vector2(graphRect.Position.X, graphRect.End.Y + 15),
            $"-{TIME_WINDOW:F1}s",
            modulate: Colors.White);

        canvas.DrawString(font,
            new Vector2(graphRect.End.X - 40, graphRect.End.Y + 15),
            "0s",
            modulate: Colors.White);

        // Draw memory labels
        canvas.DrawString(font,
            new Vector2(5, graphRect.Position.Y),
            FormatBytes(maxMemory),
            modulate: Colors.White);

        canvas.DrawString(font,
            new Vector2(5, graphRect.End.Y - 15),
            "0",
            modulate: Colors.White);
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

    public override void Clear()
    {
        snapshots.Clear();
        QueueRedraw();
    }

    public override void WriteLine(string message, Color? color = null)
    {
        // This tab doesn't handle text output
    }
}
