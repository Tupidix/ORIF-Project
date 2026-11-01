using System.Collections.Generic;
using UnityEngine;

/// <summary>Owns the action history for one game session, independently of scene objects.</summary>
public sealed class ScenarioActionLog
{
    // Kept in the existing format consumed by MissionRecap and EndGameReport.
    public List<List<ScenarioAction>> Records { get; private set; } = new List<List<ScenarioAction>>();

    public void Reset()
    {
        Records = new List<List<ScenarioAction>>();
    }

    public void AddMenu()
    {
        Records.Add(new List<ScenarioAction>());
    }

    public void AddDefaults(int menuIndex, Menu menu)
    {
        foreach (var entry in menu.cutleryToPlace)
        {
            Records[menuIndex].Add(new ScenarioAction(false, false, false, entry.Key,
                Vector3.zero, Quaternion.identity, entry.Value.placementOrderIndex));
        }
    }

    public void RecordPlacement(int menuIndex, bool positionCorrect, bool rotationCorrect,
        bool orderCorrect, string tag, Vector3 position, Quaternion rotation, int order)
    {
        Records[menuIndex].Add(new ScenarioAction(positionCorrect, rotationCorrect, orderCorrect,
            tag, position, rotation, order));
    }

    public void RecordUnexpectedItem(int menuIndex, string tag, Vector3 position, Quaternion rotation)
    {
        RecordPlacement(menuIndex, false, false, false, tag, position, rotation, -1);
    }

    public List<ScenarioAction> GetLatestActions(int menuIndex)
    {
        return GetLatestActions(Records[menuIndex]);
    }

    public static List<ScenarioAction> GetLatestActions(IEnumerable<ScenarioAction> actions)
    {
        var latest = new Dictionary<string, ScenarioAction>();
        foreach (ScenarioAction action in actions)
            latest[action.tag] = action;
        return new List<ScenarioAction>(latest.Values);
    }
}
