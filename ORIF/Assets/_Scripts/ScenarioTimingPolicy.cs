/// <summary>Defines timing behavior independently of timer UI and scene objects.</summary>
public static class ScenarioTimingPolicy
{
    public enum ExpiryAction { None, ContinueWithOvertime, EndScenario }

    public static bool UsesCountdown(Scenario scenario)
    {
        return scenario.mode == Scenario.Mode.Exam ||
            (scenario.mode == Scenario.Mode.Training && scenario.level == Scenario.Level.Expert);
    }

    public static ExpiryAction GetExpiryAction(Scenario scenario)
    {
        if (scenario.mode == Scenario.Mode.Exam)
            return ExpiryAction.EndScenario;
        return UsesCountdown(scenario) ? ExpiryAction.ContinueWithOvertime : ExpiryAction.None;
    }
}
