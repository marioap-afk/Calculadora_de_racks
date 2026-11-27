using System.Text.Json;
using System.Text.Json.Nodes;
using RackCad.Application.Persistence;
using RackCad.Application.VentanaRackCad;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// T6 lib (I-64, sixth batch; UT-S10 and CM-S13): a design of the library opened in the window as a new Dinámico, Push Back or Cantilever
    /// takes the JSON fields of its file this build does not know to the drawing when it is inserted, as the classic editor opened from the
    /// library takes them (I-11, matrix B5). The rule of the journey (<see cref="RackLibrarySourceDesign"/>) and, by their source, the
    /// commands of the door that hand it to the dispatch of «Nuevo».
    /// </summary>
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
        }

        [Fact]
        public void NothingTravelsWithoutADesignOfTheLibraryAndNothingIsReadFromAnotherSystemOrFromWhatIsNotAProject()
        {
            Assert.Null(RackLibrarySourceDesign.Of(null));
            Assert.Null(RackLibrarySourceDesign.Read(null, RackSystemKind.PushBack));
            Assert.Null(RackLibrarySourceDesign.Read("  ", RackSystemKind.PushBack));
            Assert.Null(RackLibrarySourceDesign.Read("{ esto no es un diseño", RackSystemKind.PushBack));

            var json = RackLibrarySourceDesign.Of(new RackProjectStore().Deserialize(FileOfTheFuture()));
            Assert.Null(RackLibrarySourceDesign.Read(json, RackSystemKind.PalletFlow));
            Assert.Null(RackLibrarySourceDesign.Read(json, RackSystemKind.Cantilever));
            Assert.Equal(RackSystemKind.PushBack, RackLibrarySourceDesign.Read(json, RackSystemKind.PushBack)!.Kind);
        }

        // The commands of the door (Plugin, read as text: the Core tests cannot reference AutoCAD) hand the design of the library the window
        // sent to the request of «Nuevo», which carries it to the dispatch as the classic editor carries its SourceProjectToInsert.
        [Fact]
        public void TheCommandsOfTheNewRacksOfTheWindowHandTheDesignOfTheLibraryToTheDispatchOfNuevo()
        {
            var dynamic = WorkspaceHostSource.BodyOf(WorkspaceHostSource.Code("src/RackCad.Plugin/VentanaRackCad/RackCadRackDoorCommands.cs"),
                @"public\s+void\s+InsertNewRack\s*\(");
            Assert.Matches(@"DynamicDraftOutcome\.NewRackRequest\([^;]*pending\.Views,\s*pending\.SourceDesignJson\)", dynamic);

            var pushBack = WorkspaceHostSource.BodyOf(WorkspaceHostSource.Code("src/RackCad.Plugin/VentanaRackCad/RackCadPushBackDoorCommands.cs"),
                @"public\s+void\s+InsertNewPushBack\s*\(");
            Assert.Matches(@"PushBackDoorInsertion\.NewRackRequest\([^;]*write\.InsertViews,\s*write\.SourceDesignJson\)", pushBack);

            var cantilever = WorkspaceHostSource.BodyOf(WorkspaceHostSource.Code("src/RackCad.Plugin/VentanaRackCad/RackCadCantileverDoorCommands.cs"),
                @"public\s+void\s+InsertNewCantilever\s*\(");
            Assert.Matches(@"CantileverDraftOutcome\.NewRackRequest\([^;]*views,\s*write\.SourceDesignJson\)", cantilever);

            // The new Dinámico that waits keeps it (Push Back and Cantilever keep the whole write of the window).
            var port = WorkspaceHostSource.BodyOf(WorkspaceHostSource.Code("src/RackCad.Plugin/VentanaRackCad/RackCadRackDoorPort.cs"),
                @"public\s+RackCadClassicResult\s+RequestDynamicUpdate\s*\(");
            Assert.Matches(@"_pendingNew\s*=\s*new\s+PendingWrite\([^;]*SourceDesignJson:\s*write\.SourceDesignJson\)", port);
        }
    }
}
