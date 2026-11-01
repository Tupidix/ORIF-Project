using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>Routes a spawned object's XR events to the manager of its actual game session.</summary>
public sealed class CutleryInteractionBinding : MonoBehaviour
{
    private GameManager _manager;
    private XRGrabInteractable _interactable;

    public void Initialize(GameManager manager, XRGrabInteractable interactable)
    {
        Unsubscribe();
        _manager = manager;
        _interactable = interactable;
        DisableLegacyCallbacks(interactable.selectEntered, nameof(GameManager.HandleGrapEvent));
        DisableLegacyCallbacks(interactable.selectExited, nameof(GameManager.HandleReleaseEvent));
        interactable.selectEntered.AddListener(OnGrab);
        interactable.selectExited.AddListener(OnRelease);
    }

    private static void DisableLegacyCallbacks(UnityEventBase evt, string methodName)
    {
        for (int i = 0; i < evt.GetPersistentEventCount(); i++)
        {
            if (evt.GetPersistentTarget(i) is GameManager && evt.GetPersistentMethodName(i) == methodName)
                evt.SetPersistentListenerState(i, UnityEventCallState.Off);
        }
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        if (_manager != null)
            _manager.HandleGrapEvent(_interactable.gameObject.tag);
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        // Destruction/disable during cleanup is not a player placement.
        if (!args.isCanceled && _manager != null)
            _manager.HandleReleaseEvent(_interactable.gameObject.tag);
    }

    private void Unsubscribe()
    {
        if (_interactable == null) return;
        _interactable.selectEntered.RemoveListener(OnGrab);
        _interactable.selectExited.RemoveListener(OnRelease);
    }

    private void OnDestroy() => Unsubscribe();
}
