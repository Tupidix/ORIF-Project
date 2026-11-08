using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>Returns an unheld, unvalidated tableware item to its original trolly slot.</summary>
public sealed class FallenItemRecovery : MonoBehaviour
{
    private Transform _home;
    private Vector3 _localPosition;
    private Quaternion _localRotation;
    private Rigidbody _body;
    private XRGrabInteractable _grab;
    private Collider[] _colliders;
    private TrollyRecoveryNotice _notice;
    private Func<bool> _canRecover;
    private Action<GameObject> _releaseFromTray;
    private float _floorHeight;
    private bool _initialized;

    public void Initialize(XRGrabInteractable grab, TrollyRecoveryNotice notice,
        float floorHeight, Func<bool> canRecover, Action<GameObject> releaseFromTray)
    {
        _grab = grab;
        _body = grab.GetComponent<Rigidbody>();
        _colliders = grab.GetComponentsInChildren<Collider>();
        _home = transform.parent;
        _localPosition = transform.localPosition;
        _localRotation = transform.localRotation;
        _notice = notice;
        _floorHeight = floorHeight;
        _canRecover = canRecover;
        _releaseFromTray = releaseFromTray;
        _initialized = true;
    }

    private void FixedUpdate()
    {
        // Cloned locked objects have no initialized runtime binding.
        if (!_initialized || _home == null || _notice == null || _body == null ||
            _grab.isSelected || !_canRecover()) return;

        bool atFloor = transform.position.y < _floorHeight;
        foreach (Collider collider in _colliders)
        {
            if (collider != null && collider.enabled && !collider.isTrigger &&
                collider.bounds.min.y <= _floorHeight + 0.015f)
                atFloor = true;
        }
        if (!atFloor) return;

        _releaseFromTray?.Invoke(gameObject);
        transform.SetParent(_home, false);
        Vector3 position = _home.TransformPoint(_localPosition);
        Quaternion rotation = _home.rotation * _localRotation;
        _body.velocity = Vector3.zero;
        _body.angularVelocity = Vector3.zero;
        transform.SetPositionAndRotation(position, rotation);
        _body.position = position;
        _body.rotation = rotation;
        _body.WakeUp();
        _notice.Show();
    }
}
