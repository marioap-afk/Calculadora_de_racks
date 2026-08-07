using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-37D ronda 2 — las guardas que impiden volver a lo que el dueño rechazó.
    ///
    /// Los seis motivos del rechazo son de ARQUITECTURA, y una arquitectura no la protege una prueba de
    /// comportamiento: nada falla si mañana alguien vuelve a meter el <c>SectionId</c> de la columna en la ventana
    /// principal. Estas guardas leen el CÓDIGO como texto y fallan cuando eso pasa.
    ///
    /// Ninguna guarda vigente se debilita aquí. Las dos que la ronda 1 re-apuntó están documentadas en su propio
    /// commit; éstas son nuevas y más estrechas.
    /// </summary>
    public class CantileverRoundTwoSourceGuardTests
    {
        private static DirectoryInfo RepoRoot()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);

            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "RackCad.sln")))
            {
                dir = dir.Parent;
            }

            Assert.True(dir != null, "No se encontro la raiz del repositorio (RackCad.sln).");
            return dir;
        }

        private static string Read(params string[] parts)
        {
            var path = Path.Combine(new[] { RepoRoot().FullName }.Concat(parts).ToArray());
            Assert.True(File.Exists(path), "No existe el archivo: " + path);
            return File.ReadAllText(path);
        }

        private static string CodeOnly(string source)
        {
            var withoutBlocks = Regex.Replace(source, @"/\*.*?\*/", string.Empty, RegexOptions.Singleline);
            return Regex.Replace(withoutBlocks, @"//[^\n]*", string.Empty);
        }

        /// <summary>XAML with its comments stripped, so prose that NAMES a forbidden thing does not trip a guard.</summary>
        private static string XamlCodeOnly(string source) =>
            Regex.Replace(source, @"<!--.*?-->", string.Empty, RegexOptions.Singleline);

        private static string MainWindowXaml =>
            XamlCodeOnly(Read("src", "RackCad.UI", "Systems", "Cantilever", "RackCantileverWindow.xaml"));

        private static string MainWindowCode =>
            CodeOnly(Read("src", "RackCad.UI", "Systems", "Cantilever", "RackCantileverWindow.xaml.cs"));

        private static string ComponentDirectory =>
            Path.Combine(RepoRoot().FullName, "src", "RackCad.UI", "Systems", "Cantilever", "Components");

        // ---- 1. Los parametros de componente NO vuelven a la ventana principal ---------------------------

        [Theory]
        [InlineData("ColumnSectionBox")]
        [InlineData("BaseSectionBox")]
        [InlineData("BaseLengthBox")]
        [InlineData("ColumnPlateThicknessBox")]
        [InlineData("BottomPlateEndOffsetBox")]
        [InlineData("TopPunchOffsetBox")]
        [InlineData("PunchDiameterBox")]
        [InlineData("RegularPitchBox")]
        [InlineData("ArmSectionBox")]
        [InlineData("ArmCutLengthBox")]
        [InlineData("ArmVerticalOffsetBox")]
        [InlineData("SeparatorSectionBox")]
        [InlineData("BraceSectionBox")]
        [InlineData("RodDiameterBox")]
        [InlineData("CellCutLengthBox")]
        public void LaVentanaPrincipalNoVuelveATraerParametrosDeCOMPONENTE(string control)
        {
            // Motivos 1 y 2 del rechazo. La ventana principal edita el AGREGADO; un campo interno de un
            // componente aqui es exactamente la saturacion que el dueno rechazo.
            Assert.DoesNotContain(control, MainWindowXaml, StringComparison.Ordinal);
        }

        [Fact]
        public void LaVentanaPrincipalSigueTeniendoSusCuatroTARJETAS()
        {
            var xaml = MainWindowXaml;

            foreach (var card in new[]
                     {
                         "ConfigureColumnBaseButton", "ConfigureArmButton",
                         "ConfigureSeparatorButton", "ConfigureBraceButton"
                     })
            {
                Assert.Contains(card, xaml, StringComparison.Ordinal);
            }

            foreach (var summary in new[] { "ColumnBaseSummary", "ArmSummary", "SeparatorSummary", "BraceSummary" })
            {
                Assert.Contains(summary, xaml, StringComparison.Ordinal);
            }
        }

        [Fact]
        public void ElFormularioDelBrazoNoVuelveDebajoDeLaMatriz()
        {
            var xaml = MainWindowXaml;

            Assert.Contains("EditCellArmButton", xaml, StringComparison.Ordinal);
            Assert.Contains("CellSummaryText", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("CellArrangementBox", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("ApplyCellButton", xaml, StringComparison.Ordinal);
        }

        // ---- 2. Un ComboBox de catalogo NO sustituye al selector buscable ---------------------------------

        [Fact]
        public void LosConfiguradoresEligenSeccionConElSelectorBUSCABLE()
        {
            // Motivo 3. Un ComboBox sobre 983 filas es una barra de scroll: si alguien lo devuelve, esto falla.
            foreach (var file in Directory.GetFiles(ComponentDirectory, "*.xaml"))
            {
                var xaml = XamlCodeOnly(File.ReadAllText(file));

                if (!xaml.Contains("SectionPicker", StringComparison.Ordinal) &&
                    !xaml.Contains("ColumnPicker", StringComparison.Ordinal))
                {
                    continue; // a window with no section to choose
                }

                Assert.Contains("StructuralSectionPicker", xaml, StringComparison.Ordinal);
                Assert.DoesNotMatch(
                    new Regex(@"<ComboBox[^>]*Name=""\w*Section\w*Box"""),
                    xaml);
            }
        }

        [Fact]
        public void ElSelectorNoLeeCSVNiConoceCantilever()
        {
            var picker = CodeOnly(Read("src", "RackCad.UI", "Controls", "StructuralSectionPicker.cs"));

            Assert.DoesNotContain("CsvStructuralSectionCatalogProvider", picker, StringComparison.Ordinal);
            Assert.DoesNotContain("Cantilever", picker, StringComparison.Ordinal);
            Assert.DoesNotMatch(new Regex(@"\bFile\s*\."), picker);
            Assert.DoesNotMatch(new Regex(@"\bDirectory\s*\."), picker);
        }

        // ---- 3. Buscar NO es resolver -----------------------------------------------------------------------

        [Fact]
        public void LaBusquedaParcialNoSeUsaComoRESOLUCION()
        {
            // El resolvedor de linea busca por id EXACTO. Si alguien enchufa la busqueda al resolver, esto falla.
            var applicationCantilever = Path.Combine(
                RepoRoot().FullName, "src", "RackCad.Application", "Systems", "Cantilever");

            foreach (var file in Directory.GetFiles(applicationCantilever, "*.cs", SearchOption.AllDirectories))
            {
                var code = CodeOnly(File.ReadAllText(file));

                Assert.DoesNotContain("StructuralSectionSearch", code, StringComparison.Ordinal);
                Assert.DoesNotContain("FindByDesignation", code, StringComparison.Ordinal);
                Assert.DoesNotContain("TryGetByDesignation", code, StringComparison.Ordinal);
            }
        }

        [Fact]
        public void LaBusquedaDevuelveFilasYNoResuelveNada()
        {
            var search = CodeOnly(Read("src", "RackCad.Application", "StructuralSections", "StructuralSectionSearch.cs"));

            Assert.Contains("StructuralSectionChoice", search, StringComparison.Ordinal);
            Assert.DoesNotContain("Resolve", search, StringComparison.Ordinal);
            Assert.DoesNotContain("GetById", search, StringComparison.Ordinal);
        }

        // ---- 4. La regla del follow vive en Application ------------------------------------------------------

        [Fact]
        public void LaReglaBaseFollowsColumnNoVuelveAlCodeBehind()
        {
            var window = CodeOnly(Read(
                "src", "RackCad.UI", "Systems", "Cantilever", "Components", "CantileverColumnBaseWindow.xaml.cs"));

            // La ventana PIDE; no decide. Ninguna asignacion de la bandera ni ninguna comparacion de ids aqui.
            Assert.DoesNotMatch(new Regex(@"BaseFollowsColumn\s*="), window);
            Assert.DoesNotMatch(new Regex(@"ColumnSectionId\s*==\s*\w*BaseSectionId"), window);
            Assert.Contains("state.SelectColumn(", window, StringComparison.Ordinal);
            Assert.Contains("state.SelectBase(", window, StringComparison.Ordinal);
            Assert.Contains("state.UseSameSectionAsColumn()", window, StringComparison.Ordinal);
        }

        [Fact]
        public void ElFollowNoSeDEDUCEComparandoIds()
        {
            var state = CodeOnly(Read(
                "src", "RackCad.Application", "Systems", "Cantilever", "CantileverColumnBaseEditorState.cs"));

            Assert.DoesNotMatch(new Regex(@"BaseFollowsColumn\s*=\s*\w+\s*==\s*"), state);
            Assert.Contains("Template.BaseFollowsColumn", state, StringComparison.Ordinal);
        }

        // ---- 5. La geometria del preview no se reconstruye en la UI -------------------------------------------

        [Fact]
        public void NingunaVentanaReconstruyeGeometriaDePreview()
        {
            foreach (var file in Directory.GetFiles(ComponentDirectory, "*.cs")
                         .Concat(new[] { Path.Combine(
                             RepoRoot().FullName, "src", "RackCad.UI", "Systems", "Cantilever",
                             "RackCantileverWindow.xaml.cs") }))
            {
                var code = CodeOnly(File.ReadAllText(file));

                Assert.DoesNotContain("StructuralSectionPlanBuilder", code, StringComparison.Ordinal);
                Assert.DoesNotContain("new CantileverViewCurve", code, StringComparison.Ordinal);
                Assert.DoesNotContain("SectionViewpoint.Custom", code, StringComparison.Ordinal);
            }
        }

        [Fact]
        public void ElPreviewYElMaterializadorConsumenELMISMOTipoDePlan()
        {
            var materializer = CodeOnly(Read(
                "src", "RackCad.Plugin", "Drawing", "Cantilever", "CantileverViewMaterializer.cs"));
            var renderer = CodeOnly(Read(
                "src", "RackCad.UI", "Systems", "Cantilever", "CantileverPreviewRenderer.cs"));

            Assert.Contains("CantileverViewPlan", materializer, StringComparison.Ordinal);
            Assert.Contains("CantileverViewPlan", renderer, StringComparison.Ordinal);

            // Y ninguno de los dos proyecta: los dos reciben puntos ya proyectados.
            Assert.DoesNotContain("Viewpoint", materializer, StringComparison.Ordinal);
            Assert.DoesNotContain("Viewpoint", renderer, StringComparison.Ordinal);
        }

        // ---- 6. Nada de AutoCAD fuera del Plugin --------------------------------------------------------------

        [Theory]
        [InlineData("RackCad.UI")]
        [InlineData("RackCad.Application")]
        [InlineData("RackCad.Domain")]
        public void NingunProyectoFueraDelPluginNombraAutodesk(string project)
        {
            var root = Path.Combine(RepoRoot().FullName, "src", project);

            foreach (var file in Directory.GetFiles(root, "*.cs", SearchOption.AllDirectories)
                         .Where(p => !p.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar))
                         .Where(p => !p.Contains(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar)))
            {
                Assert.DoesNotMatch(new Regex(@"\bAutodesk\."), CodeOnly(File.ReadAllText(file)));
            }
        }

        // ---- 7. El componente suelto NO es una linea -----------------------------------------------------------

        [Fact]
        public void ElComponenteSueltoNoEscribePayloadNiKindDeLinea()
        {
            var commands = CodeOnly(Read("src", "RackCad.Plugin", "RackCantileverCommands.cs"));
            var start = commands.IndexOf("DrawCantileverComponent(CantileverComponentInsertionRequest", StringComparison.Ordinal);

            Assert.True(start > 0, "No existe DrawCantileverComponent.");

            var open = commands.IndexOf('{', start);
            var depth = 0;
            var end = open;

            for (var i = open; i < commands.Length; i++)
            {
                if (commands[i] == '{') depth++;
                else if (commands[i] == '}' && --depth == 0) { end = i; break; }
            }

            var body = commands.Substring(open, end - open + 1);

            // Lo que NO hace es lo que lo mantiene honesto: sin payload, sin envelope, sin kind.
            Assert.DoesNotContain("RackBlockData", body, StringComparison.Ordinal);
            Assert.DoesNotContain("KindCantilever", body, StringComparison.Ordinal);
            Assert.DoesNotContain("RackEmbedComposer", body, StringComparison.Ordinal);
            Assert.DoesNotContain("BuildCantileverPayload", body, StringComparison.Ordinal);
        }

        [Fact]
        public void ElComponenteSueltoNoUsaElIdDeLaLinea()
        {
            var request = CodeOnly(Read("src", "RackCad.UI", "Editor", "CantileverComponentInsertionRequest.cs"));

            Assert.Contains("Guid.NewGuid", request, StringComparison.Ordinal);
            Assert.DoesNotContain("RackId", request, StringComparison.Ordinal);
            Assert.DoesNotContain("LineId", request, StringComparison.Ordinal);
            Assert.DoesNotContain("Design.Id", request, StringComparison.Ordinal);
        }

        [Fact]
        public void ElPuntoSePideANTESDeCrearNada()
        {
            var commands = CodeOnly(Read("src", "RackCad.Plugin", "RackCantileverCommands.cs"));
            var start = commands.IndexOf("DrawCantileverComponent(CantileverComponentInsertionRequest", StringComparison.Ordinal);
            var body = commands.Substring(start);

            var prompt = body.IndexOf("GetPoint(", StringComparison.Ordinal);
            var create = body.IndexOf("CreateBlockDefinitionNamed", StringComparison.Ordinal);

            Assert.True(prompt > 0 && create > 0);
            Assert.True(prompt < create, "El punto debe pedirse antes de crear una sola definicion.");
        }

        // ---- 8. Una operacion de matriz, una regeneracion -------------------------------------------------------

        [Fact]
        public void NoHayUnRecomputeDENTRODeUnBucleDeCeldas()
        {
            var window = CodeOnly(MainWindowCode);

            // Las escrituras de matriz pasan por Apply/Restore, que son operaciones AGREGADAS; el recompute va
            // despues, una sola vez. Un Recompute dentro de un foreach de celdas seria una regeneracion por celda.
            foreach (Match loop in Regex.Matches(window, @"foreach\s*\([^)]*[Cc]ell[^)]*\)\s*\{"))
            {
                var from = loop.Index;
                var depth = 0;
                var end = from;

                for (var i = window.IndexOf('{', from); i < window.Length; i++)
                {
                    if (window[i] == '{') depth++;
                    else if (window[i] == '}' && --depth == 0) { end = i; break; }
                }

                var body = window.Substring(from, end - from + 1);
                Assert.DoesNotContain("Recompute()", body, StringComparison.Ordinal);
            }
        }

        [Fact]
        public void LaMatrizAplicaPorALCANCEYNoCeldaACelda()
        {
            var window = CodeOnly(MainWindowCode);

            Assert.Contains("matrix.Apply(SelectedScope()", window, StringComparison.Ordinal);
            Assert.Contains("Matrix.Restore(SelectedScope()", window, StringComparison.Ordinal);
        }

        // ---- 9. Cancelar no muta -------------------------------------------------------------------------------

        [Fact]
        public void LaVentanaPrincipalSoloAplicaLoQueElConfiguradorACEPTO()
        {
            var window = CodeOnly(MainWindowCode);

            // Los CUATRO caminos —columna y base, separador, tensor y brazo— comprueban el resultado nulo y
            // RETORNAN antes de tocar el diseno. Si alguien anade un quinto configurador sin esa guarda, el
            // conteo deja de cuadrar y esto falla.
            Assert.Equal(4, Regex.Matches(window, @"if \(window\.Result == null\)").Count);

            // Y cada una de esas cuatro guardas RETORNA. El comentario que lo explica no se busca aqui: el
            // codigo se lee sin comentarios a proposito.
            Assert.Equal(
                4,
                Regex.Matches(window, @"if \(window\.Result == null\)\s*\{\s*return;").Count);
        }

        [Fact]
        public void LosConfiguradoresEditanUnaCOPIA()
        {
            foreach (var file in Directory.GetFiles(ComponentDirectory, "*.xaml.cs"))
            {
                var code = CodeOnly(File.ReadAllText(file));

                Assert.Contains("DeepCopy()", code, StringComparison.Ordinal);
            }
        }

        // ---- 10. Los diagnosticos del componente se ven EN el componente ----------------------------------------

        [Fact]
        public void CadaConfiguradorMuestraSusPropiosDiagnosticos()
        {
            foreach (var file in Directory.GetFiles(ComponentDirectory, "*.xaml"))
            {
                var xaml = XamlCodeOnly(File.ReadAllText(file));

                Assert.Contains("DiagnosticsText", xaml, StringComparison.Ordinal);
                Assert.Contains("EditorShell.Diagnostics", xaml, StringComparison.Ordinal);
            }
        }

        [Fact]
        public void ElShellDeComponenteTieneSusSieteRanuras()
        {
            // I-39A REAPUNTA esta guarda, no la debilita. La intencion original -- que el shell que componen los
            // cuatro configuradores expone SIETE ranuras y ninguna se pierde -- se conserva palabra por palabra; lo
            // unico que cambia es donde vive el tipo que las declara. El shell nunca supo nada de Cantilever (siete
            // DP de tipo object, cero ramas): solo su nombre y su ubicacion lo ataban a un sistema, y un consumidor
            // de otro sistema habria tenido que depender de este namespace para reusarlo. Ahora vive en
            // RackCad.UI/Shell y CantileverComponentEditorShell es una fachada que deriva de el, de modo que los
            // cuatro XAML de componente siguen sin tocarse. I-39C los migra y retira la fachada.
            var shell = CodeOnly(Read("src", "RackCad.UI", "Shell", "RackBoundedEditorShell.cs"));

            foreach (var slot in new[]
                     {
                         "HeaderProperty", "ParametersProperty", "SectionPickerProperty", "PreviewProperty",
                         "DiagnosticsProperty", "BomSummaryProperty", "ActionsProperty"
                     })
            {
                Assert.Contains(slot, shell, StringComparison.Ordinal);
            }
        }

        [Fact]
        public void LaFachadaDeCantileverSigueSiendoElShellQueLosCuatroXamlNOMBRAN()
        {
            // El complemento de la guarda anterior: reapuntarla no puede significar que nadie vigile la fachada.
            // Los cuatro XAML siguen nombrando components:CantileverComponentEditorShell, asi que el tipo tiene que
            // existir en su ruta, derivar del shell neutral y NO re-declarar ninguna ranura (si las re-declarase
            // seria codigo escrito para satisfacer una guarda, que es justo lo prohibido).
            var facade = CodeOnly(Read(
                "src", "RackCad.UI", "Systems", "Cantilever", "Components", "CantileverComponentEditorShell.cs"));

            Assert.Contains("class CantileverComponentEditorShell : RackBoundedEditorShell", facade, StringComparison.Ordinal);
            Assert.DoesNotContain("DependencyProperty.Register", facade, StringComparison.Ordinal);
            Assert.DoesNotContain("DefaultStyleKeyProperty", facade, StringComparison.Ordinal);

            foreach (var file in Directory.GetFiles(ComponentDirectory, "*.xaml"))
            {
                var xaml = XamlCodeOnly(File.ReadAllText(file));

                Assert.Contains("CantileverComponentEditorShell", xaml, StringComparison.Ordinal);
            }
        }
    }
}
