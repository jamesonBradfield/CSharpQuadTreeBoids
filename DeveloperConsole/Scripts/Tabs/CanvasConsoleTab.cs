using Godot;

public abstract partial class CanvasConsoleTab : ConsoleTab
{
    protected Control canvas;
    protected const float PADDING = 20.0f; // Default padding around visualizations

    protected override void OnTabReady()
    {
        SetupCanvas();
    }

    protected virtual void SetupCanvas()
    {
        canvas = new Control();
        canvas.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        AddChild(canvas);
        canvas.Draw += OnCanvasDraw;
    }

    protected virtual void OnCanvasDraw()
    {
        // Override in derived classes to implement specific drawing logic
    }

    public override void Clear()
    {
        QueueRedraw();
    }

    public override void WriteLine(string message, Color? color = null)
    {
        // Canvas-based tabs typically don't handle text output
    }

    // Helper method to calculate scale for fitting content
    protected (float scale, Vector2 offset) CalculateScaleAndOffset(Vector2 contentMin, Vector2 contentMax)
    {
        var bounds = new Vector2(contentMax.X - contentMin.X, contentMax.Y - contentMin.Y);
        var canvasSize = canvas.Size;
        
        float scaleX = (canvasSize.X - 2 * PADDING) / bounds.X;
        float scaleY = (canvasSize.Y - 2 * PADDING) / bounds.Y;
        float scale = Mathf.Min(scaleX, scaleY);

        Vector2 center = (contentMin + contentMax) / 2;
        Vector2 canvasCenter = canvasSize / 2;
        Vector2 offset = canvasCenter - (center * scale);

        return (scale, offset);
    }
}
