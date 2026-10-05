using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Manages the display of timed notification popups in the scene.
/// </summary>
public class Notification : MonoBehaviour
{
    [SerializeField]
    private GameObject _notification;

    [SerializeField]
    private TextMeshProUGUI _description;

    public SoundManager soundManager;

    private void Start()
    {
        HideNotification();
    }

    /// <summary>
    /// Displays a notification with a sound, then hides it after the given duration.
    /// </summary>
    /// <param name="desc">The text to display in the notification.</param>
    /// <param name="duration">The display duration in seconds.</param>
    public void DisplayNotification(string desc, int duration)
    {
        SoundManager.Instance.PlayNotificationSound();
        StartCoroutine(ShowNotificationForDuration(desc, duration));
    }

    /// <summary>
    /// Returns the underlying notification GameObject for external positioning.
    /// </summary>
    /// <returns>The notification panel GameObject.</returns>
    public GameObject GetNotificationObject()
    {
        return _notification;
    }

    /// <summary>
    /// Coroutine that shows the notification, waits, then hides it.
    /// </summary>
    /// <param name="desc">The notification text.</param>
    /// <param name="duration">The wait time in seconds before hiding.</param>
    private IEnumerator ShowNotificationForDuration(string desc, int duration)
    {
        ShowNotification(desc);
        yield return new WaitForSeconds(duration);
        HideNotification();
    }

    /// <summary>
    /// Sets the description text and activates the notification panel.
    /// </summary>
    /// <param name="desc">The text to display.</param>
    private void ShowNotification(string desc)
    {
        _description.text = desc;
        _notification.SetActive(true);
    }

    /// <summary>
    /// Deactivates the notification panel.
    /// </summary>
    private void HideNotification()
    {
        _notification.SetActive(false);
    }
}