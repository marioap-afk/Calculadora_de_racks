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

            // Inserted, it is a line of the drawing (review of 39addd8f, point 3): «Actualizar» redraws its blocks in place, each with its own
            // metadata, and sends no source.
            rig.Door.Start(write.RequestId);
            rig.Door.AnswerLastWrite(true, "AutoCAD insertó el rack nuevo «Linea futura» con 2 vistas.", "Linea futura");
            Assert.False(editor.IsNewRack);
            Write(editor.StructureItems, "Spacing", "120");
            editor.RequestWrite();
            Assert.Equal(2, rig.Door.CantileverWrites.Count);
            Assert.False(rig.Door.CantileverWrites[1].NewRack);
            Assert.Null(rig.Door.CantileverWrites[1].SourceDesignJson);
            rig.Door.Start(rig.Door.CantileverWrites[1].RequestId);
            rig.Door.AnswerLastWrite(true, "AutoCAD redibujó 2 vistas del rack con el borrador.");

            // A new line that does not come from the library carries no source.
            rig.Window.StartNewCantilever();
            var fresh = rig.Window.NewCantileverEditor!;
            FillEssentials(fresh);
            Assert.NotNull(fresh.InsertWith(RackViewAddress.Whole(DimensionViewKind.Frontal)));
            Assert.Equal(3, rig.Door.CantileverWrites.Count);
            Assert.True(rig.Door.CantileverWrites[2].NewRack);
            Assert.Null(rig.Door.CantileverWrites[2].SourceDesignJson);
        }

        // T6lib-S3 (the Owner, 2026-11-27: «No, la copia es nueva»): the copy of a draft of the library made with «Duplicar» is a new line,
        // inserted without the design of the library; the original still carries it.
        [Fact]
        public void TheCopyOfACantileverLineOfTheLibraryIsANewLineAndIsInsertedWithoutItsDesign()
        {
            var rig = new Rig();
            var library = LibraryUnknownFields.Library(RackProject.ForCantilever(LineOfTheDrawing()), "Cantilever");
            Assert.True(rig.Window.OpenDesignAsNewRack(library, "Linea futura"));
            var original = Assert.IsType<CantileverEditorViewModel>(rig.Window.NewCantileverEditor);
            var copy = rig.Window.DuplicateDraft(Assert.Single(rig.Window.Drafts));
            Assert.True(copy.Opened, copy.Reason);
            var copied = Assert.IsType<CantileverEditorViewModel>(rig.Window.NewCantileverEditor);
            Assert.NotSame(original, copied);

            Assert.NotNull(copied.InsertWith(RackViewAddress.Whole(DimensionViewKind.Frontal)));
            var write = Assert.Single(rig.Door.CantileverWrites);
            Assert.True(write.NewRack);
            Assert.Equal(copied.RackId, write.RackId);
            Assert.Null(write.SourceDesignJson);
            rig.Door.Start(write.RequestId);
            rig.Door.AnswerLastWrite(true, "AutoCAD insertó el rack nuevo «Cantilever 2» con 1 vista.", "Cantilever 2");

            rig.Window.OpenDraft(Assert.Single(rig.Window.Drafts));
            Assert.Same(original, rig.Window.NewCantileverEditor);
            Assert.NotNull(original.InsertWith(RackViewAddress.Whole(DimensionViewKind.Frontal)));
            Assert.Equal(2, rig.Door.CantileverWrites.Count);
            Assert.Equal(original.RackId, rig.Door.CantileverWrites[1].RackId);
            Assert.NotNull(rig.Door.CantileverWrites[1].SourceDesignJson);
        }
    }
}
