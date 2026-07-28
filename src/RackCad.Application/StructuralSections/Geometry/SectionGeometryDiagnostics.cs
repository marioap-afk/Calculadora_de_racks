namespace RackCad.Application.StructuralSections.Geometry
{
    /// <summary>
    /// The stable diagnostic codes the section builders emit.
    ///
    /// They are constants rather than free strings because the tests, and one day a UI filter, key on them.
    /// The message next to each one is Spanish and may be reworded; the code may not.
    /// </summary>
    public static class SectionGeometryDiagnostics
    {
        /// <summary>A dimension the family requires is missing, so nothing can be built at all.</summary>
        public const string MissingRequiredDimension = "SG_MISSING_REQUIRED_DIMENSION";

        /// <summary>The root fillet could not be derived, so the corners stayed sharp.</summary>
        public const string FilletNotDerivable = "SG_FILLET_NOT_DERIVABLE";

        /// <summary>The fillet would not fit inside the shape, so the corners stayed sharp.</summary>
        public const string FilletDoesNotFit = "SG_FILLET_DOES_NOT_FIT";

        /// <summary>The HSS corner radius could not be derived from the flat walls.</summary>
        public const string CornerRadiusNotDerivable = "SG_CORNER_RADIUS_NOT_DERIVABLE";

        /// <summary>The two flat-wall derivations of the HSS corner radius disagree beyond the rounding tolerance.</summary>
        public const string CornerRadiusInconsistent = "SG_CORNER_RADIUS_INCONSISTENT";

        /// <summary>The derived radius is not physically possible for these outer dimensions.</summary>
        public const string CornerRadiusNotPhysical = "SG_CORNER_RADIUS_NOT_PHYSICAL";

        /// <summary>The inner corner would have a non-positive radius, so the void kept sharp corners.</summary>
        public const string InnerCornerSquared = "SG_INNER_CORNER_SQUARED";

        /// <summary>The source does not publish the toe rounding, so the tabulated contour omits it.</summary>
        public const string ToeRoundingNotPublished = "SG_TOE_ROUNDING_NOT_PUBLISHED";

        /// <summary>The channel's real flanges are tapered; this contour uses the tabulated MEAN thickness.</summary>
        public const string ChannelFlangeTaperNotModelled = "SG_CHANNEL_FLANGE_TAPER_NOT_MODELLED";

        /// <summary>The HSS contour uses the NOMINAL wall, so its area cannot match the tabulated one.</summary>
        public const string HssContourUsesNominalThickness = "SG_HSS_CONTOUR_USES_NOMINAL_THICKNESS";

        /// <summary>The section was centred with the value the source tabulates, not with a computed centroid.</summary>
        public const string CentredWithTabulatedCentroid = "SG_CENTRED_WITH_TABULATED_CENTROID";
    }
}
