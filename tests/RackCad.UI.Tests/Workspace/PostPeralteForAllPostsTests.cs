#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Input;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Selective;
using RackCad.Application.VentanaRackCad;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.Selective;
using RackCad.Domain.Systems.Shared;
using RackCad.UI.VentanaRackCad;
using Xunit;
using static RackCad.UI.Tests.SelectiveWindowFixtures;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-64, fifth batch, part «pp»: the request of the Owner of 2026-11-26, «en el selectivo no hay forma de colocar un peralte de poste en
    /// todos los postes, tienes que ir uno por uno, no sé si sea el mismo caso en el dinámico, hay que arreglar eso».
    /// <list type="bullet">
    /// <item>Selectivo: «Avanzado» › «Postes y cabeceras» › «Todos los postes» + «Aplicar a todos» makes one peralte the one of the rack and
    /// takes the own ones away, in the draft, and says how many; «Lo esencial» › «Poste» shows the peralte of the rack «para todos los
    /// postes» without unfolding «Poste y anotaciones».</item>
    /// <item>Dinámico: ONE peralte for the rack (every cabecera, the custom ones too): «Lo esencial» shows it with the same text.</item>
    /// </list>
    /// What is written goes to the draft: the drawing changes with «Actualizar», no new command.
    /// </summary>
    public class PostPeralteForAllPostsTests
    {
        private const string Tail = "«Descartar» vuelve a lo último que se leyó o se escribió.";

        // Structure (one fondo, 3 posts) has an own peralte of 5 in on P1; this adds one of 6 in on P3 and keeps the rack at 3 in.
        private static SelectivePalletDesign TwoOwnPeraltes()
        {
            var design = Structure();
            design.PostPeraltes[2] = 6.0;
            return design;
        }

        private static string[] OwnTexts(SelectiveEditorViewModel vm) => vm.PostRows.Select(r => r.Field.Text).ToArray();

        // ------------------------------------------------------------------ Selectivo: «Todos los postes» (the view-model)

        [Fact]
        public void AllPostsMakesOnePeralteTheOneOfTheRackTakesTheOwnOnesAwayInTheDraftAndSaysHowMany()
        {
            var writes = 0;
            var vm = SelectiveEditor(TwoOwnPeraltes(), update: _ => writes++);
            Assert.Equal("3", vm.AllPostsPeralteText);
            Assert.Equal(new[] { "5", "", "6" }, OwnTexts(vm));
            Assert.Equal("3 postes · 2 peraltes propios · 0 cabeceras personalizadas", vm.PostsSummary);
            Assert.Equal("Pone este peralte como el del rack y quita los 2 peraltes propios: P1 (5 in), P3 (6 in).", vm.AllPostsHint);
            Assert.True(vm.CanApplyPeralteToAllPosts);

            vm.AllPostsPeralteText = "4";
            vm.ApplyPeralteToAllPosts();

            Assert.Equal("Peralte de 4 in en todos los postes del borrador: se quitaron 2 peraltes propios (P1: 5 in, P3: 6 in). " + Tail, vm.Message);
            Assert.False(vm.MessageIsError);
            Assert.Equal(string.Empty, vm.AllPostsError);
            Assert.Equal(4.0, vm.Draft.PostPeralte);
            Assert.All(Enumerable.Range(0, vm.Draft.PostCount), post => Assert.Null(vm.Draft.PostPeralteOverride(post)));
            Assert.All(Enumerable.Range(0, vm.Draft.PostCount), post => Assert.Equal(4.0, SelectivePostGeometry.PostPeralteAt(vm.Draft.System!, post)));

            // The table and the rest of the editor follow the draft.
            Assert.Equal(new[] { "", "", "" }, OwnTexts(vm));
            Assert.All(vm.PostRows, r => Assert.Equal("Del rack: 4", r.Field.Hint));
            Assert.Equal("3 postes · 0 peraltes propios · 0 cabeceras personalizadas", vm.PostsSummary);
            Assert.Equal("4", vm.AllPostsPeralteText);
            Assert.Equal("Pone este peralte como el del rack, para todos los postes. Ningún poste tiene peralte propio.", vm.AllPostsHint);
            Assert.Equal("4", Field(vm.Essentials, "peralte").Text);
            Assert.True(vm.IsDirty);
            Assert.Equal("Modificado", vm.StatusText);

            // Nothing went to AutoCAD: the drawing changes with «Actualizar», which hands over that draft.
            Assert.Equal(0, writes);
            Assert.True(vm.CanUpdate);
            vm.RequestWrite();
            Assert.Equal(1, writes);
            var written = vm.Draft.SnapshotForWrite()!.Design;
            Assert.Equal(4.0, written.PostPeralte);
            Assert.All(written.PostPeraltes, own => Assert.Equal(0.0, own));

            // «Descartar» goes back to what was read.
            vm.Discard();
            Assert.Equal(new[] { "5", "", "6" }, OwnTexts(vm));
            Assert.Equal("3", vm.AllPostsPeralteText);
        }

        [Fact]
        public void ItIsTheWholeRackWhateverTheFondosOfTheViewAreAndANewRackSaysWhatDiscardDoesForIt()
        {
            var vm = SelectiveEditor(TwoFondos()); // 4 posts, the own peralte of P1
            vm.FondoScopeIndex = 2;

            vm.AllPostsPeralteText = "3.5";
            vm.ApplyPeralteToAllPosts();

            Assert.Equal("Peralte de 3.5 in en todos los postes del borrador: se quitó 1 peralte propio (P1: 5 in). " + Tail, vm.Message);
            Assert.Equal(4, vm.Draft.PostCount);
            Assert.All(Enumerable.Range(0, 4), post => Assert.Null(vm.Draft.PostPeralteOverride(post)));
            Assert.Equal(2, vm.FondoScopeIndex); // the view of the matrix is not changed

            var draft = SelectiveDraftEditor.CreateNew(UiSupport.LoadCatalogSafe());
            Assert.False(draft.SetPostPeralte(1, 2.5).Refused);
            Assert.True(draft.Recompose(), draft.LastError);
            var created = SelectiveEditorViewModel.ForNewRack(draft, "nuevo");
            created.ApplyPeralteToAllPosts(); // the row shows the peralte of the rack: the own one goes
            Assert.Equal("Peralte de 3 in en todos los postes del borrador: se quitó 1 peralte propio (P2: 2.5 in). " + SelectiveLayoutRestoreResult.NewRackDiscardText,
                created.Message);
        }

        [Fact]
        public void WithNothingToChangeItSaysSoAndTheDraftStaysClean()
        {
            var design = Structure();
            design.PostPeraltes[0] = 0.0; // no own peralte: every post takes the 3 in of the rack
            var vm = SelectiveEditor(design);
            var before = Json(vm.Draft.Design);

            vm.ApplyPeralteToAllPosts(); // the row shows the 3 in of the rack

            Assert.Equal("Todos los postes ya tienen 3 in y ninguno tiene peralte propio: el borrador no cambió.", vm.Message);
            Assert.Equal(before, Json(vm.Draft.Design));
            Assert.False(vm.IsDirty);
            Assert.Equal("Limpio", vm.StatusText);
        }

        [Theory]
        [InlineData("0")]
        [InlineData("-1")]
        [InlineData("abc")]
        [InlineData("")]
        public void APeralteThatIsNotANumberGreaterThanZeroIsRefusedInTheRowAndWritesNothing(string text)
        {
            var vm = SelectiveEditor(TwoOwnPeraltes());
            var before = Json(vm.Draft.Design);

            vm.AllPostsPeralteText = text;
            vm.ApplyPeralteToAllPosts();

            Assert.Equal(SelectiveDraftEditor.InvalidAllPostsPeralteText, vm.AllPostsError);
            Assert.Equal(before, Json(vm.Draft.Design));
            Assert.False(vm.IsDirty);
            Assert.Equal(text, vm.AllPostsPeralteText); // what was written stays, to correct it

            // Writing again clears the reason.
            vm.AllPostsPeralteText = "4";
            Assert.Equal(string.Empty, vm.AllPostsError);
        }

        [Fact]
        public void WhileAutoCadWritesTheRowCannotBeUsed()
        {
            var vm = SelectiveEditor(TwoOwnPeraltes());
            Write(Field(vm.CellFields, "cHeight"), "72");
            vm.BeginWriting();
            Assert.False(vm.CanApplyPeralteToAllPosts);
            Assert.True(vm.IsAllPostsReadOnly);

            vm.AllPostsPeralteText = "4";
            vm.ApplyPeralteToAllPosts();

            Assert.True(vm.MessageIsError);
            Assert.Equal(vm.WritingText, vm.Message);
            Assert.Equal(3.0, vm.Draft.PostPeralte);
            Assert.Equal(5.0, vm.Draft.PostPeralteOverride(0));
            vm.EndWriting();
            Assert.True(vm.CanApplyPeralteToAllPosts);
            Assert.False(vm.IsAllPostsReadOnly);
        }

        [Fact]
        public void TheRowFollowsThePeralteOfTheRackUntilTheUserWritesAnotherOne()
        {
            var vm = SelectiveEditor(TwoOwnPeraltes());
            Write(Field(vm.Essentials, "peralte"), "3.5");
            Assert.Equal("3.5", vm.AllPostsPeralteText);

            vm.AllPostsPeralteText = "4";
            Write(Field(vm.AnnotationFields, "scale"), "2"); // another write of the draft keeps what the user is writing
            Assert.Equal("4", vm.AllPostsPeralteText);
            Assert.Equal(3.5, vm.Draft.PostPeralte); // the box alone writes nothing

            vm.AllPostsPeralteText = "3.5"; // back to the peralte of the rack: it follows it again
            Write(Field(vm.Essentials, "peralte"), "2.75");
            Assert.Equal("2.75", vm.AllPostsPeralteText);
        }

        // ------------------------------------------------------------------ Selectivo: «Lo esencial» › «Poste»

        [Fact]
        public void TheEssentialsShowThePeralteForEveryPostAndWhichPostsKeepAnOwnOne()
        {
            var vm = SelectiveEditor(TwoOwnPeraltes());

            var group = vm.EssentialGroups.Single(g => g.Title == "Poste");
            var peralte = Assert.IsType<DynamicFieldItem>(Assert.Single(group.Fields));
            Assert.Same(Field(vm.Essentials, "peralte"), peralte);
            Assert.Equal(("Peralte del poste", "in", "3"), (peralte.Label, peralte.Unit, peralte.Text));
            Assert.Equal("Para todos los postes salvo los de peralte propio: P1, P3. «Aplicar a todos», en «Avanzado», los quita.", peralte.Hint);
            Assert.DoesNotContain(vm.AnnotationFields, f => f is DynamicFieldItem d && d.Key == "peralte");

            // Its write is the one the box of «Poste y anotaciones» had: the peralte of the rack, the own ones stay.
            Write(peralte, "4");
            Assert.Equal(4.0, vm.Draft.PostPeralte);
            Assert.Equal((5.0, 6.0), (vm.Draft.PostPeralteOverride(0), vm.Draft.PostPeralteOverride(2)));
            Assert.Equal(4.0, SelectivePostGeometry.PostPeralteAt(vm.Draft.System!, 1));

            Write(vm.PostRows[2].Field, string.Empty);
            Assert.Equal("Para todos los postes salvo el de peralte propio: P1. «Aplicar a todos», en «Avanzado», lo quita.", Field(vm.Essentials, "peralte").Hint);
            vm.ApplyPeralteToAllPosts();
            Assert.Equal("Para todos los postes. Un peralte propio, poste por poste, en «Avanzado».", Field(vm.Essentials, "peralte").Hint);

            Write(Field(vm.Essentials, "peralte"), "0");
            Assert.Equal("El peralte de poste debe ser un número mayor que cero.", Field(vm.Essentials, "peralte").Error);
            Assert.Equal(4.0, vm.Draft.PostPeralte);
        }

        // ------------------------------------------------------------------ Dinámico

        private static DynamicRackDesign DynamicStructure()
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

        private static DynamicEditorViewModel DynamicEditor(Action<DynamicEditorViewModel>? update = null)
        {
            var recomposed = new DynamicDraftEditor(UiSupport.LoadCatalogSafe());
            recomposed.LoadFrom(DynamicStructure(), "DIN 1");
            Assert.True(recomposed.Recompose(), recomposed.LastError);
            var json = new RackProjectStore().Serialize(RackProject.ForDynamic(recomposed.Design));
            var draft = new DynamicDraftEditor(UiSupport.LoadCatalogSafe());
            draft.LoadFrom(new RackProjectStore().Deserialize(json)!.DynamicDesign, "DIN 1");
            return new DynamicEditorViewModel(draft, "DIN-1", update ?? (_ => { }));
        }

        private static IReadOnlyList<double> HeaderPeraltes(DynamicEditorViewModel vm)
            => vm.Draft.System!.Modules.Where(m => m.IsHeader && m.AssociatedFrameConfiguration != null).Select(m => m.AssociatedFrameConfiguration.PostPeralte).ToList();

        [Fact]
        public void TheDynamicPeralteOfThePostIsInTheEssentialsForEveryPostAndGoesToEveryCabecera()
        {
            var writes = 0;
            var vm = DynamicEditor(_ => writes++);

            var peralte = vm.Essentials.Single(f => f.Key == "peralte");
            Assert.Equal(("Peralte de poste", "in"), (peralte.Label, peralte.Unit));
            Assert.Equal("Para todos los postes, también los de las cabeceras personalizadas.", peralte.Hint);
            Assert.DoesNotContain(vm.PostFields, f => f.Key == "peralte");
            Assert.Equal(new[] { "scale" }, vm.PostFields.Select(f => f.Key).ToArray());
            Assert.NotEmpty(HeaderPeraltes(vm));

            peralte.Text = "4.5";
            peralte.Commit();

            Assert.Equal(string.Empty, peralte.Error);
            Assert.Equal(4.5, vm.Draft.PostPeralte);
            Assert.All(HeaderPeraltes(vm), p => Assert.Equal(4.5, p));
            Assert.Equal("4.5", vm.Essentials.Single(f => f.Key == "peralte").Text);
            Assert.True(vm.IsDirty);
            Assert.Equal(0, writes); // the drawing changes with «Actualizar»

            peralte.Text = "0";
            peralte.Commit();
            Assert.Equal("El peralte de poste debe ser un número mayor que cero.", peralte.Error);
            Assert.Equal(4.5, vm.Draft.PostPeralte);
        }

        // ------------------------------------------------------------------ the views (real WPF)

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

        private static void WithNoBindingError(Action body)
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
                    body();
                }
                finally
                {
                    source.Listeners.Remove(errors);
                    source.Switch.Level = level;
                }

                Assert.Empty(errors.Messages);
            });
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

        private static bool IsInside(DependencyObject element, DependencyObject container)
        {
            for (var at = element; at != null; at = System.Windows.Media.VisualTreeHelper.GetParent(at))
                if (ReferenceEquals(at, container)) return true;
            return false;
        }

        private static void Click(Button button) => button.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));

        private static void PressEnter(UIElement element) =>
            element.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice, new System.Windows.Interop.HwndSource(0, 0, 0, 0, 0, "t", IntPtr.Zero), 0, Key.Enter)
            {
                RoutedEvent = UIElement.KeyDownEvent,
            });

        private static string Name(UIElement element) => UIElementAutomationPeer.CreatePeerForElement(element).GetName();

        [Fact]
        public void TheRowAllPostsOfAdvancedAppliesWithItsButtonOrEnterAndLeavingTheBoxWritesNothing()
        {
            WithNoBindingError(() =>
            {
                var vm = SelectiveEditor(TwoOwnPeraltes());
                var view = new SelectiveEditorView(vm);
                Lay(view);
                view.Tabs.SelectedItem = view.AdvancedTab;
                Lay(view);

                Assert.Equal("Todos los postes", System.Windows.Automation.AutomationProperties.GetName(view.AllPostsRow));
                Assert.Equal("Todos los postes: peralte", Name(view.AllPostsPeralteBox));
                Assert.Equal("Aplicar el peralte a todos los postes", Name(view.ApplyAllPostsButton));
                Assert.Equal("Aplicar a todos", view.ApplyAllPostsButton.Content);
                Assert.Equal("3", view.AllPostsPeralteBox.Text);
                Assert.Equal(vm.AllPostsHint, view.AllPostsHintText.Text);
                Assert.Equal(Visibility.Collapsed, view.AllPostsErrorText.Visibility);
                Assert.True(view.ApplyAllPostsButton.IsEnabled);
                Assert.False(view.AllPostsPeralteBox.IsReadOnly);
                // The names of «Avanzado» stay unique with the row.
                var names = Descendants<TextBox>(view.PostRowList).Select(Name).Concat(new[] { Name(view.AllPostsPeralteBox), Name(view.ApplyAllPostsButton) }).ToList();
                Assert.Equal(names.Count, names.Distinct().Count());

                // Leaving the box writes nothing.
                view.AllPostsPeralteBox.Text = "6";
                view.AllPostsPeralteBox.RaiseEvent(new RoutedEventArgs(UIElement.LostFocusEvent, view.AllPostsPeralteBox));
                Assert.Equal(3.0, vm.Draft.PostPeralte);
                Assert.Equal(5.0, vm.Draft.PostPeralteOverride(0));
                Assert.False(vm.IsDirty);

                // A value that is not valid: the reason under the row.
                view.AllPostsPeralteBox.Text = "0";
                Click(view.ApplyAllPostsButton);
                Lay(view);
                Assert.Equal(Visibility.Visible, view.AllPostsErrorText.Visibility);
                Assert.Equal(SelectiveDraftEditor.InvalidAllPostsPeralteText, view.AllPostsErrorText.Text);
                Assert.False(vm.IsDirty);

                // «Aplicar a todos».
                view.AllPostsPeralteBox.Text = "4";
                Click(view.ApplyAllPostsButton);
                Lay(view);
                Assert.Equal(Visibility.Collapsed, view.AllPostsErrorText.Visibility);
                Assert.Equal("Peralte de 4 in en todos los postes del borrador: se quitaron 2 peraltes propios (P1: 5 in, P3: 6 in). " + Tail, view.MessageText.Text);
                Assert.Equal("Modificado", view.StatusText.Text);
                Assert.All(Descendants<TextBox>(view.PostRowList), box => Assert.Equal(string.Empty, box.Text));
                Assert.Equal("3 postes · 0 peraltes propios · 0 cabeceras personalizadas", view.PostsSummaryText.Text);
                Assert.Equal("4", view.AllPostsPeralteBox.Text);

                // Enter in the box does the same.
                view.AllPostsPeralteBox.Text = "4.5";
                PressEnter(view.AllPostsPeralteBox);
                Assert.Equal(4.5, vm.Draft.PostPeralte);
                Assert.StartsWith("Peralte de 4.5 in en todos los postes del borrador: ningún poste tenía peralte propio.", vm.Message);

                // While AutoCAD writes, the row cannot be used.
                vm.BeginWriting();
                Lay(view);
                Assert.False(view.ApplyAllPostsButton.IsEnabled);
                Assert.True(view.AllPostsPeralteBox.IsReadOnly);
                vm.EndWriting();
                Lay(view);
                Assert.True(view.ApplyAllPostsButton.IsEnabled);
            });
        }

        [Fact]
        public void TheSelectivoShowsThePeralteForEveryPostInTheEssentialsWithPostAndAnnotationsFolded()
        {
            WithNoBindingError(() =>
            {
                var vm = SelectiveEditor(TwoOwnPeraltes());
                var view = new SelectiveEditorView(vm);
                Lay(view);
                view.Tabs.SelectedItem = view.RackTab;
                Lay(view);

                Assert.False(view.PostExpander.IsExpanded);
                Assert.Contains(Descendants<TextBlock>(view.EssentialsList), t => t.Text == "Poste");
                var box = Descendants<TextBox>(view.EssentialsList).Single(b => Name(b) == "Peralte del poste");
                Assert.False(IsInside(box, view.PostExpander));
                Assert.Equal("3", box.Text);
                Assert.Contains(Descendants<TextBlock>(view.EssentialsList), t => t.Text.StartsWith("Para todos los postes", StringComparison.Ordinal));

                // Written there, it is the peralte of the rack.
                box.Text = "3.5";
                box.RaiseEvent(new RoutedEventArgs(UIElement.LostFocusEvent, box));
                Assert.Equal(3.5, vm.Draft.PostPeralte);

                // «Poste y anotaciones» keeps the post, not a second box of the peralte.
                view.PostExpander.IsExpanded = true;
                Lay(view);
                Assert.DoesNotContain(Descendants<TextBox>(view.AnnotationList), b => Name(b) == "Peralte del poste");
                Assert.Contains(Descendants<ContentPresenter>(view.AnnotationList), c => c.Content is SelectiveChoiceItem choice && choice.Key == "post");
            });
        }

        [Fact]
        public void TheDinamicoShowsThePeralteForEveryPostInTheEssentialsWithPostAndAnnotationsFolded()
        {
            WithNoBindingError(() =>
            {
                var vm = DynamicEditor();
                var view = new DynamicEditorView(vm);
                Lay(view);
                view.Tabs.SelectedItem = view.RackTab;
                Lay(view);

                Assert.False(view.PostExpander.IsExpanded);
                var box = Descendants<TextBox>(view.EssentialsList).Single(b => Name(b) == "Peralte de poste");
                Assert.False(IsInside(box, view.PostExpander));
                Assert.Contains(Descendants<TextBlock>(view.EssentialsList), t => t.Text == "Para todos los postes, también los de las cabeceras personalizadas.");

                box.Text = "4.5";
                box.RaiseEvent(new RoutedEventArgs(UIElement.LostFocusEvent, box));
                Assert.Equal(4.5, vm.Draft.PostPeralte);

                view.PostExpander.IsExpanded = true;
                Lay(view);
                Assert.DoesNotContain(Descendants<TextBox>(view.PostExpander), b => Name(b) == "Peralte de poste");
            });
        }
    }
}
