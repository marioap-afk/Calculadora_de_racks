#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Bom;
using RackCad.Application.Drawing;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.FlowBed;
using RackCad.Application.VentanaRackCad;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.FlowBed;
using RackCad.Domain.Systems.Shared;
using RackCad.UI.Systems.Dynamic;
using RackCad.UI.VentanaRackCad;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// Step 3 of Dinámico (the Owner: «Debe de ser todo modificable»; by front; the pitch keeps its default; the slope as a rise per foot,
    /// by front; the width of the bed follows the pallet): the roller, its pitch, a brake every N rollers and the slope of each front, from
    /// the editor to the drawing format, the recipe of the bed, the slope, the BOM and the classic editor. A front without them is as before.
    /// </summary>
    public class DynamicBedAndSlopeTests
    {
        private const string Roller25 = "RODILLO_DE_TUBO_DE_2.5_CALIBRE_14";

        private static DynamicRackDesign Structure()
        {
            var design = new DynamicRackDesign
            {
                Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                PalletsDeep = 6,
                LoadLevels = 3,
                FirstLevelHeight = 6.0,
                BeamDepth = 4.0,
            };
            design.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 2, LoadLevels = 3, PalletsDeep = 6, DepthStartPosition = 1 });
            design.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1, LoadLevels = 2, PalletsDeep = 4, DepthStartPosition = 3 });
            return design;
        }

        private static string Json(DynamicRackDesign design) => new RackProjectStore().Serialize(RackProject.ForDynamic(design));

        private static DynamicDraftEditor Draft()
        {
            var draft = new DynamicDraftEditor(UiSupport.LoadCatalogSafe());
            draft.LoadFrom(Structure(), "DIN 1");
            Assert.True(draft.Recompose(), draft.LastError);
            return draft;
        }

        private static DynamicDraftEditor WithBed()
        {
            var draft = Draft();
            var front = draft.Matrix.Fronts[0];
            front.RollerId = Roller25;
            front.RollerPitch = 4.0;
            front.BrakeEveryRollers = 6;
            front.SlopePerFoot = 0.75;
            Assert.True(draft.Recompose(), draft.LastError);
            return draft;
        }

        [Fact]
        public void ARackWithoutABedWrittenIsWrittenExactlyAsBefore()
        {
            var json = Json(Draft().Design);
            foreach (var member in new[] { "RollerId", "RollerPitch", "BrakeEveryRollers", "SlopePerFoot" })
                Assert.DoesNotContain("\"" + member + "\"", json, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void TheBedAndTheSlopeOfAFrontGoToTheDrawingAndComeBack()
        {
            var draft = WithBed();
            var json = Json(draft.Design);

            var reopened = new DynamicDraftEditor(UiSupport.LoadCatalogSafe());
            reopened.LoadFrom(new RackProjectStore().Deserialize(json)!.DynamicDesign, "DIN 1");

            var front = reopened.Matrix.Fronts[0];
            Assert.Equal((Roller25, 4.0, 6, 0.75), (front.RollerId, front.RollerPitch, front.BrakeEveryRollers, front.SlopePerFoot));
            var other = reopened.Matrix.Fronts[1];
            Assert.Equal(((string?)null, (double?)null, (int?)null, (double?)null), (other.RollerId, other.RollerPitch, other.BrakeEveryRollers, other.SlopePerFoot));
            Assert.Equal(json, Json(reopened.Design));
        }

        [Fact]
        public void TheSlopePerFootChangesTheRiseOfItsFrontOnly()
        {
            var draft = Draft();
            var before = draft.FrontRises;

            draft.Matrix.Fronts[0].SlopePerFoot = 0.0;
            Assert.True(draft.Recompose(), draft.LastError);
            Assert.Equal(0.0, draft.FrontRises[0]);
            Assert.Equal(before[1], draft.FrontRises[1]);

            draft.Matrix.Fronts[0].SlopePerFoot = 1.5;
            Assert.True(draft.Recompose(), draft.LastError);
            Assert.True(draft.FrontRises[0] > before[0], draft.FrontRises[0] + " vs " + before[0]);
        }

        [Fact]
        public void ABrakeEveryNRollersPlacesTheBrakesByCountAndWithoutItTheyGoByThePalletAsBefore()
        {
            var catalog = UiSupport.LoadCatalogSafe();
            IReadOnlyList<HeaderBlockInstance> Bed(int? every) => new FlowBedLateralBuilder().Build(new FlowBedConfiguration
            {
                BedType = FlowBedType.Dynamic,
                LaneDepth = 300.0,
                PalletDepth = 48.0,
                RollerId = FlowBedDefaults.RollerId,
                BrakeEveryRollers = every,
            }, catalog);

            // With N = 5: between two brakes there are exactly 5 rollers (the one after the previous brake counts).
            var roles = Bed(5).Where(i => i.Role == HeaderBlockRole.Roller || i.Role == HeaderBlockRole.Brake)
                .OrderBy(i => i.Insertion.X).Select(i => i.Role).ToList();
            var runs = new List<int>();
            var count = 0;
            foreach (var role in roles)
            {
                if (role == HeaderBlockRole.Brake) { runs.Add(count); count = 0; }
                else count++;
            }

            Assert.NotEmpty(runs);
            Assert.All(runs, run => Assert.Equal(5, run));
            Assert.Equal(Bed(null).Count(i => i.Role == HeaderBlockRole.Brake), Bed(0).Count(i => i.Role == HeaderBlockRole.Brake));
            Assert.NotEqual(Bed(5).Count(i => i.Role == HeaderBlockRole.Brake), Bed(null).Count(i => i.Role == HeaderBlockRole.Brake));
        }

        [Fact]
        public void TheBomSaysTheBedOfAFrontAndABedAsBeforeKeepsItsLine()
        {
            var catalog = UiSupport.LoadCatalogSafe();
            var plain = SystemBomBuilder.Build(Draft().System, catalog).Components.Where(c => c.Category == "Cama").Select(c => c.Description).ToList();
            Assert.All(plain, description => Assert.DoesNotContain("rodillo", description));

            var beds = SystemBomBuilder.Build(WithBed().System, catalog).Components.Where(c => c.Category == "Cama").Select(c => c.Description).ToList();
            Assert.Contains(beds, d => d.Contains(" · rodillo 2.5\"") && d.Contains(" · paso 4\"") && d.Contains(" · freno cada 6 rodillos"));
            Assert.Contains(beds, d => !d.Contains("rodillo"));
        }

        [Fact]
        public void TheClassicEditorKeepsTheBedAndTheSlopeOfAFront()
        {
            StaTestRunner.Run(() =>
            {
                var window = new RackDynamicSystemWindow();
                try
                {
                    var design = new RackProjectStore().Deserialize(Json(WithBed().Design))!.DynamicDesign;
                    window.LoadExisting(design, "DIN-1", "DIN 1");
                    var classic = window.BuildDesignForTest(out var ok);
                    Assert.True(ok);

                    var front = classic.Fronts[0];
                    Assert.Equal((Roller25, 4.0, 6, 0.75), (front.RollerId, front.RollerPitch, front.BrakeEveryRollers, front.SlopePerFoot));
                }
                finally { window.Close(); }
            });
        }

        // ------------------------------------------------------------------ the editor: «Cama y pendiente» in «Avanzado»

        private static DynamicEditorViewModel Editor()
        {
            var draft = new DynamicDraftEditor(UiSupport.LoadCatalogSafe());
            draft.LoadFrom(new RackProjectStore().Deserialize(Json(Draft().Design))!.DynamicDesign, "DIN 1");
            return new DynamicEditorViewModel(draft, "DIN-1", _ => { });
        }

        private static DynamicFieldItem Field(DynamicEditorViewModel vm, string key) => vm.BedFields.Single(f => f.Key == key);

        private static string Commit(DynamicEditorViewModel vm, string key, string text)
        {
            var field = Field(vm, key);
            field.Text = text;
            field.Commit();
            return field.Error;
        }

        [Fact]
        public void TheBedFieldsWriteTheActiveFrontsOfTheSelection()
        {
            var vm = Editor();
            vm.SelectFront(0);
            Assert.StartsWith("Frentes de la selección: F1.", vm.BedScopeText);
            Assert.Equal(new[] { "bRoller", "bPitch", "bBrakes", "bSlope" }, vm.BedFields.Select(f => f.Key).ToArray());

            Assert.Equal(string.Empty, Commit(vm, "bRoller", "2.5"));
            Assert.Equal(string.Empty, Commit(vm, "bPitch", "4"));
            Assert.Equal(string.Empty, Commit(vm, "bBrakes", "6"));
            Assert.Equal(string.Empty, Commit(vm, "bSlope", "0.75"));

            var front = vm.Draft.Matrix.Fronts[0];
            Assert.Equal((Roller25, 4.0, 6, 0.75), (front.RollerId, front.RollerPitch, front.BrakeEveryRollers, front.SlopePerFoot));
            Assert.Null(vm.Draft.Matrix.Fronts[1].RollerId);
            Assert.True(vm.IsDirty);
            Assert.Equal(0.75, vm.Draft.Design.Fronts[0].SlopePerFoot);

            // The default roller is written as nothing, and empty fields give the front back its bed as before.
            Assert.Equal(string.Empty, Commit(vm, "bPitch", ""));
            Assert.Equal(string.Empty, Commit(vm, "bRoller", "1.9"));
            Assert.Null(vm.Draft.Matrix.Fronts[0].RollerId);
        }

        [Fact]
        public void ABedThatCannotBeBuiltIsRefusedInItsField()
        {
            var vm = Editor();
            vm.SelectFront(0);
            var before = Json(vm.Draft.Design);

            Assert.Equal("El rodillo debe ser uno del catálogo: 1.9 o 2.5 in.", Commit(vm, "bRoller", "3"));
            Assert.Equal("El paso no puede ser menor que el rodillo de F1: mínimo 2 in.", Commit(vm, "bPitch", "1.5"));
            Assert.Equal("Debe ser un entero mayor o igual que 1.", Commit(vm, "bBrakes", "0"));
            Assert.Equal("La pendiente debe ser un número mayor o igual que cero.", Commit(vm, "bSlope", "-1"));

            Assert.Equal(string.Empty, Commit(vm, "bPitch", "2"));
            Assert.StartsWith("El paso de F1 (2 in) es menor que este rodillo", Commit(vm, "bRoller", "2.5"));
            Assert.False(vm.Draft.Matrix.Fronts[0].RollerId == Roller25);
            Assert.NotEqual(before, Json(vm.Draft.Design));
        }

        [Fact]
        public void TwoFrontsWithDifferentBedsSayVaries()
        {
            var vm = Editor();
            vm.SelectFront(0);
            Commit(vm, "bRoller", "2.5");
            vm.SelectAll();

            Assert.Equal(("", "Varía"), (Field(vm, "bRoller").Text, Field(vm, "bRoller").Hint));
            Assert.StartsWith("Frentes de la selección: F1, F2.", vm.BedScopeText);
        }
    }
}
