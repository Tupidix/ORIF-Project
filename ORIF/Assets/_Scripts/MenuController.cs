using UnityEngine;

/// <summary>Coordinates menu panels and tips; scenario content belongs to the scenario picker.</summary>
public sealed class MenuController
{
    private readonly GameObject _main;
    private readonly GameObject _picker;
    private readonly GameObject _tips;
    private readonly GameObject _learningTips;
    private readonly GameObject _examTips;
    private readonly ScenarioSelectionController _selection;

    public MenuController(GameObject main, GameObject picker, GameObject tips,
        GameObject learningTips, GameObject examTips, ScenarioSelectionController selection)
    {
        _main = main;
        _picker = picker;
        _tips = tips;
        _learningTips = learningTips;
        _examTips = examTips;
        _selection = selection;
    }

    public void ShowMain()
    {
        _main.SetActive(true);
        _picker.SetActive(false);
        _tips.SetActive(false);
    }

    public void ShowScenarioPicker()
    {
        _main.SetActive(false);
        _picker.SetActive(true);
        _selection.SelectDefaultLocal();
    }

    public void ShowTips()
    {
        Hide();
        _tips.SetActive(true);
    }

    public void Hide()
    {
        _main.SetActive(false);
        _picker.SetActive(false);
        _tips.SetActive(false);
    }

    public void HideScenarioTips()
    {
        _learningTips.SetActive(false);
        _examTips.SetActive(false);
    }

    public bool ShowScenarioTips(Scenario.Mode mode)
    {
        HideScenarioTips();
        if (mode == Scenario.Mode.Learning)
            _learningTips.SetActive(true);
        else if (mode == Scenario.Mode.Exam)
            _examTips.SetActive(true);
        else
            return false;
        return true;
    }

    public void Position(Camera camera, float distance)
    {
        Helper.PlaceUIElement(_main, camera, distance, 1.5f);
        Helper.PlaceUIElement(_picker, camera, distance, 1.515f);
        Helper.PlaceUIElement(_tips, camera, distance, 1.5f);
        Helper.PlaceUIElement(_learningTips, camera, distance, 1.5f);
        Helper.PlaceUIElement(_examTips, camera, distance, 1.5f);
    }

    public void FacePlayer()
    {
        Helper.RotateUIElement(_main, true);
        Helper.RotateUIElement(_picker, true);
        Helper.RotateUIElement(_tips, true);
        Helper.RotateUIElement(_learningTips, true);
        Helper.RotateUIElement(_examTips, true);
    }
}
