using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.PushBack;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-42 — DECISION FISICA DEL DUEÑO sobre la cama CORRIDA, validada a mano en AutoCAD:
    ///
    /// <para>
    /// El extremo BAJO —por donde se carga y se descarga— queda SIEMPRE anclado al poste EXTERIOR de su lado. El que
    /// se mueve hacia dentro cuando la cama pide menos fondo que la estructura disponible es el ALTO. La ronda
    /// anterior lo hacia al reves: la cama arrancaba metida dentro del rack, con el pasillo delante inaccesible.
    /// </para>
    /// <para>
    /// Son DOS preguntas distintas y las dos siguen siendo ciertas:
    /// <list type="bullet">
    /// <item><b>LONGITUDINALMENTE</b> manda el BAJO: fija el origen exterior, y el alto se resuelve por demanda.</item>
    /// <item><b>VERTICALMENTE</b> manda el ALTO: fija la elevacion y el troquel (I-32), y el bajo se deriva por la
    /// pendiente.</item>
    /// </list>
    /// </para>
    /// </summary>
    public class PushBackCorridaAnchorTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static PushBackDesign Design(
            PushBackRunDirection direction, int? corridaDepth, double gap = 0.0, int deepA = 6, int deepB = 8)
        {
            var design = PushBackCompositeStructureTests.Composite(
                slotsA: 1, slotsB: 1, deepA: deepA, deepB: deepB, levelsA: 1, levelsB: 1, gap: gap);
            design.Composite.DefaultTopology = PushBackCellTopology.Corrida;
            design.Composite.DefaultDirection = direction;
            if (corridaDepth.HasValue)
            {
                design.Composite.SetCorridaDepth(0, 0, corridaDepth.Value);
            }

            return design;
        }

        private static PushBackSystem Resolve(PushBackDesign design) => new PushBackResolver(Catalog).Resolve(design);

        private static PushBackRunAxis Axis(PushBackSystem system)
            => PushBackRunGeometry.Axes(PushBackRuns.Resolve(system), Catalog).Single();

        // ================= D: LOW fijo, HIGH movil ==============================================================

        /// <summary>
        /// PRUEBA VINCULANTE. Misma estructura y mismo sentido, tres demandas distintas: el BAJO no se mueve ni una
        /// milesima, el ALTO se desplaza monotonamente y la longitud resuelta crece con la demanda.
        /// </summary>
        [Theory]
        [InlineData(PushBackRunDirection.AToB, 0.0)]
        [InlineData(PushBackRunDirection.AToB, 24.0)]
        [InlineData(PushBackRunDirection.BToA, 0.0)]
        [InlineData(PushBackRunDirection.BToA, 24.0)]
        public void TheLowEndIsFixed_AndTheHighEndMoves_WithTheDemand(PushBackRunDirection direction, double gap)
        {
            var lows = new List<double>();
            var highs = new List<double>();
            var lengths = new List<double>();

            foreach (var depth in new[] { 6, 8, 10 })
            {
                var system = Resolve(Design(direction, depth, gap));
                var bed = system.Composite.Cell(0, 1).Beds.Single();
                Assert.Equal(depth, bed.DemandPositions);

                var axis = Axis(system);
                lows.Add(axis.LowContact.X);
                highs.Add(axis.HighContact.X);
                lengths.Add(bed.ResolvedBedLength);
            }

            // 1) El BAJO es EL MISMO en las tres. Es el ancla longitudinal.
            Assert.Equal(lows[0], lows[1], 6);
            Assert.Equal(lows[1], lows[2], 6);

            // 2) El ALTO se mueve, y siempre hacia fuera conforme crece la demanda.
            Assert.NotEqual(highs[0], highs[1], 3);
            Assert.NotEqual(highs[1], highs[2], 3);
            if (direction == PushBackRunDirection.AToB)
            {
                Assert.True(highs[0] < highs[1] && highs[1] < highs[2]);
            }
            else
            {
                Assert.True(highs[0] > highs[1] && highs[1] > highs[2]);
            }

            // 3) Y la longitud resuelta crece con la demanda.
            Assert.True(lengths[0] < lengths[1]);
            Assert.True(lengths[1] < lengths[2]);
        }

        /// <summary>
        /// El BAJO esta en la ORILLA. Con el sentido A-&gt;B es el extremo exterior de A (el arranque del rack) y con
        /// B-&gt;A el de B (el final). No es «cerca»: es la misma linea de postes del primer modulo del marco.
        /// </summary>
        [Theory]
        [InlineData(PushBackRunDirection.AToB)]
        [InlineData(PushBackRunDirection.BToA)]
        public void TheLowEnd_SitsOnTheOuterPostLine(PushBackRunDirection direction)
        {
            var full = Resolve(Design(direction, corridaDepth: null));
            var half = Resolve(Design(direction, corridaDepth: 6));

            // La cama COMPLETA y la CORTA arrancan exactamente en el mismo sitio.
            Assert.Equal(Axis(full).LowContact.X, Axis(half).LowContact.X, 6);

            var total = full.Structure.TotalLength;
            var low = Axis(half).LowContact.X;
            if (direction == PushBackRunDirection.AToB)
            {
                Assert.True(low < total * 0.15, "el bajo de una corrida A->B esta en la orilla de A");
            }
            else
            {
                Assert.True(low > total * 0.85, "el bajo de una corrida B->A esta en la orilla de B");
            }
        }

        // ================= E: el ORACULO legacy =================================================================

        /// <summary>
        /// Un Push Back de UN SENTIDO con la misma tarima: es el oraculo del producto. Su cama arranca en la linea
        /// exterior con el desfase de acoplamiento del larguero, y la corrida tiene que usar EXACTAMENTE esa regla.
        /// </summary>
        private static PushBackSystem Legacy(int deep)
        {
            var design = new PushBackDesign
            {
                Structure = new DynamicRackDesign
                {
                    Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                    PalletsDeep = deep,
                    LoadLevels = 1,
                    FirstLevelHeight = 4.0,
                    BeamDepth = 4.0
                }
            };
            design.Structure.Fronts.Add(new DynamicRackFrontDesign
            {
                PalletCount = 1, LoadLevels = 1, PalletsDeep = deep, DepthStartPosition = 1
            });
            design.Fronts.Add(new PushBackFrontConfig { DefaultPalletsDeep = deep });
            return new PushBackResolver(Catalog).Resolve(design);
        }

        /// <summary>
        /// PRUEBA VINCULANTE contra el oraculo: el mate BAJO de una corrida cae en la MISMA coordenada que el de una
        /// cama legacy que arranca en la misma linea exterior. No se comprueba «esta dentro del primer modulo» —eso
        /// dejaba pasar un desplazamiento de casi un fondo entero—: se comparan coordenadas reales.
        /// </summary>
        [Theory]
        [InlineData(6)]
        [InlineData(8)]
        [InlineData(10)]
        public void TheCorridaLowMate_MatchesTheLegacyLowMate(int depth)
        {
            var legacy = Legacy(depth);
            var legacyLow = PushBackFlowBedGeometry
                .Resolve(legacy, Catalog, legacy.Structure.Fronts[0])
                .Single()
                .ExitMate;

            // A->B: la corrida arranca en la orilla de A, que es el origen del rack, igual que la legacy.
            var forward = Resolve(Design(PushBackRunDirection.AToB, depth));
            var forwardRun = PushBackRuns.Resolve(forward).Runs.Single();
            var forwardLow = PushBackFlowBedGeometry
                .Resolve(forwardRun.Source, Catalog, forwardRun.Front())
                .Single()
                .ExitMate;

            Assert.Equal(legacyLow.X, forwardLow.X, 6);

            // B->A: la corrida arranca en la orilla de B. En SU marco —el espejado— el arranque es el mismo origen,
            // asi que el mate tiene que coincidir tambien ahi: es la misma pieza, colocada con la misma regla.
            var backward = Resolve(Design(PushBackRunDirection.BToA, depth));
            var backwardRun = PushBackRuns.Resolve(backward).Runs.Single();
            var backwardLow = PushBackFlowBedGeometry
                .Resolve(backwardRun.Source, Catalog, backwardRun.Front())
                .Single()
                .ExitMate;

            Assert.Equal(legacyLow.X, backwardLow.X, 6);
        }

        // ================= V: piezas dentro del tramo, y solo dentro ============================================

        /// <summary>
        /// Ninguna pieza de la cama antes del BAJO ni despues del ALTO, y los intermedios crecen conforme la demanda
        /// descubre nuevos apoyos. Las tarimas arrancan desde el lado BAJO y ninguna cae en el hueco.
        /// </summary>
        [Theory]
        [InlineData(PushBackRunDirection.AToB, 0.0)]
        [InlineData(PushBackRunDirection.AToB, 24.0)]
        [InlineData(PushBackRunDirection.BToA, 0.0)]
        [InlineData(PushBackRunDirection.BToA, 24.0)]
        public void NoPieceFallsOutsideTheRun_AndIntermediatesGrowWithIt(PushBackRunDirection direction, double gap)
        {
            var previous = -1;
            foreach (var depth in new[] { 6, 8, 10 })
            {
                var design = Design(direction, depth, gap);
                design.Fronts[0].DrawPallets.Add(true);
                var system = Resolve(design);

                var run = PushBackRuns.Resolve(system).Runs.Single();
                var front = run.Front();
                var frame = run.Source.Structure;

                var low = frame.Modules[0].StartX;
                var high = PushBackCellDepth.RearX(run.Source, front, run.SourceLevel);
                Assert.True(high > low);

                var intermediates = new PushBackIntermediateBeamLateralBuilder()
                    .BuildFor(run.Source, Catalog, front, new[] { run.SourceLevel });
                Assert.All(intermediates, beam =>
                {
                    Assert.True(beam.Insertion.X > low + 1e-6, "ninguna pieza antes del bajo");
                    Assert.True(beam.Insertion.X < high - 1e-6, "ninguna pieza despues del alto");
                });

                // Mas demanda = mas apoyos internos descubiertos, nunca menos.
                Assert.True(intermediates.Count >= previous, "los intermedios no pueden disminuir al pedir mas fondo");
                previous = intermediates.Count;

                var pallets = PushBackTarimaPlacement.Lateral(
                    run.Source, Catalog, front, int.MaxValue, new[] { run.SourceLevel });
                Assert.Equal(depth, pallets.Count);
                Assert.All(pallets, pallet =>
                {
                    Assert.True(pallet.Insertion.X > low - 1e-6);
                    Assert.True(pallet.Insertion.X < high + 1e-6);
                });

                if (gap > 0.0)
                {
                    var hole = frame.Modules.Single(module => module.Kind == DynamicRackModuleKind.Gap);
                    Assert.All(pallets, pallet => Assert.True(
                        pallet.Insertion.X <= hole.StartX + 1e-6 || pallet.Insertion.X >= hole.EndX - 1e-6,
                        "ninguna tarima puede caer dentro del hueco"));
                }
            }
        }

        // ================= La autoridad VERTICAL sigue siendo el ALTO ===========================================

        /// <summary>
        /// Cambiar el ancla LONGITUDINAL no toca la autoridad VERTICAL: el extremo alto sigue siendo el elevado y el
        /// que gobierna la elevacion, exactamente como en I-32. Son dos preguntas distintas.
        /// </summary>
        [Theory]
        [InlineData(PushBackRunDirection.AToB)]
        [InlineData(PushBackRunDirection.BToA)]
        public void TheHighEnd_StillGovernsElevation(PushBackRunDirection direction)
        {
            var axis = Axis(Resolve(Design(direction, corridaDepth: 7)));

            Assert.True(axis.HighContact.Y > axis.LowContact.Y, "el alto sigue siendo el extremo elevado");
            Assert.True(Math.Abs(axis.Slope) > 0.0, "la pendiente sigue existiendo");
            Assert.Equal(direction == PushBackRunDirection.AToB, axis.FlowsForward);
        }
    }
}
