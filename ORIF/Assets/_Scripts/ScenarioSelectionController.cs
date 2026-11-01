using System;
using System.Collections;
using System.Collections.Generic;
using Scenarios;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Owns the scenario picker, its buttons and selection presentation.</summary>
public sealed class ScenarioSelectionController
{
    private readonly JSONLoader _loader;
    private readonly GameObject _buttonPrefab;
    private readonly Transform _content;
    private readonly MainMenuUserInterfaceUpdater _menuUI;
    private readonly JobSheetUserInterfaceUpdater _jobUI;
    private readonly Action<Scenario> _onSelected;
    private readonly List<GameObject> _buttons = new List<GameObject>();
    private List<Scenario> _scenarios = new List<Scenario>();
    private Button _activeButton;

    public ScenarioSelectionController(JSONLoader loader, GameObject buttonPrefab, GameObject picker,
        MainMenuUserInterfaceUpdater menuUI, JobSheetUserInterfaceUpdater jobUI, Action<Scenario> onSelected)
    {
        _loader = loader;
        _buttonPrefab = buttonPrefab;
        _content = picker.transform.Find("Scroll View/Viewport/Content");
        _menuUI = menuUI;
        _jobUI = jobUI;
        _onSelected = onSelected;
    }

    public void Initialize()
    {
        ClearButtons();
        SetScenarios(_loader.GetLoadedScenarios(), 1);
    }

    public IEnumerator Reload(bool remote)
    {
        ClearButtons();
        _scenarios = new List<Scenario>();
        if (remote)
            yield return _loader.LoadScenarioServer();
        else
            _loader.LoadScenariosLocal();
        SetScenarios(_loader.GetLoadedScenarios(), remote ? 0 : 1);
    }

    private void SetScenarios(List<Scenario> scenarios, int preferredIndex)
    {
        _scenarios = scenarios ?? new List<Scenario>();
        for (int i = 0; i < _scenarios.Count; i++)
        {
            int index = i;
            GameObject instance = UnityEngine.Object.Instantiate(_buttonPrefab, _content, false);
            _buttons.Add(instance);
            instance.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 40.734f);
            instance.GetComponentInChildren<TextMeshProUGUI>().text = "Scénario " + (i + 1);
            instance.GetComponent<Button>().onClick.AddListener(() =>
            {
                Select(index);
                SoundManager.Instance.PlayTouchscreenSound();
            });
        }
        if (_scenarios.Count > 0)
            Select(Math.Min(preferredIndex, _scenarios.Count - 1));
    }

    public void Select(int index)
    {
        if (index < 0 || index >= _scenarios.Count)
        {
            Debug.LogWarning($"Scenario index {index} is not available.");
            return;
        }

        if (_activeButton != null)
        {
            ColorBlock previous = _activeButton.colors;
            previous.normalColor = Color.white;
            _activeButton.colors = previous;
        }
        _activeButton = _buttons[index].GetComponent<Button>();
        ColorBlock current = _activeButton.colors;
        current.normalColor = current.pressedColor;
        _activeButton.colors = current;

        Scenario scenario = _scenarios[index];
        _onSelected(scenario);
        _menuUI.Scenario = scenario;
        _jobUI.Scenario = scenario;
        if (scenario.mode == Scenario.Mode.Training)
            _menuUI.ShowUILevel();
        else
            _menuUI.HideUILevel();
        _menuUI.UpdateUI();
    }

    public void SelectDefaultLocal()
    {
        if (_scenarios.Count > 0)
            Select(Math.Min(1, _scenarios.Count - 1));
    }

    private void ClearButtons()
    {
        foreach (GameObject button in _buttons)
        {
            if (button == null) continue;
            button.SetActive(false);
            UnityEngine.Object.Destroy(button);
        }
        _buttons.Clear();
        _activeButton = null;
    }
}
