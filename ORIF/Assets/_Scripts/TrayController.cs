using UnityEngine;

/// <summary>Controls tray visibility and hand following; TrayCollider manages carried objects.</summary>
public sealed class TrayController
{
    private readonly GameObject _tray;
    private readonly HandInteractionController _hands;
    private bool _isOpen;

    public TrayController(GameObject tray, HandInteractionController hands)
    {
        _tray = tray;
        _hands = hands;
    }

    public void Open()
    {
        _hands.EnableLeftDirectInteractor();
        _isOpen = true;
        _hands.SetLeftHandVisible(false);
        _tray.SetActive(true);
    }

    public void Close()
    {
        _isOpen = false;
        _hands.SetLeftHandVisible(true);
        _tray.SetActive(false);
    }

    public void UpdatePose()
    {
        if (!_isOpen) return;
        Transform hand = _hands.LeftDirectTransform;
        Vector3 offset = hand.TransformVector(new Vector3(-0.175f, 0, 0));
        _tray.transform.rotation = hand.rotation * Quaternion.Euler(90, 0, 0);
        _tray.transform.position = hand.position + offset;
    }

    public void ClearCarriedObjects()
    {
        _tray.GetComponent<TrayCollider>().ClearAllReferences();
    }
}
