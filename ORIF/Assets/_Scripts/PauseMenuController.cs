using UnityEngine;

/// <summary>Controls pause presentation, hand interactions and timer suspension.</summary>
public sealed class PauseMenuController
{
    private readonly GameObject _panel;
    private readonly GameObject _validateButton;
    private readonly HandInteractionController _hands;
    public bool IsOpen => _panel.activeSelf;

    public PauseMenuController(GameObject panel, GameObject validateButton, HandInteractionController hands)
    {
        _panel = panel;
        _validateButton = validateButton;
        _hands = hands;
    }

    public void Hide() => _panel.SetActive(false);

    public void UpdateValidateButton(Scenario.Mode mode, int reportCount)
        => _validateButton.SetActive(mode == Scenario.Mode.Exam && reportCount == 0);

    public void Open(CountdownTimer timer)
    {
        _panel.SetActive(true);
        _hands.SetLeftInteractors(true, true);
        _hands.SetRightInteractors(true, true);
        if (timer == null) return;
        if (!timer.GetIsCountingDownOver() && timer.GetIsCountingDown())
            timer.StopCountdown();
        if (timer.GetIsTimerEnabled())
            timer.StopTimer();
    }

    public void Close(CountdownTimer timer, bool resumeAllowed)
    {
        Hide();
        _hands.SetLeftInteractors(false, true);
        _hands.SetRightInteractors(false, true);
        if (timer == null || !resumeAllowed) return;
        if (!timer.GetIsCountingDownOver() && !timer.GetIsCountingDown())
            timer.StartCountdown();
        if (timer.GetIsCountingDownOver() && !timer.GetIsTimerEnabled())
            timer.StartTimer();
    }
}
