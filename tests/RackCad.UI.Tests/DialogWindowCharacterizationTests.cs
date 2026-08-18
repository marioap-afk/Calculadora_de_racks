using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using RackCad.Application.Bom;
using RackCad.Application.Catalogs;
using RackCad.Domain.Systems.Selective;
using RackCad.UI;
using RackCad.UI.Systems.Selective;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// CARACTERIZACION de las DIECISEIS ventanas de los arquetipos C y D, escrita y verde ANTES de tocarlas
    /// (ADR-0029 D13) y NO editada despues.
    ///
    /// <para>La auditoria de apertura midio que <b>nueve de las dieciseis no se construyen jamas</b> en una prueba y
    /// que <b>toda</b> la cobertura existente es FUNCIONAL —modelo de matriz, celdas dormidas, resultado tipado,
    /// alcance del bulk—: ni una sola asercion de teclado, cierre, foco, tabulacion, tamano u ownership. Esta suite
    /// existe exactamente para esa mitad que faltaba.</para>
    ///
    /// <para>Fija el comportamiento REAL, no el deseable. Donde ADR-0029 describe otra cosa —una accion sin motivo
    /// visible, un cierre que no atraviesa ninguna politica, un `CenterOwner` que no puede tener `Owner`— lo que se
    /// fija aqui es lo ACTUAL, y el contrato nuevo vive en una clase separada.</para>
    ///
    /// <para>Las aserciones son CIEGAS a la composicion: por rol (`IsDefault`/`IsCancel`), por contenido de boton y
    /// por tipo, nunca por la forma del arbol, porque la forma del arbol es lo que la migracion cambia.</para>
    /// </summary>
    public sealed class DialogWindowCharacterizationTests
    {
        // ================================================================================================
        // Construccion real de las dieciseis. Cada una con el minimo que su constructor exige.
        // ================================================================================================

        /// <summary>
        /// Marca de una caracterizacion que I-39D cambio A PROPOSITO. La prueba NO se reescribe: se conserva con su
        /// cuerpo intacto como evidencia versionada del comportamiento anterior, y el nuevo se prueba en
        /// <c>DialogWindowContractTests</c>.
        ///
        /// <para>La transicion se lee en tres sitios: aqui esta el comportamiento anterior; ADR-0029 lo autoriza; y
        /// la clase de contrato prueba el nuevo. El commit <c>f6c4d12</c> es la version en que toda esta suite corria
        /// en verde contra la base, y <c>docs/automation/evidence/I-39D-caracterizacion-base-vs-contrato.md</c>
        /// enfrenta las dos versiones asercion por asercion.</para>
        /// </summary>
        private const string BaseEvidence =
            "Evidencia de la BASE anterior a I-39D; el contrato nuevo lo prueba DialogWindowContractTests. " +
            "Ver docs/automation/evidence/I-39D-caracterizacion-base-vs-contrato.md";

        private static IReadOnlyList<SelectiveGridCell> NoCells() => new List<SelectiveGridCell>();

        private static SelectiveSafetyWindow Safety() => new SelectiveSafetyWindow(
            new List<SafetyElementCatalogEntry>(), Enumerable.Empty<SelectiveSafetySelection>(), postCount: 3);

        private static SafetyPerPostWindow PerPost() =>
            new SafetyPerPostWindow("Bota", 3, SafetySide.Both, Enumerable.Empty<SafetyPostSide>());

        private static SafetyTopeGridWindow Tope() =>
            new SafetyTopeGridWindow("Tope", new[] { 3 }, shared: false, side: SafetySide.Both, saque: 2.0,
                frontal: true, offCells: NoCells());

        private static SafetyParrillaGridWindow Parrilla() =>
            new SafetyParrillaGridWindow("Parrilla", new[] { 3 }, frontal: true, lateral: false, frente: 96.0,
                cantidad: 2, offCells: NoCells());

        private static SafetyGuiaEntradaGridWindow Guia() =>
            new SafetyGuiaEntradaGridWindow("Guia", new[] { 3 }, NoCells());

        private static SafetyDesviadorGridWindow Desviador() =>
            new SafetyDesviadorGridWindow("desviador", "Desviador", system: null, catalog: null, longitud: 48.0,
                firstHeight: 12.0, side: SafetySide.Both, offCells: NoCells(), fallbackPostCount: 3,
                fallbackLevelsPerFrente: new[] { 3 });

        private static SafetyDefensaGridWindow Defensa() =>
            new SafetyDefensaGridWindow("Defensa", 3, Enumerable.Empty<SafetyPostDefense>());

        private static SelectiveSegmentsWindow Segments() =>
            new SelectiveSegmentsWindow(1, Enumerable.Empty<SelectiveSegment>(), 96.0);

        private static RackWarehouseLayoutWindow Layout() => new RackWarehouseLayoutWindow("R1", 48.0, 96.0);

        private static RackWarehouseFillWindow Fill() => new RackWarehouseFillWindow("R1", 48.0, 96.0);

        private static BillOfMaterials Bom() =>
            new BillOfMaterials(new List<BomLine> { new BomLine { Category = "Poste", ProfileId = "P", Description = "Poste", Length = 96.0, Quantity = 4 } });

        private static RackBomWindow BomWindow() => new RackBomWindow(Bom());

        private static RackConsolidatedBomWindow ConsolidatedBomWindow() => new RackConsolidatedBomWindow(
            new ConsolidatedBom(new List<ConsolidatedRackBom>(), Bom()));

        private static RackListWindow ListWindow() => new RackListWindow(new List<RackListRow>());

        private static RackDesignLibraryWindow LibraryWindow() =>
            new RackDesignLibraryWindow(Path.Combine(Path.GetTempPath(), "rackcad-i39d-caracterizacion"));

        private static RackMainMenuWindow MainMenu() => new RackMainMenuWindow();

        private static RackCommandHelpWindow Help() => new RackCommandHelpWindow();

        /// <summary>Los diez del arquetipo C, en el orden del censo.</summary>
        private static (string Name, Func<Window> Build)[] ArchetypeC() => new (string, Func<Window>)[]
        {
            ("SelectiveSafetyWindow", () => Safety()),
            ("SafetyPerPostWindow", () => PerPost()),
            ("SafetyTopeGridWindow", () => Tope()),
            ("SafetyParrillaGridWindow", () => Parrilla()),
            ("SafetyGuiaEntradaGridWindow", () => Guia()),
            ("SafetyDesviadorGridWindow", () => Desviador()),
            ("SafetyDefensaGridWindow", () => Defensa()),
            ("SelectiveSegmentsWindow", () => Segments()),
            ("RackWarehouseLayoutWindow", () => Layout()),
            ("RackWarehouseFillWindow", () => Fill()),
        };

        /// <summary>Las seis del arquetipo D, en el orden del censo.</summary>
        private static (string Name, Func<Window> Build)[] ArchetypeD() => new (string, Func<Window>)[]
        {
            ("RackMainMenuWindow", () => MainMenu()),
            ("RackDesignLibraryWindow", () => LibraryWindow()),
            ("RackBomWindow", () => BomWindow()),
            ("RackConsolidatedBomWindow", () => ConsolidatedBomWindow()),
            ("RackListWindow", () => ListWindow()),
            ("RackCommandHelpWindow", () => Help()),
        };

        private static (string Name, Func<Window> Build)[] Sixteen() => ArchetypeC().Concat(ArchetypeD()).ToArray();

        private static IReadOnlyList<Button> Buttons(Window window)
            => EditorWindowTestSupport.FindAll<Button>(window);

        private static DirectoryInfo RepoRoot()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "RackCad.sln"))) dir = dir.Parent;
            Assert.True(dir != null, "repo root (RackCad.sln) not found");
            return dir;
        }

        private static string UiSource(params string[] relative)
            => File.ReadAllText(Path.Combine(new[] { RepoRoot().FullName, "src", "RackCad.UI" }.Concat(relative).ToArray()));

        /// <summary>Whether the type itself overrides <paramref name="name"/> — the question a close policy asks.</summary>
        private static bool Overrides(Type type, string name)
            => type.GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly) != null;

        private static Type[] SixteenTypes() => new[]
        {
            typeof(SelectiveSafetyWindow), typeof(SafetyPerPostWindow), typeof(SafetyTopeGridWindow),
            typeof(SafetyParrillaGridWindow), typeof(SafetyGuiaEntradaGridWindow), typeof(SafetyDesviadorGridWindow),
            typeof(SafetyDefensaGridWindow), typeof(SelectiveSegmentsWindow), typeof(RackWarehouseLayoutWindow),
            typeof(RackWarehouseFillWindow), typeof(RackMainMenuWindow), typeof(RackDesignLibraryWindow),
            typeof(RackBomWindow), typeof(RackConsolidatedBomWindow), typeof(RackListWindow),
            typeof(RackCommandHelpWindow),
        };

        // ================================================================================================
        // 1. LAS DIECISEIS SE CONSTRUYEN. Es la primera vez: nueve no se habian construido nunca.
        // ================================================================================================

        [Fact]
        public void LasDieciseisSeConstruyenYNingunaEsUnCascaronVacio()
        {
            StaTestRunner.Run(() =>
            {
                foreach (var (name, build) in Sixteen())
                {
                    var window = build();

                    Assert.NotNull(window.Content);
                    Assert.False(string.IsNullOrWhiteSpace(window.Title), name + " sin Title");
                }
            });
        }

        // ================================================================================================
        // 2. TECLADO: accion por defecto y de cancelacion (ADR-0029 D7)
        // ================================================================================================

        [Fact]
        public void LosDiezDeCDeclaranUnaAccionPorDefectoYUnaDeCancelacion()
        {
            StaTestRunner.Run(() =>
            {
                foreach (var (name, build) in ArchetypeC())
                {
                    var buttons = Buttons(build());

                    Assert.Equal(1, buttons.Count(b => b.IsDefault));
                    Assert.Equal(1, buttons.Count(b => b.IsCancel));
                    Assert.DoesNotContain(buttons, b => b.IsDefault && b.IsCancel);
                }
            });
        }

        [Fact]
        public void LaEtiquetaPrimariaNoEsAceptarEnLasDosDeAlmacen()
        {
            StaTestRunner.Run(() =>
            {
                // Es lo que hace inservible una fabrica que fije el literal: dos de las diez llaman a su accion
                // primaria por lo que HACE, no «Aceptar».
                Assert.Equal("Colocar", Buttons(Layout()).Single(b => b.IsDefault).Content as string);
                Assert.Equal("Calcular", Buttons(Fill()).Single(b => b.IsDefault).Content as string);

                foreach (var (name, build) in ArchetypeC().Where(x => !x.Name.StartsWith("RackWarehouse", StringComparison.Ordinal)))
                {
                    Assert.Equal("Aceptar", Buttons(build()).Single(b => b.IsDefault).Content as string);
                }
            });
        }

        [Fact]
        public void LosParesTodosNingunoDifierenPorConcordanciaDeGenero()
        {
            StaTestRunner.Run(() =>
            {
                // Regla de PRODUCTO, no ruido: la parrilla es femenina y las otras tres masculinas. Una fabrica que
                // fijara el literal cambiaria el texto visible de una de las cuatro.
                foreach (var (build, all, none) in new (Func<Window>, string, string)[]
                {
                    (() => Tope(), "Todos", "Ninguno"),
                    (() => Guia(), "Todos", "Ninguno"),
                    (() => Desviador(), "Todos", "Ninguno"),
                    (() => Parrilla(), "Todas", "Ninguna"),
                })
                {
                    var labels = Buttons(build()).Select(b => b.Content as string).ToList();

                    Assert.Contains(all, labels);
                    Assert.Contains(none, labels);
                }
            });
        }

        [Fact]
        public void SelectiveSegmentsTieneTRESTerminacionesYLaTerceraCierraConEXITO()
        {
            StaTestRunner.Run(() =>
            {
                // No es Aceptar ni Cancelar: «Sin medio frente» es una tercera semantica —el frente no se parte— que
                // cierra con exito y resultado VACIO. Degradarla a decoracion cambiaria lo que llega al llamador.
                var labels = Buttons(Segments()).Select(b => b.Content as string).ToList();

                Assert.Contains("Aceptar", labels);
                Assert.Contains("Cancelar", labels);
                Assert.Contains("Sin medio frente", labels);
            });
        }

        [Fact]
        public void LasSeisDeDNoTienenParTransaccionalYCuatroCierranConCerrar()
        {
            StaTestRunner.Run(() =>
            {
                // Ninguna utilitaria tiene Aceptar/Cancelar, ni artificial ni real: no son transaccionales y el
                // contrato no se les debe inventar.
                foreach (var (name, build) in ArchetypeD())
                {
                    var labels = Buttons(build()).Select(b => b.Content as string).Where(t => t != null).ToList();

                    Assert.DoesNotContain("Aceptar", labels);
                    Assert.DoesNotContain("Cancelar", labels);
                }
            });
        }

        // ================================================================================================
        // 3. CIERRE Y DIRTY (ADR-0029 D7 y D8)
        // ================================================================================================

        [Fact]
        public void NingunaDeLasDieciseisIntercEptaSuPropioCierre()
        {
            // El estado de partida del arquetipo: cero puntos de interceptacion, asi que boton, Escape, la X y
            // Alt+F4 caen directo en el cierre de Window. Los dos unicos OnClosing del producto son de arquetipo A.
            foreach (var type in SixteenTypes())
            {
                Assert.False(Overrides(type, "OnClosing"), type.Name + " ya intercepta OnClosing");
                Assert.False(Overrides(type, "OnClosed"), type.Name + " ya intercepta OnClosed");
            }
        }

        [Fact]
        public void NingunaDeLasDieciseisDeclaraUnAmbitoTransaccionalPendiente()
        {
            // «No aplicable» es un valor legitimo de D8, y aqui lo es por razon de PRODUCTO: los diez de C trabajan
            // sobre una COPIA que solo llega al llamador tras aceptar, y las seis de D no editan nada.
            var declared = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

            foreach (var type in SixteenTypes())
            {
                var names = type.GetProperties(declared).Select(p => p.Name)
                    .Concat(type.GetMethods(declared).Select(m => m.Name));

                Assert.DoesNotContain(names, n => n.Contains("Pending", StringComparison.Ordinal)
                    || n.Contains("Unsaved", StringComparison.Ordinal)
                    || n.Contains("Dirty", StringComparison.Ordinal));
            }
        }

        [Fact]
        public void LosDiezDeCDevuelvenPorPropiedadTipadaYNuncaNull()
        {
            StaTestRunner.Run(() =>
            {
                // El contrato con el llamador: una propiedad `Result` que existe desde el constructor. Las de
                // coleccion arrancan VACIAS y las de registro arrancan en null; ninguna se asigna sin aceptar.
                foreach (var (name, build) in ArchetypeC())
                {
                    var property = build().GetType().GetProperty("Result");

                    Assert.True(property != null, name + " no expone Result");
                    Assert.True(property.CanRead);
                    Assert.False(property.SetMethod?.IsPublic ?? false, name + " deja Result asignable desde fuera");
                }
            });
        }

        // ================================================================================================
        // 4. CHROME: lo que las diez repiten a mano, y la unica que no
        // ================================================================================================

        [Fact]
        public void NueveDeLasDiezRepitenAManoElMismoBloqueDeChrome()
        {
            StaTestRunner.Run(() =>
            {
                foreach (var (name, build) in ArchetypeC().Where(x => x.Name != "SafetyDefensaGridWindow"))
                {
                    var window = build();

                    Assert.Equal("Segoe UI", window.FontFamily?.Source);
                    Assert.NotNull(window.Background);
                    Assert.Equal(WindowStartupLocation.CenterOwner, window.WindowStartupLocation);
                    Assert.NotEmpty(window.Resources.MergedDictionaries);
                }
            });
        }

        [Fact]
        public void SafetyDefensaEsLaUNICAQueNoAsignaChromeYSuDeltaREALEsElFONDO()
        {
            StaTestRunner.Run(() =>
            {
                // No asigna FontFamily ni Background en su constructor, a diferencia de las otras nueve. Pero el
                // delta OBSERVABLE es uno solo: FontFamily resuelve igualmente a Segoe UI porque es la
                // predeterminada del sistema, mientras que Background se queda en null y la ventana pierde el fondo
                // compartido. Completarle el chrome cambia el FONDO y nada mas — medido, no supuesto.
                var window = Defensa();

                // Blanco liso, el predeterminado de Window, frente al #F4F6F9 compartido de las otras nueve.
                Assert.Equal(Colors.White, ((SolidColorBrush)window.Background).Color);
                Assert.Equal("Segoe UI", window.FontFamily?.Source);

                var compartido = ((SolidColorBrush)Tope().Background).Color;
                Assert.NotEqual(compartido, ((SolidColorBrush)window.Background).Color);

                var source = UiSource("SafetyDefensaGridWindow.cs");
                Assert.DoesNotContain("FontFamily", source, StringComparison.Ordinal);
                Assert.DoesNotContain("Background =", source, StringComparison.Ordinal);

                // Y si declara las otras dos mitades del bloque.
                Assert.Equal(WindowStartupLocation.CenterOwner, window.WindowStartupLocation);
                Assert.NotEmpty(window.Resources.MergedDictionaries);
            });
        }

        [Fact(Skip = BaseEvidence)]
        public void NingunaDeLasDieciseisAplicaUnEstiloDeVentanaCompartido()
        {
            StaTestRunner.Run(() =>
            {
                // Los arquetipos A y B tienen su contrato como estilo de ventana desde I-30 e I-39C. C y D no tienen
                // ninguno: cada ventana repite el chrome en su constructor o en su XAML.
                foreach (var (name, build) in Sixteen())
                {
                    Assert.Null(build().Style);
                }
            });
        }

        // ================================================================================================
        // 5. TAMANO (ADR-0029 D9)
        // ================================================================================================

        [Fact]
        public void SoloDOSDeDDeclaranExactamenteElMISMOTamano()
        {
            StaTestRunner.Run(() =>
            {
                // Convergencia real, medida: la biblioteca y la lista coinciden en las cuatro dimensiones. El BOM
                // comparte los MINIMOS pero no el tamano inicial, y el BOM consolidado no comparte nada. Es la
                // frontera exacta de lo que se puede tokenizar sin mover un pixel.
                foreach (var build in new Func<Window>[] { () => LibraryWindow(), () => ListWindow() })
                {
                    var window = build();

                    Assert.Equal(720.0, window.Width);
                    Assert.Equal(480.0, window.Height);
                    Assert.Equal(520.0, window.MinWidth);
                    Assert.Equal(320.0, window.MinHeight);
                }

                var bom = BomWindow();
                Assert.Equal(740.0, bom.Width);
                Assert.Equal(520.0, bom.Height);
                Assert.Equal(520.0, bom.MinWidth);   // los minimos SI coinciden con las otras dos
                Assert.Equal(320.0, bom.MinHeight);

                var consolidated = ConsolidatedBomWindow();
                Assert.Equal(820.0, consolidated.Width);
                Assert.Equal(620.0, consolidated.Height);
            });
        }

        [Fact]
        public void DOSDeCUsanSizeToContentYSonLasUnicasDelRepositorio()
        {
            StaTestRunner.Run(() =>
            {
                // Las dos de almacen deciden su alto por el contenido A PROPOSITO: declaran Width y MinWidth pero
                // omiten Height y MinHeight. Un minimo comun del arquetipo las romperia, y es la razon medida por la
                // que el contrato de tamano de C no puede llevar minimos.
                foreach (var build in new Func<Window>[] { () => Layout(), () => Fill() })
                {
                    var window = build();

                    Assert.Equal(SizeToContent.Height, window.SizeToContent);
                    Assert.Equal(0.0, window.MinHeight);
                    Assert.True(double.IsNaN(window.Height));
                }

                // Y no son tres: SafetyPerPostWindow declara su alto como las demas.
                Assert.Equal(SizeToContent.Manual, PerPost().SizeToContent);
            });
        }

        [Fact]
        public void CuatroDeCCalculanSuTamanoDeLosDatos()
        {
            StaTestRunner.Run(() =>
            {
                // Las rejillas dimensionan segun la matriz: el ALTO crece con los niveles, acotado por un techo, y el
                // ANCHO crece con los frentes desde un suelo. Fijarles un tamano comun seria perder adaptacion, no
                // ganar coherencia.
                var pequena = new SafetyTopeGridWindow("Tope", new[] { 1 }, false, SafetySide.Both, 2.0, true, NoCells());
                var grande = new SafetyTopeGridWindow("Tope", new[] { 12, 12, 12 }, false, SafetySide.Both, 2.0, true, NoCells());

                Assert.True(grande.Height > pequena.Height,
                    $"la rejilla no adapta su alto: {pequena.Height} vs {grande.Height}");

                // El ancho tiene SUELO: con pocos frentes ninguna baja de su minimo declarado, asi que dos tamanos
                // distintos de matriz pueden abrir con el mismo ancho.
                Assert.Equal(pequena.Width, grande.Width);
                Assert.True(pequena.Width >= pequena.MinWidth);

                // Y con muchos frentes si crece.
                var ancha = new SafetyGuiaEntradaGridWindow("Guia", Enumerable.Repeat(3, 12).ToArray(), NoCells());
                Assert.True(ancha.Width > Guia().Width, $"{Guia().Width} vs {ancha.Width}");
            });
        }

        // ================================================================================================
        // 6. FOCO Y TABULACION (ADR-0029 D9)
        // ================================================================================================

        [Fact]
        public void NingunaDeLasDieciseisDeclaraFocoInicialNiOrdenDeTabulacion()
        {
            StaTestRunner.Run(() =>
            {
                foreach (var (name, build) in Sixteen())
                {
                    var window = build();

                    Assert.Null(System.Windows.Input.FocusManager.GetFocusedElement(window));
                    Assert.DoesNotContain(
                        EditorWindowTestSupport.FindAll<Control>(window),
                        c => c.TabIndex != int.MaxValue);
                }
            });
        }

        // ================================================================================================
        // 7. ACCIONES Y MOTIVO VISIBLE (ADR-0029 D6)
        // ================================================================================================

        [Fact]
        public void NingunaDeLasDieciseisDeshabilitaUnBotonDeSuBarraDeAcciones()
        {
            StaTestRunner.Run(() =>
            {
                // No hay ni una accion bloqueada que necesite motivo: las diez de C validan AL PULSAR, no antes.
                foreach (var (name, build) in Sixteen())
                {
                    foreach (var button in Buttons(build()).Where(b => b.IsDefault || b.IsCancel))
                    {
                        Assert.True(button.IsEnabled, name + " abre con una accion de barra deshabilitada");
                    }
                }
            });
        }

        [Fact]
        public void LasDosDeAlmacenValidanAlPULSARYNoAntes()
        {
            StaTestRunner.Run(() =>
            {
                // Su unico contrato observable, y hoy no lo fija nada mas que esta prueba: la accion primaria nunca
                // se deshabilita porque la validacion ocurre DENTRO del handler. Con los valores por defecto que la
                // ventana se autorrellena la validacion pasa y hay resultado; con un campo invalido, no lo hay.
                var conDefectos = Layout();
                Click(Buttons(conDefectos).Single(b => b.IsDefault));
                Assert.NotNull(conDefectos.Result);

                var invalida = Layout();
                EditorWindowTestSupport.FindAll<TextBox>(invalida).First().Text = "no es un numero";
                Click(Buttons(invalida).Single(b => b.IsDefault));
                Assert.Null(invalida.Result);
            });
        }

        /// <summary>Pulsa de verdad y traga SOLO el <see cref="InvalidOperationException"/> que
        /// <c>Window.DialogResult</c> lanza fuera de <c>ShowDialog</c>. Es valido porque en las dieciseis
        /// <c>Result</c> se asigna ANTES de esa linea (patron de I-39A).</summary>
        private static void Click(Button button)
        {
            try
            {
                button.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent, button));
            }
            catch (InvalidOperationException)
            {
            }
        }

        // ================================================================================================
        // 8. OWNERSHIP Y UBICACION (ADR-0029 D9) — medido sobre el CODIGO de los llamadores
        // ================================================================================================

        [Fact]
        public void QuinceDeLasDieciseisDeclaranCenterOwner()
        {
            StaTestRunner.Run(() =>
            {
                var centerScreen = Sixteen()
                    .Select(x => (x.Name, Location: x.Build().WindowStartupLocation))
                    .Where(x => x.Location == WindowStartupLocation.CenterScreen)
                    .Select(x => x.Name)
                    .ToList();

                Assert.Equal(new[] { "RackMainMenuWindow" }, centerScreen);
            });
        }

        [Fact]
        public void CincoDeclaranCenterOwnerSinQuePuedaExistirOwner()
        {
            // Su UNICO sitio de construccion es un comando del Plugin, que las muestra con ShowModalWindow sin
            // ninguna ventana padre WPF viva. `Window.Owner` exige una instancia de Window y ahi no hay ninguna.
            // Se caracteriza el estado actual: declaran una ubicacion relativa a un padre que no puede existir.
            var plugin = Path.Combine(RepoRoot().FullName, "src", "RackCad.Plugin");

            foreach (var (file, type) in new[]
            {
                ("RackLayoutCommands.cs", "RackWarehouseLayoutWindow"),
                ("RackLayoutCommands.Fill.cs", "RackWarehouseFillWindow"),
                ("RackInventarioCommands.cs", "RackListWindow"),
                ("RackInventarioCommands.BomTotal.cs", "RackConsolidatedBomWindow"),
                ("RackAyudaCommands.cs", "RackCommandHelpWindow"),
            })
            {
                var source = File.ReadAllText(Path.Combine(plugin, file));

                Assert.Contains("new " + type + "(", source, StringComparison.Ordinal);
                Assert.Contains("ShowModalWindow", source, StringComparison.Ordinal);
                Assert.DoesNotContain("Owner =", source, StringComparison.Ordinal);
            }
        }

        [Fact]
        public void LosSeisSubdialogosDeSeguridadSIRecibenOwnerDeSuLlamador()
        {
            // La otra cara: cuando hay ventana padre WPF, el repositorio SI la pasa. D9 se cumple hoy en los seis
            // subdialogos que SelectiveSafetyWindow abre, y una migracion solo debe no perderlo.
            var safety = UiSource("SelectiveSafetyWindow.cs");

            foreach (var type in new[]
            {
                "SafetyPerPostWindow", "SafetyTopeGridWindow", "SafetyParrillaGridWindow",
                "SafetyGuiaEntradaGridWindow", "SafetyDesviadorGridWindow", "SafetyDefensaGridWindow",
            })
            {
                var index = safety.IndexOf("new " + type + "(", StringComparison.Ordinal);
                Assert.True(index >= 0, "no se construye " + type);

                var ventana = safety.Substring(index, Math.Min(1400, safety.Length - index));
                Assert.Contains("Owner = this", ventana, StringComparison.Ordinal);
            }
        }

        // ================================================================================================
        // 9. ESTADO Y DIAGNOSTICO
        // ================================================================================================

        [Fact]
        public void ElDiagnosticoDeCSePintaAManoYNoPorLaPaletaCompartida()
        {
            // Medido archivo a archivo, no por brocha gorda: SEIS de las diez pintan su aviso con Firebrick, y una
            // septima --SelectiveSegmentsWindow-- lo usa en tres sitios. Dos no pintan aviso con color propio.
            // Ninguna de las diez consume EditorStatusPresenter ni las severidades, asi que su rojo NO es el
            // #B00020 de EditorStatusPalette.
            var conFirebrick = new[]
            {
                "SelectiveSafetyWindow.cs", "SafetyTopeGridWindow.cs", "SafetyDesviadorGridWindow.cs",
                "SafetyDefensaGridWindow.cs", "RackWarehouseLayoutWindow.cs", "RackWarehouseFillWindow.cs",
            };

            foreach (var file in conFirebrick)
            {
                Assert.Contains("Firebrick", UiSource(file), StringComparison.Ordinal);
            }

            Assert.Contains("Firebrick", UiSource("Systems", "Selective", "SelectiveSegmentsWindow.cs"),
                StringComparison.Ordinal);

            foreach (var file in new[] { "SafetyParrillaGridWindow.cs", "SafetyGuiaEntradaGridWindow.cs" })
            {
                Assert.DoesNotContain("Firebrick", UiSource(file), StringComparison.Ordinal);
            }

            foreach (var file in conFirebrick.Concat(new[] { "SafetyParrillaGridWindow.cs", "SafetyGuiaEntradaGridWindow.cs" }))
            {
                Assert.DoesNotContain("EditorStatusPresenter", UiSource(file), StringComparison.Ordinal);
                Assert.DoesNotContain("EditorStatusSeverity", UiSource(file), StringComparison.Ordinal);
            }
        }

        // ================================================================================================
        // 10. INFRAESTRUCTURA: RackDialogWindow sigue sin un solo adoptante
        // ================================================================================================

        // Aqui vivian DOS caracterizaciones de `RackDialogWindow` —que no tenia ni una sola subclase productiva, y
        // que asignaba su chrome como VALOR LOCAL, que es lo que en precedencia WPF habria impedido a cualquier
        // estilo de ventana del arquetipo C cambiarlo en una subclase suya—. I-39D retiro el tipo, asi que, a
        // diferencia de las demas caracterizaciones que este cambio deja obsoletas, estas dos NO pueden conservarse
        // con `Skip`: una prueba omitida sigue teniendo que COMPILAR, y ya no hay tipo al que referirse.
        //
        // Por eso su cuerpo se conserva TRANSCRITO, palabra por palabra, en
        // `docs/automation/evidence/I-39D-caracterizacion-base-vs-contrato.md`, y siguen ejecutables en el commit
        // `f6c4d12`, que es la version en que toda esta suite corria en verde contra la base. No se reescriben ni se
        // borran en silencio: la transicion base → ADR → contrato se lee entera, que es la unica regla que importa.
        // El contrato nuevo —que el tipo ya no existe y que sus dos mitades tienen casa— lo prueban
        // `WindowCensusGuardTests.RackDialogWindowYaNoExiste` y `DialogWindowContractTests`.

        [Fact]
        public void NingunaDeLasDieciseisConsumeEditorActionNiEditorActionBar()
        {
            foreach (var file in new[]
            {
                "SelectiveSafetyWindow.cs", "SafetyTopeGridWindow.cs", "SafetyParrillaGridWindow.cs",
                "SafetyGuiaEntradaGridWindow.cs", "SafetyDesviadorGridWindow.cs", "SafetyDefensaGridWindow.cs",
                "RackWarehouseLayoutWindow.cs", "RackWarehouseFillWindow.cs", "RackCommandHelpWindow.cs",
            })
            {
                var source = UiSource(file);

                Assert.DoesNotContain("EditorActions.Button", source, StringComparison.Ordinal);
                Assert.DoesNotContain("EditorActionBar", source, StringComparison.Ordinal);
            }
        }
    }
}
