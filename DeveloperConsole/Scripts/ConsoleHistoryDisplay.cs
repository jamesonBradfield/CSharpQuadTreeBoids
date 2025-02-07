using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class ConsoleHistoryDisplay : PanelContainer
{
    private ItemList historyList;
    private int selectedIndex = -1;
    private const int MAX_HEIGHT = 200;
    private const int ITEM_PADDING = 4;
    
    public event Action<string> HistorySelected;

    public override void _Ready()
    {
        SetupHistoryList();
    }

    private void SetupHistoryList()
    {
        historyList = new ItemList
        {
            SelectMode = ItemList.SelectModeEnum.Single,
            AllowReselect = true,
            AutoHeight = true,
            CustomMinimumSize = new Vector2(0, 0),
            FocusMode = Control.FocusModeEnum.None
        };
        
        AddChild(historyList);
        Hide();
    }

    private float CalculateContentHeight()
    {
        if (historyList.ItemCount == 0) return 0;

        var theme = historyList.Theme;
        if (theme == null) return historyList.ItemCount * (20 + ITEM_PADDING * 2);

        float fontSize;
        try
        {
            fontSize = theme.DefaultFontSize;
        }
        catch
        {
            fontSize = 16;
        }

        float itemHeight = fontSize + (ITEM_PADDING * 2);
        return itemHeight * historyList.ItemCount;
    }

    public void UpdateHistory(List<string> history)
    {
        if (historyList == null) return;

        historyList.Clear();
        selectedIndex = -1;

        if (history == null || history.Count == 0)
        {
            Hide();
            return;
        }

        // Add items in reverse order (newest first)
        for (int i = history.Count - 1; i >= 0; i--)
        {
            historyList.AddItem(history[i]);
        }

        Show();
        float contentHeight = CalculateContentHeight();
        historyList.CustomMinimumSize = new Vector2(0, Mathf.Min(contentHeight, MAX_HEIGHT));
        
        // Select the most recent command
        UpdateSelection(0);
    }

    private void UpdateSelection(int index)
    {
        if (index >= 0 && index < historyList.ItemCount)
        {
            if (selectedIndex >= 0 && selectedIndex < historyList.ItemCount)
            {
                historyList.Deselect(selectedIndex);
            }
            
            selectedIndex = index;
            historyList.Select(selectedIndex);
            historyList.EnsureCurrentIsVisible();
        }
    }

    public void NavigateHistory(int direction)
    {
        if (historyList.ItemCount == 0) return;

        int newIndex = (selectedIndex + direction + historyList.ItemCount) % historyList.ItemCount;
        UpdateSelection(newIndex);
        
        string selected = GetSelectedHistory();
        if (selected != null)
        {
            HistorySelected?.Invoke(selected);
        }
    }

    public string GetSelectedHistory()
    {
        return selectedIndex >= 0 && selectedIndex < historyList.ItemCount 
            ? historyList.GetItemText(selectedIndex) 
            : null;
    }

    public void AcceptHistory()
    {
        string selected = GetSelectedHistory();
        if (selected != null)
        {
            HistorySelected?.Invoke(selected);
        }
        Hide();
    }

    public bool IsVisible => Visible;

    public void CancelDisplay()
    {
        selectedIndex = -1;
        Hide();
    }
}
