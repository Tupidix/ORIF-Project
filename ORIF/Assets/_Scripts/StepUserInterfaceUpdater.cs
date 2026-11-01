using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StepUserInterfaceUpdater : MonoBehaviour
{
    public class Step
    {
        public string itemName;
        public string itemDescription;
        public int currentStep;

        public Step(string name, string desc, int current)
        {
            itemName = name;
            itemDescription = desc;
            currentStep = current;
        }
    }

    [SerializeField] private Scenario _scenario;
    [SerializeField] private TextMeshProUGUI txtMenuName;
    [SerializeField] private TextMeshProUGUI txtItemName;
    [SerializeField] private TextMeshProUGUI txtDescription;
    [SerializeField] private TextMeshProUGUI txtCurrentStep;
    [SerializeField] private TextMeshProUGUI txtTotalStep;

    [SerializeField] private int stepIndex = 0;
    [SerializeField] private int totalSteps = 0;

    public int TotalSteps => totalSteps;
    public void SetStepIndex(int index) => stepIndex = index;

    private List<Step> _steps;
    private string _currentMenuName;

    /// <summary>
    /// Generates all placement steps for a given menu in the scenario.
    /// </summary>
    /// <param name="scenario">The active scenario.</param>
    /// <param name="menuIndex">The index of the menu to generate steps for.</param>
    public void GenerateSteps(Scenario scenario, int menuIndex)
    {
        totalSteps = scenario.menus[menuIndex].cutleryToPlace.Count;
        _steps = new List<Step>();
        _currentMenuName = scenario.menus[menuIndex].name;

        foreach (var cutlery in scenario.menus[menuIndex].cutleryToPlace)
        {
            var step = BuildStep(cutlery, menuIndex);
            _steps.Add(step);
        }
    }

    /// <summary>
    /// Builds a single Step from a cutlery entry.
    /// </summary>
    /// <param name="cutlery">The cutlery key-value pair from the menu.</param>
    /// <param name="menuIndex">The current menu index for position computation.</param>
    /// <returns>A new Step instance.</returns>
    private Step BuildStep(KeyValuePair<string, Cutlery> cutlery, int menuIndex)
    {
        string description = BuildStepDescription(cutlery, menuIndex);
        return new Step(cutlery.Value.tag, description, cutlery.Value.placementOrderIndex);
    }

    /// <summary>
    /// Builds the French description text for a single cutlery placement step.
    /// </summary>
    /// <param name="cutlery">The cutlery key-value pair.</param>
    /// <param name="menuIndex">The current menu index for position computation.</param>
    /// <returns>A French description string.</returns>
    private string BuildStepDescription(KeyValuePair<string, Cutlery> cutlery, int menuIndex)
    {
        string positionText = GetPositionText(cutlery, menuIndex);
        string parentName = Helper.ConvertTagToFrenchName(cutlery.Value.parent);
        return $"L'objet est plac� {positionText} par rapport � {parentName}";
    }

    /// <summary>
    /// Returns the position text for a cutlery relative to its parent.
    /// Returns "au bord" if the parent is the table.
    /// </summary>
    /// <param name="cutlery">The cutlery key-value pair.</param>
    /// <param name="menuIndex">The menu index for direction computation.</param>
    /// <returns>A French position description string.</returns>
    private string GetPositionText(KeyValuePair<string, Cutlery> cutlery, int menuIndex)
    {
        if (cutlery.Value.parent == "Table")
            return "au bord";

        var tmp = CreateTemporaryTransformObject(cutlery.Value.offset.position, cutlery.Value.offset.rotation);
        return Helper.GetPositionDescription(tmp.transform, menuIndex);
    }

    /// <summary>
    /// Creates a temporary GameObject with the given position and rotation for offset computation.
    /// </summary>
    /// <param name="position">The local position to set.</param>
    /// <param name="rotation">The local rotation to set.</param>
    /// <returns>The temporary GameObject.</returns>
    private GameObject CreateTemporaryTransformObject(Vector3 position, Quaternion rotation)
    {
        var tmp = new GameObject("tmp");
        tmp.transform.position = position;
        tmp.transform.rotation = rotation;
        return tmp;
    }

    /// <summary>
    /// Refreshes all step UI text fields from the current step data.
    /// </summary>
    public void UpdateUI()
    {
        print("Update UI step");
        txtMenuName.text = _currentMenuName;
        txtItemName.text = Helper.ConvertTagToFrenchName(CurrentStep.itemName);
        txtDescription.text = CurrentStep.itemDescription;
        txtCurrentStep.text = (stepIndex + 1).ToString();
        txtTotalStep.text = totalSteps.ToString();
    }

    /// <summary>
    /// Returns the step at the current step index.
    /// </summary>
    private Step CurrentStep => _steps[stepIndex];

    /// <summary>
    /// Returns the full list of generated steps.
    /// </summary>
    /// <returns>The list of Step objects.</returns>
    public List<Step> GetSteps()
    {
        return _steps;
    }
}