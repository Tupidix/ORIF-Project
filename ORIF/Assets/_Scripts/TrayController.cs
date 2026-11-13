using UnityEngine;

/// <summary>Controls tray visibility and hand following; TrayCollider manages carried objects.</summary>
public sealed class TrayController
{
    private readonly GameObject _tray;
    private readonly HandInteractionController _hands;
    private readonly Vector3 _localCenter;
    private readonly TrayCollider _carriedObjects;
    private bool _isOpen;

    public TrayController(GameObject tray, HandInteractionController hands)
    {
        _tray = tray;
        _hands = hands;
        _carriedObjects = tray.GetComponent<TrayCollider>();
        BoxCollider surface = tray.GetComponent<BoxCollider>();
        _localCenter = surface != null ? surface.center : Vector3.zero;
    }

    public void Open()
    {
        _hands.EnableLeftDirectInteractor();
        _isOpen = true;
        UpdatePose();
    }

    public void Close()
    {
        _isOpen = false;
        _hands.SetLeftHandVisible(true);
        _tray.SetActive(false);
    }

    public void HandlePalmGestureEnded()
    {
        // A missing hand can end gesture recognition without an intentional closing gesture.
        if (_isOpen && _hands.TryGetLeftPalmPose(out _))
            Close();
    }

    public void UpdatePose()
    {
        if (!_isOpen) return;
        if (!_hands.TryGetLeftPalmPose(out Pose hand))
        {
            // Leave both pose and visibility unchanged so colliders and carried items stay supported.
            return;
        }
        Vector3 previousPosition = _tray.transform.position;
        Quaternion previousRotation = _tray.transform.rotation;
        _tray.transform.rotation = hand.rotation * Quaternion.Euler(90, 0, 0);
        // Align the tray surface center to the palm, accounting for its actual scaled pivot.
        _tray.transform.position = hand.position - _tray.transform.TransformVector(_localCenter);
        _carriedObjects.MoveCarriedObjectsWithTray();
        _hands.SetLeftHandVisible(false);
        _tray.SetActive(true);
    }

    public void ClearCarriedObjects()
    {
        _carriedObjects.ClearAllReferences();
    }
}
