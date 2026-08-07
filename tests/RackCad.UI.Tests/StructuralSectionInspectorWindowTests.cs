using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using RackCad.Application.Catalogs;
using RackCad.Application.StructuralSections;
using RackCad.Application.StructuralSections.Geometry;
using RackCad.UI.Controls;
using RackCad.UI.StructuralSections;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// CARACTERIZACION del piloto de I-39A: <see cref="StructuralSectionInspectorWindow"/>.
    ///
    /// <para>Escrita y verde ANTES de migrar la ventana al shell del arquetipo B, y NO editada despues. Fija el
    /// comportamiento observable REAL, no el deseable: donde ADR-0029 describe un contrato distinto del que la
    /// ventana exhibe hoy y no existe autorizacion para cambiarlo en I-39A, lo que se fija aqui es el actual y la
    /// deuda queda registrada. Un cambio funcional no se disfraza de caracterizacion (decision 19 del Owner).</para>
    ///
    /// <para>Las pruebas son deliberadamente CIEGAS a la composicion: localizan los controles por su panel de
    /// captura y por el contenido de sus botones, nunca por la forma del arbol, porque la forma del arbol es
    /// exactamente lo que la migracion cambia.</para>
    ///
    /// <para>Lo que la suite hermana <c>StructuralSectionInspectorTests</c> ya cubre sobre el ESTADO puro no se
    /// repite aqui: esta suite existe para el cableado, el teclado, el foco, el tamano y los caminos de salida,
    /// que no tenian ninguna cobertura.</para>
    /// </summary>
    public sealed class StructuralSectionInspectorWindowTests
    {
        private static StructuralSectionCatalog Catalog() =>
            new CsvStructuralSectionCatalogProvider(CatalogDirectory.Resolve()).Load();

        /// <summary>The window populates its list in its <c>Loaded</c> handler, so a test that never shows a window
        /// must raise it: otherwise the list is empty and every filter handler returns early by design.</summary>
        private static StructuralSectionInspectorWindow Loaded()
        {
            var window = new StructuralSectionInspectorWindow(Catalog());
            window.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent, window));
            return window;
        }

        /// <summary>The capture panel, located by a label it owns. Its child ORDER is part of the contract this
        /// suite pins, so the panel is found once and read positionally from there.</summary>
        private static StackPanel Parameters(Window window) =>
            EditorWindowTestSupport.FindAll<StackPanel>(window)
                .First(panel => panel.Children.OfType<TextBlock>()
                    .Any(label => label.Text == "Buscar designacion"));

        private static IReadOnlyList<T> In<T>(Panel panel) where T : DependencyObject =>
            panel.Children.OfType<T>().ToList();

        private static ButtonBase Action(Window window, string content) =>
            EditorWindowTestSupport.Find<ButtonBase>(window, b => (b.Content as string) == content);

        /// <summary>
        /// Clicks an action and swallows ONLY the <see cref="InvalidOperationException"/> that
        /// <c>Window.DialogResult</c> raises outside a dialog.
        ///
        /// <para>Both actions end by assigning <c>DialogResult</c>, whose setter throws unless the window was opened
        /// with <c>ShowDialog</c>; in production AutoCAD opens it with <c>ShowModalWindow</c>. Everything the caller
        /// actually reads — <c>Result</c> and <c>AcceptedSection</c> — is assigned BEFORE that line, so swallowing
        /// it characterises the output contract without showing a modal window in a test.</para>
        /// </summary>
        private static void Click(Window window, string content)
        {
            var button = Action(window, content);
            Assert.NotNull(button);

            try
            {
                button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent, button));
            }
            catch (InvalidOperationException)
            {
            }
        }

        // ---- 1. La ventana: identidad, tamano y ubicacion ------------------------------------------------

        [Fact]
        public void TheWindowKeepsItsTitleSizeAndStartupLocation()
        {
            StaTestRunner.Run(() =>
            {
                var window = new StructuralSectionInspectorWindow(Catalog());

                Assert.Equal("Secciones estructurales", window.Title);
                Assert.Equal(1040.0, window.Width);
                Assert.Equal(680.0, window.Height);
                Assert.Equal(820.0, window.MinWidth);
                Assert.Equal(520.0, window.MinHeight);

                // CenterScreen, y no CenterOwner: el inspector lo abre un comando del Plugin y no recibe Owner.
                Assert.Equal(WindowStartupLocation.CenterScreen, window.WindowStartupLocation);
            });
        }

        [Fact]
        public void TheWindowMatchesTheSizeContractOfItsArchetype()
        {
            StaTestRunner.Run(() =>
            {
                var styles = new ResourceDictionary
                {
                    Source = new Uri("/RackCad.UI;component/Themes/AppStyles.xaml", UriKind.Relative)
                };
                var window = new StructuralSectionInspectorWindow(Catalog());

                Assert.Equal((double)styles["BoundedEditorInitialWidth"], window.Width);
                Assert.Equal((double)styles["BoundedEditorInitialHeight"], window.Height);
                Assert.Equal((double)styles["BoundedEditorMinWidth"], window.MinWidth);
                Assert.Equal((double)styles["BoundedEditorMinHeight"], window.MinHeight);
            });
        }

        // ---- 2. El panel de captura: que controles hay y en que orden -----------------------------------

        [Fact]
        public void TheCapturePanelKeepsItsLabelsInOrder()
        {
            StaTestRunner.Run(() =>
            {
                var labels = In<TextBlock>(Parameters(Loaded())).Select(t => t.Text).ToArray();

                Assert.Equal(
                    new[]
                    {
                        "Buscar designacion", "Familia", "Seccion", "Longitud (pulgadas)",
                        "Vista", "Detalle", "Representacion", "Rotacion (grados)"
                    },
                    labels);
            });
        }

        [Fact]
        public void TheCapturePanelKeepsItsControlsInOrder()
        {
            StaTestRunner.Run(() =>
            {
                var panel = Parameters(Loaded());

                Assert.Equal(3, In<TextBox>(panel).Count);
                Assert.Equal(4, In<ComboBox>(panel).Count);
                Assert.Single(In<ListBox>(panel));

                Assert.Equal(
                    new[] { "Espejo", "Mostrar eje", "Mostrar envolvente" },
                    In<CheckBox>(panel).Select(c => c.Content as string).ToArray());
            });
        }

        [Fact]
        public void TheFamilyComboOffersEveryFamilyBehindAnAllEntry()
        {
            StaTestRunner.Run(() =>
            {
                var family = In<ComboBox>(Parameters(Loaded()))[0];

                Assert.Equal(
                    new[]
                    {
                        "Todas las familias", "W — ala ancha", "HSS rectangular y cuadrado",
                        "C — canal", "L — angulo", "S / IPS — viga estandar americana"
                    },
                    family.Items.OfType<ComboBoxItem>().Select(i => i.Content as string).ToArray());

                Assert.Equal(0, family.SelectedIndex);
            });
        }

        [Fact]
        public void TheOptionCombosKeepTheirItemsAndTheirDefaults()
        {
            StaTestRunner.Run(() =>
            {
                var combos = In<ComboBox>(Parameters(Loaded()));

                Assert.Equal(
                    new[] { "Seccion", "Longitudinal X", "Longitudinal Y", "Isometrica" },
                    combos[1].Items.OfType<ComboBoxItem>().Select(i => i.Content as string).ToArray());
                Assert.Equal(
                    new[] { "Tabulada", "Simplificada" },
                    combos[2].Items.OfType<ComboBoxItem>().Select(i => i.Content as string).ToArray());
                Assert.Equal(
                    new[] { "Wireframe", "Envolvente", "Eje" },
                    combos[3].Items.OfType<ComboBoxItem>().Select(i => i.Content as string).ToArray());

                Assert.Equal(0, combos[1].SelectedIndex);
                Assert.Equal(0, combos[2].SelectedIndex);
                Assert.Equal(0, combos[3].SelectedIndex);
            });
        }

        // ---- 3. Cableado: cada control llega a su propiedad de estado -----------------------------------

        [Fact]
        public void TheListShowsTheWholeCatalogueAndSelectingARowReachesTheState()
        {
            StaTestRunner.Run(() =>
            {
                var window = Loaded();
                var list = In<ListBox>(Parameters(window)).Single();

                Assert.Equal(window.State.Matches().Count, list.Items.Count);

                list.SelectedIndex = 5;
                var chosen = (StructuralSectionDefinition)((ListBoxItem)list.Items[5]).Tag;

                Assert.Same(chosen, window.State.Selected);
            });
        }

        [Fact]
        public void TypingASearchNarrowsTheListThroughTheState()
        {
            StaTestRunner.Run(() =>
            {
                var window = Loaded();
                var panel = Parameters(window);
                var search = In<TextBox>(panel)[0];
                var list = In<ListBox>(panel).Single();

                search.Text = "W12X26";

                Assert.Equal("W12X26", window.State.Search);
                Assert.Equal(window.State.Matches().Count, list.Items.Count);
                Assert.True(list.Items.Count < window.State.Catalog.Count);
            });
        }

        [Fact]
        public void ASearchThatMatchesNothingEmptiesTheListAndLeavesNoSelection()
        {
            StaTestRunner.Run(() =>
            {
                var window = Loaded();
                var panel = Parameters(window);

                In<TextBox>(panel)[0].Text = "ZZZZ-NO-EXISTE";

                Assert.Empty(In<ListBox>(panel).Single().Items);
                Assert.False(window.State.HasSelection);
            });
        }

        [Fact]
        public void ChoosingAFamilyReachesTheState()
        {
            StaTestRunner.Run(() =>
            {
                var window = Loaded();
                var family = In<ComboBox>(Parameters(window))[0];

                family.SelectedIndex = 5;

                Assert.Equal(StructuralSectionFamily.S, window.State.Family);
            });
        }

        [Fact]
        public void ViewDetailAndModeReachTheState()
        {
            StaTestRunner.Run(() =>
            {
                var window = Loaded();
                var combos = In<ComboBox>(Parameters(window));

                combos[1].SelectedIndex = 3;
                combos[2].SelectedIndex = 1;
                combos[3].SelectedIndex = 2;

                Assert.Equal(SectionViewKind.Isometric, window.State.View);
                Assert.Equal(SectionDetailLevel.Simplified, window.State.Detail);
                Assert.Equal(SectionRepresentationMode.Axis, window.State.Mode);
            });
        }

        [Fact]
        public void TheThreeCheckboxesReachTheState()
        {
            StaTestRunner.Run(() =>
            {
                var window = Loaded();
                var boxes = In<CheckBox>(Parameters(window));

                foreach (var box in boxes)
                {
                    box.IsChecked = true;
                }

                Assert.True(window.State.Mirrored);
                Assert.True(window.State.ShowAxis);
                Assert.True(window.State.ShowEnvelope);

                foreach (var box in boxes)
                {
                    box.IsChecked = false;
                }

                Assert.False(window.State.Mirrored);
                Assert.False(window.State.ShowAxis);
                Assert.False(window.State.ShowEnvelope);
            });
        }

        // ---- 4. Entradas invalidas: el valor aplicado valido sobrevive ----------------------------------

        [Fact]
        public void TheLengthBoxIsSeededWithTheDefaultInTheCurrentCulture()
        {
            StaTestRunner.Run(() =>
            {
                var length = In<TextBox>(Parameters(Loaded()))[1];

                Assert.Equal(
                    StructuralSectionInspectorState.DefaultLengthInches.ToString(CultureInfo.CurrentCulture),
                    length.Text);
            });
        }

        [Theory]
        [InlineData("0")]
        [InlineData("-5")]
        [InlineData("abc")]
        [InlineData("")]
        public void AnInvalidLengthIsRejectedTheValidOneSurvivesAndTheFieldWarns(string text)
        {
            StaTestRunner.Run(() =>
            {
                var window = Loaded();
                var length = In<TextBox>(Parameters(window))[1];

                length.Text = "144";
                Assert.Equal(144.0, window.State.Length);

                length.Text = text;

                Assert.Equal(144.0, window.State.Length);
                Assert.Same(PreviewPalette.Warning, length.Foreground);
            });
        }

        [Fact]
        public void AnInvalidRotationWarnsAndLeavesTheAppliedValueUntouched()
        {
            StaTestRunner.Run(() =>
            {
                var window = Loaded();
                var rotation = In<TextBox>(Parameters(window))[2];

                rotation.Text = "45";
                Assert.Equal(45.0, window.State.RotationDegrees);

                rotation.Text = "no-es-un-numero";

                Assert.Equal(45.0, window.State.RotationDegrees);
                Assert.Same(PreviewPalette.Warning, rotation.Foreground);
            });
        }

        // ---- 5. El preview: por TIPO, nunca por x:Name --------------------------------------------------

        [Fact]
        public void ThePreviewIsTheSharedPreviewCanvasType()
        {
            StaTestRunner.Run(() =>
            {
                // El inspector es el UNICO consumidor productivo del TIPO PreviewCanvas: lo consume por herencia,
                // via StructuralSectionPreview. Las otras diez previews son un Canvas ordinario homonimo.
                var preview = EditorWindowTestSupport.Find<StructuralSectionPreview>(Loaded());

                Assert.NotNull(preview);
                Assert.IsAssignableFrom<PreviewCanvas>(preview);
                Assert.Equal(320.0, preview.MinHeight);
            });
        }

        [Fact]
        public void WithoutSelectionThePreviewIsEmptied()
        {
            StaTestRunner.Run(() =>
            {
                var window = Loaded();
                var preview = EditorWindowTestSupport.Find<StructuralSectionPreview>(window);

                In<TextBox>(Parameters(window))[0].Text = "ZZZZ-NO-EXISTE";

                // Autoridad: derivada del borrador. Frescura: NO disponible, nunca "ultimo valido obsoleto":
                // Redraw() limpia el lienzo como primera linea, asi que aqui no queda imagen anterior.
                Assert.False(window.State.HasSelection);
                Assert.Null(preview.Plan);
            });
        }

        // ---- 6. Acciones, teclado y caminos de salida ---------------------------------------------------

        [Fact]
        public void TheTwoActionsAreInsertarAndCerrarWithTheirKeyboardRoles()
        {
            StaTestRunner.Run(() =>
            {
                var window = Loaded();

                var insert = (Button)Action(window, "Insertar");
                var close = (Button)Action(window, "Cerrar");

                // Enter dispara Insertar y Escape dispara Cerrar. Es el contrato de teclado vigente y lo unico
                // que hoy lo expresa: EditorActions.Button no sabe fijar IsDefault ni IsCancel, asi que adoptar
                // esa fabrica en el piloto ROMPERIA Enter y Escape. Queda como deuda de I-39C.
                Assert.True(insert.IsDefault);
                Assert.False(insert.IsCancel);
                Assert.True(close.IsCancel);
                Assert.False(close.IsDefault);

                Assert.True(insert.IsEnabled);
                Assert.True(close.IsEnabled);
            });
        }

        /// <summary>
        /// Marca de la UNICA caracterizacion de esta suite que I-39C cambio a proposito, con la autorizacion que su
        /// propio comentario anticipaba: I-39A midio que <c>Insertar</c> no se deshabilitaba nunca y que sin
        /// seleccion era un no-op silencioso, y dejo la deuda asignada a la subiniciativa del arquetipo. La prueba NO
        /// se reescribe: se conserva intacta como evidencia versionada del comportamiento anterior, y el nuevo lo
        /// prueba <c>BoundedEditorContractTests</c>. Las otras veintinueve siguen corriendo sin tocarse.
        /// </summary>
        private const string BaseEvidenceI39C =
            "Evidencia de la BASE anterior a I-39C, que paga la deuda que esta misma prueba registraba; " +
            "el contrato nuevo lo prueba BoundedEditorContractTests. " +
            "Ver docs/automation/evidence/I-39C-caracterizacion-base-vs-contrato.md";

        [Fact(Skip = BaseEvidenceI39C)]
        public void InsertarIsNeverDisabledAndWithoutSelectionItIsASilentNoOp()
        {
            StaTestRunner.Run(() =>
            {
                var window = Loaded();

                In<TextBox>(Parameters(window))[0].Text = "ZZZZ-NO-EXISTE";
                Assert.False(window.State.HasSelection);

                var insert = (Button)Action(window, "Insertar");
                Assert.True(insert.IsEnabled);
                Assert.Null(insert.ToolTip);

                Click(window, "Insertar");

                // Comportamiento ACTUAL, fijado tal cual: no materializa, no cierra y no dice nada.
                Assert.Null(window.Result);
                Assert.Null(window.AcceptedSection);
            });
        }

        [Fact]
        public void InsertarPublishesThePlanAndTheAcceptedSection()
        {
            StaTestRunner.Run(() =>
            {
                var window = Loaded();
                var list = In<ListBox>(Parameters(window)).Single();
                list.SelectedIndex = 3;
                var chosen = (StructuralSectionDefinition)((ListBoxItem)list.Items[3]).Tag;

                Click(window, "Insertar");

                Assert.NotNull(window.Result);
                Assert.Same(chosen, window.AcceptedSection);
                Assert.Equal(window.State.Length, window.Result.Length);
            });
        }

        [Fact]
        public void AnInvalidLengthOrRotationDoesNotBlockTheInsertion()
        {
            StaTestRunner.Run(() =>
            {
                var window = Loaded();
                var panel = Parameters(window);
                In<ListBox>(panel).Single().SelectedIndex = 2;

                In<TextBox>(panel)[1].Text = "144";
                In<TextBox>(panel)[1].Text = "abc";
                In<TextBox>(panel)[2].Text = "tampoco";

                Click(window, "Insertar");

                // Comportamiento ACTUAL: se materializa con el ultimo valor valido, sin aviso ni bloqueo.
                Assert.NotNull(window.Result);
                Assert.Equal(144.0, window.Result.Length);
            });
        }

        [Fact]
        public void CerrarPublishesNoResult()
        {
            StaTestRunner.Run(() =>
            {
                var window = Loaded();
                In<ListBox>(Parameters(window)).Single().SelectedIndex = 3;

                Click(window, "Cerrar");

                Assert.Null(window.Result);
            });
        }

        [Fact]
        public void ClosingByTheSystemPublishesNoResult()
        {
            StaTestRunner.Run(() =>
            {
                // Cerrar por la X (o por Alt+F4) no pasa por ningun handler: Result se queda en su valor inicial.
                // Es lo que hace seguro el contrato del llamador, que discrimina EXCLUSIVAMENTE por Result == null.
                var window = Loaded();
                In<ListBox>(Parameters(window)).Single().SelectedIndex = 3;

                window.Close();

                Assert.Null(window.Result);
            });
        }

        // ---- 7. Foco y tabulacion: se fija la AUSENCIA de contrato --------------------------------------

        [Fact]
        public void NoControlDeclaresAnExplicitTabIndex()
        {
            StaTestRunner.Run(() =>
            {
                // Hoy el orden de tabulacion es el de declaracion, porque nadie fija TabIndex. Se caracteriza la
                // ausencia: darle un orden explicito es contrato de ADR-0029 D9 y trabajo de otra subiniciativa.
                var explicitOrder = EditorWindowTestSupport.FindAll<Control>(Loaded())
                    .Where(c => c.TabIndex != int.MaxValue)
                    .ToList();

                Assert.Empty(explicitOrder);
            });
        }

        [Fact]
        public void TheWindowDeclaresNoInitialFocus()
        {
            StaTestRunner.Run(() =>
            {
                // Ninguna llamada a Focus() ni FocusManager.FocusedElement. Caracterizado, no corregido.
                var window = Loaded();

                Assert.Null(FocusManager.GetFocusedElement(window));
            });
        }

        // ---- 8. Resize: no toca el modelo ---------------------------------------------------------------

        [Fact]
        public void ResizingToTheMinimumChangesNeitherSelectionNorState()
        {
            StaTestRunner.Run(() =>
            {
                var window = Loaded();
                In<ListBox>(Parameters(window)).Single().SelectedIndex = 4;

                var selected = window.State.Selected;
                var length = window.State.Length;

                var content = (UIElement)window.Content;
                content.Measure(new Size(window.MinWidth, window.MinHeight));
                content.Arrange(new Rect(0, 0, window.MinWidth, window.MinHeight));
                content.UpdateLayout();

                Assert.Same(selected, window.State.Selected);
                Assert.Equal(length, window.State.Length);
            });
        }

        [Fact]
        public void AtTheMinimumSizeTheActionsAndTheDiagnosticsStayReachable()
        {
            StaTestRunner.Run(() =>
            {
                // Accesibilidad real, no un numero de resolucion (decision 25 del Owner): al minimo, las acciones y
                // el diagnostico siguen midiendo y siguen visibles.
                var window = Loaded();
                var content = (FrameworkElement)window.Content;
                content.Measure(new Size(window.MinWidth, window.MinHeight));
                content.Arrange(new Rect(0, 0, window.MinWidth, window.MinHeight));
                content.UpdateLayout();

                foreach (var label in new[] { "Insertar", "Cerrar" })
                {
                    var button = (Button)Action(window, label);

                    Assert.Equal(Visibility.Visible, button.Visibility);
                    Assert.True(button.ActualWidth > 0.0);
                    Assert.True(button.ActualHeight > 0.0);
                }
            });
        }
    }
}
