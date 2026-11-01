using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Creates scenario furniture and placement guides. References remain owned by GameManager.
/// </summary>
public static class ScenarioEnvironmentFactory
{
    public static GameObject CreateTable(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        return Object.Instantiate(prefab, position, rotation);
    }

    public static GameObject CreateTrolly(GameObject prefab, Transform table)
    {
        // The existing layout applies a world offset before rotating the whole environment.
        return Object.Instantiate(prefab, table.position + new Vector3(-1.2f, 0, -0.6f), Quaternion.identity);
    }

    public static void PopulateTrolly(GameObject trolly, GameObject[] groups,
        Dictionary<string, GameObject> instances)
    {
        Transform parent = trolly.transform.Find("SM_Trolly");
        foreach (GameObject group in groups)
        {
            GameObject instance = Object.Instantiate(group);
            instance.transform.SetParent(parent, false);
            instance.transform.localPosition = group.transform.localPosition;
            instances.Add(instance.name, instance);
        }
    }

    public static Pose CalculateGuidePose(Vector3 parentPosition, Quaternion parentRotation,
        Vector3 offsetPosition, Quaternion offsetRotation)
    {
        // Preserve the additive world-space offset used by the existing scenario format.
        return new Pose(parentPosition + offsetPosition, parentRotation * offsetRotation);
    }

    public static bool PopulatePlacementGuides(Menu menu, Scenario scenario,
        PrefabCutleryManager prefabs, Dictionary<string, GameObject> instances,
        GameObject menuObject, int menuIndex, Material materialA, Material materialB)
    {
        foreach (var entry in menu.cutleryToPlace)
        {
            Cutlery cutlery = entry.Value;
            if (!instances.TryGetValue(cutlery.parent, out GameObject parent))
            {
                Debug.Log("Parent doesn't exist.");
                return false;
            }

            Pose pose = CalculateGuidePose(parent.transform.position, parent.transform.rotation,
                cutlery.offset.position, cutlery.offset.rotation);
            string tag = Helper.GetTagBasedOnScenarioLevel(scenario, cutlery.tag);
            GameObject guide = prefabs.InstantiatePrefabByTag(tag, pose.position, pose.rotation);
            guide.tag = cutlery.tag;
            guide.GetComponent<BoxCollider>().enabled = false;
            guide.GetComponent<Rigidbody>().useGravity = false;
            if (tag != "None")
                guide.GetComponent<MeshRenderer>().material = menuIndex % 2 == 0 ? materialA : materialB;
            guide.transform.SetParent(menuObject.transform);
            instances.Add(cutlery.tag, guide);
        }
        return true;
    }

    public static GameObject ArrangeMenuAndCreateChair(GameObject chairPrefab, Transform table,
        Transform menu, float tableLength, int menuIndex)
    {
        Vector3 chairPosition;
        Quaternion chairRotation;
        if (menuIndex % 2 == 0)
        {
            menu.position = new Vector3(0, 0, tableLength / 3);
            chairPosition = new Vector3(table.position.x, 0.451f, table.position.z + 0.515f);
            chairRotation = Quaternion.Euler(-90, 180, 0);
        }
        else
        {
            menu.position = new Vector3(0, 0, -tableLength / 3);
            menu.RotateAround(table.position, Vector3.up, 180);
            menu.position = new Vector3(menu.position.x, menu.position.y, menu.position.z - 0.615f);
            chairPosition = new Vector3(table.position.x, 0.451f, table.position.z - 0.515f);
            chairRotation = Quaternion.Euler(-90, 0, 0);
        }

        GameObject chair = Object.Instantiate(chairPrefab, chairPosition, chairRotation);
        chair.tag = "Chair";
        return chair;
    }
}
