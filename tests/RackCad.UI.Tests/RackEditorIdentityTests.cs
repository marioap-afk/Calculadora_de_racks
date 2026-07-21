using RackCad.UI.Editor;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>The rack identity helper (lazy GUID + name) the shell centralizes (I-15). Pure: no WPF.</summary>
    public sealed class RackEditorIdentityTests
    {
        [Fact]
        public void New_HasNoIdYet()
        {
            var identity = new RackEditorIdentity();

            Assert.Null(identity.Id);
            Assert.False(identity.HasId);
            Assert.Null(identity.Name);
        }

        [Fact]
        public void EnsureId_MintsOnceThenStaysStable()
        {
            var mints = 0;
            var identity = new RackEditorIdentity(() => "id-" + (++mints));

            var first = identity.EnsureId();
            var second = identity.EnsureId();

            Assert.Equal("id-1", first);
            Assert.Equal("id-1", second); // stable: re-inserting another view reuses the id
            Assert.Equal(1, mints);       // minted exactly once
            Assert.True(identity.HasId);
        }

        [Fact]
        public void EnsureId_DefaultFactory_IsAGuid()
        {
            var identity = new RackEditorIdentity();

            var id = identity.EnsureId();

            Assert.True(System.Guid.TryParse(id, out _));
        }

        [Fact]
        public void SetName_TrimsValueButKeepsBlankAsIs()
        {
            var identity = new RackEditorIdentity();

            identity.SetName("  Rack 5  ");
            Assert.Equal("Rack 5", identity.Name);

            identity.SetName(null);
            Assert.Null(identity.Name);
        }

        [Fact]
        public void Adopt_TakesExistingIdentity()
        {
            var mints = 0;
            var identity = new RackEditorIdentity(() => "fresh-" + (++mints));

            identity.Adopt("EXISTING-GUID", " Bodega A ");

            Assert.Equal("EXISTING-GUID", identity.Id);
            Assert.Equal("Bodega A", identity.Name);
            Assert.Equal("EXISTING-GUID", identity.EnsureId()); // adopted id is kept; nothing minted
            Assert.Equal(0, mints);
        }

        [Fact]
        public void Adopt_BlankId_LeavesIdUnmintedSoLibraryTemplateGetsFreshGuidOnInsert()
        {
            var identity = new RackEditorIdentity(() => "minted");

            identity.Adopt("   ", "plantilla");

            Assert.Null(identity.Id);
            Assert.False(identity.HasId);
            Assert.Equal("minted", identity.EnsureId()); // a LoadForNew-style open mints a NEW id on insert
        }
    }
}
