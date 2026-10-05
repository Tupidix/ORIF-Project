using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Generates and updates the in-game mission recap panel showing per-item validation status.
/// </summary>
public class MissionRecap : MonoBehaviour
{
    [SerializeField] private GameObject _gameManager;
    [SerializeField] private TextMeshProUGUI _scenarioName;
    [SerializeField] private TextMeshProUGUI _menuName;
    [SerializeField] private GameObject _rowPrefab;

    private const float RowStep = 0.208f;
    private const float RowStartingY = -0.77f;

    /// <summary>
    /// Sets the initial report header from the scenario and menu name.
    /// </summary>
    /// <param name="scenario">The active scenario.</param>
    /// <param name="menuName">The name of the menu to display.</param>
    public void generateReport(Scenario scenario, string menuName)
    {
        _scenarioName.text = scenario.name;
        _menuName.text = menuName;
    }

    /// <summary>
    /// Rebuilds the full report: header, rows and validation toggles.
    /// </summary>
    /// <param name="scenario">The active scenario.</param>
    /// <param name="menuName">The menu name to display.</param>
    /// <param name="actions">The list of actions to display as rows.</param>
    /// <param name="contentArea">The RectTransform used as parent for row positioning.</param>
    /// <param name="menuIndex">The index of the menu being reported.</param>
    public void UpdateReport(Scenario scenario, string menuName, List<ScenarioAction> actions, RectTransform contentArea, int menuIndex)
    {
        if (_rowPrefab == null)
        {
            print("Row prefab is null...");
            return;
        }
        UpdateHeader(scenario.name, menuName);
        ClearExistingRows(menuIndex);
        CreateRows(scenario, actions, contentArea, menuIndex);
    }

    /// <summary>
    /// Updates the header text fields.
    /// </summary>
    /// <param name="scenarioName">The scenario name text.</param>
    /// <param name="menuName">The menu name text.</param>
    private void UpdateHeader(string scenarioName, string menuName)
    {
        _scenarioName.text = scenarioName;
        _menuName.text = menuName;
    }

    /// <summary>
    /// Destroys all existing report rows for the given menu index.
    /// </summary>
    /// <param name="menuIndex">The menu index whose rows should be cleared.</param>
    private void ClearExistingRows(int menuIndex)
    {
        string rowTag = GetRowTag(menuIndex);
        foreach (var row in GameObject.FindGameObjectsWithTag(rowTag))
        {
            Destroy(row);
        }
    }

    /// <summary>
    /// Returns the tag string used to identify rows of a specific menu.
    /// </summary>
    /// <param name="menuIndex">The menu index.</param>
    /// <returns>A tag string in the form "ReportRowMenuN".</returns>
    private string GetRowTag(int menuIndex)
    {
        return "ReportRowMenu" + menuIndex;
    }

    /// <summary>
    /// Creates report rows for all valid actions in the menu.
    /// </summary>
    /// <param name="scenario">The scenario data for cutlery lookup.</param>
    /// <param name="actions">The actions to create rows for.</param>
    /// <param name="contentArea">The parent RectTransform for positioning.</param>
    /// <param name="menuIndex">The menu index.</param>
    private void CreateRows(Scenario scenario, List<ScenarioAction> actions, RectTransform contentArea, int menuIndex)
    {
        var position = new Vector2(0, RowStartingY);
        foreach (var action in actions)
        {
            if (!IsActionInMenu(action.tag, scenario, menuIndex)) continue;
            CreateRow(action, scenario, contentArea, menuIndex, ref position);
        }
    }

    /// <summary>
    /// Returns whether the given tag belongs to the specified menu's cutlery list.
    /// </summary>
    /// <param name="tag">The cutlery tag to check.</param>
    /// <param name="scenario">The scenario data.</param>
    /// <param name="menuIndex">The menu index.</param>
    /// <returns>True if the tag is in the menu.</returns>
    private bool IsActionInMenu(string tag, Scenario scenario, int menuIndex)
    {
        return scenario.menus[menuIndex].cutleryToPlace.ContainsKey(tag);
    }

    /// <summary>
    /// Creates a single report row, instantiates it, fills its content, and advances the position.
    /// </summary>
    /// <param name="action">The action data to display.</param>
    /// <param name="scenario">The scenario for validator flags.</param>
    /// <param name="contentArea">The parent RectTransform.</param>
    /// <param name="menuIndex">The menu index for tagging.</param>
    /// <param name="position">The current anchored position, advanced after creation.</param>
    private void CreateRow(ScenarioAction action, Scenario scenario, RectTransform contentArea, int menuIndex, ref Vector2 position)
    {
        if (action == null)
        {
            print("Action is null");
            return;
        }
        var row = InstantiateRow(contentArea, menuIndex, position);
        if (row == null) return;
        FillRowContent(row, action, scenario);
        position.x -= RowStep;
    }

    /// <summary>
    /// Instantiates a row prefab, tags it, and sets its anchored position.
    /// </summary>
    /// <param name="contentArea">The parent RectTransform.</param>
    /// <param name="menuIndex">The menu index for row tagging.</param>
    /// <param name="position">The anchored position for the row.</param>
    /// <returns>The instantiated row, or null if the RectTransform is missing.</returns>
    private GameObject InstantiateRow(RectTransform contentArea, int menuIndex, Vector2 position)
    {
        var row = Instantiate(_rowPrefab, contentArea);
        row.tag = GetRowTag(menuIndex);
        row.transform.SetParent(this.transform);
        var rectTransform = row.GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            print("RectTransform is null");
            return null;
        }
        rectTransform.anchoredPosition = position;
        return row;
    }

    /// <summary>
    /// Fills a row with item name and validation toggle states.
    /// </summary>
    /// <param name="row">The row GameObject.</param>
    /// <param name="action">The action data.</param>
    /// <param name="scenario">The scenario for validator flags.</param>
    private void FillRowContent(GameObject row, ScenarioAction action, Scenario scenario)
    {
        SetRowItemName(row, action.tag);
        SetRowToggle(row, "cbxPosition", scenario.validator.position, action.isPositionCorrect);
        SetRowToggle(row, "cbxRotation", scenario.validator.rotation, action.isRotationCorrect);
        SetRowToggle(row, "cbxOrder", scenario.validator.order, action.isOrderCorrect);
    }

    /// <summary>
    /// Sets the item name text in a report row.
    /// </summary>
    /// <param name="row">The row GameObject.</param>
    /// <param name="tag">The cutlery tag to convert to a French name.</param>
    private void SetRowItemName(GameObject row, string tag)
    {
        var itemName = FindChildComponent<TextMeshProUGUI>(row, "txtItemName");
        if (itemName == null) return;
        itemName.text = Helper.ConvertTagToFrenchName(tag);
    }

    /// <summary>
    /// Sets a toggle in a report row based on validation state.
    /// Disables the toggle if the validator flag is off.
    /// </summary>
    /// <param name="row">The row GameObject.</param>
    /// <param name="childName">The name of the Toggle child.</param>
    /// <param name="validatorEnabled">Whether the validator for this criterion is active.</param>
    /// <param name="isCorrect">Whether the action passed this criterion.</param>
    private void SetRowToggle(GameObject row, string childName, bool validatorEnabled, bool isCorrect)
    {
        var toggle = FindChildComponent<Toggle>(row, childName);
        if (toggle == null) return;
        if (validatorEnabled)
            toggle.isOn = isCorrect;
        else
            toggle.interactable = false;
    }

    /// <summary>
    /// Finds a component of type T on a named child of the given parent.
    /// </summary>
    /// <typeparam name="T">The component type to retrieve.</typeparam>
    /// <param name="parent">The parent GameObject.</param>
    /// <param name="childName">The name of the child to search for.</param>
    /// <returns>The component, or null if the child or component is not found.</returns>
    private T FindChildComponent<T>(GameObject parent, string childName) where T : Component
    {
        var child = parent.transform.Find(childName);
        if (child == null)
        {
            print($"{childName} is null");
            return null;
        }
        return child.GetComponent<T>();
    }
}
