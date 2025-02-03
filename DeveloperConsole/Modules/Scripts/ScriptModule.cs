using Godot;
using System;  // Add this for StringSplitOptions
using System.Linq;
using System.Collections.Generic;

[GlobalClass]
public partial class ScriptModule : ConsoleModule
{
    protected override string TabId => "script";
    private Dictionary<string, Script> scripts = new();

    protected override void RegisterCommands()
    {
        base.RegisterCommands();
        RegisterCommand("script.create", HandleCreateScript);
        RegisterCommand("script.run", HandleRunScript);
        RegisterCommand("script.list", HandleListScripts);
        RegisterCommand("script.show", HandleShowScript);
    }

    private void HandleCreateScript(string[] args)
    {
        if (args.Length < 2)
        {
            LogWarning("Usage: script.create <name> command1; command2; ...");
            return;
        }

        string scriptName = args[0];
        // Join everything after the script name and split by semicolons
        string commandString = string.Join(" ", args.Skip(1));
        var commands = commandString.Split(';', StringSplitOptions.RemoveEmptyEntries)
                                  .Select(cmd => cmd.Trim())  // Remove leading/trailing spaces
                                  .Where(cmd => !string.IsNullOrEmpty(cmd))  // Skip empty commands
                                  .ToList();

        var script = new Script
        {
            Commands = commands,
            Parameters = ExtractParameters(commands)
        };
        
        LogWarning("Commands: " + string.Join("\n  ", script.Commands));
        LogWarning("Parameters: " + string.Join(", ", script.Parameters));
        
        scripts[scriptName] = script;
        LogSuccess($"Script '{scriptName}' created with {script.Commands.Count} commands");
    }

	private List<string> ExtractParameters(IEnumerable<string> commands){
		HashSet<string> parameters = new HashSet<string>();
		foreach(var command in commands){
			for(int i = 0; i < 10; i++){
				if (command.Contains($"{{{i}}}")){
					parameters.Add($"{{{i}}}");
				}
			}
		}
		return parameters.OrderBy(p => int.Parse(p.Trim('{','}'))).ToList();
	}
    private void HandleRunScript(string[] args)
    {
        if (args.Length < 1)
        {
            LogWarning("Usage: script.run <scriptName> [arg1 arg2 ...]");
            return;
        }

        string scriptName = args[0];
        if (!scripts.ContainsKey(scriptName))
        {
            LogError($"Script '{scriptName}' not found");
            return;
        }

        var script = scripts[scriptName];
        var scriptArgs = args.Skip(1).ToList();

        // Validate argument count
        if (scriptArgs.Count < script.Parameters.Count)
        {
            LogError($"Script '{scriptName}' requires {script.Parameters.Count} arguments, but only {scriptArgs.Count} provided");
            Log($"Expected parameters: {string.Join(", ", script.Parameters)}");
            return;
        }

        Log($"Running script '{scriptName}'...");
        foreach (var command in script.Commands)
        {
            string processedCommand = ProcessCommand(command, scriptArgs);
            Console.ExecuteCommand(processedCommand);
        }
        LogSuccess($"Script '{scriptName}' completed");
    }	
    private void HandleShowScript(string[] args)
    {
        if (args.Length != 1)
        {
            LogWarning("Usage: script.show <scriptName>");
            return;
        }

        string scriptName = args[0];
        if (!scripts.ContainsKey(scriptName))
        {
            LogError($"Script '{scriptName}' not found");
            return;
        }

        var script = scripts[scriptName];
        Log($"Script '{scriptName}':");
        Log($"Parameters: {string.Join(", ", script.Parameters)}");
        Log("Commands:");
        foreach (var command in script.Commands)
        {
            Log($"  {command}");
        }
    }


	private string ProcessCommand(string command, List<string> args)
	{
		// Process the entire command string at once
		string processedCommand = command;
		for (int i = 0; i < args.Count; i++)
		{
			processedCommand = processedCommand.Replace($"{{{i}}}", args[i]);
		}
		return processedCommand;
	}

    private void HandleListScripts(string[] args)
    {
        if (scripts.Count == 0)
        {
            Log("No scripts defined");
            return;
        }

        Log("Available scripts:");
        foreach (var script in scripts)
        {
            Log($"  {script.Key} ({script.Value.Commands.Count} commands)");
        }
    }

}
