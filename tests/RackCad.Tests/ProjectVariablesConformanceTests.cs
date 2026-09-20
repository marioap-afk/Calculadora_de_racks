using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using RackCad.Application.Expressions;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-47 gate G18 — las garantías TRANSVERSALES del contrato, las que no son de ningún gate.
    ///
    /// <para>
    /// Cada gate fijó lo suyo sobre los archivos que tocaba. Quedan cuatro promesas que no viven en un
    /// archivo sino en la AUSENCIA de algo en todo el repositorio, y una ausencia solo se puede fijar
    /// buscándola entera: si se comprueba sobre los archivos que uno recuerda, el día que aparezca en otro
    /// nadie se entera. Eso es exactamente lo que estas guardas impiden.
    /// </para>
    /// <para>
    /// No repiten lo que ya está probado. Son las cuatro cosas que la auditoría de conformidad de G18
    /// verificó a mano y que, sin esto, habría que volver a verificar a mano cada vez.
    /// </para>
    /// </summary>
    public class ProjectVariablesConformanceTests
    {
        private static DirectoryInfo RepoRoot()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);

            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "RackCad.sln")))
            {
                dir = dir.Parent;
            }

            Assert.NotNull(dir);
            return dir;
        }

        /// <summary>Every production C# file of a project, excluding generated output.</summary>
        private static IReadOnlyList<string> Sources(string project)
        {
            var root = Path.Combine(RepoRoot().FullName, "src", project);

            return Directory
                .GetFiles(root, "*.cs", SearchOption.AllDirectories)
                .Where(path => !path.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar,
                                   StringComparison.Ordinal)
                            && !path.Contains(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar,
                                   StringComparison.Ordinal))
                .ToList();
        }

        /// <summary>The file's CODE — comment lines removed, because a prohibition is about what runs.</summary>
        private static string Code(string path)
            => string.Join(
                "\n",
                File.ReadAllLines(path).Where(line => !line.TrimStart().StartsWith("//", StringComparison.Ordinal)));

        private static void AssertAbsentFrom(string project, params string[] forbidden)
        {
            foreach (var path in Sources(project))
            {
                var code = Code(path);

                foreach (var token in forbidden)
                {
                    Assert.False(
                        code.Contains(token, StringComparison.Ordinal),
                        Path.GetFileName(path) + " contiene '" + token + "'");
                }
            }
        }

        // ================================================================ 0: la barrida mira de verdad

        /// <summary>
        /// La primera guarda es sobre las demás. Una comprobación de AUSENCIA pasa sola si el barrido no
        /// encuentra nada que mirar —una ruta mal escrita, un filtro de más—, y entonces las cinco de abajo
        /// dirían que sí a cualquier cosa. Así que aquí se comprueba que el barrido ve los archivos, que
        /// excluye lo generado, y que ENCUENTRA algo que sabemos que está.
        /// </summary>
        [Fact]
        public void LA_BARRIDA_MIRA_DE_VERDAD_Y_NO_LO_GENERADO()
        {
            foreach (var project in new[] { "RackCad.Application", "RackCad.Plugin", "RackCad.UI", "RackCad.Domain" })
            {
                var sources = Sources(project);

                Assert.True(sources.Count > 10, project + ": el barrido apenas encontró archivos");
                Assert.DoesNotContain(sources, path => path.Contains(
                    Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar, StringComparison.Ordinal));
            }

            // Y encuentra lo que tiene que encontrar: si esto fallara, las ausencias de abajo no valdrían nada.
            Assert.Contains(Sources("RackCad.Plugin"), path => Code(path).Contains("Autodesk.AutoCAD", StringComparison.Ordinal));
            Assert.Contains(Sources("RackCad.Application"), path => Code(path).Contains("PropertyValues", StringComparison.Ordinal));
        }

        // ================================================================ 29: la UI no conoce AutoCAD

        /// <summary>
        /// ADR-0006, comprobado sobre TODA la assembly y no sobre los archivos que uno recuerda. El proyecto
        /// no referencia AutoCAD, así que hoy no compilaría; la guarda es lo que hace que siga sin poder.
        /// </summary>
        [Fact]
        public void LA_UI_ENTERA_SIGUE_SIN_CONOCER_AUTOCAD()
        {
            AssertAbsentFrom("RackCad.UI", "Autodesk.AutoCAD");
        }

        /// <summary>Ni el dominio: una variable de proyecto es persistencia, no geometría.</summary>
        [Fact]
        public void EL_DOMINIO_ENTERO_SIGUE_SIN_CONOCER_LAS_VARIABLES()
        {
            AssertAbsentFrom("RackCad.Domain", "ProjectVariables", "VariableId", "PropertyValues");
        }

        /// <summary>
        /// I-49 G5 (V6 P28.4, guarda G4: «Domain sin tokens del núcleo»). La guarda de arriba no cambia; esta añade
        /// que el dominio tampoco nombra el núcleo de expresiones: ni su namespace ni ninguno de sus tipos. La lista
        /// sale del ensamblado compilado, así que cada tipo nuevo del núcleo queda cubierto sin tocar esta prueba.
        ///
        /// <para>
        /// Un nombre de tipo se busca como IDENTIFICADOR completo y no como subcadena: <c>Number</c> dentro de
        /// <c>LevelNumber</c> no es el tipo del núcleo, y una coincidencia parcial empujaría a renombrar tipos para
        /// esquivar la guarda, que es justo lo que P28.4 prohíbe. El namespace sí se busca como texto.
        /// </para>
        /// </summary>
        [Fact]
        public void EL_DOMINIO_ENTERO_TAMPOCO_CONOCE_EL_NUCLEO_DE_EXPRESIONES()
        {
            const string nucleo = "RackCad.Application.Expressions";

            var tiposDelNucleo = typeof(ExpressionParser).Assembly
                .GetTypes()
                .Where(type => !type.IsNested
                               && type.Namespace != null
                               && (type.Namespace == nucleo || type.Namespace.StartsWith(nucleo + ".", StringComparison.Ordinal))
                               && !type.IsDefined(typeof(CompilerGeneratedAttribute), false))
                .Select(type => type.Name.Split('`')[0])
                .Distinct(StringComparer.Ordinal)
                .ToList();

            Assert.Contains("ExpressionParser", tiposDelNucleo);

            var identificadores = tiposDelNucleo
                .Select(name => new Regex("(?<![A-Za-z0-9_])" + Regex.Escape(name) + "(?![A-Za-z0-9_])", RegexOptions.CultureInvariant))
                .ToList();

            // El buscador ve lo que tiene que ver: el núcleo se nombra a sí mismo en Application.
            Assert.Contains(Sources("RackCad.Application"), path => identificadores.Any(pattern => pattern.IsMatch(Code(path))));

            foreach (var path in Sources("RackCad.Domain"))
            {
                var code = Code(path);

                Assert.False(code.Contains(nucleo, StringComparison.Ordinal), Path.GetFileName(path) + " contiene '" + nucleo + "'");

                foreach (var pattern in identificadores)
                {
                    Assert.False(pattern.IsMatch(code), Path.GetFileName(path) + " nombra el tipo del núcleo " + pattern);
                }
            }
        }

        // ================================================================ 30: ni fórmulas ni ID21

        /// <summary>
        /// ID22A implementó UN caso: literal. Ni fórmulas, ni parser, ni AST, ni grafo de dependencias, ni
        /// referencias de una propiedad a la propiedad de otro rack (ID21). Todo eso estaba DISEÑADO para caber
        /// después sin romper nada, y esta guarda fijaba que todavía no había entrado.
        ///
        /// <para>
        /// I-49 G5 la evoluciona exactamente como fija la Proposal V6 (P28.4, guarda G1), en el mismo gate que el
        /// comportamiento y sin renombrar nada para esquivarla: <c>ExpressionParser</c> pasa a permitirse SOLO en
        /// Application —el núcleo de expresiones y sus adaptadores— y sigue prohibido en Plugin y UI.
        /// <c>RackPropertyReference</c>, <c>rackProperty</c> y <c>FormulaParser</c> siguen prohibidos en las tres
        /// capas. I-49 G7 permite <c>DependencyGraph</c> solo en Application; Plugin y UI siguen sin poseer el grafo.
        /// </para>
        /// </summary>
        [Fact]
        public void NO_HAY_FORMULAS_NI_REFERENCIAS_A_PROPIEDADES_DE_OTROS_RACKS()
        {
            AssertAbsentFrom(
                "RackCad.Application",
                "FormulaParser",
                "RackPropertyReference",
                "rackProperty");

            foreach (var project in new[] { "RackCad.Plugin", "RackCad.UI" })
            {
                AssertAbsentFrom(
                    project,
                    "ExpressionParser",
                    "FormulaParser",
                    "DependencyGraph",
                    "RackPropertyReference",
                    "rackProperty");
            }
        }

        /// <summary>G8 abre exactamente Literal y Expression; Formula y cualquier tercer caso siguen prohibidos.</summary>
        [Fact]
        public void LA_DEFINICION_DE_UNA_VARIABLE_TIENE_SOLO_LITERAL_Y_EXPRESSION()
        {
            var definition = File.ReadAllText(Path.Combine(
                RepoRoot().FullName, "src", "RackCad.Application", "ProjectVariables", "VariableDefinition.cs"));

            Assert.Contains("Literal = 1", definition);
            Assert.Contains("Expression = 2", definition);
            Assert.DoesNotContain("Formula = ", definition);
        }

        // ================================================================ el mapa de vínculos, fuera de Application

        /// <summary>
        /// Escribir un vínculo es de G6 y G11. Que ningún archivo del Plugin ni de la UI nombre el mapa es lo
        /// que hace estructural la promesa: no hay una segunda forma de crear, mover o borrar un vínculo.
        /// </summary>
        [Fact]
        public void NADIE_FUERA_DE_APPLICATION_TOCA_EL_MAPA_DE_VINCULOS()
        {
            foreach (var project in new[] { "RackCad.Plugin", "RackCad.UI" })
            {
                AssertAbsentFrom(project, "PropertyValues", "SelectivePropertyValueDocument");
            }
        }

        // ================================================================ 17/18: ToDomain no dibuja lo vinculado

        /// <summary>
        /// <c>ToDomain()</c> es, exactamente, el caso SIN vínculo: no consulta el registro, así que un rack
        /// vinculado que pasara por ahí se dibujaría con su literal congelado — el defecto que G12 y G13
        /// cerraron, uno en el editor y otro en el BOM.
        ///
        /// <para>
        /// La sobrecarga histórica de un argumento de <c>LoadExisting</c> sigue existiendo para la biblioteca
        /// y para las suites, y por eso llama a <c>ToDomain()</c>. Lo que esta guarda fija es que NINGÚN camino
        /// de producción la use: el editor entra por la sobrecarga que recibe el diseño ya resuelto.
        /// </para>
        /// </summary>
        [Fact]
        public void NINGUN_CAMINO_DE_PRODUCCION_ABRE_UN_SELECTIVO_POR_LA_SOBRECARGA_SIN_RESOLVER()
        {
            var callers = new List<string>();

            foreach (var project in new[] { "RackCad.Plugin", "RackCad.UI" })
            {
                foreach (var path in Sources(project))
                {
                    // La declaración de la propia sobrecarga no es una llamada.
                    if (Path.GetFileName(path) == "RackSelectiveWindow.xaml.cs")
                    {
                        continue;
                    }

                    foreach (var line in File.ReadAllLines(path))
                    {
                        if (line.Contains("LoadExisting(saved)", StringComparison.Ordinal) ||
                            line.Contains("LoadExisting(document)", StringComparison.Ordinal))
                        {
                            callers.Add(Path.GetFileName(path) + ": " + line.Trim());
                        }
                    }
                }
            }

            Assert.Empty(callers);
        }

        /// <summary>Y el camino real sigue entrando por la sobrecarga que trae el diseño EFECTIVO.</summary>
        [Fact]
        public void EL_CAMINO_REAL_ABRE_CON_EL_DISENO_YA_RESUELTO()
        {
            var commands = File.ReadAllText(Path.Combine(
                RepoRoot().FullName, "src", "RackCad.Plugin", "RackSelectivoCommands.cs"));

            Assert.Contains("LoadExisting(saved, open.Design, open.LinkedPropertyStates)", commands);
        }

        // ================================================================ la resolución sigue teniendo un dueño

        /// <summary>
        /// Una sola función convierte authored + registro en efectivo, en TODO el repositorio. Dos serían dos
        /// respuestas para el mismo número, que es el defecto original: el rack dibujado con la variable y
        /// cotizado con el literal.
        /// </summary>
        [Fact]
        public void SIGUE_HABIENDO_UN_SOLO_RESOLVEDOR_EFECTIVO()
        {
            var declaring = new[] { "RackCad.Application", "RackCad.Plugin", "RackCad.UI" }
                .SelectMany(Sources)
                .Where(path => File.ReadAllText(path).Contains("class SelectiveEffectiveDesignResolver", StringComparison.Ordinal))
                .ToList();

            Assert.Single(declaring);
        }
    }
}
