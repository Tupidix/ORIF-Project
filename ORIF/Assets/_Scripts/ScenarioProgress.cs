/// <summary>
/// Tracks menu and step indexes for one session, without changing scene objects or UI.
/// </summary>
public sealed class ScenarioProgress
{
    public int MenuIndex { get; private set; }
    public int StepIndex { get; private set; }

    public void ResetMenus() => MenuIndex = 0;
    public void ResetSteps() => StepIndex = 0;
    public void AdvanceStep() => StepIndex++;

    // Step reset remains explicit: GameManager initializes the next menu after scene cleanup.
    public void AdvanceMenu() => MenuIndex++;
    public void SelectLastMenu(int menuCount) => MenuIndex = menuCount - 1;

    public bool IsCurrentMenuComplete(int totalSteps) => StepIndex == totalSteps;
    public bool IsLastMenu(int menuCount) => MenuIndex == menuCount - 1;
    public bool HasCurrentMenu(int menuCount) => MenuIndex < menuCount;

    public bool IsScenarioComplete(int totalSteps, int menuCount)
    {
        return IsCurrentMenuComplete(totalSteps) && IsLastMenu(menuCount);
    }
}
