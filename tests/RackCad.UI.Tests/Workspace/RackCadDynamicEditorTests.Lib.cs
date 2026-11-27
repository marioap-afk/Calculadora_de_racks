#nullable enable
using System.Linq;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Shared;
using RackCad.Application.VentanaRackCad;
using RackCad.UI.VentanaRackCad;
using Xunit;

namespace RackCad.UI.Tests
{
    public partial class RackCadDynamicEditorTests
    {
        // ------------------------------------------------------------------ T6 lib: the design of the library at the insertion (UT-S10, T6lib-S3)

        private static RackProject DynamicOfTheLibrary()
        {
            var snapshot = DynamicDraftEditor.CreateNew(UiSupport.LoadCatalogSafe()).SnapshotForWrite()!;
            return LibraryUnknownFields.Library(RackProject.ForDynamic(snapshot.Design, snapshot.System), "DynamicSystem");
        }

        private static readonly RackViewAddress Planta = RackViewAddress.Whole(DimensionViewKind.Planta);

        // Review of 39addd8f, point 3: inserted, a Dinámico of the library is a rack of the drawing; its «Actualizar» redraws its blocks in
        // place, each with its own metadata, and sends no source.
        [Fact]
        public void ADinamicoOfTheLibraryOnceInsertedIsUpdatedWithoutTheDesignOfTheLibrary()
        {
            var rig = new Rig(new ModalPortDouble());
            Assert.True(rig.Window.OpenDesignAsNewRack(DynamicOfTheLibrary(), "Dinamico futuro"));
            var editor = rig.Window.NewDynamicEditor!;
            editor.InsertWith(Planta);
            var write = Assert.Single(rig.Door.Writes);
            Assert.True(write.NewRack);
            Assert.NotNull(write.SourceDesignJson);

            rig.Door.Written(new RackDesignWritten(write.RequestId, write.RackId, true, "AutoCAD insertó el rack nuevo «Dinamico futuro» con 1 vista.",
                "Dinamico futuro"));
            Assert.False(editor.IsNewRack);
            Write(editor, v => v.Essentials, "weight", "1300");
            editor.RequestWrite();

            Assert.Equal(2, rig.Door.Writes.Count);
            Assert.False(rig.Door.Writes[1].NewRack);
            Assert.Null(rig.Door.Writes[1].SourceDesignJson);
        }

        // T6lib-S3 (the Owner, 2026-11-27: «No, la copia es nueva»): the copy of a draft of the library made with «Duplicar» is a new rack,
        // inserted without the design of the library; the original still carries it.
        [Fact]
        public void TheCopyOfADinamicoOfTheLibraryIsANewRackAndIsInsertedWithoutItsDesign()
        {
            var rig = new Rig(new ModalPortDouble());
            Assert.True(rig.Window.OpenDesignAsNewRack(DynamicOfTheLibrary(), "Dinamico futuro"));
            var original = rig.Window.NewDynamicEditor!;
            var copy = rig.Window.DuplicateDraft(rig.Window.Drafts.Single());
            Assert.True(copy.Opened, copy.Reason);
            var copied = rig.Window.NewDynamicEditor!;
            Assert.NotSame(original, copied);

            copied.InsertWith(Planta);
            var write = Assert.Single(rig.Door.Writes);
            Assert.True(write.NewRack);
            Assert.Equal(copied.RackId, write.RackId);
            Assert.Null(write.SourceDesignJson);
            rig.Door.Written(new RackDesignWritten(write.RequestId, write.RackId, true, "AutoCAD insertó el rack nuevo «Dinámico 1» con 1 vista.", "Dinámico 1"));

            rig.Window.OpenDraft(rig.Window.Drafts.Single());
            Assert.Same(original, rig.Window.NewDynamicEditor);
            original.InsertWith(Planta);
            Assert.Equal(2, rig.Door.Writes.Count);
            Assert.Equal(original.RackId, rig.Door.Writes[1].RackId);
            Assert.NotNull(rig.Door.Writes[1].SourceDesignJson);
        }
    }
}
