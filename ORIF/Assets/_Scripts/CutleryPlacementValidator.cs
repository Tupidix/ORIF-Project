/// <summary>
/// Placement rules independent of scene objects, UI and game state changes.
/// </summary>
public static class CutleryPlacementValidator
{
    public const float DistanceThreshold = 0.15f;
    public const float RotationThreshold = 9.5f;

    public readonly struct Result
    {
        public bool PositionCorrect { get; }
        public bool RotationCorrect { get; }
        public bool OrderCorrect { get; }
        public bool IsCorrect => PositionCorrect && RotationCorrect && OrderCorrect;

        public Result(bool positionCorrect, bool rotationCorrect, bool orderCorrect)
        {
            PositionCorrect = positionCorrect;
            RotationCorrect = rotationCorrect;
            OrderCorrect = orderCorrect;
        }
    }

    public static Result Validate(Scenario scenario, string tag, float distance,
        float rotation, int placementOrder, int currentStep)
    {
        bool positionCorrect = !scenario.validator.position || distance <= DistanceThreshold;
        bool roundItem = tag == "Plate" || tag == "GlassWine" || tag == "GlassWater";
        bool rotationCorrect = roundItem || !scenario.validator.rotation || rotation <= RotationThreshold;
        bool orderRequired = scenario.validator.order || scenario.mode == Scenario.Mode.Learning ||
            (scenario.mode == Scenario.Mode.Training && scenario.level == Scenario.Level.Begginner);
        bool orderCorrect = !orderRequired || placementOrder == currentStep;

        return new Result(positionCorrect, rotationCorrect, orderCorrect);
    }
}
