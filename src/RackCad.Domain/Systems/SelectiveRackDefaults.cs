namespace RackCad.Domain.Systems
{
    /// <summary>Domain-level constants for the selective rack (frontal view).</summary>
    public static class SelectiveRackDefaults
    {
        /// <summary>The only view built so far.</summary>
        public const string View = "FRONTAL";

        /// <summary>Connection point ON THE POST where a larguero hooks (its X slides with the post peralte).</summary>
        public const string PostBeamPoint = "TROQUEL_LARGUERO";

        /// <summary>Troquel pitch on the post (in): levels snap to this grid.</summary>
        public const double TroquelPaso = 2.0;

        /// <summary>Block parameter that stretches a piece to a length/height.</summary>
        public const string LengthParam = "LONGITUD";

        /// <summary>Block parameter for the section peralte.</summary>
        public const string PeralteParam = "PERALTE";

        /// <summary>Connection point on the base plate that mates to the post (lands on the post origin).</summary>
        public const string PlateMatePoint = "MONTAJE_POSTE";
    }
}
