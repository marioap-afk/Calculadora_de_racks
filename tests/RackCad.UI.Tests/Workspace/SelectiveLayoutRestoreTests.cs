#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.ProjectVariables;
using RackCad.Application.RackFrames;
using RackCad.Application.Systems.Selective;
using RackCad.Application.VentanaRackCad;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems.Selective;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-64 A-14, line D (Selectivo E2-B, decision 8): «Restaurar layout…» over the draft. With every box checked the draft becomes the
    /// same rack without its personalisations (byte for byte, as the equivalence tests of E1 compare); each box restores only its own
    /// personalisation, and the result counts what it restored for the notice.
    /// </summary>
    public class SelectiveLayoutRestoreTests
    {
        private const string PostId = "POSTE_OMEGA_ATORNILLABLE_CON_TROQUEL_GOTA_DE_AGUA";
        private const string BeamId = "LARGUERO_ESCALON_CAL14_3_REMACHES";
        private static readonly SelectivePalletDesignStore Store = new SelectivePalletDesignStore();

        private static string Json(SelectivePalletDesign? design)
            => Store.Serialize(SelectivePalletDesignDocument.From(design!, "GUID-SEL-RESTAURAR", "Selectivo R"));

        private static SelectiveCell Cell(double frente = 42.0, double alto = 60.0)
            => new SelectiveCell { Pallet = new Tarima { Frente = frente, Alto = alto }, PalletCount = 2, BeamId = BeamId, BeamPeralte = 4.0 };

        private static SelectiveBayDesign Bay(int levels, double frente = 42.0, double alto = 60.0)
        {
            var bay = new SelectiveBayDesign { FloorBeamRiseOverride = 4.0 };
            for (var level = 0; level < levels; level++) bay.Levels.Add(Cell(frente, alto));
            return bay;
        }

        /// <summary>Two fronts, three posts; <paramref name="personalised"/> adds the six personalisations of the list.</summary>
        private static SelectivePalletDesign Design(RackCatalog catalog, bool personalised)
        {
            var design = new SelectivePalletDesign
            {
                PostId = PostId, PostPeralte = 3.0, PalletTolerance = 4.0, VerticalClearance = 6.0, FloorBeamRise = 4.0,
                PalletDepth = 48.0, DepthCount = 1, DrawBasePlate = true,
            };
            design.Bays.Add(Bay(3, frente: 40.0));
            design.Bays.Add(Bay(2, frente: 44.0, alto: 55.0));
            design.CabeceraFondoOverrides.Add(0.0);
            foreach (var peralte in new[] { personalised ? 5.0 : 0.0, 0.0, 0.0 }) design.PostPeraltes.Add(peralte);
            if (!personalised) return design;

            design.Bays[0].Levels[0].BeamLengthOverride = 100.0;
            design.Bays[1].Levels[1].ClearOverride = 80.0;
            design.Bays[0].Segments.Add(new SelectiveSegment { Length = 40.0, Loaded = true });
            design.Bays[0].Segments.Add(new SelectiveSegment { Length = 0.0, Loaded = true });
            design.Bays[1].HeightOverride = 200.0;
            var custom = new RackFrameConfigurationFactory(catalog).Build(RackFrameTemplateCatalog.FindStandardOrDefault(), PostId, 120.0, 42.0);
            foreach (var cabecera in new[] { null, custom, null }) design.PostCabeceras.Add(cabecera);
            return design;
        }

        private static SelectiveDraftEditor Open(RackCatalog catalog, bool personalised)
        {
            var draft = new SelectiveDraftEditor(catalog);
            Assert.True(draft.LoadFrom(Design(catalog, personalised), new Dictionary<PropertyId, LinkedPropertyEditState>(), "Selectivo R"), draft.LastError);
            return draft;
        }

        // The six personalisations, read off the draft, in the order of the list.
        private static bool[] Present(SelectiveDraftEditor draft) => new[]
        {
            draft.Cell(0, 0, 0).BeamLength.HasValue,
            draft.Cell(0, 1, 1).Clear.HasValue,
            draft.Front(0, 0).IsSplit,
            draft.Front(0, 1).HeightOverride.HasValue,
            draft.PostPeralteOverride(0).HasValue,
            draft.HasCustomCabecera(0, 1),
        };

        private static SelectiveLayoutRestoreOptions Only(int box, bool value) => new SelectiveLayoutRestoreOptions(
            box == 0 ? value : !value, box == 1 ? value : !value, box == 2 ? value : !value,
            box == 3 ? value : !value, box == 4 ? value : !value, box == 5 ? value : !value);

        [Fact]
        public void EveryBoxCheckedIsTheSameRackWithoutItsPersonalisations()
        {
            var catalog = UiSupport.LoadCatalogSafe();
            var plain = Open(catalog, personalised: false);
            var draft = Open(catalog, personalised: true);
            Assert.All(Present(draft), Assert.True);

            var preview = SelectiveLayoutRestore.Preview(draft, SelectiveLayoutRestoreOptions.All);
            Assert.All(Present(draft), Assert.True); // the preview writes nothing
            var result = SelectiveLayoutRestore.Apply(draft, SelectiveLayoutRestoreOptions.All);

            Assert.Equal(preview, result);
            Assert.Equal((1, 1, 1, 1, 1, 1, false), (result.BeamLengths, result.Clears, result.HalfFronts, result.ManualHeights, result.PostPeraltes, result.CustomCabeceras, result.PostPeraltesSkipped));
            Assert.All(Present(draft), Assert.False);
            Assert.Equal(Json(plain.Design), Json(draft.Design));
            // The notice of the screen (spec-restaurar §6); it replaced the provisional «Restaurado: …» of the model part.
            Assert.Equal("Layout restaurado en el borrador: 6 valores manuales quitados (1 largo de larguero, 1 claro, 1 medio frente, 1 altura manual, "
                         + "1 peralte propio, 1 cabecera personalizada). «Descartar» vuelve a lo último que se leyó o se escribió.", result.Notice);
        }

        /// <summary>
        /// Equivalence with the classic editor: every box checked writes the design the classic window builds for the same rack without
        /// its personalisations (opened as RACKEDITAR opens it).
        /// </summary>
        [Fact]
        public void EveryBoxCheckedIsWhatTheClassicEditorBuildsForThePlainRack()
        {
            StaTestRunner.Run(() =>
            {
                var window = SelectiveWindowTestSupport.Open(canInsertInAutoCad: true);
                try
                {
                    var catalog = window.Session.Catalog;
                    var plainJson = Json(Design(catalog, personalised: false));
                    var open = SelectiveEditorOpen.Resolve(Store.Deserialize(plainJson), ProjectVariablesReadResult.Absent());
                    Assert.True(open.IsOpen, open.Error);
                    window.LoadExisting(Store.Deserialize(plainJson), open.Design, open.LinkedPropertyStates);
                    var classic = window.BuildDesignForTest(out var error);
                    Assert.Null(error);

                    var draft = Open(catalog, personalised: true);
                    SelectiveLayoutRestore.Apply(draft, SelectiveLayoutRestoreOptions.All);
                    Assert.Equal(Json(classic), Json(draft.Design));
                }
                finally { window.Close(); }
            });
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        public void EachBoxRestoresOnlyItsOwnPersonalisation(int box)
        {
            var catalog = UiSupport.LoadCatalogSafe();

            var onlyThis = Open(catalog, personalised: true);
            var restored = SelectiveLayoutRestore.Apply(onlyThis, Only(box, true));
            Assert.Equal(1, restored.Total);
            var present = Present(onlyThis);
            for (var i = 0; i < present.Length; i++) Assert.Equal(i != box, present[i]);

            var allButThis = Open(catalog, personalised: true);
            var kept = SelectiveLayoutRestore.Apply(allButThis, Only(box, false));
            Assert.Equal(5, kept.Total);
            present = Present(allButThis);
            for (var i = 0; i < present.Length; i++) Assert.Equal(i == box, present[i]);
        }

        /// <summary>The editor of the window: «Restaurar» with one box unchecked restores the rest, marks the draft modified and says what it restored.</summary>
        [Fact]
        public void TheEditorRestoresTheCheckedBoxesAndSaysWhatItRestored()
        {
            var catalog = UiSupport.LoadCatalogSafe();
            var vm = SelectiveWindowFixtures.SelectiveEditor(Design(catalog, personalised: true));
            Assert.False(vm.IsDirty);
            Assert.Equal(SelectiveLayoutRestoreOptions.All, vm.RestoreOptions);

            vm.RestoreOptions = SelectiveLayoutRestoreOptions.All with { CustomCabeceras = false };
            Assert.Null(vm.RestoreLayout());

            Assert.True(vm.IsDirty);
            Assert.Equal("Layout restaurado en el borrador: 5 valores manuales quitados (1 largo de larguero, 1 claro, 1 medio frente, 1 altura manual, "
                         + "1 peralte propio). «Descartar» vuelve a lo último que se leyó o se escribió. Se conservan: 1 cabecera personalizada.", vm.Message);
        }

        /// <summary>
        /// «Ver lo que se pierde»: one line per value with the texts of the approved draft (<c>lossList</c>) and in its order (front by front:
        /// half front, manual height, then each level; the own peraltes; the custom cabeceras), and each box has as many lines as it counts.
        /// </summary>
        [Fact]
        public void TheLinesOfWhatIsLostAreThoseOfTheDraftOnePerValueAsManyAsEachBoxCounts()
        {
            var catalog = UiSupport.LoadCatalogSafe();
            var draft = Open(catalog, personalised: true);
            var losses = SelectiveLayoutRestore.Losses(draft);

            Assert.Equal(new[]
            {
                "Medio frente en F1 (2 tramos)",
                "Largo manual de larguero en F1 · N1 (100 in)",
                "Altura manual de cabecera en F2 (200 in)",
                "Claro manual en F2 · N2 (80 in)",
                "Peralte propio en P1 (5 in)",
                "Cabecera personalizada en P2",
            }, losses.Select(l => l.Text).ToArray());
            var preview = SelectiveLayoutRestore.Preview(draft, SelectiveLayoutRestoreOptions.All);
            foreach (var box in Enum.GetValues<SelectiveLayoutRestoreBox>())
            {
                Assert.Equal(preview.Count(box), losses.Count(l => l.Box == box));
                Assert.True(SelectiveLayoutRestoreOptions.All.Includes(box));
                Assert.False(SelectiveLayoutRestoreOptions.None.Includes(box));
            }

            Assert.Empty(SelectiveLayoutRestore.Losses(Open(catalog, personalised: false)));
        }

        /// <summary>The notice (spec §6): the total, singular with one value, the categories with something in the order of the list, and what the unchecked boxes keep.</summary>
        [Fact]
        public void TheNoticeSaysHowManyValuesWereRemovedWhichAndWhatTheUncheckedBoxesKeep()
        {
            var catalog = UiSupport.LoadCatalogSafe();
            var result = SelectiveLayoutRestore.Apply(Open(catalog, personalised: true), SelectiveLayoutRestoreOptions.All with { Clear = false, CustomCabeceras = false });
            Assert.Equal("Layout restaurado en el borrador: 4 valores manuales quitados (1 largo de larguero, 1 medio frente, 1 altura manual, 1 peralte propio). "
                         + "«Descartar» vuelve a lo último que se leyó o se escribió. Se conservan: 1 claro, 1 cabecera personalizada.", result.Notice);
            Assert.Equal((0, 1, 0, 0, 0, 1), (result.Kept.BeamLengths, result.Kept.Clears, result.Kept.HalfFronts, result.Kept.ManualHeights, result.Kept.PostPeraltes, result.Kept.CustomCabeceras));

            var one = SelectiveLayoutRestore.Apply(Open(catalog, personalised: true), Only(1, true));
            Assert.Equal("Layout restaurado en el borrador: 1 valor manual quitado (1 claro). «Descartar» vuelve a lo último que se leyó o se escribió. "
                         + "Se conservan: 1 largo de larguero, 1 medio frente, 1 altura manual, 1 peralte propio, 1 cabecera personalizada.", one.Notice);
        }

        [Fact]
        public void NothingCheckedOrNothingPersonalisedRestoresNothing()
        {
            var catalog = UiSupport.LoadCatalogSafe();
            var draft = Open(catalog, personalised: true);
            Assert.False(SelectiveLayoutRestoreOptions.None.Any);
            Assert.Equal(0, SelectiveLayoutRestore.Apply(draft, SelectiveLayoutRestoreOptions.None).Total);
            Assert.All(Present(draft), Assert.True);

            var plain = Open(catalog, personalised: false);
            var result = SelectiveLayoutRestore.Apply(plain, SelectiveLayoutRestoreOptions.All);
            Assert.Equal(0, result.Total);
            Assert.Equal("No había nada que restaurar.", result.Notice);
        }
    }
}
