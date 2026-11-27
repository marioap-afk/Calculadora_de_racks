#nullable enable
using RackCad.Application.Persistence;
using RackCad.UI.VentanaRackCad;
using Xunit;

namespace RackCad.UI.Tests
{
    public partial class RackCadCamaWindowTests
    {
        // ------------------------------------------------------------------ T6 lib: the copy of a draft of the library (T6lib-S3)

        // T6lib-S3 (the Owner, 2026-11-27: «No, la copia es nueva»): the copy of a cama of the library made with «Duplicar» is a new cama,
        // inserted without the document of the library (CM-S13 is for the draft «Abrir» opened); the original still carries it.
        [Fact]
        public void TheCopyOfACamaOfTheLibraryIsANewCamaAndIsInsertedWithoutItsDocument()
        {
            using var rig = new Rig();
            var library = LibraryUnknownFields.Library(RackProject.ForCama(Bed(120.0)), "FlowBed");
            Assert.True(rig.Window.OpenDesignAsNewRack(library, "Cama futura"));
            var original = Assert.IsType<CamaEditorViewModel>(rig.Window.NewCamaEditor);
            var copy = rig.Window.DuplicateDraft(Assert.Single(rig.Window.Drafts));
            Assert.True(copy.Opened, copy.Reason);
            var copied = Assert.IsType<CamaEditorViewModel>(rig.Window.NewCamaEditor);
            Assert.NotSame(original, copied);

            copied.RequestWrite();
            var write = Assert.Single(rig.Door.CamaWrites);
            Assert.True(write.NewRack);
            Assert.Equal(copied.RackId, write.RackId);
            Assert.Null(write.SourceDesignJson);
            rig.Door.AnswerLastWrite(true, "AutoCAD insertó la cama nueva «Cama 1».", "Cama 1");

            rig.Window.OpenDraft(Assert.Single(rig.Window.Drafts));
            Assert.Same(original, rig.Window.NewCamaEditor);
            original.RequestWrite();
            Assert.Equal(2, rig.Door.CamaWrites.Count);
            Assert.Equal(original.RackId, rig.Door.CamaWrites[1].RackId);
            Assert.Contains(LibraryUnknownFields.PayloadField, rig.Door.CamaWrites[1].SourceDesignJson);
        }
    }
}
