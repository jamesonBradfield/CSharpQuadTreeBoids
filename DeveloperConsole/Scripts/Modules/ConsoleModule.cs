using Godot;

[GlobalClass]
public abstract partial class ConsoleModule : Resource
{
    protected DeveloperConsole Console { get; private set; }
    protected ConsoleTab ModuleTab { get; set; }
    
    // Use TabId for command prefixes
    protected virtual string TabId => GetType().Name.Replace("Module", "").ToLower();
    
    // Use ModuleName for display purposes
    protected virtual string ModuleName => GetType().Name.Replace("Module", "");
    
    public virtual void Initialize(DeveloperConsole console)
    {
        Console = console;
        RegisterCommands();
    }

    protected virtual void RegisterCommands()
    {
        RegisterCommand($"{TabId}.help", (args) => DisplayHelp());
        RegisterCommand($"{TabId}.clear", (args) => ModuleTab?.Clear());
    }

    protected void RegisterCommand(string name, DeveloperConsole.ConsoleCommand command)
    {
        Console.RegisterCommand(name.ToLower(), command);
    }

    protected virtual void DisplayHelp()
    {
        Log($"Available commands for {ModuleName}:");
        Log($"  {TabId}.help - Shows this help message");
        Log($"  {TabId}.clear - Clears the {ModuleName} tab");
    }

	protected void Log(string message, Color? color = null)
	{
		DeveloperConsole.Log(message, color); // Writes to the main "console" tab
	}
    protected virtual void OnCleanup()
    {
        // Base implementation does nothing
    }

    public void Cleanup()
    {
        OnCleanup();
    }
	protected void LogError(string message) => DeveloperConsole.LogError(message);
	protected void LogWarning(string message) => DeveloperConsole.LogWarning(message);
	protected void LogSuccess(string message) => DeveloperConsole.LogSuccess(message);
}
