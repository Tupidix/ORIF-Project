using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>Applies XR interaction and lock visuals without driving scenario progression.</summary>
public static class ScenarioInteractionController
{
    public static void SetGrabEnabledForTag(string tag, bool enabled)
    {
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag(tag))
        {
            XRGrabInteractable interactable = obj.GetComponent<XRGrabInteractable>();
            if (interactable != null)
                interactable.enabled = enabled;
        }
    }

    public static void SetCatalogGrabEnabled(IReadOnlyList<PrefabCutleryManager.PrefabEntry> entries, bool enabled)
    {
        // Preserve the existing exclusion of the final two catalog entries.
        // This operates on all matching scene objects, as the original implementation did.
        for (int i = 0; i < entries.Count - 2; i++)
            SetGrabEnabledForTag(entries[i].tag, enabled);
    }

    public static void SetLockedGrabEnabled(IEnumerable<string> lockedTags, bool enabled)
    {
        foreach (string tag in lockedTags)
            SetGrabEnabledForTag(tag, enabled);
    }

    public static void ApplyPlacementLock(GameObject[] objects, int menuIndex,
        Material defaultCutleryMaterial, Material defaultGlassMaterial)
    {
        Material material = defaultCutleryMaterial;
        int guideIndex = menuIndex % 2 != 0 ? 1 : 0;
        if (objects[0].tag.Contains("Glass")) material = defaultGlassMaterial;
        // Preserve the current plate material assignment during extraction.
        if (objects[0].tag.Contains("Plate")) material = defaultGlassMaterial;
        Helper.RemoveCurrentItemMaterial(objects[guideIndex].GetComponent<MeshRenderer>(), material);
        objects[0].GetComponent<Rigidbody>().mass = 9999;
    }
}
