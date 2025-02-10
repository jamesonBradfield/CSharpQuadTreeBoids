using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class ConsoleAutoComplete : PanelContainer
{
    private ItemList _list;
    private int _selectedIndex = -1;

    public event Action<string> SuggestionSelected;

    public override void _Ready() => SetupList();

    private void SetupList()
    {
        _list = new ItemList
        {
            SelectMode = ItemList.SelectModeEnum.Single,
            FocusMode = Control.FocusModeEnum.None
        };
        AddChild(_list);
        Hide();
    }
    public string GetSelectedText() =>
        _selectedIndex >= 0 && _selectedIndex < _list.ItemCount
            ? _list.GetItemText(_selectedIndex)
            : null;
    public void UpdateSuggestions(string input, IEnumerable<string> commands)
    {
        _list.Clear();
        Visible = false;

        if (string.IsNullOrEmpty(input)) return;

        var matches = commands
            .Where(c => c?.StartsWith(input, StringComparison.OrdinalIgnoreCase) ?? false)
            .OrderBy(c => c)
            .ToArray();

        foreach (var match in matches)
            _list.AddItem(match);

        if (matches.Length > 0)
        {
            UpdateSelection(0);
            Visible = true;
        }
    }

    public void Navigate(int direction)
    {
        if (_list.ItemCount == 0) return;
        UpdateSelection((_selectedIndex + direction + _list.ItemCount) % _list.ItemCount);
    }

    private void UpdateSelection(int index)
    {
        if (index == _selectedIndex || index < 0 || index >= _list.ItemCount) return;

        _list.Deselect(_selectedIndex);
        _list.Select(_selectedIndex = index);
        _list.EnsureCurrentIsVisible();
    }

    public void Accept() => SuggestionSelected?.Invoke(_list.GetItemText(_selectedIndex));
    public void Cancel() => Hide();
}
