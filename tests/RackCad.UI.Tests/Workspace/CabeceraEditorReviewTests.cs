#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.RackFrames;
using RackCad.Application.VentanaRackCad;
using RackCad.Domain.RackFrames;
using RackCad.UI.VentanaRackCad;
using Xunit;
using static RackCad.UI.Tests.SelectiveWindowFixtures;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-64, fourth batch, part cb: the corrections of the independent review of the editor of cabeceras (points 1 to 6; point 7, the name,
    /// is in <see cref="RackCadCabeceraWindowTests"/> and in Core). The standard cabecera of 132 × 42 in has H1 4 · H2 48 · H3 92 · H4 110 ·
    /// H5 128 in; at 180 in the template gives 4 · 48 · 92 · 136 · 156 · 176 in.
    /// </summary>
    public class CabeceraEditorReviewTests
    {
        private static RackFrameConfiguration Standard132()
            => new RackFrameConfigurationFactory(UiSupport.LoadCatalogSafe()).Build(RackFrameTemplateCatalog.FindStandardOrDefault(), PostId, 132.0, 42.0);

        private static CabeceraDraftEditor Draft(RackFrameConfiguration? opened = null, IReadOnlyList<RackFrameTemplate>? templates = null)
            => new CabeceraDraftEditor(opened ?? Standard132(), UiSupport.LoadCatalogSafe(), templates ?? RackFrameTemplateCatalog.All, CabeceraDraftContext.Standalone);

        private static CabeceraEditorViewModel Standalone(CabeceraDraftEditor? draft = null)
            => CabeceraEditorViewModel.Standalone(draft ?? Draft(), "CAB-1", _ => { });

        // ------------------------------------------------------------------ 1. the peralte of the same post in another fondo

        [Fact]
        public void CopyingToTheSamePostInAnotherFondoKeepsThePeralteJustEdited()
        {
            // Two fondos, the peralte of the rack 3; P2 has none of its own. Its peralte is written 4 in P2 · Fondo 1 and copied to P2 · Fondo 2.
            var vm = SelectiveEditor(TwoFondos());
            vm.EditCabecera(0, 1);
            var cab = vm.EmbeddedCabecera!;
            Write(Field(cab.Essentials, "peralte"), "4");
            Assert.True(cab.IsDirty);

            cab.OpenCopy();
            cab.CopyTargets.Single(t => t.Label == "P2 · Fondo 2").IsChecked = true;
            cab.CopyTargets.Single(t => t.Label == "P3 · Fondo 2").IsChecked = true;
            cab.ConfirmCopy();

            Assert.False(vm.HasEmbeddedCabecera, vm.Message);
            var design = vm.Draft.Design;
            Assert.Equal(4.0, design.PostPeraltes[1]);
            Assert.Equal(4.0, design.PostCabeceras[1].PostPeralte);
            Assert.Equal(4.0, design.ExtraFondoPostCabeceras[0][1].PostPeralte);

            // Another post keeps its own peralte: P3 has none, so the one of the rack.
            Assert.Equal(0.0, design.PostPeraltes[2]);
            Assert.Equal(3.0, design.ExtraFondoPostCabeceras[0][2].PostPeralte);
        }

        // ------------------------------------------------------------------ 2. a calculated cabecera with no change is not a source

        [Fact]
        public void ACalculatedCabeceraWithNoChangeIsNotCopiedAndACustomOneIs()
        {
            var vm = SelectiveEditor(Structure());
            vm.EditCabecera(0, 1);
            var cab = vm.EmbeddedCabecera!;

            Assert.False(cab.CanCopy);
            Assert.Equal("Una cabecera calculada y sin cambios no se copia: cámbiala aquí primero (en el clásico, «Reutilizar» tampoco toma un origen estándar).", cab.CopyBlockedText);
            cab.OpenCopy();
            Assert.False(cab.IsCopyOpen);
            cab.ConfirmCopy();
            Assert.True(vm.HasEmbeddedCabecera);
            Assert.False(vm.Draft.HasCustomCabecera(0, 1));
            Assert.False(vm.Draft.HasCustomCabecera(0, 2));

            // Made custom, the same cabecera opened again with no change is a source.
            Choice(cab.PanelFields, "pArr").SelectedValue = BracingPattern.XBracing.ToString();
            cab.Apply();
            vm.EditCabecera(0, 1);
            var again = vm.EmbeddedCabecera!;
            Assert.False(again.IsDirty);
            Assert.True(again.CanCopy);
            Assert.Equal(string.Empty, again.CopyBlockedText);
        }

        // ------------------------------------------------------------------ 3. what a regeneration loses: notes and faces

        [Fact]
        public void RegeneratingKeepsTheNotesWhereTheirElevationStaysAndAsksForTheOthers()
        {
            var vm = Standalone();
            vm.ClickRow(false, 1, false, false);
            Write(Field(vm.HorizontalFields, "hNotes"), "revisar soldadura");

            // 48 in stays at 180 in: nothing is lost, it regenerates without asking and the note stays on its horizontal.
            Write(Field(vm.Essentials, "alto"), "180");
            Assert.False(vm.IsRegenOpen);
            Assert.Equal(180.0, vm.Editor.Height);
            Assert.Equal("revisar soldadura", vm.Editor.Horizontals.Single(h => Math.Abs(h.Elevation - 48.0) < 0.01).Notes);

            // 136 in does not stay at 132 in: the regeneration asks and says the note.
            vm.ClickRow(false, 3, false, false);
            Write(Field(vm.HorizontalFields, "hNotes"), "placa");
            Write(Field(vm.Essentials, "alto"), "132");
            Assert.True(vm.IsRegenOpen);
            Assert.Equal(new[] { "H4: nota «placa» (su elevación no queda)" }, vm.RegenLosses.ToArray());
            vm.ConfirmRegen(false);
            Assert.Equal(180.0, vm.Editor.Height);
        }

        [Fact]
        public void AFaceThatIsNotTheFrontOneIsALossOfRegeneratingAndOfRestoring()
        {
            var opened = Standard132();
            opened.BracingPanels[0].MountingFace = FrameSide.Back;
            opened.Horizontals[2].MountingFace = FrameSide.Both;
            var draft = Draft(opened);

            var losses = draft.RegenerationLossesForTemplate("TALL-4P").Select(c => c.Line).ToList();
            Assert.Contains("H3: ambas caras → cara frontal", losses);
            Assert.Contains("P1: cara posterior → cara frontal", losses);
            Assert.Contains("P1: cara posterior → cara frontal", draft.RestoreLosses(new[] { CabeceraChangeCategory.Panels }).Select(c => c.Line));
            Assert.Empty(draft.RegenerationLossesForTemplate(draft.TemplateId)); // the same template does not regenerate
        }

        // ------------------------------------------------------------------ 4. «Restaurar» of the horizontals only

        [Fact]
        public void RestoringOnlyTheHorizontalsSaysWhichPanelsLoseTheirValues()
        {
            var draft = Draft();
            Assert.True(draft.SetElevation(1, 54.0).Applied);
            Assert.True(draft.SetPanelArrangement(new[] { 0 }, BracingPattern.XBracing).Applied);

            var lines = draft.RestoreLosses(new[] { CabeceraChangeCategory.Horizontals }).Select(c => c.Line).ToList();

            Assert.Contains("H2: elevación 48 → 54 in", lines);
            Assert.Contains("P1: su tramo de 4 a 54 in no queda; pierde celosía en X y toma los valores de la plantilla", lines);
            // P2 (54 to 92 in, a single diagonal) becomes the standard P2 (48 to 92 in, a single diagonal): it loses nothing.
            Assert.DoesNotContain(lines, l => l.StartsWith("P2:", StringComparison.Ordinal));

            // What it said is what it does.
            Assert.True(draft.Restore(new[] { CabeceraChangeCategory.Horizontals }).Applied);
            Assert.Equal(BracingPattern.SingleDiagonal, draft.Panels[0].Arrangement);

            // The view-model lists it with «Paneles» unmarked.
            var vm = Standalone();
            vm.ClickRow(false, 1, false, false);
            Write(Field(vm.HorizontalFields, "hElev"), "54");
            vm.ClickRow(true, 0, false, false);
            Choice(vm.PanelFields, "pArr").SelectedValue = BracingPattern.XBracing.ToString();
            vm.OpenRestore();
            vm.RestoreBoxes.Single(b => b.Category == CabeceraChangeCategory.Panels).IsChecked = false;
            Assert.Contains("P1: su tramo de 4 a 54 in no queda; pierde celosía en X y toma los valores de la plantilla", vm.RestoreLosses);
        }

        [Fact]
        public void RestoringTheHorizontalsKeepsTheNotesWhereTheirElevationStays()
        {
            var draft = Draft();
            draft.SetHorizontalNotes(new[] { 2 }, "revisar");
            draft.SetHorizontalQuantity(new[] { 2 }, 2);

            Assert.True(draft.Restore(new[] { CabeceraChangeCategory.Horizontals }).Applied);

            Assert.Equal(1, draft.Horizontals[2].Quantity);
            Assert.Equal("revisar", draft.Horizontals[2].Notes);
        }

        // ------------------------------------------------------------------ 5. the minimum clear

        [Fact]
        public void AClearUnderTheMinimumIsRefusedWithItsReasonAndNothingChanges()
        {
            var draft = Draft();
            var json = draft.Json;

            var clear = draft.SetPanelClear(new[] { 0 }, 3.0);
            Assert.False(clear.Applied);
            Assert.Equal("El claro debe ser de al menos 4 in (el mínimo de un panel).", clear.Error);
            Assert.False(draft.AddPanelOnTop(2.0).Applied);
            var near = draft.SetElevation(1, 6.0);
            Assert.False(near.Applied);
            Assert.Contains("H1", near.Error);
            Assert.False(draft.ShiftHorizontals(new[] { 1 }, 42.0).Applied); // 90 in, 2 in under H3
            Assert.Equal(json, draft.Json);

            // «Dividir en dos» leaves halves of the minimum at least.
            Assert.True(draft.SetPanelClear(new[] { 0 }, 6.0).Applied);
            var split = draft.SplitPanel(0);
            Assert.False(split.Applied);
            Assert.Equal("El panel es demasiado bajo para dividirlo: cada mitad debe tener al menos 4 in.", split.Error);

            // The view-model says it in the field and under «Agregar panel arriba».
            var vm = Standalone();
            var field = Field(vm.PanelFields, "pClear");
            Write(field, "3");
            Assert.Equal("El claro debe ser de al menos 4 in (el mínimo de un panel).", field.Error);
            Assert.False(vm.IsDirty);
            vm.AddClear = "3";
            Assert.False(vm.CanAddPanel);
            Assert.Equal("El claro del panel nuevo debe ser de al menos 4 in (el mínimo de un panel).", vm.AddPanelNote);
        }

        // ------------------------------------------------------------------ 6. the seed and the standard, from the same templates

        [Fact]
        public void TheSeedOfACalculatedPostAndTheStandardOfTheEditorComeFromTheSameTemplates()
        {
            // A catalog whose standard template differs from the built-in one (here, a celosía in X).
            var builtIn = RackFrameTemplateCatalog.FindStandardOrDefault();
            var crossed = new RackFrameTemplate
            {
                Id = builtIn.Id, Name = builtIn.Name, DefaultHeight = builtIn.DefaultHeight, DefaultDepth = builtIn.DefaultDepth,
                Horizontals = builtIn.Horizontals, DefaultArrangement = BracingPattern.XBracing, DiagonalProfile = builtIn.DiagonalProfile,
                BraceStartConnectionPoint = builtIn.BraceStartConnectionPoint, BraceEndConnectionPoint = builtIn.BraceEndConnectionPoint,
                BasePlate = builtIn.BasePlate, Post = builtIn.Post,
            };
            var templates = new[] { crossed }.Concat(RackFrameTemplateCatalog.All.Skip(1)).ToList();
            var vm = SelectiveEditor(Structure());

            var seed = vm.Draft.CabeceraSeed(0, 1, templates)!;
            var draft = new CabeceraDraftEditor(seed, vm.Draft.Catalog, templates, vm.Draft.CabeceraContext(0, 1, "Selectivo A"));

            Assert.Equal(BracingPattern.XBracing, seed.BracingPanels[0].Arrangement);
            Assert.Empty(draft.Changes());
            Assert.False(draft.IsDirty);
        }
    }
}
