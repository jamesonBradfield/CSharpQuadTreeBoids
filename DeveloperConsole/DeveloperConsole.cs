using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class DeveloperConsole : Control
{
    [Export]
    public ConsoleModule[] Modules { get; set; } = new ConsoleModule[0];

    // UI components
    private TabContainer tabContainer;
    private LineEdit inputField;
    private ConsoleAutoComplete autoComplete;
    
    // Core data structures
    private Dictionary<string, ConsoleTab> tabs = new();
    private Dictionary<string, ConsoleCommand> commands = new();
    private List<string> commandHistory = new();
    private int historyIndex = -1;

    private static DeveloperConsole instance;
    public delegate void ConsoleCommand(string[] args);

    public override void _Ready()
    {
		#region ConsoleSetup
        ProcessMode = ProcessModeEnum.Always;  // Ensure we process input even when paused
        instance = this;
        tabContainer = GetNode<TabContainer>("MarginContainer/VBoxContainer/TabContainer");
        inputField = GetNode<LineEdit>("MarginContainer/VBoxContainer/InputField");
        // More aggressive focus control
        inputField.FocusMode = Control.FocusModeEnum.All;
        
        // Disable tab navigation on the TabContainer
        tabContainer.FocusMode = Control.FocusModeEnum.None;
        foreach (var child in tabContainer.GetChildren())
        {
            if (child is Control control)
            {
                control.FocusMode = Control.FocusModeEnum.None;
            }
        }
        // Create default console tab first
        var consoleTab = AddTab<ConsoleOutputTab>("Console", "console");
        
        Hide(); // Start hidden
		#endregion

        foreach (var module in Modules)
        {
            if (module != null)
            {
                module.Initialize(this);
            }
        }
        
        if (tabContainer.GetTabCount() > 0)
        {
            tabContainer.CurrentTab = 0;
        }
		#region CommandSetup
        RegisterCommand("echo", new ConsoleCommand(HandleEchoCommand));
        RegisterCommand("log", new ConsoleCommand(HandleLogCommand));
        RegisterCommand("quit", new ConsoleCommand((args) => GetTree().Quit()));
        RegisterCommand("help", new ConsoleCommand(HandleGeneralHelp));
        RegisterCommand("clear", new ConsoleCommand((args) => GetTab<ConsoleOutputTab>("console")?.Clear()));
        autoComplete = new ConsoleAutoComplete();
        autoComplete.SuggestionSelected += OnSuggestionSelected;
        
        // Position the autocomplete panel below the input field
        var inputContainer = GetNode<VBoxContainer>("MarginContainer/VBoxContainer");
        inputContainer.AddChild(autoComplete);
        
        // Ensure it's positioned after the input field
        autoComplete.Position = new Vector2(0, inputField.Size.Y);
		#endregion
        inputField.TextSubmitted += OnCommandSubmitted;
        inputField.TextChanged += OnInputChanged;
    }

	public override void _Process(double delta)
	{
		base._Process(delta);
		foreach (var module in Modules)
		{
			if (module is MemoryProfileModule memModule)
			{
				memModule.Process(delta);
			}
		}
	}



    private void OnInputChanged(string newText)
    {
        if (autoComplete != null)
        {
            autoComplete.UpdateSuggestions(newText, commands.Keys);
        }
    }

    private void OnSuggestionSelected(string suggestion)
    {
        inputField.Text = suggestion;
        inputField.CaretColumn = suggestion.Length;
    }

    public override void _Input(InputEvent @event)
    {
		// we need to handle backtick input even when not shown
        if (@event is InputEventKey inputKey && inputKey.Pressed)
        {
            if (inputKey.Keycode == Key.Quoteleft)
            {
                if (Visible)
                {
                    Hide();
                    inputField.ReleaseFocus();
                }
                else
                {
                    Show();
                    inputField.GrabFocus();
                }
                GetTree().Root.SetInputAsHandled();
                AcceptEvent();
                return;
            }
        }
        
        // Handle other keys if console is visible
        if (Visible)
        {
			// handle escape exiting
			if (Visible && @event is InputEventKey escapeKey && 
				escapeKey.Pressed && escapeKey.Keycode == Key.Escape)
			{
				Hide();
				inputField.ReleaseFocus();
				GetTree().Root.SetInputAsHandled();
			}
            
            if (@event is InputEventKey tabKey && tabKey.Pressed && tabKey.Keycode == Key.Tab)
            {
				if (!Visible || autoComplete == null) return;
				
				if (autoComplete.HasSuggestions)
				{
					if (tabKey.ShiftPressed)
					{
						autoComplete.NavigateSuggestions(-1);
					}
					else
					{
						autoComplete.NavigateSuggestions(1);
					}
					string suggestion = autoComplete.GetSelectedSuggestion();
					if (suggestion != null)
					{
						inputField.Text = suggestion;
						inputField.CaretColumn = suggestion.Length;
					}
				}
                GetTree().Root.SetInputAsHandled();
                AcceptEvent();
            }
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (!Visible) return;

        if (@event is InputEventKey key && key.Pressed)
        {
            bool handled = false;
            
            switch (key.Keycode)
            {
                case Key.Up:
                    if (autoComplete.HasSuggestions)
                    {
                        autoComplete.NavigateSuggestions(-1);
                        handled = true;
                    }
                    else
                    {
                        NavigateHistory(-1);
                        handled = true;
                    }
                    break;
                    
                case Key.Down:
                    if (autoComplete.HasSuggestions)
                    {
                        autoComplete.NavigateSuggestions(1);
                        handled = true;
                    }
                    else
                    {
                        NavigateHistory(1);
                        handled = true;
                    }
                    break;

                case Key.Enter:
                case Key.KpEnter:
                    if (autoComplete.HasSuggestions)
                    {
                        autoComplete.AcceptSuggestion();
                        autoComplete.CancelSuggestions();
                        handled = true;
                    }
                    break;

                case Key.Escape:
                    if (autoComplete.HasSuggestions)
                    {
                        autoComplete.CancelSuggestions();
                        handled = true;
                    }
                    break;
            }

            if (handled)
            {
                GetTree().Root.SetInputAsHandled();
            }
        }
    }

    private void NavigateHistory(int direction)
    {
        if (commandHistory.Count == 0)
            return;

        historyIndex = Math.Clamp(historyIndex + direction, -1, commandHistory.Count - 1);

        if (historyIndex == -1)
            inputField.Text = "";
        else
            inputField.Text = commandHistory[historyIndex];
            
        inputField.CaretColumn = inputField.Text.Length;
    }

    public T AddTab<T>(string title, string id) where T : ConsoleTab, new()
    {
        var tab = new T();
        tab.Name = id;
        tab.SizeFlagsHorizontal = Control.SizeFlags.Fill;
        tab.SizeFlagsVertical = Control.SizeFlags.Fill;
        
        tabContainer.AddChild(tab);
        int tabIndex = tab.GetIndex();
        tabContainer.SetTabTitle(tabIndex, title);
        
        tab.Show();
        tab.CustomMinimumSize = tabContainer.Size;
        
        tabs[id] = tab;
        return tab;
    }

    public ConsoleTab GetTab(string id)
    {
        return tabs.GetValueOrDefault(id);
    }

    public T GetTab<T>(string id) where T : ConsoleTab
    {
        if (tabs.TryGetValue(id, out var tab) && tab is T typedTab)
            return typedTab;
        return null;
    }

    public IEnumerable<string> GetModuleIds()
    {
        return Modules.Select(m => m.GetType().Name.Replace("Module", "").ToLower());
    }

    private void HandleEchoCommand(string[] args)
    {
        Log(string.Join(" ", args));
    }

    private void HandleLogCommand(string[] args)
    {
        if (args.Length < 2)
        {
            LogWarning("Usage: log <color> <message>");
            return;
        }
        
        Color color = Colors.White;
        switch (args[0].ToLower())
        {
            case "red": color = Colors.Red; break;
            case "green": color = Colors.Green; break;
            case "blue": color = Colors.Blue; break;
            case "yellow": color = Colors.Yellow; break;
            case "white": color = Colors.White; break;
            default: LogWarning($"Unknown color: {args[0]}. Using white."); break;
        }
        
        Log(string.Join(" ", args.Skip(1)), color);
    }

    private void HandleGeneralHelp(string[] args)
    {
        Log("Available Console Commands:");
        Log("  help - Shows this help message");
        Log("  help <module> - Shows help for a specific module");
        Log("  echo <message> - Displays a message in the console");
        Log("  log <color> <message> - Logs a colored message");
        Log("  quit - Exits the application");
        Log("  clear - Clears the console output");
        Log("");
        
        var modules = GetModuleIds()
            .Where(id => id != "console")
            .OrderBy(id => id);
                           
        if (modules.Any())
        {
            Log("Available Modules:");
            foreach (var moduleId in modules)
            {
                Log($"  {moduleId} (use '{moduleId}.help' for module commands)");
            }
        }
    }

    public void RegisterCommand(string name, ConsoleCommand command)
    {
        commands[name.ToLower()] = command;
    }

    public IEnumerable<string> GetCommands()
    {
        return commands.Keys;
    }

    private void OnCommandSubmitted(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return;

        Log($"> {text}");
        ExecuteCommand(text);
        
        commandHistory.Add(text);
        historyIndex = -1;
        inputField.Clear();
    }

    public void ExecuteCommand(string commandLine)
    {
        string[] parts = commandLine.Split(' ');
        string commandName = parts[0].ToLower();
        string[] args = parts.Skip(1).ToArray();

        if (commands.ContainsKey(commandName))
        {
            try
            {
                commands[commandName].Invoke(args);
            }
            catch (Exception e)
            {
                LogError($"Error executing command: {e.Message}");
            }
        }
        else
        {
            LogError($"Unknown command: {commandName}");
        }
    }

    public static void Log(string message, Color? color = null)
    {
            instance.GetTab<ConsoleOutputTab>("console")?.WriteLine(message, color);
    }

    public static void LogError(string message) => Log(message, Colors.Red);
    public static void LogWarning(string message) => Log(message, Colors.Yellow);
    public static void LogSuccess(string message) => Log(message, Colors.Green);
}
