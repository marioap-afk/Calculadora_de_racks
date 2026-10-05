using System;
using System.IO;
using System.Text.RegularExpressions;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-42 (A1C/H11) — GUARDAS DE ORIGEN DEL COMANDO DE BOM.
    ///
    /// <para>
    /// El Plugin referencia AutoCAD, asi que esta suite no puede cargarlo (ADR-0003) y CI no tiene AutoCAD donde
    /// ejecutarlo. Estas guardas leen el <c>.cs</c> del comando como TEXTO y fijan lo que solo existe ahi: que
    /// <c>RACKBOMTOTAL</c> consulta la puerta de salida antes de construir nada, que ABORTA el total cuando algun
    /// rack esta bloqueado —un total al que le falta un rack no puede parecer completo— y que un rack ilegible
    /// nunca se salta en silencio.
    /// </para>
    /// <para>
    /// La decision en si es pura y esta probada aparte (<see cref="PushBackOutputGateTests"/>); lo que aqui se pin
    /// es que el comando la CONSUME.
    /// </para>
    /// </summary>
    public class PushBackBomCommandGuardTests
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

        private static string ReadSource(params string[] parts)
        {
            var path = Path.Combine(RepoRoot().FullName, Path.Combine(parts));
            Assert.True(File.Exists(path), "No existe el archivo: " + path);
            return File.ReadAllText(path);
        }

        private static string BomTotal =>
            ReadSource("src", "RackCad.Plugin", "RackInventarioCommands.BomTotal.cs");

        private static string PushBackHandler =>
            ReadSource("src", "RackCad.Plugin", "KindHandlers", "PushBackKindHandler.cs");

        [Fact]
        public void BlockingDiagnostic_PreventsRackBomTotalOutput()
        {
            var source = BomTotal;

            // Pregunta la puerta ANTES de construir ningun BOM...
            //
            // I-47 G13 reapunta el ancla del segundo indice: el comando ya no indexa handlers en paralelo,
            // recorre los racks cuya autoridad multi-vista quedo aprobada. La propiedad protegida no cambia.
            Assert.Contains("OutputBlockedReason", source, StringComparison.Ordinal);
            Assert.True(
                source.IndexOf("OutputBlockedReason", StringComparison.Ordinal)
                < source.IndexOf("BuildRackBom(rack.Handler", StringComparison.Ordinal),
                "la puerta se consulta antes de construir el BOM de ningun rack");

            // ...y ABORTA el total, como ya hacia con un kind sin handler.
            Assert.Contains("RackBomOutputGate.DescribeBlocked(blocked)", source, StringComparison.Ordinal);
            Assert.Contains("if (blocked.Count > 0)", source, StringComparison.Ordinal);
        }

        [Fact]
        public void BlockingDiagnostic_CommandWritesTheReason()
        {
            var source = BomTotal;
            var abort = source.IndexOf("if (blocked.Count > 0)", StringComparison.Ordinal);

            Assert.True(abort > 0);
            var block = source.Substring(abort, Math.Min(400, source.Length - abort));
            Assert.Contains("editor.WriteMessage", block, StringComparison.Ordinal);
            Assert.Contains("DescribeBlocked", block, StringComparison.Ordinal);
            Assert.Contains("return;", block, StringComparison.Ordinal);
        }

        [Fact]
        public void UnreadableRack_IsNeverSkippedSilentlyByTheCommand()
        {
            var source = BomTotal;

            // I-47 G13: el salto ya no se decide sobre un null, sino sobre el resultado TIPADO -que ademas
            // distingue el payload ilegible del vinculo roto, que aborta el total en vez de saltarse-.
            var skip = source.IndexOf("if (!result.IsSuccess)", StringComparison.Ordinal);

            Assert.True(skip > 0, "el comando sigue teniendo el salto por payload ilegible");
            var block = source.Substring(skip, Math.Min(500, source.Length - skip));
            Assert.Contains("DescribeUnreadable", block, StringComparison.Ordinal);
            Assert.Contains("editor.WriteMessage", block, StringComparison.Ordinal);
        }

        /// <summary>El codigo sin comentarios: un comentario XML nunca puede hacer pasar una guarda de fuente.</summary>
        private static string CodeOnly(string source)
        {
            var withoutBlocks = Regex.Replace(source, @"/\*.*?\*/", string.Empty, RegexOptions.Singleline);
            return Regex.Replace(withoutBlocks, @"//[^\n]*", string.Empty);
        }

        /// <summary>
        /// El cuerpo del metodo <paramref name="name"/> (con cuerpo de expresion hasta el <c>;</c>, o con bloque hasta su llave de
        /// cierre), o una cadena vacia si no existe.
        /// </summary>
        private static string MethodText(string code, string name)
        {
            var signature = Regex.Match(code, @"\b" + name + @"\s*\(");
            if (!signature.Success)
            {
                return string.Empty;
            }

            var depth = 0;
            var index = signature.Index + signature.Length - 1;
            for (; index < code.Length; index++)
            {
                if (code[index] == '(')
                {
                    depth++;
                }
                else if (code[index] == ')' && --depth == 0)
                {
                    break;
                }
            }

            var rest = code.Substring(index + 1).TrimStart();
            if (rest.StartsWith("=>", StringComparison.Ordinal))
            {
                var end = rest.IndexOf(';');
                return end < 0 ? rest : rest.Substring(0, end + 1);
            }

            depth = 0;
            for (var position = 0; position < rest.Length; position++)
            {
                if (rest[position] == '{')
                {
                    depth++;
                }
                else if (rest[position] == '}' && --depth == 0)
                {
                    return rest.Substring(0, position + 1);
                }
            }

            return rest;
        }

        [Fact]
        public void ThePushBackHandler_ConsumesTheSharedGateAndNotASecondRule()
        {
            // I-63 A-4 (DEBT-I63-G2-01): desde D-27 la puerta se compone en UN solo sitio de Application, RackOutputVerdict.
            // La guarda lee el codigo sin comentarios y fija la arquitectura vigente: OutputBlockedReason delega y no compone.
            // La autoridad primaria es INV-33 (ComputedParametersPopulationGuardTests); esta es su defensa legacy redundante.
            var code = CodeOnly(PushBackHandler);
            var method = MethodText(code, "OutputBlockedReason");

            Assert.False(string.IsNullOrWhiteSpace(method), "el handler sigue declarando OutputBlockedReason");
            Assert.Matches(@"\bRackOutputVerdict\s*\.", method);
            Assert.DoesNotMatch(@"\bPushBackResolver\b", method);
            Assert.DoesNotMatch(@"\bRackBomOutputGate\s*\.\s*For\b", method);

            // No hay una segunda regla de validez escrita a mano en el Plugin.
            Assert.DoesNotContain("IsInvalidForBom", code, StringComparison.Ordinal);
            Assert.DoesNotContain("RequiredBedLength", code, StringComparison.Ordinal);
        }
    }
}
