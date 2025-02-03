using Godot;    
public partial class ConsoleOutputTab : ConsoleTab
    {
        private RichTextLabel outputField;

        public override void _Ready()
        {
            SetupOutputField();
        }

        private void SetupOutputField()
        {
            outputField = new RichTextLabel
            {
                BbcodeEnabled = true,
                ScrollFollowing = true,
                SizeFlagsHorizontal = SizeFlags.Fill,
                SizeFlagsVertical = SizeFlags.Fill,
                Name = "OutputField",
                AnchorsPreset = (int)LayoutPreset.FullRect,
				SelectionEnabled = true
            };

            outputField.LayoutMode = 1;
            outputField.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect, margin: 0);
            
            AddChild(outputField);
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


