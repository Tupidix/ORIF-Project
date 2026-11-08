using UnityEngine;
using UnityEngine.UI;

/// <summary>Shows the authored dialog at a fixed offset from the trolly, preserving its dimensions.</summary>
public sealed class TrollyRecoveryNotice : MonoBehaviour
{
    private GameObject _displayRoot;
    private Graphic[] _graphics;
    private RectTransform _visibleAnchor;
    private Vector3 _offset;
    private float _hideAt;

    public void Initialize(GameObject dialog, Vector3 offset)
    {
        if (_displayRoot != null || dialog == null) return;
        // Use the scene object itself: retain its parent, scale and child layout.
        _displayRoot = dialog;
        _graphics = dialog.GetComponentsInChildren<Graphic>(true);
        _visibleAnchor = _graphics.Length > 0 ? _graphics[0].rectTransform : null;
        _offset = offset;
        _displayRoot.SetActive(false);
    }

    public void Show()
    {
        if (_displayRoot != null)
        {
            _displayRoot.SetActive(true);
            _hideAt = Time.time + 4f;
            UpdatePose();
        }
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayNotificationSound();
    }

    private void LateUpdate()
    {
        if (_displayRoot == null || !_displayRoot.activeSelf) return;
        if (Time.time >= _hideAt)
            _displayRoot.SetActive(false);
        else
            UpdatePose();
    }

    private void UpdatePose()
    {
        // Move the visible box, not the distant root pivot, to the trolly-relative position.
        Vector3 current = _visibleAnchor != null
            ? _visibleAnchor.TransformPoint(_visibleAnchor.rect.center)
            : _displayRoot.transform.position;
        Vector3 target = transform.position + transform.rotation * _offset;
        _displayRoot.transform.position += target - current;
        UpdateOrientation();
    }

    private void UpdateOrientation()
    {
        Camera camera = Camera.main;
        if (camera != null && _graphics != null)
        {
            // The authored root/canvas pivots need not coincide with the visible dialog.
            // Rotate the actual graphics in place, not the distant root or empty canvas.
            foreach (Graphic graphic in _graphics)
            {
                if (graphic == null) continue;
                Vector3 direction = graphic.transform.position - camera.transform.position;
                if (direction.sqrMagnitude > 0.000001f)
                    graphic.transform.rotation = Quaternion.LookRotation(direction);
            }
        }
    }

    private void OnDestroy()
    {
        if (_displayRoot != null)
            _displayRoot.SetActive(false);
    }
}
