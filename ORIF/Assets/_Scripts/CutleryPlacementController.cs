using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Coordinates grab/release validation, history and placement feedback.</summary>
public sealed class CutleryPlacementController
{
    private readonly ScenarioProgress _progress;
    private readonly ScenarioActionLog _actionLog;
    private readonly ScenarioPlacementLocks _placementLocks;
    private readonly Material _defaultCutleryMaterialRef;
    private readonly Material _defaultGlassMaterialRef;
    private readonly Action<GameObject> _prepareExamRelease;
    private readonly Action<int> _updateReport;
    private readonly Action _advanceStep;
    private readonly Action _checkEnd;
    private Scenario _scenario;
    private List<Dictionary<string, GameObject>> _placementGuides;

    public CutleryPlacementController(ScenarioProgress progress, ScenarioActionLog actionLog,
        ScenarioPlacementLocks placementLocks, Material cutleryMaterial, Material glassMaterial,
        Action<GameObject> prepareExamRelease, Action<int> updateReport, Action advanceStep, Action checkEnd)
    {
        _progress = progress;
        _actionLog = actionLog;
        _placementLocks = placementLocks;
        _defaultCutleryMaterialRef = cutleryMaterial;
        _defaultGlassMaterialRef = glassMaterial;
        _prepareExamRelease = prepareExamRelease;
        _updateReport = updateReport;
        _advanceStep = advanceStep;
        _checkEnd = checkEnd;
    }

    public void HandleGrab(string tag, Scenario scenario)
    {
        _scenario = scenario;
        SoundManager.Instance.PlayGrapSound();
        Debug.Log($"[{tag}] - Current step {_progress.StepIndex} | Grapped element index {_scenario.menus[_progress.MenuIndex].cutleryToPlace[tag].placementOrderIndex}");

        if (_scenario.menus[_progress.MenuIndex].cutleryToPlace[tag].placementOrderIndex != _progress.StepIndex)
        {
            Debug.Log("This isn't the cultery to place..!");
        }
        else
        {
            Debug.Log("This is the cultery to place !");
        }
    }

    private bool IsExtra(string tag)
    {
        foreach (var extra in _scenario.extras)
        {
            if (extra.tag == tag) return true;
        }
        return false;
    }

    public void HandleRelease(string tag, Scenario scenario, List<Dictionary<string, GameObject>> placementGuides, bool allCutleriesPlaced)
    {
        _scenario = scenario;
        _placementGuides = placementGuides;
        Debug.Log("Released");

        if (allCutleriesPlaced) {
            _checkEnd();
            return;
        }

        SoundManager.Instance.PlayReleaseSound(tag);
        var objects = GameObject.FindGameObjectsWithTag(tag);

        if (objects.Length == 0)
        {
            Debug.Log($"There isn't any {tag}");
            return;
        }

        if (_scenario.mode == Scenario.Mode.Exam)
        {
            _prepareExamRelease(objects[0]);
        }

        if (!IsCutleryInMenu(tag))
        {
            if (!IsExtra(tag)) { 
                AddActionForMissingCutlery(tag, objects[0]);
                return;
            }
        }

        var pCutlery = _placementGuides[_progress.MenuIndex][tag];
        if (pCutlery == null)
        {
            Debug.Log($"pCutlery is null. Menu INDEX [{_progress.MenuIndex}] TAG [{tag}]");
            return;
        }

        var index = Helper.GetIndexOfObject(objects, pCutlery);

        if (tag == "Plate")
        {
            Helper.ShiftObjectsPositionBasedOnPlate(objects, _progress.MenuIndex, _placementGuides);
        }

        if (IsValid(objects, _scenario.menus[_progress.MenuIndex].cutleryToPlace[tag].placementOrderIndex, _progress.StepIndex))
        {
            HandleValidCutlery(objects, index);
        }
    }

    private bool IsCutleryInMenu(string tag)
    {
        return _placementGuides[_progress.MenuIndex].ContainsKey(tag);
    }

    private void AddActionForMissingCutlery(string tag, GameObject obj)
    {
        _actionLog.RecordUnexpectedItem(_progress.MenuIndex, obj.tag, obj.transform.position, obj.transform.rotation);
        Debug.Log($"Added action to the menu but the cultery {tag} is not in the menu.");
    }

    private void HandleValidCutlery(GameObject[] objects, int index)
    {
        if (_scenario.mode != Scenario.Mode.Exam)
            SoundManager.Instance.PlayLockedSound();

        Debug.Log("Destroying.");
        UnityEngine.Object.Destroy(objects[index]);
        _advanceStep();
    }

    private bool IsValid(GameObject[] objects, int pickedUpOrderIndex, int currentOrderIndex)
    {
        Debug.Log("Checking validity");

        if (!AreObjectsCountValid(objects.Length))
        {
            return false;
        }

        var (distance, rotation) = CalculateDistanceAndRotation(objects);
        var result = CutleryPlacementValidator.Validate(_scenario, objects[0].tag,
            distance, rotation, pickedUpOrderIndex, currentOrderIndex);

        Helper.LogValidationResults(distance, rotation, currentOrderIndex, pickedUpOrderIndex);

        AddScenarioAction(result.PositionCorrect, result.RotationCorrect, result.OrderCorrect, objects, pickedUpOrderIndex);

        if (IsCutleryPlacementCorrect(result.OrderCorrect, result.PositionCorrect, result.RotationCorrect, objects))
        {
            LockCutlery(objects);
        }

        if (_scenario.mode != Scenario.Mode.Exam)
        {
            _updateReport(_progress.MenuIndex);
        }

        return result.IsCorrect;
    }

    private bool AreObjectsCountValid(int objectsCount)
    {
        if (objectsCount > _scenario.menus.Length + 1) // + 1 because of the trolly culteries
        {
            Debug.Log("Objects length are greater than " + (_scenario.menus.Length + 1) + "!");
            return false;
        }
        return true;
    }

    private (float distance, float rotation) CalculateDistanceAndRotation(GameObject[] objects)
    {
        var distance = Vector3.Distance(objects[0].transform.position, objects[1].transform.position);
        var rotation = Helper.GetRotationDifference(objects[0].transform.rotation, objects[1].transform.rotation);
        return (distance, rotation);
    }

    private void AddScenarioAction(bool correctDistance, bool correctRotation, bool correctOrder, GameObject[] objects, int pickedUpOrderIndex)
    {
        _actionLog.RecordPlacement(_progress.MenuIndex, correctDistance, correctRotation, correctOrder, objects[0].tag, objects[1].transform.position, objects[1].transform.rotation, pickedUpOrderIndex);
        Debug.Log("Action added");
    }

    private bool IsCutleryPlacementCorrect(bool correctOrder, bool correctDistance, bool correctRotation, GameObject[] objects)
    {
        return !_placementLocks.IsLocked(_progress.MenuIndex, objects[0].tag) && (correctOrder && correctDistance && correctRotation);
    }

    private void LockCutlery(GameObject[] objects)
    {
        _placementLocks.LockCorrect(_progress.MenuIndex, objects[0].tag);
        ScenarioInteractionController.ApplyPlacementLock(objects, _progress.MenuIndex,
            _defaultCutleryMaterialRef, _defaultGlassMaterialRef);
    }
}
