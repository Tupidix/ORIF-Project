using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Generates and displays the end-of-game report showing per-menu results.
/// </summary>
public class EndGameReport : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _txtScenarioName;
    [SerializeField] private TextMeshProUGUI _txtCountdown;
    [SerializeField] private TextMeshProUGUI _txtTimer;
    [SerializeField] private GameObject _endGameMenuReport;
    [SerializeField] private GameObject _endGameMenuReportRow;
    [SerializeField] private GameObject _pnlEndGameReport;

    /// <summary>
    /// Generates the complete end-game report UI for all menus.
    /// </summary>
    /// <param name="scenario">The scenario that was played.</param>
    /// <param name="countdownInSeconds">The total countdown time used, in seconds.</param>
    /// <param name="timerInSeconds">The overtime elapsed, in seconds.</param>
    /// <param name="actions">The list of action lists, one per menu.</param>
    /// <param name="report">The parent report GameObject for positioning.</param>
    public void GenerateEndGameReport(Scenario scenario, float countdownInSeconds, float timerInSeconds, List<List<ScenarioAction>> actions, GameObject report)
    {
        print("GenerateEndGameReport");
        UpdateHeader(scenario, countdownInSeconds, timerInSeconds);
        for (int menuIndex = 0; menuIndex < scenario.menus.Length; menuIndex++)
        {
            var xPos = CalculateMenuXPosition(scenario.menus.Length, menuIndex);
            var reportMenu = CreateReportMenu(report, xPos, menuIndex);
            SetReportMenuName(reportMenu, scenario.menus[menuIndex]);
            PopulateReportMenuRows(reportMenu, actions, scenario, menuIndex);
        }
    }

    /// <summary>
    /// Updates the report header with scenario name, countdown and timer values.
    /// </summary>
    /// <param name="scenario">The scenario data.</param>
    /// <param name="countdownInSeconds">The countdown duration in seconds.</param>
    /// <param name="timerInSeconds">The overtime duration in seconds.</param>
    private void UpdateHeader(Scenario scenario, float countdownInSeconds, float timerInSeconds)
    {
        _txtScenarioName.text = scenario.name;
        _txtCountdown.text = Helper.FormatTime(countdownInSeconds);
        _txtTimer.text = IsExamMode(scenario) ? "" : "+" + Helper.FormatTime(timerInSeconds);
    }

    /// <summary>
    /// Returns whether the scenario is in exam mode.
    /// </summary>
    /// <param name="scenario">The scenario to check.</param>
    /// <returns>True if the scenario mode is Exam.</returns>
    private bool IsExamMode(Scenario scenario)
    {
        return scenario.mode == Scenario.Mode.Exam;
    }

    /// <summary>
    /// Calculates the horizontal position for a menu report panel.
    /// </summary>
    /// <param name="menuCount">The total number of menus.</param>
    /// <param name="index">The index of the current menu.</param>
    /// <returns>The X position offset.</returns>
    private float CalculateMenuXPosition(int menuCount, int index)
    {
        if (menuCount == 1) return 0.55f;
        return index % 2 != 0 ? 0.9f : -0.45f;
    }

    /// <summary>
    /// Instantiates and positions a menu report panel.
    /// </summary>
    /// <param name="report">The parent report object for positioning reference.</param>
    /// <param name="xPos">The horizontal offset multiplier.</param>
    /// <param name="index">The menu index.</param>
    /// <returns>The instantiated menu report GameObject.</returns>
    private GameObject CreateReportMenu(GameObject report, float xPos, int index)
    {
        var menu = Instantiate(_endGameMenuReport);
        menu.transform.SetParent(_pnlEndGameReport.transform);
        menu.transform.position = new Vector3(
            report.transform.position.x + xPos * (index + 0.5f),
            2.65f,
            report.transform.position.z
        );
        return menu;
    }

    /// <summary>
    /// Sets the menu name text in a report menu panel.
    /// </summary>
    /// <param name="reportMenu">The report menu GameObject.</param>
    /// <param name="menu">The menu data source.</param>
    private void SetReportMenuName(GameObject reportMenu, Menu menu)
    {
        foreach (var text in reportMenu.GetComponentsInChildren<TextMeshProUGUI>(true))
        {
            if (text.name == "txtMenuName")
                text.text = menu.name;
        }
    }

    /// <summary>
    /// Creates and populates all rows for a single menu in the report.
    /// </summary>
    /// <param name="reportMenu">The parent menu panel.</param>
    /// <param name="actions">All actions across all menus.</param>
    /// <param name="scenario">The scenario data.</param>
    /// <param name="menuIndex">The index of the menu to populate.</param>
    private void PopulateReportMenuRows(GameObject reportMenu, List<List<ScenarioAction>> actions, Scenario scenario, int menuIndex)
    {
        int rowIndex = 0;
        foreach (var action in Helper.GetLastActionsOfEachType(menuIndex, actions))
        {
            var row = CreateReportRow(reportMenu, menuIndex, rowIndex);
            FillReportRow(row, action, actions[menuIndex], scenario, menuIndex);
            rowIndex++;
        }
    }

    /// <summary>
    /// Instantiates and positions a single report row.
    /// </summary>
    /// <param name="reportMenu">The parent menu panel.</param>
    /// <param name="menuIndex">The menu index for horizontal offset.</param>
    /// <param name="rowIndex">The row index for vertical offset.</param>
    /// <returns>The instantiated row GameObject.</returns>
    private GameObject CreateReportRow(GameObject reportMenu, int menuIndex, int rowIndex)
    {
        var row = Instantiate(_endGameMenuReportRow);
        row.transform.SetParent(reportMenu.transform);
        row.transform.position = new Vector3(
            reportMenu.transform.position.x - (menuIndex + 1) * -0.045f,
            reportMenu.transform.position.y + (rowIndex + 0.5f) * -0.145f,
            reportMenu.transform.position.z
        );
        return row;
    }

    /// <summary>
    /// Fills a report row with the cutlery name and evaluation result.
    /// </summary>
    /// <param name="row">The row GameObject to fill.</param>
    /// <param name="action">The latest action for this cutlery.</param>
    /// <param name="allActions">All actions recorded for this menu.</param>
    /// <param name="scenario">The scenario data.</param>
    /// <param name="menuIndex">The menu index.</param>
    private void FillReportRow(GameObject row, ScenarioAction action, List<ScenarioAction> allActions, Scenario scenario, int menuIndex)
    {
        foreach (var text in row.GetComponentsInChildren<TextMeshProUGUI>(true))
        {
            if (text.name == "txtCutlery")
                text.text = Helper.ConvertTagToFrenchName(action.tag);
            if (text.name == "txtTry")
                text.text = EvaluateActionResult(action, allActions, scenario, menuIndex);
        }
    }

    /// <summary>
    /// Evaluates the result of an action: "Intrus", "Incorrect", or the attempt count.
    /// </summary>
    /// <param name="action">The action to evaluate.</param>
    /// <param name="allActions">All actions for this menu.</param>
    /// <param name="scenario">The scenario data for cutlery lookup.</param>
    /// <param name="menuIndex">The menu index.</param>
    /// <returns>A result string for display.</returns>
    private string EvaluateActionResult(ScenarioAction action, List<ScenarioAction> allActions, Scenario scenario, int menuIndex)
    {
        if (!IsCutleryInMenu(action.tag, scenario, menuIndex))
            return "Intrus";
        int attemptCount = CountAttemptsForTag(action.tag, allActions) - 1;
        if (IsActionIncorrect(action, attemptCount))
            return "Incorrect";
        return attemptCount.ToString();
    }

    /// <summary>
    /// Returns whether a cutlery tag exists in the specified menu.
    /// </summary>
    /// <param name="tag">The cutlery tag to look for.</param>
    /// <param name="scenario">The scenario data.</param>
    /// <param name="menuIndex">The menu index.</param>
    /// <returns>True if the tag is part of the menu.</returns>
    private bool IsCutleryInMenu(string tag, Scenario scenario, int menuIndex)
    {
        return scenario.menus[menuIndex].cutleryToPlace.ContainsKey(tag);
    }

    /// <summary>
    /// Returns whether an action is considered incorrect (zero attempts or any validation failure).
    /// </summary>
    /// <param name="action">The action to check.</param>
    /// <param name="attemptCount">The number of attempts (excluding the default).</param>
    /// <returns>True if the action is incorrect.</returns>
    private bool IsActionIncorrect(ScenarioAction action, int attemptCount)
    {
        return attemptCount == 0 || !action.isOrderCorrect || !action.isRotationCorrect || !action.isPositionCorrect;
    }

    /// <summary>
    /// Counts how many actions match a given tag in the action list.
    /// </summary>
    /// <param name="tag">The tag to count occurrences of.</param>
    /// <param name="actions">The list of actions to search.</param>
    /// <returns>The number of matching actions.</returns>
    private int CountAttemptsForTag(string tag, List<ScenarioAction> actions)
    {
        int count = 0;
        foreach (var action in actions)
        {
            if (action.tag == tag)
                count++;
        }
        return count;
    }
}
