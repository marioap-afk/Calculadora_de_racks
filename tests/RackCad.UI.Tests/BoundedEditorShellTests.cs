using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using RackCad.UI.Shell;
using RackCad.UI.Systems.Cantilever.Components;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-39A: the bounded-editor shell (archetype B of ADR-0029) and its temporary Cantilever facade.
    ///
    /// <para>The shell was already neutral as a TYPE before I-39A — seven <c>object</c> slots, no branches, only
    /// <c>System.Windows</c> usings — but it lived under <c>Systems/Cantilever/Components</c> and
    /// <c>Themes/Generic.xaml</c> declared an xmlns to that namespace, so any consumer from another system would
    /// have had to depend on Cantilever to reuse it. These tests fix the move and, above all, fix that the four
    /// Cantilever component XAMLs keep resolving the very same template without being touched.</para>
    ///
    /// <para>Assertions are structural and semantic — never screenshots or pixel comparisons.</para>
    /// </summary>
    public sealed class BoundedEditorShellTests
    {
        /// <summary><c>FrameworkElement.DefaultStyleKeyProperty</c> is protected by design; a derived type may read
        /// it. This probe exists so the inheritance of the style key can be asserted without reflection.</summary>
        private sealed class StyleKeyProbe : RackBoundedEditorShell
        {
            public static object Of(RackBoundedEditorShell shell) => shell.GetValue(DefaultStyleKeyProperty);
        }

        private static ResourceDictionary Generic() =>
            new ResourceDictionary { Source = new Uri("/RackCad.UI;component/Themes/Generic.xaml", UriKind.Relative) };

        private static ResourceDictionary AppStyles() =>
            new ResourceDictionary { Source = new Uri("/RackCad.UI;component/Themes/AppStyles.xaml", UriKind.Relative) };

        /// <summary>A standalone control does not get its theme style auto-resolved without a shown HWND, so assign
        /// the REAL default style from Generic.xaml explicitly — the same one a shown window resolves — then measure
        /// so the template expands and the PART_ hosts become reachable.</summary>
        private static T Measured<T>(Action<T> setup = null, double w = 1000, double h = 700)
            where T : RackBoundedEditorShell, new()
        {
            var shell = new T();
            setup?.Invoke(shell);
            shell.Style = (Style)Generic()[typeof(RackBoundedEditorShell)];
            shell.Measure(new Size(w, h));
            shell.Arrange(new Rect(0, 0, w, h));
            shell.UpdateLayout();
            return shell;
        }

        // ---- 1. the seven zones ----

        [Fact]
        public void TheShellExposesItsSevenZonesAndEachAcceptsAnyContent()
        {
            StaTestRunner.Run(() =>
            {
                var header = new TextBlock { Text = "h" };
                var parameters = new StackPanel();
                var picker = new Border();
                var preview = new Canvas();
                var diagnostics = new TextBlock { Text = "d" };
                var bom = new TextBlock { Text = "b" };
                var actions = new StackPanel();

                var shell = Measured<RackBoundedEditorShell>(s =>
                {
                    s.Header = header;
                    s.Parameters = parameters;
                    s.SectionPicker = picker;
                    s.Preview = preview;
                    s.Diagnostics = diagnostics;
                    s.BomSummary = bom;
                    s.Actions = actions;
                });

                Assert.Same(header, shell.HeaderHost.Content);
                Assert.Same(parameters, shell.ParametersHost.Content);
                Assert.Same(picker, shell.PickerHost.Content);
                Assert.Same(preview, shell.PreviewHost.Content);
                Assert.Same(diagnostics, shell.DiagnosticsHost.Content);
                Assert.Same(bom, shell.BomHost.Content);
                Assert.Same(actions, shell.ActionsHost.Content);
            });
        }

        [Fact]
        public void EverySlotIsDeclaredAsObjectSoTheShellImposesNoType()
        {
            foreach (var name in new[]
                     {
                         "Header", "Parameters", "SectionPicker", "Preview", "Diagnostics", "BomSummary", "Actions"
                     })
            {
                var property = typeof(RackBoundedEditorShell).GetProperty(name);
                Assert.NotNull(property);
                Assert.Equal(typeof(object), property.PropertyType);
            }
        }

        [Fact]
        public void TheOptionalZonesCollapseWhenEmptyAndActionsIsAlwaysPresent()
        {
            StaTestRunner.Run(() =>
            {
                // A bounded editor that chooses nothing from a catalogue, produces no BOM and needs no header: the
                // structural section inspector is exactly that shape, and the shell must not leave four gaps for it.
                var shell = Measured<RackBoundedEditorShell>(s =>
                {
                    s.Parameters = new StackPanel();
                    s.Preview = new Canvas();
                    s.Actions = new StackPanel();
                });

                Assert.Equal(Visibility.Collapsed, shell.HeaderHost.Visibility);
                Assert.Equal(Visibility.Collapsed, shell.PickerHost.Visibility);
                Assert.Equal(Visibility.Collapsed, shell.BomHost.Visibility);
                Assert.Equal(Visibility.Collapsed, shell.DiagnosticsHost.Visibility);

                Assert.Equal(Visibility.Visible, shell.ParametersHost.Visibility);
                Assert.Equal(Visibility.Visible, shell.PreviewHost.Visibility);
                Assert.Equal(Visibility.Visible, shell.ActionsHost.Visibility);
            });
        }

        [Fact]
        public void DiagnosticsAndActionsLiveOutsideTheParametersScroll()
        {
            StaTestRunner.Run(() =>
            {
                // The reason the archetype exists: a bounded editor's problems and its actions must stay reachable
                // no matter how long the parameter list grows (owner decision 25 — accessibility, not a DPI number).
                var shell = Measured<RackBoundedEditorShell>(s =>
                {
                    s.Parameters = new StackPanel();
                    s.Diagnostics = new TextBlock { Text = "d" };
                    s.Actions = new StackPanel();
                });

                Assert.False(HasAncestorScrollViewer(shell.DiagnosticsHost));
                Assert.False(HasAncestorScrollViewer(shell.ActionsHost));
                Assert.True(HasAncestorScrollViewer(shell.ParametersHost));
            });
        }

        private static bool HasAncestorScrollViewer(DependencyObject node)
        {
            for (var current = node; current != null; current = System.Windows.Media.VisualTreeHelper.GetParent(current))
            {
                if (current is ScrollViewer)
                {
                    return true;
                }
            }

            return false;
        }

        // ---- 2. the shell is consumable from code, not only from XAML ----

        [Fact]
        public void AWindowBuiltEntirelyInCodeCanHostTheShell()
        {
            StaTestRunner.Run(() =>
            {
                // The pilot of I-39A is code-only. If the shell were only usable from XAML the archetype would be a
                // XAML contract, not a functional one.
                var shell = Measured<RackBoundedEditorShell>(s =>
                {
                    s.Parameters = new StackPanel();
                    s.Preview = new Canvas();
                    s.Actions = new StackPanel();
                });

                var window = new Window { Content = shell };

                Assert.Same(shell, window.Content);
            });
        }

        // ---- 3. the temporary Cantilever facade ----

        [Fact]
        public void TheCantileverFacadeIsTheBoundedShell()
        {
            Assert.True(typeof(RackBoundedEditorShell).IsAssignableFrom(typeof(CantileverComponentEditorShell)));
        }

        [Fact]
        public void TheFacadeDeclaresNothingOfItsOwn()
        {
            // A facade that re-declared the seven dependency properties would be code written to satisfy a guard,
            // which the owner forbade explicitly (decision 14). It declares nothing: it inherits everything.
            const BindingFlags declared = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic |
                                          BindingFlags.Instance | BindingFlags.Static;

            Assert.Empty(typeof(CantileverComponentEditorShell).GetFields(declared));
            Assert.Empty(typeof(CantileverComponentEditorShell).GetProperties(declared));
            Assert.DoesNotContain(typeof(CantileverComponentEditorShell).GetMethods(declared), m => !m.IsSpecialName);
        }

        [Fact]
        public void TheFacadeResolvesTheSameTemplateAsTheNeutralShell()
        {
            StaTestRunner.Run(() =>
            {
                // It does NOT override DefaultStyleKeyProperty, so it inherits the base type's style key and with it
                // the very same Style and ControlTemplate. This is what lets the four component XAMLs stay untouched.
                var generic = Generic();

                Assert.True(generic.Contains(typeof(RackBoundedEditorShell)));
                Assert.False(generic.Contains(typeof(CantileverComponentEditorShell)));

                Assert.Equal(typeof(RackBoundedEditorShell), StyleKeyProbe.Of(new CantileverComponentEditorShell()));
                Assert.Equal(typeof(RackBoundedEditorShell), StyleKeyProbe.Of(new RackBoundedEditorShell()));
            });
        }

        [Fact]
        public void TheFacadeStillHostsTheSevenZones()
        {
            StaTestRunner.Run(() =>
            {
                var parameters = new StackPanel();
                var preview = new Canvas();
                var actions = new StackPanel();

                var facade = Measured<CantileverComponentEditorShell>(s =>
                {
                    s.Parameters = parameters;
                    s.Preview = preview;
                    s.Actions = actions;
                });

                Assert.Same(parameters, facade.ParametersHost.Content);
                Assert.Same(preview, facade.PreviewHost.Content);
                Assert.Same(actions, facade.ActionsHost.Content);
            });
        }

        // ---- 4. the archetype's size contract is its own ----

        [Fact]
        public void TheBoundedEditorHasItsOwnSizeTokensAndDoesNotInheritTheRichEditorMinimums()
        {
            StaTestRunner.Run(() =>
            {
                // ADR-0029 D9: "un arquetipo no hereda implicitamente restricciones de tamano de otro. En particular,
                // el arquetipo B no hereda los minimos del editor rico A."
                var styles = AppStyles();

                foreach (var token in new[]
                         {
                             "BoundedEditorInitialWidth", "BoundedEditorInitialHeight",
                             "BoundedEditorMinWidth", "BoundedEditorMinHeight"
                         })
                {
                    Assert.True(styles.Contains(token), "falta el token " + token);
                }

                Assert.NotEqual((double)styles["ShellMinWidth"], (double)styles["BoundedEditorMinWidth"]);
                Assert.NotEqual((double)styles["ShellMinHeight"], (double)styles["BoundedEditorMinHeight"]);

                // The minimums must be reachable: a bounded editor that cannot shrink to its own minimum has the
                // very defect measured on the four Cantilever windows, whose declared widths are dead letters.
                Assert.True((double)styles["BoundedEditorMinWidth"] <= (double)styles["BoundedEditorInitialWidth"]);
                Assert.True((double)styles["BoundedEditorMinHeight"] <= (double)styles["BoundedEditorInitialHeight"]);
            });
        }
    }
}
