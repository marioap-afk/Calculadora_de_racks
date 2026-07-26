using System;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Geometry;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// PB-004, regla vigente tras el rechazo de la validación round 1: NINGUNO de los dos largueros de extremo se
    /// desplaza fuera de su troquel. El POSTERIOR es el ancla y conserva la elevación que le dio el resolver; el de
    /// ENTRADA/SALIDA se deriva de él por la pendiente nominal y se ajusta a su propio troquel. La cama es la línea
    /// entre los dos contactos reales.
    ///
    /// Las reglas anteriores —el bajo clavado al mate del riel y el posterior arrastrado fuera de la retícula— quedan
    /// DEROGADAS, y las pruebas que las fijaban se retiraron de aquí. La regla vigente se fija en
    /// <see cref="PushBackBedAnchorTests"/>; lo que queda en este archivo son los invariantes que siguen valiendo.
    /// </summary>
    public class PushBackBedTangencyTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static DynamicRackDesign BaseStructure() => new DynamicRackDesign
        {
            Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
            PalletsDeep = 4,
            LoadLevels = 3,
            FirstLevelHeight = 6.0,
            BeamDepth = 4.0
        };

        private static PushBackSystem System(RackCatalog catalog)
            => new PushBackResolver(catalog).Resolve(new PushBackDesign { Structure = BaseStructure() });

        private static string HighBeamId(PushBackSystem system)
            => string.IsNullOrWhiteSpace(system.HighEndBeamCatalogId)
                ? PushBackDefaults.HighEndBeamCatalogId
                : system.HighEndBeamCatalogId;

        // ---- (1) the low beam is bolted: TROQUEL_CAMA == TROQUEL_IN, in WORLD coordinates ----

        [Fact]
        public void RearBeam_ContactEdge_IsThePointTheBedLandsOn_ChosenByGeometryNotByAFixedSide()
        {
            var catalog = Catalog;
            var system = System(catalog);
            var beamId = HighBeamId(system);

            var left = catalog.ConnectionLayout.FindConnectionLayout(
                beamId, PushBackDefaults.HighEndBeamLeftBedMatePoint, PushBackDefaults.HighEndBeamView);
            var right = catalog.ConnectionLayout.FindConnectionLayout(
                beamId, PushBackDefaults.HighEndBeamRightBedMatePoint, PushBackDefaults.HighEndBeamView);
            Assert.NotNull(left);
            Assert.NotNull(right);

            // The bed line rises toward +X, so contact happens at the edge with the GREATER world X.
            var straight = PushBackLoadBeamGeometry.RearBeamTangencyPointWorld(catalog, beamId, 100.0, 50.0, mirroredX: false);
            Assert.True(straight.HasValue);
            Assert.Equal(100.0 + Math.Max(left.LocalX, right.LocalX), straight.Value.X, 9);

            // Mirrored, the local X flips, so the correct edge becomes the OTHER one — chosen by geometry.
            var mirrored = PushBackLoadBeamGeometry.RearBeamTangencyPointWorld(catalog, beamId, 100.0, 50.0, mirroredX: true);
            Assert.True(mirrored.HasValue);
            Assert.Equal(100.0 - Math.Min(left.LocalX, right.LocalX), mirrored.Value.X, 9);
        }

        [Fact]
        public void NoFallbackToTheRawOrigin_WhenTheBlockHasNoMeasuredContactFace()
        {
            var catalog = Catalog;
            // No measured edges => no anchor at all, never the insertion point.
            Assert.Null(PushBackLoadBeamGeometry.RearBeamTangencyPointWorld(catalog, "PIEZA_SIN_PUNTOS", 10.0, 20.0, false));
            Assert.Null(PushBackLoadBeamGeometry.RearBeamTangencyPointWorld(null, "CUALQUIERA", 10.0, 20.0, false));
            Assert.Null(PushBackLoadBeamGeometry.BedTangencyPointLocal(catalog, "PIEZA_SIN_PUNTO_DE_CAMA"));
            Assert.Null(PushBackLoadBeamGeometry.BedTangencyPointLocal(null, DynamicRackDefaults.InOutBeamCatalogId));

            // Y sin contacto medido la autoridad no resuelve esa celda en vez de inventarle una elevación.
            Assert.DoesNotContain(
                PushBackElevations.Resolve(null, catalog, null),
                entry => true);
        }

        // ---- the bed itself is authority and must come out bit-identical ----

        [Fact]
        public void TheBed_StaysBitIdentical_OriginSlopeAxisAndLength()
        {
            var catalog = Catalog;
            var system = System(catalog);
            var front = system.Structure.Fronts[0];

            var before = PushBackFlowBedGeometry.Resolve(system, catalog, front);
            var low = PushBackLoadBeamGeometry.LowBeams(system, catalog, front);
            var high = PushBackLoadBeamGeometry.HighBeams(system, catalog, 0, front);
            var after = PushBackFlowBedGeometry.Resolve(system, catalog, front);
            Assert.NotEmpty(low);
            Assert.NotEmpty(high);
            Assert.Equal(before.Count, after.Count);

            for (var i = 0; i < before.Count; i++)
            {
                Assert.Equal(before[i].ExitMate.X, after[i].ExitMate.X, 12);
                Assert.Equal(before[i].ExitMate.Y, after[i].ExitMate.Y, 12);
                Assert.Equal(before[i].HighMate.X, after[i].HighMate.X, 12);
                Assert.Equal(before[i].HighMate.Y, after[i].HighMate.Y, 12);
                Assert.Equal(before[i].RailOrigin.X, after[i].RailOrigin.X, 12);
                Assert.Equal(before[i].RailOrigin.Y, after[i].RailOrigin.Y, 12);
                Assert.Equal(before[i].AngleRadians, after[i].AngleRadians, 12);
                Assert.Equal(before[i].Length, after[i].Length, 12);
            }

            // The commercial bed length is the FULL span, with no 4" deduction.
            var front0 = system.Structure.Fronts[0];
            Assert.Equal(front0.EndX - front0.StartX, PushBackFlowBedGeometry.ResolveBedLength(system, front0), 9);
        }

        [Fact]
        public void TheBedSlope_IsUnchanged_AndTheShiftIsVerticalOnly()
        {
            var catalog = Catalog;
            var system = System(catalog);
            var front = system.Structure.Fronts[0];

            var axes = PushBackFlowBedGeometry.Resolve(system, catalog, front);
            var slopes = axes.Select(a => Math.Round(a.Rise / a.Run, 9)).Distinct().ToList();
            Assert.Single(slopes);   // one lane slope for every level

            // Every drawn beam keeps its placement X: the correction never moves anything horizontally.
            var placements = DynamicLoadBeamGeometry.Placements(system.Structure, front).ToList();
            foreach (var beam in PushBackLoadBeamGeometry.LowBeams(system, catalog, front)
                .Concat(PushBackLoadBeamGeometry.HighBeams(system, catalog, 0, front)))
            {
                Assert.Contains(placements, p => Math.Abs(p.X - beam.Insertion.X) < 1e-12);
            }
        }
    }
}
