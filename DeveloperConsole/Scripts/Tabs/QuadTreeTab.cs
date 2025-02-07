using Godot;

public partial class QuadTreeTab : ConsoleTab
{
    private Control canvas;
    private QuadTree currentTree;
    private const float POINT_SIZE = 1.0f;
    private const float PADDING = 20.0f; // Padding around the visualization

    public override void _Ready()
    {
        base._Ready();
        SetupCanvas();
    }

    private void SetupCanvas()
    {
        canvas = new Control();
        canvas.SetAnchorsPreset(Control.LayoutPreset.FullRect);
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

        // Calculate bounds of the entire quadtree
        (Vector2 min, Vector2 max) = CalculateTreeBounds(currentTree);

        // Calculate scale and offset to fit and center the tree
        var bounds = new Vector2(max.X - min.X, max.Y - min.Y);
        var canvasSize = canvas.Size;

        // Calculate scale to fit the tree with padding
        float scaleX = (canvasSize.X - 2 * PADDING) / bounds.X;
        float scaleY = (canvasSize.Y - 2 * PADDING) / bounds.Y;
        float scale = Mathf.Min(scaleX, scaleY);

        // Calculate offset to center the tree
        Vector2 center = (min + max) / 2;
        Vector2 canvasCenter = canvasSize / 2;
        Vector2 offset = canvasCenter - (center * scale);

        // Draw the tree with transformation
        DrawQuadTree(currentTree, scale, offset);
    }

    private (Vector2 min, Vector2 max) CalculateTreeBounds(QuadTree tree)
    {
        var boundary = tree.GetBoundary();
        float x = (float)boundary.GetX();
        float y = (float)boundary.GetY();
        float s = (float)boundary.GetS();

        Vector2 min = new Vector2(x - s, y - s);
        Vector2 max = new Vector2(x + s, y + s);

        if (tree.IsDivided())
        {
            void UpdateBounds(QuadTree child)
            {
                var (childMin, childMax) = CalculateTreeBounds(child);
                min = new Vector2(Mathf.Min(min.X, childMin.X), Mathf.Min(min.Y, childMin.Y));
                max = new Vector2(Mathf.Max(max.X, childMax.X), Mathf.Max(max.Y, childMax.Y));
            }

            UpdateBounds(tree.GetNorthwest());
            UpdateBounds(tree.GetNortheast());
            UpdateBounds(tree.GetSouthwest());
            UpdateBounds(tree.GetSoutheast());
        }

        return (min, max);
    }

    private void DrawQuadTree(QuadTree tree, float scale, Vector2 offset)
    {
        var boundary = tree.GetBoundary();
        var points = tree.GetPoints();

        // Transform boundary coordinates
        float x = (float)boundary.GetX() * scale + offset.X;
        float y = (float)boundary.GetY() * scale + offset.Y;
        float s = (float)boundary.GetS() * scale;

        // Draw boundary
        // var rect = new Rect2(x - s, y - s, s * 2);
        // canvas.DrawRect(rect, Colors.White, false, 1.0f);

        // Draw points
        foreach (var point in points)
        {
            Vector2 transformedPoint = new Vector2(
                (float)point.GetX() * scale + offset.X,
                (float)point.GetY() * scale + offset.Y
            );
            canvas.DrawCircle(transformedPoint, POINT_SIZE, Colors.Yellow);
        }

        // Recursively draw subdivisions
        if (tree.IsDivided())
        {
            DrawQuadTree(tree.GetNorthwest(), scale, offset);
            DrawQuadTree(tree.GetNortheast(), scale, offset);
            DrawQuadTree(tree.GetSouthwest(), scale, offset);
            DrawQuadTree(tree.GetSoutheast(), scale, offset);
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

