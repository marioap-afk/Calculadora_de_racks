#nullable enable
using System.Linq;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Shared;
using RackCad.UI.Editor;
using RackCad.UI.Systems.Cantilever;
using RackCad.UI.VentanaRackCad;
using Xunit;

namespace RackCad.UI.Tests
{
    public partial class RackCadCantileverWindowTests
    {
        // ------------------------------------------------------------------ T6 lib: the unknown fields at the insertion (UT-S10)

        [Fact]
        public void ACantileverLineOfTheLibraryInsertedFromTheWindowTakesTheUnknownFieldsOfItsFileToTheDrawingAsTheClassicEditor()
        {
            var rig = new Rig();
            var file = LibraryUnknownFields.Inject(new RackProjectStore().Serialize(RackProject.ForCantilever(LineOfTheDrawing())), "Cantilever");
            var library = new RackProjectStore().Deserialize(file);
            Assert.True(rig.Window.OpenDesignAsNewRack(library, "Linea futura"));
            var editor = Assert.IsType<CantileverEditorViewModel>(rig.Window.NewCantileverEditor);

            Assert.NotNull(editor.InsertWith(RackViewAddress.Whole(DimensionViewKind.Frontal), RackViewAddress.Station(0)));
            var write = Assert.Single(rig.Door.CantileverWrites);
            Assert.True(write.NewRack);
            Assert.NotNull(write.SourceDesignJson);

            // What RACKCANTILEVERNUEVO hands to the dispatch of «Nuevo», and the inner design of its views: the same as the classic editor's.
            var request = Assert.IsType<CantileverInsertionRequest>(CantileverDraftOutcome.NewRackRequest(write.RackId, write.Name, write.Design,
                write.Line, write.InsertViews!.ToList(), write.SourceDesignJson));
            var inner = LibraryUnknownFields.InnerDesign(RackProject.ForCantilever(write.Design), request.SourceProject);
            Assert.Equal(LibraryUnknownFields.InnerDesign(RackProject.ForCantilever(write.Design), library), inner);
            LibraryUnknownFields.AssertCarried(inner, "Cantilever", payloadKeepsItsFields: true);

            // A new line that does not come from the library carries no source.
            rig.Door.Start(write.RequestId);
            rig.Door.AnswerLastWrite(true, "AutoCAD insertó el rack nuevo «Linea futura» con 2 vistas.", "Linea futura");
            Assert.False(editor.IsNewRack);
            rig.Window.StartNewCantilever();
            var fresh = rig.Window.NewCantileverEditor!;
            FillEssentials(fresh);
            Assert.NotNull(fresh.InsertWith(RackViewAddress.Whole(DimensionViewKind.Frontal)));
            Assert.Equal(2, rig.Door.CantileverWrites.Count);
            Assert.True(rig.Door.CantileverWrites[1].NewRack);
            Assert.Null(rig.Door.CantileverWrites[1].SourceDesignJson);
        }
    }
}
