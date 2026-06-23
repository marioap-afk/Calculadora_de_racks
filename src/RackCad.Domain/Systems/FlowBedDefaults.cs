namespace RackCad.Domain.Systems
{
    /// <summary>Domain-level constants for roller beds ("camas de rodamiento"), flow and pushback.</summary>
    public static class FlowBedDefaults
    {
        // ---- Component catalog ids (match flow-bed-profiles.csv) ----
        public const string RailId = "RIEL_CAMA_RODAMIENTO_CAL_12";
        public const string RollerId = "RODILLO_CAMA_GALV_1_9";
        public const string BrakeId = "FRENO_CAMA_VELOCIDAD";
        public const string StopId = "TOPE_CAMA_RODAMIENTO";

        // ---- View + connection points used by the lateral drawing ----
        public const string View = "LATERAL";
        public const string RailMatePoint = "MONTAJE_RIEL";
        public const string RollerPoint = "POSICION_RODILLO";
        public const string BrakePoint = "POSICION_FRENO";
        public const string StopPoint = "FIN_RIEL";

        // ---- Initial (non-calculated) assembly defaults ----
        /// <summary>Spacing between rollers along the rail (in). Future: derived from roller capacity.</summary>
        public const double RollerPitch = 4.0;

        /// <summary>A brake replaces every Nth roller (flow beds only). Future: N-1 rule by depth.</summary>
        public const int BrakeEveryNRollers = 5;
    }
}
