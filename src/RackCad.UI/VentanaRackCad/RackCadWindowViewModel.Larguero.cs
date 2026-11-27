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
    /// opens, when it reads the two catalogs, and it lives as long as the window. Nothing here reads or writes the drawing.
    /// </summary>
    public sealed partial class RackCadWindowViewModel
    {
        private LargueroWorkbenchViewModel _largueroWorkbench;

        /// <summary>The neutral catalog of structural sections the page reads (the one of RACKSECCION); the tests give theirs.</summary>
        internal Func<StructuralSectionCatalog> LargueroSectionsLoader { get; set; } = LoadStructuralSections;

        /// <summary>«Nuevo › Larguero» is shown.</summary>
        public bool IsLargueroPage => _page == RackCadWindowPage.Larguero;

        /// <summary>The page «Nuevo › Larguero»; null until it is opened for the first time.</summary>
        public LargueroWorkbenchViewModel LargueroWorkbench => _largueroWorkbench;

        /// <summary>
        /// «Larguero en esta ventana» of «Nuevo»: shows the page (made, and its catalogs read, the first time). What was chosen stays while the
        /// window lives. Contained, as every action of the window.
        /// </summary>
        public void OpenLargueroWorkbench()
        {
            Contained(() =>
            {
                if (_largueroWorkbench == null)
                {
                    _largueroWorkbench = new LargueroWorkbenchViewModel(_catalog, LargueroSectionsLoader,
                        () => ShowPage(RackCadWindowPage.Nuevo), () => Run(RackCadClassicAction.NewLarguero), ShowError);
                    OnPropertyChanged(nameof(LargueroWorkbench));
                }

                ShowPage(RackCadWindowPage.Larguero);
            });
        }

        // ShowPage: what the center shows follows the page.
        private void RaiseLargueroPage() => OnPropertyChanged(nameof(IsLargueroPage));

        // The side column marks «Nuevo» while its page «Larguero» is shown (the crumb «Nuevo › Larguero»).
        private bool IsUnderNuevo(RackCadWindowPage section) => section == RackCadWindowPage.Nuevo && _page == RackCadWindowPage.Larguero;

        private static StructuralSectionCatalog LoadStructuralSections() =>
            new CsvStructuralSectionCatalogProvider(CatalogDirectory.Resolve()).Load();
    }
}
