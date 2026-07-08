using RackCad.Application.Persistence;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>The uniform embed envelope (kind + id + name + design) round-trips and tolerates junk.</summary>
    public class RackEmbedDocumentTests
    {
        [Fact]
        public void RoundTrips_KindIdNameAndDesign()
        {
            var store = new RackEmbedStore();
            var document = new RackEmbedDocument
            {
                Kind = RackEmbedDocument.KindDynamic,
                Id = "id-1",
                Name = "Rack A",
                Design = "{\"any\":1}"
            };

            var back = store.Deserialize(store.Serialize(document));

            Assert.Equal(RackEmbedDocument.KindDynamic, back.Kind);
            Assert.Equal("id-1", back.Id);
            Assert.Equal("Rack A", back.Name);
            Assert.Equal("{\"any\":1}", back.Design);
        }

        [Fact]
        public void Deserialize_JunkOrEmpty_ReturnsNull()
        {
            var store = new RackEmbedStore();
            Assert.Null(store.Deserialize("no soy json"));
            Assert.Null(store.Deserialize(""));
            Assert.Null(store.Deserialize(null));
        }
    }
}
