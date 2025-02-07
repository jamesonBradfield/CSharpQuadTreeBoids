using Godot;
public partial class ConsoleOutputTab : ConsoleTab
{
    private RichTextLabel outputField;

    public override void _Ready()
    {
        // Create the RichTextLabel with specific settings
        outputField = new RichTextLabel
        {
            BbcodeEnabled = true,
            ScrollFollowing = true,
            SizeFlagsHorizontal = SizeFlags.Fill,
            SizeFlagsVertical = SizeFlags.Fill,
            Name = "OutputField",  // Give it a name for debugging
            AnchorsPreset = (int)LayoutPreset.FullRect,  // Make it fill the parent
			SelectionEnabled = true
        };

        // Ensure layout properties are set
        outputField.LayoutMode = 1;  // Use anchors
        outputField.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect, margin: 0);
        
        // Add to scene tree
        AddChild(outputField);
        
        // Verify creation
        GD.Print($"OutputField created and added to {Name}");
    }

    public override void Clear()
    {
        if (outputField != null)
            outputField.Clear();
    }

    public override void WriteLine(string message, Color? color = null)
    {
        if (outputField == null)
        {
            GD.PrintErr("OutputField is null in WriteLine");
            return;
        }

        if (color.HasValue)
            outputField.PushColor(color.Value);
            
        outputField.AddText(message + "\n");
        
        if (color.HasValue)
            outputField.Pop();
    }
}
