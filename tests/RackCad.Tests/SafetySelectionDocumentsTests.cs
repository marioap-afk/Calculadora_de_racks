using RackCad.Application.Persistence;
using RackCad.Domain.Systems;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>Direct tests of the per-subtype safety DTOs (I-22, E7 round 2): each family type round-trips its config
    /// via From/ToDomain, applies its exact legacy fallback when read from a document that predates a field, and
    /// flattens/unflattens against the flat <see cref="SafetySelectionDocument"/> without changing the wire format. The
    /// selective and dynamic JSON characterizations stay untouched (SystemKindPersistenceCharacterizationTests,
    /// RackProjectStoreTests).</summary>
    public class SafetySelectionDocumentsTests
    {
        // ---- TOPE ----

        [Fact]
        public void Tope_From_ToDomain_RoundTripsEveryField()
        {
            var config = new SelectiveTopeConfig { Shared = false, Fondo = 2, Saque = 7.5, Frontal = true };
            config.OffCells.Add(new SelectiveGridCell { Frente = 1, Level = 3 });

            var restored = TopeSelectionDocument.From(config).ToDomain();

            Assert.False(restored.Shared);
            Assert.Equal(2, restored.Fondo);
            Assert.Equal(7.5, restored.Saque);
            Assert.True(restored.Frontal);
            Assert.False(restored.At(1, 3));
        }

        [Fact]
        public void Tope_ReadFrom_LegacyDocument_AppliesFallbacks()
        {
            var config = TopeSelectionDocument.ReadFrom(new SafetySelectionDocument()).ToDomain();
            Assert.True(config.Shared);
            Assert.Equal(-1, config.Fondo);
            Assert.Equal(SelectiveSafetyDefaults.TopeSaque, config.Saque);
            Assert.False(config.Frontal);
            Assert.Empty(config.OffCells);

            // A persisted non-positive SAQUE still collapses to the domain default (the <= 0 guard).
            var guarded = TopeSelectionDocument.ReadFrom(new SafetySelectionDocument { TopeSaque = 0.0 }).ToDomain();
            Assert.Equal(SelectiveSafetyDefaults.TopeSaque, guarded.Saque);
        }

        [Fact]
        public void Tope_WriteInto_FlattensToTheDocumentFields()
        {
            var doc = new SafetySelectionDocument();
            var config = new SelectiveTopeConfig { Shared = false, Fondo = 1, Saque = 5.0, Frontal = true };
            config.OffCells.Add(new SelectiveGridCell { Frente = 0, Level = 1 });

            TopeSelectionDocument.From(config).WriteInto(doc);

            Assert.False(doc.TopeShared);
            Assert.Equal(1, doc.TopeFondo);
            Assert.Equal(5.0, doc.TopeSaque);
            Assert.True(doc.TopeFrontal);
            Assert.Equal(0, Assert.Single(doc.TopeOffCells).Frente);
        }

        // ---- DESVIADOR ----

        [Fact]
        public void Desviador_From_ToDomain_RoundTrips_AndLegacyFallsBack()
        {
            var config = new SelectiveDesviadorConfig { Longitud = 24.0, PrimerNivelAltura = 20.0 };
            config.OffCells.Add(new SelectiveGridCell { Frente = 2, Level = 0 });
            var restored = DesviadorSelectionDocument.From(config).ToDomain();
            Assert.Equal(24.0, restored.Longitud);
            Assert.Equal(20.0, restored.PrimerNivelAltura);
            Assert.False(restored.At(2, 0));

            // Legacy / non-positive -> domain defaults (18 / 18), matching the pre-decomposition <= 0 guard.
            var legacy = DesviadorSelectionDocument.ReadFrom(new SafetySelectionDocument { DesviadorLongitud = 0.0, DesviadorPrimerNivelAltura = -3.0 }).ToDomain();
            Assert.Equal(SelectiveSafetyDefaults.DesviadorLongitud, legacy.Longitud);
            Assert.Equal(SelectiveSafetyDefaults.DesviadorPrimerNivelAltura, legacy.PrimerNivelAltura);
        }

        [Fact]
        public void Desviador_WriteInto_FlattensToTheDocumentFields()
        {
            var doc = new SafetySelectionDocument();
            DesviadorSelectionDocument.From(new SelectiveDesviadorConfig { Longitud = 22.0, PrimerNivelAltura = 16.0 }).WriteInto(doc);
            Assert.Equal(22.0, doc.DesviadorLongitud);
            Assert.Equal(16.0, doc.DesviadorPrimerNivelAltura);
        }

        // ---- PARRILLA ----

        [Fact]
        public void Parrilla_From_ToDomain_RoundTrips_AndLegacyPreservesPresentZero()
        {
            var config = new SelectiveParrillaConfig { Frontal = false, Lateral = true, Frente = 36.0, Cantidad = 2 };
            config.OffCells.Add(new SelectiveGridCell { Frente = 0, Level = 1 });
            var restored = ParrillaSelectionDocument.From(config).ToDomain();
            Assert.False(restored.Frontal);
            Assert.True(restored.Lateral);
            Assert.Equal(36.0, restored.Frente);
            Assert.Equal(2, restored.Cantidad);
            Assert.False(restored.At(0, 1));

            // Legacy (all null) -> Frontal/Lateral default true, Frente/Cantidad 0; a present 0 is preserved (plain ??).
            var legacy = ParrillaSelectionDocument.ReadFrom(new SafetySelectionDocument()).ToDomain();
            Assert.True(legacy.Frontal);
            Assert.True(legacy.Lateral);
            Assert.Equal(0.0, legacy.Frente);
            Assert.Equal(0, legacy.Cantidad);
            var presentZero = ParrillaSelectionDocument.ReadFrom(new SafetySelectionDocument { ParrillaFrente = 0.0 }).ToDomain();
            Assert.Equal(0.0, presentZero.Frente);
        }

        [Fact]
        public void Parrilla_WriteInto_FlattensToTheDocumentFields()
        {
            var doc = new SafetySelectionDocument();
            ParrillaSelectionDocument.From(new SelectiveParrillaConfig { Frontal = false, Lateral = true, Frente = 42.0, Cantidad = 3 }).WriteInto(doc);
            Assert.False(doc.ParrillaFrontal);
            Assert.True(doc.ParrillaLateral);
            Assert.Equal(42.0, doc.ParrillaFrente);
            Assert.Equal(3, doc.ParrillaCantidad);
        }

        // ---- DEFENSA ----

        [Fact]
        public void Defensa_From_ToDomain_ClampsLengths_AndSkipsNegativePosts()
        {
            var config = new SelectiveDefensaConfig();
            config.Posts.Add(new SafetyPostDefense { PostIndex = 1, ExitLength = 12.0, EntranceLength = 36.0 });
            var restored = DefensaSelectionDocument.From(config).ToDomain();
            var post = Assert.Single(restored.Posts);
            Assert.Equal(1, post.PostIndex);
            Assert.Equal(12.0, post.ExitLength);
            Assert.Equal(36.0, post.EntranceLength);

            // Legacy/tolerant read: negative post skipped; null lengths -> 0; negative length clamped to 0.
            var doc = new SafetySelectionDocument
            {
                DefensaPosts = new System.Collections.Generic.List<PostDefenseDocument>
                {
                    new PostDefenseDocument { PostIndex = -1, ExitLength = 5.0, EntranceLength = 5.0 },
                    new PostDefenseDocument { PostIndex = 0, ExitLength = null, EntranceLength = -8.0 }
                }
            };
            var legacy = DefensaSelectionDocument.ReadFrom(doc).ToDomain();
            var kept = Assert.Single(legacy.Posts);
            Assert.Equal(0, kept.PostIndex);
            Assert.Equal(0.0, kept.ExitLength);
            Assert.Equal(0.0, kept.EntranceLength);
        }

        // ---- GUIA ----

        [Fact]
        public void Guia_From_ToDomain_RoundTripsOffCells_AndLegacyIsEmpty()
        {
            var config = new SelectiveGuiaConfig();
            config.OffCells.Add(new SelectiveGridCell { Frente = 1, Level = 0 });
            var restored = GuiaSelectionDocument.From(config).ToDomain();
            Assert.False(restored.At(1, 0));

            Assert.Empty(GuiaSelectionDocument.ReadFrom(new SafetySelectionDocument()).ToDomain().OffCells);
        }

        // ---- Composition: the flat document From()/ToDomain() drives every family DTO ----

        [Fact]
        public void SafetySelectionDocument_From_FlattensEveryFamily_AndRoundTrips()
        {
            var selection = new SelectiveSafetySelection { ElementId = "E", Quantity = 2, Side = SafetySide.Right };
            selection.Tope.Frontal = true;
            selection.Desviador.Longitud = 20.0;
            selection.Parrilla.Frente = 44.0;
            selection.Guia.OffCells.Add(new SelectiveGridCell { Frente = 0, Level = 0 });
            selection.Defensa.Posts.Add(new SafetyPostDefense { PostIndex = 0, ExitLength = 12.0, EntranceLength = 24.0 });

            var document = SafetySelectionDocument.From(selection);

            // The flat properties are populated by the family DTOs (no nested JSON).
            Assert.True(document.TopeFrontal);
            Assert.Equal(20.0, document.DesviadorLongitud);
            Assert.Equal(44.0, document.ParrillaFrente);
            Assert.Single(document.GuiaEntradaOffCells);
            Assert.Single(document.DefensaPosts);

            var restored = document.ToDomain();
            Assert.Equal(SafetySide.Right, restored.Side);
            Assert.True(restored.Tope.Frontal);
            Assert.Equal(20.0, restored.Desviador.Longitud);
            Assert.Equal(44.0, restored.Parrilla.Frente);
            Assert.False(restored.Guia.At(0, 0));
            Assert.Equal(24.0, Assert.Single(restored.Defensa.Posts).EntranceLength);
        }
    }
}
