using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using RackCad.Application.Persistence;
using RackCad.Application.VentanaRackCad;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// T6 lib (I-64, sixth batch; UT-S10 and CM-S13): a design of the library opened in the window as a new Dinámico, Push Back or Cantilever
    /// takes the JSON fields of its file this build does not know to the drawing when it is inserted, as the classic editor opened from the
    /// library takes them (I-11, matrix B5). The rule of the journey (<see cref="RackLibrarySourceDesign"/>), its warning in the log of I-03
    /// and, by their source, the commands and the queues of the door that hand it to the dispatch of «Nuevo». In the collection of the log:
    /// a JSON that is not used writes a warning (review of 39addd8f, point 1), and the log is process-wide.
    /// </summary>
    [Collection("RackLog")]
    public sealed class RackLibrarySourceDesignTests
    {
        // A file of the library written by a newer build of the same major: a field this build does not know and a newer minor version, in the
        // wrapper and in the payload of the Push Back.
        private static string FileOfTheFuture()
        {
            var root = JsonNode.Parse(new RackEmbedStore().Deserialize(PilotKit.Text("pb-03-fondos")).Design)!.AsObject();
            root["SchemaVersion"] = "2.7";
            root["FuturoRaiz"] = new JsonObject { ["nivel"] = 1 };
            root["PushBack"]!.AsObject()["FuturoAnidado"] = 42;
            root["PushBack"]!.AsObject()["SchemaVersion"] = "1.9";
            return root.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
        }

        // The inner design of a new view (RackViewBatchProducts.Project): the project with the metadata of its source.
        private static string Inner(RackProject project, RackProject source)
            => new RackProjectStore().Serialize(project.WithSourceMetadataFrom(source));

        [Fact]
        public void TheDesignOfTheLibraryTravelsWithItsUnknownFieldsAndTheInsertionWritesWhatTheClassicEditorWrites()
        {
            using var log = LogCapture.Begin();
            var library = new RackProjectStore().Deserialize(FileOfTheFuture());
            var json = RackLibrarySourceDesign.Of(library);
            var source = RackLibrarySourceDesign.Read(json, RackSystemKind.PushBack);
            Assert.NotNull(source);

            var design = library.PushBackDesign;
            var inner = Inner(RackProject.ForPushBack(design), source);
            Assert.Equal(Inner(RackProject.ForPushBack(design), library), inner); // the classic editor: SourceProjectToInsert
            var written = JsonNode.Parse(inner)!.AsObject();
            Assert.Equal("2.7", (string)written["SchemaVersion"]);
            Assert.Equal(1, (int)written["FuturoRaiz"]!["nivel"]!);
            Assert.Equal(42, (int)written["PushBack"]!["FuturoAnidado"]!);
            Assert.Equal("1.9", (string)written["PushBack"]!["SchemaVersion"]);

            // Without the source, the insertion is the one of a new rack: no unknown field, the version of this build.
            var fresh = JsonNode.Parse(Inner(RackProject.ForPushBack(design), null))!.AsObject();
            Assert.Equal(RackProjectDocument.CurrentSchemaVersion, (string)fresh["SchemaVersion"]);
            Assert.False(fresh.ContainsKey("FuturoRaiz"));
            Assert.Equal(string.Empty, log.Text); // a design that is used leaves nothing in the log
        }

        // Point 1 of the review of 39addd8f: a JSON the command does not use (it does not read back, a newer major wrote it, or it is a project
        // of another system) is said in the log of I-03, with the system and why; the rack is still inserted, without the fields of its file.
        // No JSON is no design of the library: nothing is said.
        [Fact]
        public void AJsonThatIsNotUsedLeavesAWarningInTheLogAndNoJsonSaysNothing()
        {
            using var log = LogCapture.Begin();
            Assert.Null(RackLibrarySourceDesign.Of(null));
            Assert.Null(RackLibrarySourceDesign.Read(null, RackSystemKind.PushBack));
            Assert.Null(RackLibrarySourceDesign.Read("  ", RackSystemKind.PushBack));
            Assert.Equal(string.Empty, log.Text);

            Assert.Null(RackLibrarySourceDesign.Read("{ esto no es un diseño", RackSystemKind.PushBack));
            Assert.Contains(RackLibrarySourceDesign.LogContext, log.Text);
            Assert.Contains("WARN", log.Text);
            Assert.Contains("El diseño de la biblioteca del rack nuevo (PushBack) no se pudo leer: se inserta sin los campos de su archivo que esta "
                            + "versión no conoce (I-11).", log.Text);

            var json = RackLibrarySourceDesign.Of(new RackProjectStore().Deserialize(FileOfTheFuture()));
            Assert.Null(RackLibrarySourceDesign.Read(json, RackSystemKind.PalletFlow));
            Assert.Null(RackLibrarySourceDesign.Read(json, RackSystemKind.Cantilever));
            Assert.Contains("(PalletFlow) es un proyecto de otro sistema (PushBack)", log.Text);
            Assert.Contains("(Cantilever) es un proyecto de otro sistema (PushBack)", log.Text);

            var future = JsonNode.Parse(json)!.AsObject();
            future["SchemaVersion"] = "3.0";
            Assert.Null(RackLibrarySourceDesign.Read(future.ToJsonString(), RackSystemKind.PushBack));
            Assert.Contains("(PushBack) lo escribió una versión más nueva", log.Text);

            Assert.Equal(RackSystemKind.PushBack, RackLibrarySourceDesign.Read(json, RackSystemKind.PushBack)!.Kind);
            Assert.Equal(4, Regex.Matches(log.Text, Regex.Escape(RackLibrarySourceDesign.LogContext)).Count); // one line per JSON not used
        }

        // ------------------------------------------------------------------ the commands and the queues of the door (Plugin, by its source)

        // The arguments of every call that starts with the pattern <paramref name="call"/> (an opening parenthesis included), split at its
        // top-level commas. The comments are gone already (WorkspaceHostSource.Code), so a call in a comment is no call.
        private static IReadOnlyList<IReadOnlyList<string>> CallsOf(string code, string call)
        {
            var calls = new List<IReadOnlyList<string>>();
            foreach (Match match in Regex.Matches(code, call))
            {
                var arguments = new List<string>();
                var depth = 1;
                var from = match.Index + match.Length;
                for (var i = from; i < code.Length && depth > 0; i++)
                {
                    var c = code[i];
                    if (c == '(' || c == '{' || c == '[') depth++;
                    else if ((c == ')' || c == '}' || c == ']') && --depth == 0) arguments.Add(Squeezed(code.Substring(from, i - from)));
                    else if (c == ',' && depth == 1)
                    {
                        arguments.Add(Squeezed(code.Substring(from, i - from)));
                        from = i + 1;
                    }
                }

                calls.Add(arguments);
            }

            return calls;
        }

        private static string Squeezed(string text) => Regex.Replace(text, @"\s+", " ").Trim();

        // True when a call passes <paramref name="value"/> as one of its arguments, in any position, by position or by name: the part pr adds
        // «Properties:» to the same calls, so the guard does not depend on the order of the arguments.
        private static bool Passes(string code, string call, string value)
            => CallsOf(WorkspaceHostSource.CodeOnly(code), call)
                .Any(arguments => arguments.Any(a => a == value || Regex.IsMatch(a, @"^\w+\s*:\s*" + Regex.Escape(value) + "$")));

        // The queue keeps the whole write of the window: a copy made with «with» that does not give SourceDesignJson another value.
        private static bool KeepsTheWholeWrite(string firstArgument)
            => Regex.IsMatch(firstArgument, @"^write with \{")
               && !Regex.IsMatch(firstArgument, @"\bSourceDesignJson\s*=(?!\s*write\.SourceDesignJson\b)");

        private const string DynamicNew = @"DynamicDraftOutcome\.NewRackRequest\s*\(";
        private const string PushBackNew = @"PushBackDoorInsertion\.NewRackRequest\s*\(";
        private const string CantileverNew = @"CantileverDraftOutcome\.NewRackRequest\s*\(";

        [Fact]
        public void TheGuardsOfTheCommandsFindTheArgumentInAnyPositionAndNeverInAComment()
        {
            Assert.True(Passes("F.G(a, b, pending.SourceDesignJson);", @"F\.G\s*\(", "pending.SourceDesignJson"));
            Assert.True(Passes("F.G(a,\n  pending.SourceDesignJson, Properties: p);", @"F\.G\s*\(", "pending.SourceDesignJson"));
            Assert.True(Passes("F.G(a, sourceDesignJson: pending.SourceDesignJson, b);", @"F\.G\s*\(", "pending.SourceDesignJson"));
            Assert.True(Passes("F.G(a, H(b, c), new List<X>(d), pending.SourceDesignJson);", @"F\.G\s*\(", "pending.SourceDesignJson"));

            Assert.False(Passes("F.G(a, b);", @"F\.G\s*\(", "pending.SourceDesignJson"));
            Assert.False(Passes("F.G(a, b); // F.G(a, pending.SourceDesignJson);", @"F\.G\s*\(", "pending.SourceDesignJson"));
            Assert.False(Passes("/* F.G(a, pending.SourceDesignJson); */ F.G(a, b);", @"F\.G\s*\(", "pending.SourceDesignJson"));
            Assert.False(Passes("F.G(a, b // , pending.SourceDesignJson\n);", @"F\.G\s*\(", "pending.SourceDesignJson"));
            Assert.False(Passes("F.G(a, H(pending.SourceDesignJson));", @"F\.G\s*\(", "pending.SourceDesignJson"));
            Assert.False(Passes("F.G(a, b); var x = pending.SourceDesignJson;", @"F\.G\s*\(", "pending.SourceDesignJson"));

            Assert.True(KeepsTheWholeWrite("write with { RackId = write.RackId.Trim(), InsertViews = null }"));
            Assert.True(KeepsTheWholeWrite("write with { SourceDesignJson = write.SourceDesignJson }"));
            Assert.False(KeepsTheWholeWrite("write with { RackId = write.RackId.Trim(), SourceDesignJson = null }"));
            Assert.False(KeepsTheWholeWrite("new PushBackDoorWrite(write.RequestId, write.Session, write.RackId, write.Name, write.Design, write.System)"));
        }

        // The commands of the door (Plugin, read as text: the Core tests cannot reference AutoCAD) hand the design of the library the window
        // sent to the request of «Nuevo», which carries it to the dispatch as the classic editor carries its SourceProjectToInsert.
        [Fact]
        public void TheCommandsOfTheNewRacksOfTheWindowHandTheDesignOfTheLibraryToTheDispatchOfNuevo()
        {
            var dynamic = WorkspaceHostSource.BodyOf(WorkspaceHostSource.Code("src/RackCad.Plugin/VentanaRackCad/RackCadRackDoorCommands.cs"),
                @"public\s+void\s+InsertNewRack\s*\(");
            Assert.True(Passes(dynamic, DynamicNew, "pending.SourceDesignJson"), dynamic);

            var pushBack = WorkspaceHostSource.BodyOf(WorkspaceHostSource.Code("src/RackCad.Plugin/VentanaRackCad/RackCadPushBackDoorCommands.cs"),
                @"public\s+void\s+InsertNewPushBack\s*\(");
            Assert.True(Passes(pushBack, PushBackNew, "write.SourceDesignJson"), pushBack);
            Assert.Matches(@"var\s+write\s*=\s*pending\.Write\s*;", pushBack);

            var cantilever = WorkspaceHostSource.BodyOf(WorkspaceHostSource.Code("src/RackCad.Plugin/VentanaRackCad/RackCadCantileverDoorCommands.cs"),
                @"public\s+void\s+InsertNewCantilever\s*\(");
            Assert.True(Passes(cantilever, CantileverNew, "write.SourceDesignJson"), cantilever);
            Assert.Matches(@"var\s+write\s*=\s*pending\.Write\s*;", cantilever);
        }

        // The queues of the door keep it until the command takes it: the new Dinámico in its PendingWrite; Push Back and Cantilever keep the
        // whole write of the window, as a copy made with «with» (RackCadRackDoorPort.PushBack.cs and .Cantilever.cs).
        [Fact]
        public void TheQueuesOfTheDoorKeepTheDesignOfTheLibraryUntilTheCommandTakesIt()
        {
            var port = WorkspaceHostSource.BodyOf(WorkspaceHostSource.Code("src/RackCad.Plugin/VentanaRackCad/RackCadRackDoorPort.cs"),
                @"public\s+RackCadClassicResult\s+RequestDynamicUpdate\s*\(");
            Assert.True(Passes(port, @"_pendingNew\s*=\s*new\s+PendingWrite\s*\(", "write.SourceDesignJson"), port);

            var pushBackPort = WorkspaceHostSource.Code("src/RackCad.Plugin/VentanaRackCad/RackCadRackDoorPort.PushBack.cs");
            var update = WorkspaceHostSource.BodyOf(pushBackPort, @"public\s+RackCadClassicResult\s+RequestPushBackUpdate\s*\(");
            var pending = Assert.Single(CallsOf(update, @"new\s+PendingPushBackWrite\s*\("));
            Assert.True(KeepsTheWholeWrite(pending[0]), pending[0]);
            Assert.Matches(@"if\s*\(\s*write\.NewRack\s*\)\s*_pendingPushBackNew\s*=\s*pending\s*;",
                WorkspaceHostSource.BodyOf(pushBackPort, @"private\s+static\s+void\s+SetPushBackPending\s*\("));

            var cantileverPort = WorkspaceHostSource.Code("src/RackCad.Plugin/VentanaRackCad/RackCadRackDoorPort.Cantilever.cs");
            var copy = WorkspaceHostSource.BodyOf(cantileverPort, @"private\s+static\s+PendingCantileverWrite\s+Copy\s*\(");
            var copied = Assert.Single(CallsOf(copy, @"new\s+PendingCantileverWrite\s*\("));
            Assert.True(KeepsTheWholeWrite(copied[0]), copied[0]);
            Assert.Matches(@"_pendingCantileverNew\s*=\s*Copy\s*\(\s*write\s*,\s*document\s*\)",
                WorkspaceHostSource.BodyOf(cantileverPort, @"public\s+RackCadClassicResult\s+RequestCantileverUpdate\s*\("));
        }
    }
}
