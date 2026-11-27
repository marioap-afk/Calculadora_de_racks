#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using RackCad.Application.Catalogs;
using RackCad.Application.CustomProperties;
using RackCad.Application.Persistence;
using RackCad.Application.StructuralSections;
using RackCad.Application.Systems.Cantilever;
using RackCad.Application.Systems.Shared;
using RackCad.Application.VentanaRackCad;
using RackCad.Application.Workspace;
using RackCad.Domain.Systems.Cantilever;
using RackCad.UI.VentanaRackCad;
using RackCad.UI.Workspace;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// T6 pr (I-64, sexta tanda; VA-S9, answered by the Owner on 2026-11-27: «Sí, y se escriben al insertarlo»): every draft of «Nuevo»
    /// (Selectivo, Dinámico, cama, Cantilever and Push Back) has the tab «Propiedades» of VA-07, editable; its rows are part of the draft, a
    /// wrong one stops «Insertar en el dibujo…», the NUEVO request carries them, and the answer of AutoCAD leaves the tab of the rack of the
    /// drawing: written, what the drawing holds; not written, the rows over what the rack holds, to write with «Actualizar».
    /// </summary>
    public class RackCadNewDraftPropertiesTests
    {
        private static readonly SessionId Session = new(7);
        private const string IdA = "6f1c2a3b-0000-4000-8000-000000000001";
        private static readonly RackViewAddress Planta = RackViewAddress.Whole(DimensionViewKind.Planta);

        public enum Sys
        {
            Selectivo,
            Dinamico,
            Cama,
            Cantilever,
            PushBack,
        }

        public static TheoryData<Sys> Systems() => new() { Sys.Selectivo, Sys.Dinamico, Sys.Cama, Sys.Cantilever, Sys.PushBack };

        // ------------------------------------------------------------------ the door of every system

        private sealed record Sent(long RequestId, string RackId, bool NewRack, RackPropertiesWrite? Properties);

        private sealed class AllDoors : IRackCadSelectiveDoorPort, IRackCadCamaDoorPort, IRackCadCantileverDoorPort, IRackCadPushBackDoorPort
        {
            public List<Sent> Writes { get; } = new();

            public event Action<RackDesignRead>? DesignRead { add { } remove { } }

            public event Action<RackDesignWritten>? DesignWritten;

            public event Action<long>? RequestStarted { add { } remove { } }

            public event Action<SelectiveDesignRead>? SelectiveRead { add { } remove { } }

            public event Action<CamaDesignRead>? CamaRead { add { } remove { } }

            public event Action<CantileverDesignRead>? CantileverRead { add { } remove { } }

            public event Action<RackDesignRead>? PushBackRead { add { } remove { } }

            public RackCadClassicResult RequestDesign(RackDoorRequest request) => RackCadClassicResult.Started();

            public RackCadClassicResult RequestSelectiveDesign(RackDoorRequest request) => RackCadClassicResult.Started();

            public RackCadClassicResult RequestCamaDesign(RackDoorRequest request) => RackCadClassicResult.Started();

            public RackCadClassicResult RequestCantileverDesign(RackDoorRequest request) => RackCadClassicResult.Started();

            public RackCadClassicResult RequestPushBackDesign(RackDoorRequest request) => RackCadClassicResult.Started();

            public RackCadClassicResult RequestDynamicUpdate(RackDoorWrite write) => Keep(new Sent(write.RequestId, write.RackId, write.NewRack, write.Properties));

            public RackCadClassicResult RequestSelectiveUpdate(SelectiveDoorWrite write) => Keep(new Sent(write.RequestId, write.RackId, write.NewRack, write.Properties));

            public RackCadClassicResult RequestCamaWrite(CamaDoorWrite write) => Keep(new Sent(write.RequestId, write.RackId, write.NewRack, write.Properties));

            public RackCadClassicResult RequestCantileverUpdate(CantileverDoorWrite write) => Keep(new Sent(write.RequestId, write.RackId, write.NewRack, write.Properties));

            public RackCadClassicResult RequestPushBackUpdate(PushBackDoorWrite write) => Keep(new Sent(write.RequestId, write.RackId, write.NewRack, write.Properties));

            public void Abandon(long requestId)
            {
            }

            /// <summary>What a NUEVO command answers when it placed the rack (RackPropertiesRules.AnswerForNewRack decides the rest).</summary>
            public void Placed(Sent sent, RackPropertiesRead? after, string? error = null)
                => DesignWritten?.Invoke(RackPropertiesRules.AnswerForNewRack(RackDoorRules.AnswerForNewRack(sent.RequestId, sent.RackId, 1, "Rack 9"),
                    sent.Properties, error, after));

            /// <summary>What a NUEVO command answers when nothing was placed (the jig cancelled): the rule of the door, which the properties do not change.</summary>
            public void NotPlaced(Sent sent, bool cama)
                => DesignWritten?.Invoke(RackPropertiesRules.AnswerForNewRack(cama
                        ? CamaDoorRules.AnswerForNewCama(sent.RequestId, sent.RackId, 0, null)
                        : RackDoorRules.AnswerForNewRack(sent.RequestId, sent.RackId, 0, null),
                    sent.Properties, null, null));

            public void Written(Sent sent, RackPropertiesRead? after)
                => DesignWritten?.Invoke(new RackDesignWritten(sent.RequestId, sent.RackId, true, "Escrito. También escribió sus propiedades.", Properties: after));

            private RackCadClassicResult Keep(Sent sent)
            {
                Writes.Add(sent);
                return RackCadClassicResult.Started();
            }
        }

        // ------------------------------------------------------------------ a draft of each system, as the tests use it

        private sealed record Draft(object Editor, RackPropertiesTabViewModel Tab, Func<bool> IsDirty, Func<string> InsertBlocked, Action Insert,
            Action Update, Action Discard, Func<bool> IsNewRack);

        private static Draft Of(object editor) => editor switch
        {
            SelectiveEditorViewModel s => new Draft(s, s.PropertiesTab, () => s.IsDirty, () => s.InsertViewBlockedText,
                () => s.InsertWith(RackViewAddress.Fondo(0), Planta), s.RequestWrite, s.Discard, () => s.IsNewRack),
            DynamicEditorViewModel d => new Draft(d, d.PropertiesTab, () => d.IsDirty, () => d.InsertViewBlockedText,
                () => d.InsertWith(Planta), d.RequestWrite, d.Discard, () => d.IsNewRack),
            CamaEditorViewModel c => new Draft(c, c.PropertiesTab, () => c.IsDirty, () => c.PrimaryBlockedText, c.RequestWrite, c.RequestWrite,
                c.Discard, () => c.IsNewRack),
            CantileverEditorViewModel t => new Draft(t, t.PropertiesTab, () => t.IsDirty, () => t.InsertViewBlockedText,
                () => t.InsertWith(RackViewAddress.Whole(DimensionViewKind.Frontal), RackViewAddress.Station(0)), t.RequestWrite, t.Discard, () => t.IsNewRack),
            PushBackEditorViewModel p => new Draft(p, p.PropertiesTab, () => p.IsDirty, () => p.InsertViewBlockedText, () =>
            {
                p.InsertView();
                var panel = p.InsertPanel;
                if (panel == null) return;
                panel.Mark(Planta, RackViewAddress.PushBackCut(RackPushBackEnd.Posterior, RackPushBackSide.A));
                panel.Insert();
            }, p.RequestWrite, p.Discard, () => p.IsNewRack),
            _ => throw new ArgumentException("not an editor of a draft", nameof(editor)),
        };

        private sealed class Rig
        {
            public Rig()
            {
                var host = new WorkspacePortDouble { Snapshot = WorkspacePanelFixtures.Ready(session: 7) };
                var summary = new WorkspaceBrowserSummaryPortDouble { ListResponder = (_, _) => BrowserRackList.Ready(Array.Empty<BrowserRackListEntry>()) };
                var panel = new WorkspacePanelViewModel(host, _ => { }, new WorkspaceNavigationPortDouble(), summary, new WorkspacePilotEditPortDouble());
                Window = new RackCadWindowViewModel(panel, null, null, Door, UiSupport.LoadCatalogSafe);
                Window.Refresh();
            }

            public AllDoors Door { get; } = new();

            public RackCadWindowViewModel Window { get; }

            public Draft Start(Sys system)
            {
                switch (system)
                {
                    case Sys.Selectivo: Window.StartNewSelective(); return Of(Window.NewSelectiveEditor!);
                    case Sys.Dinamico: Window.StartNewDynamic(); return Of(Window.NewDynamicEditor!);
                    case Sys.Cama: Window.StartNewCama(); return Of(Window.NewCamaEditor!);
                    case Sys.PushBack: Window.StartNewPushBack(); return Of(Window.NewPushBackEditor!);
                    default:
                        // A line that resolves: the defaults of the classic editor do not (CT-05), a design of the library does.
                        var opening = Window.OpenAsNewDraft(RackCadDraftSeed.FromProject(RackProject.ForCantilever(Line()), "de la prueba"));
                        Assert.True(opening.Opened, opening.Reason);
                        return Of(Window.NewCantileverEditor!);
                }
            }
        }

        // The line of RackCadCantileverWindowTests: 3 stations at 96 in, 3 levels, clear 24, W10X33 with a base W12X26 of 48 in, an arm of 36 in.
        private static CantileverLineDesign Line()
        {
            var draft = new CantileverDraftEditor(new CsvStructuralSectionCatalogProvider(CatalogDirectory.Resolve()).Load());
            Assert.Null(draft.Write(CantileverLineField.StationCount, "3"));
            Assert.Null(draft.Write(CantileverLineField.Spacing, "96"));
            Assert.Null(draft.Write(CantileverLineField.LevelCount, "3"));
            Assert.Null(draft.Write(CantileverLineField.ClearHeight, "24"));
            Assert.Null(draft.Write(CantileverLineField.ColumnSection, "AISC-W-W10X33"));
            Assert.Null(draft.Write(CantileverLineField.BaseSection, "AISC-W-W12X26"));
            Assert.Null(draft.Write(CantileverLineField.BaseLength, "48"));
            Assert.Null(draft.WriteArm(null, CantileverArmField.Section, "AISC-HSS-RECT-HSS4X4X_250"));
            Assert.Null(draft.WriteArm(null, CantileverArmField.CutLength, "36"));
            Assert.True(draft.IsResolved);
            return draft.Design;
        }

        private static RackPropertiesRead Read(params (string Id, string Name, string Value)[] entries)
            => new(true, false, null, entries.Select(e => new CustomPropertyDraftEntry(e.Id, e.Name, e.Value)).ToList(),
                Array.Empty<RackPropertiesViewSummary>(), 2, "fp-after");

        private static void Add(RackPropertiesTabViewModel tab, string name, string value)
        {
            tab.AddProperty();
            tab.Properties[^1].Name = name;
            tab.Properties[^1].Value = value;
            tab.CommitProperties();
        }

        // ------------------------------------------------------------------ the tab of a draft

        [Theory]
        [MemberData(nameof(Systems))]
        public void TheTabOfADraftIsEditableAndPartOfItsDraftAndAWrongValueStopsItsInsertion(Sys system)
        {
            var rig = new Rig();
            var draft = rig.Start(system);
            var tab = draft.Tab;

            // VA-S9 resolved: the tab of VA-07, editable, says when its rows are written.
            Assert.True(tab.IsNewRack);
            Assert.True(tab.IsEditable);
            Assert.True(tab.CanAddProperty);
            Assert.Equal(string.Empty, tab.ReadOnlyText);
            Assert.Equal(RackPropertiesTabViewModel.NewRackNote, tab.PropertiesNote);
            Assert.Equal("Propiedades", tab.TabHeader);
            Assert.False(draft.IsDirty());
            Assert.Null(rig.Window.UnwrittenWorkOf(Session)); // ND-05: a draft with nothing changed

            // Its rows are part of the draft: «Modificado», the veto of the close (D-08), and «Descartar» drops them.
            Add(tab, "Zona", "A");
            Assert.True(tab.IsDirty);
            Assert.True(draft.IsDirty());
            Assert.Equal("Sin insertar, con cambios", rig.Window.Drafts.Single().StatusText);
            Assert.Contains("Borrador 1", rig.Window.UnwrittenWorkOf(Session));
            Assert.Equal("Nueva", tab.Properties[0].Tag);
            draft.Discard();
            Assert.Empty(tab.Properties);
            Assert.False(draft.IsDirty());
            Assert.True(tab.IsNewRack);

            // A wrong one stops «Insertar en el dibujo…», which would write them, and says why; nothing is sent.
            Add(tab, "Zona", "A");
            Add(tab, "zona", "B");
            Assert.True(tab.HasProblems);
            Assert.Equal("Propiedades: hay 1 valor no válido.", draft.InsertBlocked());
            draft.Insert();
            Assert.Empty(rig.Door.Writes);

            tab.RemoveProperty(tab.Properties[1]);
            Assert.Equal(string.Empty, draft.InsertBlocked());
        }

        [Theory]
        [MemberData(nameof(Systems))]
        public void InsertarEnElDibujoSendsTheRowsAndTheWrittenRackTakesTheTabOfTheDrawing(Sys system)
        {
            var rig = new Rig();
            var draft = rig.Start(system);
            var tab = draft.Tab;
            Add(tab, "Zona", "A");

            draft.Insert();
            var sent = Assert.Single(rig.Door.Writes);
            Assert.True(sent.NewRack);
            Assert.NotNull(sent.Properties);
            Assert.Null(sent.Properties!.Fingerprint); // nothing was read: only the NUEVO command writes it, over the rack it inserts
            Assert.Null(sent.Properties.UnifiedFrom);
            var entry = Assert.Single(sent.Properties.Entries);
            Assert.Equal(((string?)null, "Zona", "A"), ((string?)entry.Id, entry.Name, entry.Value));
            Assert.False(tab.CanEditProperties); // locked while AutoCAD writes

            // Placed and written: the tab of the rack of the drawing (with the id the create minted), clean.
            rig.Door.Placed(sent, Read((IdA, "Zona", "A")));
            Assert.False(draft.IsNewRack());
            Assert.False(tab.IsNewRack);
            Assert.False(tab.IsDirty);
            Assert.False(draft.IsDirty());
            Assert.True(tab.CanEditProperties);
            Assert.Equal(("Zona", "A"), (tab.Properties[0].Name, tab.Properties[0].Value));
            Assert.Equal(string.Empty, tab.Properties[0].Tag);
            Assert.Empty(rig.Window.Drafts);
            Assert.EndsWith("También escribió sus propiedades.", rig.Window.ActionMessage, StringComparison.Ordinal);

            // From now on «Actualizar» writes them as for any rack of the drawing, over what was read.
            tab.Properties[0].Value = "B";
            tab.CommitProperties();
            draft.Update();
            var update = rig.Door.Writes.Last();
            Assert.False(update.NewRack);
            Assert.Equal("fp-after", update.Properties!.Fingerprint);
            Assert.Equal(IdA, Assert.Single(update.Properties.Entries).Id);
        }

        [Theory]
        [MemberData(nameof(Systems))]
        public void PropertiesThatCouldNotBeWrittenAtTheInsertionStayInTheTabOfTheRackToWriteWithActualizar(Sys system)
        {
            var rig = new Rig();
            var draft = rig.Start(system);
            var tab = draft.Tab;
            Add(tab, "Zona", "A");
            draft.Insert();
            var sent = rig.Door.Writes.Single();

            // The rack is in the drawing, its properties are not: the rows stay, new, over what the rack holds.
            rig.Door.Placed(sent, Read(), "x.");
            Assert.False(draft.IsNewRack());
            Assert.False(tab.IsNewRack);
            Assert.True(tab.IsDirty);
            Assert.True(draft.IsDirty());
            Assert.Equal(("Zona", "A", "Nueva"), (tab.Properties[0].Name, tab.Properties[0].Value, tab.Properties[0].Tag));
            Assert.Contains("Pero sus propiedades no se escribieron: x. Siguen en su pestaña «Propiedades»", rig.Window.ActionMessage, StringComparison.Ordinal);

            draft.Update();
            var update = rig.Door.Writes.Last();
            Assert.False(update.NewRack);
            Assert.Equal("fp-after", update.Properties!.Fingerprint);
            Assert.Equal(("Zona", "A"), (update.Properties.Entries.Single().Name, update.Properties.Entries.Single().Value));
        }

        [Fact]
        public void ARackThatCannotHoldPropertiesSaysWhyAndDoesNotKeepThem()
        {
            var rig = new Rig();
            var draft = rig.Start(Sys.Selectivo);
            Add(draft.Tab, "Zona", "A");
            draft.Insert();
            var readOnly = new RackPropertiesRead(false, false, "Las propiedades de este rack son de solo lectura en esta versión: x", Array.Empty<CustomPropertyDraftEntry>(),
                Array.Empty<RackPropertiesViewSummary>(), 2, "fp-ro");

            rig.Door.Placed(rig.Door.Writes.Single(), readOnly, "x.");
            Assert.False(draft.Tab.IsNewRack);
            Assert.False(draft.Tab.IsEditable);
            Assert.Equal("Las propiedades de este rack son de solo lectura en esta versión: x", draft.Tab.ReadOnlyText);
            Assert.False(draft.IsDirty());
            Assert.EndsWith("Pero sus propiedades no se escribieron: x.", rig.Window.ActionMessage, StringComparison.Ordinal);
        }

        [Theory]
        [MemberData(nameof(Systems))]
        public void ADraftWithNoPropertiesSendsNoneAndTheRackOfTheDrawingGetsItsTab(Sys system)
        {
            var rig = new Rig();
            var draft = rig.Start(system);
            draft.Insert();
            var sent = Assert.Single(rig.Door.Writes);
            Assert.Null(sent.Properties);

            rig.Door.Placed(sent, Read());
            Assert.False(draft.Tab.IsNewRack);
            Assert.True(draft.Tab.IsEditable);
            Assert.False(draft.IsDirty());
            Assert.DoesNotContain("propiedades", rig.Window.ActionMessage, StringComparison.Ordinal); // nothing was written: the message as it was

            // «Actualizar» writes them later over what the drawing held after the insertion.
            Add(draft.Tab, "Zona", "A");
            draft.Update();
            var update = rig.Door.Writes.Last();
            Assert.False(update.NewRack);
            Assert.Equal("fp-after", update.Properties!.Fingerprint);

            // Without a read of what the drawing holds (an answer with none), the tab never takes a base it did not read.
            var other = new Rig();
            var second = other.Start(system);
            second.Insert();
            other.Door.Written(other.Door.Writes.Single(), null);
            Assert.False(second.Tab.IsNewRack);
            Assert.False(second.Tab.IsEditable);
            Assert.Equal("Las propiedades no se leyeron con el editor: para editarlas aquí, vuelve a leerlo del dibujo.", second.Tab.ReadOnlyText);
            Assert.DoesNotContain("RACKPROPIEDADES", second.Tab.ReadOnlyText, StringComparison.Ordinal); // review: the part rl makes it open this window
        }

        // ------------------------------------------------------------------ corrections of the review (on b447675e)

        [Theory]
        [MemberData(nameof(Systems))]
        public void ACancelledInsertionKeepsTheRowsInTheDraftAndTheNextOneSendsThemAgain(Sys system)
        {
            // Point 1 of the review: the jig cancelled (nothing placed) leaves the draft as it was, rows included.
            var rig = new Rig();
            var draft = rig.Start(system);
            var tab = draft.Tab;
            Add(tab, "Zona", "A");
            draft.Insert();
            var first = Assert.Single(rig.Door.Writes);
            Assert.False(tab.CanEditProperties);

            rig.Door.NotPlaced(first, system == Sys.Cama);
            Assert.True(draft.IsNewRack());
            Assert.True(tab.IsNewRack);
            Assert.True(tab.IsEditable);
            Assert.True(tab.CanEditProperties);
            var row = Assert.Single(tab.Properties);
            Assert.Equal(("Zona", "A", "Nueva"), (row.Name, row.Value, row.Tag));
            Assert.True(draft.IsDirty());
            Assert.Equal("Sin insertar, con cambios", Assert.Single(rig.Window.Drafts).StatusText);
            Assert.Contains("Borrador 1", rig.Window.UnwrittenWorkOf(Session));

            // The next «Insertar en el dibujo…» sends them again, with no fingerprint.
            draft.Insert();
            Assert.Equal(2, rig.Door.Writes.Count);
            var second = rig.Door.Writes[1];
            Assert.True(second.NewRack);
            Assert.Equal(first.RackId, second.RackId);
            Assert.Null(second.Properties!.Fingerprint);
            var entry = Assert.Single(second.Properties.Entries);
            Assert.Equal(((string?)null, "Zona", "A"), ((string?)entry.Id, entry.Name, entry.Value));
        }

        [Theory]
        [MemberData(nameof(Systems))]
        public void RowsNotWrittenWhenTheDrawingCouldNotBeReadAgainStayInTheDraftAndActualizarSendsThemOverNoBase(Sys system)
        {
            // M-1 of the review: the rack is in the drawing, its properties are not, and the command could not read it again.
            var rig = new Rig();
            var draft = rig.Start(system);
            var tab = draft.Tab;
            Add(tab, "Zona", "A");
            draft.Insert();
            rig.Door.Placed(rig.Door.Writes.Single(), null, "x.");

            Assert.False(draft.IsNewRack());
            Assert.False(tab.IsNewRack);
            Assert.True(tab.IsEditable);
            Assert.Equal(RackPropertiesRules.NotReadAfterInsertionText, tab.PropertiesNote);
            var row = Assert.Single(tab.Properties);
            Assert.Equal(("Zona", "A", "Nueva"), (row.Name, row.Value, row.Tag));
            Assert.True(tab.IsDirty);
            Assert.True(draft.IsDirty()); // «Modificado»
            Assert.NotNull(rig.Window.UnwrittenWorkOf(Session)); // D-08: the drawing does not close with them
            Assert.Contains("Pero sus propiedades no se escribieron: x." + RackPropertiesRules.NewRackKeptText, rig.Window.ActionMessage, StringComparison.Ordinal);

            // «Actualizar» sends them with no fingerprint: the door writes them only where there is no property yet.
            draft.Update();
            var update = rig.Door.Writes.Last();
            Assert.False(update.NewRack);
            Assert.Null(update.Properties!.Fingerprint);
            var entry = Assert.Single(update.Properties.Entries);
            Assert.Equal(("Zona", "A"), (entry.Name, entry.Value));

            // Written then, the tab goes on from what the drawing holds, clean.
            rig.Door.Written(update, Read((IdA, "Zona", "A")));
            Assert.False(tab.IsDirty);
            Assert.False(draft.IsDirty());
        }

        [Theory]
        [MemberData(nameof(Systems))]
        public void DuplicarCopiesTheRowsOfTheTab(Sys system)
        {
            var rig = new Rig();
            var original = rig.Start(system);
            Add(original.Tab, "Zona", "A");

            var opening = rig.Window.DuplicateDraft(rig.Window.Drafts.Single());
            Assert.True(opening.Opened, opening.Reason);
            var copy = Of(system switch
            {
                Sys.Selectivo => rig.Window.NewSelectiveEditor!,
                Sys.Dinamico => rig.Window.NewDynamicEditor!,
                Sys.Cama => rig.Window.NewCamaEditor!,
                Sys.Cantilever => rig.Window.NewCantileverEditor!,
                _ => (object)rig.Window.NewPushBackEditor!,
            });
            Assert.NotSame(original.Editor, copy.Editor);
            Assert.True(copy.Tab.IsNewRack);
            Assert.Equal(("Zona", "A", "Nueva"), (copy.Tab.Properties.Single().Name, copy.Tab.Properties.Single().Value, copy.Tab.Properties.Single().Tag));
            Assert.True(copy.IsDirty()); // its original was «Modificado» (ND-07)

            // They are its own rows: changing the copy does not change the original.
            copy.Tab.Properties[0].Value = "C";
            copy.Tab.CommitProperties();
            Assert.Equal("A", original.Tab.Properties.Single().Value);
        }

        // ------------------------------------------------------------------ the view (WPF, STA)

        private sealed class BindingErrors : TraceListener
        {
            public List<string> Messages { get; } = new();

            public override void Write(string? message)
            {
            }

            public override void WriteLine(string? message)
            {
                if (message != null) Messages.Add(message);
            }
        }

        private static void Lay(FrameworkElement view)
        {
            view.Measure(new Size(1200, 900));
            view.Arrange(new Rect(0, 0, 1200, 900));
            view.UpdateLayout();
        }

        private static IEnumerable<T> Descendants<T>(DependencyObject root) where T : DependencyObject
        {
            var count = System.Windows.Media.VisualTreeHelper.GetChildrenCount(root);
            for (var i = 0; i < count; i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(root, i);
                if (child is T match) yield return match;
                foreach (var nested in Descendants<T>(child)) yield return nested;
            }
        }

        [Fact]
        public void TheTabOfADraftIsEditableInTheViewOfEveryEditorAndBindsWithNoError()
        {
            StaTestRunner.Run(() =>
            {
                var errors = new BindingErrors();
                PresentationTraceSources.Refresh();
                var source = PresentationTraceSources.DataBindingSource;
                var level = source.Switch.Level;
                source.Listeners.Add(errors);
                source.Switch.Level = SourceLevels.Error;
                try
                {
                    foreach (var system in new[] { Sys.Selectivo, Sys.Dinamico, Sys.Cama, Sys.Cantilever, Sys.PushBack })
                    {
                        var draft = new Rig().Start(system);
                        Add(draft.Tab, "Zona", "A");
                        var (view, tabs, item, tabView) = ViewOf(draft.Editor);
                        Lay(view);
                        Assert.Same(item, tabs.Items.Cast<TabItem>().Last());
                        tabs.SelectedItem = item;
                        Lay(view);

                        Assert.Equal(Visibility.Collapsed, tabView.ReadOnlyText.Visibility);
                        Assert.Equal(RackPropertiesTabViewModel.NewRackNote, tabView.Table.NoteText.Text);
                        Assert.True(tabView.Table.AddButton.IsEnabled, system.ToString());
                        var value = Assert.Single(Descendants<TextBox>(tabView), b => System.Windows.Automation.AutomationProperties.GetName(b) == "Valor de la propiedad 1");
                        Assert.Equal("A", value.Text);
                        Assert.False(value.IsReadOnly);
                    }

                    Assert.Empty(errors.Messages);
                }
                finally
                {
                    source.Listeners.Remove(errors);
                    source.Switch.Level = level;
                }
            });
        }

        private static (FrameworkElement View, TabControl Tabs, TabItem Item, RackPropertiesTabView TabView) ViewOf(object editor)
        {
            switch (editor)
            {
                case SelectiveEditorViewModel s:
                    var selective = new SelectiveEditorView(s);
                    return (selective, selective.Tabs, selective.RackPropertiesTabItem, selective.RackPropertiesView);
                case DynamicEditorViewModel d:
                    var dynamicView = new DynamicEditorView(d);
                    return (dynamicView, dynamicView.Tabs, dynamicView.RackPropertiesTabItem, dynamicView.RackPropertiesView);
                case CamaEditorViewModel c:
                    var cama = new CamaEditorView(c);
                    return (cama, cama.Tabs, cama.RackPropertiesTabItem, cama.RackPropertiesView);
                case CantileverEditorViewModel t:
                    var cantilever = new CantileverEditorView(t);
                    return (cantilever, cantilever.Tabs, cantilever.RackPropertiesTabItem, cantilever.RackPropertiesView);
                case PushBackEditorViewModel p:
                    var pushBack = new PushBackEditorView(p);
                    return (pushBack, pushBack.Tabs, pushBack.RackPropertiesTabItem, pushBack.RackPropertiesView);
                default:
                    throw new ArgumentException("not an editor of a draft", nameof(editor));
            }
        }
    }
}
