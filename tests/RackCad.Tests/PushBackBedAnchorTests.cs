using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Geometry;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.PushBack;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.FlowBed;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// PB-004, regla SUSTITUIDA por el Owner tras rechazar la validación round 1.
    ///
    /// La subida de 7/16" por pie es un <b>objetivo NOMINAL</b>, no la subida final literal. Por frente y nivel:
    /// <list type="number">
    /// <item>el larguero POSTERIOR queda conectado a un TROQUEL válido;</item>
    /// <item>desde su punto físico de contacto se calcula el punto teórico bajo con la pendiente nominal;</item>
    /// <item>ese punto se convierte a la elevación teórica de inserción del larguero de ENTRADA/SALIDA teniendo en
    /// cuenta su <c>TROQUEL_CAMA</c> local;</item>
    /// <item>esa conexión se ajusta al TROQUEL válido más cercano;</item>
    /// <item>la cama se traza entre los DOS contactos físicos REALES;</item>
    /// <item>la pendiente final es la RESULTANTE del ajuste y no tiene por qué ser exactamente la nominal.</item>
    /// </list>
    ///
    /// La versión anterior hacía lo contrario —anclaba el bajo y bajaba el posterior FUERA de la retícula para que
    /// tocara una línea teórica—, y por eso el Owner la rechazó. Estas pruebas no fijan ningún resultado final ni
    /// dependen de un rack de 204": recorren varios spans y el caso límite de redondeo entre dos troqueles.
    /// </summary>
    public class PushBackBedAnchorTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        /// <summary>Varios racks de profundidad distinta, para no fijar un solo span.</summary>
        public static IEnumerable<object[]> Spans() =>
            new[] { 3, 4, 5, 6, 7, 8 }.Select(deep => new object[] { deep });

        private static PushBackSystem System(RackCatalog catalog, int palletsDeep, double firstLevelHeight = 6.0)
            => new PushBackResolver(catalog).Resolve(new PushBackDesign
            {
                Structure = new DynamicRackDesign
                {
                    Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                    PalletsDeep = palletsDeep,
                    LoadLevels = 3,
                    FirstLevelHeight = firstLevelHeight,
                    BeamDepth = 4.0
                }
            });

        private static double GridBase(PushBackSystem system, RackCatalog catalog)
            => PushBackTroquelGrid.Base(system.Structure, catalog);

        private static bool OnGrid(double value, double gridBase)
        {
            var steps = (value - gridBase) / SelectiveRackDefaults.TroquelPaso;
            return Math.Abs(steps - Math.Round(steps)) < 1e-6;
        }

        private static double CamaLocalY(RackCatalog catalog)
            => catalog.ConnectionLayout.FindConnectionLayout(
                DynamicRackDefaults.InOutBeamCatalogId,
                DynamicRackDefaults.InOutBeamBedMatePoint,
                DynamicRackDefaults.InOutBeamView).LocalY;

        // ---- 1 y 4: los DOS largueros caen en troquel ----

        [Theory]
        [MemberData(nameof(Spans))]
        public void BothEndBeams_LandOnAValidTroquel(int palletsDeep)
        {
            var catalog = Catalog;
            var system = System(catalog, palletsDeep);
            var front = system.Structure.Fronts[0];
            var gridBase = GridBase(system, catalog);

            var low = PushBackLoadBeamGeometry.LowBeams(system, catalog, front);
            var high = PushBackLoadBeamGeometry.HighBeams(system, catalog, 0, front);
            Assert.NotEmpty(low);
            Assert.NotEmpty(high);

            Assert.All(high, beam => Assert.True(
                OnGrid(beam.Insertion.Y, gridBase),
                FormattableString.Invariant($"el larguero POSTERIOR quedó fuera de troquel en Y={beam.Insertion.Y:0.####}")));
            Assert.All(low, beam => Assert.True(
                OnGrid(beam.Insertion.Y, gridBase),
                FormattableString.Invariant($"el larguero de ENTRADA/SALIDA quedó fuera de troquel en Y={beam.Insertion.Y:0.####}")));
        }

        /// <summary>El posterior conserva EXACTAMENTE la elevación que el resolver ya había ajustado: es el ancla.</summary>
        [Theory]
        [MemberData(nameof(Spans))]
        public void TheRearBeam_KeepsItsResolvedTroquelElevation(int palletsDeep)
        {
            var catalog = Catalog;
            var system = System(catalog, palletsDeep);
            var front = system.Structure.Fronts[0];

            var levels = DynamicFrontGeometry.LoadBeamLevels(system.Structure, front);
            var high = PushBackLoadBeamGeometry.HighBeams(system, catalog, 0, front).OrderBy(b => b.Insertion.Y).ToList();
            var expected = levels.OrderBy(l => l.LevelNumber).Select(l => l.EntranceElevation).ToList();

            Assert.Equal(expected.Count, high.Count);
            for (var i = 0; i < expected.Count; i++)
            {
                Assert.Equal(expected[i], high[i].Insertion.Y, 9);
            }
        }

        // ---- 2, 3 y 4: el bajo se DERIVA del posterior y luego se ajusta ----

        /// <summary>
        /// El larguero bajo se elige sobre la RETICULA, y el criterio es la PENDIENTE.
        ///
        /// Aclaracion final del Owner (I-32): antes esta prueba fijaba «derivar la subida nominal desde el contacto
        /// posterior y ajustar al troquel». Ese criterio quedo sustituido por «elegir el troquel cuya rotacion
        /// acerque mas la pendiente a 7/192». Lo que NO cambia, y se sigue comprobando aqui, es que el resultado es
        /// siempre un troquel valido y que el posterior conserva el suyo.
        /// </summary>
        [Theory]
        [MemberData(nameof(Spans))]
        public void TheLowBeam_IsChosenOnTheGrid_ByTheSlopeCriterion(int palletsDeep)
        {
            var catalog = Catalog;
            var system = System(catalog, palletsDeep);
            var front = system.Structure.Fronts[0];
            var gridBase = PushBackTroquelGrid.Base(system.Structure, catalog);
            var railMate = CatalogLookup.Local(
                catalog, FlowBedDefaults.RailId, FlowBedDefaults.RailInOutMatePoint, FlowBedDefaults.View);
            var lowMate = PushBackLoadBeamGeometry.BedTangencyPointLocal(catalog, DynamicRackDefaults.InOutBeamCatalogId);
            Assert.True(lowMate.HasValue);

            var cells = PushBackElevations.Resolve(system, catalog, front);
            Assert.NotEmpty(cells);

            foreach (var cell in cells.Values)
            {
                // Troquel valido.
                Assert.Equal(PushBackTroquelGrid.Snap(cell.LowInsertion, gridBase), cell.LowInsertion, 9);

                // Y ningun vecino de la reticula acerca mas la pendiente al objetivo.
                var chosen = PushBackBedRotation.SlopeError(cell.RotationRadians);
                foreach (var delta in new[] { -SelectiveRackDefaults.TroquelPaso, SelectiveRackDefaults.TroquelPaso })
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

        // ---- 5: la cama se traza entre contactos FÍSICOS, sin puntos sintéticos ----

        [Theory]
        [MemberData(nameof(Spans))]
        public void TheBed_RunsBetweenTheTwoRealPhysicalContacts(int palletsDeep)
        {
            var catalog = Catalog;
            var system = System(catalog, palletsDeep);
            var front = system.Structure.Fronts[0];
            var camaY = CamaLocalY(catalog);

            var axes = PushBackFlowBedGeometry.Resolve(system, catalog, front);
            var low = PushBackLoadBeamGeometry.LowBeams(system, catalog, front);
            var high = PushBackLoadBeamGeometry.HighBeams(system, catalog, 0, front);

            foreach (var axis in axes)
            {
                // El contacto bajo pertenece a un larguero REAL dibujado.
                Assert.Contains(low, beam => Math.Abs(beam.Insertion.Y + camaY - axis.ExitMate.Y) < 1e-9);

                // Y el alto es el borde que elige la GEOMETRÍA sobre un larguero REAL, no un lado fijo del catálogo.
                Assert.Contains(high, beam =>
                {
                    var edge = PushBackLoadBeamGeometry.RearBeamTangencyPointWorld(
                        catalog, PushBackDefaults.HighEndBeamCatalogId,
                        beam.Insertion.X, beam.Insertion.Y, beam.MirroredX);
                    return edge.HasValue
                        && Math.Abs(edge.Value.X - axis.HighMate.X) < 1e-9
                        && Math.Abs(edge.Value.Y - axis.HighMate.Y) < 1e-9;
                });
            }
        }

        // ---- 6: la pendiente final es la RESULTANTE, acotada por el paso de troquel ----

        [Theory]
        [MemberData(nameof(Spans))]
        public void TheResultingSlope_IsCloseToTheTarget(int palletsDeep)
        {
            var catalog = Catalog;
            var system = System(catalog, palletsDeep);
            var front = system.Structure.Fronts[0];

            // Aclaración final del Owner (I-32): el criterio ya no es «la subida cae a menos de medio troquel del
            // objetivo nominal» sino «la PENDIENTE es la mejor que permite la retícula frente a 7/192». Que ese
            // mínimo sea global lo prueba PushBackBedAsymmetryTests recorriendo todos los candidatos.
            foreach (var axis in PushBackFlowBedGeometry.Resolve(system, catalog, front))
            {
                Assert.True(
                    PushBackBedRotation.SlopeError(axis.RotationRadians) < 0.01,
                    FormattableString.Invariant(
                        $"fondos={palletsDeep}: pendiente {axis.Slope:0.######} contra el objetivo {PushBackBedRotation.TargetSlope:0.######}"));
                Assert.True(axis.Slope > 0.0, "la cama tiene que seguir bajando hacia la salida");
            }
        }

        /// <summary>Todos los niveles comparten la misma pendiente: la cama es UNA definición por frente.</summary>
        [Theory]
        [MemberData(nameof(Spans))]
        public void EveryLevel_SharesTheSameResultingSlope(int palletsDeep)
        {
            var catalog = Catalog;
            var system = System(catalog, palletsDeep);
            var front = system.Structure.Fronts[0];

            var ratios = PushBackFlowBedGeometry.Resolve(system, catalog, front)
                .Select(axis => Math.Round(axis.Slope, 9))
                .Distinct()
                .ToList();

            Assert.Single(ratios);
        }

        // ---- Límite de redondeo: el punto teórico cae a mitad de camino entre dos troqueles ----

        /// <summary>
        /// El ajuste tiene que ser determinista y aterrizar en la retícula también cuando el punto teórico cae
        /// (casi) exactamente entre dos troqueles.
        ///
        /// Se barre el FONDO de la tarima, no la altura: la elevación del posterior ya está cuantizada al troquel, así
        /// que moverla no cambia el residuo. Lo que sí lo mueve de forma continua es el RECORRIDO entre contactos, y de
        /// ahí la subida nominal.
        /// </summary>
        [Fact]
        public void AMidpointBetweenTwoTroqueles_StillSnapsOntoTheGrid()
        {
            var catalog = Catalog;
            var camaY = CamaLocalY(catalog);
            var found = 0;

            for (var depth = 40.0; depth <= 56.0; depth += 0.25)
            {
                var system = new PushBackResolver(catalog).Resolve(new PushBackDesign
                {
                    Structure = new DynamicRackDesign
                    {
                        Pallet = new PalletSpecification(42.0, depth, 60.0, 1000.0, "kg"),
                        PalletsDeep = 4,
                        LoadLevels = 3,
                        FirstLevelHeight = 6.0,
                        BeamDepth = 4.0
                    }
                });
                var front = system.Structure.Fronts[0];
                var gridBase = GridBase(system, catalog);
                var axes = PushBackFlowBedGeometry.Resolve(system, catalog, front);
                if (axes.Count == 0) continue;

                // Sea cual sea el fondo, el larguero de entrada/salida SIEMPRE cae en troquel.
                var low = PushBackLoadBeamGeometry.LowBeams(system, catalog, front);
                Assert.All(low, beam => Assert.True(
                    OnGrid(beam.Insertion.Y, gridBase),
                    FormattableString.Invariant($"fondo {depth:0.##}: el bajo quedó fuera de troquel en {beam.Insertion.Y:0.####}")));

                var axis = axes[0];
                var bedLength = PushBackFlowBedGeometry.ResolveBedLength(system, front);
                var theoretical = axis.HighMate.Y - PushBackBedSlope.Rise(bedLength) - camaY;
                var steps = (theoretical - gridBase) / SelectiveRackDefaults.TroquelPaso;
                var residual = Math.Abs(steps - Math.Floor(steps));

                if (Math.Abs(residual - 0.5) < 0.03)
                {
                    found++;
                    // En el punto medio el resultado sigue siendo un troquel válido, determinista y con la mejor
                    // pendiente que la retícula permite — que es la garantía que la selección puede dar.
                    Assert.True(PushBackBedRotation.SlopeError(axis.RotationRadians) < 0.01);
                    Assert.True(OnGrid(low[0].Insertion.Y, gridBase));
                }
            }

            Assert.True(found > 0, "el barrido de fondos no encontró ningún caso a mitad de camino entre dos troqueles");
        }

        // ---- La cabecera cuenta las elevaciones resueltas UNA vez ----

        [Theory]
        [MemberData(nameof(Spans))]
        public void TheHeader_ReadsTheResolvedElevationsOnce(int palletsDeep)
        {
            var catalog = Catalog;
            var system = System(catalog, palletsDeep);
            var front = system.Structure.Fronts[0];

            var result = DynamicHeaderHeightCalculator.CalculateResolved(front);
            var top = front.LoadBeamLevels.OrderBy(l => l.LevelNumber).Last();
            var topCell = DynamicRackLevelGeometry.At(null, front, top.LevelNumber);

            Assert.Equal(
                top.EntranceElevation + topCell.InOutBeamDepth
                    + (topCell.Pallet.Height + topCell.ClearHeight) * DynamicHeaderHeightCalculator.TopFinishFraction,
                result.TheoreticalHeight,
                9);
        }
    }
}
