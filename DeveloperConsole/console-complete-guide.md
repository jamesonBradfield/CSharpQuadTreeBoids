# Developer Console - Complete Guide

## Initial Setup

### Scene Structure
1. Create a new scene with a Control node as root
2. Name it "DeveloperConsole"
3. Add these child nodes:
   - TabContainer (name: "TabContainer")
     - Set Layout -> Size Flags:
       - Horizontal: Expand, Fill
       - Vertical: Expand, Fill
   - LineEdit (name: "InputField")
     - Set Layout -> Size Flags:
       - Horizontal: Expand, Fill
     - Position at bottom of console

### Input Map Setup
1. Go to Project Settings -> Input Map
2. Add a new action called "toggle_console"
3. Bind it to the tilde key (~) or your preferred key

### Attaching the Script
1. Attach the DeveloperConsole script to your root Control node
2. The console will automatically:
   - Create a default console tab
   - Register basic commands
   - Set up input handling

## Basic Usage

### Console Controls
- Toggle Console: Press the tilde key (~) or your configured key
- Close Console: Press Escape
- Command History: Up/Down arrow keys
- Submit Command: Enter key

### Built-in Commands
```
help           - Lists all available commands
clear          - Clears the current or specified tab
echo <text>    - Displays text in the console
log <color> <text> - Logs colored text (colors: red, green, blue, yellow, white)
quit           - Exits the game
```

### Logging from Code
```csharp
// Basic logging
DeveloperConsole.Log("Regular message");
DeveloperConsole.LogError("Error message");  // Red
DeveloperConsole.LogWarning("Warning");      // Yellow
DeveloperConsole.LogSuccess("Success!");     // Green

// Custom colored logging
DeveloperConsole.Log("Custom color", Colors.Purple);
```

## Tab System

### Available Tab Types

1. ConsoleOutputTab (Default)
   - Standard text output
   - Supports colored text
   - Scrollable history

2. TreeViewTab
   - Hierarchical data display
   - Multi-column support
   - Colored items
   - Expandable/collapsible nodes

### Adding New Tabs

```csharp
// Get console reference
var console = GetNode<DeveloperConsole>("path/to/console");

// Add tree view tab
var entityTree = console.AddTab<DeveloperConsole.TreeViewTab>("Entities", "entity_tree");

// Configure tree columns
entityTree.SetColumns("Name", "Type", "Status");

// Add items to tree
var root = entityTree.AddItem("World");
var enemies = entityTree.AddItem("Enemies", root);
entityTree.AddItem("Goblin", enemies, Colors.Red);
```

### Accessing Tabs

```csharp
// Get any tab
var tab = console.GetTab("tab_id");

// Get specific tab type
var treeTab = console.GetTab<DeveloperConsole.TreeViewTab>("entity_tree");
```

## Creating Custom Tabs

### Basic Tab Template
```csharp
public class CustomTab : DeveloperConsole.ConsoleTab
{
    public override void _Ready()
    {
        // Initialize your tab's UI
    }

    public override void Clear()
    {
        // Implement clear functionality
    }

    public override void WriteLine(string message, Color? color = null)
    {
        // Implement text output if needed
    }

    // Add custom methods as needed
}
```

### Example: Stats Tab
```csharp
public class StatsTab : DeveloperConsole.ConsoleTab
{
    private VBoxContainer container;
    private Label fpsLabel;
    private Label memoryLabel;

    public override void _Ready()
    {
        container = new VBoxContainer();
        AddChild(container);
        
        fpsLabel = new Label();
        memoryLabel = new Label();
        container.AddChild(fpsLabel);
        container.AddChild(memoryLabel);
    }

    public override void Clear()
    {
        fpsLabel.Text = "";
        memoryLabel.Text = "";
    }

    public override void WriteLine(string message, Color? color = null)
    {
        // Optional: Implement if you want to support standard logging
    }

    public void UpdateStats(float fps, float memory)
    {
        fpsLabel.Text = $"FPS: {fps:F1}";
        memoryLabel.Text = $"Memory: {memory:F1} MB";
    }
}
```

## Adding Custom Commands

```csharp
public override void _Ready()
{
    base._Ready();  // Don't forget this!

    // Register custom command
    RegisterCommand("spawn", (args) =>
    {
        if (args.Length < 1)
        {
            LogWarning("Usage: spawn <entity_name>");
            return;
        }

        SpawnEntity(args[0]);
        LogSuccess($"Spawned entity: {args[0]}");
    });
}
```

## Integration Examples

### Entity Debug System
```csharp
public class EntityManager : Node
{
    private DeveloperConsole console;
    private DeveloperConsole.TreeViewTab entityTree;

    public override void _Ready()
    {
        console = GetNode<DeveloperConsole>("/root/DeveloperConsole");
        entityTree = console.AddTab<DeveloperConsole.TreeViewTab>("Entities", "entities");
        entityTree.SetColumns("Name", "Health", "State");

        // Register entity-related commands
        console.RegisterCommand("kill", KillEntityCommand);
        console.RegisterCommand("heal", HealEntityCommand);
        console.RegisterCommand("list", ListEntitiesCommand);
    }

    public void UpdateEntityTree()
    {
        entityTree.Clear();
        var root = entityTree.AddItem("Active Entities");

        foreach (var entity in GetEntities())
        {
            var item = entityTree.AddItem(entity.Name, root);
            entityTree.AddItem($"Health: {entity.Health}", item);
            entityTree.AddItem($"State: {entity.State}", item);
        }
    }

    private void KillEntityCommand(string[] args)
    {
        // Implementation
    }
}
```

## Best Practices

1. Organization
   - Keep the console scene at root level for easy access
   - Use meaningful tab IDs for easy reference
   - Group related commands together

2. Performance
   - Clear tabs when they're not visible
   - Update tree views only when needed
   - Use the console's built-in command history

3. User Experience
   - Add help text to all custom commands
   - Use appropriate colors for different message types
   - Keep command names short but descriptive

4. Debug Features
   - Add commands for common debug tasks
   - Include system information in custom tabs
   - Log important game events automatically

## Troubleshooting

Common Issues:
- Console not toggling: Check Input Map configuration
- Commands not working: Ensure command names are lowercase
- Tab not updating: Verify tab ID is correct
- Tree view empty: Check if Clear() was called unintentionally

## Advanced Features

### Command Aliases
```csharp
RegisterCommand("h", (args) => commands["help"].Invoke(args));
```

### Tab-Specific Commands
```csharp
RegisterCommand("filter", (args) =>
{
    if (tabContainer.CurrentTab is TreeViewTab treeTab)
    {
        // Implement filtering
    }
});
```

### Auto-Complete Support
```csharp
inputField.TextChanged += (newText) =>
{
    if (string.IsNullOrEmpty(newText)) return;
    
    var matches = commands.Keys
        .Where(cmd => cmd.StartsWith(newText))
        .ToList();
        
    if (matches.Count == 1)
    {
        inputField.Text = matches[0] + " ";
        inputField.CaretColumn = inputField.Text.Length;
    }
};
```
