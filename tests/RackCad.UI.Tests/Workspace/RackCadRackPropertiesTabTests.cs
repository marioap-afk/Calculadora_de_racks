#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using RackCad.Application.CustomProperties;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.VentanaRackCad;
using RackCad.Application.Workspace;
using RackCad.UI.VentanaRackCad;
using RackCad.UI.Workspace;
using Xunit;
using static RackCad.UI.Tests.SelectiveWindowFixtures;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-64 A-14, cuarta tanda (va), VA-07 (note of the Owner: «Hay que crear una pestaña en cada rack en donde se puedan escribir las
    /// propiedades») and VA-08: the tab «Propiedades» at the end of the editors of Selectivo and Dinámico. Its draft is part of the draft of
    /// the editor, «Descartar» drops it, «Actualizar» sends it with the rack, and views with different properties are unified into it.
    /// </summary>
    public class RackCadRackPropertiesTabTests
    {
        private const string P1 = "6f1c2a3b-0000-4000-8000-000000000001";

        private static RackPropertiesRead Writable(params (string Id, string Name, string Value)[] entries)
            => new(true, false, null, entries.Select(e => new CustomPropertyDraftEntry(e.Id, e.Name, e.Value)).ToList(),
                Array.Empty<RackPropertiesViewSummary>(), 3, "fp-1");

        private static RackPropertiesRead Divergent()
            => new(false, true, null, Array.Empty<CustomPropertyDraftEntry>(), new[]
            {
                new RackPropertiesViewSummary("H1", "Vista frontal", "Zona — B", true, new[] { new CustomPropertyDraftEntry(P1, "Zona", "B") }),
                new RackPropertiesViewSummary("H2", "Vista en planta", "vacío / sin propiedades", false, Array.Empty<CustomPropertyDraftEntry>()),
            }, 2, "fp-2");

        private static SelectiveEditorViewModel Editor(RackPropertiesRead? read = null)
        {
            var editor = SelectiveEditor();
            editor.LoadProperties(read ?? Writable((P1, "Zona", "A")));
            return editor;
        }

        [Fact]
        public void ThePropertiesOfTheRackArePartOfTheDraftOfTheEditor()
        {
            var editor = Editor();
            var tab = editor.PropertiesTab;
            Assert.False(editor.IsDirty);
            Assert.Null(editor.PropertiesWrite());
            Assert.Equal("Nombre y valor de texto guardados en las 3 vistas del rack. Van al borrador del rack y se escriben con su «Actualizar», junto con el resto del editor.",
                tab.PropertiesNote);

            tab.Properties[0].Value = "B";
            tab.CommitProperties();
            Assert.True(tab.IsDirty);
            Assert.True(editor.IsDirty);
            Assert.True(editor.CanUpdate, editor.UpdateBlockedText);
            Assert.Equal("Modificada", tab.Properties[0].Tag);
            var write = editor.PropertiesWrite()!;
            Assert.Equal("fp-1", write.Fingerprint);
            Assert.Equal("B", Assert.Single(write.Entries).Value);
            Assert.Null(write.UnifiedFrom);

            // «Descartar» of the editor drops them.
            editor.Discard();
            Assert.False(tab.IsDirty);
            Assert.False(editor.IsDirty);
            Assert.Equal("A", tab.Properties[0].Value);
        }

        [Fact]
        public void AWrongPropertyStopsActualizarAndSaysWhy()
        {
            var editor = Editor();
            var tab = editor.PropertiesTab;
            tab.AddProperty();
            tab.Properties[1].Name = "zona";
            tab.CommitProperties();

            Assert.True(tab.HasProblems);
            Assert.Equal("Ya existe otra propiedad llamada «zona».", tab.Properties[1].NameError);
            Assert.Equal("Propiedades (con problemas)", tab.TabHeader);
            Assert.True(editor.IsDirty);
            Assert.False(editor.CanUpdate);
            Assert.Equal("Propiedades: hay 1 valor no válido.", editor.UpdateBlockedText);
            Assert.Null(editor.PropertiesWrite());
        }

        [Fact]
        public void ViewsWithDifferentPropertiesAreUnifiedIntoTheDraftAfterChoosingAndConfirming()
        {
            // VA-08: no view comes chosen; the box only counts once a source is chosen; the unified draft can be retouched.
            var editor = Editor(Divergent());
            var tab = editor.PropertiesTab;
            Assert.True(tab.IsDivergent);
            Assert.False(tab.IsEditable);
            Assert.Equal("Elige la vista origen: ninguna viene elegida.", tab.UnifyWhy);
            tab.Confirmed = true;
            Assert.False(tab.Confirmed);

            tab.ChooseSource(tab.Views[1]); // cannot be a source
            Assert.False(tab.CanConfirm);
            tab.ChooseSource(tab.Views[0]);
            Assert.True(tab.Views[0].IsChosen);
            Assert.False(tab.CanUnify);
            tab.Confirmed = true;
            Assert.True(tab.CanUnify);

            tab.Unify();
            Assert.False(tab.IsDivergent);
            Assert.True(tab.IsEditable);
            Assert.Equal("B", Assert.Single(tab.Properties).Value);
            Assert.Equal("Unificado en el borrador desde «Vista frontal». Puedes retocarlo; «Actualizar» lo escribe en las 2 vistas.", tab.UnifiedText);
            Assert.True(editor.IsDirty);
            Assert.Equal("H1", editor.PropertiesWrite()!.UnifiedFrom);
        }

        [Fact]
        public void TheEditorOfDinamicoHasTheSameTab()
        {
            var editor = new DynamicEditorViewModel(DynamicDraftEditor.CreateNew(UiSupport.LoadCatalogSafe()), "DIN-1", _ => { });
            Assert.False(editor.IsDirty);
            editor.LoadProperties(Writable());
            editor.PropertiesTab.AddProperty();
            editor.PropertiesTab.Properties[0].Name = "Zona";
            editor.PropertiesTab.CommitProperties();

            Assert.True(editor.IsDirty);
            Assert.True(editor.CanUpdate, editor.UpdateBlockedText);
            Assert.Equal("Zona", Assert.Single(editor.PropertiesWrite()!.Entries).Name);
            editor.Discard();
            Assert.False(editor.IsDirty);
        }

        // ------------------------------------------------------------------ the window

        private sealed class Door : IRackCadSelectiveDoorPort
        {
            public List<RackDoorRequest> SelectiveReads { get; } = new();

            public List<SelectiveDoorWrite> SelectiveWrites { get; } = new();

            public event Action<RackDesignRead>? DesignRead { add { } remove { } }

            public event Action<RackDesignWritten>? DesignWritten;

            public event Action<long>? RequestStarted { add { } remove { } }

            public event Action<SelectiveDesignRead>? SelectiveRead;

            public RackCadClassicResult RequestDesign(RackDoorRequest request) => RackCadClassicResult.Started();

            public RackCadClassicResult RequestDynamicUpdate(RackDoorWrite write) => RackCadClassicResult.Started();

            public RackCadClassicResult RequestSelectiveDesign(RackDoorRequest request)
            {
                SelectiveReads.Add(request);
                return RackCadClassicResult.Started();
            }

            public RackCadClassicResult RequestSelectiveUpdate(SelectiveDoorWrite write)
            {
                SelectiveWrites.Add(write);
                return RackCadClassicResult.Started();
            }

            public void Abandon(long requestId)
            {
            }

            public void Read(RackPropertiesRead properties)
                => SelectiveRead?.Invoke(SelectiveWindowFixtures.Read(SelectiveReads.Last().RequestId, RackId) with { Properties = properties });

            public void Written(RackDesignWritten written) => DesignWritten?.Invoke(written);
        }

        private static (RackCadWindowViewModel Window, Door Door, SelectiveEditorViewModel Editor) OpenInWindow()
        {
            var host = new WorkspacePortDouble { Snapshot = WorkspacePanelFixtures.Ready(session: 7) };
            var summary = new WorkspaceBrowserSummaryPortDouble
            {
                ListResponder = (_, _) => BrowserRackList.Ready(new[] { new BrowserRackListEntry("SEL-1", "Selectivo A", "selective", new[] { "frontal" }) }),
            };
            var panel = new WorkspacePanelViewModel(host, _ => { }, new WorkspaceNavigationPortDouble(), summary, new WorkspacePilotEditPortDouble());
            var door = new Door();
            var window = new RackCadWindowViewModel(panel, null, null, door, UiSupport.LoadCatalogSafe);
            window.Refresh();
            window.OpenRack("SEL-1");
            window.EditSelectiveHere();
            door.Read(Writable((P1, "Zona", "A")));
            return (window, door, window.ActiveSelectiveEditor!);
        }

        [Fact]
        public void TheWindowGivesTheEditorThePropertiesItReadAndSendsTheDraftWithActualizar()
        {
            var (_, door, editor) = OpenInWindow();
            Assert.Equal("Zona", Assert.Single(editor.PropertiesTab.Properties).Name);

            editor.PropertiesTab.Properties[0].Value = "C";
            editor.PropertiesTab.CommitProperties();
            editor.RequestWrite();
            var write = Assert.Single(door.SelectiveWrites);
            Assert.Equal("C", Assert.Single(write.Properties!.Entries).Value);
            Assert.False(editor.PropertiesTab.CanEditProperties);

            // Written, with what the drawing holds now: the tab goes on from it, clean.
            door.Written(new RackDesignWritten(write.RequestId, RackId, true, "Escrito. También escribió sus propiedades.",
                Properties: Writable((P1, "Zona", "C"))));
            Assert.False(editor.PropertiesTab.IsDirty);
            Assert.False(editor.IsDirty);
            Assert.Equal("C", editor.PropertiesTab.Properties[0].Value);
            Assert.True(editor.PropertiesTab.CanEditProperties);
        }

        [Fact]
        public void PropertiesThatCouldNotBeWrittenStayInTheDraft()
        {
            var (_, door, editor) = OpenInWindow();
            editor.PropertiesTab.Properties[0].Value = "C";
            editor.PropertiesTab.CommitProperties();
            editor.RequestWrite();
            var write = door.SelectiveWrites.Single();

            door.Written(new RackDesignWritten(write.RequestId, RackId, true, "Escrito. Pero sus propiedades no se escribieron: x", PropertiesError: "x"));
            Assert.True(editor.PropertiesTab.IsDirty);
            Assert.True(editor.IsDirty);
            Assert.Equal("C", editor.PropertiesTab.Properties[0].Value);
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
        public void TheTabIsTheLastOneOfBothEditorsAndBindsWithNoError()
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
                    var selective = new SelectiveEditorView(Editor());
                    Lay(selective);
                    var item = selective.Tabs.Items.Cast<TabItem>().Last();
                    Assert.Same(selective.RackPropertiesTabItem, item);
                    Assert.Equal("Propiedades", item.Header);
                    selective.Tabs.SelectedItem = item;
                    Lay(selective);
                    Assert.Contains(Descendants<TextBox>(selective.RackPropertiesView),
                        b => System.Windows.Automation.AutomationProperties.GetName(b) == "Valor de la propiedad 1" && b.Text == "A" && b.AcceptsReturn);

                    var divergent = new SelectiveEditorView(Editor(Divergent()));
                    Lay(divergent);
                    divergent.Tabs.SelectedItem = divergent.RackPropertiesTabItem;
                    Lay(divergent);
                    Assert.Equal(Visibility.Visible, divergent.RackPropertiesView.DivergentPanel.Visibility);
                    Assert.Equal(2, Descendants<RadioButton>(divergent.RackPropertiesView.ViewsList).Count());

                    var dynamicEditor = new DynamicEditorViewModel(DynamicDraftEditor.CreateNew(UiSupport.LoadCatalogSafe()), "DIN-1", _ => { });
                    dynamicEditor.LoadProperties(Writable((P1, "Zona", "A")));
                    var dynamicView = new DynamicEditorView(dynamicEditor);
                    Lay(dynamicView);
                    Assert.Equal("Propiedades", dynamicView.Tabs.Items.Cast<TabItem>().Last().Header);
                    dynamicView.Tabs.SelectedItem = dynamicView.RackPropertiesTabItem;
                    Lay(dynamicView);
                    Assert.Empty(errors.Messages);
                }
                finally
                {
                    source.Listeners.Remove(errors);
                    source.Switch.Level = level;
                }
            });
        }
    }
}
