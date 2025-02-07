using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
//<Summary>
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
    private Dictionary<string, ConsoleCommand> commands = new Dictionary<string, ConsoleCommand>();
    private ConsoleHistory consoleHistory;
    private ConsoleHistoryDisplay historyDisplay;
    private static DeveloperConsole instance;
    public delegate void ConsoleCommand(string[] args);

    public override void _Ready()
    {
        #region ConsoleSetup
        ProcessMode = ProcessModeEnum.Always;
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
        #region ModuleSetup
        foreach (var module in Modules ?? Array.Empty<ConsoleModule>())
        {
            try
            {
                if (module != null)
                {
                    module.Initialize(this);
                }
            }
            catch (Exception e)
            {
                GD.PrintErr($"Failed to initialize module: {e.Message}");
            }
        }
        #endregion
        #region TabSetup
        if (tabContainer.GetTabCount() > 0)
        {
            tabContainer.CurrentTab = 0;
        }
        #endregion
        #region CommandSetup
        RegisterCommand("echo", new ConsoleCommand(HandleEchoCommand));
        RegisterCommand("log", new ConsoleCommand(HandleLogCommand));
        RegisterCommand("quit", new ConsoleCommand((args) => GetTree().Quit()));
        RegisterCommand("help", new ConsoleCommand(HandleGeneralHelp));
        RegisterCommand("clear", new ConsoleCommand((args) => GetTab<ConsoleOutputTab>("console")?.Clear()));
        #endregion
        autoComplete = new ConsoleAutoComplete();
        autoComplete.SuggestionSelected += OnSuggestionSelected;

        // Position the autocomplete panel below the input field
        var inputContainer = GetNode<VBoxContainer>("MarginContainer/VBoxContainer");
        inputContainer.AddChild(autoComplete);

        // Ensure it's positioned after the input field
        autoComplete.Position = new Vector2(0, inputField.Size.Y);

        historyDisplay = new ConsoleHistoryDisplay();
        inputContainer.AddChild(historyDisplay);
        historyDisplay.Position = new Vector2(0, inputField.Size.Y);
        historyDisplay.HistorySelected += OnHistorySelected;
        #region InputSetup
        inputField.TextSubmitted += OnCommandSubmitted;
        inputField.TextChanged += OnInputChanged;
        #endregion
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
    private void OnHistorySelected(string command)
    {
        inputField.Text = command;
        inputField.CaretColumn = command.Length;
    }

    private void OnInputChanged(string newText)
    {
        if (autoComplete == null)
        {
            return;
        }

        try
        {
            autoComplete.UpdateSuggestions(newText, commands?.Keys ?? Enumerable.Empty<string>());
        }
        catch (Exception e)
        {
            GD.PrintErr($"AutoComplete error: {e.Message}");
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
    private void ShowHistoryDisplay()
    {
        List<string> history = new List<string>();
        // Get history from console history
        // You'll need to add a method to ConsoleHistory to get the list
        historyDisplay.UpdateHistory(history);
    }
    public override void _UnhandledInput(InputEvent @event)
    {
        if (!Visible) return;

        if (@event is InputEventKey key && key.Pressed)
        {
            bool handled = false;

            // Check for Ctrl+H to toggle history display
            if (key.Keycode == Key.H && key.CtrlPressed)
            {
                if (historyDisplay.IsVisible)
                {
                    historyDisplay.CancelDisplay();
                }
                else
                {
                    ShowHistoryDisplay();
                }
                handled = true;
            }
            else switch (key.Keycode)
                {
                    case Key.Up:
                        if (autoComplete.HasSuggestions)
                        {
                            autoComplete.NavigateSuggestions(-1);
                            handled = true;
                        }
                        else if (historyDisplay.IsVisible)
                        {
                            historyDisplay.NavigateHistory(-1);
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
                        else if (historyDisplay.IsVisible)
                        {
                            historyDisplay.NavigateHistory(1);
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
                        else if (historyDisplay.IsVisible)
                        {
                            historyDisplay.AcceptHistory();
                            handled = true;
                        }
                        break;

                    case Key.Escape:
                        if (autoComplete.HasSuggestions)
                        {
                            autoComplete.CancelSuggestions();
                            handled = true;
                        }
                        else if (historyDisplay.IsVisible)
                        {
                            historyDisplay.CancelDisplay();
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
        consoleHistory.NavigateHistory(direction, inputField.Text);
    }
    private void OnHistoryChanged(string text)
    {
        inputField.Text = text;
        inputField.CaretColumn = text.Length;
    }
    public T AddTab<T>(string title, string id) where T : ConsoleTab, new()
    {
        EnsureTabContainer();

        if (tabs.ContainsKey(id))
        {
            GD.PrintErr($"Tab with id {id} already exists");
            return (T)tabs[id];
        }

        var tab = new T();
        tab.Name = id;
        tab.SizeFlagsHorizontal = Control.SizeFlags.Fill;
        tab.SizeFlagsVertical = Control.SizeFlags.Fill;

        try
        {
            tabContainer.AddChild(tab);
            int tabIndex = tab.GetIndex();
            tabContainer.SetTabTitle(tabIndex, title);

            tab.Show();
            if (tabContainer.Size != Vector2.Zero)
            {
                tab.CustomMinimumSize = tabContainer.Size;
            }

            tabs[id] = tab;
            return tab;
        }
        catch (Exception e)
        {
            GD.PrintErr($"Failed to create tab {id}: {e.Message}");
            return null;
        }
    }
    private void EnsureTabContainer()
    {
        if (tabContainer == null)
        {
            tabContainer = GetNode<TabContainer>("MarginContainer/VBoxContainer/TabContainer");
            if (tabContainer == null)
            {
                throw new InvalidOperationException("TabContainer not found in scene");
            }
        }
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

        consoleHistory.AddToHistory(text);
        inputField.Clear();
    }

    public void ExecuteCommand(string commandLine)
    {
        if (string.IsNullOrWhiteSpace(commandLine))
        {
            return;
        }

        try
        {
            string[] parts = commandLine.Split(' ');
            string commandName = parts[0].ToLower();
            string[] args = parts.Skip(1).ToArray();

            if (commands != null && commands.ContainsKey(commandName))
            {
                try
                {
                    commands[commandName]?.Invoke(args);
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
        catch (Exception e)
        {
            LogError($"Failed to parse command: {e.Message}");
        }
    }

    public static void Log(string message, Color? color = null)
    {
        if (instance == null)
        {
            GD.PrintErr("Console instance not initialized");
            return;
        }
        instance.GetTab<ConsoleOutputTab>("console")?.WriteLine(message, color);
    }

    public static void LogError(string message) => Log(message, Colors.Red);
    public static void LogWarning(string message) => Log(message, Colors.Yellow);
    public static void LogSuccess(string message) => Log(message, Colors.Green);
}
