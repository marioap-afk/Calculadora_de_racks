using System;
using RackCad.Application.Catalogs;
using RackCad.Application.StructuralSections;
using RackCad.Application.VentanaRackCad;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// «Nuevo › Larguero» of the RackCad window (I-64 A-14, line D, quinta tanda, part «lg»): the page that makes a larguero from a section and
    /// a ménsula, a DRAFT IN CONSTRUCTION (LG-01 to LG-03 of the Owner). It is a page of the window (<see cref="RackCadWindowPage.Larguero"/>),
    /// under «Nuevo» in the side column, and not a «Borrador N» of the drawing: a larguero is a component of the library, not a rack, it is
    /// never inserted and its work never vetoes the close of a drawing (assumption LG-S01). Its view-model is made the first time the page
    /// opens and lives as long as the window; every opening reads the two catalogs again (review of part «lg», point 1). Nothing here reads
    /// or writes the drawing.
    /// </summary>
    public sealed partial class RackCadWindowViewModel
    {
        private LargueroWorkbenchViewModel _largueroWorkbench;

        /// <summary>
        /// The neutral catalog of structural sections the page reads: the ONE source of RACKSECCION (T7 per, review I-1; the page keeps its
        /// own notices when it cannot read it); the tests give theirs.
        /// </summary>
        internal Func<StructuralSectionCatalog> LargueroSectionsLoader { get; set; } = StructuralSectionCatalogSource.Load;

        /// <summary>
        /// The catalog of RackCad the page reads: the provider itself, which caches by the signature of the files and THROWS when it cannot
        /// read them, so the page says it (review of part «lg», point 1). Not <c>UiSupport.LoadCatalogSafe</c>, which gives an empty catalog
        /// in silence. The tests give theirs.
        /// </summary>
        internal Func<RackCatalog> LargueroRackCatalogLoader { get; set; } = LoadRackCatalog;

        /// <summary>«Nuevo › Larguero» is shown.</summary>
        public bool IsLargueroPage => _page == RackCadWindowPage.Larguero;

        /// <summary>The page «Nuevo › Larguero»; null until it is opened for the first time.</summary>
        public LargueroWorkbenchViewModel LargueroWorkbench => _largueroWorkbench;

        /// <summary>
        /// «Larguero en esta ventana» of «Nuevo»: shows the page (made the first time). Every opening reads the catalogs again: an edit of a
        /// CSV shows up, and what was chosen stays (<see cref="LargueroWorkbenchViewModel.Reload"/>). Contained, as every action of the window.
        /// </summary>
        public void OpenLargueroWorkbench()
        {
            Contained(() =>
            {
                if (_largueroWorkbench == null)
                {
                    _largueroWorkbench = new LargueroWorkbenchViewModel(() => LargueroRackCatalogLoader(), () => LargueroSectionsLoader(),
                        () => ShowPage(RackCadWindowPage.Nuevo), () => Run(RackCadClassicAction.NewLarguero), ShowError);
                    OnPropertyChanged(nameof(LargueroWorkbench));
                }
                else
                {
                    _largueroWorkbench.Reload();
                }

                ShowPage(RackCadWindowPage.Larguero);
            });
        }

        // ShowPage: what the center shows follows the page.
        private void RaiseLargueroPage() => OnPropertyChanged(nameof(IsLargueroPage));

        // The side column marks «Nuevo» while one of its pages is shown: «Larguero» (the crumb «Nuevo › Larguero») and, T7 per, «Perfil
        // estructural» (the crumb «Nuevo › Perfil estructural»).
        private bool IsUnderNuevo(RackCadWindowPage section) =>
            section == RackCadWindowPage.Nuevo && (_page == RackCadWindowPage.Larguero || _page == RackCadWindowPage.PerfilEstructural); // T7 per: and «Nuevo › Perfil estructural»

        private static RackCatalog LoadRackCatalog() => JsonRackCatalogProvider.FromBaseDirectory().Load();
    }
}
