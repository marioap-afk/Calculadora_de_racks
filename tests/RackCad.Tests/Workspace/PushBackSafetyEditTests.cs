using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Application.Bom;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.PushBack;
using RackCad.Application.Systems.Selective;
using RackCad.Application.Systems.Shared;
using RackCad.Application.VentanaRackCad;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// T5 pb, PB-S03: the safety of Push Back post by post in the RackCad window writes what the classic dialog «Elementos de seguridad» of
    /// Push Back writes for the same choice. Each test takes TWO drafts of the same rack: the window edits one with
    /// <see cref="PushBackSafetyEdit"/> (a copy, then <see cref="PushBackDraftEditor.ReplaceSafety"/>), and <see cref="Classic"/> (a
    /// transcription of the classic code, independent of PushBackSafetyEdit) writes the other one as its «Aceptar» does. The two designs must
    /// be the same, once the list of safety is read in one order (the classic «Aceptar» leaves the families of the dialog first, then the
    /// defense, then the botas: declared), and they must differ from the design before (no empty test).
    /// </summary>
    public sealed class PushBackSafetyEditTests
    {
        private static readonly RackCad.Application.Catalogs.RackCatalog Catalog = PilotKit.Catalog;

        private static PushBackDesign DesignOf(string fixture)
            => new RackProjectStore().Deserialize(new RackEmbedStore().Deserialize(PilotKit.Text(fixture)).Design).PushBackDesign;

        // A new rack of one way with three fronts (four posts), with the safety the classic editor seeds.
        private static PushBackDraftEditor OneWay()
        {
            var draft = PushBackDraftEditor.CreateNew(Catalog);
            draft.SetSlotCount(3);
            Assert.True(draft.Recompose(), draft.LastError);
            return draft;
        }

        // A new rack of two sides with two slots, both sides present.
        private static PushBackDraftEditor TwoSided()
        {
            var draft = PushBackDraftEditor.CreateNew(Catalog);
            draft.SetSlotCount(2);
            draft.SetSideBPresent(true);
            for (var slot = 0; slot < 2; slot++) Assert.Null(draft.SetBlank(PushBackSide.B, slot, false));
            Assert.True(draft.Recompose(), draft.LastError);
            Assert.True(draft.IsTwoSided);
            return draft;
        }

        private static PushBackDraftEditor Loaded(string fixture)
        {
            var draft = new PushBackDraftEditor(Catalog);
            Assert.True(draft.LoadFrom(DesignOf(fixture), "PB"), draft.LastError);
            return draft;
        }

        // The design of the last valid recomposition, serialized, with its list of safety in one order (by element id). Only the design: the
        // editable state is not compared (what it keeps is what the design writes, once it recomposes).
        private static string Canonical(PushBackDraftEditor draft)
        {
            var store = new RackProjectStore();
            var copy = store.Deserialize(store.Serialize(RackProject.ForPushBack(draft.Design))).PushBackDesign;
            var sorted = copy.Structure.SafetySelections.OrderBy(s => s?.ElementId, StringComparer.Ordinal).ToList();
            copy.Structure.SafetySelections.Clear();
            foreach (var selection in sorted) copy.Structure.SafetySelections.Add(selection);
            return store.Serialize(RackProject.ForPushBack(copy));
        }

        private static IEnumerable<(string, double, int)> SafetyBom(PushBackDraftEditor draft)
            => draft.Computation.Bom.Components
                .Where(c => c != null && string.Equals(c.Category, SelectiveBomBuilder.Safety, StringComparison.Ordinal))
                .Select(c => (c.ProfileId, Math.Round(c.Length, 3), c.Quantity))
                .OrderBy(c => c.ProfileId, StringComparer.Ordinal).ThenBy(c => c.Item2);

        // The window edits W, the classic writes C; then both recompose and must give the same design and the same BOM of safety.
        private static void Same(PushBackDraftEditor window, PushBackDraftEditor classic, Action<PushBackSafetyCopy> windowEdit, Action<PushBackDraftEditor> classicEdit,
            bool changes = true)
        {
            var before = Canonical(window);
            Assert.Equal(before, Canonical(classic));
            var copy = window.CopySafety();
            windowEdit(copy);
            window.ReplaceSafety(copy);
            Assert.True(window.Recompose(), window.LastError);
            classicEdit(classic);
            Assert.True(classic.Recompose(), classic.LastError);
            Assert.Equal(Canonical(classic), Canonical(window));
            Assert.Equal(SafetyBom(classic), SafetyBom(window));
            if (changes) Assert.NotEqual(before, Canonical(window));
        }

        private static (PushBackDraftEditor, PushBackDraftEditor) Pair(Func<PushBackDraftEditor> make) => (make(), make());

        // ================================================================== the families of Push Back

        [Fact]
        public void PushBackEditsTheFamiliesItsAuthorityAdmits()
        {
            Assert.Equal(new[] { SelectiveSafetyDefaults.BotaType, SelectiveSafetyDefaults.LateralType, SelectiveSafetyDefaults.DesviadorType, SelectiveSafetyDefaults.DefensaType },
                RackCadSafetyEdit.FamiliesOf(RackCadSafetySystem.PushBack));
        }

        // ================================================================== botas

        [Fact]
        public void TheBotaOfAPostIsWrittenAsTheSectionAndItsPerPostGrid()
        {
            var (w, c) = Pair(OneWay);
            Same(w, c, copy => Assert.True(PushBackSafetyEdit.SetBootPost(copy, Catalog, PushBackSide.A, 1, BootPlacement.Rear)),
                d => Classic.Boots(d, PushBackSide.A, s => Classic.PerPost(s, 1, BootPlacement.Rear)));
            Assert.Equal(BootPlacement.Rear, PushBackSafetyEdit.BootSide(w.CopySafety(), Catalog, PushBackSide.A).OwnAt(1));
            var bota = PushBackSafetyEdit.Of(w.State.SafetySelections, Catalog, SelectiveSafetyDefaults.BotaType);
            Assert.True(bota.BootSidesDeclared);
            Assert.Equal(SafetySide.None, bota.Side);

            // Back to the general: the post leaves the grid.
            Same(w, c, copy => PushBackSafetyEdit.SetBootPost(copy, Catalog, PushBackSide.A, 1, null),
                d => Classic.Boots(d, PushBackSide.A, s => Classic.PerPost(s, 1, null)));
        }

        [Fact]
        public void TheGeneralOfTheBotasIsChosenOnlyWhenItChangesAsItsList()
        {
            var (w, c) = Pair(OneWay);

            // The automatic one shown («Entrada/Salida») chosen again changes nothing in the classic list; the window writes the same.
            Same(w, c, copy => PushBackSafetyEdit.SetBootGeneral(copy, Catalog, PushBackSide.A, BootPlacement.EntryExit),
                d => Classic.Boots(d, PushBackSide.A, s => Classic.General(s, BootPlacement.EntryExit)), changes: false);
            Same(w, c, copy => PushBackSafetyEdit.SetBootGeneral(copy, Catalog, PushBackSide.A, BootPlacement.Both),
                d => Classic.Boots(d, PushBackSide.A, s => Classic.General(s, BootPlacement.Both)));
            Same(w, c, copy => PushBackSafetyEdit.SetBootPost(copy, Catalog, PushBackSide.A, 0, BootPlacement.None),
                d => Classic.Boots(d, PushBackSide.A, s => Classic.PerPost(s, 0, BootPlacement.None)));
            Same(w, c, copy => PushBackSafetyEdit.BootPostsToGeneral(copy, Catalog, PushBackSide.A),
                d => Classic.Boots(d, PushBackSide.A, s => s.Posts.Clear()));
        }

        [Fact]
        public void TheTypeNoneOfTheBotasKeepsTheIntentDormantAndAPieceBringsItBack()
        {
            var (w, c) = Pair(OneWay);
            Same(w, c, copy => PushBackSafetyEdit.SetBootPost(copy, Catalog, PushBackSide.A, 2, BootPlacement.Both),
                d => Classic.Boots(d, PushBackSide.A, s => Classic.PerPost(s, 2, BootPlacement.Both)));
            Same(w, c, copy => PushBackSafetyEdit.SetBootPiece(copy, Catalog, PushBackSide.A, PushBackDefaults.NonePieceId),
                d => Classic.Boots(d, PushBackSide.A, s => s.PieceId = PushBackDefaults.NonePieceId));
            Assert.Empty(PushBackBootPlan.Resolve(w.System, Catalog));
            Assert.Equal(BootPlacement.Both, PushBackSafetyEdit.BootSide(w.CopySafety(), Catalog, PushBackSide.A).OwnAt(2)); // dormant

            var piece = PushBackSafetyEdit.Variants(Catalog, SelectiveSafetyDefaults.BotaType).First().Id;
            Same(w, c, copy => PushBackSafetyEdit.SetBootPiece(copy, Catalog, PushBackSide.A, piece),
                d => Classic.Boots(d, PushBackSide.A, s => s.PieceId = Classic.Combo(SelectiveSafetyDefaults.BotaType, piece)));
            Assert.NotEmpty(PushBackBootPlan.Resolve(w.System, Catalog));
        }

        [Fact]
        public void ABotaReadWithItsHistoricMatrixIsWrittenAsTheClassicSectionMigratesIt()
        {
            var (w, c) = Pair(() => Loaded("pb-06-seguridad"));
            Same(w, c, copy => PushBackSafetyEdit.SetBootPost(copy, Catalog, PushBackSide.A, 2, BootPlacement.None),
                d => Classic.Boots(d, PushBackSide.A, s => Classic.PerPost(s, 2, BootPlacement.None)));
        }

        [Fact]
        public void EachSideOfATwoSidedRackWritesItsOwnBotasAndKeepsTheOther()
        {
            var (w, c) = Pair(TwoSided);
            Same(w, c, copy => PushBackSafetyEdit.SetBootPost(copy, Catalog, PushBackSide.B, 0, BootPlacement.Rear),
                d => Classic.Boots(d, PushBackSide.B, s => Classic.PerPost(s, 0, BootPlacement.Rear)));
            Same(w, c, copy => PushBackSafetyEdit.SetBootGeneral(copy, Catalog, PushBackSide.A, BootPlacement.None),
                d => Classic.Boots(d, PushBackSide.A, s => Classic.General(s, BootPlacement.None)));
            var copyNow = w.CopySafety();
            Assert.Equal(BootPlacement.Rear, PushBackSafetyEdit.BootSide(copyNow, Catalog, PushBackSide.B).OwnAt(0));
            Assert.Equal(BootPlacement.None, PushBackSafetyEdit.BootSide(copyNow, Catalog, PushBackSide.A).Placement);
            Assert.Contains(PushBackBootPlan.Resolve(w.System, Catalog), b => b.Side == PushBackSide.B);
            Assert.DoesNotContain(PushBackBootPlan.Resolve(w.System, Catalog), b => b.Side == PushBackSide.A);

            Same(w, c, copy => PushBackSafetyEdit.SetBootPiece(copy, Catalog, PushBackSide.B, PushBackDefaults.NonePieceId),
                d => Classic.Boots(d, PushBackSide.B, s => s.PieceId = PushBackDefaults.NonePieceId));
            Assert.Throws<InvalidOperationException>(() => PushBackSafetyEdit.SetBootPost(OneWay().CopySafety(), Catalog, PushBackSide.B, 0, BootPlacement.Rear));
        }

        // ================================================================== defensa

        [Fact]
        public void TheEndsOfTheDefenseOfARackOfOneWayAreWrittenAsItsGrid()
        {
            var (w, c) = Pair(OneWay);

            // «Posterior» is off by default (PB-009): its box proposes the length of the rule, of its own.
            Same(w, c, copy => PushBackSafetyEdit.SetDefenseOn(copy, Catalog, PushBackSide.A, 1, farEnd: true, on: true),
                d => Classic.Defense(d, PushBackSide.A, rows => Classic.Check(rows[1], farEnd: true, on: true)));
            var record = PushBackSafetyEdit.Of(w.State.SafetySelections, Catalog, SelectiveSafetyDefaults.DefensaType).DefensaPosts.Single(r => r.PostIndex == 1);
            Assert.False(record.EntranceAuto);
            Assert.Equal(DynamicForkliftDefensePlan.IntermediateLength, record.EntranceLength);

            Same(w, c, copy => PushBackSafetyEdit.SetDefenseLength(copy, Catalog, PushBackSide.A, 0, farEnd: false, length: 20.0),
                d => Classic.Defense(d, PushBackSide.A, rows => Classic.Type(rows[0], farEnd: false, 20.0)));
            Same(w, c, copy => PushBackSafetyEdit.SetDefenseOn(copy, Catalog, PushBackSide.A, 3, farEnd: false, on: false),
                d => Classic.Defense(d, PushBackSide.A, rows => Classic.Check(rows[3], farEnd: false, on: false)));
            Same(w, c, copy => PushBackSafetyEdit.SetDefenseLength(copy, Catalog, PushBackSide.A, 0, farEnd: false, length: null),
                d => Classic.Defense(d, PushBackSide.A, rows => Classic.Auto(rows[0], farEnd: false)));
            Same(w, c, copy => PushBackSafetyEdit.DefenseAllAutomatic(copy, Catalog, PushBackSide.A),
                d => Classic.Defense(d, PushBackSide.A, rows => { foreach (var row in rows) { Classic.Auto(row, false); Classic.Auto(row, true); } }));
            Assert.Empty(PushBackSafetyEdit.Of(w.State.SafetySelections, Catalog, SelectiveSafetyDefaults.DefensaType).DefensaPosts);
        }

        [Fact]
        public void TheTypeNoneOfTheDefenseGoesToTheStateOfTheSideAndKeepsThePosts()
        {
            var (w, c) = Pair(OneWay);
            Same(w, c, copy => PushBackSafetyEdit.SetDefenseOn(copy, Catalog, PushBackSide.A, 2, farEnd: false, on: false),
                d => Classic.Defense(d, PushBackSide.A, rows => Classic.Check(rows[2], farEnd: false, on: false)));
            Same(w, c, copy => PushBackSafetyEdit.SetDefensePiece(copy, Catalog, PushBackSide.A, PushBackDefaults.NonePieceId),
                d => Classic.Defense(d, PushBackSide.A, null, PushBackDefaults.NonePieceId));
            Assert.Equal(PushBackDefaults.NonePieceId, w.Side(PushBackSide.A).DefensePieceId);
            Assert.Empty(PushBackDefensePlan.Resolve(w.System, Catalog));
            Assert.Single(PushBackSafetyEdit.Of(w.State.SafetySelections, Catalog, SelectiveSafetyDefaults.DefensaType).DefensaPosts); // the carrier keeps the intent
            Assert.True(PushBackSafetyEdit.DefenseHoldsPosts(w.CopySafety(), Catalog));
        }

        [Fact]
        public void WithNoTypeOfDefenseAndNothingByPostADecisionOfAPostHasNowhereToGoAsInTheClassic()
        {
            // Review of 01e76902, point 3: every end automatic and the type «Ninguno»: the family goes (nothing by post to carry), and a post
            // decided then is written nowhere, in the window as in the classic «Aceptar» (no carrier). The table disables its boxes.
            var (w, c) = Pair(OneWay);
            Assert.True(PushBackSafetyEdit.DefenseHoldsPosts(w.CopySafety(), Catalog));
            Same(w, c, copy => PushBackSafetyEdit.SetDefensePiece(copy, Catalog, PushBackSide.A, PushBackDefaults.NonePieceId),
                d => Classic.Defense(d, PushBackSide.A, null, PushBackDefaults.NonePieceId));
            Assert.Null(PushBackSafetyEdit.Of(w.State.SafetySelections, Catalog, SelectiveSafetyDefaults.DefensaType));
            Assert.False(PushBackSafetyEdit.DefenseHoldsPosts(w.CopySafety(), Catalog));

            Same(w, c, copy => PushBackSafetyEdit.SetDefenseOn(copy, Catalog, PushBackSide.A, 1, farEnd: false, on: false),
                d => Classic.Defense(d, PushBackSide.A, rows => Classic.Check(rows[1], farEnd: false, on: false)), changes: false);
            Assert.Null(PushBackSafetyEdit.Of(w.State.SafetySelections, Catalog, SelectiveSafetyDefaults.DefensaType));

            // A type again: the automatic ones come back, and a post can be decided again.
            var piece = PushBackSafetyEdit.Variants(Catalog, SelectiveSafetyDefaults.DefensaType).First().Id;
            Same(w, c, copy => PushBackSafetyEdit.SetDefensePiece(copy, Catalog, PushBackSide.A, piece),
                d => Classic.Defense(d, PushBackSide.A, null, Classic.Combo(SelectiveSafetyDefaults.DefensaType, piece)));
            Assert.True(PushBackSafetyEdit.DefenseHoldsPosts(w.CopySafety(), Catalog));
            Assert.NotEmpty(PushBackDefensePlan.Resolve(w.System, Catalog));
        }

        [Fact]
        public void EachSideOfATwoSidedRackWritesItsOwnFaceOfTheDefense()
        {
            var (w, c) = Pair(TwoSided);
            Same(w, c, copy => PushBackSafetyEdit.SetDefenseOn(copy, Catalog, PushBackSide.B, 1, farEnd: true, on: false),
                d => Classic.Defense(d, PushBackSide.B, rows => Classic.Check(rows[1], farEnd: true, on: false)));
            Same(w, c, copy => PushBackSafetyEdit.SetDefenseLength(copy, Catalog, PushBackSide.A, 0, farEnd: false, length: 30.0),
                d => Classic.Defense(d, PushBackSide.A, rows => Classic.Type(rows[0], farEnd: false, 30.0)));
            var records = PushBackSafetyEdit.Of(w.State.SafetySelections, Catalog, SelectiveSafetyDefaults.DefensaType).DefensaPosts;
            Assert.True(records.Single(r => r.PostIndex == 1).ExitAuto); // A did not change at the line B wrote
            Assert.True(records.Single(r => r.PostIndex == 0).EntranceAuto);
            Same(w, c, copy => PushBackSafetyEdit.SetDefensePiece(copy, Catalog, PushBackSide.B, PushBackDefaults.NonePieceId),
                d => Classic.Defense(d, PushBackSide.B, null, PushBackDefaults.NonePieceId));
            Assert.All(PushBackDefensePlan.Resolve(w.System, Catalog), d => Assert.Equal(PushBackSide.A, d.Side));
        }

        [Fact]
        public void BackToAutoOnAFaceWhoseOtherFaceIsAutomaticIsWrittenAsTheCorrectedClassicGrid()
        {
            // T6 pb (PB-S17, point 5): the face grid of the classic dropped the row that became automatic at both ends before the section
            // merged it, so the classic kept the length. Corrected in the classic (SafetyDefensaGridWindow.BuildFaceResult): the two write the
            // gesture. Measured on the same rack, with the same gestures.
            var (w, c) = Pair(TwoSided);
            Same(w, c, copy => PushBackSafetyEdit.SetDefenseLength(copy, Catalog, PushBackSide.A, 0, farEnd: false, length: 30.0),
                d => Classic.Defense(d, PushBackSide.A, rows => Classic.Type(rows[0], farEnd: false, 30.0)));
            Same(w, c, copy => PushBackSafetyEdit.SetDefenseLength(copy, Catalog, PushBackSide.A, 0, farEnd: false, length: null),
                d => Classic.Defense(d, PushBackSide.A, rows => Classic.Auto(rows[0], farEnd: false)));
            Assert.Empty(PushBackSafetyEdit.Of(w.State.SafetySelections, Catalog, SelectiveSafetyDefaults.DefensaType).DefensaPosts);

            // The same on the far face (side B), and «Todas automáticas» of a side: every «Auto» of its grid marked.
            Same(w, c, copy => PushBackSafetyEdit.SetDefenseOn(copy, Catalog, PushBackSide.B, 2, farEnd: true, on: false),
                d => Classic.Defense(d, PushBackSide.B, rows => Classic.Check(rows[2], farEnd: true, on: false)));
            Same(w, c, copy => PushBackSafetyEdit.SetDefenseLength(copy, Catalog, PushBackSide.B, 2, farEnd: true, length: null),
                d => Classic.Defense(d, PushBackSide.B, rows => Classic.Auto(rows[2], farEnd: true)));
            Same(w, c, copy => PushBackSafetyEdit.SetDefenseOn(copy, Catalog, PushBackSide.A, 1, farEnd: false, on: false),
                d => Classic.Defense(d, PushBackSide.A, rows => Classic.Check(rows[1], farEnd: false, on: false)));
            Same(w, c, copy => PushBackSafetyEdit.SetDefenseLength(copy, Catalog, PushBackSide.A, 0, farEnd: false, length: 25.0),
                d => Classic.Defense(d, PushBackSide.A, rows => Classic.Type(rows[0], farEnd: false, 25.0)));
            Assert.Equal(2, PushBackSafetyEdit.Of(w.State.SafetySelections, Catalog, SelectiveSafetyDefaults.DefensaType).DefensaPosts.Count);
            Same(w, c, copy => PushBackSafetyEdit.DefenseAllAutomatic(copy, Catalog, PushBackSide.A),
                d => Classic.Defense(d, PushBackSide.A, rows => { foreach (var row in rows.Where(r => r.Applies)) Classic.Auto(row, farEnd: false); }));
            Assert.Empty(PushBackSafetyEdit.Of(w.State.SafetySelections, Catalog, SelectiveSafetyDefaults.DefensaType).DefensaPosts);
        }

        [Fact]
        public void WithBothSidesInNoneTheLastOwnDecisionBackToAutoTakesTheFamilyOutAsTheWindow()
        {
            // T6 pb (review of 2f1e9e4e, M3, declared in T6pb-S1): the two sides in «Ninguno», the family only carries the decision of post 2
            // of side A. «Auto» on it: the section of B still holds the record it opened with, so this «Aceptar» keeps the family with no
            // record; the next «Aceptar» has nothing by post on either side and the family goes. The same in the window; nothing is drawn.
            var (w, c) = Pair(TwoSided);
            Same(w, c, copy => PushBackSafetyEdit.SetDefenseOn(copy, Catalog, PushBackSide.A, 1, farEnd: false, on: false),
                d => Classic.Defense(d, PushBackSide.A, rows => Classic.Check(rows[1], farEnd: false, on: false)));
            Same(w, c, copy => PushBackSafetyEdit.SetDefensePiece(copy, Catalog, PushBackSide.A, PushBackDefaults.NonePieceId),
                d => Classic.Defense(d, PushBackSide.A, null, PushBackDefaults.NonePieceId));
            Same(w, c, copy => PushBackSafetyEdit.SetDefensePiece(copy, Catalog, PushBackSide.B, PushBackDefaults.NonePieceId),
                d => Classic.Defense(d, PushBackSide.B, null, PushBackDefaults.NonePieceId));
            Assert.Single(PushBackSafetyEdit.Of(c.State.SafetySelections, Catalog, SelectiveSafetyDefaults.DefensaType).DefensaPosts);
            Assert.Empty(PushBackDefensePlan.Resolve(c.System, Catalog));

            Same(w, c, copy => PushBackSafetyEdit.SetDefenseLength(copy, Catalog, PushBackSide.A, 1, farEnd: false, length: null),
                d => Classic.Defense(d, PushBackSide.A, rows => Classic.Auto(rows[1], farEnd: false)));
            Assert.Empty(PushBackSafetyEdit.Of(c.State.SafetySelections, Catalog, SelectiveSafetyDefaults.DefensaType).DefensaPosts);

            Same(w, c, copy => PushBackSafetyEdit.WriteDefense(copy, Catalog, PushBackSide.A, _ => { }), d => Classic.Defense(d, PushBackSide.A, null));
            Assert.Null(PushBackSafetyEdit.Of(c.State.SafetySelections, Catalog, SelectiveSafetyDefaults.DefensaType));
            Assert.Empty(PushBackDefensePlan.Resolve(c.System, Catalog));
            Assert.Equal(SafetyBom(c), SafetyBom(w));
        }

        [Fact]
        public void ADefenseReadFromADrawingIsWrittenAsTheClassicGridReadsItBack()
        {
            var (w, c) = Pair(() => Loaded("pb-06-seguridad"));
            Same(w, c, copy => PushBackSafetyEdit.SetDefenseOn(copy, Catalog, PushBackSide.A, 0, farEnd: true, on: true),
                d => Classic.Defense(d, PushBackSide.A, rows => Classic.Check(rows[0], farEnd: true, on: true)));
        }

        // ================================================================== protector lateral

        [Fact]
        public void AProtectorLateralOfAPostIsWrittenAsThePerPostWindowOfTheDialog()
        {
            var (w, c) = Pair(OneWay);
            var lateral = PushBackSafetyEdit.Of(w.State.SafetySelections, Catalog, SelectiveSafetyDefaults.LateralType);
            Assert.True(PushBackSafetyEdit.LateralFollowsRule(lateral));
            var left = false;
            Same(w, c, copy => left = PushBackSafetyEdit.SetLateralPost(copy.Safety, Catalog, 1, SafetySide.Left, copy.PostCount),
                d => Classic.Lateral(d, 1, SafetySide.Left));
            Assert.True(left);
            Assert.Equal(new[] { (0, SafetySide.Left), (1, SafetySide.Left), (3, SafetySide.Right) },
                PushBackSafetyEdit.Of(w.State.SafetySelections, Catalog, SelectiveSafetyDefaults.LateralType).PostSides.Select(p => (p.PostIndex, p.Side)));

            Same(w, c, copy => PushBackSafetyEdit.SetLateralPost(copy.Safety, Catalog, 0, SafetySide.None, copy.PostCount),
                d => Classic.Lateral(d, 0, SafetySide.None));
            Same(w, c, copy => PushBackSafetyEdit.LateralToRule(copy.Safety, Catalog), d => Classic.LateralToRule(d));
        }

        // T6 seg: the pieces of the BOM of safety of a family.
        private static int BomOf(PushBackDraftEditor draft, string type)
            => SafetyBom(draft).Where(line => PushBackSafetyEdit.Variants(Catalog, type).Any(v => string.Equals(v.Id, line.Item1, StringComparison.OrdinalIgnoreCase))).Sum(line => line.Item3);

        [Fact]
        public void ALateralLeftWithNoPostKeepsItsVariantAsTheClassicAndDrawsNothing()
        {
            // C2-07, as the fourth batch: every post on «No lleva» keeps the variant. T6 seg (the Owner, 2026-11-27, «Guardarla como la
            // ventana»): the classic «Aceptar» keeps it too now (it dropped the family: PB-S17, difference 3).
            var (w, c) = Pair(OneWay);
            var posts = w.CopySafety().PostCount;
            for (var post = 0; post < posts; post++)
            {
                var at = post;
                Same(w, c, copy => PushBackSafetyEdit.SetLateralPost(copy.Safety, Catalog, at, SafetySide.None, copy.PostCount), d => Classic.Lateral(d, at, SafetySide.None));
            }

            Assert.NotNull(PushBackSafetyEdit.Of(c.State.SafetySelections, Catalog, SelectiveSafetyDefaults.LateralType));
            Assert.Equal(0, BomOf(w, SelectiveSafetyDefaults.LateralType));
        }

        [Fact]
        public void ADesviadorLeftWithNoLevelKeepsItsVariantAsTheClassicAndDrawsNothing()
        {
            // C2-07 (PB-S17, difference 3). T6 seg: «Ninguno» of the classic grid and «Aceptar» keep the family too now.
            var (w, c) = Pair(OneWay);
            var levels = w.DesviadorLevelsPerPost();
            Same(w, c, copy => PushBackSafetyEdit.SetDesviadorAll(copy.Safety, Catalog, levels, on: false),
                d => Classic.Desviador(d, off =>
                {
                    for (var post = 0; post < levels.Count; post++)
                        for (var level = 0; level < levels[post]; level++) off.Add((post, level));
                }));
            Assert.NotNull(PushBackSafetyEdit.Of(c.State.SafetySelections, Catalog, SelectiveSafetyDefaults.DesviadorType));
            Assert.Empty(PushBackDiverterPlan.Resolve(w.System, Catalog));
            Assert.Equal(0, BomOf(w, SelectiveSafetyDefaults.DesviadorType));
        }

        // ================================================================== desviador

        [Fact]
        public void TheLevelsAndTheMeasuresOfTheDesviadorAreWrittenAsItsGrid()
        {
            var (w, c) = Pair(OneWay);
            var levels = w.DesviadorLevelsPerPost();
            Same(w, c, copy => PushBackSafetyEdit.SetDesviador(copy.Safety, Catalog, levels, 1, 0, on: false),
                d => Classic.Desviador(d, off => off.Add((1, 0))));
            Same(w, c, copy => PushBackSafetyEdit.SetDesviador(copy.Safety, Catalog, levels, 2, 1, on: false),
                d => Classic.Desviador(d, off => off.Add((2, 1))));
            Same(w, c, copy => PushBackSafetyEdit.SetDesviadorMeasure(copy.Safety, Catalog, levels, s => s.DesviadorLongitud = 20.0),
                d => Classic.Desviador(d, null, longitud: 20.0));
            Same(w, c, copy => PushBackSafetyEdit.SetDesviador(copy.Safety, Catalog, levels, 1, 0, on: true),
                d => Classic.Desviador(d, off => off.Remove((1, 0))));
            Assert.Equal(PushBackDiverterPlan.Resolve(c.System, Catalog).Count, PushBackDiverterPlan.Resolve(w.System, Catalog).Count);
        }

        [Fact]
        public void AMeasureOfTheDesviadorThatIsNotValidIsTheDefaultOneThroughItsGrid()
        {
            // As the grid shows it (Effective): a longitud or a first level that is not an even number above 8 is the default 18 in.
            var (w, c) = Pair(OneWay);
            foreach (var draft in new[] { w, c })
            {
                var desviador = PushBackSafetyEdit.Of(draft.State.SafetySelections, Catalog, SelectiveSafetyDefaults.DesviadorType);
                desviador.DesviadorLongitud = 7.0;
                desviador.DesviadorPrimerNivelAltura = 9.0;
                Assert.True(draft.Recompose(), draft.LastError);
            }

            Same(w, c, copy => PushBackSafetyEdit.SetDesviador(copy.Safety, Catalog, w.DesviadorLevelsPerPost(), 1, 0, on: false),
                d => Classic.Desviador(d, off => off.Add((1, 0))));
            var written = PushBackSafetyEdit.Of(w.State.SafetySelections, Catalog, SelectiveSafetyDefaults.DesviadorType);
            Assert.Equal((SelectiveSafetyDefaults.DesviadorLongitud, SelectiveSafetyDefaults.DesviadorPrimerNivelAltura),
                (written.DesviadorLongitud, written.DesviadorPrimerNivelAltura));
        }

        [Fact]
        public void TheDesviadorOfATwoSidedRackIsWrittenByPost()
        {
            var (w, c) = Pair(TwoSided);
            var levels = w.DesviadorLevelsPerPost();
            Same(w, c, copy => PushBackSafetyEdit.SetDesviador(copy.Safety, Catalog, levels, 2, 0, on: false),
                d => Classic.Desviador(d, off => off.Add((2, 0))));
        }

        [Fact]
        public void TheDesviadorOfATwoSidedRackWithMoreLevelsOnSideBIsWrittenAsTheCorrectedClassicFromSideA()
        {
            // T6 pb (PB-S17, point 4): front 2 of side B has a fourth level, side A three, and the classic is on side A. The drawing places a
            // desviador at N4 of posts 2 and 3 (the beds of B); the corrected classic grid has those levels from either side, as the window.
            var (w, c) = Pair(TwoSided);
            foreach (var draft in new[] { w, c })
            {
                draft.Side(PushBackSide.B).AdjustLevels(1, 1);
                Assert.True(draft.Recompose(), draft.LastError);
                Assert.Equal(PushBackSide.A, draft.Composite.ActiveSide);
            }

            var levels = w.DesviadorLevelsPerPost();
            Assert.Equal(new[] { 3, 4, 4 }, levels);
            Assert.Contains(PushBackDiverterPlan.Resolve(w.System, Catalog), d => d.PostLine == 1 && d.Level == 4);
            Same(w, c, copy => PushBackSafetyEdit.SetDesviador(copy.Safety, Catalog, levels, 1, 3, on: false),
                d => Classic.Desviador(d, off => off.Add((1, 3))));
            Assert.DoesNotContain(PushBackDiverterPlan.Resolve(c.System, Catalog), d => d.PostLine == 1 && d.Level == 4);

            // Another cell from side A: the one at N4 stays off (the classic grid of side A dropped it on «Aceptar»).
            Same(w, c, copy => PushBackSafetyEdit.SetDesviador(copy.Safety, Catalog, levels, 0, 0, on: false),
                d => Classic.Desviador(d, off => off.Add((0, 0))));
            Assert.Contains(PushBackSafetyEdit.Of(c.State.SafetySelections, Catalog, SelectiveSafetyDefaults.DesviadorType).DesviadorOffCells,
                cell => cell.Frente == 1 && cell.Level == 3);
        }

        [Fact]
        public void AColumnDormantOnTheActiveSideTakesTheLevelsOfTheOtherSideAndDropsAnOffCellAboveThem()
        {
            // T6 pb (review of 2f1e9e4e, M2, declared in T6pb-S2). Front 1 of side A has four levels and the desviador of N4 of post 1 is off.
            // Front 1 of A goes blank: on side A post 1 has no level (a dormant column), on side B it has three. The grid from side A has the
            // three of B there, so that column is no longer dormant and the off cell at N4 (above them) goes on «Aceptar», in the window as in
            // the corrected classic (the classic of before kept it from side A, and dropped it from side B). Nothing changes in the drawing
            // while the front is blank; when it comes back, the desviador of N4 of post 1 is drawn again.
            var (w, c) = Pair(TwoSided);
            foreach (var draft in new[] { w, c })
            {
                draft.SetLevels(new[] { 0 }, 4);
                Assert.True(draft.Recompose(), draft.LastError);
            }

            Assert.Equal(new[] { 4, 4, 3 }, w.DesviadorLevelsPerPost());
            Same(w, c, copy => PushBackSafetyEdit.SetDesviador(copy.Safety, Catalog, w.DesviadorLevelsPerPost(), 0, 3, on: false),
                d => Classic.Desviador(d, off => off.Add((0, 3))));
            foreach (var draft in new[] { w, c })
            {
                Assert.Null(draft.SetBlank(PushBackSide.A, 0, true));
                Assert.True(draft.Recompose(), draft.LastError);
            }

            Assert.Equal(3, w.DesviadorLevelsPerPost()[0]);
            Assert.Contains(PushBackSafetyEdit.Of(c.State.SafetySelections, Catalog, SelectiveSafetyDefaults.DesviadorType).DesviadorOffCells,
                cell => cell.Frente == 0 && cell.Level == 3);
            Same(w, c, copy => PushBackSafetyEdit.SetDesviador(copy.Safety, Catalog, w.DesviadorLevelsPerPost(), 2, 0, on: false),
                d => Classic.Desviador(d, off => off.Add((2, 0))));
            Assert.DoesNotContain(PushBackSafetyEdit.Of(c.State.SafetySelections, Catalog, SelectiveSafetyDefaults.DesviadorType).DesviadorOffCells,
                cell => cell.Frente == 0 && cell.Level == 3);

            foreach (var draft in new[] { w, c })
            {
                Assert.Null(draft.SetBlank(PushBackSide.A, 0, false));
                Assert.True(draft.Recompose(), draft.LastError);
                Assert.Contains(PushBackDiverterPlan.Resolve(draft.System, Catalog), d => d.PostLine == 0 && d.Level == 4);
            }
        }

        [Fact]
        public void ANewDesviadorComesAsItsGridAcceptsItOnTheSideOfTheLowEnd()
        {
            var (w, c) = Pair(OneWay);
            var id = PushBackSafetyEdit.Variants(Catalog, SelectiveSafetyDefaults.DesviadorType).First().Id;
            Same(w, c, copy => RackCadSafetyEdit.Remove(copy.Safety, Catalog, SelectiveSafetyDefaults.DesviadorType),
                d => RackCadSafetyEdit.Remove(d.State.SafetySelections, Catalog, SelectiveSafetyDefaults.DesviadorType));
            Same(w, c, copy => PushBackSafetyEdit.SetVariant(copy.Safety, Catalog, SelectiveSafetyDefaults.DesviadorType, id, copy.PostCount, w.DesviadorLevelsPerPost()),
                d => Classic.NewDesviador(d, id));
            var desviador = PushBackSafetyEdit.Of(w.State.SafetySelections, Catalog, SelectiveSafetyDefaults.DesviadorType);
            Assert.Equal(SelectiveSafetyDefaults.DesviadorLongitud, desviador.DesviadorLongitud);
            Assert.Equal(SelectiveSafetyDefaults.DesviadorPrimerNivelAltura, desviador.DesviadorPrimerNivelAltura);

            // Review of 01e76902, point 2: the grid of Push Back has no side to choose and returns the one of the low end, so the design
            // keeps Left as the side the user wrote (AuthoredSide), as after the classic «Configurar…» and «Aceptar».
            Assert.Equal(SafetySide.Left, desviador.Side);
            var designed = PushBackSafetyEdit.Of(w.Design.Structure.SafetySelections, Catalog, SelectiveSafetyDefaults.DesviadorType);
            Assert.Equal((SafetySide.Left, (SafetySide?)SafetySide.Left), (designed.Side, designed.AuthoredSide));

            // T6 seg (the Owner, 2026-11-27, «Guardarla como la ventana»): the variant chosen and «Aceptar» WITHOUT «Configurar…» writes the
            // same; before, the classic dropped the new desviador (PB-S17, difference 6).
            var (w2, c2) = Pair(OneWay);
            Same(w2, c2, copy => RackCadSafetyEdit.Remove(copy.Safety, Catalog, SelectiveSafetyDefaults.DesviadorType),
                d => RackCadSafetyEdit.Remove(d.State.SafetySelections, Catalog, SelectiveSafetyDefaults.DesviadorType));
            Same(w2, c2, copy => PushBackSafetyEdit.SetVariant(copy.Safety, Catalog, SelectiveSafetyDefaults.DesviadorType, id, copy.PostCount, w2.DesviadorLevelsPerPost()),
                d => Classic.NewDesviadorWithoutGrid(d, id));
            Assert.Equal(Canonical(w), Canonical(w2));
        }

        [Fact]
        public void AVariantChangeOfADesviadorWritesItsRowAsTheClassicAcceptWithoutItsGrid()
        {
            // Review of 01e76902, point 1: the list of the row changed and «Aceptar» without «Configurar…». The classic writes the row as it
            // holds it, so a cell off above the levels its post has now, and a measure that is not valid, stay; only the variant changes.
            var (w, c) = Pair(OneWay);
            var first = PushBackSafetyEdit.Of(w.State.SafetySelections, Catalog, SelectiveSafetyDefaults.DesviadorType).ElementId;
            var other = PushBackSafetyEdit.Variants(Catalog, SelectiveSafetyDefaults.DesviadorType)
                .First(v => !string.Equals(v.Id, first, StringComparison.OrdinalIgnoreCase)).Id;

            // On the desviador the rack is born with (it never went through the grid): the classic writes a new row, so the side the user
            // wrote is the one of the row now, as after any «Aceptar»; mutating only the id of the selection would keep the one it had.
            Same(w, c, copy => PushBackSafetyEdit.SetVariant(copy.Safety, Catalog, SelectiveSafetyDefaults.DesviadorType, other, copy.PostCount, w.DesviadorLevelsPerPost()),
                d => Classic.DesviadorVariant(d, other));
            (first, other) = (other, first);

            foreach (var draft in new[] { w, c })
            {
                draft.SetLevels(new[] { 0, 1 }, 4);
                Assert.True(draft.Recompose(), draft.LastError);
            }

            Assert.Equal(4, w.DesviadorLevelsPerPost()[1]);
            Same(w, c, copy => PushBackSafetyEdit.SetDesviador(copy.Safety, Catalog, w.DesviadorLevelsPerPost(), 1, 3, on: false),
                d => Classic.Desviador(d, off => off.Add((1, 3)))); // N4 of P2 off
            foreach (var draft in new[] { w, c })
            {
                draft.SetLevels(new[] { 0, 1 }, 3); // P2 has three levels now: its N4 is beyond them
                Assert.True(draft.Recompose(), draft.LastError);
            }

            Assert.Equal(3, w.DesviadorLevelsPerPost()[1]);
            Same(w, c, copy => PushBackSafetyEdit.SetVariant(copy.Safety, Catalog, SelectiveSafetyDefaults.DesviadorType, other, copy.PostCount, w.DesviadorLevelsPerPost()),
                d => Classic.DesviadorVariant(d, other));
            var changed = PushBackSafetyEdit.Of(w.State.SafetySelections, Catalog, SelectiveSafetyDefaults.DesviadorType);
            Assert.Equal(other, changed.ElementId);
            Assert.Contains(changed.DesviadorOffCells, cell => cell.Frente == 1 && cell.Level == 3);

            // A measure that is not valid is not the default one here: the grid did not open.
            foreach (var draft in new[] { w, c })
            {
                PushBackSafetyEdit.Of(draft.State.SafetySelections, Catalog, SelectiveSafetyDefaults.DesviadorType).DesviadorLongitud = 7.0;
                Assert.True(draft.Recompose(), draft.LastError);
            }

            Same(w, c, copy => PushBackSafetyEdit.SetVariant(copy.Safety, Catalog, SelectiveSafetyDefaults.DesviadorType, first, copy.PostCount, w.DesviadorLevelsPerPost()),
                d => Classic.DesviadorVariant(d, first));
            var back = PushBackSafetyEdit.Of(w.State.SafetySelections, Catalog, SelectiveSafetyDefaults.DesviadorType);
            Assert.Equal((first, 7.0), (back.ElementId, back.DesviadorLongitud));
            Assert.Contains(back.DesviadorOffCells, cell => cell.Frente == 1 && cell.Level == 3);
        }

        // ================================================================== the rows of the table, counted as the BOM

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void TheRowsOfTheTableAreCountedAsTheBomOfPushBack(bool twoSided)
        {
            var draft = twoSided ? TwoSided() : OneWay();
            var copy = draft.CopySafety();
            PushBackSafetyEdit.SetBootPost(copy, Catalog, PushBackSide.A, 1, BootPlacement.Both);
            PushBackSafetyEdit.SetDefenseOn(copy, Catalog, twoSided ? PushBackSide.B : PushBackSide.A, 1, farEnd: true, on: true);
            draft.ReplaceSafety(copy);
            Assert.True(draft.Recompose(), draft.LastError);

            var rows = draft.SafetyRows();
            Assert.Equal(draft.SafetyPostCount, rows.Count);
            int Bom(string type) => draft.Computation.Bom.Components
                .Where(c => c != null && string.Equals(c.Category, SelectiveBomBuilder.Safety, StringComparison.Ordinal))
                .Where(c => SelectiveSafetyDefaults.IsType(Catalog.SafetyElements.First(e => string.Equals(e.Id, c.ProfileId, StringComparison.OrdinalIgnoreCase)).Type, type))
                .Sum(c => c.Quantity);
            Assert.Equal(Bom(SelectiveSafetyDefaults.BotaType), rows.Sum(r => r.Boots + r.BootsB));
            Assert.Equal(Bom(SelectiveSafetyDefaults.LateralType), rows.Sum(r => r.SideGuards));
            Assert.Equal(Bom(SelectiveSafetyDefaults.DesviadorType), rows.Sum(r => r.Deflectors));
            Assert.Equal(Bom(SelectiveSafetyDefaults.DefensaType), rows.Count(r => r.DefenseExit) + rows.Count(r => r.DefenseEntrance));
            Assert.All(rows.Where(r => r.DeflectorKey.HasValue), r => Assert.Equal(r.FramePost, r.DeflectorKey)); // PB-002: by post
            if (twoSided) Assert.True(rows.Sum(r => r.BootsB) > 0);
            Assert.True(rows[1].DefenseEntrance);
        }

        // ==================================================================
        /// <summary>
        /// The classic «Aceptar» of «Elementos de seguridad» of Push Back, transcribed from its source family by family, without
        /// PushBackSafetyEdit: <c>RackPushBackSystemWindow.Safety_Click</c> (the sections built before, the list replaced, the sections
        /// applied), <c>BuildBootSection</c>/<c>PushBackBootSection</c>/<c>ApplyBootSections</c>/<c>BootCarrierId</c>,
        /// <c>BuildDefenseSection</c>/<c>DefensePieceIdOf</c>/<c>PushBackDefenseSection.Configure</c>/<c>ApplyDefenseSections</c>/
        /// <c>EnsureDefenseSelection</c>, the grid <c>SafetyDefensaGridWindow</c> (its two-end OK and its face OK), the per-post window and the
        /// OK of <c>SelectiveSafetyWindow</c> (protector lateral, and the row of a desviador whose variant changed without its grid) and
        /// <c>SafetyDesviadorGridWindow.BuildResult</c> (with the side it returns in Push Back) on the levels of
        /// <c>RackPushBackSystemWindow.DesviadorLevelsPerPost</c>. T6 pb: it follows the classic as corrected in the sixth batch (PB-S17, points
        /// 4 and 5): the face grid keeps a row automatic at both ends for the merge, and the desviador grid of a rack of two sides has the
        /// higher of the two sides at each post.
        /// </summary>
        private static class Classic
        {
            // T6 seg (the Owner, 2026-11-27, «Guardarla como la ventana»): the OK of SelectiveSafetyWindow keeps a family left with nothing and a
            // new desviador without its grid, as the window (OkLateral, NewDesviadorWithoutGrid, DesviadorVariant and Grid below).
            private const string None = PushBackDefaults.NonePieceId;

            private static int PostCount(PushBackDraftEditor d) => Math.Max(2, d.Composite.Of(d.Composite.ActiveSide).Structure.Count + 1);

            private static SelectiveSafetySelection Of(IEnumerable<SelectiveSafetySelection> list, string type)
                => SelectiveSafetyFamilies.SelectedOfType(list, Catalog.SafetyElements, type);

            private static bool IsNone(string id) => !string.IsNullOrWhiteSpace(id) && string.Equals(id.Trim(), None, StringComparison.OrdinalIgnoreCase);

            // CatalogCombo.SetCatalogEntries with the «Ninguno» placeholder: a matching option, else the placeholder.
            public static string Combo(string type, string id)
            {
                var options = new[] { None }.Concat(SelectiveSafetyFamilies.VariantsOfType(Catalog.SafetyElements, type).Select(v => v.Id.Trim())).ToList();
                return string.IsNullOrWhiteSpace(id) ? None : options.FirstOrDefault(o => string.Equals(o, id.Trim(), StringComparison.OrdinalIgnoreCase)) ?? None;
            }

            // The dialog does not carry these families (bootFamilyInSections, includeDefensa: false): its «Aceptar» leaves them out of the list.
            private static void Drop(PushBackDraftEditor d, string type)
            {
                var ids = new HashSet<string>(SelectiveSafetyFamilies.VariantsOfType(Catalog.SafetyElements, type).Select(v => v.Id), StringComparer.OrdinalIgnoreCase);
                d.State.SafetySelections.RemoveAll(s => s != null && s.ElementId != null && ids.Contains(s.ElementId));
            }

            // ---- botas

            public sealed class BootSection
            {
                public PushBackSide Side;
                public int PostCount;
                public string PieceId;
                public bool Explicit;
                public BootPlacement Placement;
                public List<BootPostPlacement> Posts = new();
            }

            private static BootSection BuildBoot(PushBackDraftEditor d, PushBackSide side, int postCount)
            {
                var selection = Of(d.System?.Structure?.SafetySelections, SelectiveSafetyDefaults.BotaType) ?? Of(d.State.SafetySelections, SelectiveSafetyDefaults.BotaType);
                var config = selection == null ? null : side == PushBackSide.B ? selection.BotaB : selection.Bota;
                var source = config?.DeepCopy() ?? new SelectiveBotaConfig();
                var section = new BootSection { Side = side, PostCount = Math.Max(1, postCount) };
                section.Explicit = source.Placement.HasValue;
                section.Placement = source.Placement ?? source.Automatic ?? BootPlacement.EntryExit;
                section.Posts = source.Posts.Where(p => p != null && p.PostIndex >= 0 && p.PostIndex < section.PostCount)
                    .Select(p => new BootPostPlacement { PostIndex = p.PostIndex, Placement = p.Placement }).ToList();
                foreach (var legacy in side == PushBackSide.A ? selection?.PostSides ?? new List<SafetyPostSide>() : new List<SafetyPostSide>())
                {
                    if (legacy != null && legacy.PostIndex >= 0 && legacy.PostIndex < section.PostCount && !section.Posts.Any(p => p.PostIndex == legacy.PostIndex))
                        section.Posts.Add(new BootPostPlacement { PostIndex = legacy.PostIndex, Placement = BootPlacements.From(legacy.Side) });
                }

                var pieceId = selection?.BootPieceOf(config);
                section.PieceId = Combo(SelectiveSafetyDefaults.BotaType, IsNone(pieceId) ? None : pieceId);
                return section;
            }

            public static void General(BootSection section, BootPlacement placement)
            {
                if (section.Placement == placement) return; // the list does not change: SelectionChanged does not fire
                section.Placement = placement;
                section.Explicit = true;
            }

            // «Por poste…»: the grid opens on the posts of the section; its Result is the rows with a value of their own, in their order.
            public static void PerPost(BootSection section, int post, BootPlacement? placement)
            {
                var result = new List<BootPostPlacement>();
                for (var row = 0; row < section.PostCount; row++)
                {
                    var value = row == post ? placement : section.Posts.FirstOrDefault(p => p.PostIndex == row)?.Placement;
                    if (value.HasValue) result.Add(new BootPostPlacement { PostIndex = row, Placement = BootPlacements.From(BootPlacements.To(value.Value)) });
                }

                section.Posts = result;
            }

            public static void Boots(PushBackDraftEditor d, PushBackSide side, Action<BootSection> gesture)
            {
                var posts = PostCount(d);
                var a = BuildBoot(d, PushBackSide.A, posts);
                var b = d.Composite.SideBPresent ? BuildBoot(d, PushBackSide.B, posts) : null;
                gesture(side == PushBackSide.B ? b : a);

                string carrier = null;
                foreach (var section in new[] { a, b })
                {
                    if (section != null && !IsNone(section.PieceId) && !string.IsNullOrWhiteSpace(section.PieceId)) { carrier = section.PieceId; break; }
                }

                Drop(d, SelectiveSafetyDefaults.BotaType);
                carrier ??= SelectiveSafetyFamilies.VariantsOfType(Catalog.SafetyElements, SelectiveSafetyDefaults.BotaType).FirstOrDefault()?.Id;
                if (carrier == null) return;
                var selection = new SelectiveSafetySelection { ElementId = carrier, Quantity = 1, Side = SafetySide.None };
                d.State.SafetySelections.Add(selection);
                selection.Bota = Config(a);
                selection.BotaB = b != null ? Config(b) : new SelectiveBotaConfig();
                selection.BootSidesDeclared = true;
            }

            private static SelectiveBotaConfig Config(BootSection section)
            {
                var config = new SelectiveBotaConfig { PieceId = section.PieceId, Placement = section.Explicit ? section.Placement : (BootPlacement?)null };
                foreach (var post in section.Posts) config.Posts.Add(new BootPostPlacement { PostIndex = post.PostIndex, Placement = post.Placement });
                return config;
            }

            // ---- defensa

            public sealed class GridRow
            {
                public int Post;
                public bool Applies = true;
                public bool ExitCheck, EntranceCheck, ExitWasDrawn, EntranceWasDrawn;
                public bool? ExitAuto, EntranceAuto;
                public string ExitLength = string.Empty, EntranceLength = string.Empty;
                public double ExitSeed, EntranceSeed;
            }

            private static string Text(double value) => value.ToString("0.##", CultureInfo.InvariantCulture);

            private static bool Positive(string text) => double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var v) && v > 0.0;

            // The casilla: on proposes the seed when the box has no positive length; on or off takes the end out of the automatic.
            public static void Check(GridRow row, bool farEnd, bool on)
            {
                if (farEnd)
                {
                    row.EntranceAuto = row.EntranceAuto.HasValue ? false : (bool?)null;
                    if (on && !Positive(row.EntranceLength)) row.EntranceLength = Text(row.EntranceSeed);
                    row.EntranceCheck = on;
                }
                else
                {
                    row.ExitAuto = row.ExitAuto.HasValue ? false : (bool?)null;
                    if (on && !Positive(row.ExitLength)) row.ExitLength = Text(row.ExitSeed);
                    row.ExitCheck = on;
                }
            }

            // «Auto» marked.
            public static void Auto(GridRow row, bool farEnd)
            {
                if (farEnd) row.EntranceAuto = true;
                else row.ExitAuto = true;
            }

            // A length typed: the casilla on, «Auto» off, the number in the box.
            public static void Type(GridRow row, bool farEnd, double length)
            {
                if (!(farEnd ? row.EntranceCheck : row.ExitCheck)) Check(row, farEnd, true);
                if (farEnd) { row.EntranceAuto = false; row.EntranceLength = length.ToString(CultureInfo.InvariantCulture); }
                else { row.ExitAuto = false; row.ExitLength = length.ToString(CultureInfo.InvariantCulture); }
            }

            private static double Num(string text) => double.Parse(text, NumberStyles.Float, CultureInfo.InvariantCulture);

            // The two-end grid of a rack of one way (lowEndOnly, autoPerEnd) and its OK.
            private static List<SafetyPostDefense> TwoEndGrid(List<SafetyPostDefense> current, int postCount, Action<GridRow[]> gesture)
            {
                var rows = Enumerable.Range(0, postCount).Select(p =>
                {
                    var setting = DynamicForkliftDefensePlan.At(current, p, postCount, lowEndOnly: true);
                    var defaults = DynamicForkliftDefensePlan.At(null, p, postCount, lowEndOnly: true);
                    var stored = current.FirstOrDefault(r => r.PostIndex == p);
                    var seed = defaults.ExitLength > 0.0 ? defaults.ExitLength : defaults.EntranceLength > 0.0 ? defaults.EntranceLength : DynamicForkliftDefensePlan.EdgeLength;
                    return new GridRow
                    {
                        Post = p,
                        ExitCheck = setting.DrawsExit,
                        EntranceCheck = setting.DrawsEntrance,
                        ExitWasDrawn = setting.DrawsExit,
                        EntranceWasDrawn = setting.DrawsEntrance,
                        ExitLength = Text(setting.DrawsExit ? setting.ExitLength : defaults.ExitLength),
                        EntranceLength = Text(setting.DrawsEntrance ? setting.EntranceLength : defaults.EntranceLength),
                        ExitAuto = stored == null || stored.ExitAuto,
                        EntranceAuto = stored == null || stored.EntranceAuto,
                        ExitSeed = seed,
                        EntranceSeed = seed,
                    };
                }).ToArray();
                gesture?.Invoke(rows);

                var result = new List<SafetyPostDefense>();
                foreach (var row in rows)
                {
                    var exitAuto = row.ExitAuto == true && row.ExitCheck == row.ExitWasDrawn;
                    var entranceAuto = row.EntranceAuto == true && row.EntranceCheck == row.EntranceWasDrawn;
                    if (exitAuto && entranceAuto) continue;
                    var exit = !exitAuto && row.ExitCheck ? Num(row.ExitLength) : 0.0;
                    var entrance = !entranceAuto && row.EntranceCheck ? Num(row.EntranceLength) : 0.0;
                    Assert.True(exitAuto || !row.ExitCheck || exit > 0.0);
                    Assert.True(entranceAuto || !row.EntranceCheck || entrance > 0.0);
                    result.Add(new SafetyPostDefense
                    {
                        PostIndex = row.Post,
                        ExitLength = exitAuto ? 0.0 : exit,
                        EntranceLength = entranceAuto ? 0.0 : entrance,
                        ExitAuto = exitAuto,
                        EntranceAuto = entranceAuto,
                    });
                }

                return result;
            }

            // The grid of ONE face (a composite) and its OK: only that end, the rows where the side has its face.
            private static List<SafetyPostDefense> FaceGrid(List<SafetyPostDefense> incoming, int postCount, IReadOnlyList<bool> faces, bool farEnd, Action<GridRow[]> gesture)
            {
                bool AppliesAt(int p) => p < 0 || p >= faces.Count || faces[p];
                var rows = Enumerable.Range(0, postCount).Select(p =>
                {
                    var applies = AppliesAt(p);
                    var stored = incoming.FirstOrDefault(r => r.PostIndex == p);
                    var setting = DynamicForkliftDefensePlan.At(incoming, p, postCount, lowEndOnly: true, secondLoadFace: applies);
                    var defaults = DynamicForkliftDefensePlan.At(null, p, postCount, lowEndOnly: true, secondLoadFace: applies);
                    var drawn = farEnd ? setting.DrawsEntrance : setting.DrawsExit;
                    var length = farEnd ? setting.EntranceLength : setting.ExitLength;
                    var defaultLength = farEnd ? defaults.EntranceLength : defaults.ExitLength;
                    if (defaultLength <= 0.0) defaultLength = defaults.ExitLength > 0.0 ? defaults.ExitLength : DynamicForkliftDefensePlan.EdgeLength;
                    var autoNow = stored == null || (farEnd ? stored.EntranceAuto : stored.ExitAuto);
                    var row = new GridRow { Post = p, Applies = applies };
                    var box = Text(drawn && length > 0.0 ? length : defaultLength);
                    if (farEnd) { row.EntranceCheck = applies && drawn; row.EntranceWasDrawn = applies && drawn; row.EntranceLength = box; row.EntranceAuto = autoNow; row.EntranceSeed = defaultLength; }
                    else { row.ExitCheck = applies && drawn; row.ExitWasDrawn = applies && drawn; row.ExitLength = box; row.ExitAuto = autoNow; row.ExitSeed = defaultLength; }
                    return row;
                }).ToArray();
                gesture?.Invoke(rows);

                var result = PushBackDefenseSides.Copy(incoming);
                result.RemoveAll(r => r.PostIndex < 0 || r.PostIndex >= postCount);
                foreach (var row in rows.Where(r => r.Applies))
                {
                    var check = farEnd ? row.EntranceCheck : row.ExitCheck;
                    var auto = (farEnd ? row.EntranceAuto : row.ExitAuto) == true && check == (farEnd ? row.EntranceWasDrawn : row.ExitWasDrawn);
                    var length = !auto && check ? Num(farEnd ? row.EntranceLength : row.ExitLength) : 0.0;
                    // T6 pb (review of 2f1e9e4e, M6): BuildFaceResult refuses a length ≤ 0 on an end that is on and not automatic, as TwoEndGrid.
                    Assert.True(auto || !check || length > 0.0);
                    var record = result.FirstOrDefault(r => r.PostIndex == row.Post);
                    if (record == null)
                    {
                        record = new SafetyPostDefense { PostIndex = row.Post, ExitAuto = true, EntranceAuto = true };
                        result.Add(record);
                    }

                    var value = auto ? 0.0 : check ? length : 0.0;
                    if (farEnd) { record.EntranceLength = value; record.EntranceAuto = auto; }
                    else { record.ExitLength = value; record.ExitAuto = auto; }
                }

                // T6 pb (PB-S17, point 5): BuildFaceResult no longer drops a row automatic at both ends; the merge of the section does.
                return result.OrderBy(r => r.PostIndex).ToList();
            }

            public sealed class DefenseSection
            {
                public PushBackSide Side;
                public int PostCount;
                public string PieceId;
                public bool OwnsBothEnds;
                public IReadOnlyList<bool> Faces;
                public List<SafetyPostDefense> Posts = new();
            }

            private static DefenseSection BuildDefense(PushBackDraftEditor d, PushBackSide side, bool ownsBothEnds, int postCount)
            {
                var stored = Of(d.State.SafetySelections, SelectiveSafetyDefaults.DefensaType);
                var section = new DefenseSection { Side = side, OwnsBothEnds = ownsBothEnds, PostCount = Math.Max(1, postCount) };
                section.Posts = PushBackDefenseSides.Copy(stored?.DefensaPosts).Where(r => r.PostIndex >= 0 && r.PostIndex < section.PostCount).ToList();
                var kept = d.Composite.Of(side).DefensePieceId;
                var pieceId = string.IsNullOrWhiteSpace(kept) ? stored?.ElementId : kept;
                section.PieceId = Combo(SelectiveSafetyDefaults.DefensaType, IsNone(pieceId) ? None : pieceId);
                section.Faces = PushBackDefenseSides.FacesOf(d.System?.Structure, postCount, side);
                return section;
            }

            /// <summary>«Configurar…» of the section of a side (a grid gesture), and/or its list of type; then «Aceptar».</summary>
            public static void Defense(PushBackDraftEditor d, PushBackSide side, Action<GridRow[]> grid, string type = null)
            {
                var posts = PostCount(d);
                var carrierId = Of(d.State.SafetySelections, SelectiveSafetyDefaults.DefensaType)?.ElementId;
                var two = d.Composite.SideBPresent;
                var a = BuildDefense(d, PushBackSide.A, !two, posts);
                var b = two ? BuildDefense(d, PushBackSide.B, false, posts) : null;
                var section = side == PushBackSide.B ? b : a;
                if (type != null) section.PieceId = Combo(SelectiveSafetyDefaults.DefensaType, type);
                if (grid != null)
                {
                    section.Posts = section.OwnsBothEnds
                        ? PushBackDefenseSides.Copy(TwoEndGrid(section.Posts, section.PostCount, grid))
                        : PushBackDefenseSides.Merge(section.Posts, FaceGrid(section.Posts, section.PostCount, section.Faces, PushBackDefenseSides.IsFarEnd(section.Side), grid), section.Side);
                }

                // ApplyDefenseSections
                d.Composite.Of(PushBackSide.A).DefensePieceId = a.PieceId;
                if (b != null) d.Composite.Of(PushBackSide.B).DefensePieceId = b.PieceId;
                Drop(d, SelectiveSafetyDefaults.DefensaType);
                var chosen = new[] { a, b }.Where(s => s != null && !IsNone(s.PieceId)).Select(s => s.PieceId).FirstOrDefault(id => !string.IsNullOrWhiteSpace(id));
                if (string.IsNullOrWhiteSpace(chosen))
                {
                    var hasIntent = a.Posts.Count > 0 || (b?.Posts.Count ?? 0) > 0;
                    if (!hasIntent || string.IsNullOrWhiteSpace(carrierId)) return;
                    chosen = carrierId;
                }

                var selection = new SelectiveSafetySelection { Quantity = 1, Side = SafetySide.None };
                d.State.SafetySelections.Add(selection);
                selection.ElementId = chosen;
                PushBackSafetyAuthority.RestrictToLowEnd(selection);
                if (a.OwnsBothEnds && b == null)
                {
                    foreach (var record in PushBackDefenseSides.Copy(a.Posts)) selection.DefensaPosts.Add(record);
                    return;
                }

                var merged = new List<SafetyPostDefense>(); // T6 pb (review of 2f1e9e4e, M1): from an empty list, as the window
                merged = PushBackDefenseSides.Merge(merged, a.Posts, PushBackSide.A);
                if (b != null) merged = PushBackDefenseSides.Merge(merged, b.Posts, PushBackSide.B);
                merged.RemoveAll(r => r.PostIndex < 0 || r.PostIndex >= a.PostCount);
                foreach (var record in merged) selection.DefensaPosts.Add(record);
            }

            // ---- protector lateral

            public static void Lateral(PushBackDraftEditor d, int post, SafetySide side)
            {
                var posts = PostCount(d);
                var existing = Of(d.State.SafetySelections, SelectiveSafetyDefaults.LateralType);
                var row = existing.PostSides.Where(p => p != null).Select(p => new SafetyPostSide { PostIndex = p.PostIndex, Side = p.Side }).ToList();
                var current = row.Count > 0 ? row : new List<SafetyPostSide> { new() { PostIndex = 0, Side = SafetySide.Left }, new() { PostIndex = posts - 1, Side = SafetySide.Right } };
                var result = new List<SafetyPostSide>();
                for (var p = 0; p < posts; p++)
                {
                    SafetySide? value = p == post ? side : current.FirstOrDefault(c => c.PostIndex == p)?.Side;
                    if (value.HasValue) result.Add(new SafetyPostSide { PostIndex = p, Side = value.Value });
                }

                OkLateral(d, existing, result);
            }

            public static void LateralToRule(PushBackDraftEditor d)
                => OkLateral(d, Of(d.State.SafetySelections, SelectiveSafetyDefaults.LateralType), new List<SafetyPostSide>());

            // The OK of the dialog for the row of the lateral (useDynamicSafetyDefaults): a new selection. T6 seg: also when nothing draws
            // (every post on «Ninguno»), as the window; before, «Aceptar» dropped the family then.
            private static void OkLateral(PushBackDraftEditor d, SelectiveSafetySelection existing, List<SafetyPostSide> overrides)
            {
                var index = d.State.SafetySelections.IndexOf(existing);
                d.State.SafetySelections.RemoveAt(index);
                var selection = new SelectiveSafetySelection { ElementId = existing.ElementId, Side = SafetySide.None, Quantity = 1 };
                foreach (var p in overrides) selection.PostSides.Add(new SafetyPostSide { PostIndex = p.PostIndex, Side = p.Side });
                d.State.SafetySelections.Insert(index, selection);
            }

            // ---- desviador

            private static double Effective(double value, double fallback) => SelectiveDesviadorPlan.IsValidEvenAbove8(value) ? value : fallback;

            // The side the grid returns in Push Back: the dialog opens it with showDesviadorSide: false, so its list starts on
            // SideIndex(LowEndSide) whatever the desviador had (SafetyDesviadorGridWindow, LowEndSide = Left) and BuildResult reads it back.
            private const SafetySide GridSide = SafetySide.Left;

            public static void Desviador(PushBackDraftEditor d, Action<HashSet<(int Post, int Level)>> toggle, double? longitud = null)
            {
                var existing = Of(d.State.SafetySelections, SelectiveSafetyDefaults.DesviadorType);
                Grid(d, existing.ElementId, GridSide,
                    longitud ?? Effective(existing.DesviadorLongitud, SelectiveSafetyDefaults.DesviadorLongitud),
                    Effective(existing.DesviadorPrimerNivelAltura, SelectiveSafetyDefaults.DesviadorPrimerNivelAltura),
                    existing.Desviador.CellsByPost, existing.DesviadorOffCells.Where(c => c != null).ToList(), toggle, existing);
            }

            // A new desviador: the variant chosen, «Configurar…» and «Aceptar» with nothing changed (the side of the grid, 18 in, every cell on).
            public static void NewDesviador(PushBackDraftEditor d, string elementId)
                => Grid(d, elementId, GridSide, SelectiveSafetyDefaults.DesviadorLongitud, SelectiveSafetyDefaults.DesviadorPrimerNivelAltura, false,
                    new List<SelectiveGridCell>(), null, null);

            // T6 seg: a new desviador, its variant chosen and «Aceptar» WITHOUT «Configurar…» (SelectiveSafetyWindow.OnOk; before, it dropped
            // the family): the defaults of its row (18 in, its first level at 18 in, every cell on; a new desviador of Push Back is not read by
            // post, newDesviadorByPost is false) and the side its grid would return (showDesviadorSide: false, so the one of the low end).
            public static void NewDesviadorWithoutGrid(PushBackDraftEditor d, string elementId)
                => d.State.SafetySelections.Add(new SelectiveSafetySelection
                {
                    ElementId = elementId, Quantity = 1, Side = GridSide,
                    DesviadorLongitud = SelectiveSafetyDefaults.DesviadorLongitud, DesviadorPrimerNivelAltura = SelectiveSafetyDefaults.DesviadorPrimerNivelAltura,
                });

            // The list of an existing desviador changed and «Aceptar» without «Configurar…» (SelectiveSafetyWindow): the row is seeded from the
            // selection (its side Left or Right, else Both; its measures and its cells as they are) and is written whole. T6 seg: its cells all
            // off no longer drop it (before, only its empty level counts, from SelectiveDesviadorPlan.Build(resolvedSystem: null, …), kept it).
            public static void DesviadorVariant(PushBackDraftEditor d, string elementId)
            {
                var existing = Of(d.State.SafetySelections, SelectiveSafetyDefaults.DesviadorType);
                var side = existing.Side == SafetySide.Left || existing.Side == SafetySide.Right ? existing.Side : SafetySide.Both;
                var off = existing.DesviadorOffCells?.Where(c => c != null).Select(c => new SelectiveGridCell { Frente = c.Frente, Level = c.Level }).ToList()
                          ?? new List<SelectiveGridCell>();
                var index = d.State.SafetySelections.IndexOf(existing);
                d.State.SafetySelections.RemoveAt(index);
                var selection = new SelectiveSafetySelection
                {
                    ElementId = elementId, Quantity = 1, Side = side,
                    DesviadorLongitud = existing.DesviadorLongitud, DesviadorPrimerNivelAltura = existing.DesviadorPrimerNivelAltura,
                };
                selection.Desviador.CellsByPost = existing.Desviador.CellsByPost;
                foreach (var cell in off) selection.DesviadorOffCells.Add(new SelectiveGridCell { Frente = cell.Frente, Level = cell.Level });
                d.State.SafetySelections.Insert(index, selection);
            }

            // RackPushBackSystemWindow.DesviadorLevelsPerPost: the active side in a rack of one way; with a side B, post by post the higher of
            // the two sides (T6 pb, PB-S17 point 4: before, the active side only).
            private static IReadOnlyList<int> Levels(PushBackDraftEditor d)
            {
                if (!d.Composite.SideBPresent)
                    return DynamicFrontActivation.EffectiveLevelsPerPost(d.Composite.Of(d.Composite.ActiveSide).Structure.EffectiveLevelCounts());
                var perSide = new[] { PushBackSide.A, PushBackSide.B }
                    .Select(side => DynamicFrontActivation.EffectiveLevelsPerPost(d.Composite.Of(side).Structure.EffectiveLevelCounts())).ToList();
                return Enumerable.Range(0, perSide.Max(l => l.Count)).Select(p => perSide.Max(l => p < l.Count ? l[p] : 0)).ToList();
            }

            private static void Grid(PushBackDraftEditor d, string elementId, SafetySide side, double longitud, double first, bool cellsByPost,
                List<SelectiveGridCell> stored, Action<HashSet<(int Post, int Level)>> toggle, SelectiveSafetySelection existing)
            {
                var levels = Levels(d);
                var off = new HashSet<(int Post, int Level)>(stored.Where(c => c.Frente >= 0 && c.Frente < levels.Count && c.Level >= 0 && c.Level < levels[c.Frente])
                    .Select(c => (c.Frente, c.Level)));
                toggle?.Invoke(off);
                var live = new List<SelectiveGridCell>();
                for (var c = 0; c < levels.Count; c++)
                    for (var r = 0; r < levels[c]; r++)
                        if (off.Contains((c, r))) live.Add(new SelectiveGridCell { Frente = c, Level = r });
                var persisted = SafetyDormantCells.Merge(live, stored, levels).ToList();

                var index = existing == null ? -1 : d.State.SafetySelections.IndexOf(existing);
                if (index >= 0) d.State.SafetySelections.RemoveAt(index);
                // T6 seg: every cell off keeps the family (before, «Aceptar» dropped it: SelectiveSafetyGrid.AllCellsOff).
                var selection = new SelectiveSafetySelection { ElementId = elementId, Quantity = 1, Side = side, DesviadorLongitud = longitud, DesviadorPrimerNivelAltura = first };
                selection.Desviador.CellsByPost = cellsByPost;
                foreach (var cell in persisted) selection.DesviadorOffCells.Add(new SelectiveGridCell { Frente = cell.Frente, Level = cell.Level });
                if (index >= 0) d.State.SafetySelections.Insert(index, selection);
                else d.State.SafetySelections.Add(selection);
            }
        }
    }
}
