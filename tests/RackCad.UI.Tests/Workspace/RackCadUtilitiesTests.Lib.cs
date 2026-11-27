#nullable enable
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using RackCad.Application.Persistence;
using RackCad.Application.RackFrames;
using RackCad.Domain.Systems.Shared;
using RackCad.UI.Editor;
using RackCad.UI.Systems.Dynamic;
using RackCad.UI.VentanaRackCad;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// T6 lib (I-64, sixth batch; UT-S10 and CM-S13): a library file written by a newer build of the same major, and what the drawing keeps of
    /// it when a design of the library is inserted. The classic editor opened from the library hands its project to the dispatch of «Nuevo»
    /// (<c>SourceProjectToInsert</c>), and <c>RackViewBatchProducts</c> writes the inner design of every new view as
    /// <see cref="InnerDesign"/> does; the window has to write exactly the same.
    /// </summary>
    internal static class LibraryUnknownFields
    {
        public const string RootField = "FuturoRaiz";
        public const string PayloadField = "FuturoAnidado";
        public const string RootVersion = "2.7";
        public const string PayloadVersion = "1.9";

        /// <summary>
        /// The JSON of a project with a field this build does not know and a newer minor version in its wrapper, and a field this build does not
        /// know in its payload <paramref name="payload"/> (with a newer minor version where the payload has a version).
        /// </summary>
        public static string Inject(string projectJson, string payload)
        {
            var root = JsonNode.Parse(projectJson)!.AsObject();
            root["SchemaVersion"] = RootVersion;
            root[RootField] = new JsonObject { ["nivel"] = 1, ["texto"] = "de una version futura" };
            var inner = root[payload]!.AsObject();
            inner[PayloadField] = 42;
            if (inner.ContainsKey("SchemaVersion")) inner["SchemaVersion"] = PayloadVersion;
            return root.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
        }

        /// <summary>The inner design of a new view of the rack (<c>RackViewBatchProducts.Project</c>): the project with the metadata of its source.</summary>
        public static string InnerDesign(RackProject project, RackProject? source)
            => new RackProjectStore().Serialize(project.WithSourceMetadataFrom(source));

        /// <summary>
        /// The inner design keeps the wrapper of the file (its unknown field and its version) and, where I-11 keeps them, the unknown field and the
        /// version of the payload; not more (the inner design of a Dinámico keeps none of its own).
        /// </summary>
        public static void AssertCarried(string inner, string payload, bool payloadKeepsItsFields)
        {
            var root = JsonNode.Parse(inner)!.AsObject();
            Assert.Equal(RootVersion, (string?)root["SchemaVersion"]);
            Assert.Equal(1, (int?)root[RootField]?["nivel"]);
            var nested = root[payload]!.AsObject();
            Assert.Equal(payloadKeepsItsFields, nested.ContainsKey(PayloadField));
            if (payloadKeepsItsFields) Assert.Equal(PayloadVersion, (string?)nested["SchemaVersion"]);
        }
    }

    public partial class RackCadUtilitiesTests
    {
        // ------------------------------------------------------------------ T6 lib: the unknown fields at the insertion (UT-S10)

        [Fact]
        public void ADinamicoOfTheLibraryInsertedFromTheWindowTakesTheUnknownFieldsOfItsFileToTheDrawingAsTheClassicEditor()
        {
            using var rig = new Rig();
            var path = rig.File("Dinamico futuro");
            SaveDynamic(path);
            System.IO.File.WriteAllText(path, LibraryUnknownFields.Inject(System.IO.File.ReadAllText(path), "DynamicSystem"));
            var library = new RackProjectStore().Load(path); // what the classic editor opened from the library loads (EditorModules)

            rig.Window.ShowPage(RackCadWindowPage.Biblioteca);
            Select(rig, "Dinamico futuro");
            rig.Library.OpenSelected();
            var editor = rig.Window.NewDynamicEditor;
            Assert.NotNull(editor);
            InsertFirstView(editor!);
            var write = Assert.Single(rig.Door.Writes);
            Assert.True(write.NewRack);
            Assert.NotNull(write.SourceDesignJson);

            // What RACKDINAMICONUEVO hands to the dispatch of «Nuevo», and the inner design of its views: the same as the classic editor's.
            var request = Assert.IsType<DynamicInsertionRequest>(DynamicDraftOutcome.NewRackRequest(write.RackId, write.Name, write.Design,
                write.System, write.InsertViews, write.SourceDesignJson));
            var inner = LibraryUnknownFields.InnerDesign(RackProject.ForDynamic(write.Design), request.SourceProject);
            Assert.Equal(LibraryUnknownFields.InnerDesign(RackProject.ForDynamic(write.Design), library), inner);
            LibraryUnknownFields.AssertCarried(inner, "DynamicSystem", payloadKeepsItsFields: false);
        }

        [Fact]
        public void ANewDinamicoThatDoesNotComeFromTheLibraryCarriesNoSourceToItsInsertion()
        {
            using var rig = new Rig();
            rig.Window.StartNewDynamic();
            InsertFirstView(rig.Window.NewDynamicEditor!);

            var write = Assert.Single(rig.Door.Writes);
            Assert.True(write.NewRack);
            Assert.Null(write.SourceDesignJson);
            var request = Assert.IsType<DynamicInsertionRequest>(DynamicDraftOutcome.NewRackRequest(write.RackId, write.Name, write.Design,
                write.System, write.InsertViews, write.SourceDesignJson));
            Assert.Null(request.SourceProject);
        }

        // A cabecera of the library is not opened in the window: it goes to its classic editor through the numbered door, and that editor hands
        // the project of the file to the dispatch of «Nuevo» (EditorModules: HeaderInsertionRequest with sourceProject), as before.
        [Fact]
        public void ACabeceraOfTheLibraryOpensInItsClassicEditorThroughTheNumberedDoor()
        {
            using var rig = new Rig();
            var path = rig.File("Cabecera futura");
            Projects.Save(RackProject.ForSelective(new HardcodedStandardRackFrameService().CreateDefault()), path);
            rig.Window.ShowPage(RackCadWindowPage.Biblioteca);
            var row = rig.Library.Rows.Single(r => r.Path == path);
            Assert.Equal(RackSystemKind.Selective, row.Kind);
            rig.Library.SelectPath(path);
            Assert.True(rig.Library.SelectionOpensClassic);

            rig.Library.OpenSelected();

            Assert.Equal(path, Assert.Single(rig.Classic.LibraryOpens).Path);
            Assert.Empty(rig.Window.Drafts);
        }
    }
}
