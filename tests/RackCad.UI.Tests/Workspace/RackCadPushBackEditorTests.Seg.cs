#nullable enable
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.PushBack;
using RackCad.Application.VentanaRackCad;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;
using RackCad.UI.VentanaRackCad;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// T5 pb, PB-S03: the safety of a Push Back post by post in «Extremos y seguridad» (the table and the inspector of the fourth batch, with
    /// the words «Entrada/Salida / Posterior» and the authority by side of I-42), the model and the view (STA). And the corrections of the
    /// review of 24adfee7 that live in the window: «En blanco» with «Ambos lados» (point 1), the gate of the BOM of the draft (point 2), the
    /// base of «Descartar» of a rack read without a design that recomposes (point 3) and PB-S10 in «Nuevo» from a design (point 6).
    /// </summary>
    public partial class RackCadPushBackEditorTests
    {
        private static PushBackEditorViewModel NewDraft(Rig rig)
        {
            rig.Window.StartNewPushBack();
            return Assert.IsType<PushBackEditorViewModel>(rig.Window.NewPushBackEditor);
        }

        private static SelectiveSafetySelection Family(PushBackEditorViewModel editor, string type)
            => PushBackSafetyEdit.Of(editor.Draft.State.SafetySelections, editor.Draft.Catalog, type);

        private static void Choose(SelectiveChoiceItem item, string value) => item.SelectedValue = value;

        private static bool IsShown(DependencyObject element)
        {
            for (var at = element; at != null; at = System.Windows.Media.VisualTreeHelper.GetParent(at))
            {
                if (at is UIElement ui && ui.Visibility != Visibility.Visible) return false;
            }

            return true;
        }

        private static SelectiveChoiceItem ListOf(RackCadSafetyInspector inspector, string key) => inspector.Fields.OfType<SelectiveChoiceItem>().Single(f => f.Key == key);

        // ------------------------------------------------------------------ PB-S03, the model

        [Fact]
        public void TheSafetyOfAPushBackIsEditedPostByPostWithItsWordsAndTheTableAddsUpToTheBom()
        {
            var editor = NewDraft(new Rig());
            var panel = editor.SafetyPanel;
            Assert.True(panel.IsPushBack);
            Assert.False(panel.IsSelective);
            Assert.False(panel.IsDynamic);
            Assert.Equal(new[] { "BOTA", "LATERAL", "DESVIADOR", "DEFENSA" }, panel.Families);
            Assert.Equal("Botas", panel.BootsHeader);
            Assert.False(panel.ShowsBootsB);
            Assert.True(panel.ShowsDefense);
            Assert.False(panel.ShowsGuides);
            Assert.Equal("Defensa Entrada/Salida (in)", panel.DefenseExitHeader);
            Assert.Equal("Defensa Posterior (in)", panel.DefenseEntranceHeader);

            var posts = editor.Draft.SafetyPostCount;
            Assert.Equal(posts + 1, panel.Table.Count);
            Assert.Equal("Total", panel.Table[^1].Post);

            // The guide of the protector lateral keeps «Izquierda / Derecha»; at the ends it replaces the botas (the rule of the ends).
            var lateral = panel.Table[0].SideGuard.Choice!;
            Assert.Equal(new[] { "No lleva", "Izquierda", "Derecha", "Ambos (frente-puente)" }, lateral.Options.Select(o => o.Label));
            Assert.Equal("Regla de los extremos", panel.Table[0].SideGuard.Sub);
            Assert.Equal("La reemplaza el protector lateral", panel.Table[0].Boots.Text);

            // A post that the protector lateral leaves gets its botas back, with the words of Push Back (SG-02): one of its own, «Posterior».
            Choose(panel.Table[0].SideGuard.Choice!, "N");
            Assert.Contains("ya no se recalcula", editor.Message);
            Assert.False(PushBackSafetyEdit.LateralFollowsRule(Family(editor, "LATERAL")));
            var boot = panel.Table[0].Boots.Choice!;
            Assert.Equal(new[] { "General (Entrada/Salida, automática)", "Ninguno", "Entrada/Salida", "Posterior", "Ambas" }, boot.Options.Select(o => o.Label));
            Choose(panel.Table[0].Boots.Choice!, RackCadSafetyPanelViewModel.PlacementCode(BootPlacement.Rear));
            Assert.Equal(BootPlacement.Rear, PushBackSafetyEdit.BootSide(editor.Draft.CopySafety(), editor.Draft.Catalog, PushBackSide.A).OwnAt(0));
            Assert.True(panel.Table[0].Boots.IsOwn);
            Assert.StartsWith("Propia · ", panel.Table[0].Boots.Sub);
            Assert.True(editor.IsDirty);

            // The foot adds up the rows, as the BOM counts them.
            var rows = editor.Draft.SafetyRows();
            Assert.Equal(rows.Sum(r => r.Boots).ToString(), panel.Table[^1].Boots.Text);
            Assert.Equal(rows.Sum(r => r.SideGuards).ToString(), panel.Table[^1].SideGuard.Text);
            Assert.Equal(rows.Sum(r => r.Deflectors).ToString(), panel.Table[^1].Deflectors.Text);

            // A level of the desviador of a post, by post (PB-002).
            var level = panel.Table[0].Deflectors.Levels.First();
            Assert.True(level.IsOn);
            level.Toggle();
            Assert.False(panel.Table[0].Deflectors.Levels.First().IsOn);
            Assert.Contains(Family(editor, "DESVIADOR").DesviadorOffCells, c => c.Frente == 0 && c.Level == 0);
        }

        [Fact]
        public void AnEndOfTheDefenseTakesTheLengthOfTheRuleOfItsOwnAndAnEmptyLengthIsTheAutomaticOne()
        {
            var editor = NewDraft(new Rig());
            var panel = editor.SafetyPanel;

            // «Posterior» is off by default (PB-009); its box proposes the length of the rule, of its own, as the classic grid.
            var posterior = panel.Table[0].DefenseEntrance;
            Assert.True(posterior.HasDefense);
            Assert.False(posterior.IsOn);
            Assert.Equal("Automática · sin defensa", posterior.Sub);
            posterior.IsOn = true;
            posterior = panel.Table[0].DefenseEntrance;
            Assert.True(posterior.IsOn);
            Assert.Equal("Propia", posterior.Sub);
            Assert.Equal("12", posterior.Length.Text);

            // An empty length is the automatic one: for «Posterior», no defense again.
            Write(posterior.Length, string.Empty);
            Assert.False(panel.Table[0].DefenseEntrance.IsOn);

            // «Entrada/Salida» is automatic (12 in at an edge post); a length written is its own, and a value refused stays in its field.
            var exit = panel.Table[0].DefenseExit;
            Assert.True(exit.IsOn);
            Assert.Equal("Automática · 12 in", exit.Sub);
            Write(exit.Length, "-3");
            Assert.Contains("mayor que cero", exit.Length.Error);
            Write(exit.Length, "20");
            Assert.Equal("Propia", panel.Table[0].DefenseExit.Sub);
            Assert.False(Family(editor, "DEFENSA").DefensaPosts.Single(r => r.PostIndex == 0).ExitAuto);

            // «Todas automáticas».
            panel.Pick("DEFENSA");
            var all = panel.Inspector.Actions.Single(a => a.Key == "autoA");
            Assert.True(all.IsEnabled);
            all.Run();
            Assert.Empty(Family(editor, "DEFENSA").DefensaPosts);
        }

        [Fact]
        public void WithNoTypeOfDefenseAndNothingByPostItsBoxesAreDisabledUntilATypeIsChosen()
        {
            // Review of 01e76902, point 3: a rack of one way, its defense all automatic, «Tipo de defensa» to «Ninguno». The family leaves the
            // rack (nothing by post to keep), so a post has nowhere to keep a decision, as in the classic: its boxes are disabled and say why.
            var editor = NewDraft(new Rig());
            var panel = editor.SafetyPanel;
            Assert.Empty(Family(editor, "DEFENSA").DefensaPosts);
            Assert.True(panel.Table[1].DefenseExit.IsEnabled);
            panel.Pick("DEFENSA");
            Assert.Contains(panel.Inspector.Notes, n => n.Contains("sus casillas quedan deshabilitadas", StringComparison.Ordinal));
            Choose(panel.Inspector.Variant, PushBackDefaults.NonePieceId);
            Assert.False(editor.MessageIsError, editor.Message);
            Assert.Contains("la defensa sale del rack", editor.Message);
            Assert.Null(Family(editor, "DEFENSA"));

            Assert.Equal(editor.Draft.SafetyPostCount, panel.Table.Count(r => r.DefenseExit.HasDefense));
            foreach (var row in panel.Table.Take(panel.Table.Count - 1))
            {
                foreach (var end in new[] { row.DefenseExit, row.DefenseEntrance }.Where(e => e.HasDefense))
                {
                    Assert.False(end.IsEnabled);
                    Assert.True(end.Length.IsReadOnly);
                    Assert.Equal(RackCadSafetyPanelViewModel.DefenseWithoutTypeSub, end.Sub);
                }
            }

            // A click that reaches the box anyway writes nothing and says why; the box shows the table again.
            var json = editor.Draft.DesignJson();
            var exit = panel.Table[1].DefenseExit;
            Assert.True(exit.IsOn); // the automatic length that comes back with a type
            exit.IsOn = false;
            Assert.True(editor.MessageIsError);
            Assert.Equal(RackCadSafetyPanelViewModel.DefenseWithoutTypeText, editor.Message);
            Assert.True(panel.Table[1].DefenseExit.IsOn);
            Assert.Equal(json, editor.Draft.DesignJson());

            // A type again: the automatic ones come back and the boxes can be decided.
            var piece = PushBackSafetyEdit.Variants(editor.Draft.Catalog, "DEFENSA").First().Id.Trim();
            Choose(panel.Inspector.Variant, piece);
            Assert.True(panel.Table[1].DefenseExit.IsEnabled);
            Assert.StartsWith("Automática · ", panel.Table[1].DefenseExit.Sub);
            panel.Table[1].DefenseExit.IsOn = false;
            Assert.False(editor.MessageIsError, editor.Message);
            Assert.False(Family(editor, "DEFENSA").DefensaPosts.Single(r => r.PostIndex == 1).ExitAuto);
        }

        [Fact]
        public void ATwoSidedPushBackHasTheBotasAndTheDefenseOfEachSide()
        {
            var editor = new Rig().OpenEditor("PB-2", "pb-20-ab", "PB 2");
            var panel = editor.SafetyPanel;
            Assert.True(panel.ShowsBootsB);
            Assert.Equal("Botas · lado A", panel.BootsHeader);
            Assert.Equal("Defensa lado A (in)", panel.DefenseExitHeader);
            Assert.Equal("Defensa lado B (in)", panel.DefenseEntranceHeader);

            // The rack was read with no safety: each side shows «Ninguno» and chooses its type in the inspector.
            panel.Pick("BOTA");
            Assert.Equal("Tipo de protector · lado A", panel.Inspector.Variant.Label);
            Assert.Equal(PushBackDefaults.NonePieceId, panel.Inspector.Variant.SelectedValue);
            var piece = PushBackSafetyEdit.Variants(editor.Draft.Catalog, "BOTA").First().Id.Trim();
            Choose(ListOf(panel.Inspector, "typeB"), piece);
            Assert.Equal(piece, PushBackSafetyEdit.BootSide(editor.Draft.CopySafety(), editor.Draft.Catalog, PushBackSide.B).PieceId);
            Assert.Equal(PushBackDefaults.NonePieceId, PushBackSafetyEdit.BootSide(editor.Draft.CopySafety(), editor.Draft.Catalog, PushBackSide.A).PieceId);
            Assert.True(editor.Draft.SafetyRows().Sum(r => r.BootsB) > 0);
            Assert.Equal(0, editor.Draft.SafetyRows().Sum(r => r.Boots));

            // A post of side B, and the general of side A, each in its own place.
            Choose(panel.Table[0].BootsB.Choice!, RackCadSafetyPanelViewModel.PlacementCode(BootPlacement.Both));
            Assert.Equal(BootPlacement.Both, PushBackSafetyEdit.BootSide(editor.Draft.CopySafety(), editor.Draft.Catalog, PushBackSide.B).OwnAt(0));
            Assert.Null(PushBackSafetyEdit.BootSide(editor.Draft.CopySafety(), editor.Draft.Catalog, PushBackSide.A).OwnAt(0));
            Choose(ListOf(panel.Inspector, "generalA"), RackCadSafetyPanelViewModel.PlacementCode(BootPlacement.Rear));
            Assert.True(PushBackSafetyEdit.BootSide(editor.Draft.CopySafety(), editor.Draft.Catalog, PushBackSide.A).Explicit);
        }

        [Fact]
        public void TheTypeNoneOfASideAsksNothingAndRemovingADesviadorAsksWhatIsLostAndDescartarBringsItBack()
        {
            var editor = NewDraft(new Rig());
            var panel = editor.SafetyPanel;
            panel.Pick("BOTA");
            Assert.Equal("Tipo de protector", panel.Inspector.Variant.Label);
            Choose(panel.Inspector.Variant, PushBackDefaults.NonePieceId);
            Assert.False(panel.Inspector.IsAsking);
            Assert.Contains("se conserva", editor.Message);
            Assert.Equal(0, editor.Draft.SafetyRows().Sum(r => r.Boots));
            Assert.NotNull(Family(editor, "BOTA")); // the family stays, as the carrier of the intent

            panel.Pick("DESVIADOR");
            Assert.Equal(new[] { "length", "first" }, panel.Inspector.Fields.OfType<DynamicFieldItem>().Select(f => f.Key)); // no side (PB-003)
            Choose(panel.Inspector.Variant, RackCadSafetyPanelViewModel.NoVariant);
            Assert.True(panel.Inspector.IsAsking);
            Assert.NotNull(Family(editor, "DESVIADOR"));
            panel.Inspector.ConfirmRemove();
            Assert.Null(Family(editor, "DESVIADOR"));
            Assert.Equal(0, editor.Draft.SafetyRows().Sum(r => r.Deflectors));

            editor.Discard();
            Assert.NotNull(Family(editor, "DESVIADOR"));
            Assert.False(editor.IsDirty);
        }

        [Fact]
        public void WhileAutoCadWritesNothingOfTheSafetyOfAPushBackCanBeEdited()
        {
            var rig = new Rig();
            var editor = rig.OpenEditor("PB-1", "pb-06-seguridad", "PB 1");
            Change(editor);
            editor.RequestWrite();
            Assert.True(editor.IsWriting);
            var panel = editor.SafetyPanel;
            Assert.All(panel.Table.Where(r => r.Boots.Choice != null), r => Assert.False(r.Boots.Choice!.IsEnabled));
            var json = editor.Draft.DesignJson();
            Assert.NotNull(((IPushBackSafetyPanelHost)editor).WritePushBackSafety(_ => null, null));
            Assert.Equal(json, editor.Draft.DesignJson());
        }

        // ------------------------------------------------------------------ PB-S03, the view (real WPF)

        [Fact]
        public void TheSafetyTabOfPushBackShowsItsColumnsWithUniqueNamesAndWritesFromItsCells()
        {
            StaTestRunner.Run(() => WithBindingErrors(errors =>
            {
                var editor = NewDraft(new Rig());
                var view = new PushBackEditorView(editor);
                Lay(view, 1500);
                view.Tabs.SelectedItem = view.SafetyTab;
                Lay(view, 1500);
                foreach (var type in editor.SafetyPanel.Families)
                {
                    editor.SafetyPanel.Pick(type);
                    Lay(view, 1500);
                }

                var table = view.SafetyTable;
                Assert.True(IsShown(table.BootsHead) && IsShown(table.SideGuardHead) && IsShown(table.DeflectorsHead) && IsShown(table.DefenseHeads));
                Assert.False(IsShown(table.BootsBHead));
                Assert.False(IsShown(table.SeparatorsHead));
                Assert.False(IsShown(table.GuidesHead));
                Assert.Equal("Defensa Entrada/Salida (in)", table.DefenseExitHead.Content);
                Assert.Equal("Defensa Posterior (in)", table.DefenseEntranceHead.Content);
                Assert.Equal("Botas: ver su variante y sus medidas", Name(table.BootsHead)); // one way: the name of always
                Assert.DoesNotContain(Descendants<TextBlock>(view), t => (t.Text ?? string.Empty).StartsWith("RackCad.", StringComparison.Ordinal)); // F-1
                var names = Descendants<Control>(table.RowList).Where(c => IsShown(c) && (c is ComboBox || c is Button || c is CheckBox || c is TextBox))
                    .Select(c => Name(c)).Where(n => !string.IsNullOrEmpty(n)).ToList();
                Assert.Equal(names.Count, names.Distinct().Count());

                // The title of a column shows its family; a level button writes its level; a list writes the bota of its post.
                Click(table.DefenseEntranceHead);
                Assert.Equal("Defensas de montacargas", view.SafetyInspector.FamilyTitle.Text);
                var level = Descendants<Button>(table.RowList).First(b => Name(b) == "P1 · N1: con desviador. Pulsa para quitarlo.");
                Click(level);
                Lay(view, 1500);
                Assert.False(editor.SafetyPanel.Table[0].Deflectors.Levels[0].IsOn);
                Choose(editor.SafetyPanel.Table[1].SideGuard.Choice!, "N"); // the protector lateral of the end leaves P2: its botas come back
                Lay(view, 1500);
                var combo = Descendants<ComboBox>(table.RowList).First(c => Name(c) == "Botas en P2");
                combo.SelectedValue = RackCadSafetyPanelViewModel.PlacementCode(BootPlacement.Both);
                Lay(view, 1500);
                Assert.Equal(BootPlacement.Both, PushBackSafetyEdit.BootSide(editor.Draft.CopySafety(), editor.Draft.Catalog, PushBackSide.A).OwnAt(1));

                // Beside the tab, and below it when it is narrow.
                Assert.False(view.SafetyInspector.IsBelow);
                Lay(view, 600);
                Assert.True(view.SafetyInspector.IsBelow);
                Assert.Empty(errors.Messages);
            }));
        }

        [Fact]
        public void TheSafetyTabOfATwoSidedPushBackShowsTheBotasOfSideB()
        {
            StaTestRunner.Run(() => WithBindingErrors(errors =>
            {
                var editor = new Rig().OpenEditor("PB-2", "pb-20-ab", "PB 2");
                var view = new PushBackEditorView(editor);
                Lay(view, 1700);
                view.Tabs.SelectedItem = view.SafetyTab;
                Lay(view, 1700);
                var table = view.SafetyTable;
                Assert.True(IsShown(table.BootsBHead));
                Assert.Equal("Botas · lado A", table.BootsHead.Content);
                Assert.Equal("Botas del lado A: ver su tipo y sus medidas", Name(table.BootsHead)); // review of 01e76902, point 6
                Assert.Equal("Botas del lado B: ver su tipo y sus medidas", Name(table.BootsBHead));
                Assert.Equal("Defensa lado B (in)", table.DefenseEntranceHead.Content);
                Assert.Contains(Descendants<ComboBox>(table.RowList), c => Name(c) == "Botas del lado B en P1");
                Assert.Empty(errors.Messages);
            }));
        }

        // ------------------------------------------------------------------ the review of 24adfee7

        [Fact]
        public void BlankWithBothSidesIsWrittenInTheTwoSidesAsTheClassicWindowDoes()
        {
            var editor = new Rig().OpenEditor("PB-2", "pb-20-ab", "PB 2");
            Assert.True(editor.Draft.IsPresent(PushBackSide.A, 0) && editor.Draft.IsPresent(PushBackSide.B, 0));
            editor.IsBothSides = true;
            editor.SelectFront(0);
            editor.SetFrontBlank(true);
            Assert.False(editor.MessageIsError, editor.Message);
            Assert.False(editor.Draft.IsPresent(PushBackSide.A, 0));
            Assert.False(editor.Draft.IsPresent(PushBackSide.B, 0));
            Assert.True(editor.IsDirty);
        }

        [Fact]
        public void TheBomOfADraftThatTheGateBlocksIsNotCounted()
        {
            var rig = new Rig();
            var editor = rig.OpenEditor("PB-2", "pb-20-ab", "PB 2");
            Change(editor);
            Assert.NotNull(((IRackBomPageHost)rig.Window).DraftBomOf(Session, "PB-2"));

            editor.Draft.Composite.SetStructureOverride(PushBackSide.A, 1); // not buildable: the gate blocks the output
            Assert.True(editor.Draft.Recompose(), editor.Draft.LastError);
            Assert.True(editor.Draft.IsOutputBlocked);
            Assert.True(((IRackBomPageHost)rig.Window).HasUnwrittenDraft(Session, "PB-2"));
            Assert.Null(((IRackBomPageHost)rig.Window).DraftBomOf(Session, "PB-2"));
        }

        [Fact]
        public void DescartarOfARackReadWithoutADesignOfItsOwnLoadsWhatWasRead()
        {
            // A draft that has no valid design yet (as a rack whose read did not recompose): the JSON read is its base.
            var json = Envelope("pb-01-base").Design;
            var editor = new PushBackEditorViewModel(new PushBackDraftEditor(UiSupport.LoadCatalogSafe()), "PB-1", null, readDesignJson: json);
            Assert.Null(editor.Draft.DesignJson());
            Assert.Equal("Limpio", editor.StatusText);
            Assert.Equal(json, editor.BaselineDesignJson);

            editor.Discard();
            Assert.NotNull(editor.Draft.DesignJson()); // what was read, loaded again
            Assert.Equal(3, editor.Draft.SlotCount);
            Assert.False(editor.IsDirty);
        }

        [Fact]
        public void NuevoFromADesignWithTheDormantHalfOfASideBIsRefusedWithTheWordsOfTheDoor()
        {
            var rig = new Rig();
            var project = new RackProjectStore().Deserialize(Envelope("pb-21-cola").Design);
            Assert.False(rig.Window.OpenDesignAsNewRack(project, "Cola"));
            Assert.Contains("mitad dormida de un lado B", rig.Window.ActionMessage);
            Assert.Null(rig.Window.NewPushBackEditor);
        }
    }
}
