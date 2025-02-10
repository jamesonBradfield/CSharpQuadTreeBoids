using Godot;

public abstract partial class CanvasConsoleTab : ConsoleTab
{
    protected Control Canvas;
    protected const float Padding = 20f;

    protected override void OnTabReady() => SetupCanvas();

    protected virtual void SetupCanvas()
    {
        Canvas = new Control { AnchorsPreset = (int)LayoutPreset.FullRect };
        AddChild(Canvas);
        Canvas.Draw += OnCanvasDraw;
    }

    protected virtual void OnCanvasDraw() { }
    public override void Clear() => Canvas.QueueRedraw();

    protected (float Scale, Vector2 Offset) GetContentTransform(Vector2 contentSize)
    {
        var canvasSize = Canvas.Size - new Vector2(Padding * 2, Padding * 2);
        float scale = Mathf.Min(canvasSize.X / contentSize.X, canvasSize.Y / contentSize.Y);
        return (scale, (Canvas.Size - contentSize * scale) / 2);
    }
}
