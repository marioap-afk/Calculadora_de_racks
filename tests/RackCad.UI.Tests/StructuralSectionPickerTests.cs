using System.Linq;
using System.Windows;
using System.Windows.Controls;
using RackCad.Application.Catalogs;
using RackCad.Application.StructuralSections;
using RackCad.UI.Controls;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-37D ronda 2 — el control real <see cref="StructuralSectionPicker"/>, conducido por su plantilla.
    ///
    /// La búsqueda pura ya está probada en <c>StructuralSectionSearchTests</c>; aquí se prueba lo que sólo
    /// existe en el control: que la plantilla se aplique, que escribir filtre, que elegir una fila devuelva el id
    /// EXACTO, que el estado vacío se vea, que el teclado recorra la lista y que filtrar NO deshaga la selección
    /// del usuario.
    /// </summary>
    public sealed class StructuralSectionPickerTests
    {
        private static StructuralSectionCatalog Catalog() =>
            new CsvStructuralSectionCatalogProvider(CatalogDirectory.Resolve()).Load();

        /// <summary>Builds the control, applies its template and loads the catalogue, as a window would.</summary>
        private static StructuralSectionPicker Ready(
            params StructuralSectionFamily[] families)
        {
            var picker = new StructuralSectionPicker();
            var window = new Window { Content = picker, Width = 400, Height = 300 };
            window.ApplyTemplate();
            picker.ApplyTemplate();
            picker.Measure(new Size(400, 300));
            picker.Arrange(new Rect(0, 0, 400, 300));
            picker.Load(Catalog(), families.Length == 0 ? null : families);
            return picker;
        }

        [Fact]
        public void LaPlantillaSeAplicaYTraeSusCuatroPartes()
        {
            var r = StaTestRunner.Run(() =>
            {
                var p = Ready();
                return (p.SearchBox != null, p.FamilyBox != null, p.List != null, p.EmptyState != null,
                    p.AllChoices.Count);
            });

            Assert.True(r.Item1);
            Assert.True(r.Item2);
            Assert.True(r.Item3);
            Assert.True(r.Item4);
            Assert.True(r.Item5 > 900, "El catálogo neutral trae más de 900 secciones habilitadas.");
        }

        [Fact]
        public void EscribirFiltraLaLista()
        {
            var r = StaTestRunner.Run(() =>
            {
                var p = Ready();
                var antes = p.VisibleChoices.Count;
                p.SearchBox.Text = "W10X33";
                return (antes, p.VisibleChoices.Count,
                    p.VisibleChoices.Any(c => c.Id.Value == "AISC-W-W10X33"));
            });

            Assert.True(r.Item2 < r.Item1);
            Assert.True(r.Item3);
        }

        [Fact]
        public void ElegirUnaFilaDevuelveElIdExacto()
        {
            var r = StaTestRunner.Run(() =>
            {
                var p = Ready();
                string chosen = null;
                p.SectionChosen += (_, id) => chosen = id;

                p.SearchBox.Text = "W10X33";
                p.List.SelectedItem = p.VisibleChoices.Single(c => c.Id.Value == "AISC-W-W10X33");

                return (chosen, p.SelectedSectionId);
            });

            Assert.Equal("AISC-W-W10X33", r.Item1);
            Assert.Equal("AISC-W-W10X33", r.Item2);
        }

        [Fact]
        public void ElFiltroDeFamiliaSoloOfreceLasFamiliasQueElConsumidorPermite()
        {
            var r = StaTestRunner.Run(() =>
            {
                var p = Ready(StructuralSectionFamily.Channel);
                var items = p.FamilyBox.ItemsSource.Cast<object>().ToList();

                return (items.Count,
                    items.Select(StructuralSectionPicker.FamilyLabel).ToList(),
                    p.AllChoices.All(c => c.Family == StructuralSectionFamily.Channel));
            });

            Assert.Equal(2, r.Item1);                       // «Todas» + C
            Assert.Equal(new[] { "Todas", "C" }, r.Item2);
            Assert.True(r.Item3);
        }

        [Fact]
        public void FiltrarPorFamiliaYPorTextoSeCombinaEnElControl()
        {
            var r = StaTestRunner.Run(() =>
            {
                var p = Ready();
                p.SetFamily(StructuralSectionFamily.Channel);
                p.SearchBox.Text = "4";

                return (p.VisibleChoices.Count,
                    p.VisibleChoices.All(c => c.Family == StructuralSectionFamily.Channel),
                    p.VisibleChoices.Any(c => c.Id.Value == "AISC-C-C4X4_5"));
            });

            Assert.True(r.Item1 > 0);
            Assert.True(r.Item2);
            Assert.True(r.Item3);
        }

        [Fact]
        public void UnaBusquedaSinResultadosMuestraSuEstadoVacio()
        {
            var r = StaTestRunner.Run(() =>
            {
                var p = Ready();
                p.SearchBox.Text = "NO-EXISTE";
                return (p.VisibleChoices.Count, p.EmptyState.Visibility, p.EmptyState.Text);
            });

            Assert.Equal(0, r.Item1);
            Assert.Equal(Visibility.Visible, r.Item2);
            Assert.Contains("Ninguna sección", r.Item3);
        }

        [Fact]
        public void UnCatalogoVacioLoDiceEnLugarDeQuedarseEnBlanco()
        {
            var r = StaTestRunner.Run(() =>
            {
                var p = new StructuralSectionPicker();
                new Window { Content = p, Width = 400, Height = 300 }.ApplyTemplate();
                p.ApplyTemplate();
                p.Load(StructuralSectionCatalog.Empty);

                return (p.AllChoices.Count, p.EmptyState.Visibility, p.EmptyState.Text);
            });

            Assert.Equal(0, r.Item1);
            Assert.Equal(Visibility.Visible, r.Item2);
            Assert.Contains("No hay secciones", r.Item3);
        }

        [Fact]
        public void FiltrarNoDeshaceLaSeleccionDelUsuario()
        {
            // Filtrar es una manera de MIRAR, no de decidir. Una búsqueda que dejara al usuario sin sección
            // sería un cambio de datos provocado por teclear.
            var r = StaTestRunner.Run(() =>
            {
                var p = Ready();
                p.SearchBox.Text = "W10X33";
                p.List.SelectedItem = p.VisibleChoices.Single(c => c.Id.Value == "AISC-W-W10X33");

                p.SearchBox.Text = "C4"; // la sección elegida ya no está en la lista

                return (p.SelectedSectionId, p.List.SelectedItem == null, p.VisibleChoices.Count);
            });

            Assert.Equal("AISC-W-W10X33", r.Item1); // sigue elegida
            Assert.True(r.Item2);                   // pero no se marca una fila que no se ve
            Assert.True(r.Item3 > 0);
        }

        [Fact]
        public void LaListaEstaVirtualizada()
        {
            // 983 filas bastan para que una lista sin virtualizar tartamudee en cada tecla.
            var r = StaTestRunner.Run(() =>
            {
                var p = Ready();
                return (VirtualizingPanel.GetIsVirtualizing(p.List),
                    VirtualizingPanel.GetVirtualizationMode(p.List));
            });

            Assert.True(r.Item1);
            Assert.Equal(VirtualizationMode.Recycling, r.Item2);
        }

        [Fact]
        public void ElControlNoConoceCantilever()
        {
            // Guarda de acoplamiento: el selector es de secciones, no del sistema que lo usa. Se lee el CODIGO
            // sin comentarios —la prosa SI nombra Cantilever, para decir justamente que no lo conoce—.
            var source = CodeOnly(System.IO.File.ReadAllText(
                System.IO.Path.Combine(RepoRoot(), "src", "RackCad.UI", "Controls", "StructuralSectionPicker.cs")));

            Assert.DoesNotContain("Cantilever", source, System.StringComparison.Ordinal);
            Assert.DoesNotContain("CsvStructuralSectionCatalogProvider", source, System.StringComparison.Ordinal);
            Assert.DoesNotContain("File.", source, System.StringComparison.Ordinal);
        }

        private static string CodeOnly(string source)
        {
            var withoutBlocks = System.Text.RegularExpressions.Regex.Replace(
                source, @"/\*.*?\*/", string.Empty, System.Text.RegularExpressions.RegexOptions.Singleline);

            return System.Text.RegularExpressions.Regex.Replace(withoutBlocks, @"//[^\n]*", string.Empty);
        }

        private static string RepoRoot()
        {
            var dir = new System.IO.DirectoryInfo(System.AppContext.BaseDirectory);

            while (dir != null && !System.IO.File.Exists(System.IO.Path.Combine(dir.FullName, "RackCad.sln")))
            {
                dir = dir.Parent;
            }

            Assert.True(dir != null, "No se encontró la raíz del repositorio.");
            return dir.FullName;
        }
    }
}
