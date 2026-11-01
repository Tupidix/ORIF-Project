using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

/// <summary>
/// Static utility class providing common operations for tag conversion,
/// geometry, materials, GameObject management and UI placement.
/// </summary>
public static class Helper
{
    private static readonly Dictionary<string, string> TagToFrenchNameMap = new Dictionary<string, string>
    {
        { "Plate", "Assiette" },
        { "ButterKnife", "Couteau à beurre" },
        { "CarvingFork", "Fourchette à découper" },
        { "DessertFork", "Fourchette à dessert" },
        { "DessertKnife", "Couteau à dessert" },
        { "DessertSpoon", "Cuillère à dessert" },
        { "DinnerFork", "Fourchette à diner" },
        { "DinnerKnife", "Couteau à diner" },
        { "DinnerSpoon", "Cuillère à diner" },
        { "FishFork", "Fourchette à poisson" },
        { "FishKnife", "Couteau à poisson" },
        { "FruitFork", "Fourchette à fruit" },
        { "FruitKnife", "Couteau à fruit" },
        { "FruitSpoon", "Cuillère à fruit" },
        { "GlassWater", "Verre à eau" },
        { "GlassWine", "Verre à vin" },
        { "SaladFork", "Fourchette à salade" },
        { "SteakKnife", "Couteau à viande" },
        { "TeaSpoon", "Cuillère à thé" },
        { "Table", "Table" }
    };

    private const string DefaultFrenchName = "N/A";
    private const float PlateProximityThreshold = 0.15f;

    // --- Conversion & Formatting ---

    /// <summary>
    /// Converts a cutlery tag to its French display name.
    /// Returns "N/A" if the tag is not found.
    /// </summary>
    /// <param name="tag">The internal tag identifier to convert.</param>
    /// <returns>The French name corresponding to the tag, or "N/A".</returns>
    public static string ConvertTagToFrenchName(string tag)
    {
        return TagToFrenchNameMap.TryGetValue(tag, out var name) ? name : DefaultFrenchName;
    }

    /// <summary>
    /// Formats a time value in seconds into a "MM:SS" string.
    /// </summary>
    /// <param name="time">The time in seconds to format.</param>
    /// <returns>A formatted string in "MM:SS" format.</returns>
    public static string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60F);
        int seconds = Mathf.FloorToInt(time - minutes * 60);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    /// <summary>
    /// Rounds a slider value up to the nearest multiple of 5.
    /// </summary>
    /// <param name="value">The raw slider value.</param>
    /// <returns>The value rounded up to the nearest multiple of 5.</returns>
    public static float RoundUpSliderValue(float value)
    {
        return Mathf.Ceil(value / 5) * 5;
    }

    // --- Position Description ---

    /// <summary>
    /// Builds a human-readable French position description from a transform offset.
    /// </summary>
    /// <param name="offset">The transform whose local position defines the offset.</param>
    /// <param name="menuIndex">The menu index used to compute the direction multiplier.</param>
    /// <returns>A trimmed French string describing the relative position.</returns>
    public static string GetPositionDescription(Transform offset, int menuIndex)
    {
        var direction = ComputeDirectionMultiplier(menuIndex);

        string horizontal = GetHorizontalLabel(offset.localPosition.x * direction);
        string vertical = GetVerticalLabel(offset.localPosition.z * direction);
        string depth = GetDepthLabel(offset.localPosition.y * direction);

        return CombinePositionLabels(horizontal, vertical, depth);
    }

    /// <summary>
    /// Returns the direction multiplier used to interpret position offsets.
    /// </summary>
    /// <param name="menuIndex">The menu index (currently unused, always returns -1).</param>
    /// <returns>The direction multiplier value.</returns>
    private static int ComputeDirectionMultiplier(int menuIndex)
    {
        return -1;
    }

    /// <summary>
    /// Returns the horizontal label based on a signed axis value.
    /// </summary>
    /// <param name="value">The signed horizontal value.</param>
    /// <returns>"a gauche", "a droite", or an empty string.</returns>
    private static string GetHorizontalLabel(float value)
    {
        if (value < 0) return "à gauche";
        if (value > 0) return "à droite";
        return "";
    }

    /// <summary>
    /// Returns the vertical label based on a signed axis value.
    /// </summary>
    /// <param name="value">The signed vertical value.</param>
    /// <returns>"en bas", "en haut", or an empty string.</returns>
    private static string GetVerticalLabel(float value)
    {
        if (value < 0) return "en bas";
        if (value > 0) return "en haut";
        return "";
    }

    /// <summary>
    /// Returns the depth label based on a signed axis value.
    /// </summary>
    /// <param name="value">The signed depth value.</param>
    /// <returns>"au dessus", "en dessous", or an empty string.</returns>
    private static string GetDepthLabel(float value)
    {
        if (value < 0) return "au dessus";
        if (value > 0) return "en dessous";
        return "";
    }

    /// <summary>
    /// Joins position label parts into a single trimmed string.
    /// </summary>
    /// <param name="labels">The label parts to combine.</param>
    /// <returns>A single space-separated, trimmed string.</returns>
    private static string CombinePositionLabels(params string[] labels)
    {
        return string.Join(" ", labels).Trim();
    }

    // --- Geometry & AR Planes ---

    /// <summary>
    /// Checks whether a world-space point lies within the boundary of an AR plane.
    /// </summary>
    /// <param name="point">The world-space point to test.</param>
    /// <param name="plane">The AR plane to test against.</param>
    /// <returns>True if the point is inside the plane boundary.</returns>
    public static bool IsPointOnPlane(Vector3 point, ARPlane plane)
    {
        Vector3 localPoint = plane.transform.InverseTransformPoint(point);
        Vector2 localPoint2D = new Vector2(localPoint.x, localPoint.z);
        return IsPointInPolygon(localPoint2D, plane.boundary);
    }

    /// <summary>
    /// Determines whether a 2D point is inside a polygon using the ray-casting algorithm.
    /// </summary>
    /// <param name="point">The 2D point to test.</param>
    /// <param name="polygon">The polygon vertices as a native array.</param>
    /// <returns>True if the point is inside the polygon.</returns>
    public static bool IsPointInPolygon(Vector2 point, NativeArray<Vector2> polygon)
    {
        bool isInside = false;
        int j = polygon.Length - 1;

        for (int i = 0; i < polygon.Length; j = i++)
        {
            if (IsRayCrossingEdge(point, polygon[i], polygon[j]))
            {
                isInside = !isInside;
            }
        }

        return isInside;
    }

    /// <summary>
    /// Checks whether a horizontal ray from the point crosses the given polygon edge.
    /// </summary>
    /// <param name="point">The origin point of the horizontal ray.</param>
    /// <param name="edgeStart">The start vertex of the polygon edge.</param>
    /// <param name="edgeEnd">The end vertex of the polygon edge.</param>
    /// <returns>True if the ray crosses this edge.</returns>
    private static bool IsRayCrossingEdge(Vector2 point, Vector2 edgeStart, Vector2 edgeEnd)
    {
        bool isWithinYBounds = (edgeStart.y > point.y) != (edgeEnd.y > point.y);
        float intersectionX = (edgeEnd.x - edgeStart.x) * (point.y - edgeStart.y) / (edgeEnd.y - edgeStart.y) + edgeStart.x;
        return isWithinYBounds && point.x < intersectionX;
    }

    /// <summary>
    /// Returns the angular difference in degrees between two quaternions.
    /// </summary>
    /// <param name="rot1">The first rotation.</param>
    /// <param name="rot2">The second rotation.</param>
    /// <returns>The angle in degrees between the two rotations.</returns>
    public static float GetRotationDifference(Quaternion rot1, Quaternion rot2)
    {
        return Quaternion.Angle(rot1, rot2);
    }

    // --- MeshRenderer & Materials ---

    /// <summary>
    /// Finds the MeshRenderer of a child named "SM_{tag}" within the instantiated cutlery prefabs.
    /// </summary>
    /// <param name="tag">The cutlery tag used to build the child name.</param>
    /// <param name="instantiatedCutleriesPrefabs">The dictionary of instantiated prefab groups.</param>
    /// <returns>The MeshRenderer of the matching child, or null if not found.</returns>
    public static MeshRenderer GetMeshRendererByTag(string tag, Dictionary<string, GameObject> instantiatedCutleriesPrefabs)
    {
        foreach (var entry in instantiatedCutleriesPrefabs)
        {
            var child = entry.Value.transform.Find("SM_" + tag);
            if (child == null) continue;
            return child.GetComponent<MeshRenderer>();
        }
        return null;
    }

    /// <summary>
    /// Applies a highlight material to the cutlery item at the given step index.
    /// </summary>
    /// <param name="steps">The list of placement steps.</param>
    /// <param name="stepIndex">The index of the current step.</param>
    /// <param name="culteries">The dictionary of instantiated cutlery prefabs.</param>
    /// <param name="material">The material to apply as highlight.</param>
    public static void SetCurrentItemMaterial(List<StepUserInterfaceUpdater.Step> steps, int stepIndex, Dictionary<string, GameObject> culteries, Material material)
    {
        var currentItemName = steps[stepIndex].itemName;
        var meshRenderer = GetMeshRendererByTag(currentItemName, culteries);
        if (meshRenderer)
            ReplaceLastMaterial(meshRenderer, material);
    }

    /// <summary>
    /// Adds a material to the last slot of the given MeshRenderer.
    /// </summary>
    /// <param name="meshRenderer">The target MeshRenderer.</param>
    /// <param name="material">The material to set in the last slot.</param>
    public static void AddCurrentItemMaterial(MeshRenderer meshRenderer, Material material)
    {
        ReplaceLastMaterial(meshRenderer, material);
    }

    /// <summary>
    /// Removes (replaces) the last material slot of the given MeshRenderer.
    /// </summary>
    /// <param name="meshRenderer">The target MeshRenderer.</param>
    /// <param name="material">The material to set in the last slot.</param>
    public static void RemoveCurrentItemMaterial(MeshRenderer meshRenderer, Material material)
    {
        ReplaceLastMaterial(meshRenderer, material);
    }

    /// <summary>
    /// Replaces the last material in the MeshRenderer's material array.
    /// </summary>
    /// <param name="meshRenderer">The MeshRenderer to modify.</param>
    /// <param name="material">The replacement material.</param>
    private static void ReplaceLastMaterial(MeshRenderer meshRenderer, Material material)
    {
        Material[] currentMaterials = meshRenderer.materials;
        currentMaterials[currentMaterials.Length - 1] = material;
        meshRenderer.materials = currentMaterials;
    }

    // --- GameObject Destruction ---

    /// <summary>
    /// Destroys all GameObjects in the scene that have the specified tag.
    /// </summary>
    /// <param name="tag">The tag to search for.</param>
    public static void DestroyObjectsWithTag(string tag)
    {
        foreach (var obj in GameObject.FindGameObjectsWithTag(tag))
        {
            GameObject.Destroy(obj);
        }
    }

    /// <summary>
    /// Immediately destroys all GameObjects in the scene that have the specified tag.
    /// </summary>
    /// <param name="tag">The tag to search for.</param>
    public static void DestroyImmediateObjectsWithTag(string tag)
    {
        foreach (var obj in GameObject.FindGameObjectsWithTag(tag))
        {
            GameObject.DestroyImmediate(obj);
        }
    }

    /// <summary>
    /// Destroys all GameObjects contained in the provided list.
    /// </summary>
    /// <param name="gameObjects">The list of GameObjects to destroy.</param>
    public static void DestroyObjectsFromList(List<GameObject> gameObjects)
    {
        foreach (var obj in gameObjects)
        {
            GameObject.Destroy(obj);
        }
    }

    /// <summary>
    /// Destroys all GameObjects stored as values in a list of dictionaries.
    /// </summary>
    /// <param name="gameObjects">The list of dictionaries containing GameObjects to destroy.</param>
    public static void DestroyObjectsFromListOfDictionnaries(List<Dictionary<string, GameObject>> gameObjects)
    {
        foreach (var dictionary in gameObjects)
        {
            foreach (var entry in dictionary)
            {
                GameObject.Destroy(entry.Value);
            }
        }
    }

    // --- Search & Index ---

    /// <summary>
    /// Returns the index of the GameObject whose transform matches the target.
    /// Returns the array length if no match is found.
    /// </summary>
    /// <param name="objects">The array of GameObjects to search.</param>
    /// <param name="pCultery">The target GameObject to find.</param>
    /// <returns>The index of the matching object, or the array length if not found.</returns>
    public static int GetIndexOfObject(GameObject[] objects, GameObject pCultery)
    {
        for (int i = 0; i < objects.Length; i++)
        {
            if (objects[i].transform == null) Debug.Log("NULL transform");
            if (pCultery.transform == null) Debug.Log("NULL transform pcultery");
            if (objects[i].transform == pCultery.transform) return i;
        }
        return objects.Length;
    }

    // --- Toggle ---

    /// <summary>
    /// Sets the active state of all GameObjects in the array.
    /// </summary>
    /// <param name="objects">The array of GameObjects to toggle.</param>
    /// <param name="state">True to activate, false to deactivate.</param>
    public static void ToggleArrayOfGameObjects(GameObject[] objects, bool state)
    {
        foreach (var obj in objects)
        {
            obj.SetActive(state);
        }
    }

    // --- Offset & Positions ---

    /// <summary>
    /// Creates a new Offset from a position and rotation.
    /// </summary>
    /// <param name="position">The position component of the offset.</param>
    /// <param name="rotation">The rotation component of the offset.</param>
    /// <returns>A new Offset instance.</returns>
    public static Offset CreateOffset(Vector3 position, Quaternion rotation)
    {
        return new Offset
        {
            position = position,
            rotation = rotation
        };
    }

    /// <summary>
    /// Collects the initial world position of each cutlery prefab found in the scene by tag.
    /// </summary>
    /// <param name="culteries">The list of prefab entries defining which tags to look for.</param>
    /// <returns>A dictionary mapping each tag to its initial world position.</returns>
    public static Dictionary<string, Vector3> GetCutleriesInitialPosition(IReadOnlyList<PrefabCutleryManager.PrefabEntry> culteries)
    {
        Debug.Log("GetCutleriesInitialPosition");
        var positions = new Dictionary<string, Vector3>();
        foreach (var entry in culteries)
        {
            var objects = GameObject.FindGameObjectsWithTag(entry.tag);
            if (objects.Length > 0)
                positions.Add(entry.tag, objects[0].transform.position);
        }
        return positions;
    }

    /// <summary>
    /// Shifts cutlery positions when two plate objects are closer than the proximity threshold.
    /// </summary>
    /// <param name="objects">The array of plate GameObjects (expects at least 2).</param>
    /// <param name="menuIndex">The current menu index, used to determine shift direction.</param>
    /// <param name="culteries">The list of cutlery dictionaries per menu.</param>
    public static void ShiftObjectsPositionBasedOnPlate(GameObject[] objects, int menuIndex, List<Dictionary<string, GameObject>> culteries)
    {
        var distance = Vector3.Distance(objects[0].transform.position, objects[1].transform.position);
        if (distance > PlateProximityThreshold) return;

        var offset = objects[0].transform.position - objects[1].transform.position;
        bool shouldInvertDirection = menuIndex % 2 != 0;

        foreach (var cutlery in culteries[menuIndex])
        {
            if (cutlery.Key == null || cutlery.Value == null) continue;
            if (IsNonMovableObject(cutlery.Value)) continue;

            ApplyPositionOffset(cutlery.Value, offset, shouldInvertDirection);
        }
    }

    /// <summary>
    /// Returns true if the object should not be moved (tagged Trolly or Table).
    /// </summary>
    /// <param name="obj">The GameObject to check.</param>
    /// <returns>True if the object is non-movable.</returns>
    private static bool IsNonMovableObject(GameObject obj)
    {
        return obj.CompareTag("Trolly") || obj.CompareTag("Table");
    }

    /// <summary>
    /// Applies a horizontal position offset to a GameObject, optionally inverted.
    /// </summary>
    /// <param name="obj">The GameObject to move.</param>
    /// <param name="offset">The position offset to apply.</param>
    /// <param name="invert">If true, the offset direction is inverted.</param>
    private static void ApplyPositionOffset(GameObject obj, Vector3 offset, bool invert)
    {
        float sign = invert ? -1f : 1f;
        var pos = obj.transform.position;
        obj.transform.position = new Vector3(
            pos.x + offset.x * sign,
            pos.y,
            pos.z + offset.z * sign
        );
    }

    // --- Scenario Actions ---

    /// <summary>
    /// Returns the last ScenarioAction recorded for each unique tag in the given menu.
    /// </summary>
    /// <param name="menuIndex">The index of the menu to query.</param>
    /// <param name="menusActions">The list of action lists, one per menu.</param>
    /// <returns>A list containing the latest action for each tag.</returns>
    public static List<ScenarioAction> GetLastActionsOfEachType(int menuIndex, List<List<ScenarioAction>> menusActions)
    {
        return ScenarioActionLog.GetLatestActions(menusActions[menuIndex]);
    }

    /// <summary>
    /// Returns the appropriate prefab tag based on the scenario difficulty level.
    /// </summary>
    /// <param name="scenario">The current scenario.</param>
    /// <param name="defaultTag">The default tag to use for beginner level.</param>
    /// <returns>The resolved tag string.</returns>
    public static string GetTagBasedOnScenarioLevel(Scenario scenario, string defaultTag)
    {
        return scenario.level switch
        {
            Scenario.Level.Begginner => defaultTag,
            Scenario.Level.Medium => "Hint",
            Scenario.Level.Expert => "None",
            _ => defaultTag
        };
    }

    // --- 3D Space UI ---

    /// <summary>
    /// Places a UI element in front of the camera at the given distance and height.
    /// </summary>
    /// <param name="uiElement">The UI GameObject to position.</param>
    /// <param name="mainCamera">The main camera used as reference.</param>
    /// <param name="distance">The forward distance from the camera.</param>
    /// <param name="height">The world-space Y height to place the element at.</param>
    public static void PlaceUIElement(GameObject uiElement, Camera mainCamera, float distance, float height)
    {
        if (!IsActiveUIElement(uiElement)) return;

        Vector3 forwardPosition = mainCamera.transform.position + mainCamera.transform.forward * distance;
        uiElement.transform.position = new Vector3(forwardPosition.x, height, forwardPosition.z);
    }

    /// <summary>
    /// Rotates a UI element (or a specific child) to face the player camera.
    /// </summary>
    /// <param name="uiElement">The UI GameObject to rotate.</param>
    /// <param name="zeroY">If true, zeroes out the Y component of the look direction.</param>
    /// <param name="isTimer">If true, rotates all child TextMeshProUGUI components individually.</param>
    /// <param name="childName">If provided, only this named child is rotated.</param>
    public static void RotateUIElement(GameObject uiElement, bool zeroY, bool isTimer = false, string childName = null)
    {
        if (!IsActiveUIElement(uiElement)) return;

        var directionToPlayer = GetDirectionToPlayer(uiElement.transform);
        if (zeroY) directionToPlayer.y = 0;

        if (isTimer)
        {
            RotateAllTimerTexts(uiElement, directionToPlayer);
        }
        else if (!string.IsNullOrEmpty(childName))
        {
            RotateChild(uiElement, childName, directionToPlayer);
        }
        else
        {
            uiElement.transform.rotation = Quaternion.LookRotation(directionToPlayer);
        }
    }

    /// <summary>
    /// Returns true if the UI element is not null and currently active.
    /// </summary>
    /// <param name="uiElement">The UI GameObject to check.</param>
    /// <returns>True if the element exists and is active.</returns>
    private static bool IsActiveUIElement(GameObject uiElement)
    {
        return uiElement != null && uiElement.activeSelf;
    }

    /// <summary>
    /// Rotates all TextMeshProUGUI children of a timer element to face a direction.
    /// </summary>
    /// <param name="uiElement">The parent timer GameObject.</param>
    /// <param name="direction">The direction to face.</param>
    private static void RotateAllTimerTexts(GameObject uiElement, Vector3 direction)
    {
        foreach (var text in uiElement.transform.GetComponentsInChildren<TextMeshProUGUI>())
        {
            text.transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    /// <summary>
    /// Rotates a specific named child of a UI element to face a direction.
    /// </summary>
    /// <param name="uiElement">The parent UI GameObject.</param>
    /// <param name="childName">The name of the child to rotate.</param>
    /// <param name="direction">The direction to face.</param>
    private static void RotateChild(GameObject uiElement, string childName, Vector3 direction)
    {
        uiElement.transform.Find(childName).transform.rotation = Quaternion.LookRotation(direction);
    }

    /// <summary>
    /// Returns the normalized direction from the main camera to the given transform.
    /// </summary>
    /// <param name="t">The target transform.</param>
    /// <returns>A normalized direction vector.</returns>
    private static Vector3 GetDirectionToPlayer(Transform t)
    {
        return (t.position - Camera.main.transform.position).normalized;
    }

    // --- Logging ---

    /// <summary>
    /// Logs the validation results for distance, rotation and order to the console.
    /// </summary>
    /// <param name="distance">The measured distance between objects.</param>
    /// <param name="rotation">The measured rotation difference in degrees.</param>
    /// <param name="currentOrderIndex">The expected placement order index.</param>
    /// <param name="pickedUpOrderIndex">The actual order index of the picked up item.</param>
    public static void LogValidationResults(float distance, float rotation, int currentOrderIndex, int pickedUpOrderIndex)
    {
        Debug.Log("===");
        Debug.Log($"Distance {distance} | Rotation {rotation} | Order [current] {currentOrderIndex} - [picked up] {pickedUpOrderIndex}");
        Debug.Log("===");
    }
}
