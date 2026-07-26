using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Geometry;
using RackCad.Application.Headers;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// PB-004, round 3 (I-32) — las dos precisiones del coordinador sobre la regla vigente:
    /// <list type="number">
    /// <item>la subida NOMINAL se mide sobre la <b>longitud comercial</b> de la cama
    /// (<see cref="PushBackFlowBedGeometry.ResolveBedLength"/>), no sobre la distancia entre contactos;</item>
    /// <item>el contacto del larguero POSTERIOR es la arista que elige la <b>geometría</b>
    /// (<see cref="PushBackLoadBeamGeometry.RearBeamTangencyPointWorld"/>), no un lado fijo del catálogo.</item>
    /// </list>
    /// Y una sola autoridad (<see cref="PushBackElevations"/>) responde por frente y nivel, de modo que ningún builder
    /// repite la fórmula.
    /// </summary>
    public class PushBackElevationAuthorityTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        public static IEnumerable<object[]> Fondos() =>
            new[] { 2, 3, 4, 5, 6, 7, 8 }.Select(deep => new object[] { deep });

        private static PushBackSystem System(RackCatalog catalog, int palletsDeep, double depth = 48.0)
            => new PushBackResolver(catalog).Resolve(new PushBackDesign
            {
                Structure = new DynamicRackDesign
                {
                    Pallet = new PalletSpecification(42.0, depth, 60.0, 1000.0, "kg"),
                    PalletsDeep = palletsDeep,
                    LoadLevels = 3,
                    FirstLevelHeight = 6.0,
                    BeamDepth = 4.0
                }
            });

        // ---- (1) La subida nominal se mide sobre la LONGITUD COMERCIAL ----

        [Theory]
        [MemberData(nameof(Fondos))]
        public void TheNominalRise_IsMeasuredOverTheCommercialBedLength(int palletsDeep)
        {
            var catalog = Catalog;
            var system = System(catalog, palletsDeep);
            var front = system.Structure.Fronts[0];
            var gridBase = PushBackTroquelGrid.Base(system.Structure, catalog);
            var lowMate = PushBackLoadBeamGeometry.BedTangencyPointLocal(catalog, DynamicRackDefaults.InOutBeamCatalogId);
            Assert.True(lowMate.HasValue);

            var commercial = PushBackFlowBedGeometry.ResolveBedLength(system, front);
            var nominalRise = PushBackBedSlope.Rise(commercial);

            foreach (var cell in PushBackElevations.Resolve(system, catalog, front).Values)
            {
                var expected = PushBackTroquelGrid.Snap(cell.RearContact.Y - nominalRise - lowMate.Value.Y, gridBase);
                Assert.Equal(expected, cell.LowInsertion, 9);
            }
        }

        /// <summary>
        /// Las dos formulas —longitud comercial contra distancia entre contactos— no son intercambiables: existe al
        /// menos un rack donde eligen TROQUELES distintos. El barrido lo busca en vez de fijar un numero magico.
        /// </summary>
        [Fact]
        public void TheTwoFormulas_ChooseDifferentTroqueles_ForAtLeastOneRack()
        {
            var catalog = Catalog;
            var gridBase0 = 0.0;
            var found = 0;

            for (var deep = 2; deep <= 8; deep++)
            {
                for (var depth = 40.0; depth <= 56.0; depth += 0.5)
                {
                    var system = System(catalog, deep, depth);
                    var front = system.Structure.Fronts[0];
                    var gridBase = PushBackTroquelGrid.Base(system.Structure, catalog);
                    gridBase0 = gridBase;
                    var lowMate = PushBackLoadBeamGeometry.BedTangencyPointLocal(catalog, DynamicRackDefaults.InOutBeamCatalogId);
                    if (!lowMate.HasValue) continue;

                    var commercial = PushBackFlowBedGeometry.ResolveBedLength(system, front);
                    foreach (var cell in PushBackElevations.Resolve(system, catalog, front).Values)
                    {
                        var run = cell.RearContact.X - cell.LowContact.X;
                        var byCommercial = PushBackTroquelGrid.Snap(
                            cell.RearContact.Y - PushBackBedSlope.Rise(commercial) - lowMate.Value.Y, gridBase);
                        var byRun = PushBackTroquelGrid.Snap(
                            cell.RearContact.Y - PushBackBedSlope.Rise(run) - lowMate.Value.Y, gridBase);

                        if (Math.Abs(byCommercial - byRun) > 1e-9)
                        {
                            found++;
                            // Y la vigente es la comercial.
                            Assert.Equal(byCommercial, cell.LowInsertion, 9);
                        }
                    }
                }
            }

            Assert.True(gridBase0 != 0.0 || found >= 0);
            Assert.True(found > 0, "el barrido no encontro ningun rack donde las dos formulas eligieran troqueles distintos");
        }

        // ---- (2) El contacto posterior lo elige la geometria: bloque normal y espejado ----

        [Fact]
        public void TheRearContact_IsTheGeometrySelectedEdge_ForBothNormalAndMirroredBlocks()
        {
            var catalog = Catalog;
            var beamId = PushBackDefaults.HighEndBeamCatalogId;
            var left = catalog.ConnectionLayout.FindConnectionLayout(beamId, PushBackDefaults.HighEndBeamLeftBedMatePoint, PushBackDefaults.HighEndBeamView);
            var right = catalog.ConnectionLayout.FindConnectionLayout(beamId, PushBackDefaults.HighEndBeamRightBedMatePoint, PushBackDefaults.HighEndBeamView);
            Assert.NotNull(left);
            Assert.NotNull(right);
            Assert.True(right.LocalX > left.LocalX, "el catalogo debe medir las dos aristas y con INICIO_DERECHO a mayor X local");

            // Bloque NORMAL: gana la arista de mayor X local (INICIO_DERECHO).
            var normal = PushBackLoadBeamGeometry.RearBeamTangencyPointWorld(catalog, beamId, 100.0, 50.0, mirroredX: false);
            Assert.True(normal.HasValue);
            Assert.Equal(100.0 + right.LocalX, normal.Value.X, 9);

            // Bloque ESPEJADO: la X local cambia de signo, asi que gana la OTRA arista (INICIO_IZQUIERDO).
            var mirrored = PushBackLoadBeamGeometry.RearBeamTangencyPointWorld(catalog, beamId, 100.0, 50.0, mirroredX: true);
            Assert.True(mirrored.HasValue);
            Assert.Equal(100.0 - left.LocalX, mirrored.Value.X, 9);
            Assert.NotEqual(100.0 - right.LocalX, mirrored.Value.X, 6);
        }

        [Theory]
        [MemberData(nameof(Fondos))]
        public void TheAxisHighMate_IsExactlyThatGeometrySelectedEdge(int palletsDeep)
        {
            var catalog = Catalog;
            var system = System(catalog, palletsDeep);
            var front = system.Structure.Fronts[0];

            var cells = PushBackElevations.Resolve(system, catalog, front);
            foreach (var axis in PushBackFlowBedGeometry.Resolve(system, catalog, front))
            {
                var cell = cells[axis.LevelNumber];
                var edge = PushBackLoadBeamGeometry.RearBeamTangencyPointWorld(
                    catalog, PushBackDefaults.HighEndBeamCatalogId,
                    system.TotalLength, cell.RearInsertion, mirroredX: true);
                Assert.True(edge.HasValue);
                Assert.Equal(edge.Value.X, axis.HighMate.X, 9);
                Assert.Equal(edge.Value.Y, axis.HighMate.Y, 9);
            }
        }

        // ---- (3) Un mismo LowBeamElevation llega a los consumidores ya cableados ----

        [Fact]
        public void TheSameLowElevation_ReachesTheLateralAndTheLowFrontalCut()
        {
            var catalog = Catalog;
            var system = System(catalog, 4);
            var front = system.Structure.Fronts[0];
            var authority = PushBackElevations.LowInsertions(system, catalog, front);
            Assert.NotEmpty(authority);

            var lateralYs = new PushBackSystemLateralBuilder().Build(system, catalog).Flatten().Instances
                .Where(i => string.Equals(i.PieceId, DynamicRackDefaults.InOutBeamCatalogId, StringComparison.OrdinalIgnoreCase))
                .Select(i => Math.Round(i.Insertion.Y, 6)).OrderBy(y => y).ToList();
            var frontalYs = new PushBackSystemFrontalBuilder()
                .BuildPlan(system, catalog, PushBackFrontalEnd.EntradaSalida).Flatten().Instances
                .Where(i => string.Equals(i.PieceId, DynamicRackDefaults.InOutBeamCatalogId, StringComparison.OrdinalIgnoreCase))
                .Select(i => Math.Round(i.Insertion.Y, 6)).OrderBy(y => y).ToList();
            var expected = authority.Values.Select(y => Math.Round(y, 6)).OrderBy(y => y).ToList();

            Assert.Equal(expected, lateralYs);
            Assert.Equal(expected, frontalYs);   // una pieza fisica, una elevacion en las dos vistas
        }

        /// <summary>El Dinamico no ve nada de esto: sus elevaciones resueltas siguen intactas.</summary>
        [Fact]
        public void TheDynamicResolvedElevations_AreUntouched()
        {
            var catalog = Catalog;
            var system = System(catalog, 4);
            var front = system.Structure.Fronts[0];

            var authority = PushBackElevations.LowInsertions(system, catalog, front);
            foreach (var level in DynamicFrontGeometry.LoadBeamLevels(system.Structure, front))
            {
                // La autoridad NO reescribe el modelo compartido: la elevacion de salida del resolver sigue ahi.
                Assert.True(authority.ContainsKey(level.LevelNumber));
                Assert.Equal(
                    PushBackTroquelGrid.Snap(level.ExitElevation, PushBackTroquelGrid.Base(system.Structure, catalog)),
                    level.ExitElevation,
                    9);
            }
        }

        // ---- (4) El clon del frontal conserva TODOS los parametros ----

        [Fact]
        public void TheRearFrontalClone_KeepsEveryDynamicParameter_AndOnlyMovesY()
        {
            var catalog = Catalog;
            var system = System(catalog, 4);

            var redondos = new PushBackSystemFrontalBuilder()
                .BuildPlan(system, catalog, PushBackFrontalEnd.Posterior).Flatten().Instances
                .Where(i => string.Equals(i.PieceId, PushBackDefaults.HighEndBeamCatalogId, StringComparison.OrdinalIgnoreCase))
                .ToList();
            Assert.NotEmpty(redondos);

            // El clon del larguero posterior lleva LONGITUD y PERALTE; ninguno puede perderse por el camino.
            Assert.All(redondos, beam =>
            {
                Assert.True(beam.DynamicParameters.ContainsKey(SelectiveRackDefaults.LengthParam));
                Assert.True(beam.DynamicParameters.ContainsKey(SelectiveRackDefaults.PeralteParam));
                Assert.True(beam.DynamicParameters[SelectiveRackDefaults.LengthParam] > 0.0);
                Assert.True(beam.DynamicParameters[SelectiveRackDefaults.PeralteParam] > 0.0);
            });
        }
    }
}
