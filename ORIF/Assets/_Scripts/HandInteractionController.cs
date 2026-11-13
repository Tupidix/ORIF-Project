using UnityEngine;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Hands;

/// <summary>Coordinates configured hand interactors and visuals without replacing XR tracking.</summary>
public sealed class HandInteractionController
{
    private readonly GameObject[] _leftInteractors;
    private readonly GameObject[] _rightInteractors;
    private readonly GameObject _leftHandVisual;
    private readonly Transform _trackingSpace;
    private readonly List<XRHandSubsystem> _handSubsystems = new List<XRHandSubsystem>();
    private XRHandSubsystem _handSubsystem;

    public HandInteractionController(GameObject[] leftInteractors, GameObject[] rightInteractors,
        GameObject leftHandVisual)
    {
        _leftInteractors = leftInteractors;
        _rightInteractors = rightInteractors;
        _leftHandVisual = leftHandVisual;
        XROrigin origin = LeftDirectTransform.GetComponentInParent<XROrigin>();
        if (origin != null && origin.CameraFloorOffsetObject != null)
            _trackingSpace = origin.CameraFloorOffsetObject.transform;
    }

    // The existing Inspector configuration places the direct interactor last.
    public Transform LeftDirectTransform => _leftInteractors[_leftInteractors.Length - 1].transform;

    public void SetLeftInteractors(bool active, bool keepDirectEnabled = false)
        => SetInteractors(_leftInteractors, active, keepDirectEnabled);

    public void SetRightInteractors(bool active, bool keepDirectEnabled = false)
        => SetInteractors(_rightInteractors, active, keepDirectEnabled);

    public void EnableLeftDirectInteractor()
        => _leftInteractors[_leftInteractors.Length - 1].SetActive(true);

    public void SetLeftHandVisible(bool visible) => _leftHandVisual.SetActive(visible);

    public bool TryGetLeftPalmPose(out Pose worldPose)
    {
        worldPose = default;
        if (_handSubsystem == null || !_handSubsystem.running)
        {
            _handSubsystem = null;
            SubsystemManager.GetSubsystems(_handSubsystems);
            foreach (XRHandSubsystem subsystem in _handSubsystems)
            {
                if (!subsystem.running) continue;
                _handSubsystem = subsystem;
                break;
            }
        }
        if (_handSubsystem == null || !_handSubsystem.leftHand.isTracked || _trackingSpace == null)
            return false;

        // Follow the palm center directly, independently of disabled interactor transforms.
        if (!_handSubsystem.leftHand.GetJoint(XRHandJointID.Palm).TryGetPose(out Pose pose))
            return false;
        worldPose = new Pose(_trackingSpace.TransformPoint(pose.position),
            _trackingSpace.rotation * pose.rotation);
        return true;
    }

    private static void SetInteractors(GameObject[] interactors, bool active, bool keepDirectEnabled)
    {
        Helper.ToggleArrayOfGameObjects(interactors, active);
        if (keepDirectEnabled)
            interactors[interactors.Length - 1].SetActive(true);
    }
}
