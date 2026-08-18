using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using RackCad.Application.Catalogs;
using RackCad.Application.StructuralSections;
using RackCad.Domain.Systems.Cantilever;
using RackCad.UI.Systems.Cantilever.Components;
using RackCad.UI.Systems.Larguero;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// CARACTERIZACION de las CINCO ventanas del arquetipo B que hoy no tienen ninguna cobertura de contrato:
    /// los cuatro configuradores de componente Cantilever y el Larguero. La sexta, el inspector de secciones,
    /// ya la fijo I-39A en <see cref="StructuralSectionInspectorWindowTests"/> y no se repite aqui.
    ///
    /// <para>Escrita y verde ANTES de migrar, y NO editada despues (ADR-0029 D13). Fija el comportamiento
    /// observable REAL, no el deseable: donde el contrato describe algo distinto de lo que la ventana exhibe hoy,
    /// lo que se fija aqui es lo ACTUAL. Un cambio funcional no se disfraza de caracterizacion.</para>
    ///
    /// <para>La suite hermana <c>CantileverComponentEditorTests</c> cubre lo FUNCIONAL de los cuatro
    /// componentes —modelo devuelto, receta, firma del plan, identidad de insercion— y no toca ninguna de las
    /// dimensiones del contrato: tamano, teclado, foco, cierre, dirty, motivos de bloqueo ni composicion. Esta
    /// suite existe exactamente para eso.</para>
    ///
    /// <para>Las pruebas son deliberadamente CIEGAS a la composicion: localizan los controles por el contenido
    /// de sus botones y por sus <c>x:Name</c>, nunca por la forma del arbol, porque la forma del arbol es
    /// exactamente lo que la migracion cambia.</para>
    /// </summary>
    public sealed class BoundedEditorCharacterizationTests
    {
        private const string ColumnW = "AISC-W-W10X33";
        private const string ArmHss = "AISC-HSS-RECT-HSS4X4X_250";

        /// <summary>
        /// Marca de una caracterizacion que I-39C cambio A PROPOSITO. La prueba NO se reescribe: se conserva con su
        /// cuerpo intacto como evidencia versionada del comportamiento anterior, y el comportamiento nuevo se prueba
        /// en <c>BoundedEditorContractTests</c>.
        ///
        /// <para>La transicion se lee en tres sitios: aqui esta el comportamiento anterior; ADR-0029 lo autoriza; y
        /// la clase de contrato prueba el nuevo. El commit <c>7cc6260</c> es la version en que toda esta suite corria
        /// en verde contra la base, y <c>docs/automation/evidence/I-39C-caracterizacion-base-vs-contrato.md</c>
        /// enfrenta las dos versiones asercion por asercion.</para>
        /// </summary>
        private const string BaseEvidence =
            "Evidencia de la BASE anterior a I-39C; el contrato nuevo lo prueba BoundedEditorContractTests. " +
            "Ver docs/automation/evidence/I-39C-caracterizacion-base-vs-contrato.md";

        private static StructuralSectionCatalog Catalog() =>
            new CsvStructuralSectionCatalogProvider(CatalogDirectory.Resolve()).Load();

        private static CantileverStationColumnBaseTemplateDesign ColumnBaseTemplate()
        {
            var template = new CantileverStationColumnBaseTemplateDesign
            {
                ColumnSectionId = ColumnW,
                Base = new CantileverBaseDesign { SectionId = ColumnW, Length = 48.0 }
            };

            template.Connection.Punches.ColumnBottomPlateEndOffset = 1.5;
            template.Connection.Punches.ColumnTopPunchOffset = 4.0;
            return template;
        }

        private static CantileverArmTemplateDesign ArmTemplate() => new CantileverArmTemplateDesign
        {
            Body = new CantileverArmBodyDesign { SectionId = ArmHss, CutLength = 36.0 },
            MountingPlate = new CantileverArmMountingPlateTemplateDesign
            {
                VerticalPunchCount = 2,
                VerticalEndOffset = 1.5
            }
        };

        /// <summary>Las cuatro ventanas de componente, cada una con el argumento minimo que su constructor exige.</summary>
        private static Window[] FourCantileverComponents()
        {
            var catalogue = Catalog();
            return new Window[]
            {
                new CantileverColumnBaseWindow(ColumnBaseTemplate(), catalogue, canInsertInAutoCad: true),
                new CantileverArmWindow(ArmTemplate(), ColumnBaseTemplate(), catalogue),
                new CantileverSeparatorWindow(new CantileverBracingDesign(), catalogue),
                new CantileverBraceWindow(new CantileverBracingDesign(), catalogue)
            };
        }

        private static ResourceDictionary AppStyles()
            => new ResourceDictionary { Source = new Uri("/RackCad.UI;component/Themes/AppStyles.xaml", UriKind.Relative) };

        private static DirectoryInfo RepoRoot()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "RackCad.sln"))) dir = dir.Parent;
            Assert.True(dir != null, "repo root (RackCad.sln) not found");
            return dir;
        }

        private static string UiSource(params string[] relative)
        {
            var parts = new[] { RepoRoot().FullName, "src", "RackCad.UI" }.Concat(relative).ToArray();
            var path = Path.Combine(parts);
            Assert.True(File.Exists(path), $"missing source {path}");
            return File.ReadAllText(path);
        }

        private static string ComponentXaml(string name)
            => UiSource("Systems", "Cantilever", "Components", name);

        private static Button Action(Window window, string content)
            => EditorWindowTestSupport.Find<Button>(window, b => (b.Content as string) == content);

        /// <summary>Lays out the window's CONTENT, which is what actually gives a preview canvas a size: a window that
        /// is never shown does not lay its own content out. Deliberately reads <c>Content</c> as a
        /// <see cref="FrameworkElement"/> and nothing more specific, so the helper survives the migration that
        /// replaces exactly that element.</summary>
        private static void Layout(Window window, double width, double height)
        {
            var root = (FrameworkElement)window.Content;
            root.Measure(new Size(width, height));
            root.Arrange(new Rect(0, 0, width, height));
            root.UpdateLayout();
        }

        /// <summary>Whether <paramref name="type"/> itself overrides <paramref name="name"/> — the question a close
        /// policy asks: is there ANY interception point of its own, or does the window inherit Window's default?</summary>
        private static bool Overrides(Type type, string name)
            => type.GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly) != null;

        // ================================================================================================
        // 1. TAMANO. La anomalia que I-39A midio y dejo asignada: las cuatro declaran un ancho que su minimo
        //    HEREDADO clampea, de modo que ninguna abre en el tamano que escribe.
        // ================================================================================================

        [Fact(Skip = BaseEvidence)]
        public void LasCuatroCantileverAplicanElContratoDeTamanoDelArquetipoRICO()
        {
            StaTestRunner.Run(() =>
            {
                var expected = (Style)AppStyles()["EditorShellWindowStyle"];

                foreach (var window in FourCantileverComponents())
                {
                    // El estilo se resuelve por DynamicResource desde el diccionario que la propia ventana mergea,
                    // asi que la instancia no es la misma; lo que se caracteriza es que aplica EL contrato del
                    // arquetipo A, y eso se ve en los minimos que hereda.
                    Assert.NotNull(window.Style);
                    Assert.Equal((double)AppStyles()["ShellMinWidth"], window.MinWidth);
                    Assert.Equal((double)AppStyles()["ShellMinHeight"], window.MinHeight);
                    Assert.Equal(expected.TargetType, window.Style.TargetType);
                }
            });
        }

        [Theory(Skip = BaseEvidence)]
        [InlineData(0, 1000.0, 700.0)]
        [InlineData(1, 1000.0, 700.0)]
        [InlineData(2, 900.0, 640.0)]
        [InlineData(3, 900.0, 640.0)]
        public void CadaCantileverDeclaraUnTamanoQueSuMinimoHeredadoNoRESPETA(int index, double declaredWidth, double declaredHeight)
        {
            StaTestRunner.Run(() =>
            {
                var window = FourCantileverComponents()[index];

                Assert.Equal(declaredWidth, window.Width);
                Assert.Equal(declaredHeight, window.Height);

                // Letra muerta: el ancho declarado SIEMPRE pierde contra el minimo, y en dos de las cuatro
                // tambien el alto. Lo que el usuario ve al abrir es el maximo de los dos.
                Assert.True(window.MinWidth > window.Width);
                Assert.Equal(1120.0, Math.Max(window.Width, window.MinWidth));
                Assert.Equal(index < 2 ? 700.0 : 672.0, Math.Max(window.Height, window.MinHeight));
            });
        }

        [Fact(Skip = BaseEvidence)]
        public void ElLargueroDeclaraSuTamanoAManoYNoAplicaNingunEstiloDeVentana()
        {
            StaTestRunner.Run(() =>
            {
                var window = new RackLargueroWindow();

                Assert.Null(window.Style);
                Assert.Equal(720.0, window.Width);
                Assert.Equal(440.0, window.Height);
                Assert.Equal(640.0, window.MinWidth);
                Assert.Equal(380.0, window.MinHeight);
            });
        }

        [Fact(Skip = BaseEvidence)]
        public void NingunaDeLasCincoLeeLosTokensDelArquetipoB()
        {
            StaTestRunner.Run(() =>
            {
                var styles = AppStyles();
                var minWidth = (double)styles["BoundedEditorMinWidth"];
                var minHeight = (double)styles["BoundedEditorMinHeight"];

                foreach (var window in FourCantileverComponents().Concat(new Window[] { new RackLargueroWindow() }))
                {
                    Assert.NotEqual(minWidth, window.MinWidth);
                    Assert.NotEqual(minHeight, window.MinHeight);
                }
            });
        }

        // ================================================================================================
        // 2. UBICACION Y OWNERSHIP
        // ================================================================================================

        [Fact]
        public void LasCincoSeCentranEnSuVentanaPadre()
        {
            StaTestRunner.Run(() =>
            {
                foreach (var window in FourCantileverComponents().Concat(new Window[] { new RackLargueroWindow() }))
                {
                    Assert.Equal(WindowStartupLocation.CenterOwner, window.WindowStartupLocation);
                }
            });
        }

        // ================================================================================================
        // 3. FOCO INICIAL
        // ================================================================================================

        [Fact(Skip = BaseEvidence)]
        public void NingunaDeLasCuatroCantileverDeclaraFocoInicial()
        {
            foreach (var name in new[]
            {
                "CantileverColumnBaseWindow.xaml", "CantileverArmWindow.xaml",
                "CantileverSeparatorWindow.xaml", "CantileverBraceWindow.xaml"
            })
            {
                Assert.DoesNotContain("FocusManager", ComponentXaml(name), StringComparison.Ordinal);
            }
        }

        [Fact]
        public void ElLargueroDeclaraSuFocoInicialEnElNombre()
        {
            // Se lee del XAML y no de FocusManager.GetFocusedElement: la declaracion es un Binding por ElementName
            // que no resuelve hasta que la ventana se muestra, y una caracterizacion no debe mostrar ventanas.
            Assert.Contains(
                "FocusManager.FocusedElement=\"{Binding ElementName=NameBox}\"",
                UiSource("Systems", "Larguero", "RackLargueroWindow.xaml"),
                StringComparison.Ordinal);
        }

        // ================================================================================================
        // 4. ACCION POR DEFECTO Y ACCION DE CANCELACION (ADR-0029 D6 y D7)
        // ================================================================================================

        [Fact]
        public void LasCuatroCantileverNoTienenNingunaAccionPorDefecto()
        {
            StaTestRunner.Run(() =>
            {
                foreach (var window in FourCantileverComponents())
                {
                    var buttons = EditorWindowTestSupport.FindAll<ButtonBase>(window);
                    Assert.NotEmpty(buttons);
                    Assert.DoesNotContain(buttons, b => b is Button button && button.IsDefault);
                }
            });
        }

        [Fact]
        public void LasCuatroCantileverCancelanConCancelarYEsSuUnicoIsCancel()
        {
            StaTestRunner.Run(() =>
            {
                foreach (var window in FourCantileverComponents())
                {
                    var cancels = EditorWindowTestSupport.FindAll<Button>(window).Where(b => b.IsCancel).ToList();

                    Assert.Single(cancels);
                    Assert.Equal("Cancelar", cancels[0].Content as string);
                }
            });
        }

        [Fact]
        public void LasCuatroCantileverOfrecenLasMismasCuatroAcciones()
        {
            StaTestRunner.Run(() =>
            {
                var bar = new[] { "Restaurar", "Insertar sólo esta pieza", "Aceptar", "Cancelar" };

                foreach (var window in FourCantileverComponents())
                {
                    // Solo Button: las casillas y los radios de vista derivan de ButtonBase y no son acciones.
                    var labels = EditorWindowTestSupport.FindAll<Button>(window)
                        .Select(b => b.Content as string)
                        .Where(text => text != null)
                        .ToArray();

                    // La barra de acciones es la MISMA en las cuatro y siempre va al final del recorrido.
                    Assert.Equal(bar, labels.Skip(labels.Length - bar.Length).ToArray());

                    // Y solo la columna-base tiene un boton mas, que NO es una accion de la barra sino un atajo
                    // del formulario: vive entre los parametros y actua sobre la seccion elegida.
                    Assert.Equal(
                        window is CantileverColumnBaseWindow ? new[] { "Usar misma sección" } : Array.Empty<string>(),
                        labels.Take(labels.Length - bar.Length).ToArray());
                }
            });
        }

        [Fact]
        public void ElLargueroNoTieneAccionPorDefectoYCancelaConCerrar()
        {
            StaTestRunner.Run(() =>
            {
                var window = new RackLargueroWindow();
                var buttons = EditorWindowTestSupport.FindAll<Button>(window);

                Assert.Equal(
                    new[] { "Ver lista de materiales", "Guardar en biblioteca", "Cerrar" },
                    buttons.Select(b => b.Content as string).ToArray());

                Assert.DoesNotContain(buttons, b => b.IsDefault);
                var cancel = Assert.Single(buttons, b => b.IsCancel);
                Assert.Equal("Cerrar", cancel.Content as string);
            });
        }

        // ================================================================================================
        // 5. CAMINOS DE CIERRE Y DIRTY (ADR-0029 D7 y D8)
        // ================================================================================================

        [Fact]
        public void NingunaDeLasCincoIntercEptaSuPropioCierre()
        {
            // El estado de partida: ninguna ventana del arquetipo B tiene punto de interceptacion propio, asi que
            // los cuatro caminos de cierre —boton, Escape, la X y Alt+F4— caen directo en el cierre de Window.
            foreach (var type in new[]
            {
                typeof(CantileverColumnBaseWindow), typeof(CantileverArmWindow),
                typeof(CantileverSeparatorWindow), typeof(CantileverBraceWindow),
                typeof(RackLargueroWindow)
            })
            {
                Assert.False(Overrides(type, "OnClosing"), $"{type.Name} ya intercepta OnClosing");
                Assert.False(Overrides(type, "OnClosed"), $"{type.Name} ya intercepta OnClosed");
            }
        }

        [Fact]
        public void NingunaDeLasCincoDeclaraUnAmbitoTransaccionalPendiente()
        {
            // «No aplicable» es un valor legitimo de D8: estas cinco ventanas no tienen dirty declarado, y las
            // cuatro Cantilever trabajan sobre una COPIA que solo se devuelve al aceptar.
            var declared = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

            foreach (var type in new[]
            {
                typeof(CantileverColumnBaseWindow), typeof(CantileverArmWindow),
                typeof(CantileverSeparatorWindow), typeof(CantileverBraceWindow),
                typeof(RackLargueroWindow)
            })
            {
                Assert.DoesNotContain(
                    type.GetProperties(declared).Select(p => p.Name).Concat(type.GetMethods(declared).Select(m => m.Name)),
                    name => name.Contains("Pending", StringComparison.Ordinal)
                        || name.Contains("Unsaved", StringComparison.Ordinal)
                        || name.Contains("Dirty", StringComparison.Ordinal));
            }
        }

        // ================================================================================================
        // 6. ACCIONES BLOQUEADAS Y SU MOTIVO (ADR-0029 D6)
        // ================================================================================================

        [Fact]
        public void ElUnicoMotivoQueApagaInsertarEsAbrirFueraDeAutoCad()
        {
            StaTestRunner.Run(() =>
            {
                // Columna-base y brazo reciben el unico motivo de bloqueo que hoy existe, y lo dicen: el boton se
                // apaga con la razon en el ToolTip, visible aun deshabilitado.
                var window = new CantileverColumnBaseWindow(ColumnBaseTemplate(), Catalog(), canInsertInAutoCad: false);
                var insert = Action(window, "Insertar sólo esta pieza");

                Assert.False(insert.IsEnabled);
                Assert.True(ToolTipService.GetShowOnDisabled(insert));
                Assert.Equal("Disponible solo cuando la ventana se abre desde AutoCAD.", insert.ToolTip as string);
            });
        }

        [Fact(Skip = BaseEvidence)]
        public void SeparadorYTensorDejanInsertarHabilitadoSinLineaResuelta()
        {
            StaTestRunner.Run(() =>
            {
                // El separador y el tensor solo existen DENTRO de un intervalo: sin linea resuelta no hay nada que
                // insertar, y sus constructores lo saben (resolved == null). Aun asi el boton queda HABILITADO y su
                // ayuda DESCRIBE la accion en vez de dar el motivo: pulsarlo no produce ninguna insercion.
                foreach (var window in new Window[]
                {
                    new CantileverSeparatorWindow(new CantileverBracingDesign(), Catalog()),
                    new CantileverBraceWindow(new CantileverBracingDesign(), Catalog())
                })
                {
                    var insert = Action(window, "Insertar sólo esta pieza");

                    Assert.True(insert.IsEnabled);
                    Assert.StartsWith("Dibuja la pieza sola", insert.ToolTip as string, StringComparison.Ordinal);
                    Assert.DoesNotContain("resuelta", insert.ToolTip as string, StringComparison.OrdinalIgnoreCase);
                }
            });
        }

        [Fact]
        public void LasAccionesDelLargueroNuncaSeApaganYElFalloSeCuentaEnElEstado()
        {
            StaTestRunner.Run(() =>
            {
                var window = new RackLargueroWindow();

                foreach (var button in EditorWindowTestSupport.FindAll<Button>(window))
                {
                    Assert.True(button.IsEnabled);
                    Assert.Null(button.ToolTip);
                }
            });
        }

        // ================================================================================================
        // 7. PREVIEW: AUTORIDAD Y FRESCURA (ADR-0029 D4)
        // ================================================================================================

        [Fact]
        public void ElPreviewDelLargueroSeDerivaDeLaCapturaYNoDeclaraNingunaFrescura()
        {
            StaTestRunner.Run(() =>
            {
                var window = new RackLargueroWindow();
                Layout(window, 720, 440);

                var canvas = (Canvas)window.FindName("PreviewCanvas");
                var status = (TextBlock)window.FindName("StatusText");
                Assert.NotEmpty(canvas.Children);

                // Longitud invalida: el dibujo se REHACE igual, rotulado «(longitud)». No conserva el ultimo
                // valido, no se marca obsoleto y no dice nada en el estado.
                var length = (TextBox)window.FindName("LengthBox");
                length.Text = "no es un numero";
                length.RaiseEvent(new RoutedEventArgs(UIElement.LostFocusEvent, length));

                var labels = EditorWindowTestSupport.FindAll<TextBlock>(canvas).Select(t => t.Text).ToArray();
                Assert.Contains("(longitud)", labels);
                Assert.Equal(string.Empty, status.Text);
            });
        }

        // ================================================================================================
        // 8. COMPOSICION. Lo que la migracion cambia, fijado tal como esta hoy.
        // ================================================================================================

        [Fact(Skip = BaseEvidence)]
        public void LosCuatroXAMLNombranUnShellConNombreDeSISTEMA()
        {
            foreach (var name in new[]
            {
                "CantileverColumnBaseWindow.xaml", "CantileverArmWindow.xaml",
                "CantileverSeparatorWindow.xaml", "CantileverBraceWindow.xaml"
            })
            {
                Assert.Contains("components:CantileverComponentEditorShell", ComponentXaml(name), StringComparison.Ordinal);
            }
        }

        [Fact(Skip = BaseEvidence)]
        public void ElLargueroNoUsaNingunShellYArmaSuChromeAMano()
        {
            var source = UiSource("Systems", "Larguero", "RackLargueroWindow.xaml");

            Assert.DoesNotContain("Shell", source, StringComparison.Ordinal);

            // Colores literales en vez de tokens compartidos: el tercer chrome que el censo de I-39A registro.
            foreach (var literal in new[] { "#1F2933", "#9AA7B4", "#617080", "#D8DEE6" })
            {
                Assert.Contains(literal, source, StringComparison.Ordinal);
            }
        }
    }
}
