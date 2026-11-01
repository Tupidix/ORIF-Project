using System.Collections.Generic;
using UnityEngine;

/// <summary>Owns destruction operations for the scenario environment, independently of game state.</summary>
public static class ScenarioEnvironmentCleanup
{
    public static void ClearTrolly(Dictionary<string, GameObject> groups)
    {
        foreach (GameObject group in groups.Values)
        {
            if (group != null)
                Object.Destroy(group);
        }
        groups.Clear();
    }

    public static void ClearPlacedEnvironment(List<Dictionary<string, GameObject>> menus,
        List<GameObject> reports)
    {
        if (menus != null)
            Helper.DestroyObjectsFromListOfDictionnaries(menus);
        Helper.DestroyObjectsFromList(reports);
        // Preserve immediate removal of table/menu roots before a same-frame rebuild.
        Helper.DestroyImmediateObjectsWithTag("Table");
        Helper.DestroyImmediateObjectsWithTag("Menu");
        Helper.DestroyObjectsWithTag("Trolly");
        Helper.DestroyObjectsWithTag("Chair");
    }

    public static void ClearSessionObjects(GameObject tray)
    {
        Helper.DestroyObjectsWithTag("EndGameReport");
        Object.Destroy(tray);
        Helper.DestroyObjectsWithTag("Locked");
    }

    public static void ClearMenuGuides(Dictionary<string, GameObject> objects)
    {
        foreach (var entry in objects)
        {
            if (entry.Key != "Table" && entry.Key != "Trolly")
                Object.Destroy(entry.Value);
        }
    }
}
