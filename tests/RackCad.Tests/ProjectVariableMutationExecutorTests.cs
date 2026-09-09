using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RackCad.Application.Persistence;
using RackCad.Application.ProjectVariables;
using RackCad.Domain.Systems.Selective;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-47 gate G11 — EJECUTAR un <see cref="MutationPlan"/> sobre el dibujo.
    ///
    /// <para>
    /// El ejecutor no decide nada: el plan ya dijo QUÉ tiene que cambiar y contra qué vistas. Su trabajo es
    /// escribirlo entero o no escribir nada. De ahí las dos mitades que se prueban aquí.
    /// </para>
    /// <para>
    /// La mitad PURA es el enlace destino↔dibujo. Un plan se construye contra una foto del dibujo, y entre
    /// esa foto y la escritura el dibujo pudo cambiar. Si una vista nombrada por el plan ya no está, la
    /// propagación sería parcial; si apareció una vista que el plan no nombra, esa vista se quedaría con el
    /// valor viejo y el rack quedaría divergente — que es exactamente el defecto que ID22A existe para
    /// impedir. Las dos cosas son motivo de cancelar, no de continuar con lo que sí se pueda.
    /// </para>
    /// <para>
    /// La mitad FÍSICA vive en el Plugin, que ninguna suite carga (ADR-0003) y que el CI no puede ejecutar sin
    /// AutoCAD, así que se fija con guardas de FUENTE. Fijan propiedad y orden — un solo Commit, un solo
    /// Regen, importar antes de la transacción, envolvente por hermano, cero replanificación. <b>No afirman
    /// que el dibujo quede bien</b>: eso es validación física y se cobra en G16.
    /// </para>
    /// </summary>
    public class ProjectVariableMutationExecutorTests
    {
        private const string RackA = "3f2b1c9e-6d4a-4f38-9b71-0c2a5e8d1f44";

        private static SelectivePalletDesignDocument Doc()
        {
            var design = new SelectivePalletDesign { VerticalClearance = 6.0 };
            var bay = new SelectiveBayDesign();
            bay.Levels.Add(new SelectiveCell
            {
                Pallet = new Tarima { Frente = 48, Alto = 50 },
                PalletCount = 1,
                BeamId = "BEAM_A",
                BeamPeralte = 4.5,
            });
            design.Bays.Add(bay);
            return SelectivePalletDesignDocument.From(design, RackA, "Rack A");
        }

        private static ProjectVariableScanEntry Vista(string definitionId)
            => ProjectVariableScanEntry.Selective(definitionId, RackA, Doc());

        private static DestinationBindingResult Bind(IReadOnlyList<ProjectVariableScanEntry> destinos, params string[] presentes)
            => MutationDestinationBinding.Bind(RackA, destinos, presentes);

        // ================================================================ el caso que sí enlaza

        [Fact]
        public void ENLAZA_CuandoElDibujoTieneEXACTAMENTE_LasVistasDelPlan()
        {
            var resultado = Bind(new[] { Vista("A1"), Vista("B2"), Vista("C3") }, "A1", "B2", "C3");

            Assert.True(resultado.IsBound);
            Assert.Null(resultado.Error);
        }

        [Fact]
        public void EL_ORDEN_NO_IMPORTA()
        {
            Assert.True(Bind(new[] { Vista("A1"), Vista("B2") }, "B2", "A1").IsBound);
        }

        /// <summary>Los handles de AutoCAD son hexadecimal: comparar por mayúsculas sería una diferencia inventada.</summary>
        [Fact]
        public void LA_CAJA_DEL_IDENTIFICADOR_NO_IMPORTA()
        {
            Assert.True(Bind(new[] { Vista("2ab") }, "2AB").IsBound);
        }

        // ================================================================ una vista del plan que ya no está

        [Fact]
        public void UNA_VISTA_DEL_PLAN_QUE_DESAPARECIO_CANCELA()
        {
            var resultado = Bind(new[] { Vista("A1"), Vista("B2") }, "A1");

            Assert.False(resultado.IsBound);
            Assert.Contains("B2", resultado.Error);
        }

        [Fact]
        public void EL_DIAGNOSTICO_NOMBRA_EL_RACK()
        {
            Assert.Contains(RackA, Bind(new[] { Vista("A1") }, "Z9").Error);
        }

        // ================================================================ una vista que el plan NO nombra

        /// <summary>
        /// La asimetría importa: una vista de más no es "una vista de menos al revés". Es un hermano que el
        /// plan no va a reescribir y que, por tanto, se quedaría mostrando el valor anterior.
        /// </summary>
        [Fact]
        public void UNA_VISTA_QUE_EL_PLAN_NO_NOMBRA_CANCELA()
        {
            var resultado = Bind(new[] { Vista("A1") }, "A1", "SORPRESA");

            Assert.False(resultado.IsBound);
            Assert.Contains("SORPRESA", resultado.Error);
        }

        [Fact]
        public void UNA_VISTA_DE_MAS_CANCELA_AUNQUE_TODAS_LAS_DEL_PLAN_ESTEN()
        {
            Assert.False(Bind(new[] { Vista("A1"), Vista("B2") }, "A1", "B2", "C3").IsBound);
        }

        // ================================================================ un plan que no puede ejecutarse

        [Fact]
        public void UN_RACK_SIN_NINGUNA_VISTA_DESTINO_CANCELA()
        {
            Assert.False(Bind(new ProjectVariableScanEntry[0], "A1").IsBound);
            Assert.False(Bind(null, "A1").IsBound);
        }

        [Fact]
        public void UN_DESTINO_SIN_IDENTIFICADOR_CANCELA()
        {
            Assert.False(Bind(new[] { ProjectVariableScanEntry.Selective(null, RackA, Doc()) }, "A1").IsBound);
            Assert.False(Bind(new[] { ProjectVariableScanEntry.Selective("   ", RackA, Doc()) }, "A1").IsBound);
        }

        [Fact]
        public void UN_DESTINO_NULO_CANCELA()
        {
            Assert.False(Bind(new ProjectVariableScanEntry[] { null }, "A1").IsBound);
        }

        /// <summary>Escribir dos veces la misma definición no es redundante: es escribirla una vez de más.</summary>
        [Fact]
        public void UN_DESTINO_REPETIDO_CANCELA()
        {
            Assert.False(Bind(new[] { Vista("A1"), Vista("A1") }, "A1").IsBound);
        }

        [Fact]
        public void SIN_NINGUNA_VISTA_EN_EL_DIBUJO_CANCELA()
        {
            Assert.False(Bind(new[] { Vista("A1") }).IsBound);
            Assert.False(MutationDestinationBinding.Bind(RackA, new[] { Vista("A1") }, null).IsBound);
        }

        /// <summary>
        /// Una definición que el dibujo no sabe nombrar no se puede demostrar ajena: cancelar es lo único
        /// honesto, igual que en el barrido (C4.7-1).
        /// </summary>
        [Fact]
        public void UNA_DEFINICION_PRESENTE_SIN_NOMBRE_CANCELA()
        {
            Assert.False(Bind(new[] { Vista("A1") }, "A1", null).IsBound);
            Assert.False(Bind(new[] { Vista("A1") }, "A1", "  ").IsBound);
        }

        // ================================================================ guardas de fuente del ejecutor

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

        private static string Executor()
            => File.ReadAllText(Path.Combine(
                RepoRoot().FullName, "src", "RackCad.Plugin", "ProjectVariableMutationExecutor.cs"));

        private static int Count(string source, string token)
        {
            var total = 0;
            var at = source.IndexOf(token, StringComparison.Ordinal);

            while (at >= 0)
            {
                total++;
                at = source.IndexOf(token, at + token.Length, StringComparison.Ordinal);
            }

            return total;
        }

        private static int At(string source, string token)
        {
            var at = source.IndexOf(token, StringComparison.Ordinal);
            Assert.True(at >= 0, "no se encontró: " + token);
            return at;
        }

        /// <summary>
        /// G11 es un SERVICIO permanente, no una superficie de usuario. Un comando aquí sería o bien producto
        /// —y eso es G16— o bien infraestructura descartable.
        /// </summary>
        [Fact]
        public void EL_EJECUTOR_NO_ES_UN_COMANDO()
        {
            Assert.DoesNotContain("CommandMethod", Executor());
        }

        [Fact]
        public void EL_EJECUTOR_CONSUME_UN_MutationPlan()
        {
            Assert.Contains("MutationPlan plan", Executor());
        }

        // ---------------------------------------------------------------- un solo Commit, un solo Regen

        [Fact]
        public void HAY_EXACTAMENTE_UN_COMMIT()
        {
            Assert.Equal(1, Count(Executor(), "Commit()"));
        }

        [Fact]
        public void HAY_EXACTAMENTE_UNA_TRANSACCION()
        {
            Assert.Equal(1, Count(Executor(), "StartTransaction()"));
        }

        [Fact]
        public void HAY_EXACTAMENTE_UN_REGEN()
        {
            var source = Executor();

            Assert.Equal(1, Count(source, "ApplyRegen"));
            Assert.DoesNotContain("Editor.Regen()", source);
        }

        /// <summary>Una operación que solo toca el registro no redibuja nada, así que tampoco regenera.</summary>
        [Fact]
        public void EL_REGEN_ESTA_CONDICIONADO_A_QUE_HAYA_VISTAS()
        {
            Assert.Contains("ApplyRegen(document, prepared.Count > 0)", Executor());
        }

        // ---------------------------------------------------------------- PREPARE fuera, MUTATE dentro

        [Fact]
        public void PREPARAR_OCURRE_ANTES_DE_ABRIR_LA_TRANSACCION()
        {
            var source = Executor();

            Assert.True(At(source, "PrepareRedraw(") < At(source, "StartTransaction()"));
        }

        [Fact]
        public void EL_EJECUTOR_NO_IMPORTA_BLOQUES_POR_SU_CUENTA()
        {
            var source = Executor();

            Assert.DoesNotContain("EnsureForPlan", source);
            Assert.DoesNotContain("BlockLibraryImporter", source);
        }

        [Fact]
        public void EL_PURGE_OCURRE_DESPUES_DEL_COMMIT()
        {
            var source = Executor();

            Assert.True(At(source, "Commit()") < At(source, "PurgeAfterCommit"));
        }

        // ---------------------------------------------------------------- la envolvente es POR HERMANO

        /// <summary>
        /// El diseño se serializa UNA vez por rack —las vistas llevan el mismo JSON—, pero la envolvente NO:
        /// cada hermano conserva la suya, con sus campos desconocidos y su versión (I-11). Copiar una sola
        /// envolvente a todas las vistas destruiría exactamente eso.
        /// </summary>
        [Fact]
        public void LA_ENVOLVENTE_SE_COMPONE_CON_LA_DEL_PROPIO_HERMANO()
        {
            var source = Executor();

            Assert.Equal(1, Count(source, "RackEmbedComposer.Compose("));

            var at = At(source, "RackEmbedComposer.Compose(");
            var call = source.Substring(at, Math.Min(200, source.Length - at));

            Assert.Contains("view.Embed", call);
        }

        [Fact]
        public void EL_DISENO_SE_SERIALIZA_UNA_VEZ_POR_RACK()
        {
            Assert.Equal(1, Count(Executor(), "Serialize(rack.AuthoredOutput)"));
        }

        // ---------------------------------------------------------------- cero replanificación

        /// <summary>
        /// El ejecutor no vuelve a decidir. Si recalculara el efectivo, el alcance o el preflight, el plan
        /// dejaría de ser lo que se aprobó y "cero mutación en fallo" dejaría de ser demostrable.
        /// </summary>
        [Fact]
        public void EL_EJECUTOR_NO_REPLANIFICA()
        {
            var source = Executor();

            Assert.DoesNotContain("ProjectVariableMutationPreflight", source);
            Assert.DoesNotContain("ProjectVariableConsumerDiscovery", source);
            Assert.DoesNotContain("SelectiveEffectiveDesignResolver", source);
            Assert.DoesNotContain("SelectiveAuthoredAuthority", source);
        }

        [Fact]
        public void EL_ENLACE_DESTINO_LO_DECIDE_LA_CAPA_PURA()
        {
            Assert.Contains("MutationDestinationBinding.Bind(", Executor());
        }

        // ---------------------------------------------------------------- el registro pasa por su guarda

        [Fact]
        public void EL_REGISTRO_SE_ESCRIBE_POR_SU_GUARDA_Y_ANTES_DEL_COMMIT()
        {
            var source = Executor();

            Assert.Contains("ProjectVariablesRegistry.TryWrite", source);
            Assert.True(At(source, "ProjectVariablesRegistry.TryWrite") < At(source, "Commit()"));
        }

        // ---------------------------------------------------------------- el plan sigue sin ser físico

        /// <summary>
        /// La promesa que hace demostrable "cero mutación en fallo": el plan describe, no toca. Si un
        /// <c>ObjectId</c> entrara en la capa pura, la mitad del contrato dejaría de poder probarse aquí.
        /// </summary>
        [Fact]
        public void LA_CAPA_PURA_SIGUE_SIN_CONOCER_EL_DIBUJO()
        {
            var carpeta = Path.Combine(RepoRoot().FullName, "src", "RackCad.Application", "ProjectVariables");

            foreach (var file in Directory.GetFiles(carpeta, "*.cs"))
            {
                // Sin los comentarios: la prohibición es sobre el CÓDIGO. Nombrar lo que no se usa —«no lleva
                // ObjectId»— es justamente la documentación de esta regla.
                var code = string.Join(
                    "\n",
                    File.ReadAllLines(file).Where(line => !line.TrimStart().StartsWith("//", StringComparison.Ordinal)));

                Assert.DoesNotContain("ObjectId", code);
                Assert.DoesNotContain("Autodesk.AutoCAD", code);
            }
        }
    }
}
