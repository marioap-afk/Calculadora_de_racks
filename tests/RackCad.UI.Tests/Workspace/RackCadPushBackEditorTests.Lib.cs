#nullable enable
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Shared;
using RackCad.UI.Editor;
using RackCad.UI.VentanaRackCad;
using Xunit;

namespace RackCad.UI.Tests
{
    public partial class RackCadPushBackEditorTests
    {
        // ------------------------------------------------------------------ T6 lib: the unknown fields at the insertion (UT-S10)

        [Fact]
        public void APushBackOfTheLibraryInsertedFromTheWindowTakesTheUnknownFieldsOfItsFileToTheDrawingAsTheClassicEditor()
        {
            var rig = new Rig();
            var library = new RackProjectStore().Deserialize(LibraryUnknownFields.Inject(Envelope("pb-03-fondos").Design, "PushBack"));
            Assert.True(rig.Window.OpenDesignAsNewRack(library, "Diseño PB"));
            var editor = Assert.IsType<PushBackEditorViewModel>(rig.Window.NewPushBackEditor);

            editor.InsertView();
            var panel = editor.InsertPanel!;
            panel.Mark(RackViewAddress.Whole(DimensionViewKind.Planta));
            panel.Insert();
            var write = Assert.Single(rig.Door.Writes);
            Assert.True(write.NewRack);
            Assert.NotNull(write.SourceDesignJson);

            // What RACKPUSHBACKNUEVO hands to the dispatch of «Nuevo», and the inner design of its views: the same as the classic editor's,
            // with the wrapper of the file and, where I-11 keeps them, the unknown field and the version of its payload.
            var request = Assert.IsType<PushBackInsertionRequest>(PushBackDoorInsertion.NewRackRequest(write.RackId, write.Name, write.Design,
                write.System, write.InsertViews, write.SourceDesignJson));
            var inner = LibraryUnknownFields.InnerDesign(RackProject.ForPushBack(write.Design), request.SourceProject);
            Assert.Equal(LibraryUnknownFields.InnerDesign(RackProject.ForPushBack(write.Design), library), inner);
            LibraryUnknownFields.AssertCarried(inner, "PushBack", payloadKeepsItsFields: true);

            // Inserted, it is a Push Back of the drawing: «Actualizar» redraws its blocks in place, each with its own metadata, and sends no source.
            rig.Door.AnswerLastWrite(true, "AutoCAD insertó el rack nuevo «Diseño PB» con 1 vista.", "Diseño PB");
            Assert.False(editor.IsNewRack);
            Change(editor);
            editor.RequestWrite();
            Assert.Equal(2, rig.Door.Writes.Count);
            Assert.False(rig.Door.Writes[1].NewRack);
            Assert.Null(rig.Door.Writes[1].SourceDesignJson);
        }

        // T6lib-S3 (the Owner, 2026-11-27: «No, la copia es nueva»): the copy of a draft of the library made with «Duplicar» is a new rack,
        // inserted without the design of the library; the original still carries it.
        [Fact]
        public void TheCopyOfAPushBackOfTheLibraryIsANewRackAndIsInsertedWithoutItsDesign()
        {
            var rig = new Rig();
            var library = new RackProjectStore().Deserialize(LibraryUnknownFields.Inject(Envelope("pb-03-fondos").Design, "PushBack"));
            Assert.True(rig.Window.OpenDesignAsNewRack(library, "Diseño PB"));
            var original = Assert.IsType<PushBackEditorViewModel>(rig.Window.NewPushBackEditor);
            var copy = rig.Window.DuplicateDraft(Assert.Single(rig.Window.Drafts));
            Assert.True(copy.Opened, copy.Reason);
            var copied = Assert.IsType<PushBackEditorViewModel>(rig.Window.NewPushBackEditor);
            Assert.NotSame(original, copied);

            InsertPlanta(copied);
            var write = Assert.Single(rig.Door.Writes);
            Assert.True(write.NewRack);
            Assert.Equal(copied.RackId, write.RackId);
            Assert.Null(write.SourceDesignJson);
            rig.Door.AnswerLastWrite(true, "AutoCAD insertó el rack nuevo «Push Back 1» con 1 vista.", "Push Back 1");

            rig.Window.OpenDraft(Assert.Single(rig.Window.Drafts));
            Assert.Same(original, rig.Window.NewPushBackEditor);
            InsertPlanta(original);
            Assert.Equal(2, rig.Door.Writes.Count);
            Assert.Equal(original.RackId, rig.Door.Writes[1].RackId);
            Assert.NotNull(rig.Door.Writes[1].SourceDesignJson);
        }

        private static void InsertPlanta(PushBackEditorViewModel editor)
        {
            editor.InsertView();
            var panel = editor.InsertPanel!;
            panel.Mark(RackViewAddress.Whole(DimensionViewKind.Planta));
            panel.Insert();
        }
    }
}
