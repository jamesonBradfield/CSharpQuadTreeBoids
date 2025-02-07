using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

public partial class ConsoleAutoComplete : PanelContainer
{
    private List<string> suggestions = new List<string>();
    private ItemList suggestionList;
    private int selectedIndex = -1;
    private const int MAX_HEIGHT = 200;
    private const int ITEM_PADDING = 4;

    public event Action<string> SuggestionSelected;

    public override void _Ready()
    {
        SetupSuggestionList();
    }

    private void SetupSuggestionList()
    {
        suggestionList = new ItemList
        {
            SelectMode = ItemList.SelectModeEnum.Single,
            AllowReselect = true,
            AutoHeight = true,
            CustomMinimumSize = new Vector2(0, 0),
            FocusMode = Control.FocusModeEnum.None  
        };

        AddChild(suggestionList);

        Hide();
    }

    private float CalculateContentHeight()
    {
        if (suggestionList.ItemCount == 0) return 0;

        var theme = suggestionList.Theme;
        if (theme == null) return suggestionList.ItemCount * (20 + ITEM_PADDING * 2);
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
        return itemHeight * suggestionList.ItemCount;
    }

    public void UpdateSuggestions(string input, IEnumerable<string> availableCommands)
    {
        if (suggestionList == null) return;

        suggestions?.Clear();
        suggestionList.Clear();
        selectedIndex = -1;

        if (string.IsNullOrEmpty(input) || availableCommands == null)
        {
            Hide();
            return;
        }

        suggestions = availableCommands
            .Where(cmd => cmd != null && cmd.StartsWith(input, StringComparison.OrdinalIgnoreCase))
            .OrderBy(cmd => cmd)
            .ToList();

        foreach (var suggestion in suggestions)
        {
            suggestionList.AddItem(suggestion);
        }

        if (suggestions.Count > 0)
        {
            Show();
            float contentHeight = CalculateContentHeight();
            suggestionList.CustomMinimumSize = new Vector2(0, Mathf.Min(contentHeight, MAX_HEIGHT));

            UpdateSelection(0);
        }
        else
        {
            Hide();
        }
    }

    // New method to handle visual selection without focus change
    private void UpdateSelection(int index)
    {
        if (index >= 0 && index < suggestions.Count)
        {
            // Deselect the current item if one is selected
            if (selectedIndex >= 0 && selectedIndex < suggestions.Count)
            {
                suggestionList.Deselect(selectedIndex);
            }

            selectedIndex = index;
            suggestionList.Select(selectedIndex);
            suggestionList.EnsureCurrentIsVisible();
        }
    }

    public void NavigateSuggestions(int direction)
    {
        if (suggestions.Count == 0) return;

        int newIndex = (selectedIndex + direction + suggestions.Count) % suggestions.Count;
        UpdateSelection(newIndex);
    }

    public string GetSelectedSuggestion()
    {
        return selectedIndex >= 0 && selectedIndex < suggestions.Count
            ? suggestions[selectedIndex]
            : null;
    }

    // New method to handle suggestion acceptance
    public void AcceptSuggestion()
    {
        string selected = GetSelectedSuggestion();
        if (selected != null)
        {
            SuggestionSelected?.Invoke(selected);
        }
    }

    public bool HasSuggestions => suggestions.Count > 0;

    public void CancelSuggestions()
    {
        suggestions.Clear();
        suggestionList.Clear();
        selectedIndex = -1;
        Hide();
    }
}
