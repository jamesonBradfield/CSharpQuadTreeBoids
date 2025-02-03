using Godot;

[GlobalClass]
public partial class QuadTreeModule : ConsoleModule
{
    protected override string TabId => "quadtree";
    private QuadTree currentTree;
    private QuadTreeTab visualTab;

    public override void Initialize(DeveloperConsole console)
    {
        base.Initialize(console);
        visualTab = console.AddTab<QuadTreeTab>("QuadTree", TabId);
    }

    protected override void RegisterCommands()
    {
        base.RegisterCommands();
        
        RegisterCommand("quadtree.create", HandleCreateCommand);
        RegisterCommand("quadtree.insert", HandleInsertPoint);
        RegisterCommand("quadtree.help", HandleHelpCommand);
        RegisterCommand("quadtree.randInsert", HandleRandInsertCommand);
        RegisterCommand("quadtree.clear", HandleClearCommand);
    }

    private void HandleRandInsertCommand(string[] args)
    {
		ulong start_time = Time.GetTicksUsec();
        if (args.Length != 1)
        {
            LogWarning("Usage: quadtree.randInsert <count>");
            return;
        }
        if (!int.TryParse(args[0], out int count))
        {
            LogError("Invalid parameter. parameter must be int");
            return;
        }
        if (currentTree == null)
        {
            LogError("No Tree found, create a tree first with quadtree.create");
            return;
        }
        var boundary = currentTree.GetBoundary();
        currentTree.Dispose();
        currentTree = new QuadTree(boundary, 4);
        for (int i = 0; i < count; i++)
        {
            var point = new Point(
                (int)GD.RandRange(-boundary.GetW(), boundary.GetW()),
				(int)GD.RandRange(-boundary.GetH(), boundary.GetH())
            );
			currentTree.Insert(point);
        }
		ulong end_time = Time.GetTicksUsec();
		double elapsed_time = (end_time - start_time) / 1000000.0; // Convert to seconds

		Log($"RandInsert completed in {elapsed_time:F2} seconds");
        visualTab?.UpdateTree(currentTree);
    }

    private void HandleCreateCommand(string[] args)
    {
        if (args.Length != 3)
        {
            LogWarning("Usage: quadtree.create <size> <x> <y>");
            return;
        }

        if (!int.TryParse(args[0], out int size) ||
            !int.TryParse(args[1], out int x) ||
            !int.TryParse(args[2], out int y))
        {
            LogError("Invalid parameters. All parameters must be numbers.");
            return;
        }

        var boundary = new Rectangle(x, y, size, size);
        currentTree?.Dispose();
        currentTree = new QuadTree(boundary, 4);
        LogSuccess("QuadTree created successfully!");
        visualTab?.UpdateTree(currentTree);
    }

    private void HandleInsertPoint(string[] args)
    {
        if (currentTree == null)
        {
            LogError("No QuadTree exists. Create one first with 'quadtree.create'");
            return;
        }

        if (args.Length != 2)
        {
            LogWarning("Usage: quadtree.insert <x> <y>");
            return;
        }

        if (!int.TryParse(args[0], out int x) ||
            !int.TryParse(args[1], out int y))
        {
            LogError("Invalid coordinates. Both parameters must be numbers.");
            return;
        }

        var point = new Point(x, y);
        if (currentTree.Insert(point))
        {
            LogSuccess($"Point ({x}, {y}) inserted successfully!");
            visualTab?.UpdateTree(currentTree);
        }
        else
        {
            LogError($"Failed to insert point ({x}, {y})");
        }
    }

    private void HandleClearCommand(string[] args)
    {
        currentTree?.Dispose();
        currentTree = null;
        visualTab?.UpdateTree(null);
        LogSuccess("QuadTree cleared");
    }

    private void HandleHelpCommand(string[] args)
    {
        Log("QuadTree Module Commands:");
        Log("  quadtree.create <size> <x> <y> - Create a new QuadTree with given size and center position");
        Log("    size: The half-width/height of the boundary");
        Log("    x,y: The center coordinates of the boundary");
        Log("");
        Log("  quadtree.insert <x> <y> - Insert a point at the specified coordinates");
        Log("    x,y: The coordinates of the point to insert");
        Log("");
        Log("  quadtree.randInsert <count> - Insert random points into the tree");
        Log("  quadtree.clear - Clear the current tree");
        Log("  quadtree.help - Display this help message");
    }

    protected override void OnCleanup()
    {
        base.OnCleanup();
        currentTree?.Dispose();
        currentTree = null;
    }
}
