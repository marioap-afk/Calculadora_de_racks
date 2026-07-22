using System;
using RackCad.Application.Catalogs;
using RackCad.Application.Diagnostics;

namespace RackCad.Plugin
{
    /// <summary>
    /// The one place the Plugin loads the product catalog: the base-directory provider, swallowing a load
    /// failure into an empty <see cref="RackCatalog"/> so commands and draw services keep working when the
    /// catalog is missing or invalid. Extracted from <c>LateralHeaderDrawService.LoadCatalog</c> (I-16 F2),
    /// which now forwards here. Kept INTERNAL to RackCad.Plugin — deliberately NOT unified with the UI's own
    /// <c>UiSupport.LoadCatalogSafe</c> (the Plugin -> UI dependency direction is out of I-16's scope).
    /// </summary>
    internal static class RackCatalogLoader
    {
        internal static RackCatalog Load()
        {
            RackCatalog catalog;
            try
            {
                catalog = JsonRackCatalogProvider.FromBaseDirectory().Load();
            }
            catch (Exception ex)
            {
                // I-03 (P1): a broken catalog used to degrade to "faltan bloques" with no clue. Record WHY it
                // failed, then keep working with an empty catalog — the control flow is unchanged.
                RackLog.Exception("Carga del catalogo de producto", ex);
                return new RackCatalog();
            }

            if (IsEmpty(catalog))
            {
                // I-03 (P1): the "aviso de catálogo vacío". A folder that loaded with no profiles and no blocks
                // will make every draw omit pieces as "faltan bloques"; leave a trace (log only — this runs
                // mid-draw, so it must not write to the command line).
                RackLog.Warning(
                    "Catalogo de producto",
                    "El catalogo se cargo vacio (sin perfiles ni bloques): el dibujo omitira piezas por 'faltan bloques'.");
            }

            return catalog;
        }

        /// <summary>True when the catalog has no structural profiles and no block definitions — effectively unusable.</summary>
        private static bool IsEmpty(RackCatalog catalog)
        {
            return catalog == null
                || (catalog.PostProfiles.Count == 0
                    && catalog.TrussProfiles.Count == 0
                    && catalog.BeamProfiles.Count == 0
                    && catalog.Blocks.Count == 0);
        }
    }
}
