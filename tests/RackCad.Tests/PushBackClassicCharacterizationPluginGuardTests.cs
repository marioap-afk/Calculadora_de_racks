using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// F4-T1B (I-64) — GUARDAS DE FUENTE de lo que el PLUGIN hace con la ventana CLASICA de Push Back despues de que
    /// esta se cierra (clase C de INV-F4T1B-16): RACKPUSHBACK/RPB, RACKEDITAR, el menu y las tres fachadas de dibujo.
    /// CT-24 a CT-27 del paquete v5.
    ///
    /// <para><b>Nunca se caracteriza una escritura del Plugin como escritura de la ventana.</b> La ventana solo
    /// EXPONE una intencion (<c>InsertionRequest</c>, que se fija en ejecucion en <c>RackCad.UI.Tests</c>); quien
    /// escribe el DWG es el Plugin, DESPUES de cerrarla, y eso solo se puede fijar aqui por texto: el Plugin
    /// referencia AutoCAD y <c>RackCad.Tests</c> no lo carga (ADR-0003), ni la CI tiene AutoCAD. El comportamiento
    /// fisico lo cobra Smoke-2.</para>
    ///
    /// <para>Son guardas de TEXTO (INV-F4T1B-08): normalizan CRLF a LF (Core corre en Ubuntu y en local se trabaja
    /// con <c>core.autocrlf=true</c>), quitan los comentarios y colapsan el espacio en blanco. F4-T4B anadira el
    /// primer llamador de los caller-owned de las fachadas: por eso aqui se fija solo que los caminos CLASICOS no los
    /// llaman y que existen, no que no tengan llamador.</para>
    /// </summary>
    public sealed class PushBackClassicCharacterizationPluginGuardTests
    {
        /// <summary>La constante que marca estas pruebas como guardas de texto y que F5 sustituye (patron de I-39).</summary>
        private const string TextGuard =
            "GUARDA DE TEXTO (F4-T1B, INV-F4T1B-08): fija una estructura del codigo fuente del Plugin que RackCad.Tests no puede ejecutar (ADR-0003); "
            + "la validacion fisica es de Smoke-2 y F5 la sustituye segun el patron de I-39.";

        private static DirectoryInfo RepoRoot()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "RackCad.sln")))
            {
                dir = dir.Parent;
            }

            Assert.True(dir != null, "No se localizo la raiz del repo (RackCad.sln) desde el directorio de salida de las pruebas.");
            return dir;
        }

        private static string StripCSharpComments(string text)
        {
            var result = new StringBuilder(text.Length);
            var i = 0;
            while (i < text.Length)
            {
                var c = text[i];
                var next = i + 1 < text.Length ? text[i + 1] : '\0';
                if (c == '/' && next == '/')
                {
                    while (i < text.Length && text[i] != '\n')
                    {
                        i++;
                    }

                    continue;
                }

                if (c == '/' && next == '*')
                {
                    i += 2;
                    while (i + 1 < text.Length && !(text[i] == '*' && text[i + 1] == '/'))
                    {
                        i++;
                    }

                    i += 2;
                    result.Append(' ');
                    continue;
                }

                if (c == '@' && next == '"')
                {
                    result.Append("@\"");
                    i += 2;
                    while (i < text.Length)
                    {
                        if (text[i] == '"')
                        {
                            if (i + 1 < text.Length && text[i + 1] == '"')
                            {
                                result.Append("\"\"");
                                i += 2;
                                continue;
                            }

                            result.Append('"');
                            i++;
                            break;
                        }

                        result.Append(text[i]);
                        i++;
                    }

                    continue;
                }

                if (c == '"' || c == '\'')
                {
                    result.Append(c);
                    i++;
                    while (i < text.Length)
                    {
                        if (text[i] == '\\' && i + 1 < text.Length)
                        {
                            result.Append(text[i]);
                            result.Append(text[i + 1]);
                            i += 2;
                            continue;
                        }

                        if (text[i] == c)
                        {
                            result.Append(c);
                            i++;
                            break;
                        }

                        result.Append(text[i]);
                        i++;
                    }

                    continue;
                }

                result.Append(c);
                i++;
            }

            return result.ToString();
        }

        /// <summary>CRLF a LF, sin comentarios y con los espacios en blanco colapsados.</summary>
        private static string Source(params string[] relative)
        {
            var path = Path.Combine(new[] { RepoRoot().FullName, "src", "RackCad.Plugin" }.Concat(relative).ToArray());
            Assert.True(File.Exists(path), "Fuente del Plugin no encontrada: " + string.Join("/", relative));
            var lf = File.ReadAllText(path).Replace("\r\n", "\n").Replace('\r', '\n');
            return Regex.Replace(StripCSharpComments(lf), @"\s+", " ").Trim();
        }

        private static string Body(string source, string marker)
        {
            var at = source.IndexOf(marker, StringComparison.Ordinal);
            Assert.True(at >= 0, "no se encontro: " + marker);
            var open = source.IndexOf('{', at);
            Assert.True(open >= 0, "sin cuerpo: " + marker);
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

        private static void Has(string source, string expected, string what)
            => Assert.True(source.Contains(expected, StringComparison.Ordinal), TextGuard + " Falta " + what + ": " + expected);

        private static void Lacks(string source, string forbidden, string what)
            => Assert.True(!source.Contains(forbidden, StringComparison.Ordinal), TextGuard + " Sobra " + what + ": " + forbidden);

        private static void Matches(string source, string pattern, string what)
            => Assert.True(Regex.IsMatch(source, pattern, RegexOptions.CultureInvariant), TextGuard + " Falta " + what + " (patron " + pattern + ")");

        private static void InOrder(string source, params string[] steps)
        {
            var previous = -1;
            foreach (var step in steps)
            {
                var at = source.IndexOf(step, previous + 1, StringComparison.Ordinal);
                Assert.True(at > previous, TextGuard + " Falta o esta fuera de orden: " + step);
                previous = at;
            }
        }

        // ================================================================================================== CT-24

        [Fact]
        public void RackPushBackShowsTheWindowModallyWithoutAnOwnerAndReadsOnlyTheViewAndSectionOfTheRequest()
        {
            var body = Body(Source("RackPushBackCommands.cs"), "public void RackPushBack()");

            Has(body, "new RackPushBackSystemWindow(canInsertInAutoCad: true);", "la ventana creada para AutoCAD");
            Has(body, "AcApplication.ShowModalWindow(window);", "ShowModalWindow(window)");
            Has(body, "window.InsertionRequest is PushBackInsertionRequest request", "la lectura tipada de la peticion");
            Has(body, "request.View,", "request.View");
            Has(body, "request.Section,", "request.Section");
            // Ancla (anadir una asignacion no es un mutante textual de comportamiento identico): ni la ventana ni su
            // llamador asignan Owner; ShowModalWindow la muestra sin dueno WPF.
            Lacks(body, "Owner =", "una asignacion de Owner en RACKPUSHBACK");
            // RACKPUSHBACK lee View/Section, no Views: «Insertar varias…» dibujaria solo Views[0] (OD-6).
            Lacks(body, "Views", "la lectura de Views en RACKPUSHBACK");
        }

        [Fact]
        public void RackPushBackDrawsAfterTheCloseThroughDrawPushBackViewThatNamesTheSystem()
        {
            var source = Source("RackPushBackCommands.cs");
            var command = Body(source, "public void RackPushBack()");

            // Clase C: el DWG lo escribe el Plugin, despues de cerrar la ventana, por DrawPushBackView.
            Matches(
                command,
                @"RackUnitsGuard\.WarnIfNotInches\(document\); DrawPushBackView\( request\.View, request\.Section, request\.System, request\.Design, request\.RackId, request\.RackName, source: null, innerSource: request\.SourceProject\);",
                "la llamada a DrawPushBackView sin cualificar, tras el aviso de unidades");

            var draw = Body(source, "internal static void DrawPushBackView(");
            Has(draw, "system.Name = rackName;", "la mutacion de System.Name");
            InOrder(
                draw,
                "rackName = RackNewRackName.Resolve(document, RackSystemKind.PushBack, rackName);",
                "system.Name = rackName;");
        }

        // ================================================================================================== CT-25

        [Fact]
        public void RackEditarReadsTheWindowOutcomeAndWritesInTheFixedOrderWithNoRegenPerView()
        {
            var edit = Body(Source("RackPushBackCommands.cs"), "internal static void EditPushBack(");

            // Lo que lee del resultado de la ventana.
            InOrder(
                edit,
                "window.LoadExisting(project.PushBackDesign, embed.Id, embed.Name, project);",
                "if (!window.InsertRequested)",
                "var design = window.DesignToInsert;",
                "var system = window.SystemToInsert;");
            Has(edit, "string.IsNullOrWhiteSpace(embed.Id) ? window.RackId : embed.Id", "RackId como respaldo del GUID del bloque");
            Has(edit, "string.IsNullOrWhiteSpace(window.RackName) ? embed.Name : window.RackName", "RackName con el nombre del bloque como respaldo");
            Has(edit, "system.Name = name;", "la mutacion de System.Name");
            Has(edit, "window.UpdateOnly", "UpdateOnly");
            Has(edit, "window.InsertionRequest.Views", "InsertionRequest.Views");

            // El orden de las escrituras (clase C): el lote se prepara ENTERO antes del primer RedrawInPlace; despues
            // SyncName < EraseViewBlocks < un unico Regen() < RackViewBatchExecution.Run.
            InOrder(
                edit,
                "batchProducts.PrepareAll(batchRequest, out var batchDiagnostic)",
                ".RedrawInPlace(",
                "RackBlockRenamer.SyncName(",
                "RackCommandSupport.EraseViewBlocks(",
                "document.Editor.Regen();",
                "RackViewBatchExecution.Run(");

            // Cada RedrawInPlace de una vista ligada va con regen: false, y el unico Regen() es el del final.
            Assert.True(
                Regex.Matches(edit, Regex.Escape("regen: false")).Count == 3,
                TextGuard + " Las tres llamadas RedrawInPlace de RACKEDITAR deben llevar regen: false.");
            Assert.True(
                Regex.Matches(edit, Regex.Escape("Editor.Regen();")).Count == 1,
                TextGuard + " RACKEDITAR hace UN unico Regen() explicito.");
        }

        // ================================================================================================== CT-26

        [Fact]
        public void TheMenuHostNamesTheNewRackThenPreparesAndRunsTheBatchWithoutAKindSwitch()
        {
            var menu = Source("RackMenuCommands.cs");
            var start = menu.IndexOf("case PushBackInsertionRequest pushBack:", StringComparison.Ordinal);
            Assert.True(start >= 0, TextGuard + " Falta el case tipado de Push Back en el menu");
            var end = menu.IndexOf("break;", start, StringComparison.Ordinal);
            Assert.True(end > start, TextGuard + " El case tipado de Push Back no termina");
            var typedCase = menu.Substring(start, end - start);

            InOrder(
                typedCase,
                "Systems.Shared.RackNewRackName.Resolve(",
                "pushBack.System.Name = pushBackName;",
                "= RackViewBatchProducts.PushBack(document, pushBack.System, pushBack.Design, pushBack.RackId, pushBackName, null, pushBack.SourceProject, null);",
                "RackViewBatchExecution.Run(document, pushBackProducts,");
            Has(typedCase, "RackProductSourceKind.NewRack, pushBack.Kind, pushBack.RackId, pushBack.Views", "la cola de vistas de la peticion");
        }

        [Fact]
        public void TheBatchProductsPlaceEveryPushBackViewWithoutRegeneratingEachOne()
        {
            Has(
                Source("Views", "RackViewBatchProducts.cs"),
                "product => RackViewPlacement.PlacePushBack(document, product, regen: false)",
                "PlacePushBack(..., regen: false) de la cola");
        }

        [Fact]
        public void ThePlacementFacadeForwardsTheRegenFlagToPlaceHeaderWithThePushBackKind()
        {
            var placement = Source("Views", "RackViewPlacement.cs");

            Has(
                placement,
                "PlacePushBack(Document document, RackPreparedProductView<HeaderRunPlan> product, bool regen) => PlaceHeader(document, product, RackEmbedDocument.KindPushBack, regen: regen);",
                "PlacePushBack(..., regen) hacia PlaceHeader");
        }

        // ================================================================================================== CT-27

        public static TheoryData<string, string[], string[]> Facades => new TheoryData<string, string[], string[]>
        {
            {
                "PushBackSystemDrawService (lateral)",
                new[] { "Systems", "PushBack", "PushBackSystemDrawService.cs" },
                new[]
                {
                    "public HeaderPlacementResult DrawAndPlace( Document document, PushBackSystem system, string payloadJson = null, string rackName = null, int postIndex = -1)",
                    "public HeaderPlacementResult RedrawInPlace( Document document, ObjectId blockId, PushBackSystem system, string payloadJson, bool regen = true, int postIndex = -1)",
                    "internal PreparedViewRedraw PrepareRedraw(Database database, ObjectId blockId, PushBackSystem system, string payloadJson, int postIndex = -1)",
                    "internal LateralHeaderDrawOutcome RedrawInTransaction(Database database, Transaction transaction, PreparedViewRedraw prepared, out IReadOnlyCollection<ObjectId> staleDefinitions)",
                }
            },
            {
                "PushBackFrontalDrawService",
                new[] { "Systems", "PushBack", "PushBackFrontalDrawService.cs" },
                new[]
                {
                    "public HeaderPlacementResult DrawAndPlace( Document document, PushBackSystem system, PushBackFrontalEnd end, string payloadJson = null, string rackName = null, PushBackSide side = PushBackSide.A)",
                    "public HeaderPlacementResult RedrawInPlace( Document document, ObjectId blockId, PushBackSystem system, PushBackFrontalEnd end, string payloadJson, bool regen = true, PushBackSide side = PushBackSide.A)",
                    "internal PreparedViewRedraw PrepareRedraw(Database database, ObjectId blockId, PushBackSystem system, PushBackFrontalEnd end, string payloadJson, PushBackSide side = PushBackSide.A)",
                    "internal LateralHeaderDrawOutcome RedrawInTransaction(Database database, Transaction transaction, PreparedViewRedraw prepared, out IReadOnlyCollection<ObjectId> staleDefinitions)",
                }
            },
            {
                "PushBackPlantaDrawService",
                new[] { "Systems", "PushBack", "PushBackPlantaDrawService.cs" },
                new[]
                {
                    "public HeaderPlacementResult DrawAndPlace( Document document, PushBackSystem system, string payloadJson = null, string rackName = null)",
                    "public HeaderPlacementResult RedrawInPlace( Document document, ObjectId blockId, PushBackSystem system, string payloadJson, bool regen = true)",
                    "internal PreparedViewRedraw PrepareRedraw(Database database, ObjectId blockId, PushBackSystem system, string payloadJson)",
                    "internal LateralHeaderDrawOutcome RedrawInTransaction(Database database, Transaction transaction, PreparedViewRedraw prepared, out IReadOnlyCollection<ObjectId> staleDefinitions)",
                }
            },
        };

        [Theory]
        [MemberData(nameof(Facades))]
        public void EachPushBackFacadeKeepsItsExactPublicSignaturesAndItsCallerOwnedPrimitives(
            string facade, string[] path, string[] signatures)
        {
            var source = Source(path);

            foreach (var signature in signatures)
            {
                Has(source, signature, facade + ": la firma");
            }
        }

        [Fact]
        public void TheClassicPathsNeverCallTheCallerOwnedPrimitivesOfTheFacades()
        {
            // Que las primitivas caller-owned no tengan hoy ningun llamador NO se congela: F4-T4B anadira el primero.
            // Se fija que los caminos CLASICOS (RACKPUSHBACK, RACKEDITAR y su handler, el menu y la cola de vistas) NO
            // las llaman: siguen por los wrappers self-owned.
            foreach (var path in new[]
                     {
                         new[] { "RackPushBackCommands.cs" },
                         new[] { "KindHandlers", "PushBackKindHandler.cs" },
                         new[] { "RackMenuCommands.cs" },
                         new[] { "Views", "RackViewBatchProducts.cs" },
                     })
            {
                var source = Source(path);
                Lacks(source, "PrepareRedraw(", path.Last() + " no llama a PrepareRedraw");
                Lacks(source, "RedrawInTransaction(", path.Last() + " no llama a RedrawInTransaction");
            }

            // Ancla: el handler solo reenvia a la unica implementacion de la edicion (su archivo no es de los mutantes).
            Has(
                Source("KindHandlers", "PushBackKindHandler.cs"),
                "=> RackPushBackCommands.EditPushBack(document, blockId, embed);",
                "el reenvio del handler a EditPushBack");
        }
    }
}
