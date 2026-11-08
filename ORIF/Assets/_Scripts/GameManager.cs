using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Scenarios;
using TMPro;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.Hands.Gestures;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Central game manager controlling state transitions, scenario lifecycle,
/// table placement, cutlery interaction, validation and UI orchestration.
/// </summary>
public class GameManager : MonoBehaviour
{
    enum GameState
    {
        MainMenu,
        TablePlacement,
        PlayScenario,
        ScenarioEnded
    }

    private GameState _gameState;

    #region VARIABLES

    // UI Elements
    [SerializeField]
    private GameObject _mainMenuSelectionUI;
    [SerializeField]
    private GameObject _mainMenuScenarioPickerUI;
    [SerializeField]
    private Toggle _useRemoteScenariosToggle;
    [SerializeField]
    private GameObject _btnScenarioSelector;
    [SerializeField]
    private GameObject _pauseMenu;
    [SerializeField]
    private GameObject _tipsMainMenu;
    [SerializeField]
    private GameObject _tipsLearningMode;
    [SerializeField]
    private GameObject _tipsExamMode;
    [SerializeField]
    private GameObject _btnValidateCurrentMenu;
    [SerializeField]
    private GameObject _jobSheet;

    // Scenario Elements
    [SerializeField] private PrefabCutleryManager prefabManager;
    private static Scenario _selectedScenario;
    private readonly ScenarioActionLog _actionLog = new ScenarioActionLog();
    private readonly ScenarioPlacementLocks _placementLocks = new ScenarioPlacementLocks();
    private static List<Dictionary<string, GameObject>> _lstInstantiatedPrefabs;
    private static Dictionary<string, GameObject> _instantiatedPrefabs;
    private static Dictionary<string, GameObject> _instantiatedCutleriesPrefabsOnTrolly;
    private static Dictionary<string, Vector3> _culteriesInitialPosition;


    // Materials
    [SerializeField]
    private Material _placementMaterialA;
    [SerializeField]
    private Material _placementMaterialB;
    [SerializeField]
    private Material _defaultCutleryMaterial;
    private static Material _defaultCutleryMaterialRef;
    [SerializeField]
    private Material _defaultPlateMaterial;
    private static Material _defaultPlateMaterialRef;
    [SerializeField]
    private Material _defaultGlassMaterial;
    private static Material _defaultGlassMaterialRef;
    [SerializeField]
    private Material _currentItemMaterial;
    private static Material _currentItemMaterialRef;

    // Interactors
    [SerializeField]
    private GameObject[] _interactorsL;
    [SerializeField]
    private GameObject[] _interactorsR;
    private HandInteractionController _hands;
    [SerializeField]
    private XRRayInteractor _interactor;

    // Game State and some references
    private static GameState _gameStateRef;
    private static GameObject _stepHelperRef;
    private static GameObject _folderPaperRef;
    private GameObject _pTray;
    private TrayController _trayController;
    private static GameObject _timerRef;
    private static CountdownTimer _countdownTimer;
    private TransformSnapshot _tableTransform;
    private static List<Transform> _chairsTransform;
    private static List<GameObject> _menuReports;

    // Game Objects
    [SerializeField]
    private GameObject _table;
    [SerializeField]
    private GameObject _trolly;
    [SerializeField]
    private GameObject _chair;
    [SerializeField]
    private GameObject[] _trollyCutleries;
    private static GameObject[] _trollyCutleriesRef;
    private GameObject[] _chairs;
    [SerializeField] private GameObject light;

    // Specific Game Object References
    [SerializeField]
    private GameObject _tray;
    [SerializeField]
    private GameObject _stepHelper;
    [SerializeField]
    private GameObject _folderPaper;
    [SerializeField]
    private GameObject _leftHand;
    [SerializeField]
    private GameObject _timer;
    [SerializeField]
    private GameObject _endGameReport;
    private static GameObject _endGameReportRef;
    [SerializeField]
    private GameObject _celebrationEndGame;
    private static GameObject _celebrationEndGameRef;
    [SerializeField]
    private GameObject _celebrationEndGameTip;
    private static GameObject _celebrationEndGameTipRef;
    [SerializeField]
    private GameObject _tablePlacement;
    private static GameObject _tablePlacementRef;

    // AR Plane Manager
    [SerializeField]
    private GameObject _arPlaneManager;
    private TablePlacementPreview _placementPreview;

    // Flags and States
    private bool _isRaycasting = false;
    private static bool _tablePlaced = false;
    private static bool _readyToPlace = false;
    private bool _tipsRead = false;
    private readonly ScenarioProgress _progress = new ScenarioProgress();
    private static int txtTimerValue = 1;
    private static bool gameEnded = false;
    private Vector3 _tablePosition = new Vector3(0, 0, 0);
    private Quaternion _tableRotation = Quaternion.identity;
    private static bool _isTenSecondsAchieved = false;
    private Vector3 _trollyInitialLocalPosition;
    private Quaternion _trollyInitialLocalRotation;
    private static bool _areCutleriesAllPlaced = false;
    private static bool _areExtrasAllPlaced = false;
    private static bool _extraPlacement = false;

    // UI Updaters
    private MainMenuUserInterfaceUpdater _uiMainMenuUpdater;
    private JobSheetUserInterfaceUpdater _uiJobSheetUpdater;
    private static StepUserInterfaceUpdater _stepInterfaceUpdater;

    // Others
    private static int _drdMode;
    private static int _drdLevel;
    private ScenarioSelectionController _scenarioSelection;
    private MenuController _menus;
    private CutleryPlacementController _cutleryController;
    private readonly PlacementZoneController _placementZones = new PlacementZoneController();
    private PauseMenuController _pauseController;
    private InterfacePresentationController _presentation;
    private Coroutine _scenarioReload;

    [SerializeField] private Notification notification;

    [SerializeField]
    private BoxCollider _trollyCollider;

    #endregion

    void Awake()
    {
        print("Awake");
        InitializeCollections();
        InitializeReferences();
        InitializeInteractors();
        InitializeScenarioAndMenus();
    }

    private void Start()
    {
        CreateTray();
        ToggleStepHelper(false);

        _tablePlacementRef.SetActive(false);
        NextState(GameState.MainMenu);
        _useRemoteScenariosToggle.onValueChanged.AddListener(HandleUseLocalOrRemoteScenarios);
    }

    /// <summary>
    /// Updates the table placement each frame. Moves the preview in front of the camera,
    /// and places the table at the displayed pose when confirmed.
    /// </summary>
    private void UpdateTablePlacement()
    {
        if (_readyToPlace && !_tablePlaced)
        {
            _tablePlacementRef.SetActive(false);
            PlaceTableAndCutleries(_tablePosition, _tableRotation);
            _tablePlaced = true;
        }

        if (_isRaycasting)
        {
            if (_placementPreview == null)
                _placementPreview = new TablePlacementPreview(_tablePlacementRef);

            _placementPreview.Update(Camera.main.transform, out _tablePosition, out _tableRotation);
        }
    }

    void Update()
    {

        UpdateTablePlacement();


        _trayController?.UpdatePose();

        _presentation.UpdatePose(Camera.main, notification.GetNotificationObject());

        UpdateScenarioTimer();
    }

    private void UpdateScenarioTimer()
    {
        if (gameEnded || _countdownTimer == null || !ScenarioTimingPolicy.UsesCountdown(_selectedScenario))
            return;

        if (_countdownTimer.IsRemainingTenSeconds() && !_isTenSecondsAchieved)
        {
            SoundManager.Instance.PlayLastTenSecondsCountdownSound();
            _isTenSecondsAchieved = true;
        }

        if (!_countdownTimer.GetIsCountingDownOver())
            return;

        switch (ScenarioTimingPolicy.GetExpiryAction(_selectedScenario))
        {
            case ScenarioTimingPolicy.ExpiryAction.ContinueWithOvertime:
                _countdownTimer.StartTimer();
                break;
            case ScenarioTimingPolicy.ExpiryAction.EndScenario:
                gameEnded = true;
                HandleEndScenario();
                break;
        }
    }

    /// <summary>
    /// Initializes all collection fields to empty instances.
    /// </summary>
    private void InitializeCollections()
    {
        _culteriesInitialPosition = new Dictionary<string, Vector3>();
        _chairsTransform = new List<Transform>();
        _placementLocks.Reset();
        _menuReports = new List<GameObject>();
        _instantiatedPrefabs = new Dictionary<string, GameObject>();
        _actionLog.Reset();
    }

    /// <summary>
    /// Copies serialized field references to static references for cross-method access,
    /// and retrieves required sibling components.
    /// </summary>
    private void InitializeReferences()
    {
        _tablePlacementRef = _tablePlacement;
        _celebrationEndGameTipRef = _celebrationEndGameTip;
        _celebrationEndGameRef = _celebrationEndGame;
        _timerRef = _timer;
        _folderPaperRef = _folderPaper;
        _defaultCutleryMaterialRef = _defaultCutleryMaterial;
        _defaultPlateMaterialRef = _defaultPlateMaterial;
        _defaultGlassMaterialRef = _defaultGlassMaterial;
        _currentItemMaterialRef = _currentItemMaterial;
        _trollyCutleriesRef = _trollyCutleries;
        _stepInterfaceUpdater = GetComponent<StepUserInterfaceUpdater>();
        _uiMainMenuUpdater = GetComponent<MainMenuUserInterfaceUpdater>();
        _uiJobSheetUpdater = GetComponent<JobSheetUserInterfaceUpdater>();
        _stepHelperRef = _stepHelper;
        _scenarioSelection = new ScenarioSelectionController(GetComponent<JSONLoader>(),
            _btnScenarioSelector, _mainMenuScenarioPickerUI, _uiMainMenuUpdater, _uiJobSheetUpdater,
            scenario => _selectedScenario = scenario);
        _menus = new MenuController(_mainMenuSelectionUI, _mainMenuScenarioPickerUI,
            _tipsMainMenu, _tipsLearningMode, _tipsExamMode, _scenarioSelection);
        _cutleryController = new CutleryPlacementController(_progress, _actionLog, _placementLocks,
            _defaultCutleryMaterial, _defaultGlassMaterial, _placementZones.PrepareExamRelease, UpdateReport, UpdateStep, CheckEndScenario);
    }

    /// <summary>Initializes the hand coordinator from the existing Inspector references.</summary>
    private void InitializeInteractors()
    {
        _hands = new HandInteractionController(_interactorsL, _interactorsR, _leftHand);
        _pauseController = new PauseMenuController(_pauseMenu, _btnValidateCurrentMenu, _hands);
        _presentation = new InterfacePresentationController(_menus, _pauseMenu, _stepHelper,
            _timer, _celebrationEndGame, _celebrationEndGameTip);
    }

    /// <summary>
    /// Initializes the selected scenario to a default empty instance and resets the menu index to zero.
    /// </summary>
    private void InitializeScenarioAndMenus()
    {
        _selectedScenario = new Scenario();
        _progress.ResetMenus();
    }

    /// <summary>
    /// Instantiates the tray prefab at the left hand position and deactivates it until needed.
    /// </summary>
    private void CreateTray()
    {
        _pTray = ScenarioEnvironmentFactory.CreateHiddenTray(_tray, _hands.LeftDirectTransform.position);
        _trayController = new TrayController(_pTray, _hands);
    }

    /// <summary>
    /// Transitions to a new game state and executes its entry logic.
    /// </summary>
    /// <param name="newState">The target game state to transition to.</param>
    private void NextState(GameState newState)
    {
        _gameState = newState;
        _gameStateRef = _gameState;
        print("-> GameManager::NextState()");
        print("New state is : " + newState.ToString());

        switch (newState)
        {
            case GameState.MainMenu:
                EnterMainMenuState();
                break;
            case GameState.TablePlacement:
                EnterTablePlacementState();
                break;
            case GameState.PlayScenario:
                EnterPlayScenarioState();
                break;
            case GameState.ScenarioEnded:
                EnterScenarioEndedState();
                break;
        }
    }

    /// <summary>
    /// Entry logic for the MainMenu state: shows the menu, loads scenarios,
    /// creates selector buttons and hides overlays.
    /// </summary>
    private void EnterMainMenuState()
    {
        print("Entering Main Menu");
        ShowMainMenu();
        _scenarioSelection.Initialize();

        _pauseController.Hide();
        _presentation.ShowCelebration(false);
        _presentation.ShowCelebrationTip(false);
        _menus.HideScenarioTips();
    }

    /// <summary>
    /// Entry logic for the TablePlacement state: notifies the user and starts placement mode.
    /// </summary>
    private void EnterTablePlacementState()
    {
        notification.DisplayNotification("Veuillez placer la table", 3);
        TablePlacement();
    }

    /// <summary>
    /// Entry logic for the PlayScenario state: initializes steps, starts the countdown timer
    /// and enables cutlery interaction on the trolly.
    /// </summary>
    private void EnterPlayScenarioState()
    {
        print("Entering Scenario");
        InitStep();
        InitCountdownTimer();
        EnableCutleryInteractionForCutleriesOnTrolly();
        ToggleInteractorsRightWithDirectEnable(false);
    }

    /// <summary>
    /// Entry logic for the ScenarioEnded state: disables the palm/tray, shows the left hand
    /// and generates the end-game report.
    /// </summary>
    private void EnterScenarioEndedState()
    {
        print("Scenario Ended");

        _trayController.Close();

        CreateEndGameReport();
    }

    /// <summary>
    /// Initializes and starts the countdown timer if the scenario mode requires one
    /// (Exam mode or Expert Training mode).
    /// </summary>
    private void InitCountdownTimer()
    {
        if (ScenarioTimingPolicy.UsesCountdown(_selectedScenario))
        {
            _presentation.ShowTimer(true);
            _countdownTimer = GetComponent<CountdownTimer>();
            _countdownTimer.SetDurationMinutes(_selectedScenario.allowedTime);
            _countdownTimer.SetCountdown();
            _countdownTimer.StartCountdown();
        }
    }

    /// <summary>
    /// Creates the appropriate end-game report based on the current scenario mode and level.
    /// </summary>
    private void CreateEndGameReport()
    {
        if (IsExamOrExpertMode())
        {
            HandleExamOrExpertMode();
        }
        else if (IsLearningOrBegginnerMode())
        {
            HandleLearningOrBegginnerMode();
        }
    }

    /// <summary>
    /// Returns whether the current scenario is in Exam mode or Expert Training mode.
    /// </summary>
    /// <returns>True if the scenario is Exam or Expert Training.</returns>
    private bool IsExamOrExpertMode()
    {
        return _selectedScenario.mode == Scenario.Mode.Exam ||
               (_selectedScenario.mode == Scenario.Mode.Training &&
                _selectedScenario.level == Scenario.Level.Expert);
    }

    /// <summary>
    /// Returns whether the current scenario is in Learning mode or Beginner Training mode.
    /// </summary>
    /// <returns>True if the scenario is Learning or Beginner Training.</returns>
    private bool IsLearningOrBegginnerMode()
    {
        return _selectedScenario.mode == Scenario.Mode.Learning ||
               (_selectedScenario.mode == Scenario.Mode.Training &&
                _selectedScenario.level == Scenario.Level.Begginner);
    }

    /// <summary>
    /// Instantiates the end-game report, stops the timer and populates it with scenario results
    /// for Exam or Expert Training modes.
    /// </summary>
    private void HandleExamOrExpertMode()
    {
        print("Create end report");
        _endGameReportRef = Instantiate(_endGameReport);
        float countdownTimeDone = CalculateCountdownTimeDone();
        _countdownTimer.StopCountdown();
        _countdownTimer.StopTimer();
        _endGameReportRef.GetComponent<EndGameReport>().GenerateEndGameReport(_selectedScenario, countdownTimeDone, _countdownTimer.ElapsedTime, _actionLog.Records, _endGameReportRef);
    }

    /// <summary>
    /// Displays the celebration tip for Learning or Beginner Training modes.
    /// </summary>
    private void HandleLearningOrBegginnerMode()
    {
        _presentation.ShowCelebrationTip(true);
    }

    /// <summary>
    /// Calculates the elapsed countdown time in seconds (total allowed minus remaining).
    /// </summary>
    /// <returns>The time spent on the countdown in seconds.</returns>
    private float CalculateCountdownTimeDone()
    {
        if (_countdownTimer.RemainingSeconds <= 0)
        {
            return _selectedScenario.allowedTime * 60.0f;
        }
        else
        {
            return _selectedScenario.allowedTime * 60.0f - _countdownTimer.RemainingSeconds;
        }
    }

    /// <summary>Reloads the scenario picker from the selected local or remote source.</summary>
    public void HandleUseLocalOrRemoteScenarios(bool useRemoteScenarios)
    {
        if (_scenarioReload != null)
            StopCoroutine(_scenarioReload);
        _scenarioReload = StartCoroutine(_scenarioSelection.Reload(useRemoteScenarios));
    }

    /// <summary>
    /// Shows or hides the step helper panel.
    /// </summary>
    /// <param name="active">True to show the step helper, false to hide it.</param>
    private void ToggleStepHelper(bool active)
    {
        _presentation.ShowSteps(active);
    }

    /// <summary>
    /// Initializes step tracking for the current menu: resets indexes, generates steps
    /// and applies mode-specific behavior (Learning or Beginner Training).
    /// </summary>
    private void InitStep()
    {
        print("initStep");
        InitializeStepIndexes();
        InitializeStepInterfaceUpdater();
        HandleLearningMode();
        HandleBegginnerTrainingMode();
    }

    /// <summary>
    /// Resets both the step interface updater index and the internal step index to zero.
    /// </summary>
    private void InitializeStepIndexes()
    {
        _stepInterfaceUpdater.SetStepIndex(0);
        _progress.ResetSteps();
    }

    /// <summary>
    /// Configures the step interface updater with the current menu's total step count
    /// and generates the step data.
    /// </summary>
    private void InitializeStepInterfaceUpdater()
    {
        _stepInterfaceUpdater.GenerateSteps(_selectedScenario, _progress.MenuIndex);
    }

    /// <summary>
    /// Activates the step helper, updates the step UI and highlights the current item
    /// on the trolly when in Learning mode.
    /// </summary>
    private void HandleLearningMode()
    {
        if (_selectedScenario.mode == Scenario.Mode.Learning)
        {
            if (_progress.MenuIndex == 0)
                ToggleStepHelper(true);

            _stepInterfaceUpdater.UpdateUI();
            Helper.SetCurrentItemMaterial(_stepInterfaceUpdater.GetSteps(), _progress.StepIndex, _instantiatedCutleriesPrefabsOnTrolly, _currentItemMaterialRef);
        }
    }

    /// <summary>
    /// Activates the step helper and updates the step UI when in Beginner Training mode.
    /// </summary>
    private void HandleBegginnerTrainingMode()
    {
        if (_selectedScenario.mode == Scenario.Mode.Training && _selectedScenario.level == Scenario.Level.Begginner)
        {
            if (_progress.MenuIndex == 0)
                ToggleStepHelper(true);

            if (_stepInterfaceUpdater == null)
                print("_stepInterfaceUpdater == null");

            _stepInterfaceUpdater.UpdateUI();
        }
    }

    /// <summary>
    /// Advances to the next placement step and handles menu completion or scenario completion accordingly.
    /// </summary>
    private void UpdateStep()
    {
        _progress.AdvanceStep();
        print("UpdateStep");

        if (IsLastStepOfLastMenu())
        {
            HandleLastStepOfLastMenu();
        }
        else if (IsLastStepOfCurrentMenu())
        {
            HandleLastStepOfCurrentMenu();
        }
        else
        {
            UpdateCurrentStep();
        }

        _stepInterfaceUpdater.UpdateUI();
    }

    /// <summary>
    /// Returns whether the current step is the last step of the last menu in the scenario.
    /// </summary>
    /// <returns>True if all steps of all menus have been completed.</returns>
    private bool IsLastStepOfLastMenu()
    {
        return _progress.IsScenarioComplete(_stepInterfaceUpdater.TotalSteps, _selectedScenario.menus.Length);
    }

    /// <summary>
    /// Returns whether the current step is the last step of the current menu.
    /// </summary>
    /// <returns>True if all steps of the current menu have been completed.</returns>
    private bool IsLastStepOfCurrentMenu()
    {
        return _progress.IsCurrentMenuComplete(_stepInterfaceUpdater.TotalSteps);
    }

    /// <summary>
    /// Handles completion of the final step in the final menu. Triggers end-scenario check
    /// unless in Exam mode.
    /// </summary>
    private void HandleLastStepOfLastMenu()
    {
        print("All cutleries are placed!");
        if (_selectedScenario.mode != Scenario.Mode.Exam)
        {
            CheckEndScenario();
        }
    }

    /// <summary>
    /// Handles completion of the final step in a non-final menu. Initializes the next menu
    /// unless in Exam mode.
    /// </summary>
    private void HandleLastStepOfCurrentMenu()
    {
        if (_selectedScenario.mode != Scenario.Mode.Exam)
        {
            InitNextMenu();
        }
    }

    /// <summary>
    /// Updates the step interface index and highlights the next cutlery item on the trolly
    /// if applicable (non-Expert Learning mode).
    /// </summary>
    private void UpdateCurrentStep()
    {
        _stepInterfaceUpdater.SetStepIndex(_progress.StepIndex);
        if (_selectedScenario.level != Scenario.Level.Expert && _selectedScenario.mode == Scenario.Mode.Learning)
        {
            Helper.SetCurrentItemMaterial(_stepInterfaceUpdater.GetSteps(), _progress.StepIndex, _instantiatedCutleriesPrefabsOnTrolly, _currentItemMaterialRef);
        }
    }

    /// <summary>
    /// Prepares the transition to the next menu: clears the tray, clones placed objects,
    /// recreates trolly cutleries and advances the menu index.
    /// </summary>
    private void InitNextMenu()
    {
        print("InitNextMenu");
        _trayController.ClearCarriedObjects();
        CloneObjectPlacedBeforeNextMenu();
        RecreateTrolly();
        MenuIndexValueUpdater();

        bool cond = _progress.HasCurrentMenu(_selectedScenario.menus.Length);

        if (cond)
        {
            InitStep();
        }
        else
        {
            HandleEndOfMenus();
        }
    }

    /// <summary>
    /// Increments the menu index to advance to the next menu.
    /// </summary>
    private void MenuIndexValueUpdater()
    {
        print("MenuIndexValueUpdater");
        _progress.AdvanceMenu();
        print("increment menuIndex");
    }

    /// <summary>
    /// Handles the completion of all menus: stops the countdown timer, hides the timer UI
    /// and transitions to the ScenarioEnded state.
    /// </summary>
    private void HandleEndOfMenus()
    {
        if (_countdownTimer == null)
        {
            print("Timer is null");
        }
        else
        {
            _countdownTimer.StopCountdown();
        }

        _presentation.ShowTimer(false);
        NextState(GameState.ScenarioEnded);
    }

    /// <summary>
    /// Destroys existing trolly cutleries and re-instantiates fresh ones on the trolly.
    /// </summary>
    private void RecreateTrolly()
    {
        print("RecreateTrolly");
        var trolly = GameObject.FindGameObjectsWithTag("Trolly")[0];
        DestroyAllCutleriesOnTrolly();
        InstantiateAndPlaceCutleriesOnTrolly(trolly);
    }

    /// <summary>
    /// Clones all cutleries that were moved from the trolly before transitioning to the next menu,
    /// preserving their placed positions as locked objects.
    /// </summary>
    private void CloneObjectPlacedBeforeNextMenu()
    {
        print("CloneObjectPlacedBeforeNextMenu");
        List<GameObject> clonedObjects = new List<GameObject>();

        foreach (var action in _actionLog.GetLatestActions(_progress.MenuIndex))
        {
            ProcessAction(action, clonedObjects);
        }
    }

    /// <summary>
    /// Processes a single scenario action: searches for the matching cutlery in trolly groups
    /// and clones it if it was moved from the trolly.
    /// </summary>
    /// <param name="action">The scenario action containing the cutlery tag and placement data.</param>
    /// <param name="clonedObjects">The list to track all cloned GameObjects.</param>
    private void ProcessAction(ScenarioAction action, List<GameObject> clonedObjects)
    {
        print($"Action for {action.tag}");

        foreach (var group in _instantiatedCutleriesPrefabsOnTrolly)
        {
            var cutlery = FindCutleryInGroup(action.tag, group);
            if (cutlery == null)
            {
                print("Cutlery is null, continuing.");
                continue;
            }

            if (IsCutleryMoved(action, cutlery))
            {
                CloneCutlery(cutlery, clonedObjects);
            }
        }
    }

    /// <summary>
    /// Finds a cutlery child transform by tag within a prefab group on the trolly.
    /// </summary>
    /// <param name="tag">The cutlery tag used to build the child name "SM_{tag}".</param>
    /// <param name="group">The prefab group key-value pair to search within.</param>
    /// <returns>The matching child Transform, or null if not found.</returns>
    private Transform FindCutleryInGroup(string tag, KeyValuePair<string, GameObject> group)
    {
        var cutleryToFind = "SM_" + tag;
        var cutlery = group.Value.transform.Find(cutleryToFind);

        if (cutlery == null)
        {
            print($"No cutlery found for {cutleryToFind} in {group.Key}");
        }

        return cutlery;
    }

    /// <summary>
    /// Determines whether a cutlery item has been moved away from the trolly bounds.
    /// </summary>
    /// <param name="action">The scenario action containing the expected tag.</param>
    /// <param name="cutlery">The cutlery transform to check.</param>
    /// <param name="distanceThreshold">Unused distance threshold (reserved for future use).</param>
    /// <returns>True if the cutlery tag matches and is outside the trolly bounds.</returns>
    private bool IsCutleryMoved(ScenarioAction action, Transform cutlery, float distanceThreshold = 0.01f)
    {
        print($"IsCutleryMoved");
        _trollyCollider = GameObject.FindGameObjectsWithTag("Trolly")[0].GetComponent<BoxCollider>();

        if (action.tag != cutlery.tag)
        {
            return false;
        }

        if (_trollyCollider.bounds.Contains(cutlery.position))
        {
            print($"The object {cutlery.tag} didn't move from the trolly");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Creates a locked clone of a cutlery item at its current position with high mass and gravity enabled.
    /// </summary>
    /// <param name="cutlery">The cutlery transform to clone.</param>
    /// <param name="clonedObjects">The list to track the newly cloned object.</param>
    private void CloneCutlery(Transform cutlery, List<GameObject> clonedObjects)
    {
        print($"CloneCutlery");
        var position = cutlery.gameObject.transform.position;
        GameObject clonedObject = Instantiate(cutlery.gameObject, position, cutlery.gameObject.transform.rotation);

        var rigidbody = clonedObject.GetComponent<Rigidbody>();
        rigidbody.useGravity = true;
        rigidbody.mass = 10000;

        var collider = clonedObject.GetComponent<BoxCollider>();
        collider.enabled = true;

        clonedObject.tag = "Locked";
        clonedObjects.Add(clonedObject);
    }

    /// <summary>
    /// Rounds the slider value to the nearest multiple of 5 and stores the integer result.
    /// </summary>
    /// <param name="slider">The timer slider to round.</param>
    public void RoundValue(Slider slider)
    {
        slider.value = Helper.RoundUpSliderValue(slider.value);
        txtTimerValue = (int)slider.value;
    }

    /// <summary>
    /// Updates a text field with the current rounded timer slider value.
    /// </summary>
    /// <param name="t">The TextMeshProUGUI component to update.</param>
    public void UpdateSliderText(TextMeshProUGUI t)
    {
        t.text = txtTimerValue.ToString();
    }

    /// <summary>
    /// Toggles the right-hand interactors on or off.
    /// </summary>
    /// <param name="active">True to enable, false to disable.</param>
    private void ToggleInteractorsR(bool active)
    {
        _hands.SetRightInteractors(active);
    }

    /// <summary>
    /// Toggles the left-hand interactors on or off.
    /// </summary>
    /// <param name="active">True to enable, false to disable.</param>
    private void ToggleInteractorsL(bool active)
    {
        _hands.SetLeftInteractors(active);
    }

    /// <summary>
    /// Activates the main menu selection panel and hides other panels.
    /// </summary>
    public void ShowMainMenu()
    {
        _menus.ShowMain();
    }

    /// <summary>
    /// Switches to the scenario picker panel and selects the first scenario by default.
    /// </summary>
    public void ShowScenarioPicker()
    {
        _menus.ShowScenarioPicker();
    }

    /// <summary>
    /// Switches to the tips panel, hiding other menu panels.
    /// </summary>
    public void ShowTips()
    {
        _menus.ShowTips();
    }

    /// <summary>
    /// Hides all main menu panels (selection, picker and tips).
    /// </summary>
    public void HideMenu()
    {
        _menus.Hide();
    }

    /// <summary>
    /// Exits the application.
    /// </summary>
    public void ExitGame()
    {
        Application.Quit();
    }

    /// <summary>
    /// Placeholder callback for reading the mode dropdown value (currently unused).
    /// </summary>
    /// <param name="dropdown">The mode dropdown.</param>
    public void GetValueOfDropdownMode(TMP_Dropdown dropdown)
    {
    }

    /// <summary>
    /// Handles a mode change from the dropdown: shows or hides the level dropdown
    /// and updates the scenario mode and UI accordingly.
    /// </summary>
    /// <param name="dropdown">The mode dropdown that triggered the change.</param>
    public void HandleModeChange(TMP_Dropdown dropdown)
    {
        print($"HandleModeChange {dropdown.value}");
        bool hideLevelDropdown = true;
        switch (dropdown.value)
        {
            case (int)Scenario.Mode.Training:
                hideLevelDropdown = false;
                break;
            default:
                break;
        }

        if (hideLevelDropdown)
            _uiMainMenuUpdater.HideUILevel();
        else
            _uiMainMenuUpdater.ShowUILevel();

        _uiMainMenuUpdater.UpdateUIMode(dropdown.value);
        _uiMainMenuUpdater.UpdateUI();
    }

    /// <summary>
    /// Stores the selected level dropdown value for later constraint application.
    /// </summary>
    /// <param name="dropdown">The level dropdown.</param>
    public void GetValueOfDropdownLevel(TMP_Dropdown dropdown)
    {
        print($"GetValueOfDropdownLevel {dropdown.value}");
        _drdLevel = dropdown.value;
    }

    /// <summary>
    /// Handles a level change from the dropdown, applying mode-based constraints
    /// to enforce valid level values and refreshing the UI.
    /// </summary>
    /// <param name="dropdown">The level dropdown that triggered the change.</param>
    public void HandleLevelChange(TMP_Dropdown dropdown)
    {
        print($"HandleLevelChange {dropdown.value}");
        int value = -1;
        switch (_drdLevel)
        {
            case 0:
                if (_drdMode == 0)
                    value = 0;
                break;
            case 1:
                value = dropdown.value;
                break;
            case 2:
                if (_drdMode == 2)
                    value = 2;
                break;
            default:
                break;
        }
        dropdown.value = value;
        _uiMainMenuUpdater.UpdateUI();
    }

    /// <summary>
    /// Starts the simulation: resets all flags, hides menus, reads the scenario configuration
    /// from the UI and routes to the appropriate mode entry point.
    /// </summary>
    public void LoadSimulation()
    {
        print("Load simulation");

        ResetFlagsAndReferences();
        ResetStepIndex();

        ToggleInteractorsLeftWithDirectEnable(false);
        HideMenu();
        _selectedScenario = _uiMainMenuUpdater.GetInformations();

        HandleScenarioMode(_selectedScenario.mode);
    }

    /// <summary>
    /// Routes to the correct state or tip screen based on the selected scenario mode.
    /// </summary>
    /// <param name="mode">The scenario mode to handle.</param>
    private void HandleScenarioMode(Scenario.Mode mode)
    {
        if (!_menus.ShowScenarioTips(mode))
            NextState(GameState.TablePlacement);
    }

    /// <summary>
    /// Toggles left-hand interactors while keeping the direct interactor (last element) always enabled.
    /// </summary>
    /// <param name="active">True to enable ray interactors, false to disable them.</param>
    private void ToggleInteractorsLeftWithDirectEnable(bool active)
    {
        _hands.SetLeftInteractors(active, true);
    }

    /// <summary>
    /// Toggles right-hand interactors while keeping the direct interactor (last element) always enabled.
    /// </summary>
    /// <param name="active">True to enable ray interactors, false to disable them.</param>
    private void ToggleInteractorsRightWithDirectEnable(bool active)
    {
        _hands.SetRightInteractors(active, true);
    }

    /// <summary>
    /// Marks tips as read, hides tip panels and transitions to the TablePlacement state.
    /// </summary>
    public void TipsRead()
    {
        _tipsRead = true;
        _menus.HideScenarioTips();
        NextState(GameState.TablePlacement);
    }

    /// <summary>
    /// Activates and immediately positions the table placement preview.
    /// </summary>
    public void TablePlacement()
    {
        print("Table placement"); 
        _tablePlacementRef.SetActive(true);
        _isRaycasting = true;
        UpdateTablePlacement();

    }

    /// <summary>
    /// Creates the trolly at an offset relative to the table, populates it with cutlery groups
    /// and returns the trolly GameObject.
    /// </summary>
    /// <param name="tableTransform">The table transform used to compute the trolly position.</param>
    /// <returns>The instantiated trolly GameObject.</returns>
    private GameObject CreateTrolly(Transform tableTransform)
    {
        print("Creating trolly");
        _instantiatedCutleriesPrefabsOnTrolly = new Dictionary<string, GameObject>();

        GameObject pTrolly = ScenarioEnvironmentFactory.CreateTrolly(_trolly, tableTransform);
        _trollyCollider = FindObjectsByType<BoxCollider>(FindObjectsSortMode.InstanceID).FirstOrDefault(o => o.CompareTag("Trolly"));

        InstantiateAndPlaceCutleriesOnTrolly(pTrolly);

        return pTrolly;
    }

    /// <summary>
    /// Instantiates all cutlery group prefabs and parents them under the trolly mesh.
    /// </summary>
    /// <param name="pTrolly">The trolly GameObject to parent cutleries under.</param>
    private void InstantiateAndPlaceCutleriesOnTrolly(GameObject pTrolly)
    {
        ScenarioEnvironmentFactory.PopulateTrolly(pTrolly, _trollyCutleriesRef, _instantiatedCutleriesPrefabsOnTrolly);
        foreach (var group in _instantiatedCutleriesPrefabsOnTrolly.Values)
        {
            foreach (var interactable in group.GetComponentsInChildren<XRGrabInteractable>(true))
            {
                var binding = interactable.GetComponent<CutleryInteractionBinding>();
                if (binding == null)
                    binding = interactable.gameObject.AddComponent<CutleryInteractionBinding>();
                binding.Initialize(this, interactable);
            }
        }
    }

    /// <summary>
    /// Destroys all instantiated cutlery objects on the trolly and clears the tracking dictionary.
    /// </summary>
    private void DestroyAllCutleriesOnTrolly()
    {
        ScenarioEnvironmentCleanup.ClearTrolly(_instantiatedCutleriesPrefabsOnTrolly);
    }

    /// <summary>
    /// Places the table, trolly, cutleries, chairs and job sheet at the specified position and rotation.
    /// Initializes all placement data structures and configures the scene layout.
    /// </summary>
    /// <param name="tablePosition">The world position for the table.</param>
    /// <param name="tableRotation">The world rotation for the table and placement group.</param>
    public void PlaceTableAndCutleries(Vector3 tablePosition, Quaternion tableRotation)
    {
        print("PlaceTableAndCutleries");
        _tableTransform = new TransformSnapshot(tablePosition, tableRotation, new Vector3(1, 1, 1));

        InitializePlacement(tablePosition);
        var pTable = InstantiateTable(tablePosition, Quaternion.identity);
        var pTrolly = CreateTrolly(pTable.transform);
        _culteriesInitialPosition = Helper.GetCutleriesInitialPosition(prefabManager.PrefabEntries);
        DisableCutleryInteractionForCutleriesOnTrolly();

        int i = 0;
        foreach (var menu in _selectedScenario.menus)
        {
            var menuObject = InitializeMenu(menu, pTable, pTrolly);
            PlaceCutleriesForMenu(menu, pTable, ref i, menuObject);
            i++;
        }

        ArrangeChairsAndMenus(pTable);
        RotatePlacementGroup(tableRotation);
        SetupJobSheet(tablePosition, tableRotation);
    }

    /// <summary>
    /// Activates the job sheet, positions it above the table and refreshes its UI content.
    /// </summary>
    /// <param name="tablePosition">The table world position for job sheet placement.</param>
    /// <param name="tableRotation">The table rotation to apply to the job sheet.</param>
    private void SetupJobSheet(Vector3 tablePosition, Quaternion tableRotation)
    {
        _jobSheet.SetActive(true);
        _jobSheet.transform.position = new Vector3(tablePosition.x, tablePosition.y + 1.15f, tablePosition.z + 0.14f);
        _jobSheet.transform.rotation = tableRotation;
        _jobSheet.GetComponent<Rigidbody>().useGravity = false;
        _uiJobSheetUpdater.UpdateUI();
    }

    /// <summary>
    /// Rotates all placed scene objects (table, menus, chairs, trolly) as a group
    /// around the table position using a temporary anchor.
    /// </summary>
    /// <param name="tableRotation">The target rotation for the entire placement group.</param>
    private void RotatePlacementGroup(Quaternion tableRotation)
    {
        print("RotatePlacementGroup");
        var objectsToRotate = new List<GameObject>();
        objectsToRotate.AddRange(GameObject.FindGameObjectsWithTag("Table"));
        objectsToRotate.AddRange(GameObject.FindGameObjectsWithTag("Menu"));
        objectsToRotate.AddRange(GameObject.FindGameObjectsWithTag("Chair"));
        objectsToRotate.AddRange(GameObject.FindGameObjectsWithTag("Trolly"));

        if (objectsToRotate.Count == 0)
        {
            Debug.LogWarning("Aucun objet trouvé à faire pivoter.");
            return;
        }

        GameObject anchor = new GameObject("RotationAnchor");
        anchor.transform.position = objectsToRotate[0].transform.position;
        anchor.transform.rotation = Quaternion.identity;

        foreach (var obj in objectsToRotate)
        {
            obj.transform.SetParent(anchor.transform, true);
        }

        anchor.transform.rotation = tableRotation;

        foreach (var obj in objectsToRotate)
        {
            if (obj.CompareTag("Trolly"))
            {
                _trollyInitialLocalPosition = obj.transform.position;
                _trollyInitialLocalRotation = obj.transform.rotation;
            }
            obj.transform.SetParent(null, true);
        }

        Destroy(anchor);
    }

    /// <summary>
    /// Initializes placement data structures and positions the celebration objects above the table.
    /// </summary>
    /// <param name="tablePosition">The table world position used as reference.</param>
    private void InitializePlacement(Vector3 tablePosition)
    {
        print("InitializePlacement");
        _lstInstantiatedPrefabs = new List<Dictionary<string, GameObject>>();
        var tmpCelebrationPos = new Vector3(tablePosition.x, tablePosition.y + 1f, tablePosition.z);
        _celebrationEndGameRef.transform.position = tmpCelebrationPos;
        _celebrationEndGameTipRef.transform.position = new Vector3(tablePosition.x, tablePosition.y + 1.15f, tablePosition.z);
    }

    /// <summary>
    /// Instantiates the table prefab at the specified position and rotation.
    /// </summary>
    /// <param name="tablePosition">The world position for the table.</param>
    /// <param name="tableRotation">The rotation for the table.</param>
    /// <returns>The instantiated table GameObject.</returns>
    private GameObject InstantiateTable(Vector3 tablePosition, Quaternion tableRotation)
    {
        print("InstantiateTable");
        var pTable = ScenarioEnvironmentFactory.CreateTable(_table, tablePosition, tableRotation);
        return pTable;
    }

    /// <summary>
    /// Initializes tracking structures for a new menu: adds a locked cutleries dictionary,
    /// sets up the instantiated prefabs map with table and trolly, and creates a parent GameObject.
    /// </summary>
    /// <param name="menu">The menu data to initialize.</param>
    /// <param name="pTable">The table GameObject reference.</param>
    /// <param name="pTrolly">The trolly GameObject reference.</param>
    /// <returns>The menu parent GameObject used to group its cutleries.</returns>
    private GameObject InitializeMenu(Menu menu, GameObject pTable, GameObject pTrolly)
    {
        print("InitializeMenu");
        _placementLocks.AddMenu();
        _instantiatedPrefabs = new Dictionary<string, GameObject>
    {
        { "Table", pTable },
        { "Trolly", pTrolly }
    };

        // Create a parent GameObject for the current menu, so it will be rotated later on for the menu placement
        GameObject menuObject = new GameObject(menu.name)
        {
            tag = "Menu"
        };
        return menuObject;
    }

    /// <summary>
    /// Instantiates and configures all cutlery placement guides for a single menu,
    /// computing their world positions from parent offsets.
    /// </summary>
    /// <param name="menu">The menu data containing the cutlery-to-place definitions.</param>
    /// <param name="pTable">The table GameObject used as a potential parent reference.</param>
    /// <param name="i">The menu index counter (passed by reference for material alternation).</param>
    /// <param name="menuObject">The parent GameObject to group cutleries under.</param>
    private void PlaceCutleriesForMenu(Menu menu, GameObject pTable, ref int i, GameObject menuObject)
    {
        print("PlaceCutleriesForMenu");
        if (ScenarioEnvironmentFactory.PopulatePlacementGuides(menu, _selectedScenario, prefabManager,
            _instantiatedPrefabs, menuObject, i, _placementMaterialA, _placementMaterialB))
        {
            _lstInstantiatedPrefabs.Add(_instantiatedPrefabs);
        }
    }

    /// <summary>
    /// Positions menus and chairs around the table based on table bounds,
    /// alternating between top and bottom edges. Initializes per-menu actions
    /// and creates folder papers for non-Exam modes.
    /// </summary>
    /// <param name="pTable">The table GameObject used for spatial reference.</param>
    private void ArrangeChairsAndMenus(GameObject pTable)
    {
        print("ArrangeChairsAndMenus");
        var tableBounds = pTable.transform.Find("SM_Table").GetComponent<Renderer>().bounds;
        float tableLength = tableBounds.size.z;

        int i = 0;
        _chairs = GameObject.FindGameObjectsWithTag("Chair");
        foreach (var menu in GameObject.FindGameObjectsWithTag("Menu"))
        {
            ScenarioEnvironmentFactory.ArrangeMenuAndCreateChair(_chair, pTable.transform, menu.transform, tableLength, i);

            _actionLog.AddMenu();
            AddDefaultActionsForMenu(i);
            if (_selectedScenario.mode != Scenario.Mode.Exam)
            {
                CreateFolderPaper(_selectedScenario, _selectedScenario.menus[i].name, pTable.transform, i);
            }
            i++;
        }
    }

    /// <summary>
    /// Creates a folder paper (mission recap) for a menu, positions it beside the table
    /// and generates its initial report content.
    /// </summary>
    /// <param name="scenario">The active scenario for report generation.</param>
    /// <param name="menuName">The menu name to display on the report.</param>
    /// <param name="parent">The table transform used for positioning.</param>
    /// <param name="index">The menu index for position calculation and report tracking.</param>
    private void CreateFolderPaper(Scenario scenario, string menuName, Transform parent, int index)
    {
        print("Create folder paper");
        var pFolderPaper = Instantiate(_folderPaperRef, parent.transform);
        var (position, rotation) = CalculateFolderPaperTransform(index, parent);
        SetFolderPaperTransform(pFolderPaper, position, rotation);
        GenerateFolderPaperReport(pFolderPaper, scenario, menuName);
        _menuReports.Add(pFolderPaper);

        UpdateReport(index);
    }

    /// <summary>
    /// Computes the world position and Euler rotation for a folder paper based on the menu index.
    /// Odd-indexed menus are mirrored to the opposite side of the table.
    /// </summary>
    /// <param name="index">The menu index (even = right side, odd = left side).</param>
    /// <param name="parent">The table transform for position reference.</param>
    /// <returns>A tuple of (world position, Euler rotation) for the folder paper.</returns>
    private (Vector3 position, Vector3 rotation) CalculateFolderPaperTransform(int index, Transform parent)
    {
        var tmpPosition = new Vector3(0.52f, 0f, 0.45f);
        var position = tmpPosition;
        var tmpRotation = Vector3.zero;
        if (index % 2 != 0)
        {
            tmpPosition *= -1;
            position = new Vector3(tmpPosition.x, tmpPosition.y, tmpPosition.z);
            tmpRotation = new Vector3(0, -180, 0);
        }
        var finalPosition = new Vector3(parent.position.x + position.x, 0.9125f, parent.position.z + position.z);
        return (finalPosition, tmpRotation);
    }

    /// <summary>
    /// Applies position and rotation to a folder paper GameObject.
    /// </summary>
    /// <param name="folderPaper">The folder paper to position.</param>
    /// <param name="position">The world position to set.</param>
    /// <param name="rotation">The Euler rotation to apply.</param>
    private void SetFolderPaperTransform(GameObject folderPaper, Vector3 position, Vector3 rotation)
    {
        folderPaper.transform.position = position;
        folderPaper.transform.rotation = Quaternion.Euler(rotation);
    }

    /// <summary>
    /// Triggers the initial report generation on a folder paper's MissionRecap component.
    /// </summary>
    /// <param name="folderPaper">The folder paper GameObject containing the MissionRecap.</param>
    /// <param name="scenario">The active scenario.</param>
    /// <param name="menuName">The menu name for the report header.</param>
    private void GenerateFolderPaperReport(GameObject folderPaper, Scenario scenario, string menuName)
    {
        folderPaper.GetComponent<MissionRecap>().generateReport(scenario, menuName);
    }

    /// <summary>
    /// Creates a new transform by combining two transforms' positions (additive)
    /// and rotations (multiplicative). Returns the transform of a temporary tagged object.
    /// </summary>
    /// <param name="t1">The base transform (typically the parent).</param>
    /// <param name="t2">The offset transform to apply.</param>
    /// <returns>The combined transform from a temporary GameObject tagged "ToDelete".</returns>
    public Transform ComputeTransformWithOffset(Transform t1, Transform t2)
    {
        GameObject offsetObject = new GameObject("toDelete");
        offsetObject.tag = "ToDelete";
        offsetObject.transform.position = t1.position + t2.position;
        offsetObject.transform.rotation = t1.rotation * t2.rotation;
        return offsetObject.transform;
    }

    /// <summary>
    /// Selects a scenario by index, updates all related UI updaters and refreshes the main menu display.
    /// </summary>
    /// <param name="scenarioIndex">The index of the scenario to select from the loaded list.</param>
    public void SelectScenario(int scenarioIndex)
    {
        _scenarioSelection.Select(scenarioIndex);
    }

    /// <summary>
    /// Handles a grab event: plays the grab sound and logs whether the grabbed item
    /// matches the expected placement order.
    /// </summary>
    /// <param name="tag">The tag of the grabbed cutlery object.</param>
    public void HandleGrapEvent(string tag)
    {
        _cutleryController.HandleGrab(tag, _selectedScenario);
    }

    /// <summary>
    /// Handles a release event for a cutlery item: validates placement zone (Exam mode),
    /// checks if the item belongs to the menu, validates position/rotation/order
    /// and processes the result.
    /// </summary>
    /// <param name="tag">The tag of the released cutlery object.</param>
    public void HandleReleaseEvent(string tag)
    {
        _cutleryController.HandleRelease(tag, _selectedScenario, _lstInstantiatedPrefabs, _areCutleriesAllPlaced);
    }







    private void OnEnable()
    {
        Debug.Log("[GameManager] ACTIVE !");
    }

    private void OnDisable()
    {
        Debug.LogError("[GameManager] NOT ACTIVE !");
    }

    /// <summary>
    /// Checks whether all cutleries have been placed and triggers the end-of-scenario flow if so.
    /// </summary>
    private void CheckEndScenario()
    {
        print("CheckEndScenario");

        CheckCutleries();

        if (_areCutleriesAllPlaced )
        {
            HandleEndScenario();
        }
    }

    /// <summary>
    /// Compares the total number of cutleries to place against the total number of locked cutleries.
    /// Sets the all-placed flag if they match.
    /// </summary>
    private void CheckCutleries()
    {
        print("CheckCutleries");
        int totalCutleriesToPlace = CalculateTotalCutleriesToPlace();
        int totalLockedCutleries = CalculateTotalLockedCutleries();

        if (totalLockedCutleries == totalCutleriesToPlace)
        {
            _areCutleriesAllPlaced = true;
        }
    }

    /// <summary>
    /// Coroutine that waits 0.5 seconds before checking extra item placements.
    /// </summary>
    /// <returns>An IEnumerator for coroutine execution.</returns>
    private IEnumerator DelayedCheckExtras()
    {
        yield return new WaitForSeconds(0.5f);
        CheckExtras();
    }

    /// <summary>
    /// Checks whether all extra items defined in the scenario are correctly placed
    /// on the table and within their placement areas.
    /// </summary>
    private void CheckExtras() {
        print("CheckExtras");

        Extra[] extras = _selectedScenario.extras;
        print($"Extras {extras.Length}");
        ExtraPlacementChecker[] extraPlacements = FindObjectsOfType<ExtraPlacementChecker>();
        print($"extraPlacements {extraPlacements.Length}");

        _areExtrasAllPlaced = true;

        foreach (var extra in extras) 
        {
            print($"Extra : {extra.tag}");
            ExtraPlacementChecker extraPlacement = extraPlacements.FirstOrDefault(e => e.gameObject.CompareTag(extra.tag));
            print($"Extra OBJ : {extraPlacement.name}");
            print($"Checking {extra.tag} placement");
            if (!extraPlacement.IsCorrectlyPlaced()) 
            {
                print($"Extra {extra.tag} is NOT correctly placed.");
                _areExtrasAllPlaced = false;
                break;
            }
        }

        print("DONE");
    }

    /// <summary>
    /// Calculates the total number of cutleries to place across all menus in the scenario.
    /// </summary>
    /// <returns>The total cutlery count.</returns>
    private int CalculateTotalCutleriesToPlace()
 {
        int total = 0;
        foreach (var menu in _selectedScenario.menus)
        {
            total += menu.cutleryToPlace.Count;
        }
        return total;
    }

    /// <summary>
    /// Calculates the total number of locked (validated) cutleries across all menus.
    /// </summary>
    /// <returns>The total locked cutlery count.</returns>
    private int CalculateTotalLockedCutleries()
    {
        return _placementLocks.TotalLockedCount;
    }

    /// <summary>
    /// Finalizes the scenario: stops timers, disables interactions, validates all menu placements,
    /// plays the appropriate sound and transitions to the ScenarioEnded state.
    /// </summary>
    private void HandleEndScenario()
    {
        StopCountdownTimer();
        DisableCutleryInteractionForLockedCutleries();
        bool allMenusAreCorrectlyPlaced = ValidateMenusPlacement();

        if (allMenusAreCorrectlyPlaced)
        {
            SoundManager.Instance.PlaySuccessSound();
            _presentation.ShowCelebration(true);
        } else
        {
            SoundManager.Instance.PlayFailureSound();
        }

        NextState(GameState.ScenarioEnded);
    }

    /// <summary>
    /// Stops both the countdown and overtime timer if the countdown timer exists.
    /// </summary>
    private void StopCountdownTimer()
    {
        if (_countdownTimer != null)
        {
            _countdownTimer.StopCountdown();
            _countdownTimer.StopTimer();
        }
    }

    /// <summary>
    /// Disables XR grab interaction on all locked cutleries for the current menu.
    /// </summary>
    private void DisableCutleryInteractionForLockedCutleries()
    {
        ScenarioInteractionController.SetLockedGrabEnabled(_placementLocks.GetLockedTags(_progress.MenuIndex), false);
    }

    /// <summary>
    /// Disables XR grab interaction for all cutlery objects currently on the trolly.
    /// </summary>
    private void DisableCutleryInteractionForCutleriesOnTrolly()
    {
        print("DisableCutleryInteractionForCutleriesOnTrolly");
        ScenarioInteractionController.SetCatalogGrabEnabled(prefabManager.PrefabEntries, false);
    }

    /// <summary>
    /// Enables XR grab interaction for all cutlery objects currently on the trolly.
    /// </summary>
    private void EnableCutleryInteractionForCutleriesOnTrolly()
    {
        print("EnableCutleryInteractionForCutleriesOnTrolly");
        ScenarioInteractionController.SetCatalogGrabEnabled(prefabManager.PrefabEntries, true);
    }

    /// <summary>
    /// Validates that all menus have their cutleries correctly placed.
    /// Always returns true for non-Exam modes.
    /// </summary>
    /// <returns>True if all placements are correct or the mode does not require validation.</returns>
    private bool ValidateMenusPlacement()
    {
        if (_selectedScenario.mode != Scenario.Mode.Exam)
            return true;

        for (int i = 0; i < _placementLocks.MenuCount; i++)
        {
            if (!_placementLocks.IsMenuCorrect(i, _actionLog.GetLatestActions(i).Count))
                return false;
        }
        return true;
    }













    /// <summary>
    /// Updates the mission recap report for a specific menu with the latest action results.
    /// </summary>
    /// <param name="menuIndex">The index of the menu whose report to update.</param>
    public void UpdateReport(int menuIndex)
    {
        print($"Update report {menuIndex}");
        _menuReports[menuIndex].GetComponent<MissionRecap>().UpdateReport(_selectedScenario, _selectedScenario.menus[menuIndex].name, _actionLog.GetLatestActions(menuIndex), _menuReports[menuIndex].GetComponent<RectTransform>(), menuIndex);
    }

    /// <summary>
    /// Handles a right thumbs-up gesture: starts the scenario if the table is placed,
    /// or shows an error notification otherwise.
    /// </summary>
    public void HandleThumbsUpRight()
    {
        if (_gameStateRef == GameState.TablePlacement && _tablePlaced)
        {
            notification.DisplayNotification("Lancement du scénario", 3);
            NextState(GameState.PlayScenario);
        } else
        {
            notification.DisplayNotification("Impossible de démarrer le scénario", 3);
        }
    }

    /// <summary>
    /// Handles a left thumbs-up gesture: confirms the displayed table position once,
    /// while placement preview is active.
    /// </summary>
    public void HandleThumbsUpLeft()
    {
        if (_gameStateRef == GameState.TablePlacement)
        {
            if (_isRaycasting && !_readyToPlace && !_tablePlaced)
                HandleSuccessfulTablePlacement();
        }
    }

    /// <summary>
    /// Confirms a successful table placement: stops raycasting, marks as ready to place
    /// and disables right interactors.
    /// </summary>
    private void HandleSuccessfulTablePlacement()
    {
        DisplayNotification("Table placée", 2);
        _isRaycasting = false;
        _readyToPlace = true;
        ToggleInteractorsR(false);
    }

    /// <summary>
    /// Displays a timed notification through the Notification component.
    /// </summary>
    /// <param name="message">The notification text to display.</param>
    /// <param name="duration">The display duration in seconds.</param>
    private void DisplayNotification(string message, int duration)
    {
        notification.DisplayNotification(message, duration);
    }

    /// <summary>
    /// Handles a left thumbs-down gesture: removes the placed table if one exists.
    /// </summary>
    public void HandleThumbsDownLeft()
    {
        if (_gameStateRef == GameState.TablePlacement && _tablePlaced)
        {
            HandleTableRemoval();
        }
    }

    /// <summary>
    /// Removes the placed table: notifies the user, resets placement flags,
    /// re-enables raycasting and destroys all instantiated scene objects.
    /// </summary>
    private void HandleTableRemoval()
    {
        DisplayNotification("Table retirée", 2);
        ResetPlacementFlags();
        ActivateTablePlacementReference();
        ToggleInteractorsR(true);
        DestroyAllInstantiatedObjects();
        _jobSheet.SetActive(false);
    }

    /// <summary>
    /// Resets all placement-related flags to allow a new table placement cycle.
    /// </summary>
    private void ResetPlacementFlags()
    {
        _isRaycasting = true;
        _readyToPlace = false;
        _tablePlaced = false;
    }

    /// <summary>
    /// Re-activates the table placement preview object.
    /// </summary>
    private void ActivateTablePlacementReference()
    {
        _tablePlacementRef.SetActive(true);
    }

    /// <summary>
    /// Destroys all instantiated scene objects: prefab dictionaries, menu reports,
    /// tables, menus, trollies and chairs.
    /// </summary>
    private void DestroyAllInstantiatedObjects()
    {
        print("DestroyAllInstantiatedObjects");
        ScenarioEnvironmentCleanup.ClearPlacedEnvironment(_lstInstantiatedPrefabs, _menuReports);
        _lstInstantiatedPrefabs = new List<Dictionary<string, GameObject>>();
        _menuReports = new List<GameObject>();
    }

    /// <summary>Opens the tray when the left palm gesture is detected during play.</summary>
    public void HandleOpenPalmUpLeftPerformed()
    {
        if (_gameStateRef == GameState.PlayScenario)
            _trayController.Open();
    }

    /// <summary>Closes the tray when the left palm gesture ends during play.</summary>
    public void HandleOpenPalmUpLeftEnded()
    {
        if (_gameStateRef == GameState.PlayScenario)
            _trayController.Close();
    }

    /// <summary>
    /// Adds default (all-incorrect) scenario actions for every cutlery in the specified menu.
    /// These serve as baseline entries before actual placements occur.
    /// </summary>
    /// <param name="menuIndex">The index of the menu to add default actions for.</param>
    public void AddDefaultActionsForMenu(int menuIndex)
    {
        print($"AddDefaultActionsForMenu {menuIndex}");

        _actionLog.AddDefaults(menuIndex, _selectedScenario.menus[menuIndex]);
    }

    /// <summary>
    /// Reloads the entire game by resetting flags and loading the main menu scene.
    /// </summary>
    public void ReloadGame()
    {
        ResetPlacementFlags();
        ResetFlagsAndReferences();
        SceneManager.LoadScene("MainMenuScene");
    }

    /// <summary>
    /// Reloads the current scenario: notifies the user, resets all state,
    /// destroys old objects and sets up a fresh scenario at the same table position.
    /// </summary>
    public void ReloadScenario()
    {
        print("ReloadScenario");
        DisplayReloadNotification();
        TogglePauseMenu();
        DestroyAllInstantiatedObjects();
        ResetFlagsAndReferences();
        ResetStepIndex();
        CleanupOldObjects();
        SetupNewScenario();
    }

    /// <summary>
    /// Displays a notification informing the user that the scenario is being reloaded.
    /// </summary>
    private void DisplayReloadNotification()
    {
        notification.DisplayNotification("Rechargement du scénario", 2);
    }

    /// <summary>
    /// Resets all game state flags, UI elements and tracking collections to their initial values.
    /// </summary>
    private void ResetFlagsAndReferences()
    {
        gameEnded = false;
        _hands.SetLeftHandVisible(true);
        _presentation.ShowCelebration(false);
        _presentation.ShowCelebrationTip(false);
        _progress.ResetMenus();
        _actionLog.Reset();
        _placementLocks.Reset();
        _isTenSecondsAchieved = false;
        _placementZones.Reset();
        _areCutleriesAllPlaced = false;
        _areExtrasAllPlaced = false;
    }

    /// <summary>
    /// Resets the step index to zero on both the internal counter and the step interface updater.
    /// </summary>
    private void ResetStepIndex()
    {
        _progress.ResetSteps();
        _stepInterfaceUpdater.SetStepIndex(_progress.StepIndex);
    }

    /// <summary>
    /// Destroys leftover objects from the previous scenario: end-game reports, the tray and locked items.
    /// </summary>
    private void CleanupOldObjects()
    {
        _trayController?.Close();
        ScenarioEnvironmentCleanup.ClearSessionObjects(_pTray);
        _trayController = null;
    }

    /// <summary>
    /// Sets up a fresh scenario: creates a new tray, places the table and cutleries
    /// at the saved position and transitions to the PlayScenario state.
    /// </summary>
    private void SetupNewScenario()
    {
        print("SetupNewScenario");
        CreateTray();
        PlaceTableAndCutleries(_tableTransform.position, _tableTransform.rotation);
        NextState(GameState.PlayScenario);
    }

    /// <summary>
    /// Validates the current menu in Exam mode: destroys placement guides,
    /// locks remaining cutleries as incorrect, closes the pause menu
    /// and proceeds to the next menu or ends the scenario.
    /// </summary>
    public void ValidateCurrentMenu()
    {
        DestroyUnnecessaryPrefabs();
        LockCutleryForCurrentMenu();
        TogglePauseMenu();
        ProceedToNextStepOrEndScenario();
    }

    /// <summary>
    /// Destroys all instantiated placement guide prefabs for the current menu,
    /// excluding the table and trolly references.
    /// </summary>
    private void DestroyUnnecessaryPrefabs()
    {
        ScenarioEnvironmentCleanup.ClearMenuGuides(_lstInstantiatedPrefabs[_progress.MenuIndex]);
    }

    /// <summary>
    /// Marks all unplaced cutleries in the current menu as locked with an incorrect (false) status.
    /// </summary>
    private void LockCutleryForCurrentMenu()
    {
        _placementLocks.FinalizeMissing(_progress.MenuIndex,
            _selectedScenario.menus[_progress.MenuIndex].cutleryToPlace.Keys);
    }

    /// <summary>
    /// Determines whether to proceed to the next menu or trigger the end-scenario check
    /// based on whether the current menu is the last one.
    /// </summary>
    private void ProceedToNextStepOrEndScenario()
    {
        print("ProceedToNextStepOrEndScenario");
        bool cond = _progress.IsLastMenu(_selectedScenario.menus.Length);

        if (cond)
        {
            CheckEndScenario();
        }
        else
        {
            InitNextMenu();
        }
    }

    /// <summary>Toggles the pause menu only while playing or showing the scenario results.</summary>
    public void TogglePauseMenu()
    {
        _pauseController.UpdateValidateButton(_selectedScenario.mode,
            GameObject.FindGameObjectsWithTag("EndGameReport").Length);
        if (_gameStateRef != GameState.PlayScenario && _gameStateRef != GameState.ScenarioEnded)
            return;
        if (_pauseController.IsOpen)
            ClosePauseMenu();
        else
            OpenPauseMenu();
    }

    /// <summary>Opens pause, enables menu interactions and suspends the timer.</summary>
    public void OpenPauseMenu()
    {
        _pauseController.Open(_countdownTimer);
    }

    /// <summary>Closes pause and resumes the timer only if the scenario is still running.</summary>
    public void ClosePauseMenu()
    {
        _pauseController.Close(_countdownTimer, !gameEnded && _gameStateRef != GameState.ScenarioEnded);
    }

}
