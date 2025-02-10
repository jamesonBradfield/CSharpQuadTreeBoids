using Godot;
using System;
using System.Collections.Generic;

public partial class ConsoleHistory : Node
{
    private readonly List<string> _history = new();
    private int _currentIndex = -1;
    private string _tempInput = string.Empty;

    public event Action<string> HistoryChanged;

    public void AddCommand(string command)
    {
        if (string.IsNullOrWhiteSpace(command) || (_history.Count > 0 && _history[^1] == command))
            return;

        _history.Add(command);
        _currentIndex = -1;
    }

    public void Navigate(int direction, string currentInput)
    {
        if (_history.Count == 0) return;

        if (_currentIndex == -1 && direction < 0)
            _tempInput = currentInput;

        _currentIndex = direction switch
        {
            -1 => Mathf.Clamp(_currentIndex == -1 ? _history.Count - 1 : _currentIndex - 1, 0, _history.Count - 1),
            1 => _currentIndex >= 0 ? Mathf.Min(++_currentIndex, _history.Count) : -1,
            _ => _currentIndex
        };

        HistoryChanged?.Invoke(_currentIndex >= 0 ? _history[_currentIndex] : _tempInput);
    }

    public void Reset() => (_currentIndex, _tempInput) = (-1, string.Empty);
    public void Clear()
    {
        _history.Clear();
        _currentIndex = -1;
        _tempInput = string.Empty;
    }
    public List<string> GetHistory() => new(_history);
}
