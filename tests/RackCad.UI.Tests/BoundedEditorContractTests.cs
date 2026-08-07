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
