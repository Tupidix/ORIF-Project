using UnityEngine;

/// <summary>Controls tray visibility and hand following; TrayCollider manages carried objects.</summary>
public sealed class TrayController
{
    private readonly GameObject _tray;
    private readonly GameObject _handVisual;
    private readonly GameObject _directInteractor;
    private bool _isOpen;

    public TrayController(GameObject tray, GameObject handVisual, GameObject directInteractor)
    {
        _tray = tray;
        _handVisual = handVisual;
        _directInteractor = directInteractor;
    }

    public void Open()
    {
        _directInteractor.SetActive(true);
        _isOpen = true;
        _handVisual.SetActive(false);
        _tray.SetActive(true);
    }

    public void Close()
    {
        _isOpen = false;
        _handVisual.SetActive(true);
        _tray.SetActive(false);
    }

    public void UpdatePose()
    {
        if (!_isOpen) return;
        Transform hand = _directInteractor.transform;
        Vector3 offset = hand.TransformVector(new Vector3(-0.175f, 0, 0));
        _tray.transform.rotation = hand.rotation * Quaternion.Euler(90, 0, 0);
        _tray.transform.position = hand.position + offset;
    }

    public void ClearCarriedObjects()
    {
        _tray.GetComponent<TrayCollider>().ClearAllReferences();
    }
}
