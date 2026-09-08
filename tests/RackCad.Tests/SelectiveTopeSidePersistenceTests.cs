using System.Linq;
using RackCad.Application.Persistence;
using RackCad.Domain.Systems.Selective;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-46 (ID12) — what the tope's SIDE must survive on disk. I-46 makes <c>Ninguno</c> a reachable, meaningful
    /// choice, so the persisted side stops being "always one of three" and has to round-trip all four values exactly.
    /// It does so with NO new field, NO DTO change and NO schema bump: <c>Side</c> was already an <c>int?</c> on
    /// <see cref="SelectiveSafetySelectionDocument"/> and <c>SafetyDocumentMapping.ToSafetySide</c> already accepted
    /// <c>0 = None</c> — what used to destroy the value was the editor, not the store.
    /// <para>
    /// <c>TopeShared</c> and <c>TopeFondo</c> are NOT re-tested here: they already round-trip in
    /// <see cref="SelectiveSafetyConfigTests.RoundTrip_EachFamilyConfig_PreservedThroughTheStore"/> and
    /// <see cref="SafetySelectionDocumentsTests.Tope_From_ToDomain_RoundTripsEveryField"/>. They ride along in the
    /// theory below only to prove the side does not disturb them.
    /// </para>
    /// </summary>
    public class SelectiveTopeSidePersistenceTests
    {
        private const string TopeId = TestCatalogIds.Safety.Stops.Post;

        private static SelectivePalletDesign DesignWithTope(SafetySide side, bool shared, int fondo)
        {
            var design = new SelectivePalletDesign { PostId = "P", PostPeralte = 3.0, DepthCount = 2 };
            design.Bays.Add(new SelectiveBayDesign());
            design.SafetySelections.Add(new SelectiveSafetySelection
            {
                ElementId = TopeId, Quantity = 1, Side = side, TopeShared = shared, TopeFondo = fondo, TopeSaque = 6.0
            });
            return design;
        }

        private static SelectivePalletDesign RoundTrip(SelectivePalletDesign design)
        {
            var store = new SelectivePalletDesignStore();
            var document = SelectivePalletDesignDocument.From(design, "id-46", "Rack I-46");
            return store.Deserialize(store.Serialize(document)).ToDomain();
        }

        // ---- All four sides survive the store byte for byte, and they do not disturb Shared/Fondo ----
        [Theory]
        [InlineData(SafetySide.None)]
        [InlineData(SafetySide.Left)]
        [InlineData(SafetySide.Right)]
        [InlineData(SafetySide.Both)]
        public void EverySide_RoundTripsThroughTheStore(SafetySide side)
        {
            var restored = Assert.Single(RoundTrip(DesignWithTope(side, shared: false, fondo: 1)).SafetySelections);

            Assert.Equal(side, restored.Side);
            Assert.False(restored.TopeShared);   // the side does not disturb its neighbours in the same document
            Assert.Equal(1, restored.TopeFondo);
        }

        // ---- The wire format is the ORDINAL, and the ordinals are the historic ones ----
        [Theory]
        [InlineData(SafetySide.None, 0)]
        [InlineData(SafetySide.Left, 1)]
        [InlineData(SafetySide.Right, 2)]
        [InlineData(SafetySide.Both, 3)]
        public void PersistedSide_IsTheHistoricOrdinal(SafetySide side, int ordinal)
        {
            Assert.Equal(ordinal, (int)side); // the enum itself is unchanged

            var document = SafetySelectionDocument.From(new SelectiveSafetySelection { ElementId = TopeId, Side = side });

            Assert.Equal(ordinal, document.Side);                       // ...and that is what reaches the document
            Assert.Equal(side, document.ToDomain().Side);               // ...and it reads back as itself
        }

        // ---- A document written before the field existed still means "Ambas", exactly as it always did ----
        [Fact]
        public void LegacyDocument_WithoutSide_KeepsTheHistoricBothFallback()
        {
            var legacy = new SafetySelectionDocument { ElementId = TopeId, Side = null };

            Assert.Equal(SafetySide.Both, legacy.ToDomain().Side);
        }

        // ---- ...and an out-of-range value is still coerced to Both rather than to the new None ----
        [Theory]
        [InlineData(-1)]
        [InlineData(4)]
        public void OutOfRangeSide_StillFallsBackToBoth_NotToNone(int stored)
        {
            var document = new SafetySelectionDocument { ElementId = TopeId, Side = stored };

            Assert.Equal(SafetySide.Both, document.ToDomain().Side);
        }

        // ---- No schema moved: the side is expressible in the format that already existed ----
        [Theory]
        [InlineData(SafetySide.None)]
        [InlineData(SafetySide.Left)]
        [InlineData(SafetySide.Right)]
        [InlineData(SafetySide.Both)]
        public void SchemaVersion_IsUntouchedByTheSide(SafetySide side)
        {
            var document = SelectivePalletDesignDocument.From(DesignWithTope(side, shared: true, fondo: -1), "id", "R");

            Assert.Equal("1.0", SelectivePalletDesignDocument.CurrentSchemaVersion);
            Assert.Equal(SelectivePalletDesignDocument.CurrentSchemaVersion, document.SchemaVersion);

            var store = new SelectivePalletDesignStore();
            var reread = store.Deserialize(store.Serialize(document));
            Assert.Equal(SelectivePalletDesignDocument.CurrentSchemaVersion, reread.SchemaVersion);
        }

        // ---- The saved side is what the resolver then places: the disk value drives the physical result ----
        [Theory]
        [InlineData(SafetySide.None, 0)]
        [InlineData(SafetySide.Left, 1)]
        [InlineData(SafetySide.Right, 1)]
        [InlineData(SafetySide.Both, 2)]
        public void ReopenedSide_DrivesThePhysicalResult(SafetySide side, int expectedSpots)
        {
            // TopeFondo = 1 on a 2-fondo rack: the LAST fondo, so the span degenerates and TopeShared is inert.
            var restored = RoundTrip(DesignWithTope(side, shared: true, fondo: 1));
            var selection = Assert.Single(restored.SafetySelections);

            Assert.Equal(expectedSpots, RackCad.Application.Systems.Selective.SelectiveSafetyPlacement
                .TopeSpots(selection, fondoCount: 2).Count());
        }
    }
}
