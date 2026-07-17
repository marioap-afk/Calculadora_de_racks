namespace RackCad.Domain.Systems
{
    /// <summary>Domain-level constants for roller beds ("camas de rodamiento"), flow and pushback.</summary>
    public static class FlowBedDefaults
    {
        // ---- Component catalog ids (match flow-bed-profiles.csv) ----
        public const string RailId = "RIEL_DE_CINTA_CALIBRE_12";
        public const string StopId = "TOPE_DE_CAMA_DE_RODILLOS_DE_PLACA_CALIBRE_3_16";
        public const string BrakeId = "FRENO_TIPO_RODILLO_DE_TUBO_DE_3_1_8";

        /// <summary>Default roller (the smaller 1.9"); the bed can pick the 2.5" one instead.</summary>
        public const string RollerId = "RODILLO_DE_TUBO_DE_1.9_CALIBRE_14";

        /// <summary>Role tag in flow-bed-profiles.csv that marks an entry as a roller (for pickers).</summary>
        public const string RollerRole = "RODILLO";

        // ---- View + connection point used by the lateral drawing ----
        public const string View = "LATERAL";

        /// <summary>Connection point ON THE RAIL of the first troquel (where the tope's origin lands).</summary>
        public const string RailTopePoint = "TROQUEL_TOPE";

        /// <summary>Connection point on the rail that bolts the complete bed to the dynamic IN/OUT beam.</summary>
        public const string RailInOutMatePoint = "TROQUEL_IN";

        // ---- Assembly geometry (troquel grid + clearances) ----
        /// <summary>Troquel pitch on the rail (in): elements can be placed every 1".</summary>
        public const double TroquelPitch = 1.0;

        /// <summary>Length the tope occupies from TROQUEL_TOPE (in); the first roller starts at the next troquel.</summary>
        public const double TopeOccupiedLength = 6.5;

        /// <summary>A brake sits this many troqueles after the last roller (its 4.21" bracket needs the room).</summary>
        public const double BrakeAfterLastRoller = 5.0;

        /// <summary>The roller after a brake sits this many troqueles from the brake origin.</summary>
        public const double RollerAfterBrake = 2.0;

        /// <summary>A brake must clear the pallet by at least this much (brake spacing >= pallet depth + 1").</summary>
        public const double BrakeClearanceOverPallet = 1.0;
    }
}
