using System;
using System.Collections.Generic;
using Godot;

public partial class ConsoleHistoryDisplay : PanelContainer
{
    private ItemList _list;
    private int _selectedIndex = -1;

    public event Action<string> HistorySelected;

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

    public void ShowHistory(IReadOnlyList<string> history)
    {
        _list.Clear();
        for (int i = history.Count - 1; i >= 0; i--)
            _list.AddItem(history[i]);

        Visible = history.Count > 0;
        UpdateSelection(0);
    }

    public void Navigate(int direction)
    {
        if (_list.ItemCount == 0) return;
        UpdateSelection((_selectedIndex + direction + _list.ItemCount) % _list.ItemCount);
        HistorySelected?.Invoke(_list.GetItemText(_selectedIndex));
    }

    private void UpdateSelection(int index)
    {
        if (index == _selectedIndex || index < 0 || index >= _list.ItemCount) return;

        _list.Deselect(_selectedIndex);
        _list.Select(_selectedIndex = index);
        _list.EnsureCurrentIsVisible();
    }

    public void Accept() => HistorySelected?.Invoke(_list.GetItemText(_selectedIndex));
    public void Cancel() => Hide();
}
