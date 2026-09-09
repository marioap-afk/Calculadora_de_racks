using System;
using System.IO;
using System.Linq;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-47 gate G9.1 — la frontera caller-owned llega hasta ARRIBA, no solo al writer.
    ///
    /// <para>
    /// G9 creó la primitiva en <c>SystemBlockWriter</c>, pero el camino real de un rack Selectivo no entra
    /// por ahí: frontal y planta bajan por <c>SelectiveFrontalDrawService</c> / <c>SelectivePlantaDrawService</c>
    /// → <c>ViewBlockDraw</c> → el writer, y el lateral por su propia fachada. Las tres capas de arriba
    /// seguían abriendo y commiteando su transacción, así que una operación no podía alcanzar la primitiva
    /// desde arriba bajo una sola transacción — que es justo lo que la propagación necesita.
    /// </para>
    /// <para>
    /// Cada fachada conserva su wrapper histórico —los comandos existentes no cambian— y expone además una
    /// primitiva equivalente que NO toma el lock, NO abre transacción, NO commitea, NO regenera y NO importa.
    /// Y no puede volver a bajar por un wrapper self-owned: eso reintroduciría el commit interno por la
    /// puerta de atrás.
    /// </para>
    /// <para>
    /// Son guardas de FUENTE porque ninguna suite carga el Plugin (ADR-0003) y el CI no tiene AutoCAD.
    /// Fijan propiedad y orden. <b>No afirman que AutoCAD funcione</b>: eso es validación física, y se cobra
    /// cuando exista una invocación real.
    /// </para>
    /// </summary>
    public class CallerOwnedFacadeGuardTests
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

        private static string Source(params string[] relative)
            => File.ReadAllText(Path.Combine(
                new[] { RepoRoot().FullName, "src", "RackCad.Plugin" }.Concat(relative).ToArray()));

        /// <summary>The four authorized facades, by the file that holds them.</summary>
        public static TheoryData<string, string[]> Facades => new TheoryData<string, string[]>
        {
            { "ViewBlockDraw", new[] { "Systems", "Shared", "ViewBlockDraw.cs" } },
            { "SelectiveFrontalDrawService", new[] { "Systems", "Selective", "SelectiveFrontalDrawService.cs" } },
            { "SelectivePlantaDrawService", new[] { "Systems", "Selective", "SelectivePlantaDrawService.cs" } },
            { "LateralHeaderDrawService", new[] { "Drawing", "LateralHeaderDrawService.cs" } },
        };

        private static string Body(string source, string marker)
        {
            var at = source.IndexOf(marker, StringComparison.Ordinal);
            Assert.True(at >= 0, "no se encontró: " + marker);

            var open = source.IndexOf('{', at);
            var arrow = source.IndexOf("=>", at);

            // Expression-bodied member: everything up to the terminating semicolon.
            if (arrow >= 0 && (open < 0 || arrow < open))
            {
                return source.Substring(arrow, source.IndexOf(';', arrow) - arrow + 1);
            }

            var depth = 0;

            for (var i = open; i < source.Length; i++)
            {
                if (source[i] == '{')
                {
                    depth++;
                }
                else if (source[i] == '}')
                {
                    depth--;

                    if (depth == 0)
                    {
                        return source.Substring(open, i - open + 1);
                    }
                }
            }

            throw new InvalidOperationException("cuerpo sin cerrar: " + marker);
        }

        // ================================================================ cada fachada expone la primitiva

        [Theory]
        [MemberData(nameof(Facades))]
        public void CADA_FACHADA_EXPONE_UNA_PRIMITIVA_CALLER_OWNED(string facade, string[] path)
        {
            Assert.Contains("RedrawInTransaction", Source(path));
        }

        [Theory]
        [MemberData(nameof(Facades))]
        public void CADA_FACHADA_EXPONE_UN_SEAM_DE_PREPARACION(string facade, string[] path)
        {
            Assert.Contains("PrepareRedraw", Source(path));
        }

        // ================================================================ lo que la primitiva NO hace

        [Theory]
        [MemberData(nameof(Facades))]
        public void LA_PRIMITIVA_NO_COMMITEA(string facade, string[] path)
        {
            Assert.DoesNotContain("Commit()", Body(Source(path), "RedrawInTransaction("));
        }

        [Theory]
        [MemberData(nameof(Facades))]
        public void LA_PRIMITIVA_NO_ABRE_TRANSACCION(string facade, string[] path)
        {
            Assert.DoesNotContain("StartTransaction", Body(Source(path), "RedrawInTransaction("));
        }

        [Theory]
        [MemberData(nameof(Facades))]
        public void LA_PRIMITIVA_NO_BLOQUEA_EL_DOCUMENTO(string facade, string[] path)
        {
            Assert.DoesNotContain("LockDocument", Body(Source(path), "RedrawInTransaction("));
        }

        [Theory]
        [MemberData(nameof(Facades))]
        public void LA_PRIMITIVA_NO_REGENERA(string facade, string[] path)
        {
            Assert.DoesNotContain("Regen", Body(Source(path), "RedrawInTransaction("));
        }

        [Theory]
        [MemberData(nameof(Facades))]
        public void LA_PRIMITIVA_NO_IMPORTA_BLOQUES(string facade, string[] path)
        {
            var body = Body(Source(path), "RedrawInTransaction(");

            Assert.DoesNotContain("EnsureForPlan", body);
            Assert.DoesNotContain("BlockLibraryImporter", body);
        }

        /// <summary>
        /// La regla que impide reintroducir el commit por la puerta de atrás: la primitiva no puede volver a
        /// bajar por un wrapper que posea su propia transacción.
        /// </summary>
        [Theory]
        [MemberData(nameof(Facades))]
        public void LA_PRIMITIVA_NO_BAJA_POR_UN_WRAPPER_SELF_OWNED(string facade, string[] path)
        {
            var body = Body(Source(path), "RedrawInTransaction(");

            Assert.DoesNotContain("RedrawInPlace(", body);
        }

        // ================================================================ la preparación sí importa, y va fuera

        [Theory]
        [MemberData(nameof(Facades))]
        public void LA_PREPARACION_ES_QUIEN_IMPORTA(string facade, string[] path)
        {
            var body = Body(Source(path), "PrepareRedraw(");

            // O prepara de verdad, o delega en quien lo hace: en ningún caso lo hace la primitiva.
            Assert.True(
                body.Contains("EnsureForPlan") || body.Contains("PrepareRedraw") || body.Contains("Prepare("),
                facade + ": la preparación debe importar o delegar en quien importa");
        }

        [Theory]
        [MemberData(nameof(Facades))]
        public void LA_PREPARACION_NO_ABRE_LA_TRANSACCION_SEMANTICA(string facade, string[] path)
        {
            Assert.DoesNotContain("StartTransaction", Body(Source(path), "PrepareRedraw("));
        }

        // ================================================================ compatibilidad

        [Theory]
        [MemberData(nameof(Facades))]
        public void EL_WRAPPER_HISTORICO_SE_CONSERVA(string facade, string[] path)
        {
            Assert.Contains("RedrawInPlace(", Source(path));
        }

        /// <summary>
        /// Y sigue siendo el wrapper quien posee el ciclo completo, para que los comandos existentes no
        /// cambien de comportamiento. Se comprueba en las dos fachadas que lo poseen de verdad;
        /// <c>ViewBlockDraw</c> y las de selectivo delegan hacia abajo.
        /// </summary>
        [Fact]
        public void LOS_WRAPPERS_QUE_POSEEN_EL_CICLO_LO_SIGUEN_POSEYENDO()
        {
            var lateral = Body(Source("Drawing", "LateralHeaderDrawService.cs"), "public HeaderPlacementResult RedrawInPlace(");
            var writer = Body(Source("Systems", "Shared", "SystemBlockWriter.cs"), "internal static HeaderPlacementResult RedrawInPlace(");

            foreach (var body in new[] { lateral, writer })
            {
                Assert.Contains("LockDocument", body);
                Assert.Contains("StartTransaction", body);
                Assert.Contains("Commit()", body);
            }
        }

        [Fact]
        public void LAS_FACHADAS_DE_SELECTIVO_SIGUEN_DELEGANDO_EN_ViewBlockDraw()
        {
            foreach (var path in new[]
            {
                new[] { "Systems", "Selective", "SelectiveFrontalDrawService.cs" },
                new[] { "Systems", "Selective", "SelectivePlantaDrawService.cs" },
            })
            {
                Assert.Contains("ViewBlockDraw.RedrawInPlace", Source(path));
            }
        }
    }
}
