using UnityEngine;

/// <summary>Shows the table pose that the user can confirm, without AR zone validation.</summary>
public sealed class TablePlacementPreview
{
    private readonly Transform _preview;

    public TablePlacementPreview(GameObject preview)
    {
        _preview = preview.transform;
        Color color = new Color(0, 0.8f, 0.6f, 0.58f);
        _preview.Find("SM_Table").GetComponent<MeshRenderer>().material.color = color;
        _preview.Find("SM_Trolly").GetComponent<MeshRenderer>().material.color = color;
    }

    public void Update(Transform camera, out Vector3 position, out Quaternion rotation)
    {
        Vector3 target = camera.position + camera.forward * 0.75f;
        position = new Vector3(target.x, 0, target.z);
        rotation = Quaternion.LookRotation(new Vector3(camera.forward.x, 0, camera.forward.z), Vector3.up);
        _preview.SetPositionAndRotation(position, rotation);
    }
}