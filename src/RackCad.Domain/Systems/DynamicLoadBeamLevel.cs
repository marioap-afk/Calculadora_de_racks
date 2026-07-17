namespace RackCad.Domain.Systems
{
    /// <summary>
    /// Resolved elevations of the complete entrance/exit beam pair for one pallet-flow load level. The exit is the
    /// low side; the entrance is raised by the lane slope. Longitudinal coordinates remain a system-level rule.
    /// </summary>
    public sealed class DynamicLoadBeamLevel
    {
        public DynamicLoadBeamLevel(int levelNumber, double exitElevation, double entranceElevation)
        {
            LevelNumber = levelNumber;
            ExitElevation = exitElevation;
            EntranceElevation = entranceElevation;
        }

        public int LevelNumber { get; }
        public double ExitElevation { get; }
        public double EntranceElevation { get; }
    }
}
