using System;
using System.IO;
using System.Linq;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-47 gate G9 — la frontera transaccional de los DOS writers del slice, fijada como fuente.
    ///
    /// <para>
    /// Hoy los dos caminos de redibujo abren su propia transaccion y la commitean por bloque. Un lote sobre N
    /// racks que falle en el rack <c>k</c> deja <c>k-1</c> ya commiteados, y el dibujo queda con dos valores
    /// para la misma variable — exactamente el estado que "una sola operacion" existe para impedir. Anadir un
    /// preflight no lo arregla: el preflight evita empezar mal, no deshace lo ya commiteado.
    /// </para>
    /// <para>
    /// Lo que se fija aqui son propiedades de ORDEN y PROPIEDAD que solo existen en el codigo: quien abre la
    /// transaccion, quien la commitea, quien regenera, y que la preparacion —que puede importar bloques—
    /// ocurre FUERA. Ninguna suite puede cargar el Plugin (ADR-0003) y el CI no tiene AutoCAD, asi que estas
    /// son guardas de fuente, y lo son por la misma razon que las que el repositorio ya tiene sobre el Plugin.
    /// </para>
    /// <para>
    /// <b>No sustituyen a la validacion real.</b> Que la frontera FUNCIONE bajo una transaccion ajena solo lo
    /// demuestra AutoCAD, y eso es developer smoke del dueño. Estas guardas demuestran que la forma es la
    /// correcta; no que el dibujo salga bien.
    /// </para>
    /// </summary>
    public class CallerOwnedTransactionGuardTests
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

        private static string PluginSource(params string[] relative)
            => File.ReadAllText(Path.Combine(
                new[] { RepoRoot().FullName, "src", "RackCad.Plugin" }.Concat(relative).ToArray()));

        private static string Writer => PluginSource("Systems", "Shared", "SystemBlockWriter.cs");

        private static string Lateral => PluginSource("Drawing", "LateralHeaderDrawService.cs");

        /// <summary>The body of a method, by brace matching from its signature.</summary>
        private static string Body(string source, string signature)
        {
            var at = source.IndexOf(signature, StringComparison.Ordinal);
            Assert.True(at >= 0, "no se encontró la firma: " + signature);

            var open = source.IndexOf('{', at);
            Assert.True(open >= 0);

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

            throw new InvalidOperationException("cuerpo sin cerrar: " + signature);
        }

        // ================================================================ MUTATE

        [Fact]
        public void MUTATE_LaPrimitivaCallerOwnedEXISTE()
        {
            Assert.Contains("RedefineInTransaction", Writer);
        }

        [Fact]
        public void MUTATE_NO_COMMITEA()
        {
            Assert.DoesNotContain("Commit()", Body(Writer, "internal static LateralHeaderDrawOutcome RedefineInTransaction"));
        }

        [Fact]
        public void MUTATE_NO_REGENERA()
        {
            var body = Body(Writer, "internal static LateralHeaderDrawOutcome RedefineInTransaction");

            Assert.DoesNotContain("Regen", body);
        }

        [Fact]
        public void MUTATE_NO_ABRE_UNA_TRANSACCION_ANIDADA_NI_BLOQUEA_EL_DOCUMENTO()
        {
            var body = Body(Writer, "internal static LateralHeaderDrawOutcome RedefineInTransaction");

            Assert.DoesNotContain("StartTransaction", body);
            Assert.DoesNotContain("LockDocument", body);
        }

        /// <summary>
        /// La regla que hace posible la transaccion unica: dentro de MUTATE no se importa nada. Con la
        /// biblioteca ausente <c>EnsureBlocks</c> devuelve 0 en silencio, asi que apoyarse en ella dentro de la
        /// mutacion convertiria una biblioteca ausente en geometria incompleta sin aviso.
        /// </summary>
        [Fact]
        public void MUTATE_NO_IMPORTA_BLOQUES()
        {
            var body = Body(Writer, "internal static LateralHeaderDrawOutcome RedefineInTransaction");

            Assert.DoesNotContain("EnsureForPlan", body);
            Assert.DoesNotContain("BlockLibraryImporter", body);
        }

        [Fact]
        public void MUTATE_RECIBE_LA_TRANSACCION_DEL_LLAMADOR()
        {
            var at = Writer.IndexOf("internal static LateralHeaderDrawOutcome RedefineInTransaction", StringComparison.Ordinal);
            Assert.True(at >= 0);

            var signature = Writer.Substring(at, Writer.IndexOf(')', at) - at + 1);

            Assert.Contains("Transaction transaction", signature);
            Assert.Contains("Database database", signature);
        }

        // ================================================================ una sola autoridad

        [Fact]
        public void LOS_DOS_WRITERS_DELEGAN_EN_LA_MISMA_PRIMITIVA()
        {
            Assert.Contains("RedefineInTransaction", Body(Writer, "internal static HeaderPlacementResult RedrawInPlace"));
            Assert.Contains("RedefineInTransaction", Body(Lateral, "public HeaderPlacementResult RedrawInPlace"));
        }

        /// <summary>
        /// La autoridad no se duplica: la escritura del payload y la redefinicion viven en UN sitio. Si cada
        /// writer las repitiera, arreglar uno dejaria el otro roto — que es exactamente como el lateral se
        /// quedo fuera de la frontera la primera vez que se dibujo.
        /// </summary>
        [Fact]
        public void LA_ESCRITURA_DEL_PAYLOAD_VIVE_EN_UN_SOLO_SITIO()
        {
            Assert.DoesNotContain("RackBlockData.Write", Body(Lateral, "public HeaderPlacementResult RedrawInPlace"));
            Assert.DoesNotContain("RedefineSystemBlock", Body(Lateral, "public HeaderPlacementResult RedrawInPlace"));
        }

        // ================================================================ PREPARE fuera, POST despues

        [Fact]
        public void PREPARE_LaImportacionSIGUE_OCURRIENDO_EN_LOS_WRAPPERS_HISTORICOS()
        {
            // El comportamiento de los comandos existentes NO cambia: siguen preparando antes de mutar.
            Assert.Contains("EnsureForPlan", Body(Writer, "internal static HeaderPlacementResult RedrawInPlace"));
            Assert.Contains("EnsureForPlan", Body(Lateral, "public HeaderPlacementResult RedrawInPlace"));
        }

        [Fact]
        public void PREPARE_OCURRE_ANTES_DE_ABRIR_LA_TRANSACCION()
        {
            foreach (var body in new[]
            {
                Body(Writer, "internal static HeaderPlacementResult RedrawInPlace"),
                Body(Lateral, "public HeaderPlacementResult RedrawInPlace"),
            })
            {
                var prepare = body.IndexOf("EnsureForPlan", StringComparison.Ordinal);
                var mutate = body.IndexOf("StartTransaction", StringComparison.Ordinal);

                Assert.True(prepare >= 0 && mutate >= 0);
                Assert.True(prepare < mutate, "la preparación debe ocurrir antes de abrir la transacción");
            }
        }

        [Fact]
        public void POST_LaPurgaOCURRE_DESPUES_DEL_COMMIT()
        {
            foreach (var body in new[]
            {
                Body(Writer, "internal static HeaderPlacementResult RedrawInPlace"),
                Body(Lateral, "public HeaderPlacementResult RedrawInPlace"),
            })
            {
                var commit = body.IndexOf("Commit()", StringComparison.Ordinal);
                var purge = body.IndexOf("PurgeAfterCommit", StringComparison.Ordinal);

                Assert.True(commit >= 0 && purge >= 0);
                Assert.True(commit < purge, "la purga debe ocurrir después del commit");
            }
        }

        [Fact]
        public void POST_ElRegenOCURRE_DESPUES_DE_LA_PURGA_Y_SIGUE_SIENDO_UNO()
        {
            foreach (var body in new[]
            {
                Body(Writer, "internal static HeaderPlacementResult RedrawInPlace"),
                Body(Lateral, "public HeaderPlacementResult RedrawInPlace"),
            })
            {
                var purge = body.IndexOf("PurgeAfterCommit", StringComparison.Ordinal);
                var regen = body.IndexOf("ApplyRegen", StringComparison.Ordinal);

                Assert.True(purge >= 0 && regen >= 0);
                Assert.True(purge < regen, "el regen va después de la purga");

                // Un solo regen por camino: el patrón de multi-vista sigue siendo regen: false + uno al final.
                Assert.Equal(1, body.Split(new[] { "ApplyRegen" }, StringSplitOptions.None).Length - 1);
            }
        }

        [Fact]
        public void POST_LaPurgaTIENE_UN_SOLO_PUNTO_DE_ENTRADA()
        {
            Assert.Contains("PurgeAfterCommit", Writer);
            Assert.DoesNotContain("LateralHeaderDrawer.PurgeUnreferenced", Body(Lateral, "public HeaderPlacementResult RedrawInPlace"));
        }

        // ================================================================ compatibilidad

        [Fact]
        public void LOS_WRAPPERS_HISTORICOS_SIGUEN_EXISTIENDO_CON_SU_FIRMA()
        {
            Assert.Contains("internal static HeaderPlacementResult RedrawInPlace(", Writer);
            Assert.Contains("public HeaderPlacementResult RedrawInPlace(Document document, ObjectId blockId", Lateral);
        }

        [Fact]
        public void LOS_WRAPPERS_SIGUEN_SIENDO_LOS_QUE_ABREN_LOCK_TRANSACCION_Y_COMMIT()
        {
            foreach (var body in new[]
            {
                Body(Writer, "internal static HeaderPlacementResult RedrawInPlace"),
                Body(Lateral, "public HeaderPlacementResult RedrawInPlace"),
            })
            {
                Assert.Contains("LockDocument", body);
                Assert.Contains("StartTransaction", body);
                Assert.Contains("Commit()", body);
            }
        }
    }
}
