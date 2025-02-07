using Godot;
using System;
using System.Collections.Generic;

public partial class ConsoleHistory : Node
{
    private ConsoleHistoryDisplay historyDisplay;
    private List<string> history = new();
    private int currentIndex = -1;
    private string tempInput = string.Empty; // Stores user input when navigating history
    
    public event Action<string> HistoryChanged;
    
    public void AddToHistory(string command)
    {
        if (string.IsNullOrWhiteSpace(command)) return;
        
        // Don't add if it's the same as the last command
        if (history.Count > 0 && history[^1] == command) return;
        
        history.Add(command);
        currentIndex = -1; // Reset index after adding new command
    }
    
    public void NavigateHistory(int direction, string currentInput)
    {
        if (history.Count == 0) return;
        
        // Save current input when starting navigation
        if (currentIndex == -1 && direction < 0)
        {
            tempInput = currentInput;
        }
        
        // Calculate new index
        if (direction < 0) // Up
        {
            if (currentIndex == -1)
            {
                currentIndex = history.Count - 1;
            }
            else
            {
                currentIndex = Mathf.Max(0, currentIndex - 1);
            }
            HistoryChanged?.Invoke(history[currentIndex]);
        }
        else // Down
        {
            if (currentIndex >= 0)
            {
                currentIndex++;
                if (currentIndex >= history.Count)
                {
                    currentIndex = -1;
                    HistoryChanged?.Invoke(tempInput);
                }
                else
                {
                    HistoryChanged?.Invoke(history[currentIndex]);
                }
            }
        }
    }
    
    public void Reset()
    {
        currentIndex = -1;
        tempInput = string.Empty;
    }
    
    public bool IsNavigatingHistory => currentIndex != -1;
    
    public void Clear()
    {
        history.Clear();
        Reset();
    }
    
    public List<string> GetHistory()
    {
        return new List<string>(history);
    }
}
