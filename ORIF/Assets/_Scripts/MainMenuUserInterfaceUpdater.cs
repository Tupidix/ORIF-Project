using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUserInterfaceUpdater : MonoBehaviour
{
    [SerializeField] private Scenario _scenario;
    [SerializeField] private TextMeshProUGUI txtScenarioName;
    [SerializeField] private TextMeshProUGUI txtScenarioDescription;
    [SerializeField] private TMP_Dropdown drdMode;
    [SerializeField] private TMP_Dropdown drdLevel;
    [SerializeField] private TextMeshProUGUI txtLevel;
    [SerializeField] private Toggle cbxPosition;
    [SerializeField] private Toggle cbxRotation;
    [SerializeField] private Toggle cbxOrder;
    [SerializeField] private Slider rngTimer;

    public Scenario Scenario { get => _scenario; set => _scenario = value; }

    /// <summary>
    /// Refreshes all main menu UI fields from the current Scenario data.
    /// </summary>
    public void UpdateUI()
    {
        print("Update UI main menu");
        txtScenarioName.text = Scenario.name;
        txtScenarioDescription.text = Scenario.description;
        drdMode.value = (int)Scenario.mode;
        drdLevel.value = (int)Scenario.level;
        cbxPosition.isOn = Scenario.validator.position;
        cbxRotation.isOn = Scenario.validator.rotation;
        cbxOrder.isOn = Scenario.validator.order;
        rngTimer.value = Scenario.allowedTime;
    }

    /// <summary>
    /// Updates the scenario mode and applies the corresponding level constraint.
    /// </summary>
    /// <param name="mode">The integer value of the selected mode.</param>
    public void UpdateUIMode(int mode)
    {
        Scenario.mode = (Scenario.Mode)mode;
        ApplyLevelConstraintFromMode(Scenario.mode);
    }

    /// <summary>
    /// Hides the level dropdown and its label.
    /// </summary>
    public void HideUILevel()
    {
        SetLevelDropdownVisible(false);
    }

    /// <summary>
    /// Shows the level dropdown and its label.
    /// </summary>
    public void ShowUILevel()
    {
        SetLevelDropdownVisible(true);
    }

    /// <summary>
    /// Toggles visibility of the level dropdown and its associated label.
    /// </summary>
    /// <param name="visible">True to show, false to hide.</param>
    private void SetLevelDropdownVisible(bool visible)
    {
        drdLevel.gameObject.SetActive(visible);
        txtLevel.gameObject.SetActive(visible);
    }

    /// <summary>
    /// Builds and returns a Scenario populated from the current UI state.
    /// </summary>
    /// <returns>A new Scenario reflecting the user's UI selections.</returns>
    public Scenario GetInformations()
    {
        var scenario = BuildScenarioFromUI();
        ApplyLevelConstraintFromMode(scenario.mode);
        return scenario;
    }

    /// <summary>
    /// Constructs a new Scenario object from the current UI field values.
    /// </summary>
    /// <returns>A Scenario populated with the UI state.</returns>
    private Scenario BuildScenarioFromUI()
    {
        var scenario = new Scenario
        {
            name = txtScenarioName.text,
            description = txtScenarioDescription.text,
            mode = (Scenario.Mode)drdMode.value,
            level = (Scenario.Level)drdLevel.value,
            menus = Scenario.menus,
            validator = BuildValidator(),
            allowedTime = rngTimer.value,
            drinks = Scenario.drinks,
            extras = Scenario.extras
        };

        ApplyLevelConstraintFromMode(scenario.mode);
        return scenario;
    }

    /// <summary>
    /// Builds a Validator from the current checkbox states.
    /// </summary>
    /// <returns>A Validator with position, rotation and order flags.</returns>
    private Validator BuildValidator()
    {
        return new Validator
        {
            position = cbxPosition.isOn,
            rotation = cbxRotation.isOn,
            order = cbxOrder.isOn
        };
    }

    /// <summary>
    /// Forces the scenario level to match mandatory constraints based on the mode.
    /// Learning forces Beginner, Exam forces Expert.
    /// </summary>
    /// <param name="mode">The scenario mode to apply constraints for.</param>
    private void ApplyLevelConstraintFromMode(Scenario.Mode mode)
    {
        if (mode == Scenario.Mode.Learning)
            Scenario.level = Scenario.Level.Begginner;
        if (mode == Scenario.Mode.Exam)
            Scenario.level = Scenario.Level.Expert;
    }
}