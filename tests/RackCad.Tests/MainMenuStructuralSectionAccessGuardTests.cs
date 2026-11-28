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

        /// <summary>
        /// T7 per (revision I-1): los archivos del Plugin Y de la UI cuyo CODIGO menciona un simbolo, por su ruta desde la raiz. La pagina
        /// «Perfil estructural» de la ventana RackCad es una segunda superficie del mismo inspector: una copia en la UI seria la misma
        /// deriva que esta guarda impide en el Plugin.
        /// </summary>
        private static string[] ProductFilesMentioning(string symbol) =>
            new[] { PluginDirectory, Path.Combine(RepoRoot().FullName, "src", "RackCad.UI") }
                .SelectMany(directory => Directory.GetFiles(directory, "*.cs", SearchOption.AllDirectories))
                .Where(path => !path.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar))
                .Where(path => !path.Contains(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar))
                .Where(path => CodeOnly(File.ReadAllText(path)).Contains(symbol, StringComparison.Ordinal))
                .Select(path => Path.GetRelativePath(RepoRoot().FullName, path).Replace(Path.DirectorySeparatorChar, '/'))
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

            // T7 per (I-64, septima tanda; el Owner: «Abren la ventana»): RACKSECCION abre la pagina «Perfil estructural» de la ventana RackCad, y
            // la pagina dibuja por RACKSECCIONINSERTAR con la materializacion del MISMO flujo (Place). Reapuntada, no debilitada: sigue habiendo un
            // solo flujo; el menu lo corre entero, la puerta de la ventana corre su materializacion, y RACKSECCION no reimplementa nada.
            Assert.Contains("StructuralSectionCommandFlow.Place(", CodeOnly(ReadPlugin("RackSeccionCommands.Ventana.cs")), StringComparison.Ordinal);
            Assert.DoesNotContain("StructuralSectionCommandFlow", CodeOnly(Command), StringComparison.Ordinal);
        }

        [Theory]
        [InlineData("StructuralSectionInspectorWindow", "src/RackCad.Plugin/StructuralSectionCommandFlow.cs;src/RackCad.UI/StructuralSections/StructuralSectionInspectorWindow.cs")] // el inspector
        [InlineData("StructuralSectionInsertService", "src/RackCad.Plugin/StructuralSectionCommandFlow.cs")] // la insercion
        [InlineData("WeightInPounds", "src/RackCad.Plugin/StructuralSectionCommandFlow.cs;src/RackCad.UI/StructuralSections/StructuralSectionInspectorState.cs")] // el calculo de peso
        [InlineData("SectionFidelity", "src/RackCad.Plugin/StructuralSectionCommandFlow.cs;src/RackCad.UI/StructuralSections/StructuralSectionInspectorState.cs")] // la fidelidad
        [InlineData("CsvStructuralSectionCatalogProvider", "")] // la carga: StructuralSectionCatalogSource, en Application
        [InlineData("StructuralSectionCatalogException", "")]   // el fallo cerrado: tambien alli
        public void ExactlyOnePlaceInThePluginOwnsEachPieceOfTheFlow(string symbol, string owners)
        {
            // I-37D: la CARGA del catalogo y su fallo cerrado dejaron de ser exclusivos del flujo de secciones,
            // porque la linea Cantilever tambien se construye sobre el catalogo neutral. La respuesta fue EXTRAER
            // el duenno —StructuralSectionCatalogAccess— y RE-APUNTAR la guarda, no debilitarla: sigue habiendo
            // exactamente un sitio por pieza, y copiar la carga en un segundo archivo seguiria fallando aqui.
            //
            // T7 per (revision I-1): la guarda mira tambien la UI, donde la pagina «Perfil estructural» es la segunda superficie del
            // inspector. En la UI, el inspector, el peso y la fidelidad viven solo en el inspector clasico (su ventana y su estado, que la
            // pagina usa tal cual). La carga y su fallo cerrado salieron del Plugin y de la UI a UN sitio de Application,
            // StructuralSectionCatalogSource, que usan StructuralSectionCatalogAccess, la pagina, «Larguero», Cantilever y el editor clasico de
            // Cantilever: ningun archivo del Plugin ni de la UI lee el catalogo ni decide su fallo (StructuralSectionDoorTests vigila el sitio
            // de Application).
            var files = ProductFilesMentioning(symbol);

            Assert.Equal(owners.Split(';', StringSplitOptions.RemoveEmptyEntries), files);
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
            // T7 per (el Owner: «Abren la ventana»): el camino directo lleva a la pagina «Perfil estructural» de la ventana RackCad, por el
            // patron de sus atajos; sigue sin modal y sin flujo propio.
            var command = CodeOnly(Command);

            Assert.Contains("[CommandMethod(\"RACKSECCION\", RackCadWindowShortcuts.WindowFlags)]", command, StringComparison.Ordinal);
            Assert.Contains("RackCadWindowShortcuts.Open(document, new RackCadShortcutAsk(RackCadShortcutTarget.StructuralSection, null, null))",
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

            // T7 per (revision I-1): la CAPTURA y sus palabras se mudaron al sitio unico de Application (StructuralSectionCatalogSource), que
            // usan el Plugin y la ventana. Reapuntada, no debilitada: el sitio unico captura el catalogo invalido con las palabras de siempre,
            // y el duenno del Plugin devuelve false con ellas.
            var source = File.ReadAllText(Path.Combine(RepoRoot().FullName, "src", "RackCad.Application", "StructuralSections", "StructuralSectionCatalogSource.cs"));
            var handler = source.IndexOf("catch (StructuralSectionCatalogException", StringComparison.Ordinal);

            Assert.True(handler > 0, "El sitio unico de la carga debe capturar StructuralSectionCatalogException.");
            Assert.Contains("StructuralSectionCatalogFailure.Invalid", source.Substring(handler, Math.Min(400, source.Length - handler)), StringComparison.Ordinal);
            Assert.Contains("no es valido, asi que no se dibujara nada", source, StringComparison.Ordinal);

            var access = CodeOnly(ReadPlugin("StructuralSectionCatalogAccess.cs"));
            Assert.Contains("StructuralSectionCatalogSource.TryLoad()", access, StringComparison.Ordinal);
            Assert.Contains("editor?.WriteMessage(\"\\n\" + load.CommandLineText);", access, StringComparison.Ordinal);
            Assert.Contains("return false;", access, StringComparison.Ordinal);
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
            // I-64 A-14: the rack switch of the menu lives in DispatchInsertion, called after this branch.
            var rackSwitch = menu.IndexOf("DispatchInsertion(document, menu.InsertionRequest)", StringComparison.Ordinal);

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
