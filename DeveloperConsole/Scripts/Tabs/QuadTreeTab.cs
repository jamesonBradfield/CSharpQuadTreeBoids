using Godot;

public partial class QuadTreeTab : ConsoleTab
{
    private Control canvas;
    private QuadTree currentTree;
    private const float POINT_SIZE = 1.0f;
    private const float PADDING = 20.0f;

    protected override void OnTabReady()
    {
        canvas = new Control();
        AddChild(canvas);
        canvas.Draw += OnCanvasDraw;
    }

    public void UpdateTree(QuadTree tree)
    {
        currentTree = tree;
        QueueRedraw();
    }

    private void OnCanvasDraw()
    {
        if (currentTree == null) return;
        
        var bounds = CalculateTreeBounds();
        var (scale, offset) = CalculateTransform(bounds);
        DrawQuadTree(currentTree, scale, offset);
    }

    private (Vector2 min, Vector2 max) CalculateTreeBounds()
    {
        var boundary = currentTree.Boundary;
        float size = boundary.HalfSize;
        return (
            new Vector2(boundary.X - size, boundary.Y - size),
            new Vector2(boundary.X + size, boundary.Y + size)
        );
    }

    private (float scale, Vector2 offset) CalculateTransform((Vector2 min, Vector2 max) bounds)
    {
        var size = bounds.max - bounds.min;
        var canvasSize = canvas.Size;

        // Calculate scale to fit with padding
        float scale = Mathf.Min(
            (canvasSize.X - 2 * PADDING) / size.X,
            (canvasSize.Y - 2 * PADDING) / size.Y
        );

        // Center the visualization
        Vector2 center = (bounds.min + bounds.max) / 2;
        Vector2 offset = canvasSize / 2 - (center * scale);

        return (scale, offset);
    }

    private void DrawQuadTree(QuadTree tree, float scale, Vector2 offset)
    {
        var boundary = tree.Boundary;
        float x = boundary.X * scale + offset.X;
        float y = boundary.Y * scale + offset.Y;
        float size = boundary.HalfSize * scale;

        // Draw boundary
        var rect = new Rect2(x - size, y - size, size * 2, size * 2);
        canvas.DrawRect(rect, Colors.White, false);

        // Draw points
        foreach (var point in tree.Points)
        {
            Vector2 pos = new Vector2(
                point.GetX() * scale + offset.X,
                point.GetY() * scale + offset.Y
            );
            canvas.DrawCircle(pos, POINT_SIZE, Colors.Yellow);
        }

        // Draw subdivisions
        if (tree.Divided)
        {
            DrawQuadTree(tree.Nw, scale, offset);
            DrawQuadTree(tree.Ne, scale, offset);
            DrawQuadTree(tree.Sw, scale, offset);
            DrawQuadTree(tree.Se, scale, offset);
        }
    }

    public override void Clear()
    {
        currentTree = null;
        QueueRedraw();
    }

    public override void WriteLine(string message, Color? color = null)
    {
        // This tab doesn't handle text output
    }
}
