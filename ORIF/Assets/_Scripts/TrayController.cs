using UnityEngine;

/// <summary>Controls tray visibility and hand following; TrayCollider manages carried objects.</summary>
public sealed class TrayController
{
    private readonly GameObject _tray;
    private readonly HandInteractionController _hands;
    private readonly Vector3 _localCenter;
    private bool _isOpen;

    public TrayController(GameObject tray, HandInteractionController hands)
    {
        _tray = tray;
        _hands = hands;
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

    public void UpdatePose()
    {
        if (!_isOpen) return;
        if (!_hands.TryGetLeftPalmPose(out Pose hand))
        {
            _tray.SetActive(false);
            _hands.SetLeftHandVisible(true);
            return;
        }
        _tray.transform.rotation = hand.rotation * Quaternion.Euler(90, 0, 0);
        // Align the tray surface center to the palm, accounting for its actual scaled pivot.
        _tray.transform.position = hand.position - _tray.transform.TransformVector(_localCenter);
        _hands.SetLeftHandVisible(false);
        _tray.SetActive(true);
    }

    public void ClearCarriedObjects()
    {
        _tray.GetComponent<TrayCollider>().ClearAllReferences();
    }
}
