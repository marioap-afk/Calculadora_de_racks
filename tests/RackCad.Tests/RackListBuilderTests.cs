using System.Linq;
using RackCad.Application.Persistence;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>RACKLISTA's pure fold: envelopes grouped by GUID into display rows (name/kind/views).</summary>
    public class RackListBuilderTests
    {
        private static RackEmbedDocument Embed(string id, string kind, string view = null, int section = -1, string name = null) =>
            new RackEmbedDocument { Id = id, Kind = kind, View = view, Section = section, Name = name };

        [Fact]
        public void Build_GroupsViewBlocksOfTheSameRackIntoOneEntry()
        {
            var entries = RackListBuilder.Build(new[]
            {
                Embed("id-1", RackEmbedDocument.KindSelective, RackEmbedDocument.ViewFrontal, name: "Rack A"),
                Embed("id-1", RackEmbedDocument.KindSelective, RackEmbedDocument.ViewLateral, section: 0, name: "Rack A"),
                Embed("id-2", RackEmbedDocument.KindCabecera, RackEmbedDocument.ViewLateral, name: "Rack B")
            });

            Assert.Equal(2, entries.Count);
            Assert.Equal(new[] { "id-1", "id-2" }, entries.Select(entry => entry.Id).ToArray());
        }

        [Fact]
        public void Build_IgnoresEnvelopesWithoutIdOrKind()
        {
            var entries = RackListBuilder.Build(new[]
            {
                Embed(null, RackEmbedDocument.KindSelective),
                Embed("  ", RackEmbedDocument.KindSelective),
                Embed("id-1", null),
                null,
                Embed("id-2", RackEmbedDocument.KindCama, name: "Cama 1")
            });

            var entry = Assert.Single(entries);
            Assert.Equal("id-2", entry.Id);
        }

        [Fact]
        public void Build_NameIsFirstNonEmptyOfTheGroup_OrFallback()
        {
            var entries = RackListBuilder.Build(new[]
            {
                Embed("id-1", RackEmbedDocument.KindSelective, RackEmbedDocument.ViewFrontal, name: "  "),
                Embed("id-1", RackEmbedDocument.KindSelective, RackEmbedDocument.ViewPlanta, name: "Rack A"),
                Embed("id-2", RackEmbedDocument.KindDynamic)
            });

            Assert.Equal("(sin nombre)", entries[0].Name); // id-2 sorts first: "(sin nombre)" < "Rack A"
            Assert.Equal("Rack A", entries[1].Name);
        }

        [Fact]
        public void Build_MapsKindLabelsToSpanish()
        {
            var entries = RackListBuilder.Build(new[]
            {
                Embed("id-1", RackEmbedDocument.KindSelective, name: "A"),
                Embed("id-2", RackEmbedDocument.KindDynamic, name: "B"),
                Embed("id-3", RackEmbedDocument.KindCabecera, name: "C"),
                Embed("id-4", RackEmbedDocument.KindCama, name: "D")
            });

            Assert.Equal(new[] { "Selectivo", "Sistema dinámico", "Cabecera", "Cama de rodamiento" },
                entries.Select(entry => entry.KindLabel).ToArray());
        }

        [Fact]
        public void Build_CountsSelectiveLateralsByDistinctSection()
        {
            var entries = RackListBuilder.Build(new[]
            {
                Embed("id-1", RackEmbedDocument.KindSelective, RackEmbedDocument.ViewPlanta, name: "Rack A"),
                Embed("id-1", RackEmbedDocument.KindSelective, RackEmbedDocument.ViewLateral, section: 0),
                Embed("id-1", RackEmbedDocument.KindSelective, RackEmbedDocument.ViewLateral, section: 1),
                Embed("id-1", RackEmbedDocument.KindSelective, RackEmbedDocument.ViewLateral, section: 2),
                Embed("id-1", RackEmbedDocument.KindSelective, RackEmbedDocument.ViewFrontal)
            });

            var entry = Assert.Single(entries);
            Assert.Equal("frontal, lateral ×3, planta", entry.ViewsLabel);
            Assert.Equal(5, entry.ViewCount);
        }

        [Fact]
        public void Build_SingleViewHasNoMultiplier()
        {
            var entries = RackListBuilder.Build(new[]
            {
                Embed("id-1", RackEmbedDocument.KindCabecera, RackEmbedDocument.ViewLateral, name: "Marco"),
                Embed("id-1", RackEmbedDocument.KindCabecera, RackEmbedDocument.ViewPlanta, name: "Marco")
            });

            var entry = Assert.Single(entries);
            Assert.Equal("lateral, planta", entry.ViewsLabel);
            Assert.Equal(2, entry.ViewCount);
        }

        [Fact]
        public void Build_NullViewCountsAsLateral_LegacyDynamicAndCama()
        {
            var entries = RackListBuilder.Build(new[]
            {
                Embed("id-1", RackEmbedDocument.KindDynamic, view: null, name: "Dinamico legacy")
            });

            var entry = Assert.Single(entries);
            Assert.Equal("lateral", entry.ViewsLabel);
            Assert.Equal(1, entry.ViewCount);
        }

        [Fact]
        public void Build_OrdersByNameThenById()
        {
            var entries = RackListBuilder.Build(new[]
            {
                Embed("id-9", RackEmbedDocument.KindSelective, name: "Rack B"),
                Embed("id-2", RackEmbedDocument.KindSelective, name: "rack a"),
                Embed("id-1", RackEmbedDocument.KindSelective, name: "Rack A")
            });

            Assert.Equal(new[] { "id-1", "id-2", "id-9" }, entries.Select(entry => entry.Id).ToArray());
        }

        [Fact]
        public void Build_EmptyOrNullInput_ReturnsEmpty()
        {
            Assert.Empty(RackListBuilder.Build(null));
            Assert.Empty(RackListBuilder.Build(System.Array.Empty<RackEmbedDocument>()));
        }

        // Characterization for fix/kind-handler-missing-errors F5: RACKLISTA's KindLabel is display-only — an
        // unrecognized kind is shown verbatim in the listing, never turned into an error, and there is no partial
        // result or inconsistent identity to guard (unlike RACKBOMTOTAL / the copy restamp). It lives in Application
        // and cannot consult the Plugin's KindHandlerRegistry (Application must not depend on the Plugin), and its
        // labels deliberately differ from the Plugin's BOM labels, so it is left intact (evaluated, out of scope).
        [Fact]
        public void KindLabel_UnknownKind_ReturnsRawKind_DisplayOnly()
        {
            Assert.Equal("noSuchKind", RackListBuilder.KindLabel("noSuchKind"));
            Assert.Equal(string.Empty, RackListBuilder.KindLabel(null));
            Assert.Equal("Sistema dinámico", RackListBuilder.KindLabel(RackEmbedDocument.KindDynamic));
        }
    }
}
