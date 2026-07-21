namespace RackCad.Application.Catalogs
{
    /// <summary>The typed list a unified <c>secciones.csv</c> row is routed into by its "rol" column.</summary>
    public enum SeccionRole
    {
        /// <summary>Unrecognized or blank rol: the provider DROPS the row (a typo silently vanishes).</summary>
        Unknown = 0,
        Post,
        Truss,
        Beam,
        Spacer
    }

    /// <summary>
    /// Single source of truth for how a unified <c>secciones.csv</c> "rol" maps to a typed list. BOTH the
    /// provider (<see cref="JsonRackCatalogProvider"/>, which splits rows into posts/celosía/beams/spacers)
    /// and the catalog validator (which reports rows dropped by an unknown rol) classify through here, so the
    /// recognized set can never drift between "what loads" and "what is reported as discarded" (AGENTS.md §2).
    /// Behavior matches the historical inline switch exactly: trimmed, case-insensitive, accent tolerated on
    /// celosía. Adding a role means changing ONLY this method — the provider and the validator follow.
    /// </summary>
    public static class SeccionRoles
    {
        /// <summary>Classify a raw "rol" cell; <see cref="SeccionRole.Unknown"/> is the dropped case.</summary>
        public static SeccionRole Classify(string rol)
        {
            var normalized = (rol ?? string.Empty).Trim().ToUpperInvariant();

            switch (normalized)
            {
                case "POSTE":
                    return SeccionRole.Post;
                case "CELOSIA":
                case "CELOSÍA":
                    return SeccionRole.Truss;
                case "LARGUERO":
                    return SeccionRole.Beam;
                case "SEPARADOR":
                    return SeccionRole.Spacer;
                default:
                    return SeccionRole.Unknown;
            }
        }

        /// <summary>True when a row with this rol is loaded; false when the provider would drop it.</summary>
        public static bool IsRecognized(string rol) => Classify(rol) != SeccionRole.Unknown;
    }
}
