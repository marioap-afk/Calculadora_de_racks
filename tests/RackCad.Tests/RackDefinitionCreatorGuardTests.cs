using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// AUTH-15 — la primitiva caller-owned que crea UNA definicion RackCad con su sobre, fijada como fuente.
    ///
    /// <para>
    /// El contrato es de PROPIEDAD: la transaccion, el commit, el rollback, la colocacion de la referencia, el
    /// RackId, la transformacion, el orden y la semantica de lote pertenecen al llamador; AUTH-15 solo crea la
    /// definicion, escribe el sobre ya compuesto y lo verifica. Ninguna suite puede cargar el Plugin (ADR-0003) y
    /// el CI no tiene AutoCAD, asi que estas son guardas de fuente, igual que <see cref="CallerOwnedTransactionGuardTests"/>.
    /// </para>
    /// <para>
    /// <b>No sustituyen a la validacion en AutoCAD.</b> Que la definicion exista solo dentro de la transaccion
    /// ajena y desaparezca con su rollback lo demuestra el host; estas guardas demuestran que la forma es la
    /// correcta.
    /// </para>
    /// </summary>
    public class RackDefinitionCreatorGuardTests
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

        private static string Creator => PluginSource("Systems", "Shared", "RackDefinitionCreator.cs");

        private static string Result => PluginSource("Systems", "Shared", "RackDefinitionCreationResult.cs");

        /// <summary>The code only: comments (which legitimately say what the primitive does NOT do) are removed.</summary>
        private static string Code(string source)
            => string.Join("\n", source.Split('\n').Select(line =>
            {
                var comment = line.IndexOf("//", StringComparison.Ordinal);
                return comment >= 0 ? line.Substring(0, comment) : line;
            }));

        private static string CreatorCode => Code(Creator);

        /// <summary>The body of a method, by brace matching from the n-th occurrence of its signature.</summary>
        private static string Body(string source, string signature, int occurrence = 0)
        {
            var at = -1;
            for (var i = 0; i <= occurrence; i++)
            {
                at = source.IndexOf(signature, at + 1, StringComparison.Ordinal);
                Assert.True(at >= 0, "no se encontró la firma: " + signature + " #" + occurrence);
            }

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

        private const string Entry = "internal static RackDefinitionCreationResult CreateInTransaction(";

        // ================================================================ forma

        [Fact]
        public void AUTH15_EXISTEN_LAS_DOS_FAMILIAS_Y_RECIBEN_LA_TRANSACCION_DEL_LLAMADOR()
        {
            var entries = Regex.Matches(CreatorCode, Regex.Escape(Entry));
            Assert.Equal(2, entries.Count);

            foreach (Match entry in entries)
            {
                var signature = CreatorCode.Substring(entry.Index, CreatorCode.IndexOf(')', entry.Index) - entry.Index + 1);

                Assert.Contains("Database database", signature);
                Assert.Contains("Transaction transaction", signature);
                Assert.Contains("string requestedBlockName", signature);
                Assert.Contains("RackEmbedDocument envelope", signature);
            }

            Assert.Contains("HeaderRunPlan plan", CreatorCode);
            Assert.Contains("CantileverViewPlan plan", CreatorCode);
        }

        [Fact]
        public void AUTH15_UN_PLAN_POR_LLAMADA_SIN_LOTE_NI_ESTADO()
        {
            // One call creates one definition. No loop, no collection of plans, no field that could carry state
            // from one call to the next: batch and partial-batch semantics are the caller's.
            Assert.DoesNotContain("foreach", CreatorCode);
            Assert.DoesNotMatch(new Regex(@"\bfor\s*\("), CreatorCode);
            Assert.DoesNotMatch(new Regex(@"\bwhile\s*\("), CreatorCode);
            Assert.DoesNotContain("IEnumerable<", CreatorCode);
            Assert.DoesNotMatch(new Regex(@"\bstatic\s+(readonly\s+)?(?!RackDefinitionCreationResult)[\w<>\[\],\s]+\s+\w+\s*(=|;)"), CreatorCode);
        }

        // ================================================================ lo que NO hace

        [Fact]
        public void AUTH15_NO_COMMITEA_NI_ABORTA()
        {
            Assert.DoesNotContain("Commit(", CreatorCode);
            Assert.DoesNotContain("Abort(", CreatorCode);
        }

        [Fact]
        public void AUTH15_NO_ABRE_TRANSACCION_NI_BLOQUEA_EL_DOCUMENTO()
        {
            Assert.DoesNotContain("StartTransaction", CreatorCode);
            Assert.DoesNotContain("StartOpenCloseTransaction", CreatorCode);
            Assert.DoesNotContain("LockDocument", CreatorCode);
        }

        [Fact]
        public void AUTH15_NO_DISPONE_DE_LA_TRANSACCION_DEL_LLAMADOR()
        {
            // Disposing a transaction without committing it aborts it: the lifetime is the caller's, whole.
            foreach (var code in new[] { CreatorCode, Code(Result) })
            {
                Assert.DoesNotContain("Dispose(", code);
                Assert.DoesNotContain("using (", code);
                Assert.DoesNotContain("using var", code);
            }
        }

        [Fact]
        public void AUTH15_NO_REGENERA_NI_PURGA_NI_IMPORTA()
        {
            Assert.DoesNotContain("Regen", CreatorCode);
            Assert.DoesNotContain("Purge", CreatorCode);
            Assert.DoesNotContain("EnsureForPlan", CreatorCode);
            Assert.DoesNotContain("BlockLibraryImporter", CreatorCode);
            Assert.DoesNotContain("EnsureBlocks", CreatorCode);
        }

        [Fact]
        public void AUTH15_NO_COLOCA_REFERENCIAS()
        {
            Assert.DoesNotContain("BlockReference", CreatorCode);
            Assert.DoesNotContain("ModelSpace", CreatorCode);
            Assert.DoesNotContain("PaperSpace", CreatorCode);
            Assert.DoesNotContain("AppendEntity", CreatorCode);
            Assert.DoesNotContain("InsertReference", CreatorCode);
        }

        [Fact]
        public void AUTH15_NO_TOCA_IDENTIDAD_SOBRE_NI_TRANSFORMACION()
        {
            // The envelope arrives composed and restamped by the caller: validated and written, never altered.
            Assert.DoesNotContain("RackEnvelopeRestamp", CreatorCode);
            Assert.DoesNotContain("RackEmbedComposer", CreatorCode);
            Assert.DoesNotMatch(new Regex(@"envelope\.\w+\s*=[^=]"), CreatorCode);
            Assert.DoesNotContain("Matrix3d", CreatorCode);
            Assert.DoesNotContain("TransformBy", CreatorCode);
            Assert.DoesNotContain("Rotation", CreatorCode);
        }

        [Fact]
        public void AUTH15_NO_MUESTRA_UI()
        {
            Assert.DoesNotContain("Editor", CreatorCode);
            Assert.DoesNotContain("WriteMessage", CreatorCode);
            Assert.DoesNotContain("MessageBox", CreatorCode);
            Assert.DoesNotContain("ShowAlertDialog", CreatorCode);
        }

        [Fact]
        public void AUTH15_NO_UNIFICA_LAS_POLITICAS_DE_NOMBRE_DE_CADA_FAMILIA()
        {
            // I-09: each family keeps its own uniqueness policy; AUTH-15 delegates to the family creator.
            Assert.DoesNotContain("UniqueBlockName", CreatorCode);
            Assert.DoesNotContain("SanitizeBlockName", CreatorCode);
            Assert.Contains("drawer.CreateSystemBlock(", CreatorCode);
            Assert.Contains("CantileverViewMaterializer.CreateBlockDefinitionNamed(", CreatorCode);
        }

        // ================================================================ escritores delegados

        /// <summary>
        /// Every member AUTH-15 reaches to write, by file and EXACT signature (up to its opening parenthesis). The
        /// writes happen here, not in the creator, so the ownership boundary is guarded here too — by method body,
        /// never by file: the same files hold unrelated members that legitimately open and commit transactions
        /// (for example <c>LateralHeaderDrawer.PurgeUnreferenced</c>).
        /// </summary>
        private static readonly (string[] File, string[] Signatures)[] DelegatedWriters =
        {
            (new[] { "Drawing", "LateralHeaderDrawer.cs" }, new[]
            {
                "public LateralHeaderBlockResult CreateSystemBlock(",
                "private static BlockTableRecord NewBlock(",
                "private static bool AppendInstance(",
                "private static void AppendDimension(",
                "private static (ObjectId StyleId, bool IsNamed) ResolveDimStyle(",
                "private static ObjectId EnsureAnnotationLayer(",
                "private static void ApplyDynamicParameters(",
                "private static string UniqueBlockName(",
            }),
            (new[] { "Drawing", "Cantilever", "CantileverViewMaterializer.cs" }, new[]
            {
                "internal static ObjectId CreateBlockDefinitionNamed(",
                "private static void AppendCurves(",
                "private static void EnsureRoleLayers(",
                "private static string UniqueBlockName(",
                "private static string Sanitize(",
            }),
            (new[] { "LayerHelper.cs" }, new[]
            {
                "public static ObjectId EnsureLayer(",
            }),
            (new[] { "Systems", "Shared", "RackBlockData.cs" }, new[]
            {
                "public static void Write(",
                "public static string Read(",
            }),
        };

        private static readonly string[] ForbiddenInDelegatedWriters =
        {
            "Commit(", "Abort(", "StartTransaction", "StartOpenCloseTransaction", "LockDocument", "Regen", "Purge(",
            "EnsureForPlan", "BlockLibraryImporter", "EnsureBlocks", "GetBlockModelSpaceId", "ModelSpace", "PaperSpace",
            "CurrentSpaceId", "Editor", "InsertReference", "SystemBlockWriter.", "LateralHeaderDrawService.",
            "BlockPlacement.", "ViewBlockDraw.",
        };

        /// <summary>
        /// The body of the ONE member declared with <paramref name="signature"/>: a block body by brace matching, or
        /// an expression body from its <c>=&gt;</c> to its terminating <c>;</c>. A missing or repeated signature fails:
        /// a renamed or overloaded writer must come back through this guard, not slip past it.
        /// </summary>
        private static string MemberBody(string code, string signature, string file)
        {
            Assert.EndsWith("(", signature);

            var at = code.IndexOf(signature, StringComparison.Ordinal);
            Assert.True(at >= 0, "no se encontró el escritor delegado " + file + ": " + signature);
            Assert.True(code.IndexOf(signature, at + 1, StringComparison.Ordinal) < 0, "firma ambigua en " + file + ": " + signature);

            // Past the parameter list.
            var close = -1;
            for (int i = at + signature.Length - 1, parens = 0; i < code.Length; i++)
            {
                if (code[i] == '(')
                {
                    parens++;
                }
                else if (code[i] == ')' && --parens == 0)
                {
                    close = i;
                    break;
                }
            }

            Assert.True(close > at, "lista de parametros sin cerrar en " + file + ": " + signature);

            var arrow = code.IndexOf("=>", close, StringComparison.Ordinal);
            var open = code.IndexOf('{', close);

            if (arrow >= 0 && (open < 0 || arrow < open))
            {
                var end = code.IndexOf(';', arrow);
                Assert.True(end > arrow, "cuerpo de expresion sin cerrar en " + file + ": " + signature);
                return code.Substring(arrow, end - arrow + 1);
            }

            Assert.True(open > close, "sin cuerpo en " + file + ": " + signature);

            for (int i = open, depth = 0; i < code.Length; i++)
            {
                if (code[i] == '{')
                {
                    depth++;
                }
                else if (code[i] == '}' && --depth == 0)
                {
                    return code.Substring(open, i - open + 1);
                }
            }

            throw new InvalidOperationException("cuerpo sin cerrar en " + file + ": " + signature);
        }

        [Fact]
        public void AUTH15_LOS_ESCRITORES_DELEGADOS_RESPETAN_LA_FRONTERA()
        {
            foreach (var (file, signatures) in DelegatedWriters)
            {
                var code = Code(PluginSource(file));
                var name = string.Join("/", file);

                foreach (var signature in signatures)
                {
                    var body = MemberBody(code, signature, name);

                    foreach (var forbidden in ForbiddenInDelegatedWriters)
                    {
                        Assert.False(
                            body.Contains(forbidden, StringComparison.Ordinal),
                            name + " " + signature + " contiene «" + forbidden + "»: AUTH-15 delega en el y la frontera es del llamador");
                    }
                }
            }
        }

        [Fact]
        public void AUTH15_EL_LOCALIZADOR_DE_CUERPOS_DISTINGUE_BLOQUE_Y_EXPRESION()
        {
            // The locator itself, on a known shape: an expression body must not swallow the next member.
            const string sample = "class C {\n  int A(int x)\n    => x + 1;\n  void B(string s) { if (s != null) { Commit(); } }\n}";

            Assert.Equal("=> x + 1;", MemberBody(sample, "int A(", "muestra"));
            Assert.Equal("{ if (s != null) { Commit(); } }", MemberBody(sample, "void B(", "muestra"));
        }

        // ================================================================ fallo cerrado

        [Fact]
        public void AUTH15_LOS_RECHAZOS_SE_DECIDEN_ANTES_DE_ESCRIBIR()
        {
            for (var n = 0; n < 2; n++)
            {
                var body = Body(CreatorCode, Entry, n);
                var precheck = body.IndexOf("Precheck(", StringComparison.Ordinal);
                var write = new[] { "CreateSystemBlock(", "CreateBlockDefinitionNamed(" }
                    .Select(w => body.IndexOf(w, StringComparison.Ordinal)).Where(i => i >= 0).Min();

                Assert.True(precheck >= 0 && precheck < write, "Precheck debe preceder a la escritura en la familia #" + n);
            }
        }

        [Fact]
        public void AUTH15_EXIGE_LA_TRANSACCION_ACTIVA_DE_LA_BASE_DESTINO()
        {
            var precheck = Body(CreatorCode, "private static RackDefinitionCreationResult? Precheck(");

            // B-1 (Architect exact-SHA review of fed44e56): TopTransaction returns a NEW wrapper on every read, so a
            // wrapper-identity comparison is always false and refused every call. The identity is the native one.
            Assert.Contains("database.IsDisposed", precheck);
            Assert.Contains("transaction.IsDisposed", precheck);
            Assert.Contains("TopTransaction", precheck);
            Assert.Contains("UnmanagedObject", precheck);
            Assert.DoesNotContain("ReferenceEquals(", CreatorCode);
            Assert.Contains("RackDefinitionCreationFailure.TransactionMismatch", precheck);
            Assert.Contains("RackDefinitionCreationFailure.InvalidPlan", precheck);
            Assert.Contains("RackDefinitionCreationFailure.InvalidBlockName", precheck);
            Assert.Contains("RackDefinitionCreationFailure.InvalidEnvelope", precheck);
        }

        // ================================================================ I-52-AUTH15-C1: el Name logico del sobre es opcional

        [Fact]
        public void AUTH15_C1_I1_EL_PRECHECK_NO_LEE_EL_NAME_DEL_SOBRE()
        {
            var precheck = Body(CreatorCode, "private static RackDefinitionCreationResult? Precheck(");

            Assert.DoesNotContain("envelope.Name", precheck);
        }

        [Fact]
        public void AUTH15_C1_I2_AUTH15_NUNCA_LEE_EL_NAME_LOGICO()
        {
            // El Name logico lo elige el llamador; AUTH-15 escribe el sobre ya compuesto sin leerlo, recortarlo ni sustituirlo.
            Assert.DoesNotContain("envelope.Name", CreatorCode);
            Assert.DoesNotMatch(new Regex(@"envelope\s*\??\.\s*Name"), CreatorCode);
            Assert.DoesNotMatch(new Regex(@"\.Name"), CreatorCode);
        }

        [Fact]
        public void AUTH15_C1_I3_ID_KIND_Y_SOBRE_NULO_SIGUEN_SIENDO_INVALID_ENVELOPE()
        {
            var precheck = Body(CreatorCode, "private static RackDefinitionCreationResult? Precheck(");

            Assert.Contains("envelope == null", precheck);
            Assert.Contains("string.IsNullOrWhiteSpace(envelope.Id)", precheck);
            Assert.Contains("string.IsNullOrWhiteSpace(envelope.Kind)", precheck);
            Assert.Contains("RackDefinitionCreationFailure.InvalidEnvelope", precheck);
            Assert.Contains("new RackEmbedStore().Serialize(envelope)", precheck);
        }

        [Fact]
        public void AUTH15_C1_I4_EL_DIAGNOSTICO_NO_DICE_QUE_EL_NAME_SEA_REQUERIDO()
        {
            var precheck = Body(CreatorCode, "private static RackDefinitionCreationResult? Precheck(");

            Assert.Contains("\"sobre ausente o sin Id/Kind\"", precheck);
            Assert.DoesNotContain("Id/Kind/Name", Creator);
        }

        [Fact]
        public void AUTH15_C1_I4_LA_DOCUMENTACION_DE_INVALID_ENVELOPE_NO_DICE_QUE_EL_NAME_SEA_REQUERIDO()
        {
            Assert.DoesNotContain("Id/Kind/Name", Result);
            Assert.Contains("lacks Id/Kind", Result);
        }

        [Fact]
        public void AUTH15_LAS_EXCEPCIONES_SE_TIPAN_Y_NO_SE_LIMPIA_DENTRO()
        {
            for (var n = 0; n < 2; n++)
            {
                var body = Body(CreatorCode, Entry, n);

                Assert.Contains("catch (System.Exception", body);
                Assert.Contains("RackDefinitionCreationFailure.WriteFailed", body);
            }

            // No internal cleanup: the caller's rollback is the only authority over a half-written state.
            Assert.DoesNotContain("Erase(", CreatorCode);
        }

        [Fact]
        public void AUTH15_UNA_PIEZA_FALTANTE_NUNCA_ES_SILENCIOSA_NI_LLEVA_SOBRE()
        {
            var header = Body(CreatorCode, Entry, 0);
            var missing = header.IndexOf("HasMissingBlocks", StringComparison.Ordinal);
            var envelope = header.IndexOf("Envelope(", StringComparison.Ordinal);

            Assert.True(missing >= 0 && missing < envelope, "la comprobacion de faltantes debe preceder al sobre");
            Assert.Contains("RackDefinitionCreationResult.Missing(", header);
        }

        [Fact]
        public void AUTH15_UN_NOMBRE_EFECTIVO_VACIO_NUNCA_ES_EXITO_Y_SE_DECIDE_ANTES_DEL_SOBRE()
        {
            // RUN-2 (host): HeaderRun with "<>" came back as Success with an EMPTY block name, because the family sanitizes the
            // requested name to "" and AutoCAD accepts a nameless definition. AUTH-15 does not own naming, but it must not report
            // an unusable identity as a success: the ACTUAL name is checked, post-write, before anything else is decided.
            for (var n = 0; n < 2; n++)
            {
                var body = Body(CreatorCode, Entry, n);
                var name = body.IndexOf("UnusableEffectiveName(", StringComparison.Ordinal);
                var envelope = body.IndexOf("Envelope(", StringComparison.Ordinal);

                Assert.True(name >= 0, "la familia #" + n + " no valida el nombre efectivo");
                Assert.True(name < envelope, "la validacion del nombre debe preceder a Envelope(...) en la familia #" + n);
            }

            var header = Body(CreatorCode, Entry, 0);
            Assert.True(
                header.IndexOf("UnusableEffectiveName(", StringComparison.Ordinal) < header.IndexOf("HasMissingBlocks", StringComparison.Ordinal),
                "HeaderRun valida el nombre antes de tratar los bloques faltantes");
            Assert.Contains("UnusableEffectiveName(created.BlockName)", header);
            Assert.Contains("UnusableEffectiveName(blockName)", Body(CreatorCode, Entry, 1));

            var check = Body(CreatorCode, "private static RackDefinitionCreationResult? UnusableEffectiveName(");
            Assert.Contains("IsNullOrWhiteSpace(effectiveName)", check);
            Assert.Contains("RackDefinitionCreationFailure.WriteFailed", check);

            // No new failure value, no cleanup: the failure is POST-WRITE and the caller rolls back.
            Assert.DoesNotContain("BlockNameUnavailable", Code(Result));
            Assert.DoesNotContain("Erase(", check);
        }

        [Fact]
        public void AUTH15_NO_DERIVA_NI_DUPLICA_LA_POLITICA_DE_NOMBRES_DE_LA_FAMILIA()
        {
            // The check READS the name the family produced. It must never derive one: that would be a second naming policy (I-09).
            foreach (var forbidden in new[] { "BlockNaming", "Sanitize", "UniqueBlockName", ".Replace(", ".Trim(", "\"Cabecera\"" })
            {
                Assert.DoesNotContain(forbidden, CreatorCode);
            }
        }

        [Fact]
        public void AUTH15_EL_SOBRE_SE_VERIFICA_RELEYENDOLO()
        {
            var envelope = Body(CreatorCode, "private static RackDefinitionCreationResult Envelope(");
            var write = envelope.IndexOf("RackBlockData.Write(", StringComparison.Ordinal);
            var read = envelope.IndexOf("RackBlockData.Read(", StringComparison.Ordinal);

            Assert.True(write >= 0 && read > write, "el sobre se escribe y despues se relee");
            Assert.Contains("RackDefinitionCreationFailure.EnvelopeWriteFailed", envelope);
        }

        [Fact]
        public void AUTH15_EL_RESULTADO_TIPA_CADA_FALLO_DEL_CONTRATO()
        {
            foreach (var failure in new[]
                     {
                         "None", "TransactionMismatch", "InvalidPlan", "InvalidBlockName", "MissingLibraryBlocks",
                         "InvalidEnvelope", "EnvelopeWriteFailed", "WriteFailed",
                     })
            {
                Assert.Matches(new Regex(@"\b" + failure + @",\s*$", RegexOptions.Multiline), Code(Result));
            }

            Assert.Contains("public ObjectId DefinitionId", Result);
            Assert.Contains("public string BlockName", Result);
            Assert.Contains("public IReadOnlyList<HeaderBlockInstance> MissingInstances", Result);
        }

        // ================================================================ frontera

        [Fact]
        public void AUTH15_QUEDA_EN_EL_PLUGIN_Y_FUERA_DE_LA_FOUNDATION()
        {
            var application = Path.Combine(RepoRoot().FullName, "src", "RackCad.Application");

            foreach (var file in Directory.EnumerateFiles(application, "*.cs", SearchOption.AllDirectories))
            {
                Assert.DoesNotContain("RackDefinitionCreator", File.ReadAllText(file));
            }

            Assert.Contains("internal static class RackDefinitionCreator", Creator);
        }
    }
}
