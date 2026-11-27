#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using RackCad.Application.StructuralSections;
using RackCad.Application.Systems.Larguero;
using RackCad.Application.VentanaRackCad;
using RackCad.Application.Workspace;
using RackCad.UI.VentanaRackCad;
using RackCad.UI.Workspace;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-64 A-14, line D, quinta tanda, part «lg» (LG-01 to LG-03 of the Owner): «Nuevo › Larguero» of the RackCad window, a draft in
    /// construction. A larguero is made from a section and a ménsula: the section is chosen in ONE page with a filter by catalog (the largueros
    /// of RackCad and the AISC sections) and by type, and is called by its type; then the peralte, the length to cut and the ménsula, and the
    /// summary. Nothing is saved or written to the drawing, and no command leaves the page but «Abrir el editor clásico», a gesture of the user.
    /// The view-model over the real catalogs, and the views with real WPF (STA) and without binding errors.
    /// </summary>
    public class RackCadLargueroWorkbenchTests
    {
        private sealed class ClassicDouble : IRackCadClassicCommandPort
        {
            public List<RackCadClassicAction> Runs { get; } = new();

            public RackCadClassicResult Run(RackCadClassicAction action)
            {
                Runs.Add(action);
                return RackCadClassicResult.Started();
            }
        }

        private sealed class Rig
        {
            public Rig(Func<StructuralSectionCatalog>? sections = null)
            {
                Host = new WorkspacePortDouble { Snapshot = WorkspacePanelFixtures.Ready(session: 7) };
                Summary = new WorkspaceBrowserSummaryPortDouble
                {
                    ListResponder = (_, _) => BrowserRackList.Ready(new List<BrowserRackListEntry>
                    {
                        new BrowserRackListEntry("SEL-1", "Selectivo A", "selective", new[] { "frontal" }),
                    }),
                };
                Panel = new WorkspacePanelViewModel(Host, _ => { }, new WorkspaceNavigationPortDouble(), Summary, new WorkspacePilotEditPortDouble());
                Window = new RackCadWindowViewModel(Panel, Classic, null, null, UiSupport.LoadCatalogSafe);
                if (sections != null) Window.LargueroSectionsLoader = sections;
                Window.Refresh();
            }

            public WorkspacePortDouble Host { get; }

            public WorkspaceBrowserSummaryPortDouble Summary { get; }

            public WorkspacePanelViewModel Panel { get; }

            public ClassicDouble Classic { get; } = new();

            public RackCadWindowViewModel Window { get; }

            public LargueroWorkbenchViewModel Open()
            {
                Window.OpenLargueroWorkbench();
                Assert.True(Window.LargueroWorkbench != null, Window.ActionMessage);
                return Window.LargueroWorkbench!;
            }
        }

        private static StructuralSectionCatalog Aisc() => new CsvStructuralSectionCatalogProvider(RackCad.Application.Catalogs.CatalogDirectory.Resolve()).Load();

        private static LargueroSectionRow Row(LargueroWorkbenchViewModel workbench, string name) => workbench.Rows.Single(r => r.IsSection && r.Label == name);

        // ------------------------------------------------------------------ the page under «Nuevo»

        [Fact]
        public void TheCardOfNuevoOpensThePageUnderNuevoWithTheDefaultsOfTheClassicEditor()
        {
            var rig = new Rig();
            Assert.Null(rig.Window.LargueroWorkbench); // the catalogs are read only when the page opens

            var workbench = rig.Open();

            Assert.True(rig.Window.IsLargueroPage);
            Assert.False(rig.Window.IsNuevoPage);
            Assert.False(rig.Window.ShowNuevoCards);
            Assert.False(rig.Window.ShowDraftsBar);
            // The side column marks «Nuevo»: the page is «Nuevo › Larguero», not a section of its own.
            Assert.Equal(new[] { RackCadWindowPage.Nuevo }, rig.Window.Sections.Where(s => s.IsActive).Select(s => s.Page).ToArray());
            Assert.Equal(new[] { "Nuevo", "Variables", "BOM y lista de racks", "Biblioteca de diseños", "Configuración" },
                rig.Window.Sections.Select(s => s.Label).ToArray());

            // The defaults of the classic editor: the first larguero, peralte 3, 96 in and its ménsula.
            Assert.Equal("rackcad", workbench.SelectedCatalog);
            Assert.Equal("LARGUERO_ESCALON_CAL14_3_REMACHES", workbench.Section!.Id);
            Assert.Equal("Escalón calibre 14", workbench.SelectedRow!.Label);
            Assert.Equal("3", workbench.SelectedPeralte);
            Assert.Equal("96", workbench.LengthText);
            Assert.Equal("MENSULA_3_REMACHES_CAL_10", workbench.SelectedMensula);
            Assert.Equal("Larguero 96 in · P3", workbench.ProposedName);
            Assert.True(workbench.IsComplete);
            Assert.StartsWith("El larguero está completo. Esta página todavía no lo guarda ni lo dibuja", workbench.CompleteText);
            Assert.Equal(string.Empty, workbench.CatalogNotice);

            // «Nuevo» of the crumb goes back to its home; the page keeps what was chosen while the window lives.
            workbench.LengthText = "120";
            workbench.GoHome();
            Assert.True(rig.Window.IsNuevoPage);
            Assert.True(rig.Window.ShowNuevoCards);
            Assert.False(rig.Window.IsLargueroPage);
            Assert.Same(workbench, rig.Open());
            Assert.Equal("120", workbench.LengthText);
            Assert.Equal("Larguero 120 in · P3", workbench.ProposedName);
        }

        [Fact]
        public void ALargueroIsNotADraftOfTheDrawingAndNothingLeavesThePageButTheClassicEditor()
        {
            var rig = new Rig();
            var header = rig.Window.HeaderText;
            var workbench = rig.Open();

            workbench.SelectedCatalog = "aisc";
            workbench.SelectedType = "Canal";
            workbench.SelectedRow = Row(workbench, "C10X15.3");
            workbench.SelectedMensula = "MENSULA_TROQUEL_REDONDO_CAL_10";
            workbench.LengthText = "144";

            // Not a «Borrador N»: the drafts, the header and the close of the drawing do not see it (LG-S01).
            Assert.Empty(rig.Window.Drafts);
            Assert.Equal(header, rig.Window.HeaderText);
            Assert.Empty(rig.Classic.Runs);
            Assert.False(rig.Window.IsWaitingForAutoCad);

            // «Abrir el editor clásico»: the classic card «Larguero» (RACKNUEVO Larguero), the one that saves a larguero today.
            workbench.OpenClassicEditor();
            Assert.Equal(new[] { RackCadClassicAction.NewLarguero }, rig.Classic.Runs);
            Assert.StartsWith("Se pidió a AutoCAD abrir el editor.", rig.Window.ActionMessage);
        }

        [Fact]
        public void ThePageNeedsNoDrawingAndStaysWhenTheDrawingChanges()
        {
            var rig = new Rig();
            rig.Host.Snapshot = WorkspacePanelFixtures.NoDocument();
            rig.Window.Refresh();

            var workbench = rig.Open();
            Assert.True(rig.Window.IsLargueroPage);
            Assert.True(workbench.IsComplete);
            workbench.LengthText = "72";

            rig.Host.Snapshot = WorkspacePanelFixtures.Ready(session: 9);
            rig.Window.Refresh();
            Assert.True(rig.Window.IsLargueroPage);
            Assert.Same(workbench, rig.Window.LargueroWorkbench);
            Assert.Equal("Larguero 72 in · P3", workbench.ProposedName);
        }

        // ------------------------------------------------------------------ LG-02: the filter by catalog and type

        [Fact]
        public void TheSectionIsChosenWithTheFilterByCatalogAndType()
        {
            var workbench = new Rig().Open();

            Assert.Equal(new[] { "", "rackcad", "aisc" }, workbench.CatalogOptions.Select(o => o.Key).ToArray());
            var all = workbench.Sections.All.Count;
            Assert.Equal("Todos los catálogos (" + all + ")", workbench.CatalogOptions[0].Label);
            Assert.Equal("Largueros de RackCad (8)", workbench.CatalogOptions[1].Label);

            // The largueros of RackCad, by type, each called by its type.
            Assert.Equal(new[] { "", "Escalón", "Tubo", "Canal" }, workbench.TypeOptions.Select(o => o.Key).ToArray());
            Assert.Equal("Escalón (6)", workbench.TypeOptions[1].Label);
            Assert.Equal(new[] { "Escalón", "Tubo", "Canal" }, workbench.Rows.Where(r => r.IsHeader).Select(r => r.Label).ToArray());
            Assert.Equal("Mostrando 8 de " + all + " secciones", workbench.ShownText);
            Assert.Equal("Largueros de RackCad · peraltes 3 a 6 in · calibre 14 · con Ménsula 3 remaches de cinta calibre 10",
                Row(workbench, "Escalón calibre 14").Detail);

            // The AISC sections: the type keeps when the catalog has it, and the list follows.
            workbench.SelectedType = "Canal";
            Assert.Equal(new[] { "Canal", "In n out de canal de 6\"" }, workbench.Rows.Select(r => r.Label).ToArray());
            workbench.SelectedCatalog = "aisc";
            Assert.Equal("Canal", workbench.SelectedType);
            Assert.Equal(new[] { "", "Tubo", "Canal", "Ángulo", "Viga W", "Viga S (IPS)" }, workbench.TypeOptions.Select(o => o.Key).ToArray());
            Assert.All(workbench.Rows.Where(r => r.IsSection), r => Assert.StartsWith("C", r.Label, StringComparison.Ordinal));
            Assert.Equal("Perfiles AISC · 15.3 lb/ft (22.8 kg/m) · peralte 10 in", Row(workbench, "C10X15.3").Detail);

            // A type the catalog does not have goes back to every type.
            workbench.SelectedCatalog = "rackcad";
            workbench.SelectedType = "Tubo";
            workbench.SelectedCatalog = "aisc";
            Assert.Equal("Tubo", workbench.SelectedType); // HSS
            workbench.SelectedType = "Ángulo";
            workbench.SelectedCatalog = "rackcad";
            Assert.Equal("", workbench.SelectedType);

            // The search, without case, accents or separators.
            workbench.SearchText = "ESCALON INVERTIDO";
            Assert.Equal(new[] { "Escalón", "Escalón invertido calibre 14", "Escalón invertido con ángulo" }, workbench.Rows.Select(r => r.Label).ToArray());
            workbench.SearchText = "nada que coincida";
            Assert.Empty(workbench.Rows);
            Assert.Equal("Ninguna sección coincide con el filtro.", workbench.NoMatchText);
        }

        [Fact]
        public void AFilterThatHidesTheSectionKeepsItChosenAndSaysIt()
        {
            var workbench = new Rig().Open();
            workbench.SelectedRow = Row(workbench, "Tubo 1.9\"");

            workbench.SelectedType = "Escalón";

            Assert.Equal("LARGUERO_TUBO_1_9_3_REMACHES", workbench.Section!.Id);
            Assert.Null(workbench.SelectedRow);
            Assert.Equal("La sección elegida, «Tubo 1.9\"», no coincide con el filtro: sigue elegida.", workbench.OutsideFilterText);
            Assert.Equal("Larguero 96 in · P1.9", workbench.ProposedName);

            // A null from the list (its items changed) and a header are not choices.
            workbench.SelectedRow = null;
            workbench.SelectedRow = workbench.Rows.First(r => r.IsHeader);
            Assert.Equal("LARGUERO_TUBO_1_9_3_REMACHES", workbench.Section!.Id);

            workbench.SelectedType = "";
            Assert.Equal("Tubo 1.9\"", workbench.SelectedRow!.Label);
            Assert.Equal(string.Empty, workbench.OutsideFilterText);
        }

        // ------------------------------------------------------------------ the peralte, the length and the ménsula

        [Fact]
        public void AnAiscSectionHasItsDepthAsThePeralteAndNoMensulaUntilOneIsChosen()
        {
            var workbench = new Rig().Open();
            workbench.SelectedCatalog = "aisc";
            workbench.SelectedType = "Canal";

            workbench.SelectedRow = Row(workbench, "C10X15.3");

            Assert.False(workbench.HasPeralteChoice);
            Assert.True(workbench.ShowFixedPeralte);
            Assert.Equal("10 in: el peralte de la sección, del catálogo AISC.", workbench.PeralteHint);
            Assert.Null(workbench.SelectedMensula);
            Assert.Equal("Una sección AISC no trae ménsula en el catálogo: elígela.", workbench.MensulaHint);
            Assert.False(workbench.IsComplete);
            Assert.Equal("Falta: " + LargueroDraftSummary.AiscMensulaText, workbench.MissingText);
            Assert.Equal("Larguero 96 in · C10X15.3", workbench.ProposedName);
            Assert.Contains(workbench.SummaryLines, l => l.Label == "Peso del perfil" && l.Value.StartsWith("15.3 lb/ft (22.8 kg/m) · 122.4 lb", StringComparison.Ordinal));

            workbench.SelectedMensula = "MENSULA_AJUSTE_INFINITO_CAL_10";
            Assert.True(workbench.IsComplete);
            Assert.Equal(string.Empty, workbench.MissingText);
            Assert.Equal(new[] { "Larguero", "Perfil", "Ménsula" }, workbench.BomLines.Select(b => b.Component).ToArray());
            Assert.Equal("2 por larguero", workbench.BomLines.Last().QuantityText);
        }

        [Fact]
        public void TheMensulaFollowsTheSectionUntilTheUserChoosesOneAndThePeralteStaysWhenItCan()
        {
            var workbench = new Rig().Open();
            workbench.SelectedPeralte = "4.5";

            // «Escalón troquel redondo»: its ménsula is the troquel redondo; the peralte 4.5 is one of its own and stays.
            workbench.SelectedRow = Row(workbench, "Escalón troquel redondo");
            Assert.Equal("MENSULA_TROQUEL_REDONDO_CAL_10", workbench.SelectedMensula);
            Assert.Equal("4.5", workbench.SelectedPeralte);
            Assert.EndsWith("Es la de su larguero en el catálogo.", workbench.MensulaHint);
            Assert.Contains(workbench.MensulaOptions, o => o.Key == "MENSULA_TROQUEL_REDONDO_CAL_10" && o.Label.EndsWith("(la de su larguero)", StringComparison.Ordinal));

            // «Tubo 1.9"» has one peralte: it takes it.
            workbench.SelectedRow = Row(workbench, "Tubo 1.9\"");
            Assert.Equal(new[] { "1.9" }, workbench.PeralteOptions.ToArray());
            Assert.Equal("1.9", workbench.SelectedPeralte);
            Assert.Equal("Este larguero tiene un solo peralte en el catálogo.", workbench.PeralteHint);
            Assert.Equal("MENSULA_3_REMACHES_CAL_10", workbench.SelectedMensula);

            // Once the user chooses a ménsula, a new section keeps it (LG-01: «sería elegir la ménsula»).
            workbench.SelectedMensula = "MENSULA_AJUSTE_INFINITO_CAL_10";
            workbench.SelectedRow = Row(workbench, "Escalón calibre 14");
            Assert.Equal("MENSULA_AJUSTE_INFINITO_CAL_10", workbench.SelectedMensula);
            Assert.DoesNotContain("Es la de su larguero", workbench.MensulaHint, StringComparison.Ordinal);
            Assert.Equal("3", workbench.SelectedPeralte); // 1.9 is not one of its peraltes: its first one

            // Values that are not in the lists are not choices.
            workbench.SelectedPeralte = "7";
            workbench.SelectedMensula = "NO_EXISTE";
            Assert.Equal("3", workbench.SelectedPeralte);
            Assert.Equal("MENSULA_AJUSTE_INFINITO_CAL_10", workbench.SelectedMensula);
        }

        [Theory]
        [InlineData("")]
        [InlineData("abc")]
        [InlineData("0")]
        [InlineData("-12")]
        public void ALengthThatIsNotANumberGreaterThanZeroIsMarkedAndStopsTheLarguero(string text)
        {
            var workbench = new Rig().Open();

            workbench.LengthText = text;

            Assert.True(workbench.HasLengthError);
            Assert.Equal("Escribe un número mayor que 0 (pulgadas).", workbench.LengthError);
            Assert.False(workbench.IsComplete);
            Assert.Equal("Falta: " + LargueroDraftSummary.NoLengthText, workbench.MissingText);
            Assert.Equal("Larguero sin terminar", workbench.ProposedName);
            Assert.Equal("falta", workbench.SummaryLines.Single(l => l.Label == "Longitud a corte").Value);

            workbench.LengthText = "100.5";
            Assert.False(workbench.HasLengthError);
            Assert.Equal("Larguero 100.5 in · P3", workbench.ProposedName);
        }

        [Fact]
        public void ResetGoesBackToTheDefaults()
        {
            var workbench = new Rig().Open();
            workbench.SelectedCatalog = "aisc";
            workbench.SelectedType = "Viga W";
            workbench.SearchText = "w10";
            workbench.SelectedRow = workbench.Rows.First(r => r.IsSection);
            workbench.SelectedMensula = "MENSULA_AJUSTE_INFINITO_CAL_10";
            workbench.LengthText = "x";

            workbench.ResetToDefaults();

            Assert.Equal("rackcad", workbench.SelectedCatalog);
            Assert.Equal("", workbench.SelectedType);
            Assert.Equal("", workbench.SearchText);
            Assert.Equal("LARGUERO_ESCALON_CAL14_3_REMACHES", workbench.Section!.Id);
            Assert.Equal("MENSULA_3_REMACHES_CAL_10", workbench.SelectedMensula);
            Assert.Equal("96", workbench.LengthText);
            Assert.True(workbench.IsComplete);

            // After the reset the ménsula follows the section again.
            workbench.SelectedRow = Row(workbench, "Escalón troquel redondo");
            Assert.Equal("MENSULA_TROQUEL_REDONDO_CAL_10", workbench.SelectedMensula);
        }

        [Fact]
        public void TheRowsHaveUniqueNamesWithTheirPositionAndAHeaderPerType()
        {
            var workbench = new Rig().Open();

            var sections = workbench.Rows.Where(r => r.IsSection).ToList();
            Assert.Equal(8, sections.Count);
            Assert.Equal(workbench.Rows.Count, workbench.Rows.Select(r => r.AccessibleName).Distinct().Count());
            Assert.Equal("Sección 1 de 8: Escalón calibre 14, Escalón, Largueros de RackCad", sections[0].AccessibleName);
            Assert.Equal("Tipo Escalón: 6 secciones", workbench.Rows[0].AccessibleName);
            Assert.Equal("Tipo Tubo: 1 sección", workbench.Rows.Single(r => r.IsHeader && r.Label == "Tubo").AccessibleName);
        }

        // ------------------------------------------------------------------ the catalogs that fail

        [Fact]
        public void AnAiscCatalogThatFailsIsSaidAndOnlyTheLarguerosOfRackCadAreOffered()
        {
            var rig = new Rig(() => throw new StructuralSectionCatalogException(null, "el manifiesto no coincide"));
            var workbench = rig.Open();

            Assert.True(workbench.HasCatalogNotice);
            Assert.Equal("El catálogo de perfiles AISC no se pudo leer (el manifiesto no coincide): solo se ofrecen los largueros de RackCad.",
                workbench.CatalogNotice);
            Assert.Equal("Perfiles AISC (0)", workbench.CatalogOptions[2].Label);
            Assert.Equal(8, workbench.Sections.All.Count);
            Assert.True(workbench.IsComplete);
            Assert.Equal(string.Empty, rig.Window.ActionMessage); // the page says it, the window keeps working
        }

        [Fact]
        public void ARackCatalogWithoutLarguerosOrMensulasIsSaid()
        {
            var workbench = new LargueroWorkbenchViewModel(() => new RackCad.Application.Catalogs.RackCatalog(), () => null);

            Assert.Equal("El catálogo de perfiles AISC no está disponible: solo se ofrecen los largueros de RackCad. "
                         + "El catálogo de RackCad no trae largueros (secciones.csv, rol LARGUERO). "
                         + "El catálogo de RackCad no trae ménsulas (mensulas.csv): no hay ménsula que elegir.", workbench.CatalogNotice);
            Assert.Null(workbench.Section);
            Assert.Empty(workbench.Rows);
            Assert.Equal("", workbench.SelectedCatalog);
            Assert.Equal("Falta: " + string.Join(" ", LargueroDraftSummary.NoSectionText, LargueroDraftSummary.NoMensulaText), workbench.MissingText);
        }

        [Fact]
        public void AFailureOfAGestureIsSaidInTheMessageOfTheWindow()
        {
            var rig = new Rig();
            var workbench = rig.Open();

            workbench.Contained(() => throw new InvalidOperationException("prueba"));

            Assert.Equal("La ventana tuvo un fallo contenido: prueba", rig.Window.ActionMessage);
        }

        // ------------------------------------------------------------------ the views, with real WPF

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

        private static void WithBindingErrors(Action body)
        {
            var errors = new BindingErrors();
            PresentationTraceSources.Refresh();
            var source = PresentationTraceSources.DataBindingSource;
            var level = source.Switch.Level;
            source.Listeners.Add(errors);
            source.Switch.Level = SourceLevels.Error;
            try
            {
                body();
            }
            finally
            {
                source.Listeners.Remove(errors);
                source.Switch.Level = level;
            }

            Assert.Empty(errors.Messages);
        }

        private static void Lay(FrameworkElement view, double width = 1200)
        {
            view.Measure(new Size(width, 900));
            view.Arrange(new Rect(0, 0, width, 900));
            view.UpdateLayout();
        }

        private static void Click(Button button) => button.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));

        private static string Name(UIElement element) => UIElementAutomationPeer.CreatePeerForElement(element).GetName();

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

        private static bool Shown(UIElement element)
        {
            for (DependencyObject? node = element; node != null; node = System.Windows.Media.VisualTreeHelper.GetParent(node))
                if (node is UIElement ui && ui.Visibility != Visibility.Visible) return false;
            return true;
        }

        private static Button ButtonNamed(DependencyObject root, string name)
            => Descendants<Button>(root).Single(b => Shown(b) && Name(b) == name);

        private static List<string> ShownTexts(DependencyObject root) => Descendants<TextBlock>(root).Where(Shown).Select(t => t.Text).ToList();

        [Fact]
        public void TheCardOfNuevoOpensThePageAndThePageChoosesTheSectionByCatalogAndType()
        {
            StaTestRunner.Run(() => WithBindingErrors(() =>
            {
                var rig = new Rig();
                var view = new RackCadWindowView(rig.Window);
                rig.Window.ShowPage(RackCadWindowPage.Nuevo);
                Lay(view);
                Assert.Equal(Visibility.Collapsed, view.LargueroPageHost.Visibility);

                // «Nuevo»: the card, under «Componentes en esta ventana», says it is in construction.
                Assert.Contains("Componentes en esta ventana", ShownTexts(view.NuevoPage));
                Click(ButtonNamed(view.NuevoPage, "Larguero en esta ventana, en construcción"));
                Lay(view);

                Assert.True(rig.Window.IsLargueroPage);
                Assert.Equal(Visibility.Visible, view.LargueroPageHost.Visibility);
                Assert.Equal(Visibility.Collapsed, view.NuevoPage.Visibility);
                var page = view.LargueroPage;
                var workbench = rig.Window.LargueroWorkbench!;
                Assert.Same(workbench, page.DataContext);

                // LG-03: what the page is, at the top.
                var texts = ShownTexts(page);
                Assert.Contains("En construcción", texts);
                Assert.Contains(LargueroWorkbenchViewModel.WipTitle, texts);
                Assert.Contains(LargueroWorkbenchViewModel.WipText, texts);
                Assert.Contains("Larguero 96 in · P3", texts);
                Assert.Equal(Visibility.Collapsed, page.LargueroCatalogNoticePanel.Visibility);

                // The filter: «Perfiles AISC», «Canal», then a channel of the list.
                page.LargueroCatalogFilter.SelectedValue = "aisc";
                page.LargueroTypeFilter.SelectedValue = "Canal";
                Lay(view);
                Assert.Equal("aisc", workbench.SelectedCatalog);
                Assert.Equal("Canal", workbench.SelectedType);
                Assert.Same(workbench.Rows, page.LargueroSectionList.ItemsSource);
                page.LargueroSectionList.SelectedItem = Row(workbench, "C10X15.3");
                Lay(view);
                Assert.Equal("C10X15.3", workbench.Section!.Name);
                Assert.Equal(Visibility.Collapsed, page.LargueroPeralteBox.Visibility); // the depth of the section, not a choice
                Assert.Equal("10 in: el peralte de la sección, del catálogo AISC.", page.LargueroPeralteHint.Text);
                Assert.Equal("Falta: " + LargueroDraftSummary.AiscMensulaText, page.LargueroMissingText.Text);
                Assert.True(Shown(page.LargueroMissingText));

                // The ménsula, then the larguero is complete.
                page.LargueroMensulaBox.SelectedValue = "MENSULA_3_REMACHES_CAL_10";
                Lay(view);
                Assert.Equal("MENSULA_3_REMACHES_CAL_10", workbench.SelectedMensula);
                Assert.False(Shown(page.LargueroMissingText));
                Assert.Contains("Larguero 96 in · C10X15.3", ShownTexts(page));

                // A length that is not valid is marked in its field.
                page.LargueroLengthBox.Text = "abc";
                Lay(view);
                Assert.Equal("Escribe un número mayor que 0 (pulgadas).", page.LargueroLengthError.Text);
                Assert.True(Shown(page.LargueroLengthError));

                // The rows of the list: a header per type, not a choice; every section with its position.
                var containers = Enumerable.Range(0, page.LargueroSectionList.Items.Count)
                    .Select(i => page.LargueroSectionList.ItemContainerGenerator.ContainerFromIndex(i) as ListBoxItem)
                    .Where(c => c != null).Cast<ListBoxItem>().ToList();
                Assert.NotEmpty(containers);
                Assert.False(containers[0].IsEnabled);
                Assert.Equal("Tipo Canal: " + workbench.Rows[0].Detail, Name(containers[0]));
                Assert.StartsWith("Sección 1 de ", Name(containers[1]), StringComparison.Ordinal);
                Assert.Equal(containers.Count, containers.Select(Name).Distinct().Count());

                // The buttons of the page have unique names; nothing reached AutoCAD.
                var names = Descendants<Button>(page).Where(Shown).Select(Name).ToList();
                Assert.Equal(names.Count, names.Distinct().Count());
                Assert.Empty(rig.Classic.Runs);

                // «Abrir el editor clásico» runs the classic card; «Nuevo» of the crumb goes back.
                Click(ButtonNamed(page, "Abrir el editor clásico del larguero"));
                Assert.Equal(new[] { RackCadClassicAction.NewLarguero }, rig.Classic.Runs);
                Click(ButtonNamed(page, "Volver a Nuevo"));
                Lay(view);
                Assert.True(rig.Window.IsNuevoPage);
                Assert.Equal(Visibility.Collapsed, view.LargueroPageHost.Visibility);
                Assert.Equal(Visibility.Visible, view.NuevoPage.Visibility);
            }));
        }

        [Fact]
        public void NarrowTheSummaryGoesBelowTheSectionsAndAFailingCatalogIsSaidOnThePage()
        {
            StaTestRunner.Run(() => WithBindingErrors(() =>
            {
                var rig = new Rig(() => throw new StructuralSectionCatalogException(null, "roto"));
                var view = new RackCadWindowView(rig.Window);
                rig.Window.OpenLargueroWorkbench();
                Lay(view, 1400);
                var page = view.LargueroPage;
                Assert.False(page.SummaryBelow);
                Assert.Equal(1, Grid.GetColumn(page.LargueroSummaryPanel));

                Lay(view, 600);
                Assert.True(page.SummaryBelow);
                Assert.Equal(0, Grid.GetColumn(page.LargueroSummaryPanel));
                Assert.Equal(1, Grid.GetRow(page.LargueroSummaryPanel));

                Assert.Equal(Visibility.Visible, page.LargueroCatalogNoticePanel.Visibility);
                Assert.StartsWith("El catálogo de perfiles AISC no se pudo leer (roto)", page.LargueroCatalogNoticeText.Text, StringComparison.Ordinal);

                // «Volver a los valores por defecto» from the page.
                rig.Window.LargueroWorkbench!.LengthText = "12";
                Click(ButtonNamed(page, "Volver el larguero a los valores por defecto"));
                Assert.Equal("96", rig.Window.LargueroWorkbench.LengthText);
            }));
        }
    }
}
