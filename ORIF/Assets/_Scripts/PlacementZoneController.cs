using UnityEngine;

/// <summary>Chooses the exam layout once, according to the first released object's nearest zone.</summary>
public sealed class PlacementZoneController
{
    private bool _firstPlacementChecked;

    public void Reset() => _firstPlacementChecked = false;

    public void PrepareExamRelease(GameObject item)
    {
        if (_firstPlacementChecked) return;
        _firstPlacementChecked = true;

        GameObject[] areas = GameObject.FindGameObjectsWithTag("TablePlacementArea");
        if (areas.Length < 2)
        {
            Debug.LogWarning("Exam layout unchanged: fewer than two placement areas are available.");
            return;
        }

        BoxCollider areaA = areas[0].GetComponent<BoxCollider>();
        BoxCollider areaB = areas[1].GetComponent<BoxCollider>();
        if (areaA == null || areaB == null)
        {
            Debug.LogWarning("Exam layout unchanged: a placement area has no BoxCollider.");
            return;
        }

        float distanceA = Vector3.Distance(item.transform.position, areaA.bounds.center);
        float distanceB = Vector3.Distance(item.transform.position, areaB.bounds.center);
        // Preserve the existing tie rule: equal distances choose B.
        if (distanceA >= distanceB)
            SwapMenuLocations();
    }

    private static void SwapMenuLocations()
    {
        GameObject[] menus = GameObject.FindGameObjectsWithTag("Menu");
        if (menus.Length < 2)
        {
            Debug.LogWarning("Exam layout unchanged: fewer than two menus are available.");
            return;
        }

        Vector3 firstPosition = menus[0].transform.position;
        Quaternion firstRotation = menus[0].transform.rotation;
        for (int i = 0; i < menus.Length - 1; i++)
            menus[i].transform.SetPositionAndRotation(menus[i + 1].transform.position, menus[i + 1].transform.rotation);
        menus[menus.Length - 1].transform.SetPositionAndRotation(firstPosition, firstRotation);
    }
}
