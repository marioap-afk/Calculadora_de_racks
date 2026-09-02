using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems.PushBack;
using RackCad.Application.Systems.Selective;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;
using RackCad.UI;
using RackCad.UI.Controls;
using RackCad.UI.Systems.PushBack;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// PB-005 / PB-006 (I-32) — the rear-stop experience inside "Elementos de seguridad".
    ///
    /// PB-005: the stop TYPE is picked from the catalog's TOPE family (there was no selector at all, only the default
    /// piece). The choice lives in the section's working copy, so the host applies it with the SAQUE and the cells.
    ///
    /// PB-006: Push Back has one depth line, so "Compartido (uno central)" and "Lado" have nothing to decide there and
    /// are not offered. The Selectivo path keeps both.
    /// </summary>
    public sealed class PushBackTopeDialogOptionsTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private const string SharedCheckBox = "Compartido (uno central)";
        private const string Alternative = "POSTE_3_1_5_8_TOPE";

        // ---- PB-006 ----

        [Fact]
        public void TopeGrid_ForPushBack_OffersNeitherCompartidoNorLado_ButStillOffersSaque()
        {
            var r = StaTestRunner.Run(() =>
            {
                var pushBack = new SafetyTopeGridWindow(
                    "Tope posterior", new[] { 2 }, shared: false, side: SafetySide.Left, saque: 3.0,
                    frontal: false, offCells: null, fondoCount: 1, fondo: -1, showSharedAndSide: false);
                var selective = new SafetyTopeGridWindow(
                    "Tope", new[] { 2 }, shared: true, side: SafetySide.Right, saque: 3.0,
                    frontal: false, offCells: null, fondoCount: 1, fondo: -1);

                return (
                    pbShared: SafetyDialogTestSupport.HasCheckBox(pushBack, SharedCheckBox),
                    pbSide: SafetyDialogTestSupport.HasSideSelector(pushBack),
                    pbSaque: SafetyDialogTestSupport.HasText(pushBack, "Saque (in):"),
                    pbResult: pushBack.BuildResult(),
                    selShared: SafetyDialogTestSupport.HasCheckBox(selective, SharedCheckBox),
                    selSide: SafetyDialogTestSupport.HasSideSelector(selective),
                    selResult: selective.BuildResult());
            });

            Assert.False(r.pbShared);
            Assert.False(r.pbSide);
            Assert.True(r.pbSaque);   // the walk DOES reach the options row, so the two absences above are meaningful

            // The result still carries the canonical values Push Back always passed in, with no null dereference.
            Assert.NotNull(r.pbResult);
            Assert.False(r.pbResult.Shared);
            Assert.Equal(SafetySide.Left, r.pbResult.Side);
            Assert.Equal(3.0, r.pbResult.Saque, 6);

            // The default path (Selectivo) is untouched: both controls are there and their values are honoured.
            Assert.True(r.selShared);
            Assert.True(r.selSide);
            Assert.True(r.selResult.Shared);
            Assert.Equal(SafetySide.Right, r.selResult.Side);
        }

        // ---- PB-005 ----

        [Fact]
        public void RearTopeSection_OffersTheCatalogTopeVariants_AndDefaultsToTheResolvedPiece()
        {
            var catalog = Catalog;
            var r = StaTestRunner.Run(() =>
            {
                var section = new PushBackRearTopeSection(new PushBackRearTopeConfig(), _ => null, catalog);
                return (
                    ids: section.PieceBox.Items.OfType<CatalogOption>().Select(option => option.Id).ToArray(),
                    selected: section.PieceBox.SelectedId,
                    present: SafetyDialogTestSupport.HasText(section.View as System.Windows.DependencyObject, PushBackRearTopeSection.PieceLabelText));
            });

            var expected = SelectiveSafetyFamilies
                .VariantsOfType(catalog.SafetyElements, SelectiveSafetyDefaults.TopeType)
                .Select(entry => entry.Id)
                .ToList();

            Assert.True(r.present);
            Assert.NotEmpty(expected);

            // I-42 (ronda 7C): el selector ofrece las variantes del catalogo Y «Ninguno», que es la forma EXPLICITA
            // de decir que este objetivo no lleva tope. Sigue abriendo en la pieza resuelta, no en «Ninguno».
            expected.Insert(0, PushBackRearTopeConfig.NonePieceId);
            Assert.Equal(expected.OrderBy(id => id), r.ids.OrderBy(id => id));   // the SET is the catalog's, order is the combo's
            Assert.Equal(PushBackRearTopeBuilder.TopePieceId, r.selected);
        }

        [Fact]
        public void RearTopeSection_WritesTheChoiceIntoItsWorkingCopy_AndCarriesAnExistingOne()
        {
            var catalog = Catalog;
            var r = StaTestRunner.Run(() =>
            {
                // A rack that already chose the alternative: the working COPY must carry it, or pressing Aceptar
                // without touching anything would silently reset the stop to the default piece.
                var carried = new PushBackRearTopeSection(
                    new PushBackRearTopeConfig { PieceId = Alternative }, _ => null, catalog);

                var chosen = new PushBackRearTopeSection(new PushBackRearTopeConfig(), _ => null, catalog);
                chosen.PieceBox.SelectedId = Alternative;

                return (carriedId: carried.Config.PieceId, carriedSelected: carried.PieceBox.SelectedId, chosenId: chosen.Config.PieceId);
            });

            Assert.Equal(Alternative, r.carriedId);
            Assert.Equal(Alternative, r.carriedSelected);
            Assert.Equal(Alternative, r.chosenId);
        }
    }
}
