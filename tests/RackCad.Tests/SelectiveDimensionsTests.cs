using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Headers;
using RackCad.Application.Persistence;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>Frontal dimensioning (cotas): the detail levels emit the expected set, and the detail round-trips.</summary>
    public class SelectiveDimensionsTests
    {
        private const string PostId = "POSTE_OMEGA_ATORNILLABLE_CON_TROQUEL_GOTA_DE_AGUA";
        private const string BeamId = "LARGUERO_ESCALON_CAL14_3_REMACHES";
        private const int Bays = 3;
        private const int Levels = 4; // 4 pallets, ground on floor, no floor beam → 3 beam rows

        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static SelectivePalletDesign Design(DimensionDetail detail)
        {
            var design = new SelectivePalletDesign { PostId = PostId, PostPeralte = 3.0, PalletDepth = 48.0, Dimensions = detail };
            for (var b = 0; b < Bays; b++)
            {
                var bay = new SelectiveBayDesign();
                for (var l = 0; l < Levels; l++)
                {
                    bay.Levels.Add(new SelectiveCell { Pallet = new Tarima { Frente = 42.0, Alto = 60.0 }, PalletCount = 2, BeamId = BeamId, BeamPeralte = 4.0 });
                }

                design.Bays.Add(bay);
            }

            return design;
        }

        private static SelectiveRackSystem Resolve(DimensionDetail detail)
            => new SelectiveGeometryResolver().Resolve(Design(detail), Catalog);

        private static List<HeaderBlockInstance> Dims(DimensionDetail detail)
        {
            // Go through the SAME per-fondo projection the insert/redraw path uses (InsertSelectiveFrontal →
            // FondoSystemView), so a projection that drops the Dimensions flag is caught here.
            var fondoView = SelectiveDepthLayout.FondoSystemView(Resolve(detail), 0);
            return new SelectiveFrontalBuilder().Build(fondoView, Catalog)
                .Where(i => i.Role == HeaderBlockRole.Dimension).ToList();
        }

        private static bool IsHorizontal(HeaderBlockInstance d) => Math.Abs(d.Insertion.Y - d.ConnectionAnchor.Y) < 1e-6;
        private static bool IsVertical(HeaderBlockInstance d) => Math.Abs(d.Insertion.X - d.ConnectionAnchor.X) < 1e-6;
        private static double Span(HeaderBlockInstance d) => Math.Abs(d.ConnectionAnchor.X - d.Insertion.X);

        [Fact]
        public void None_EmitsNoDimensions()
        {
            Assert.Empty(Dims(DimensionDetail.None));
        }

        [Fact]
        public void Minimal_EmitsOverallHeightAndWidthOnly()
        {
            var dims = Dims(DimensionDetail.Minimal);

            Assert.Equal(2, dims.Count);
            Assert.Single(dims.Where(IsHorizontal)); // ancho total
            var vertical = Assert.Single(dims.Where(IsVertical)); // alto total
            Assert.Equal(0.0, Math.Min(vertical.Insertion.Y, vertical.ConnectionAnchor.Y), 3); // desde el piso
            Assert.True(dims.All(d => d.DimensionOffset < 0.0)); // cotas fuera de la geometría (abajo/izquierda)
        }

        [Fact]
        public void Standard_AddsPerFrenteWidthsAndLevelSeparations()
        {
            var beamRows = Levels - 1; // ground pallet on floor, no floor beam
            var dims = Dims(DimensionDetail.Standard);

            // Horizontal: one per frente + the overall width.
            Assert.Equal(Bays + 1, dims.Count(IsHorizontal));
            // Vertical: floor→L1, L1→L2, … (beamRows of them) + the overall height.
            Assert.Equal(beamRows + 1, dims.Count(IsVertical));
        }

        [Fact]
        public void Standard_PerFrenteCota_MeasuresLargueroCutLength_NotPostSpacing()
        {
            var system = Resolve(DimensionDetail.Standard);
            var beamLength = system.Bays[0].BeamLength; // uniform bays → same cut length
            var horizontal = Dims(DimensionDetail.Standard).Where(IsHorizontal).ToList();

            // Bays cotas measure the larguero cut length; exactly one (the overall width) is wider (post-to-post).
            var larguero = horizontal.Where(d => Math.Abs(Span(d) - beamLength) < 1e-6).ToList();
            Assert.Equal(Bays, larguero.Count);
            var overall = Assert.Single(horizontal.Where(d => Span(d) > beamLength + 1e-6));
            Assert.True(Span(overall) > beamLength, "el ancho total (post a post) debe ser mayor que el largo de corte del larguero");

            // The larguero cota sits at its larguero's height (not the floor); the overall width sits on the floor.
            Assert.All(larguero, d => Assert.True(d.Insertion.Y > 0.0, "la cota del larguero va a la altura del larguero, no en el piso"));
            Assert.Equal(0.0, overall.Insertion.Y, 3);
        }

        [Fact]
        public void Detailed_AddsPerLevelElevations()
        {
            var beamRows = Levels - 1;
            var standard = Dims(DimensionDetail.Standard).Count;

            Assert.Equal(standard + beamRows, Dims(DimensionDetail.Detailed).Count);
        }

        [Fact]
        public void Detailed_ElevationCotasDoNotShareOneLine()
        {
            var beamRows = Levels - 1;
            var vertical = Dims(DimensionDetail.Detailed).Where(IsVertical).ToList();

            // Vertical dimension lines = the separation chain (one shared line) + overall height (one) + one PER
            // elevation cota (each stepped to its own line). Overlapping elevations would collapse these.
            var distinctLines = vertical.Select(d => Math.Round(d.DimensionOffset, 3)).Distinct().Count();
            Assert.Equal(beamRows + 2, distinctLines);
        }

        [Fact]
        public void Dimensions_RoundTripThroughDocument()
        {
            var design = Design(DimensionDetail.Detailed);
            design.DimensionStyle = "MI_ESTILO";
            var document = SelectivePalletDesignDocument.From(design, "id-1", "Rack A");
            var store = new SelectivePalletDesignStore();

            var restored = store.Deserialize(store.Serialize(document)).ToDomain();

            Assert.Equal(DimensionDetail.Detailed, restored.Dimensions);
            Assert.Equal("MI_ESTILO", restored.DimensionStyle); // el estilo de cota elegido sobrevive el round-trip
        }

        [Fact]
        public void LegacyDocument_WithoutDimensionsField_DefaultsToNone()
        {
            // A document that never carried the field (null) must load as None, not throw or draw stray cotas.
            var legacy = new SelectivePalletDesignDocument { Dimensions = null };

            Assert.Equal(DimensionDetail.None, legacy.ToDomain().Dimensions);
        }
    }
}
