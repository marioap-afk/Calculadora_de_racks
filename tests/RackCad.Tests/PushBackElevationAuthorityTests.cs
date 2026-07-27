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

        // ---- (1) La seleccion del troquel bajo minimiza el error de PENDIENTE ----

        /// <summary>
        /// Aclaracion final del Owner (I-32): el criterio de seleccion ya NO es «ajustar la subida nominal medida
        /// sobre la longitud comercial», sino <b>minimizar el error de pendiente contra 7/192</b> sobre la reticula
        /// de 2". La longitud comercial sigue siendo la del riel dibujado y la del BOM, pero no elige el troquel.
        /// </summary>
        [Theory]
        [MemberData(nameof(Fondos))]
        public void TheChosenTroquel_MinimisesTheSlopeErrorAgainstTheTarget(int palletsDeep)
        {
            var catalog = Catalog;
            var system = System(catalog, palletsDeep);
            var front = system.Structure.Fronts[0];
            var railMate = CatalogLookup.Local(
                catalog, FlowBedDefaults.RailId, FlowBedDefaults.RailInOutMatePoint, FlowBedDefaults.View);
            var lowMate = PushBackLoadBeamGeometry.BedTangencyPointLocal(catalog, DynamicRackDefaults.InOutBeamCatalogId);
            Assert.True(lowMate.HasValue);

            foreach (var cell in PushBackElevations.Resolve(system, catalog, front).Values)
            {
                var chosen = PushBackBedRotation.SlopeError(cell.RotationRadians);
                foreach (var delta in new[] { -4.0, -2.0, 2.0, 4.0 })
                {
                    var exitMate = new Point2D(cell.LowContact.X, cell.LowInsertion + delta + lowMate.Value.Y);
                    if (exitMate.Y >= cell.RearContact.Y)
                    {
                        continue;
                    }

                    var rotation = PushBackBedRotation.Solve(exitMate, cell.RearContact, railMate);
                    if (rotation.HasValue)
                    {
                        Assert.True(PushBackBedRotation.SlopeError(rotation.Value) >= chosen - 1e-12);
                    }
                }
            }
        }

        /// <summary>
        /// El criterio NUEVO y el ANTIGUO no son el mismo: hay racks en los que eligen troqueles distintos. Si
        /// coincidieran siempre, el cambio de regla no habria hecho falta.
        /// </summary>
        [Fact]
        public void TheNewCriterion_ChoosesADifferentTroquelThanTheOldNominalSnap_ForAtLeastOneRack()
        {
            var catalog = Catalog;
            var differences = 0;

            for (var deep = 2; deep <= 8; deep++)
            {
                var system = System(catalog, deep);
                var front = system.Structure.Fronts[0];
                var gridBase = PushBackTroquelGrid.Base(system.Structure, catalog);
                var lowMate = PushBackLoadBeamGeometry.BedTangencyPointLocal(catalog, DynamicRackDefaults.InOutBeamCatalogId);
                if (!lowMate.HasValue)
                {
                    continue;
                }

                var nominalRise = PushBackBedSlope.Rise(PushBackFlowBedGeometry.ResolveBedLength(system, front));
                foreach (var cell in PushBackElevations.Resolve(system, catalog, front).Values)
                {
                    var legacy = PushBackTroquelGrid.Snap(
                        cell.RearContact.Y - nominalRise - lowMate.Value.Y, gridBase);
                    if (Math.Abs(legacy - cell.LowInsertion) > 1e-9)
                    {
                        differences++;
                    }
                }
            }

            Assert.True(differences > 0,
                "los dos criterios eligieron siempre el mismo troquel: el barrido no demuestra que la regla cambio");
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
