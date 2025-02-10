using Godot;

public partial class ConsoleOutputTab : ConsoleTab
{
    private RichTextLabel outputField;

    protected override void OnTabReady()
    {
        outputField = new RichTextLabel
        {
            BbcodeEnabled = true,
            ScrollFollowing = true,
            SelectionEnabled = true,
            Name = "OutputField"
        };
        
        AddChild(outputField);
        GD.Print($"OutputField created and added to {Name}");
    }

    public override void Clear() => outputField?.Clear();

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
