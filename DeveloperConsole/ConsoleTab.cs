using Godot;    
public abstract partial class ConsoleTab : Control
    {
        public override void _Ready()
        {
            CustomMinimumSize = new Vector2(400, 300);
            SizeFlagsHorizontal = SizeFlags.Fill;
            SizeFlagsVertical = SizeFlags.Fill;
            Show();
            
            LayoutMode = 1;
            AnchorsPreset = (int)LayoutPreset.FullRect;
            
            OnTabReady();
        }

        protected virtual void OnTabReady() { }
        public abstract void Clear();
        public abstract void WriteLine(string message, Color? color = null);
    }

