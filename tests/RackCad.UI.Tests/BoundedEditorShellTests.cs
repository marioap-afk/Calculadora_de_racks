using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using RackCad.Application.Catalogs;
using RackCad.Application.StructuralSections;
using RackCad.Domain.Systems.Cantilever;
using RackCad.UI.Shell;
using RackCad.UI.Systems.Cantilever.Components;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-39A: the bounded-editor shell (archetype B of ADR-0029), completed by I-39C.
    ///
    /// <para>The shell was already neutral as a TYPE before I-39A — seven <c>object</c> slots, no branches, only
    /// <c>System.Windows</c> usings — but it lived under <c>Systems/Cantilever/Components</c> and
    /// <c>Themes/Generic.xaml</c> declared an xmlns to that namespace, so any consumer from another system would
    /// have had to depend on Cantilever to reuse it. I-39A moved it and left a facade in the old place so the four
    /// already-validated component XAMLs would not be touched; I-39C migrated those XAMLs and deleted the facade.
    /// These tests fix the move and, above all, that the four windows keep resolving the very same template.</para>
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

        /// <summary>The four Cantilever component windows, each with the minimum its constructor demands. They are the
        /// shell's original consumers, and after I-39C they name the neutral type directly.</summary>
        private static Window[] FourComponentWindows()
        {
            var catalogue = new CsvStructuralSectionCatalogProvider(CatalogDirectory.Resolve()).Load();
            var template = new CantileverStationColumnBaseTemplateDesign
            {
                ColumnSectionId = "AISC-W-W10X33",
                Base = new CantileverBaseDesign { SectionId = "AISC-W-W10X33", Length = 48.0 }
            };

            var arm = new CantileverArmTemplateDesign
            {
                Body = new CantileverArmBodyDesign { SectionId = "AISC-HSS-RECT-HSS4X4X_250", CutLength = 36.0 }
            };

            return new Window[]
            {
                new CantileverColumnBaseWindow(template, catalogue),
                new CantileverArmWindow(arm, template, catalogue),
                new CantileverSeparatorWindow(new CantileverBracingDesign(), catalogue),
                new CantileverBraceWindow(new CantileverBracingDesign(), catalogue)
            };
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

        // ---- 3. the Cantilever facade is gone: no bounded shell carries a system name ----

        [Fact]
        public void NoBoundedShellTypeCarriesASystemName()
        {
            // I-39A left `CantileverComponentEditorShell` as a facade so four already-validated XAMLs would not be
            // touched; I-39C migrated them and deleted it. What replaces those four facade guards is the invariant
            // they existed to protect: the bounded-editor shell has exactly ONE type, it lives in shared
            // infrastructure, and nothing derives from it to re-plant a system's name on the archetype.
            var derived = typeof(RackBoundedEditorShell).Assembly
                .GetTypes()
                .Where(type => typeof(RackBoundedEditorShell).IsAssignableFrom(type) && type != typeof(RackBoundedEditorShell))
                .ToList();

            Assert.Empty(derived);
            Assert.Equal("RackCad.UI.Shell", typeof(RackBoundedEditorShell).Namespace);
        }

        [Fact]
        public void TheFourComponentWindowsComposeOverTheNeutralShell()
        {
            StaTestRunner.Run(() =>
            {
                // The migration is real, not nominal: the four windows' content IS an instance of the neutral type,
                // and it resolves the SAME style key — i.e. the very same template they rendered with before.
                var generic = Generic();
                Assert.True(generic.Contains(typeof(RackBoundedEditorShell)));

                foreach (var window in FourComponentWindows())
                {
                    var shell = Assert.IsType<RackBoundedEditorShell>(window.Content);
                    Assert.Equal(typeof(RackBoundedEditorShell), StyleKeyProbe.Of(shell));
                }
            });
        }

        // ---- 3b. the shell carries its own tokens, whatever the consumer merged ----

        [Fact]
        public void TheShellResolvesItsSpacingTokensWithoutTheConsumerMergingAnything()
        {
            StaTestRunner.Run(() =>
            {
                // REGRESION (validacion manual de I-39A, defecto unico): los botones de accion aparecian pegados a
                // los bordes derecho e inferior. Causa: un DynamicResource con clave de cadena recorre el arbol de
                // elementos y Application.Resources, pero NO cae al diccionario de tema del ensamblado, aunque de
                // ahi salga la propia plantilla. Un consumidor construido en codigo no mergea AppStyles y no tiene
                // por que hacerlo, asi que ShellZoneSpacing quedaba sin resolver y el margen era CERO.
                var shell = Measured<RackBoundedEditorShell>(s =>
                {
                    s.Parameters = new StackPanel();
                    s.Preview = new Canvas();
                    s.Actions = new StackPanel();
                });

                Assert.Equal(
                    ShellResourcesProbe.ZoneSpacing(),
                    shell.TryFindResource("ShellZoneSpacing"));
            });
        }

        [Fact]
        public void TheShellBacksItsTokensUpWithoutShadowingAConsumerThatAlreadyHasThem()
        {
            StaTestRunner.Run(() =>
            {
                // I-39C, simetria exacta con lo que I-39B corrigio en el shell rico. I-39A mergeaba el diccionario
                // compartido SIEMPRE, en el constructor. Eso no es respaldo: es sombreado. Para el contenido que el
                // editor inyecta en las ranuras, el diccionario del control queda por delante del de la ventana, de
                // modo que ese contenido resuelve OTRA INSTANCIA del mismo estilo — misma apariencia, distinto
                // objeto—. Ahora el respaldo solo entra cuando el token NO resuelve.
                var window = FourComponentWindows()[0];
                var shell = (RackBoundedEditorShell)window.Content;
                shell.ApplyTemplate();

                Assert.Empty(shell.Resources.MergedDictionaries);
                Assert.Same(window.FindResource("PrimaryButtonStyle"), shell.FindResource("PrimaryButtonStyle"));
                Assert.Same(window.FindResource("ShellBorderBrush"), shell.FindResource("ShellBorderBrush"));

                // Y el consumidor construido en CODIGO, que no mergea nada, sigue resolviendo: ese es el respaldo.
                var standalone = Measured<RackBoundedEditorShell>(s => s.Parameters = new StackPanel());

                Assert.Single(standalone.Resources.MergedDictionaries);
                Assert.Equal(ShellResourcesProbe.ZoneSpacing(), standalone.TryFindResource("ShellZoneSpacing"));
            });
        }

        [Fact]
        public void TheActionsAreInsetFromTheShellEdgesByTheSharedZoneSpacing()
        {
            StaTestRunner.Run(() =>
            {
                const double width = 1000.0;
                const double height = 700.0;

                var button = new Button { Content = "Insertar", Width = 110 };
                var shell = Measured<RackBoundedEditorShell>(
                    s =>
                    {
                        s.Parameters = new StackPanel();
                        s.Preview = new Canvas();
                        s.Actions = new StackPanel
                        {
                            Orientation = Orientation.Horizontal,
                            HorizontalAlignment = HorizontalAlignment.Right,
                            Children = { button }
                        };
                    },
                    width,
                    height);

                var spacing = (Thickness)ShellResourcesProbe.ZoneSpacing();
                var corner = button.TransformToAncestor(shell).Transform(new Point(button.ActualWidth, button.ActualHeight));

                Assert.True(spacing.Right > 0.0 && spacing.Bottom > 0.0);
                Assert.Equal(spacing.Right, width - corner.X, 3);
                Assert.Equal(spacing.Bottom, height - corner.Y, 3);
            });
        }

        private static class ShellResourcesProbe
        {
            public static object ZoneSpacing() => AppStyles()["ShellZoneSpacing"];
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
