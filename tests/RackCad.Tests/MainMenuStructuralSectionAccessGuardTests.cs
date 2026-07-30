using System;
using System.IO;
using System.Linq;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-36C — que el menu principal sea una PUERTA al generador de perfiles estructurales, no una copia.
    ///
    /// El generador ya existe y esta cerrado: catalogo de I-36A, geometria de I-36B, inspector, preview y
    /// materializacion. Lo unico que faltaba era poder llegar a el desde `RACKCAD`. El riesgo real de ese
    /// cambio no es que el boton no funcione —eso se ve al primer clic— sino que el flujo se COPIE en
    /// `RackMenuCommands`: dos caminos que hoy coinciden y que divergen a la primera correccion, porque la
    /// carga fail-closed, el aviso de unidades, la transaccion, el regen y el mensaje de fidelidad habria que
    /// arreglarlos dos veces y nada avisaria de que solo se arreglo uno.
    ///
    /// El Plugin referencia AutoCAD, asi que esta suite NO puede cargarlo (ADR-0003) y la CI no tiene AutoCAD.
    /// Se leen los `.cs` como TEXTO, igual que <see cref="StructuralSectionPluginSourceGuardTests"/>.
    /// </summary>
    public class MainMenuStructuralSectionAccessGuardTests
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

        private static string PluginDirectory => Path.Combine(RepoRoot().FullName, "src", "RackCad.Plugin");

        private static string ReadPlugin(params string[] parts)
        {
            var path = Path.Combine(PluginDirectory, Path.Combine(parts));
            Assert.True(File.Exists(path), "No existe el archivo: " + path);
            return File.ReadAllText(path);
        }

        private static string Menu => ReadPlugin("RackMenuCommands.cs");

        private static string Command => ReadPlugin("RackSeccionCommands.cs");

        private static string Flow => ReadPlugin("StructuralSectionCommandFlow.cs");

        /// <summary>Los archivos del Plugin cuyo CODIGO —sin comentarios— menciona un simbolo.</summary>
        private static string[] PluginFilesMentioning(string symbol) =>
            Directory.GetFiles(PluginDirectory, "*.cs", SearchOption.AllDirectories)
                .Where(path => !path.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar))
                .Where(path => !path.Contains(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar))
                .Where(path => CodeOnly(File.ReadAllText(path)).Contains(symbol, StringComparison.Ordinal))
                .Select(Path.GetFileName)
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToArray();

        /// <summary>
        /// El archivo sin comentarios.
        ///
        /// Las guardas de "esto no se menciona" tienen que mirar CODIGO. La prosa de estos archivos explica
        /// justamente por que el flujo vive en un solo sitio, y prohibir nombrar aquello que se evita
        /// expulsaria el razonamiento del codigo.
        /// </summary>
        private static string CodeOnly(string source)
        {
            var withoutBlocks = System.Text.RegularExpressions.Regex.Replace(
                source, @"/\*.*?\*/", string.Empty, System.Text.RegularExpressions.RegexOptions.Singleline);

            return System.Text.RegularExpressions.Regex.Replace(withoutBlocks, @"//[^\n]*", string.Empty);
        }

        // ---- Una sola autoridad --------------------------------------------------------------------------

        [Fact]
        public void TheMenuAndTheCommandRunTheSameFlow()
        {
            Assert.Contains("StructuralSectionCommandFlow.Run(document)", CodeOnly(Menu), StringComparison.Ordinal);
            Assert.Contains("StructuralSectionCommandFlow.Run(", CodeOnly(Command), StringComparison.Ordinal);
        }

        [Theory]
        [InlineData("StructuralSectionInspectorWindow", "StructuralSectionCommandFlow.cs")]      // el inspector
        [InlineData("StructuralSectionInsertService", "StructuralSectionCommandFlow.cs")]        // la insercion
        [InlineData("WeightInPounds", "StructuralSectionCommandFlow.cs")]                        // el calculo de peso
        [InlineData("SectionFidelity", "StructuralSectionCommandFlow.cs")]                       // la fidelidad
        [InlineData("CsvStructuralSectionCatalogProvider", "StructuralSectionCatalogAccess.cs")] // la carga
        [InlineData("StructuralSectionCatalogException", "StructuralSectionCatalogAccess.cs")]   // el fallo cerrado
        public void ExactlyOnePlaceInThePluginOwnsEachPieceOfTheFlow(string symbol, string owner)
        {
            // I-37D: la CARGA del catalogo y su fallo cerrado dejaron de ser exclusivos del flujo de secciones,
            // porque la linea Cantilever tambien se construye sobre el catalogo neutral. La respuesta fue EXTRAER
            // el duenno —StructuralSectionCatalogAccess— y RE-APUNTAR la guarda, no debilitarla: sigue habiendo
            // exactamente un sitio por pieza, y copiar la carga en un segundo archivo seguiria fallando aqui.
            var files = PluginFilesMentioning(symbol);

            Assert.Equal(new[] { owner }, files);
        }

        [Theory]
        [InlineData("CsvStructuralSectionCatalogProvider")]
        [InlineData("StructuralSectionInspectorWindow")]
        [InlineData("StructuralSectionInsertService")]
        [InlineData("ShowModalWindow(window)")]
        [InlineData("WarnIfNotInches")]
        public void TheMenuDuplicatesNothingOfTheFlow(string symbol)
        {
            // El menu invoca; no reimplementa. La comprobacion se acota a la RAMA de la accion, no al resto
            // del metodo: el aviso de unidades de las inserciones de rack es anterior a este fix y sigue ahi,
            // asi que buscarlo en todo el archivo diria que hay duplicacion donde no la hay.
            Assert.DoesNotContain(symbol, StructuralSectionBranchOf(CodeOnly(Menu)), StringComparison.Ordinal);
        }

        /// <summary>
        /// El cuerpo del <c>if</c> que despacha la accion de seccion estructural, emparejando llaves.
        ///
        /// Acotar por indices fijos seria fragil; contar llaves da el bloque exacto aunque el metodo crezca.
        /// </summary>
        private static string StructuralSectionBranchOf(string menu)
        {
            var start = menu.IndexOf("if (menu.RequestedAction == MainMenuAction.GenerateStructuralSection)",
                StringComparison.Ordinal);

            Assert.True(start > 0, "El menu debe despachar la accion tipada de seccion estructural.");

            var open = menu.IndexOf('{', start);
            Assert.True(open > start);

            var depth = 0;

            for (var i = open; i < menu.Length; i++)
            {
                if (menu[i] == '{')
                {
                    depth++;
                }
                else if (menu[i] == '}')
                {
                    depth--;

                    if (depth == 0)
                    {
                        return menu.Substring(start, i - start + 1);
                    }
                }
            }

            Assert.Fail("La rama de la accion no cierra sus llaves.");
            return string.Empty;
        }

        [Fact]
        public void TheCommandIsNowAThinEntryPoint()
        {
            // RACKSECCION sigue existiendo y sigue siendo el camino directo; lo que ya no tiene es el flujo.
            var command = CodeOnly(Command);

            Assert.Contains("[CommandMethod(\"RACKSECCION\")]", command, StringComparison.Ordinal);
            Assert.Contains("StructuralSectionCommandFlow.Run(AcApplication.DocumentManager.MdiActiveDocument)",
                command, StringComparison.Ordinal);
            Assert.DoesNotContain("ShowModalWindow", command, StringComparison.Ordinal);
        }

        // ---- El flujo conserva lo que tenia ---------------------------------------------------------------

        [Fact]
        public void AnInvalidCatalogueStillFailsClosed()
        {
            // I-37D movio la CAPTURA al duenno unico de la carga (StructuralSectionCatalogAccess); lo que esta
            // guarda protege —fallar cerrado ANTES de abrir el inspector— se sigue viendo aqui, en el flujo.
            var flow = Flow;
            var guard = flow.IndexOf("StructuralSectionCatalogAccess.TryLoad", StringComparison.Ordinal);

            Assert.True(guard > 0, "El flujo debe cargar el catalogo por el duenno unico.");
            Assert.Contains("return;", flow.Substring(guard, Math.Min(200, flow.Length - guard)), StringComparison.Ordinal);

            // Y falla cerrada ANTES de abrir nada: no se enseña un inspector sobre datos que no validaron.
            Assert.True(guard < flow.IndexOf("new StructuralSectionInspectorWindow", StringComparison.Ordinal));

            var access = ReadPlugin("StructuralSectionCatalogAccess.cs");
            var handler = access.IndexOf("catch (StructuralSectionCatalogException", StringComparison.Ordinal);

            Assert.True(handler > 0, "El duenno de la carga debe capturar StructuralSectionCatalogException.");
            Assert.Contains(
                "no es valido",
                access.Substring(handler, Math.Min(400, access.Length - handler)),
                StringComparison.Ordinal);
        }

        [Fact]
        public void CancellingTheInspectorMaterialisesNothing()
        {
            var flow = CodeOnly(Flow);
            var cancel = flow.IndexOf("if (window.Result == null)", StringComparison.Ordinal);
            var insert = flow.IndexOf("StructuralSectionInsertService.Insert", StringComparison.Ordinal);

            Assert.True(cancel > 0 && insert > 0);
            Assert.True(cancel < insert, "La salida por cancelacion tiene que estar ANTES de insertar nada.");
            Assert.Contains("return;", flow.Substring(cancel, Math.Min(120, flow.Length - cancel)),
                StringComparison.Ordinal);
        }

        [Fact]
        public void TheUnitsAdvisoryAndTheFidelityMessageSurvive()
        {
            var flow = CodeOnly(Flow);

            Assert.Contains("RackUnitsGuard.WarnIfNotInches(document)", flow, StringComparison.Ordinal);
            Assert.Contains("plan.Fidelity != SectionFidelity.TabulatedComplete", flow, StringComparison.Ordinal);
            Assert.Contains("diagnostic.Message", flow, StringComparison.Ordinal);
        }

        [Fact]
        public void TheAdvisoryFiresOnceAndOnlyAfterTheUserConfirmed()
        {
            // Dos avisos de unidades para una sola insercion serian ruido. El del menu cubre las inserciones de
            // rack; el del flujo cubre la seccion, y salta despues del inspector, no antes.
            var flow = CodeOnly(Flow);

            Assert.Equal(1, System.Text.RegularExpressions.Regex.Matches(flow, @"WarnIfNotInches").Count);
            Assert.True(flow.IndexOf("if (window.Result == null)", StringComparison.Ordinal) <
                        flow.IndexOf("WarnIfNotInches", StringComparison.Ordinal));
        }

        // ---- La accion no es un rack -----------------------------------------------------------------------

        [Fact]
        public void TheActionIsDispatchedOutsideTheRackSwitch()
        {
            // Una seccion no tiene RackSystemKind ni payload de diseño. Se despacha por su propia rama, ANTES
            // del switch de InsertionRequest, y con un return que impide caer en el.
            var menu = CodeOnly(Menu);
            var action = menu.IndexOf("if (menu.RequestedAction == MainMenuAction.GenerateStructuralSection)",
                StringComparison.Ordinal);
            var rackSwitch = menu.IndexOf("switch (menu.InsertionRequest)", StringComparison.Ordinal);

            Assert.True(action > 0, "Falta el despacho de la accion tipada.");
            Assert.True(rackSwitch > 0);
            Assert.True(action < rackSwitch);

            // El cuerpo del if, no todo lo que hay hasta el switch: entre medias vive el aviso de unidades de
            // las inserciones de rack, que es anterior a este fix y menciona InsertionRequest con razon.
            var branch = StructuralSectionBranchOf(menu);
            Assert.Contains("return;", branch, StringComparison.Ordinal);
            Assert.DoesNotContain("RackSystemKind", branch, StringComparison.Ordinal);
            Assert.DoesNotContain("InsertionRequest", branch, StringComparison.Ordinal);
        }

        [Theory]
        [InlineData("RackEmbedDocument")]
        [InlineData("RackBlockData")]
        [InlineData("RackSystemKind")]
        [InlineData("Guid")]
        public void AnInsertedSectionIsStillNotARack(string forbidden)
        {
            Assert.DoesNotContain(forbidden, CodeOnly(Flow), StringComparison.Ordinal);
        }

        [Fact]
        public void TheFlowBuildsNoGeometryOfItsOwn()
        {
            // I-36C no reabre la geometria. El flujo mueve un plan que Application ya construyo.
            var flow = CodeOnly(Flow);

            Assert.DoesNotContain("StructuralSectionGeometryFactory", flow, StringComparison.Ordinal);
            Assert.DoesNotContain("GeometryBuilder", flow, StringComparison.Ordinal);
            Assert.DoesNotContain("SectionDimensions", flow, StringComparison.Ordinal);
            Assert.DoesNotContain("StructuralSectionPlanBuilder", flow, StringComparison.Ordinal);
        }

        // ---- Los sistemas vigentes siguen igual ------------------------------------------------------------

        [Theory]
        [InlineData("HeaderInsertionRequest header")]
        [InlineData("DynamicInsertionRequest dynamic")]
        [InlineData("FlowBedInsertionRequest cama")]
        [InlineData("SelectiveInsertionRequest selective")]
        [InlineData("PushBackInsertionRequest pushBack")]
        public void EveryExistingSystemKeepsItsDispatch(string branch)
        {
            Assert.Contains("case " + branch + ":", CodeOnly(Menu), StringComparison.Ordinal);
        }
    }
}
