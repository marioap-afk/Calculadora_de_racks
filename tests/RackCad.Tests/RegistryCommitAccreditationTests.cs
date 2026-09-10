using System.Collections.Generic;
using System.IO;
using RackCad.Application.Persistence;
using RackCad.Application.ProjectVariables;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-48 gate G4B — la acreditacion del RE-READ del commit (Proposal V8 R-01).
    ///
    /// <para>
    /// El plan se decide contra el registro que habia cuando se abrio la ventana; el commit escribe contra el
    /// registro que hay AHORA. Son dos documentos distintos, asi que la acreditacion del primero no vale para
    /// el segundo: una acreditacion pertenece a la lectura que se le dio. Antes de este gate el executor
    /// releia y aplicaba el cambio directamente sobre el resultado crudo, de modo que toda la precondicion de
    /// identidad se exigia solo en la via de planificacion.
    /// </para>
    /// <para>
    /// El orden es <b>leer, acreditar esa lectura, aplicar sobre el documento acreditado, escribir</b>. Y lo
    /// que estas pruebas exigen no es que el commit «aborte»: abortar tambien lo produciria un fallo posterior.
    /// Exigen evidencia de que <c>ApplyTo</c> NO se alcanzo. La evidencia es doble:
    /// </para>
    /// <list type="number">
    /// <item>no hay producto: un documento cambiado solo puede salir de <c>ApplyTo</c>, y no hay ninguno;</item>
    /// <item>el producto que <c>ApplyTo</c> HABRIA dado sobre esa misma entrada es destructivo y demostrable,
    /// asi que la assertion anterior no es vacia.</item>
    /// </list>
    /// <para>
    /// Y la garantia estructural no es una costumbre de revision: <c>RegistryMutation.ApplyTo</c> es internal,
    /// asi que el Plugin no puede llamarlo. Que el executor no pase un documento crudo es un error de
    /// compilacion, no un descuido posible.
    /// </para>
    /// </summary>
    public class RegistryCommitAccreditationTests
    {
        private const string GuidA = "11111111-1111-1111-1111-111111111111";
        private const string GuidB = "22222222-2222-2222-2222-222222222222";

        private static VariableId Id(string guid) => VariableId.Parse(guid);

        private static ProjectVariableDocument Entry(string guid, string name, double value)
            => new ProjectVariableDocument
            {
                VariableId = guid,
                Name = name,
                Type = VariableType.Length.ToString(),
                Definition = new ProjectVariableDefinitionDocument { Kind = "literal", Value = value },
            };

        private static ProjectVariablesDocument Document(params ProjectVariableDocument[] entries)
        {
            var document = ProjectVariablesDocument.CreateNew();
            document.Variables = new List<ProjectVariableDocument>(entries);
            return document;
        }

        /// <summary>El registro que el store SI acepta y la identidad NO: un VariableId por duplicado.</summary>
        private static ProjectVariablesDocument Ambiguo()
            => Document(Entry(GuidA, "Primera", 11.0), Entry(GuidA, "Segunda", 22.0));

        // ================================================================ nada que hacer

        [Fact]
        public void UNA_MUTACION_NONE_NO_FUERZA_RELECTURA_NI_PRODUCE_DOCUMENTO()
        {
            // Sin lectura siquiera: una operacion que solo toca racks no puede adquirir un modo de fallo nuevo
            // ni obligar a una relectura que no necesita.
            var preparation = RegistryCommit.Prepare(RegistryMutation.None, null);

            Assert.Equal(RegistryCommitOutcome.Unchanged, preparation.Outcome);
            Assert.Null(preparation.Changed);
            Assert.Null(preparation.Error);
            Assert.False(preparation.IsReady);
            Assert.False(preparation.IsBlocked);
        }

        [Fact]
        public void UNA_MUTACION_NONE_SIGUE_SIENDO_NONE_CON_UN_REGISTRO_AMBIGUO_DELANTE()
        {
            // El registro es inadministrable, pero la operacion no lo toca: no se le inventa un bloqueo.
            var preparation = RegistryCommit.Prepare(
                RegistryMutation.None, ProjectVariablesReadResult.Readable(Ambiguo()));

            Assert.Equal(RegistryCommitOutcome.Unchanged, preparation.Outcome);
            Assert.Null(preparation.Changed);
        }

        // ================================================================ la via feliz

        [Fact]
        public void UN_RE_READ_ACREDITADO_PRODUCE_EL_DOCUMENTO_A_ESCRIBIR()
        {
            var lastRead = ProjectVariablesReadResult.Readable(Document(Entry(GuidA, "Holgura", 11.0)));

            var preparation = RegistryCommit.Prepare(
                RegistryMutation.ChangeValue(Id(GuidA), VariableDefinition.Literal(19.0)), lastRead);

            Assert.True(preparation.IsReady);
            Assert.NotNull(preparation.Changed);
            Assert.Equal(19.0, Assert.Single(preparation.Changed.Variables).Definition.Value);
        }

        [Fact]
        public void EL_DOCUMENTO_RELEIDO_NO_SE_MUTA_AL_PREPARAR_EL_COMMIT()
        {
            var document = Document(Entry(GuidA, "Holgura", 11.0));

            var preparation = RegistryCommit.Prepare(
                RegistryMutation.ChangeValue(Id(GuidA), VariableDefinition.Literal(19.0)),
                ProjectVariablesReadResult.Readable(document));

            Assert.True(preparation.IsReady);
            Assert.NotSame(document, preparation.Changed);

            // El original sigue intacto: si preparar mutara su entrada, media operacion estaria aplicada antes
            // de que nadie decidiera escribirla.
            Assert.Equal(11.0, document.Variables[0].Definition.Value);
        }

        [Fact]
        public void UN_REGISTRO_AUSENTE_SIGUE_ADMITIENDO_LA_PRIMERA_VARIABLE()
        {
            // El caso legacy C-1: dibujo sin registro. Ausente NO es ilegible.
            var variable = ProjectVariable.Create(
                Id(GuidB), "Holgura", VariableType.Length, VariableDefinition.Literal(6.0));

            var preparation = RegistryCommit.Prepare(
                RegistryMutation.Add(variable), ProjectVariablesReadResult.Absent());

            Assert.True(preparation.IsReady);
            Assert.Equal(GuidB, Assert.Single(preparation.Changed.Variables).VariableId);
        }

        // ================================================================ fail-closed ANTES de aplicar

        [Fact]
        public void UN_RE_READ_CON_VARIABLEID_DUPLICADO_BLOQUEA_Y_NO_PRODUCE_NINGUN_DOCUMENTO()
        {
            var preparation = RegistryCommit.Prepare(
                RegistryMutation.Remove(Id(GuidA)), ProjectVariablesReadResult.Readable(Ambiguo()));

            Assert.True(preparation.IsBlocked);
            Assert.Contains(GuidA, preparation.Error);

            // La evidencia de que ApplyTo no se alcanzo: no existe producto. Un documento cambiado solo puede
            // salir de ApplyTo.
            Assert.Null(preparation.Changed);
        }

        [Fact]
        public void EL_BLOQUEO_NO_ES_VACIO_APPLYTO_SOBRE_ESA_MISMA_ENTRADA_SI_DESTRUIRIA_LAS_DOS_ENTRADAS()
        {
            // Sin este oraculo, la prueba anterior no distinguiria «no se aplico» de «se aplico y no hacia
            // nada». Aqui se llama a ApplyTo DIRECTAMENTE, como lo hacia el executor antes de G4B, y se
            // demuestra que el resultado era destructivo: Remove sobre un id duplicado borra AMBAS entradas.
            var ambiguo = Ambiguo();
            var comoAntes = RegistryMutation.Remove(Id(GuidA)).ApplyTo(ambiguo);

            Assert.Equal(2, ambiguo.Variables.Count);
            Assert.Empty(comoAntes.Variables);

            // Y la via del commit no llega ahi.
            var preparation = RegistryCommit.Prepare(
                RegistryMutation.Remove(Id(GuidA)), ProjectVariablesReadResult.Readable(ambiguo));

            Assert.True(preparation.IsBlocked);
            Assert.Null(preparation.Changed);
        }

        [Fact]
        public void UN_RE_READ_PRESENTE_PERO_ILEGIBLE_BLOQUEA_Y_JAMAS_ES_UN_REGISTRO_VACIO()
        {
            // Tratarlo como vacio y escribir encima destruiria en silencio todo lo que ese registro tuviera.
            var preparation = RegistryCommit.Prepare(
                RegistryMutation.Remove(Id(GuidA)),
                ProjectVariablesReadResult.Unreadable("El registro no se puede leer."));

            Assert.True(preparation.IsBlocked);
            Assert.Null(preparation.Changed);
            Assert.Equal("El registro no se puede leer.", preparation.Error);
        }

        [Fact]
        public void UN_RE_READ_DE_MAJOR_INCOMPATIBLE_BLOQUEA()
        {
            var preparation = RegistryCommit.Prepare(
                RegistryMutation.ChangeValue(Id(GuidA), VariableDefinition.Literal(19.0)),
                ProjectVariablesReadResult.IncompatibleMajor("Version mayor no compatible."));

            Assert.True(preparation.IsBlocked);
            Assert.Null(preparation.Changed);
        }

        [Fact]
        public void UN_RE_READ_QUE_NO_SE_CONSULTO_BLOQUEA_EN_LUGAR_DE_ASUMIR_VACIO()
        {
            var preparation = RegistryCommit.Prepare(RegistryMutation.Remove(Id(GuidA)), null);

            Assert.True(preparation.IsBlocked);
            Assert.Null(preparation.Changed);
        }

        // ================================================================ la garantia es estructural

        [Fact]
        public void EL_EXECUTOR_NO_APLICA_LA_MUTACION_SOBRE_EL_DOCUMENTO_CRUDO_DEL_RE_READ()
        {
            // La garantia que hace falta no es de estilo: `lastRead.Document` no puede volver a ser el
            // argumento directo de ApplyTo. Se comprueba en la FUENTE porque el executor necesita AutoCAD y no
            // se puede ejercitar en esta suite.
            var source = File.ReadAllText(SourcePath("src/RackCad.Plugin/ProjectVariableMutationExecutor.cs"));

            Assert.DoesNotContain("ApplyTo(", source);
            Assert.DoesNotContain("lastRead.Document", source);

            // Y lo que si hace: pasa por el seam que acredita primero.
            Assert.Contains("RegistryCommit.Prepare(plan.RegistryMutation, lastRead)", source);
            Assert.Contains("commit.IsBlocked", source);
        }

        [Fact]
        public void APPLYTO_NO_ES_ALCANZABLE_DESDE_EL_PLUGIN()
        {
            // internal, y Application solo abre sus internals a RackCad.Tests. Que el Plugin no pueda aplicar
            // un cambio sobre un documento sin acreditar es un error de compilacion, no una costumbre.
            var source = File.ReadAllText(SourcePath("src/RackCad.Application/ProjectVariables/MutationPlan.cs"));
            var csproj = File.ReadAllText(SourcePath("src/RackCad.Application/RackCad.Application.csproj"));

            Assert.Contains("internal ProjectVariablesDocument ApplyTo(", source);
            Assert.DoesNotContain("public ProjectVariablesDocument ApplyTo(", source);
            Assert.DoesNotContain("RackCad.Plugin", csproj);
        }

        /// <summary>La raiz del repo desde el directorio de ejecucion de la suite.</summary>
        private static string SourcePath(string relative)
        {
            var directory = new DirectoryInfo(Directory.GetCurrentDirectory());

            while (directory != null && !Directory.Exists(Path.Combine(directory.FullName, "src")))
            {
                directory = directory.Parent;
            }

            Assert.NotNull(directory);
            var path = Path.Combine(directory.FullName, relative.Replace('/', Path.DirectorySeparatorChar));
            Assert.True(File.Exists(path), "No se encontro la fuente: " + path);
            return path;
        }
    }
}
