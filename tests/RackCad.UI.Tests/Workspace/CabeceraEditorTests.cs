#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using RackCad.Application.Persistence;
using RackCad.Application.RackFrames;
using RackCad.Application.VentanaRackCad;
using RackCad.Domain.RackFrames;
using RackCad.UI.VentanaRackCad;
using Xunit;
using static RackCad.UI.Tests.SelectiveWindowFixtures;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-64, fourth batch, part cb (prototype «cabecera», CB-01..CB-13): the draft of the editor of cabeceras (<see cref="CabeceraDraftEditor"/>),
    /// its view-model and its view with real WPF. The standard cabecera of 132 × 42 in of the catalog has H1 4 · H2 48 · H3 92 · H4 110 · H5 128
    /// in, P1 and P2 with a single diagonal and P3 and P4 with no bracing (RackFrameConfigurationFactory: troquel 3, clear 44, remate 4).
    /// </summary>
    public class CabeceraEditorTests
    {
        private static RackFrameConfiguration Standard132()
            => new RackFrameConfigurationFactory(UiSupport.LoadCatalogSafe()).Build(RackFrameTemplateCatalog.FindStandardOrDefault(), PostId, 132.0, 42.0);

        private static CabeceraDraftEditor Draft(RackFrameConfiguration? opened = null, CabeceraDraftContext? context = null)
            => new CabeceraDraftEditor(opened ?? Standard132(), UiSupport.LoadCatalogSafe(), RackFrameTemplateCatalog.All, context ?? CabeceraDraftContext.Standalone);

        private static CabeceraEditorViewModel Standalone(CabeceraDraftEditor? draft = null, Action<CabeceraEditorViewModel>? update = null)
            => CabeceraEditorViewModel.Standalone(draft ?? Draft(), "CAB-1", update ?? (_ => { }));

        private static double[] Elevations(CabeceraDraftEditor draft) => draft.Horizontals.Select(h => h.Elevation).ToArray();

        private static double[] Clears(CabeceraDraftEditor draft) => draft.Panels.Select(p => p.EndElevation - p.StartElevation).ToArray();

        // ------------------------------------------------------------------ the draft

        [Fact]
        public void TheStandardCabeceraOpensNormalizedAsTheTemplateGivesItAndWithNoChange()
        {
            var draft = Draft();

            Assert.Equal(new[] { 4.0, 48.0, 92.0, 110.0, 128.0 }, Elevations(draft));
            Assert.Equal(new[] { "H1", "H2", "H3", "H4", "H5" }, draft.Horizontals.Select(h => h.Id).ToArray());
            Assert.Equal(new[] { BracingPattern.SingleDiagonal, BracingPattern.SingleDiagonal, BracingPattern.NoBracing, BracingPattern.NoBracing },
                draft.Panels.Select(p => p.Arrangement).ToArray());
            Assert.Equal(132.0, draft.Height);
            Assert.Equal(132.0, draft.GeneratedHeight);
            Assert.Equal("STD-3P", draft.TemplateId);
            Assert.Empty(draft.Changes());
            Assert.False(draft.IsDirty);
        }

        [Fact]
        public void TheClearOfAPanelMovesItsUpperHorizontalAndEverythingAboveSoTheOtherPanelsKeepTheirs()
        {
            // CB-04, the note of the Owner: «falta una opción para cambiar el claro del panel».
            var draft = Draft();

            var result = draft.SetPanelClear(new[] { 0 }, 50.0);

            Assert.True(result.Applied, result.Error);
            Assert.Equal(new[] { 4.0, 54.0, 98.0, 116.0, 134.0 }, Elevations(draft));
            Assert.Equal(new[] { 50.0, 44.0, 18.0, 18.0 }, Clears(draft));
            Assert.Equal(138.0, draft.Height);
            Assert.True(draft.IsDirty);
            Assert.Contains("La altura del poste es 138 in", result.Notice);
            Assert.Contains(draft.Changes(), c => c.Line == "H2: elevación 48 → 54 in");

            // Two panels at once, from the lowest up; the arrangement of each panel stays with it.
            Assert.True(draft.SetPanelClear(new[] { 1, 0 }, 40.0).Applied);
            Assert.Equal(new[] { 40.0, 40.0, 18.0, 18.0 }, Clears(draft));
            Assert.Equal(new[] { BracingPattern.SingleDiagonal, BracingPattern.SingleDiagonal, BracingPattern.NoBracing, BracingPattern.NoBracing },
                draft.Panels.Select(p => p.Arrangement).ToArray());

            Assert.False(draft.SetPanelClear(new[] { 0 }, 0.0).Applied);
            Assert.False(draft.SetPanelClear(Array.Empty<int>(), 30.0).Applied);
        }

        [Fact]
        public void TheChangesCompareWithTheTemplateAndRestoreGoesBackByCategories()
        {
            var draft = Draft();
            Assert.True(draft.SetPanelArrangement(new[] { 2 }, BracingPattern.XBracing).Applied);
            Assert.True(draft.SetHorizontalQuantity(new[] { 0 }, 2).Applied);
            Assert.True(draft.SetCelosiaStart(4).Applied);

            Assert.Equal(new[] { "H1: cantidad 1 → 2", "P3: sin celosía → celosía en X", "Celosía: inicio (troquel) 3 → 4" }, draft.Changes().Select(c => c.Line).ToArray());

            // Only the panels: the horizontal and the celosía stay changed.
            Assert.True(draft.Restore(new[] { CabeceraChangeCategory.Panels }).Applied);
            Assert.Equal(new[] { "H1: cantidad 1 → 2", "Celosía: inicio (troquel) 3 → 4" }, draft.Changes().Select(c => c.Line).ToArray());

            Assert.True(draft.Restore(new[] { CabeceraChangeCategory.Horizontals, CabeceraChangeCategory.Celosia }).Applied);
            Assert.Empty(draft.Changes());
            Assert.False(draft.IsDirty);
        }

        [Fact]
        public void TheHeightAndTheTemplateRegenerateAndSayFirstWhatIsLost()
        {
            var draft = Draft();
            Assert.Empty(draft.RegenerationLossesForHeight(180.0));
            Assert.True(draft.SetPanelArrangement(new[] { 0 }, BracingPattern.DoubleDiagonal).Applied);
            Assert.Equal(new[] { "P1: diagonal simple → diagonal doble" }, draft.RegenerationLossesForHeight(180.0).Select(c => c.Line).ToArray());

            Assert.True(draft.SetHeight(180.0).Applied);

            Assert.Equal(180.0, draft.Height);
            Assert.Equal(180.0, draft.GeneratedHeight);
            Assert.Empty(draft.Changes());
            Assert.True(draft.SetTemplate("TALL-4P").Applied);
            Assert.Equal("TALL-4P", draft.TemplateId);
            Assert.Contains(draft.Panels, p => p.Arrangement == BracingPattern.XBracing);
            Assert.False(draft.SetHeight(-1.0).Applied);
        }

        [Fact]
        public void StructuralWritesKeepTheFrameNormalizedAndRefuseWhatWouldLeaveAPanelWithNoHeight()
        {
            var draft = Draft();
            Assert.True(draft.SplitPanel(0).Applied);
            Assert.Equal(new[] { 4.0, 26.0, 48.0, 92.0, 110.0, 128.0 }, Elevations(draft));
            Assert.Equal(BracingPattern.SingleDiagonal, draft.Panels[1].Arrangement); // both halves keep the values of the panel

            Assert.True(draft.CombineWithAbove(0).Applied);
            Assert.Equal(new[] { 4.0, 48.0, 92.0, 110.0, 128.0 }, Elevations(draft));

            Assert.True(draft.AddPanelOnTop(44.0).Applied);
            Assert.Equal(176.0, draft.Height);
            Assert.Equal(BracingPattern.SingleDiagonal, draft.Panels.Last().Arrangement);

            Assert.False(draft.SetElevation(1, 92.0).Applied); // another horizontal is there
            Assert.True(draft.SetElevation(5, 140.0).Applied);
            Assert.Equal(144.0, draft.Height); // moving the top one changes the height of the post, without regenerating

            Assert.False(draft.ShiftHorizontals(new[] { 1 }, 44.0).Applied); // it would land on H3
            Assert.True(draft.ShiftHorizontals(new[] { 3, 4 }, 2.0).Applied);
            Assert.False(draft.DuplicateAbove(0).Applied); // 4 + 44 = 48: H2 is there
            Assert.True(draft.DeleteHorizontals(new[] { 1, 2 }).Applied);
            Assert.False(draft.DeleteHorizontals(Enumerable.Range(0, draft.HorizontalCount - 1)).Applied); // two must stay
            Assert.Equal(draft.HorizontalCount - 1, draft.PanelCount);
            Assert.Equal(Enumerable.Range(1, draft.PanelCount).Select(i => "P" + i).ToArray(), draft.Panels.Select(p => p.PanelId).ToArray());
        }

        [Fact]
        public void DiscardGoesBackToWhatWasOpenedAndWhatWasWrittenBecomesTheBase()
        {
            var draft = Draft();
            draft.SetPanelArrangement(new[] { 3 }, BracingPattern.XBracing);
            draft.Discard();
            Assert.False(draft.IsDirty);
            Assert.Equal(BracingPattern.NoBracing, draft.Panels[3].Arrangement);

            draft.SetHorizontalNotes(new[] { 0 }, "revisar");
            draft.MarkWritten();
            Assert.False(draft.IsDirty);
            draft.Discard();
            Assert.Equal("revisar", draft.Horizontals[0].Notes);
        }

        [Fact]
        public void InsideARackTheDepthIsImposedAndAnEmptyPeralteIsTheOneOfTheRack()
        {
            var context = new CabeceraDraftContext(true, LockedDepth: 34.0, DepthOrigin: "La del fondo 2.", RackPostPeralte: 3.0);
            var draft = Draft(context: context);

            Assert.Equal(34.0, draft.Configuration.Depth);
            Assert.Equal(3.0, draft.Configuration.PostPeralte);
            Assert.Null(draft.OwnPostPeralte());
            Assert.False(draft.SetDepth(40.0).Applied);
            Assert.False(draft.SetName("X").Applied);
            Assert.True(draft.SetPostPeralte(5.0).Applied);
            Assert.Equal(5.0, draft.OwnPostPeralte());
            Assert.True(draft.SetPostPeralte(null).Applied);
            Assert.Equal(3.0, draft.Configuration.PostPeralte);
            Assert.False(draft.IsDirty);
        }

        [Fact]
        public void TheTwoPostsAreTheSameUntilOneChangesAndMakingThemEqualCopiesTheLeftOne()
        {
            var draft = Draft();
            Assert.True(draft.SidesAreEqual);
            Assert.True(draft.SetSideReinforced(new[] { PostSide.Left }, true).Applied);
            Assert.True(draft.SetSideReinforcementHeight(new[] { PostSide.Left }, 48.0).Applied);
            Assert.False(draft.SidesAreEqual);
            Assert.Contains(draft.Changes(), c => c.Line == "Poste izquierdo: refuerzo no → sí, 48 in");

            Assert.True(draft.MakeSidesEqual().Applied);
            Assert.True(draft.SidesAreEqual);
            Assert.True(draft.Side(PostSide.Right).HasReinforcement);
            Assert.Equal(48.0, draft.Side(PostSide.Right).ReinforcementHeight);
        }

        [Fact]
        public void TheCheckSaysEveryNoticeInSpanishAndTheHeightAgainstTheRack()
        {
            var context = new CabeceraDraftContext(true, HeightReview: h => h < 150.0
                ? new CabeceraHeightNote(CabeceraHeightSeverity.Severe, "Por debajo del nivel de carga.")
                : CabeceraHeightNote.None);
            var draft = Draft(context: context);
            Assert.Contains("Por debajo del nivel de carga.", draft.Notices());

            draft.SetCelosiaStart(5);
            Assert.Contains(draft.Notices(), n => n == "La horizontal inferior debería estar en el troquel de inicio (8 in); está en 4 in.");
        }

        // ------------------------------------------------------------------ the view-model

        [Fact]
        public void TheEditorOpensInAlzadoWithTheRowsFromTheTopDownAndTheFirstPanelChosen()
        {
            var vm = Standalone();

            Assert.Equal(1, vm.SelectedTab);
            Assert.Equal(new[] { "H5", "P4", "H4", "P3", "H3", "P2", "H2", "P1", "H1" }, vm.Rows.Select(r => r.Label).ToArray());
            Assert.Equal(new[] { 0 }, vm.SelectedPanels.ToArray());
            Assert.Equal("P1 · panel de 44 in", vm.InspectorTitle);
            Assert.Equal("Limpio", vm.StatusText);
            Assert.Equal(vm.Rows.Count, vm.Rows.Select(r => r.AccessibleText).Distinct().Count());

            vm.ClickRow(true, 2, ctrl: false, shift: true);
            Assert.Equal(new[] { 0, 1, 2 }, vm.SelectedPanels.ToArray());
            vm.ClickRow(false, 4, ctrl: true, shift: false);
            Assert.Equal(new[] { 4 }, vm.SelectedHorizontals.ToArray());
            Assert.Equal("Selección: 3 paneles y 1 horizontal", vm.InspectorTitle);
            Assert.Contains("H5 es la última", vm.TopNote);
            vm.SelectKind(true, true);
            Assert.True(vm.EverythingSelected);
        }

        [Fact]
        public void TheFaceIsNotOfferedAndThePanelHasItsClear()
        {
            // CB-13, the note of the Owner: «cara de montaje no sirve para nada»; CB-04: the clear of the panel.
            var vm = Standalone();
            vm.SelectKind(true, true);

            var keys = vm.PanelFields.Concat(vm.HorizontalFields).Concat(vm.ConnectionFields)
                .Select(f => f is DynamicFieldItem d ? d.Key + d.Label : f is SelectiveChoiceItem c ? c.Key + c.Label : string.Empty).ToList();
            Assert.DoesNotContain(keys, k => k.Contains("Cara", StringComparison.Ordinal));
            Assert.Contains(vm.PanelFields.OfType<DynamicFieldItem>(), f => f.Label == "Claro");

            vm.ClickRow(true, 0, false, false);
            Write(Field(vm.PanelFields, "pClear"), "50");
            Assert.Equal("Modificado", vm.StatusText);
            Assert.Equal("P1 · panel de 50 in", vm.InspectorTitle);
            Assert.Equal("138", Field(vm.Essentials, "alto").Text);
        }

        [Fact]
        public void WritingTheHeightWithOwnChangesAsksInlineWhatIsLostAndCancelKeepsEverything()
        {
            var vm = Standalone();
            Choice(vm.PanelFields, "pArr").SelectedValue = BracingPattern.XBracing.ToString();
            Assert.False(vm.IsRegenOpen);

            Write(Field(vm.Essentials, "alto"), "180");

            Assert.True(vm.IsRegenOpen);
            Assert.Equal(new[] { "P1: diagonal simple → celosía en X" }, vm.RegenLosses.ToArray());
            vm.ConfirmRegen(false);
            Assert.False(vm.IsRegenOpen);
            Assert.Equal("132", Field(vm.Essentials, "alto").Text);
            Assert.Equal(BracingPattern.XBracing, vm.Editor.Panels[0].Arrangement);

            Write(Field(vm.Essentials, "alto"), "180");
            vm.ConfirmRegen(true);
            Assert.Equal("180", Field(vm.Essentials, "alto").Text);
            Assert.Empty(vm.Changes);
        }

        [Fact]
        public void RestoreOpensWithItsBoxesAndItsCountsAndRestoresWhatIsMarked()
        {
            var vm = Standalone();
            Choice(vm.PanelFields, "pArr").SelectedValue = BracingPattern.XBracing.ToString();
            vm.ClickRow(false, 0, false, false);
            Write(Field(vm.HorizontalFields, "hQty"), "2");

            vm.OpenRestore();
            Assert.True(vm.IsRestoreOpen);
            Assert.Equal(new[] { "Horizontales (1)", "Paneles (1)", "Postes y refuerzos (ninguno)", "Placas (ninguno)", "Celosía (ninguno)" },
                vm.RestoreBoxes.Select(b => b.Label).ToArray());
            vm.RestoreBoxes.Single(b => b.Category == CabeceraChangeCategory.Horizontals).IsChecked = false;
            Assert.Equal(new[] { "P1: diagonal simple → celosía en X" }, vm.RestoreLosses.ToArray());

            vm.ConfirmRestore();
            Assert.False(vm.IsRestoreOpen);
            Assert.Equal(new[] { "H1: cantidad 1 → 2" }, vm.Changes.ToArray());
        }

        [Fact]
        public void AStandaloneCabeceraUpdatesOnlyWithChangesIsLockedWhileItIsWrittenAndIsCleanOnceWritten()
        {
            var sent = 0;
            var vm = Standalone(update: _ => sent++);
            Assert.False(vm.CanUpdate);
            vm.RequestWrite();
            Assert.Equal(0, sent);

            Choice(vm.PanelFields, "pArr").SelectedValue = BracingPattern.KBracing.ToString();
            Assert.True(vm.CanUpdate);
            vm.RequestWrite();
            Assert.Equal(1, sent);

            vm.SetWriting(true);
            Assert.Equal("Escribiendo", vm.StatusText);
            Assert.False(vm.CanEdit);
            Assert.True(Field(vm.Essentials, "alto").IsReadOnly);
            Assert.NotNull(vm.UpdateBlockedText);
            vm.WriteTaken();
            Assert.Contains("ya tomó la escritura", vm.UpdateBlockedText);

            vm.MarkWritten(true, "AutoCAD redibujó 2 vistas.");
            Assert.Equal("Limpio", vm.StatusText);
            Assert.False(vm.MessageIsError);

            Choice(vm.PanelFields, "pArr").SelectedValue = BracingPattern.NoBracing.ToString();
            var closed = 0;
            vm.CloseRequested += _ => closed++;
            vm.RequestClose();
            Assert.True(vm.IsConfirmingClose);
            Assert.Equal(0, closed);
            vm.ConfirmClose(true);
            Assert.Equal(1, closed);
        }

        // ------------------------------------------------------------------ inside a Selectivo (CB-01, CB-08, CB-09, CB-10)

        [Fact]
        public void EachPostOfEachFondoOffersItsCabeceraAndEditingItOpensTheEditorWithTheDepthOfItsFondo()
        {
            var vm = SelectiveEditor(TwoFondos());

            var p1 = vm.PostRows[0];
            Assert.Equal(new[] { "Fondo 1", "Fondo 2" }, p1.Cabeceras.Select(c => c.FondoLabel).ToArray());
            Assert.All(p1.Cabeceras, c => Assert.Equal("Calculada", c.Status));
            Assert.Equal("Editar la cabecera de P1 en el fondo 2", p1.Cabeceras[1].EditName);
            Assert.Single(vm.PostRows[3].Cabeceras); // P4 only exists in fondo 2

            vm.EditCabecera(1, 0);

            var cab = vm.EmbeddedCabecera!;
            Assert.True(vm.HasEmbeddedCabecera);
            Assert.True(cab.IsEmbedded);
            Assert.Equal("Cabecera de P1 · Fondo 2", cab.Title);
            Assert.Equal("Calculada", cab.StatusText);
            Assert.Equal(34.0, cab.Editor.Configuration.Depth); // 40 − 6
            Assert.True(Field(cab.Essentials, "depth").IsReadOnly);
            Assert.Equal(vm.Draft.CabeceraHeightAt(1, 0), cab.Editor.Height);
            Assert.Empty(cab.CopyTargets); // the targets are built when the copy opens

            // A calculated cabecera with no change is not a source (correction 2 of the review); with a change it is.
            Assert.False(cab.CanCopy);
            Assert.StartsWith("Una cabecera calculada y sin cambios no se copia", cab.CopyBlockedText, StringComparison.Ordinal);
            Choice(cab.PanelFields, "pArr").SelectedValue = BracingPattern.XBracing.ToString();
            Assert.True(cab.CanCopy);
            Assert.Equal(string.Empty, cab.CopyBlockedText);
            cab.OpenCopy();
            Assert.Equal(6, cab.CopyTargets.Count); // P1..P3 of fondo 1 and P2..P4 of fondo 2
        }

        [Fact]
        public void ApplyingTheCabeceraWritesItInTheDraftOfTheRackAndTheRackIsModified()
        {
            var vm = SelectiveEditor(Structure());
            vm.EditCabecera(0, 1);
            var cab = vm.EmbeddedCabecera!;
            Choice(cab.PanelFields, "pArr").SelectedValue = BracingPattern.XBracing.ToString();
            Assert.True(cab.IsDirty);
            Assert.True(vm.IsDirty); // a cabecera with changes not applied is work not written (D-08)
            Assert.False(vm.Draft.HasCustomCabecera(0, 1));

            cab.Apply();

            Assert.False(vm.HasEmbeddedCabecera);
            Assert.True(vm.Draft.HasCustomCabecera(0, 1));
            Assert.True(vm.IsDirty);
            Assert.Contains("Cabecera de P2 aplicada al borrador", vm.Message);
            Assert.Equal("Personalizada", vm.PostRows[1].Cabeceras[0].Status);

            // «Volver a la calculada» puts it back, and the rack is clean again.
            vm.ResetCabecera(0, 1);
            Assert.False(vm.Draft.HasCustomCabecera(0, 1));
            Assert.False(vm.IsDirty);
        }

        [Fact]
        public void ACabeceraBelowTheLoadLevelAsksInlineBeforeItIsApplied()
        {
            var vm = SelectiveEditor(Structure());
            vm.EditCabecera(0, 0);
            var cab = vm.EmbeddedCabecera!;
            Write(Field(cab.Essentials, "alto"), "60");
            Assert.NotEqual(string.Empty, cab.HeightNote);

            cab.Apply();
            Assert.True(cab.IsConfirmingHeight);
            Assert.Contains("nivel de carga", cab.HeightQuestion);
            Assert.True(vm.HasEmbeddedCabecera);
            Assert.False(vm.Draft.HasCustomCabecera(0, 0));

            cab.ConfirmHeight(true);
            Assert.False(vm.HasEmbeddedCabecera);
            Assert.True(vm.Draft.HasCustomCabecera(0, 0));
        }

        [Fact]
        public void CopyingToOtherPostsGivesEachOneTheDepthOfItsFondoAndThePeralteOfItsPost()
        {
            var vm = SelectiveEditor(TwoFondos());
            vm.EditCabecera(0, 1);
            var cab = vm.EmbeddedCabecera!;
            Choice(cab.PanelFields, "pArr").SelectedValue = BracingPattern.XBracing.ToString();
            cab.OpenCopy();
            cab.CopyTargets.Single(t => t.Label == "P1 · Fondo 1").IsChecked = true;
            cab.CopyTargets.Single(t => t.Label == "P3 · Fondo 2").IsChecked = true;
            Assert.Equal("Aplicar aquí y copiar a 2 destinos", cab.CopyConfirmLabel);

            cab.ConfirmCopy();

            Assert.False(vm.HasEmbeddedCabecera);
            var design = vm.Draft.Design;
            Assert.Equal(BracingPattern.XBracing, design.PostCabeceras[1].BracingPanels[0].Arrangement);
            Assert.Equal(BracingPattern.XBracing, design.PostCabeceras[0].BracingPanels[0].Arrangement);
            Assert.Equal(42.0, design.PostCabeceras[0].Depth);
            Assert.Equal(34.0, design.ExtraFondoPostCabeceras[0][2].Depth);
            Assert.Equal(5.0, design.PostCabeceras[0].PostPeralte); // P1 keeps its own peralte
            Assert.Equal(3.0, design.ExtraFondoPostCabeceras[0][2].PostPeralte);
            Assert.Equal(5.0, design.PostPeraltes[0]);
            Assert.Contains("se copió a P1 · Fondo 1, P3 · Fondo 2", vm.Message);
        }

        [Fact]
        public void BackWithChangesAsksAndDroppingThemLeavesTheRackAsItWas()
        {
            var vm = SelectiveEditor(Structure());
            vm.EditCabecera(0, 0);
            var cab = vm.EmbeddedCabecera!;
            Choice(cab.PanelFields, "pArr").SelectedValue = BracingPattern.XBracing.ToString();

            cab.Back();
            Assert.True(cab.IsConfirmingBack);
            Assert.True(vm.HasEmbeddedCabecera);

            cab.ConfirmBack(drop: true);
            Assert.False(vm.HasEmbeddedCabecera);
            Assert.False(vm.IsDirty);
            Assert.False(vm.Draft.HasCustomCabecera(0, 0));

            // With no change «‹ Selectivo» goes back at once.
            vm.EditCabecera(0, 0);
            vm.EmbeddedCabecera!.Back();
            Assert.False(vm.HasEmbeddedCabecera);
        }

        // ------------------------------------------------------------------ the view (real WPF)

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

        private static void Lay(FrameworkElement view, double width = 1300, double height = 900)
        {
            view.Measure(new Size(width, height));
            view.Arrange(new Rect(0, 0, width, height));
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

        private static void Click(Button button) => button.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));

        private static string Name(UIElement element) => UIElementAutomationPeer.CreatePeerForElement(element).GetName();

        private static void WithBindingErrors(Action<BindingErrors> body)
        {
            var errors = new BindingErrors();
            PresentationTraceSources.Refresh();
            var source = PresentationTraceSources.DataBindingSource;
            var level = source.Switch.Level;
            source.Listeners.Add(errors);
            source.Switch.Level = SourceLevels.Error;
            try
            {
                body(errors);
            }
            finally
            {
                source.Listeners.Remove(errors);
                source.Switch.Level = level;
            }
        }

        [Fact]
        public void TheViewHasItsFourTabsTheRowsOfTheElevationAndUniqueNamesWithNoBindingError()
        {
            StaTestRunner.Run(() => WithBindingErrors(errors =>
            {
                var vm = Standalone();
                var view = new CabeceraEditorView(vm);
                Lay(view);

                Assert.Equal(new[] { "Cabecera", "Alzado", "Postes y placas", "Avanzado" }, view.Tabs.Items.OfType<TabItem>().Select(t => (string)t.Header).ToArray());
                Assert.Same(view.AlzadoTab, view.Tabs.SelectedItem);
                Assert.Equal(Visibility.Visible, view.StandaloneHead.Visibility);
                Assert.Equal(Visibility.Collapsed, view.EmbeddedHead.Visibility);
                var rows = Descendants<Button>(view.ElevationRows).ToList();
                Assert.Equal(9, rows.Count);
                Assert.Equal(vm.Rows.Select(r => r.AccessibleText).ToArray(), rows.Select(Name).ToArray());

                // A click on a row selects it; the inspector follows.
                Click(rows[1]);
                Lay(view);
                Assert.Equal(new[] { 3 }, vm.SelectedPanels.ToArray());
                Assert.Equal("P4 · panel de 18 in", view.InspectorTitle.Text);

                foreach (TabItem tab in view.Tabs.Items)
                {
                    view.Tabs.SelectedItem = tab;
                    Lay(view);
                }

                var named = new UIElement[]
                {
                    view.UpdateButton, view.DiscardButton, view.RestoreButton, view.CloseEditorButton, view.AddPanelButton, view.SplitButton, view.CombineButton,
                    view.PanelsStandardButton, view.DuplicateButton, view.DeleteButton, view.SelectPanelsButton, view.SelectHorizontalsButton, view.SelectAllButton,
                    view.RestoreConfirmButton, view.RestoreCancelButton, view.CloseYesButton, view.CloseNoButton, view.RegenYesButton, view.RegenNoButton,
                }.Select(Name).ToList();
                Assert.Equal(named.Count, named.Distinct().Count());
                Assert.Empty(errors.Messages);
            }));
        }

        [Fact]
        public void InsideSelectivoTheEditorOfTheCabeceraTakesThePlaceOfTheRackAndBackGivesItBack()
        {
            StaTestRunner.Run(() => WithBindingErrors(errors =>
            {
                var vm = SelectiveEditor(TwoFondos());
                var view = new SelectiveEditorView(vm);
                vm.SelectedTab = 3;
                Lay(view);
                var edit = Descendants<Button>(view.PostRowList).First(b => Name(b) == "Editar la cabecera de P2 en el fondo 1");
                Assert.True(edit.IsEnabled);

                Click(edit);
                Lay(view);

                Assert.NotNull(view.CabeceraView);
                Assert.Same(view.CabeceraView, view.Content);
                Assert.Equal(Visibility.Visible, view.CabeceraView!.EmbeddedHead.Visibility);
                Assert.Equal("‹ Selectivo A", view.CabeceraView.BackButton.Content);
                Assert.Equal("Cabecera de P2 · Fondo 1", view.CabeceraView.EmbeddedTitleText.Text);

                Click(view.CabeceraView.BackButton);
                Lay(view);
                Assert.Null(view.CabeceraView);
                Assert.IsNotType<CabeceraEditorView>(view.Content);
                Assert.Empty(errors.Messages);
            }));
        }
    }
}
