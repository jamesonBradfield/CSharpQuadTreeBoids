using Godot;

public abstract partial class ConsoleTab : Control
{
    public override void _Ready()
    {
        SizeFlagsHorizontal = SizeFlags.Fill;
        SizeFlagsVertical = SizeFlags.Fill;
        AnchorsPreset = (int)LayoutPreset.FullRect;
        OnTabReady();
    }

    protected virtual void OnTabReady() { }
    public abstract void Clear();
    public virtual void WriteLine(string message, Color? color = null) { }
}
