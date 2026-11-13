using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Manages object snapping to tray slots using physics proximity detection.
/// Tracks nearby grabbable objects and keeps released objects in their assigned slots.
/// </summary>
public class TrayCollider : MonoBehaviour
{
    [SerializeField] private List<Transform> slots;

    private HashSet<int> _occupiedSlots;
    private List<GameObject> _trackedObjects;
    private Dictionary<GameObject, int> _objectSlotMapping;

    private const float ProximityThreshold = 0.3f;

    private static readonly HashSet<string> ExcludedTags = new HashSet<string>
    {
        "JobSheet", "MissionRecap", "Untagged", "Locked"
    };

    private void Awake()
    {
        _occupiedSlots = new HashSet<int>();
        _trackedObjects = new List<GameObject>();
        _objectSlotMapping = new Dictionary<GameObject, int>();
    }

    private void FixedUpdate()
    {
        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, ProximityThreshold);
        RemoveDistantObjects(nearbyColliders);
        TrackAndAssignNewObjects(nearbyColliders);
        MoveTrackedObjectsToSlots();
    }

    /// <summary>
    /// Removes tracked objects that are no longer within the proximity sphere.
    /// </summary>
    /// <param name="nearbyColliders">The colliders currently overlapping the tray.</param>
    private void RemoveDistantObjects(Collider[] nearbyColliders)
    {
        var objectsToRemove = new List<GameObject>();
        foreach (var obj in _trackedObjects)
        {
            if (obj == null || IsHeld(obj) || (!_objectSlotMapping.ContainsKey(obj) && !IsObjectInColliders(obj, nearbyColliders)))
            {
                ReleaseSlot(obj);
                objectsToRemove.Add(obj);
                if (obj != null)
                    Debug.Log($"Removed {obj.name} from tray tracking");
            }
        }
        foreach (var obj in objectsToRemove)
        {
            _trackedObjects.Remove(obj);
        }
    }

    /// <summary>
    /// Checks whether a specific GameObject is present among the given colliders.
    /// </summary>
    /// <param name="obj">The GameObject to search for.</param>
    /// <param name="colliders">The array of colliders to search within.</param>
    /// <returns>True if the object is found among the colliders.</returns>
    private bool IsObjectInColliders(GameObject obj, Collider[] colliders)
    {
        foreach (var collider in colliders)
        {
            if (collider.gameObject == obj)
                return true;
        }
        return false;
    }

    /// <summary>
    /// Detects new grabbable objects in the proximity and assigns them to available slots.
    /// </summary>
    /// <param name="nearbyColliders">The colliders currently overlapping the tray.</param>
    private void TrackAndAssignNewObjects(Collider[] nearbyColliders)
    {
        foreach (var collider in nearbyColliders)
        {
            if (!IsGrabbableObject(collider)) continue;
            var obj = collider.GetComponentInParent<XRGrabInteractable>().gameObject;
            TrackObjectIfNew(obj);
            AssignSlotIfNeeded(obj);
        }
    }

    /// <summary>
    /// Starts tracking an object if it is not already tracked.
    /// </summary>
    /// <param name="obj">The GameObject to track.</param>
    private void TrackObjectIfNew(GameObject obj)
    {
        if (_trackedObjects.Contains(obj)) return;
        _trackedObjects.Add(obj);
        Debug.Log($"Tracking new object: {obj.name}");
    }

    /// <summary>
    /// Assigns an available slot to the object if it does not already have one.
    /// </summary>
    /// <param name="obj">The GameObject to assign a slot to.</param>
    private void AssignSlotIfNeeded(GameObject obj)
    {
        if (_objectSlotMapping.ContainsKey(obj)) return;
        int slotIndex = FindAvailableSlot();
        if (slotIndex == -1) return;
        AssignSlotToObject(obj, slotIndex);
        Debug.Log($"Assigned slot {slotIndex} to {obj.name}");
    }

    /// <summary>
    /// Keeps tracked objects at their assigned slot poses.
    /// </summary>
    private void MoveTrackedObjectsToSlots()
    {
        foreach (var obj in _trackedObjects)
        {
            if (obj == null || IsHeld(obj) || !_objectSlotMapping.ContainsKey(obj)) continue;
            int slotIndex = _objectSlotMapping[obj];
            MoveObjectToSlot(obj, slots[slotIndex]);
        }
    }

    /// <summary>
    /// Places an object at its target slot without competing interpolation.
    /// </summary>
    /// <param name="obj">The GameObject to move.</param>
    /// <param name="slot">The target slot transform.</param>
    private void MoveObjectToSlot(GameObject obj, Transform slot)
    {
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        Quaternion targetRotation = ComputeTargetRotation(obj, slot);
        FreezeRigidbody(rb);
        rb.position = slot.position;
        rb.rotation = targetRotation;
        obj.transform.SetPositionAndRotation(slot.position, targetRotation);
    }

    /// <summary>
    /// Computes the target rotation for a slot, flipping glasses upside down.
    /// </summary>
    /// <param name="obj">The object being placed.</param>
    /// <param name="slot">The target slot transform.</param>
    /// <returns>The computed target rotation.</returns>
    private Quaternion ComputeTargetRotation(GameObject obj, Transform slot)
    {
        Quaternion rotation = slot.rotation;
        if (IsGlassTag(obj))
            rotation *= Quaternion.Euler(180f, 0f, 0f);
        return rotation;
    }

    /// <summary>
    /// Returns whether the object is tagged as a glass (wine or water).
    /// </summary>
    /// <param name="obj">The GameObject to check.</param>
    /// <returns>True if the object is a glass.</returns>
    private bool IsGlassTag(GameObject obj)
    {
        return obj.CompareTag("GlassWine") || obj.CompareTag("GlassWater");
    }

    /// <summary>
    /// Zeroes out the velocity and angular velocity of a Rigidbody.
    /// </summary>
    /// <param name="rb">The Rigidbody to freeze.</param>
    private void FreezeRigidbody(Rigidbody rb)
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    /// <summary>
    /// Returns whether a collider belongs to a valid grabbable object (enabled XRGrabInteractable, not excluded by tag).
    /// </summary>
    /// <param name="collider">The collider to validate.</param>
    /// <returns>True if the collider represents a grabbable object.</returns>
    private bool IsGrabbableObject(Collider collider)
    {
        XRGrabInteractable interactable = collider.GetComponentInParent<XRGrabInteractable>();
        if (interactable == null || !interactable.enabled || interactable.isSelected) return false;
        return interactable.GetComponent<Rigidbody>() != null && !ExcludedTags.Contains(interactable.gameObject.tag);
    }

    /// <summary>
    /// Snaps an object to a slot and marks the slot as occupied.
    /// </summary>
    /// <param name="obj">The object to assign.</param>
    /// <param name="slotIndex">The index of the slot to assign.</param>
    private void AssignSlotToObject(GameObject obj, int slotIndex)
    {
        Transform slot = slots[slotIndex];
        MoveObjectToSlot(obj, slot);
        _occupiedSlots.Add(slotIndex);
        _objectSlotMapping[obj] = slotIndex;
    }

    /// <summary>
    /// Releases the slot occupied by the given object.
    /// </summary>
    /// <param name="obj">The object whose slot should be freed.</param>
    private void ReleaseSlot(GameObject obj)
    {
        if (!_objectSlotMapping.TryGetValue(obj, out int slotIndex)) return;
        _occupiedSlots.Remove(slotIndex);
        _objectSlotMapping.Remove(obj);
    }

    /// <summary>
    /// Returns the index of the first available (unoccupied) slot, or -1 if none.
    /// </summary>
    /// <returns>The index of an available slot, or -1.</returns>
    private int FindAvailableSlot()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            if (!_occupiedSlots.Contains(i))
                return i;
        }
        return -1;
    }

    /// <summary>
    /// Releases all tracked objects, clears slots and resets internal state.
    /// </summary>
    public void ClearAllReferences()
    {
        foreach (var obj in _trackedObjects)
        {
            ReleaseSlot(obj);
        }
        _trackedObjects.Clear();
        _occupiedSlots.Clear();
        _objectSlotMapping.Clear();
    }

    public void ReleaseObject(GameObject obj)
    {
        if (_objectSlotMapping.ContainsKey(obj))
            ReleaseSlot(obj);
        _trackedObjects.Remove(obj);
    }

    private static bool IsHeld(GameObject obj)
    {
        var grab = obj.GetComponent<XRGrabInteractable>();
        return grab != null && grab.isSelected;
    }

    /// <summary>Moves carried items with the tray before the next proximity check can lose them.</summary>
    public void MoveCarriedObjectsWithTray()
    {
        for (int i = _trackedObjects.Count - 1; i >= 0; i--)
        {
            GameObject obj = _trackedObjects[i];
            if (obj == null || IsHeld(obj))
            {
                ReleaseObject(obj);
                continue;
            }
            if (!_objectSlotMapping.TryGetValue(obj, out int slotIndex)) continue;
            MoveObjectToSlot(obj, slots[slotIndex]);
        }
    }
}
