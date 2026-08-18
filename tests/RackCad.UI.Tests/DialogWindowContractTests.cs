using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using RackCad.UI;
using RackCad.UI.Controls;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// El contrato NUEVO de los arquetipos C y D, el que I-39D establece. Vive en una clase aparte de
    /// <see cref="DialogWindowCharacterizationTests"/> a proposito: aquella conserva intacto el comportamiento
    /// anterior —incluidas, con <c>Skip</c>, las pruebas que este cambio deja obsoletas— de modo que la transicion
    /// base → ADR → contrato se lea entera en el historial y no como una prueba reescrita.
    ///
    /// <para>Autorizado por ADR-0029: D9 (contrato por arquetipo, sin herencia implicita de otro), D11 (adoptar antes
    /// que abstraer) y las decisiones 26 a 28 del Owner (adoptar o evolucionar lo escrito, adopcion gradual, y
    /// ningun modelo paralelo).</para>
    /// </summary>
    public sealed class DialogWindowContractTests
    {
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
            => File.ReadAllText(Path.Combine(new[] { RepoRoot().FullName, "src", "RackCad.UI" }.Concat(relative).ToArray()));

        /// <summary>Las NUEVE que adoptan el contrato. La decima, <c>SafetyDefensaGridWindow</c>, queda fuera hasta la
        /// fase observable: es la unica que no asignaba el chrome y adoptarlo le cambia el fondo.</summary>
        private static Func<Window>[] Nine() => new Func<Window>[]
        {
            () => new SelectiveSafetyWindow(new List<RackCad.Application.Catalogs.SafetyElementCatalogEntry>(),
                    Enumerable.Empty<RackCad.Domain.Systems.Selective.SelectiveSafetySelection>(), 3),
            () => new SafetyPerPostWindow("Bota", 3, RackCad.Domain.Systems.Selective.SafetySide.Both,
                    Enumerable.Empty<RackCad.Domain.Systems.Selective.SafetyPostSide>()),
            () => new SafetyTopeGridWindow("Tope", new[] { 3 }, false,
                    RackCad.Domain.Systems.Selective.SafetySide.Both, 2.0, true,
                    new List<RackCad.Domain.Systems.Selective.SelectiveGridCell>()),
            () => new SafetyParrillaGridWindow("Parrilla", new[] { 3 }, true, false, 96.0, 2,
                    new List<RackCad.Domain.Systems.Selective.SelectiveGridCell>()),
            () => new SafetyGuiaEntradaGridWindow("Guia", new[] { 3 },
                    new List<RackCad.Domain.Systems.Selective.SelectiveGridCell>()),
            () => new SafetyDesviadorGridWindow("desviador", "Desviador", null, null, 48.0, 12.0,
                    RackCad.Domain.Systems.Selective.SafetySide.Both,
                    new List<RackCad.Domain.Systems.Selective.SelectiveGridCell>(), 3, new[] { 3 }),
            () => new RackCad.UI.Systems.Selective.SelectiveSegmentsWindow(1,
                    Enumerable.Empty<RackCad.Domain.Systems.Selective.SelectiveSegment>(), 96.0),
            () => new RackWarehouseLayoutWindow("R1", 48.0, 96.0),
            () => new RackWarehouseFillWindow("R1", 48.0, 96.0),
        };

        // ---- 1. el chrome del arquetipo C vive en UNA sola fuente ----

        [Fact]
        public void LasNueveAplicanElContratoDeVentanaDelArquetipoC()
        {
            StaTestRunner.Run(() =>
            {
                var expected = (Style)AppStyles()[DialogWindowChrome.StyleKey];

                foreach (var build in Nine())
                {
                    var window = build();

                    Assert.NotNull(window.Style);
                    Assert.Equal(expected.TargetType, window.Style.TargetType);
                    Assert.Equal(expected.Setters.Count, window.Style.Setters.Count);
                }
            });
        }

        [Fact]
        public void AdoptarloNoMueveUnSoloPixel()
        {
            StaTestRunner.Run(() =>
            {
                // La prueba de que la fuente unica es equivalente: el fondo sigue siendo el MISMO
                // WindowBackgroundBrush que estas ventanas ya pintaban, y la tipografia la misma Segoe UI.
                var fondo = ((SolidColorBrush)AppStyles()["WindowBackgroundBrush"]).Color;

                foreach (var build in Nine())
                {
                    var window = build();

                    Assert.Equal(fondo, ((SolidColorBrush)window.Background).Color);
                    Assert.Equal("Segoe UI", window.FontFamily?.Source);
                }
            });
        }

        [Fact]
        public void NingunaDeLasNueveRepiteYaElBloqueAMano()
        {
            foreach (var file in new[]
            {
                new[] { "SelectiveSafetyWindow.cs" },
                new[] { "SafetyTopeGridWindow.cs" },
                new[] { "SafetyParrillaGridWindow.cs" },
                new[] { "SafetyGuiaEntradaGridWindow.cs" },
                new[] { "SafetyDesviadorGridWindow.cs" },
                new[] { "RackWarehouseLayoutWindow.cs" },
                new[] { "RackWarehouseFillWindow.cs" },
                new[] { "Systems", "Selective", "SelectiveSegmentsWindow.cs" },
            })
            {
                var source = UiSource(file);

                Assert.Contains("DialogWindowChrome.Apply(this)", source, StringComparison.Ordinal);
                Assert.DoesNotContain("FontFamily = new FontFamily(\"Segoe UI\")", source, StringComparison.Ordinal);
                Assert.DoesNotContain("TryFindResource(\"WindowBackgroundBrush\")", source, StringComparison.Ordinal);
            }
        }

        // ---- 2. el contrato NO impone tamano ni ubicacion, y eso es deliberado ----

        [Fact]
        public void ElContratoDeCNoLlevaNiTamanoNiUbicacion()
        {
            StaTestRunner.Run(() =>
            {
                // ADR-0029 D9: dos de los diez se dimensionan por su contenido y cuatro calculan su tamano de la
                // matriz; y cinco no pueden tener Owner. Un contrato con minimos o con ubicacion reproduciria aqui
                // la anomalia de letra muerta que I-39C acaba de cerrar en el arquetipo B.
                var style = (Style)AppStyles()[DialogWindowChrome.StyleKey];
                var propiedades = style.Setters.OfType<Setter>().Select(s => s.Property.Name).ToList();

                Assert.Equal(new[] { "Background", "FontFamily" }, propiedades.OrderBy(x => x, StringComparer.Ordinal).ToArray());
            });
        }

        [Fact]
        public void CadaVentanaSigueDeclarandoSuPropiaUbicacionYSuPropioTamano()
        {
            StaTestRunner.Run(() =>
            {
                // Adoptar el chrome no toco ni una sola de las dos dimensiones que la evidencia dice que divergen.
                var layout = new RackWarehouseLayoutWindow("R1", 48.0, 96.0);
                Assert.Equal(SizeToContent.Height, layout.SizeToContent);
                Assert.Equal(WindowStartupLocation.CenterOwner, layout.WindowStartupLocation);

                var tope = new SafetyTopeGridWindow("Tope", new[] { 3 }, false,
                    RackCad.Domain.Systems.Selective.SafetySide.Both, 2.0, true,
                    new List<RackCad.Domain.Systems.Selective.SelectiveGridCell>());
                Assert.True(tope.Width > 0 && !double.IsNaN(tope.Width));
                Assert.True(tope.MinWidth > 0);
            });
        }

        // ---- 3. lo observable que I-39D corrige ----

        [Fact]
        public void SafetyDefensaYaAbreConElFondoCompartidoComoSusNueveHermanas()
        {
            StaTestRunner.Run(() =>
            {
                // Era la unica de los diez que no aplicaba el chrome, y era una omision y no una decision: ni un
                // comentario ni una prueba la respaldaban. Su unico delta observable era el FONDO --abria en blanco
                // liso-- porque la tipografia ya resolvia a Segoe UI por ser la predeterminada del sistema.
                var defensa = new SafetyDefensaGridWindow("Defensa", 3,
                    Enumerable.Empty<RackCad.Domain.Systems.Selective.SafetyPostDefense>());

                var fondo = ((SolidColorBrush)AppStyles()["WindowBackgroundBrush"]).Color;

                Assert.Equal(fondo, ((SolidColorBrush)defensa.Background).Color);
                Assert.NotEqual(Colors.White, ((SolidColorBrush)defensa.Background).Color);
                Assert.NotNull(defensa.Style);
                Assert.Contains("DialogWindowChrome.Apply(this)", UiSource("SafetyDefensaGridWindow.cs"),
                    StringComparison.Ordinal);
            });
        }

        [Fact]
        public void ElMotivoDeBloqueoDeLaBarraMasivaSeLeeConElBotonAPAGADO()
        {
            // ADR-0029 D6. La barra calculaba el motivo y lo ponia en el ToolTip, pero WPF no muestra la ayuda de un
            // control deshabilitado si nadie lo pide: el motivo existia y era ilegible justo cuando importaba. Era
            // la unica cobertura de D6 en el arquetipo C, y estaba a medias.
            Assert.Contains("ToolTipService.SetShowOnDisabled(button, true)",
                UiSource("Controls", "SelectionMatrixBulkBar.cs"), StringComparison.Ordinal);
        }

        [Fact]
        public void ElDiagnosticoObsoletoSeLimpiaAlRevalidar()
        {
            // Dos ventanas escribian su aviso y no lo borraban nunca, asi que un problema ya corregido seguia en
            // pantalla acusando a un poste que ya estaba bien. En el desviador era mas sutil: solo se limpiaba
            // cuando el llamador pedia mostrarlo, y la ruta viva pasa el contrario.
            Assert.Contains("error.Text = string.Empty;", UiSource("SafetyDefensaGridWindow.cs"), StringComparison.Ordinal);
            Assert.DoesNotContain("if (showError) error.Text = string.Empty;",
                UiSource("SafetyDesviadorGridWindow.cs"), StringComparison.Ordinal);
        }

        // ---- 4. el helper es composicion, no herencia, y no conoce ningun sistema (D12) ----

        [Fact]
        public void ElChromeComunEsUnHelperYNoUnaBase()
        {
            var source = UiSource("Controls", "DialogWindowChrome.cs");

            Assert.Contains("public static class DialogWindowChrome", source, StringComparison.Ordinal);
            Assert.DoesNotContain(": Window", source, StringComparison.Ordinal);

            // Y no conoce ningun sistema (D12). Se mide sobre el CODIGO con los comentarios retirados, que es el
            // mismo criterio que usan las guardas de I-37D y de I-39A: una guarda tiene que cazar un acoplamiento
            // escrito en codigo, no una prosa que se limite a NOMBRAR el sistema del que vino la pieza.
            var code = System.Text.RegularExpressions.Regex.Replace(
                System.Text.RegularExpressions.Regex.Replace(source, @"/\*.*?\*/", string.Empty,
                    System.Text.RegularExpressions.RegexOptions.Singleline),
                @"//[^\n]*", string.Empty);

            foreach (var forbidden in new[] { "Safety", "Selective", "Warehouse", "Cantilever", "AutoCAD", "Autodesk" })
            {
                Assert.DoesNotContain(forbidden, code, StringComparison.Ordinal);
            }
        }
    }
}
