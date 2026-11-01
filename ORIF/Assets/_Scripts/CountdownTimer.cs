using System;
using TMPro;
using UnityEngine;

public class CountdownTimer : MonoBehaviour
{
    [SerializeField] private float countdownMinutes = 1.0f;
    [SerializeField] private float countdownSeconds = 0;
    [SerializeField] private float elapsedTime = 0.0f;

    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private TextMeshProUGUI overtimeText;

    public float RemainingSeconds => countdownSeconds;
    public float ElapsedTime => elapsedTime;

    public void SetDurationMinutes(float minutes) => countdownMinutes = minutes;

    private bool _isCountingDown = false;
    private bool _isTimerEnabled = false;

    private const float TenSecondsThreshold = 10f;

    /// <summary>
    /// Starts the countdown.
    /// </summary>
    public void StartCountdown()
    {
        _isCountingDown = true;
    }

    /// <summary>
    /// Stops the countdown.
    /// </summary>
    public void StopCountdown()
    {
        _isCountingDown = false;
    }

    /// <summary>
    /// Starts the overtime stopwatch.
    /// </summary>
    public void StartTimer()
    {
        _isTimerEnabled = true;
    }

    /// <summary>
    /// Stops the overtime stopwatch.
    /// </summary>
    public void StopTimer()
    {
        _isTimerEnabled = false;
    }

    void Start()
    {
        SetCountdown();
    }

    /// <summary>
    /// Initializes the countdown seconds from the configured minutes value.
    /// </summary>
    public void SetCountdown()
    {
        countdownSeconds = countdownMinutes * 60.0f;
    }

    void Update()
    {
        if (_isCountingDown)
            TickCountdown();
        else if (_isTimerEnabled)
            TickTimer();

        UpdateUI();
    }

    /// <summary>
    /// Decrements the countdown by delta time. Stops when reaching zero.
    /// </summary>
    private void TickCountdown()
    {
        countdownSeconds = Mathf.Max(0, countdownSeconds - Time.deltaTime);
        if (countdownSeconds <= 0)
            StopCountdown();
    }

    /// <summary>
    /// Increments the elapsed overtime by delta time.
    /// </summary>
    private void TickTimer()
    {
        elapsedTime += Time.deltaTime;
    }

    /// <summary>
    /// Updates both countdown and overtime UI text elements.
    /// </summary>
    private void UpdateUI()
    {
        UpdateCountdownText();
        UpdateOvertimeText();
    }

    /// <summary>
    /// Updates the countdown text element visibility and content.
    /// </summary>
    private void UpdateCountdownText()
    {
        if (countdownText == null) return;
        bool isActive = _isCountingDown || _isTimerEnabled;
        countdownText.enabled = isActive;
        if (isActive)
            countdownText.text = Helper.FormatTime(countdownSeconds);
    }

    /// <summary>
    /// Updates the overtime text element visibility and content.
    /// </summary>
    private void UpdateOvertimeText()
    {
        if (overtimeText == null) return;
        bool shouldShow = !_isCountingDown && _isTimerEnabled;
        overtimeText.enabled = shouldShow;
        if (shouldShow)
            overtimeText.text = Helper.FormatTime(elapsedTime);
    }

    /// <summary>
    /// Returns whether the countdown is currently active.
    /// </summary>
    /// <returns>True if counting down.</returns>
    public bool GetIsCountingDown()
    {
        return _isCountingDown;
    }

    /// <summary>
    /// Returns whether the overtime stopwatch is currently active.
    /// </summary>
    /// <returns>True if the timer is running.</returns>
    public bool GetIsTimerEnabled()
    {
        return _isTimerEnabled;
    }

    /// <summary>
    /// Returns whether the countdown has reached zero.
    /// </summary>
    /// <returns>True if countdown seconds equal zero.</returns>
    public bool GetIsCountingDownOver()
    {
        return countdownSeconds == 0;
    }

    /// <summary>
    /// Returns whether exactly 10 seconds remain on the countdown.
    /// </summary>
    /// <returns>True if the rounded countdown equals 10.</returns>
    public bool IsRemainingTenSeconds()
    {
        return Math.Round(countdownSeconds) == TenSecondsThreshold;
    }
}
