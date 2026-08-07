using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using RackCad.Application.Catalogs;
using RackCad.Application.StructuralSections;
using RackCad.Domain.Systems.Cantilever;
using RackCad.UI.StructuralSections;
using RackCad.UI.Systems.Cantilever.Components;
using RackCad.UI.Systems.Larguero;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// El contrato NUEVO del arquetipo B, el que I-39C establece. Vive en una clase aparte de
    /// <see cref="BoundedEditorCharacterizationTests"/> a proposito: aquella conserva intacto el comportamiento
    /// anterior —incluidas, con <c>Skip</c>, las pruebas que este cambio deja obsoletas— de modo que la transicion
    /// base → ADR → contrato se lea entera en el historial y no como una prueba reescrita.
    ///
    /// <para>Autorizado por ADR-0029: D9 (contrato de tamano por arquetipo, el B no hereda los minimos del A),
    /// D11 (adoptar antes que abstraer) y D12 (la infraestructura compartida no conoce sistemas).</para>
    /// </summary>
    public sealed class BoundedEditorContractTests
    {
        private static DirectoryInfo RepoRoot()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "RackCad.sln"))) dir = dir.Parent;
            Assert.True(dir != null, "repo root (RackCad.sln) not found");
            return dir;
        }

        private static string ComponentDirectory()
            => Path.Combine(RepoRoot().FullName, "src", "RackCad.UI", "Systems", "Cantilever", "Components");

        private static ResourceDictionary AppStyles()
            => new ResourceDictionary { Source = new Uri("/RackCad.UI;component/Themes/AppStyles.xaml", UriKind.Relative) };

        private static StructuralSectionCatalog Catalog()
            => new CsvStructuralSectionCatalogProvider(CatalogDirectory.Resolve()).Load();

        private static CantileverStationColumnBaseTemplateDesign ColumnBaseTemplate()
        {
            var template = new CantileverStationColumnBaseTemplateDesign
            {
                ColumnSectionId = "AISC-W-W10X33",
                Base = new CantileverBaseDesign { SectionId = "AISC-W-W10X33", Length = 48.0 }
            };

            template.Connection.Punches.ColumnBottomPlateEndOffset = 1.5;
            template.Connection.Punches.ColumnTopPunchOffset = 4.0;
            return template;
        }

        private static Window[] FourCantileverComponents()
        {
            var catalogue = Catalog();
            var arm = new CantileverArmTemplateDesign
            {
                Body = new CantileverArmBodyDesign { SectionId = "AISC-HSS-RECT-HSS4X4X_250", CutLength = 36.0 }
            };

            return new Window[]
            {
                new CantileverColumnBaseWindow(ColumnBaseTemplate(), catalogue, canInsertInAutoCad: true),
                new CantileverArmWindow(arm, ColumnBaseTemplate(), catalogue),
                new CantileverSeparatorWindow(new CantileverBracingDesign(), catalogue),
                new CantileverBraceWindow(new CantileverBracingDesign(), catalogue)
            };
        }

        // ---- 1. composicion: ningun shell del arquetipo B lleva nombre de sistema (D12) ----

        [Fact]
        public void LosCuatroXAMLNombranElShellNEUTRAL()
        {
            foreach (var file in Directory.GetFiles(ComponentDirectory(), "*.xaml"))
            {
                var xaml = File.ReadAllText(file);

                Assert.Contains("shell:RackBoundedEditorShell", xaml, StringComparison.Ordinal);
                Assert.Contains("xmlns:shell=\"clr-namespace:RackCad.UI.Shell\"", xaml, StringComparison.Ordinal);
                Assert.DoesNotContain("CantileverComponentEditorShell", xaml, StringComparison.Ordinal);
            }
        }

        [Fact]
        public void LaFachadaDeI39AYaNoExiste()
        {
            Assert.False(
                File.Exists(Path.Combine(ComponentDirectory(), "CantileverComponentEditorShell.cs")),
                "la fachada era un andamio con fecha de retiro escrita en su propio comentario");

            // Y no reaparece con otro nombre: nada en el ensamblado deriva del shell acotado.
            var derived = typeof(RackCad.UI.Shell.RackBoundedEditorShell).Assembly
                .GetTypes()
                .Where(type => typeof(RackCad.UI.Shell.RackBoundedEditorShell).IsAssignableFrom(type)
                    && type != typeof(RackCad.UI.Shell.RackBoundedEditorShell))
                .Select(type => type.FullName)
                .ToList();

            Assert.Empty(derived);
        }

        // ---- 2. el contrato de tamano del arquetipo B deja de ser letra muerta (D9) ----

        [Fact]
        public void LasCuatroCantileverAplicanElContratoDeTamanoDeSuPropioARQUETIPO()
        {
            StaTestRunner.Run(() =>
            {
                var styles = AppStyles();

                foreach (var window in FourCantileverComponents())
                {
                    Assert.Equal((double)styles["BoundedEditorInitialWidth"], window.Width);
                    Assert.Equal((double)styles["BoundedEditorInitialHeight"], window.Height);
                    Assert.Equal((double)styles["BoundedEditorMinWidth"], window.MinWidth);
                    Assert.Equal((double)styles["BoundedEditorMinHeight"], window.MinHeight);

                    // Y ya NO heredan los minimos del editor rico, que es lo que las clampeaba.
                    Assert.NotEqual((double)styles["ShellMinWidth"], window.MinWidth);
                    Assert.NotEqual((double)styles["ShellMinHeight"], window.MinHeight);
                }
            });
        }

        [Fact]
        public void ElTamanoQueDeclaranEsElQueABREN()
        {
            StaTestRunner.Run(() =>
            {
                // La anomalia de I-39A en una sola frase: el ancho declarado perdia SIEMPRE contra el minimo
                // heredado. Ahora ningun minimo clampea el tamano inicial, asi que lo escrito es lo que se abre.
                foreach (var window in FourCantileverComponents())
                {
                    Assert.True(window.MinWidth <= window.Width);
                    Assert.True(window.MinHeight <= window.Height);
                    Assert.Equal(window.Width, Math.Max(window.Width, window.MinWidth));
                    Assert.Equal(window.Height, Math.Max(window.Height, window.MinHeight));
                }
            });
        }

        [Fact]
        public void NingunaVentanaDelArquetipoBAplicaElEstiloDelArquetipoRICO()
        {
            foreach (var file in Directory.GetFiles(ComponentDirectory(), "*.xaml"))
            {
                var xaml = File.ReadAllText(file);

                Assert.Contains("BoundedEditorWindowStyle", xaml, StringComparison.Ordinal);
                Assert.DoesNotContain("EditorShellWindowStyle", xaml, StringComparison.Ordinal);

                // Y sin literales propios: el tamano lo declara el arquetipo UNA vez, no cada ventana.
                Assert.DoesNotContain("Width=\"", xaml.Split('>')[0], StringComparison.Ordinal);
                Assert.DoesNotContain("Height=\"", xaml.Split('>')[0], StringComparison.Ordinal);
            }
        }

        [Fact]
        public void ElInspectorLeeLosMismosTokensEnVezDeRepetirlos()
        {
            StaTestRunner.Run(() =>
            {
                var styles = AppStyles();
                var window = new StructuralSectionInspectorWindow(Catalog());

                Assert.Equal((double)styles["BoundedEditorInitialWidth"], window.Width);
                Assert.Equal((double)styles["BoundedEditorInitialHeight"], window.Height);
                Assert.Equal((double)styles["BoundedEditorMinWidth"], window.MinWidth);
                Assert.Equal((double)styles["BoundedEditorMinHeight"], window.MinHeight);
            });

            var source = File.ReadAllText(Path.Combine(
                RepoRoot().FullName, "src", "RackCad.UI", "StructuralSections", "StructuralSectionInspectorWindow.cs"));

            Assert.Contains("ShellResources.Get(\"BoundedEditorInitialWidth\"", source, StringComparison.Ordinal);
        }

        // ---- 3. EditorAction tiene por fin un consumidor productivo (D11) ----

        [Fact]
        public void ElPilotoConstruyeSusDosAccionesConLaFabricaComun()
        {
            StaTestRunner.Run(() =>
            {
                // I-39A dejo escrito el motivo por el que no pudo adoptarla: la fabrica no sabia fijar IsDefault ni
                // IsCancel, asi que sustituir los botones habria roto Enter y Escape. Resuelto eso, el piloto la
                // consume y el contrato de teclado que su caracterizacion fija sigue intacto — esa suite no se toca.
                var window = new StructuralSectionInspectorWindow(Catalog());

                var insert = EditorWindowTestSupport.Find<Button>(window, b => (b.Content as string) == "Insertar");
                var close = EditorWindowTestSupport.Find<Button>(window, b => (b.Content as string) == "Cerrar");

                Assert.True(insert.IsDefault);
                Assert.True(close.IsCancel);

                // La prueba de que salen de la fabrica y no de chrome propio: llevan los estilos compartidos, cada uno
                // el suyo, y la ayuda visible-aun-deshabilitada que la fabrica instala siempre.
                Assert.NotNull(insert.Style);
                Assert.NotNull(close.Style);
                Assert.NotSame(insert.Style, close.Style);
                Assert.True(ToolTipService.GetShowOnDisabled(insert));
                Assert.True(ToolTipService.GetShowOnDisabled(close));
            });
        }

        // ---- 3b. ninguna accion queda habilitada sin efecto, y la bloqueada dice por que (D6) ----

        [Fact]
        public void InsertarSeApagaConMotivoCuandoLaPiezaNoPuedeMaterializARSE()
        {
            StaTestRunner.Run(() =>
            {
                // D6: «una accion importante deshabilitada sin motivo es una violacion del contrato; una accion
                // habilitada sin efecto es una violacion MAYOR». Las cuatro estaban en el segundo caso: Insertar
                // quedaba encendido y pulsarlo solo escribia una linea en el diagnostico.
                var catalogue = Catalog();

                var sinLinea = new (Window Window, string Reason)[]
                {
                    (new CantileverSeparatorWindow(new CantileverBracingDesign(), catalogue),
                        "Resuelve primero la línea: el corte del separador sale de los agujeros de sus dos placas."),
                    (new CantileverBraceWindow(new CantileverBracingDesign(), catalogue),
                        "Resuelve primero la línea: el corte del tensor sale de los agujeros de sus separadores.")
                };

                foreach (var (window, reason) in sinLinea)
                {
                    var insert = (Button)window.FindName("InsertButton");

                    Assert.False(insert.IsEnabled);
                    Assert.Equal(reason, insert.ToolTip as string);
                    Assert.True(ToolTipService.GetShowOnDisabled(insert));
                }
            });
        }

        [Fact]
        public void InsertarDistingueElMotivoDelLlamadorDelMotivoDeLaPieza()
        {
            StaTestRunner.Run(() =>
            {
                // Dos motivos distintos, dos mensajes distintos: no basta con apagar el boton, hay que decir CUAL de
                // los dos falta. Aceptar sigue encendido en ambos: una pieza bloqueada es una intencion que el
                // usuario puede conservar y seguir editando.
                var catalogue = Catalog();

                var fueraDeAutoCad = new CantileverColumnBaseWindow(ColumnBaseTemplate(), catalogue, canInsertInAutoCad: false);
                var insertOutside = (Button)fueraDeAutoCad.FindName("InsertButton");
                Assert.False(insertOutside.IsEnabled);
                Assert.Equal("Disponible solo cuando la ventana se abre desde AutoCAD.", insertOutside.ToolTip as string);

                var desdeAutoCad = new CantileverColumnBaseWindow(ColumnBaseTemplate(), catalogue, canInsertInAutoCad: true);
                var insertInside = (Button)desdeAutoCad.FindName("InsertButton");
                Assert.True(insertInside.IsEnabled);
                Assert.StartsWith("Dibuja la columna y base sola", insertInside.ToolTip as string, StringComparison.Ordinal);

                Assert.True(((Button)fueraDeAutoCad.FindName("AcceptButton")).IsEnabled);
                Assert.True(((Button)desdeAutoCad.FindName("AcceptButton")).IsEnabled);
            });
        }

        [Fact]
        public void ElInspectorApagaInsertarSinSeleccionYLoDICE()
        {
            StaTestRunner.Run(() =>
            {
                // La deuda que I-39A midio y dejo escrita en su propia caracterizacion: «Insertar nunca se
                // deshabilita y sin seleccion es un no-op SILENCIOSO».
                var window = new StructuralSectionInspectorWindow(Catalog());
                window.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent, window));

                var insert = EditorWindowTestSupport.Find<Button>(window, b => (b.Content as string) == "Insertar");
                Assert.True(insert.IsEnabled);   // con seleccion, que es el estado de apertura

                EditorWindowTestSupport.FindAll<TextBox>(window)[0].Text = "ZZZZ-NO-EXISTE";

                Assert.False(insert.IsEnabled);
                Assert.Equal("Elige una sección de la lista para poder insertarla.", insert.ToolTip as string);
                Assert.True(ToolTipService.GetShowOnDisabled(insert));
            });
        }

        [Fact]
        public void UnaLongitudInvalidaNoBloqueaPorqueElValorAPLICADOSigueSiendoValido()
        {
            StaTestRunner.Run(() =>
            {
                // La otra mitad de la medicion de I-39A, revisada y NO cambiada: D5 exige que una entrada invalida no
                // sobrescriba en silencio un valor aplicado valido, y eso es exactamente lo que la ventana hace. Lo
                // que se inserta es el ultimo valor aplicado, no basura, asi que bloquear seria un error.
                var window = new StructuralSectionInspectorWindow(Catalog());
                window.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent, window));

                var length = EditorWindowTestSupport.FindAll<TextBox>(window)[1];
                var applied = window.State.Length;
                length.Text = "no es un numero";

                Assert.Equal(applied, window.State.Length);
                Assert.True(EditorWindowTestSupport.Find<Button>(window, b => (b.Content as string) == "Insertar").IsEnabled);
            });
        }

        // ---- 3c. foco inicial declarado y determinista (D9) ----

        [Fact]
        public void LasSeisDeclaranSuFocoInicialYNingunoEsUnaAccion()
        {
            // Sin declararlo, el foco caia donde el arbol VISUAL del shell lo pusiera, y la plantilla acopla la barra
            // de acciones arriba en el DockPanel, antes de la zona de parametros: el primer elemento enfocable podia
            // ser un boton, y «Restaurar» descarta lo editado. D9 exige que el foco inicial sea determinista y que no
            // recaiga en una accion destructiva ni bloqueada, asi que las seis lo declaran, cada una en su primer
            // control de captura.
            var declared = new (string File, string Element)[]
            {
                ("Systems/Cantilever/Components/CantileverColumnBaseWindow.xaml", "ColumnPlateThicknessBox"),
                ("Systems/Cantilever/Components/CantileverArmWindow.xaml", "ArrangementBox"),
                ("Systems/Cantilever/Components/CantileverBraceWindow.xaml", "KindBox"),
                ("Systems/Cantilever/Components/CantileverSeparatorWindow.xaml", "SectionPicker"),
                ("Systems/Larguero/RackLargueroWindow.xaml", "NameBox")
            };

            foreach (var (file, element) in declared)
            {
                var source = File.ReadAllText(Path.Combine(
                    new[] { RepoRoot().FullName, "src", "RackCad.UI" }.Concat(file.Split('/')).ToArray()));

                Assert.Contains(
                    "FocusManager.FocusedElement=\"{Binding ElementName=" + element + "}\"",
                    source,
                    StringComparison.Ordinal);
            }

            // El inspector se construye en codigo, asi que lo declara en codigo y se puede comprobar sobre el objeto.
            StaTestRunner.Run(() =>
            {
                var window = new StructuralSectionInspectorWindow(Catalog());
                var focused = System.Windows.Input.FocusManager.GetFocusedElement(window);

                Assert.NotNull(focused);
                var box = Assert.IsType<TextBox>(focused);
                Assert.Same(EditorWindowTestSupport.FindAll<TextBox>(window)[0], box);
            });
        }

        // ---- 3d. preview: autoridad y frescura declaradas (D4) ----

        [Fact]
        public void ElPreviewDelArquetipoSeDerivaSIEMPREDeLaCapturaActual()
        {
            StaTestRunner.Run(() =>
            {
                // D4 pide declarar autoridad y frescura, y dice expresamente que «una ventana no esta obligada a
                // implementar estados que hoy no exhibe». La medicion de las seis: su preview es SIEMPRE derivado del
                // borrador capturado y SIEMPRE actual, porque cada una lo rehace en el mismo paso en que recalcula.
                // Ninguna conserva un ultimo-valido obsoleto, asi que no se inventa un modelo de frescura que el
                // producto no tiene; lo que se fija es que no aparezca uno por accidente.
                var catalogue = Catalog();

                // Separador y tensor SIN linea resuelta: no hay plan, y el preview no muestra un residuo, muestra nada.
                var separator = new CantileverSeparatorWindow(new CantileverBracingDesign(), catalogue);
                var brace = new CantileverBraceWindow(new CantileverBracingDesign(), catalogue);

                Assert.Null(separator.CurrentPreviewPlan);
                Assert.Null(brace.CurrentPreviewPlan);

                foreach (var window in new Window[] { separator, brace })
                {
                    var canvas = (Canvas)window.FindName("PreviewCanvas");

                    // Ni una sola figura: sin plan no se dibuja geometria, y menos aun la de una captura anterior.
                    Assert.Empty(canvas.Children.OfType<System.Windows.Shapes.Shape>());

                    // Y lo que hay es un MENSAJE, no un residuo: el preview dice que no hay nada que enseñar en vez
                    // de quedarse mudo, que es lo que D4 pide de un estado «no disponible».
                    Assert.NotEmpty(canvas.Children.OfType<TextBlock>());
                }

                // Columna-base CON contexto: hay plan, y es el mismo que se insertaria (lo fija la suite funcional).
                var columnBase = new CantileverColumnBaseWindow(ColumnBaseTemplate(), catalogue, canInsertInAutoCad: true);
                Assert.NotNull(columnBase.CurrentPreviewPlan);
            });
        }

        // ---- 3e. cierre y dirty: «no aplicable» es un valor legitimo (D7 y D8) ----

        [Fact]
        public void NingunaDelArquetipoDeclaraAmbitoPendienteYPorEsoNoInterceptaElCierre()
        {
            StaTestRunner.Run(() =>
            {
                // Medido, no supuesto. D8 admite «no aplicable», y aqui lo es por una razon del PRODUCTO: las cuatro
                // Cantilever editan una COPIA que solo se devuelve al aceptar, y lo dicen con su propia accion
                // «Restaurar», que vuelve a los valores con que se abrio la ventana. El Larguero no acumula nada: lo
                // que persiste lo persiste su boton de guardar. Y el inspector no edita, inspecciona.
                //
                // Inventarles un dirty global seria justo lo que I-39B se nego a hacer en los editores ricos. La
                // guarda esta en que ese «no aplicable» siga siendo cierto: si alguna empezara a acumular trabajo
                // perdible, tendria que declarar el ambito Y consultar el cierre, y esta prueba lo obligaria.
                foreach (var window in FourCantileverComponents())
                {
                    Assert.NotNull(EditorWindowTestSupport.Find<Button>(window, b => (b.Content as string) == "Restaurar"));
                }

                var declared = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public
                    | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.DeclaredOnly;

                foreach (var type in new[]
                {
                    typeof(CantileverColumnBaseWindow), typeof(CantileverArmWindow),
                    typeof(CantileverSeparatorWindow), typeof(CantileverBraceWindow),
                    typeof(RackLargueroWindow), typeof(StructuralSectionInspectorWindow)
                })
                {
                    var interceptsClose = type.GetMethod("OnClosing", declared) != null;
                    var declaresScope = type.GetMethods(declared).Select(m => m.Name)
                        .Concat(type.GetProperties(declared).Select(p => p.Name))
                        .Any(name => name.Contains("Pending", StringComparison.Ordinal)
                            || name.Contains("Unsaved", StringComparison.Ordinal));

                    Assert.Equal(declaresScope, interceptsClose);
                }
            });
        }

        // ---- 4. el Larguero, ultima ventana del arquetipo sin shell ----

        [Fact]
        public void ElLargueroComponeSobreElShellYAplicaElContratoDelArquetipo()
        {
            StaTestRunner.Run(() =>
            {
                var styles = AppStyles();
                var window = new RackLargueroWindow();

                Assert.IsType<RackCad.UI.Shell.RackBoundedEditorShell>(window.Content);
                Assert.NotNull(window.Style);
                Assert.Equal((double)styles["BoundedEditorInitialWidth"], window.Width);
                Assert.Equal((double)styles["BoundedEditorInitialHeight"], window.Height);
                Assert.Equal((double)styles["BoundedEditorMinWidth"], window.MinWidth);
                Assert.Equal((double)styles["BoundedEditorMinHeight"], window.MinHeight);
            });
        }

        [Fact]
        public void ElLargueroConservaSusCincoCamposSuLienzoYSuEstado()
        {
            StaTestRunner.Run(() =>
            {
                // La migracion muda las MISMAS instancias: si un x:Name se perdiera, el propio code-behind dejaria de
                // encontrarlo (DrawPreview usa PreviewCanvas, SetStatus usa StatusText) y el catalogo no se cargaria.
                var window = new RackLargueroWindow();

                foreach (var name in new[] { "NameBox", "ProfileBox", "PeralteBox", "LengthBox", "MensulaBox", "PreviewCanvas", "StatusText" })
                {
                    Assert.True(window.FindName(name) != null, $"se perdio {name} al migrar");
                }

                // El catalogo sigue alimentando los combos y el perfil sigue derivando sus peraltes.
                Assert.NotEmpty(((ComboBox)window.FindName("ProfileBox")).Items);
                Assert.NotEmpty(((ComboBox)window.FindName("MensulaBox")).Items);
                Assert.Equal("96", ((TextBox)window.FindName("LengthBox")).Text);
            });
        }

        [Fact]
        public void ElChromeDelLargueroSaleYaDeLosTokensCompartidos()
        {
            var source = File.ReadAllText(Path.Combine(
                RepoRoot().FullName, "src", "RackCad.UI", "Systems", "Larguero", "RackLargueroWindow.xaml"));

            // Los cuatro literales que el censo de I-39A registro como el tercer chrome del proyecto.
            foreach (var literal in new[] { "#1F2933", "#9AA7B4", "#617080", "#D8DEE6" })
            {
                Assert.DoesNotContain(literal, source, StringComparison.Ordinal);
            }

            // La superficie del preview se queda CLARA a proposito: sus rotulos son grises y sobre el fondo oscuro
            // del editor rico serian ilegibles. El token elegido vale exactamente lo que la ventana ya pintaba.
            Assert.Contains("ShellSurfaceBrush", source, StringComparison.Ordinal);
            Assert.DoesNotContain("ShellPreviewBackgroundBrush", source, StringComparison.Ordinal);
            Assert.Equal(
                ((SolidColorBrush)AppStyles()["ShellSurfaceBrush"]).Color,
                Color.FromRgb(0xFF, 0xFF, 0xFF));
        }

        [Fact]
        public void ElLargueroSigueSinDeclararSelectorNiRecetaEnLinea()
        {
            StaTestRunner.Run(() =>
            {
                // Dos ranuras vacias A PROPOSITO, como el piloto: el larguero no elige del catalogo estructural
                // —sus combos vienen del catalogo de producto— y su lista de materiales se consulta en una ventana
                // modal. Rellenarlas seria inventar producto donde el shell solo pide composicion.
                var shell = (RackCad.UI.Shell.RackBoundedEditorShell)new RackLargueroWindow().Content;

                Assert.Null(shell.SectionPicker);
                Assert.Null(shell.BomSummary);
                Assert.NotNull(shell.Header);
                Assert.NotNull(shell.Parameters);
                Assert.NotNull(shell.Preview);
                Assert.NotNull(shell.Diagnostics);
                Assert.NotNull(shell.Actions);
            });
        }

        [Fact]
        public void AlMinimoDelArquetipoLasAccionesYElDiagnosticoSiguenCOMPLETOS()
        {
            StaTestRunner.Run(() =>
            {
                // El mismo criterio con que I-30 fijo el minimo del arquetipo A: una ventana MOSTRADA pierde el marco
                // no-cliente, asi que se simula el cliente mas apretado (el minimo menos una holgura fija y generosa,
                // para no depender de la maquina ni del DPI) y se comprueba que la barra de acciones y el diagnostico
                // caben ENTEROS. Si el minimo del arquetipo B se bajara, esto se rompe.
                var styles = AppStyles();
                var minW = (double)styles["BoundedEditorMinWidth"];
                var minH = (double)styles["BoundedEditorMinHeight"];
                const double frameAllowance = 46.0;
                var clientH = minH - frameAllowance;

                foreach (var window in FourCantileverComponents())
                {
                    var root = (FrameworkElement)window.Content;
                    root.Measure(new Size(minW, clientH));
                    root.Arrange(new Rect(0, 0, minW, clientH));
                    root.UpdateLayout();

                    foreach (var name in new[] { "DiagnosticsText", "BomText" })
                    {
                        var block = (TextBlock)window.FindName(name);
                        var bottom = block.TransformToAncestor(root).Transform(new Point(0, block.ActualHeight)).Y;
                        Assert.True(bottom <= clientH + 0.5,
                            $"{window.GetType().Name}: {name} se sale del cliente minimo ({bottom:0} > {clientH:0})");
                    }

                    foreach (var label in new[] { "Restaurar", "Aceptar", "Cancelar" })
                    {
                        var button = EditorWindowTestSupport.Find<Button>(root, b => (b.Content as string) == label);
                        Assert.True(button.ActualWidth > 0.0 && button.ActualHeight > 0.0,
                            $"{window.GetType().Name}: «{label}» no tiene tamano al minimo del arquetipo");

                        var corner = button.TransformToAncestor(root).Transform(
                            new Point(button.ActualWidth, button.ActualHeight));
                        Assert.True(corner.X <= minW + 0.5 && corner.Y <= clientH + 0.5,
                            $"{window.GetType().Name}: «{label}» se recorta al minimo del arquetipo");
                    }
                }
            });
        }
    }
}
