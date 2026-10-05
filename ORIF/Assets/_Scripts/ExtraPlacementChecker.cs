using UnityEngine;

/// <summary>
/// Tracks whether an extra object is correctly placed on the table
/// and within the designated placement area using collision and trigger events.
/// </summary>
public class ExtraPlacementChecker : MonoBehaviour
{
    private const string TablePlacementAreaTag = "TablePlacementArea";
    private const string TableTag = "Table";

    private bool _isOnTable = false;
    private bool _isOnPlacementArea = false;

    /// <summary>
    /// Called when this object enters a trigger collider.
    /// Sets placement area flag if the other collider is the table placement area.
    /// </summary>
    /// <param name="other">The trigger collider that was entered.</param>
    private void OnTriggerEnter(Collider other)
    {
        LogCollision("Enter (Trigger)", other.name, other.tag);
        if (other.CompareTag(TablePlacementAreaTag))
            _isOnPlacementArea = true;
    }

    /// <summary>
    /// Called when this object exits a trigger collider.
    /// Clears placement area flag if the other collider is the table placement area.
    /// </summary>
    /// <param name="other">The trigger collider that was exited.</param>
    private void OnTriggerExit(Collider other)
    {
        LogCollision("Exit (Trigger)", other.name, other.tag);
        if (other.CompareTag(TablePlacementAreaTag))
            _isOnPlacementArea = false;
    }

    /// <summary>
    /// Called when this object starts colliding with another collider.
    /// Sets table flag if the other object is the table.
    /// </summary>
    /// <param name="collision">The collision data.</param>
    private void OnCollisionEnter(Collision collision)
    {
        LogCollision("Enter (Collision)", collision.gameObject.name, collision.gameObject.tag);
        if (collision.gameObject.CompareTag(TableTag))
            _isOnTable = true;
    }

    /// <summary>
    /// Called when this object stops colliding with another collider.
    /// Clears table flag if the other object is the table.
    /// </summary>
    /// <param name="collision">The collision data.</param>
    private void OnCollisionExit(Collision collision)
    {
        LogCollision("Exit (Collision)", collision.gameObject.name, collision.gameObject.tag);
        if (collision.gameObject.CompareTag(TableTag))
            _isOnTable = false;
    }

    /// <summary>
    /// Returns whether the extra is both on the table and within the placement area.
    /// </summary>
    /// <returns>True if the object is correctly placed.</returns>
    public bool IsCorrectlyPlaced()
    {
        bool result = _isOnTable && _isOnPlacementArea;
        print($"isOnTable {_isOnTable} && isOnPlacementArea {_isOnPlacementArea} => {result}");
        return result;
    }

    /// <summary>
    /// Logs a collision or trigger event to the console.
    /// </summary>
    /// <param name="eventType">A label describing the event type (e.g. "Enter (Trigger)").</param>
    /// <param name="objectName">The name of the other object involved.</param>
    /// <param name="objectTag">The tag of the other object involved.</param>
    private void LogCollision(string eventType, string objectName, string objectTag)
    {
        print($"{eventType} : Other {objectName} - {objectTag}");
    }
}
