using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Persistence;
using RackCad.Application.RackFrames;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.PushBack;
using RackCad.Application.Systems.Selective;
using RackCad.Application.Systems.Shared;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-67 — obligaciones invariante→prueba de la resolucion de elevaciones. Las reglas se comprueban con numeros
    /// escritos a mano o con invariantes (separacion, reticula), nunca repitiendo la formula de produccion.
    /// </summary>
    public class LevelElevationResolutionTests
    {
        /// <summary>La base real de <c>TROQUEL_LARGUERO</c> en FRONTAL del poste estandar.</summary>
        private const double GridBase = 0.6053;
        private const double Pitch = 2.0;

        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        // ---- construccion de casos ------------------------------------------------------------------------------

        private static DynamicRackLevel Cell(int number, double palletHeight, double beamDepth = 6.0, double clear = 6.0)
            => new DynamicRackLevel
            {
                LevelNumber = number,
                Pallet = new PalletSpecification(42.0, 48.0, palletHeight, 1000.0, "kg"),
                InOutBeamDepth = beamDepth,
                ClearHeight = clear
            };

        /// <summary>Celdas con un pallet tal que el requisito de cada intervalo es exactamente el valor pedido.</summary>
        private static List<DynamicRackLevel> Cells(params double[] requirements)
            => requirements.Select((requirement, index) => Cell(index + 1, requirement - 12.0)).ToList();

        private static IReadOnlyList<DynamicLoadBeamLevel> Levels(
            IReadOnlyList<DynamicRackLevel> cells,
            double first,
            double slope = 0.0,
            double gridBase = GridBase,
            double pitch = Pitch)
            => DynamicLoadBeamGeometry.ResolveLevels(cells, first, slope, gridBase, pitch);

        private static List<double> Exits(IEnumerable<DynamicLoadBeamLevel> levels)
            => levels.OrderBy(level => level.LevelNumber).Select(level => level.ExitElevation).ToList();

        private static List<double> Entrances(IEnumerable<DynamicLoadBeamLevel> levels)
            => levels.OrderBy(level => level.LevelNumber).Select(level => level.EntranceElevation).ToList();

        private static List<double> Spacings(IReadOnlyList<double> elevations)
            => elevations.Zip(elevations.Skip(1), (lower, upper) => Math.Round(upper - lower, 6)).ToList();

        private static void AssertElevations(IReadOnlyList<double> expected, IReadOnlyList<double> actual)
        {
            Assert.Equal(expected.Count, actual.Count);
            for (var index = 0; index < expected.Count; index++)
            {
                Assert.Equal(expected[index], actual[index], 4);
            }
        }

        private static double Nearest(double value, double gridBase, double pitch)
            => gridBase + Math.Round((value - gridBase) / pitch, MidpointRounding.AwayFromZero) * pitch;

        private static void AssertOnGrid(double elevation, double gridBase, double pitch)
        {
            var steps = (elevation - gridBase) / pitch;
            Assert.True(Math.Abs(steps - Math.Round(steps)) < 1e-6, $"{elevation} no cae en la reticula {gridBase} + k*{pitch}");
        }

        private static DynamicRackDesign H58Design(
            double palletHeight = 33.0,
            int levels = 4,
            double first = 6.0,
            DimensionDetail dimensions = DimensionDetail.None)
            => new DynamicRackDesign
            {
                Pallet = new PalletSpecification(42.0, 48.0, palletHeight, 1000.0, "kg"),
                PalletsDeep = 4,
                LoadLevels = levels,
                FirstLevelHeight = first,
                BeamDepth = 6.0,
                HeaderPostCatalogId = TestCatalogIds.Profiles.Posts.Standard,
                Dimensions = dimensions
            };

        private static PushBackDesign H58PushBackDesign(DimensionDetail dimensions = DimensionDetail.None)
        {
            var design = new PushBackDesign { Structure = H58Design(dimensions: dimensions) };
            design.Structure.Fronts.Add(new DynamicRackFrontDesign
            {
                PalletCount = 1, LoadLevels = 4, PalletsDeep = 4, DepthStartPosition = 1
            });
            design.Fronts.Add(new PushBackFrontConfig());
            return design;
        }

        private static (double Base, double Pitch) RealGrid(RackCatalog catalog, DynamicRackSystem structure)
        {
            var postId = DynamicFrontGeometry.PostId(structure, catalog);
            var peralte = DynamicFrontGeometry.PostPeralte(structure, catalog, postId);
            var entry = catalog.ConnectionLayout.FindConnectionLayout(
                postId, TestCatalogIds.ConnectionPoints.BeamPunch, TestCatalogIds.Views.Front);
            var point = SelectivePostGeometry.Resolve(
                entry, new Dictionary<string, double> { [SelectiveRackDefaults.PeralteParam] = peralte });
            return (point.Y, SelectiveRackDefaults.TroquelPaso);
        }

        /// <summary>Requisito y reticula, medidos sobre las celdas reales de un frente resuelto.</summary>
        private static void AssertEveryIntervalIsItsFirstPunch(DynamicRackFront front, double gridBase, double pitch)
        {
            var levels = front.LoadBeamLevels.OrderBy(level => level.LevelNumber).ToList();
            Assert.True(levels.Count >= 2, "el caso necesita al menos dos niveles");

            var cells = front.Levels.OrderBy(level => level.LevelNumber).ToList();
            for (var index = 0; index + 1 < levels.Count; index++)
            {
                var required = cells[index].InOutBeamDepth + cells[index].Pallet.Height + cells[index].ClearHeight;
                var spacing = levels[index + 1].ExitElevation - levels[index].ExitElevation;
                Assert.True(spacing >= required - 1e-6, $"intervalo {index + 1}: {spacing} < requerido {required}");
                Assert.True(spacing < required + pitch - 1e-6, $"intervalo {index + 1}: {spacing} no es el primer troquel para {required}");
                AssertOnGrid(levels[index + 1].ExitElevation, gridBase, pitch);
            }
        }

        // ---- T2: troquel exacto ------------------------------------------------------------------------------------

        [Fact]
        public void T2_AnExactPunch_IsKeptWithoutClimbingAnotherStep()
        {
            var exits = Exits(Levels(Cells(46.0, 46.0, 46.0, 46.0), 6.6053));

            AssertElevations(new[] { 6.6053, 52.6053, 98.6053, 144.6053 }, exits);
        }

        [Theory]
        [InlineData(46.00000000001, 52.6053)]    // ruido de coma flotante: sigue siendo el mismo troquel
        [InlineData(45.99999999999, 52.6053)]    // por debajo por ruido: mismo troquel
        [InlineData(46.0001, 54.6053)]         // un exceso real sube al siguiente
        public void T2_TheToleranceOnlyAbsorbsFloatingPointNoise(double requirement, double expectedSecond)
        {
            var exits = Exits(Levels(Cells(requirement, requirement), 6.6053));

            Assert.Equal(expectedSecond, exits[1], 6);
        }

        // ---- T3 y T4: intervalos ----------------------------------------------------------------------------------

        [Fact]
        public void T3_EachIntervalGoesToItsOwnFirstPunch_WithoutImposingUniformity()
        {
            var exits = Exits(Levels(Cells(45.0, 38.0, 50.25, 44.0), 6.6053));

            AssertElevations(new[] { 6.6053, 52.6053, 90.6053, 142.6053 }, exits);
            Assert.Equal(new[] { 46.0, 38.0, 52.0 }, Spacings(exits));
        }

        [Theory]
        [InlineData(6.6053)]    // primer nivel en troquel
        [InlineData(6.0)]    // primer nivel que sube al troquel
        [InlineData(7.0)]    // primer nivel que baja al troquel
        public void T4_ManyIntervalsDoNotAlternate(double first)
        {
            var spacings = Spacings(Exits(Levels(Cells(45.0, 45.0, 45.0, 45.0, 45.0, 45.0, 45.0), first)));

            Assert.All(spacings, spacing => Assert.Equal(46.0, spacing, 6));
        }

        // ---- T5: origen y paso son datos ----------------------------------------------------------------------------

        [Theory]
        [InlineData(0.0, 2.0, 46.0)]
        [InlineData(0.6053, 2.0, 46.0)]
        [InlineData(1.25, 2.0, 46.0)]
        [InlineData(-0.75, 2.0, 46.0)]
        [InlineData(3.1, 2.0, 46.0)]
        [InlineData(0.6053, 1.5, 45.0)]
        [InlineData(0.6053, 4.0, 48.0)]
        public void T5_TheGridOriginAndPitchComeFromTheAuthority(double gridBase, double pitch, double expectedSpacing)
        {
            var exits = Exits(Levels(Cells(45.0, 45.0, 45.0, 45.0), 6.0, 0.0, gridBase, pitch));

            Assert.All(exits, exit => AssertOnGrid(exit, gridBase, pitch));
            Assert.All(Spacings(exits), spacing => Assert.Equal(expectedSpacing, spacing, 6));
        }

        // ---- T6: el invariante, sobre una matriz -------------------------------------------------------------------

        public static IEnumerable<object[]> Matrix()
        {
            var requirementSets = new[]
            {
                new[] { 45.0, 45.0, 45.0, 45.0, 45.0 },
                new[] { 45.0, 38.0, 50.25, 47.5, 44.0 },
                new[] { 46.0, 46.0, 46.0 },
                new[] { 33.3333, 33.3333, 33.3333, 33.3333 },
                new[] { 2.0, 0.75, 1.0, 12.5 }
            };

            foreach (var gridBase in new[] { 0.0, 0.6053, 1.25, -0.75, 3.1 })
            foreach (var pitch in new[] { 2.0, 1.5, 0.5 })
            foreach (var first in new[] { 0.0, 0.5, 1.0, 2.75, 3.0, 4.5, 6.0, 7.0, 9.25 })
            foreach (var requirements in requirementSets)
            {
                yield return new object[] { gridBase, pitch, first, requirements };
            }
        }

        [Theory]
        [MemberData(nameof(Matrix))]
        public void T6_EveryIntervalMeetsItsRequirement_AndIsTheFirstPunchThatDoes(
            double gridBase, double pitch, double first, double[] requirements)
        {
            var exits = Exits(Levels(Cells(requirements), first, 0.0, gridBase, pitch));

            for (var index = 0; index + 1 < exits.Count; index++)
            {
                var spacing = exits[index + 1] - exits[index];
                Assert.True(spacing >= requirements[index] - 1e-6, $"{spacing} < {requirements[index]}");
                Assert.True(spacing < requirements[index] + pitch - 1e-6, $"{spacing} no es el primer troquel de {requirements[index]}");
                AssertOnGrid(exits[index + 1], gridBase, pitch);
            }
        }

        [Fact]
        public void T17_BothOverloadsFollowTheSameRule_OnTheRealGrid()
        {
            var byPallet = DynamicLoadBeamGeometry.ResolveLevels(
                new PalletSpecification(42.0, 48.0, 33.0, 1000.0, "kg"),
                loadLevels: 4,
                firstLevelHeight: 6.0,
                beamDepth: 6.0,
                slope: 7.4375,
                troquelGridBase: GridBase,
                troquelPitch: Pitch);
            var byCell = Levels(Cells(45.0, 45.0, 45.0, 45.0), 6.0, 7.4375);

            Assert.Equal(Exits(byCell), Exits(byPallet));
            Assert.Equal(Entrances(byCell), Entrances(byPallet));
        }

        // ---- T13 y T16: D-PEND, la subida del nivel 1 ---------------------------------------------------------------

        [Fact]
        public void T13_EveryLevelKeepsTheRiseOfTheFirstLevel()
        {
            var levels = Levels(Cells(45.0, 45.0, 45.0, 45.0), 6.0, 7.4375);

            AssertElevations(new[] { 6.6053, 52.6053, 98.6053, 144.6053 }, Exits(levels));
            AssertElevations(new[] { 12.6053, 58.6053, 104.6053, 150.6053 }, Entrances(levels));
        }

        [Theory]
        [InlineData(0.0)]
        [InlineData(1.0)]
        [InlineData(3.2)]
        [InlineData(6.6)]
        [InlineData(7.0)]
        [InlineData(7.4375)]
        [InlineData(12.0)]
        public void T13_TheFirstEntranceIsHistorical_TheRiseIsUniform_AndTheEntranceIntervalsHold(double slope)
        {
            var requirements = new[] { 45.0, 38.0, 50.25, 44.0 };
            foreach (var first in new[] { 6.0, 7.0, 6.6053, 9.5 })
            {
                var levels = Levels(Cells(requirements), first, slope).OrderBy(level => level.LevelNumber).ToList();

                Assert.Equal(Nearest(first, GridBase, Pitch), levels[0].ExitElevation, 6);
                Assert.Equal(Nearest(first + slope, GridBase, Pitch), levels[0].EntranceElevation, 6);

                var rise = levels[0].EntranceElevation - levels[0].ExitElevation;
                Assert.All(levels, level => Assert.Equal(rise, level.EntranceElevation - level.ExitElevation, 6));

                var entrances = Entrances(levels);
                for (var index = 0; index + 1 < entrances.Count; index++)
                {
                    Assert.True(entrances[index + 1] - entrances[index] >= requirements[index] - 1e-6);
                }
            }
        }

        [Fact]
        public void T13_ANegativeSlopeIsNotApplied()
        {
            var levels = Levels(Cells(45.0, 45.0), 6.6053, -3.0);

            AssertElevations(Exits(levels), Entrances(levels));
        }

        /// <summary>
        /// D-PEND: un rack sin el defecto no cambia. Si cada requisito es multiplo del paso, salidas Y entradas son las
        /// de la regla anterior, sea cual sea la fase del primer nivel.
        /// </summary>
        [Theory]
        [InlineData(46.0, 46.0, 46.0)]
        [InlineData(48.0, 72.0, 50.0)]
        [InlineData(72.0, 72.0, 72.0)]
        public void T16_RequirementsOnThePitch_LeaveExitsAndEntrancesExactlyAsBefore(double a, double b, double c)
        {
            var requirements = new[] { a, b, c, 46.0 };
            foreach (var first in new[] { 3.0, 4.0, 6.0, 7.5, 9.25, 12.0 })
            foreach (var slope in new[] { 0.0, 3.2, 6.6, 7.4375, 12.0 })
            {
                var levels = Levels(Cells(requirements), first, slope);

                AssertElevations(ExitsUnderThePreviousRule(first, requirements, GridBase, Pitch), Exits(levels));
                AssertElevations(EntrancesUnderThePreviousRule(first, slope, requirements, GridBase, Pitch), Entrances(levels));
            }
        }

        // ---- T14: limites ------------------------------------------------------------------------------------------

        [Theory]
        [InlineData(double.NaN, 0.0)]
        [InlineData(0.5, 0.0)]
        [InlineData(0.5, -2.0)]
        [InlineData(double.PositiveInfinity, 2.0)]
        public void T14_WithoutAMeasuredGrid_NothingIsAdjusted(double gridBase, double pitch)
        {
            var levels = Levels(Cells(45.25, 33.75), 6.0, 12.0, gridBase, pitch);

            AssertElevations(new[] { 6.0, 51.25 }, Exits(levels));
            AssertElevations(new[] { 18.0, 63.25 }, Entrances(levels));
        }

        [Theory]
        [InlineData(0.0)]      // requisito cero
        [InlineData(-10.0)]    // requisito negativo: no hay nivel que pueda quedar debajo del anterior
        public void T14_ADegenerateRequirementClimbsOnePunch_LevelsNeverCoincideOrInvert(double requirement)
        {
            var cells = new[] { Cell(1, 1.0, 0.0, requirement - 1.0), Cell(2, 1.0, 0.0, requirement - 1.0), Cell(3, 1.0, 0.0, 0.0) };

            var exits = Exits(Levels(cells, 6.6053));

            AssertElevations(new[] { 6.6053, 8.6053, 10.6053 }, exits);
        }

        [Theory]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        public void T14_ANonFiniteRequirementPropagates_WithoutThrowing(double requirement)
        {
            var cells = new[] { Cell(1, 33.0, 6.0, requirement), Cell(2, 33.0), Cell(3, 33.0) };

            var levels = Levels(cells, 6.6053);

            Assert.Equal(3, levels.Count);
            Assert.Equal(6.6053, levels[0].ExitElevation, 4);
            Assert.False(double.IsFinite(levels[1].ExitElevation));
            Assert.False(double.IsFinite(levels[2].ExitElevation));
        }

        [Fact]
        public void T14_VeryLargeElevationsStayDeterministic()
        {
            var exits = Exits(Levels(Cells(45.0, 45.0), 1_000_000.6053));

            AssertElevations(new[] { 1_000_000.6053, 1_000_046.6053 }, exits);
        }

        // ---- T7: vistas y cotas Dinamico ------------------------------------------------------------------------------

        private static IReadOnlyList<(double Low, double High)> VerticalDimensions(IEnumerable<HeaderBlockInstance> instances, string view)
            => instances
                .Where(i => i.Role == HeaderBlockRole.Dimension && i.View == view
                            && Math.Abs(i.Insertion.X - i.ConnectionAnchor.X) < 1e-6)
                .Select(i => (Low: Math.Round(Math.Min(i.Insertion.Y, i.ConnectionAnchor.Y), 6),
                              High: Math.Round(Math.Max(i.Insertion.Y, i.ConnectionAnchor.Y), 6)))
                .Distinct()
                .ToList();

        private static void AssertChain(IReadOnlyList<(double Low, double High)> dimensions, IReadOnlyList<double> elevations)
        {
            for (var index = 0; index + 1 < elevations.Count; index++)
            {
                var low = Math.Round(elevations[index], 6);
                var high = Math.Round(elevations[index + 1], 6);
                Assert.True(
                    dimensions.Contains((low, high)),
                    $"falta la cota {low}→{high}; hay: {string.Join(", ", dimensions)}");
            }
        }

        private static List<double> DrawnYs(IEnumerable<HeaderBlockInstance> instances, string pieceId)
            => instances
                .Where(i => string.Equals(i.PieceId, pieceId, StringComparison.Ordinal))
                .Select(i => Math.Round(i.Insertion.Y, 6))
                .Distinct()
                .OrderBy(y => y)
                .ToList();

        [Fact]
        public void T7_DynamicLateral_DrawsAndDimensionsTheResolvedElevations()
        {
            var catalog = Catalog;
            var system = new DynamicRackSystemResolver(catalog).Resolve(H58Design(dimensions: DimensionDetail.Standard)).System;
            var exits = Exits(system.LoadBeamLevels);

            Assert.Equal(new[] { 46.0, 46.0, 46.0 }, Spacings(exits));

            var plan = new DynamicSystemLateralBuilder().Build(system, catalog);
            AssertChain(VerticalDimensions(plan.LooseInstances, "LATERAL"), exits);

            var drawn = DrawnYs(plan.LooseInstances, TestCatalogIds.Profiles.Beams.DynamicInOut);
            Assert.All(exits.Concat(Entrances(system.LoadBeamLevels)), elevation => Assert.Contains(Math.Round(elevation, 6), drawn));
        }

        [Theory]
        [InlineData(DynamicRackEnd.Exit)]
        [InlineData(DynamicRackEnd.Entrance)]
        public void T7_DynamicFrontal_DimensionsTheElevationsOfItsEnd(DynamicRackEnd end)
        {
            var catalog = Catalog;
            var system = new DynamicRackSystemResolver(catalog).Resolve(H58Design(dimensions: DimensionDetail.Standard)).System;
            var expected = end == DynamicRackEnd.Exit ? Exits(system.LoadBeamLevels) : Entrances(system.LoadBeamLevels);

            Assert.Equal(new[] { 46.0, 46.0, 46.0 }, Spacings(expected));

            var plan = new DynamicSystemFrontalBuilder().BuildPlan(system, catalog, end);
            AssertChain(VerticalDimensions(plan.LooseInstances, "FRONTAL"), expected);
        }

        // ---- T8 y T9: Push Back -------------------------------------------------------------------------------------

        [Fact]
        public void T8_PushBackLateral_DrawsAndDimensionsTheCorrectedLevels()
        {
            var catalog = Catalog;
            var system = new PushBackResolver(catalog).Resolve(H58PushBackDesign(DimensionDetail.Standard));
            var front = system.Structure.Fronts[0];
            var exits = Exits(front.LoadBeamLevels);
            var grid = RealGrid(catalog, system.Structure);

            AssertEveryIntervalIsItsFirstPunch(front, grid.Base, grid.Pitch);
            Assert.Equal(new[] { 46.0, 46.0, 46.0 }, Spacings(exits));

            var low = PushBackElevations.LowInsertions(system, catalog, front);
            Assert.Equal(exits, low.OrderBy(entry => entry.Key).Select(entry => entry.Value).ToList());

            var plan = new PushBackSystemLateralBuilder().Build(system, catalog);
            AssertChain(VerticalDimensions(plan.LooseInstances, "LATERAL"), exits);
        }

        [Theory]
        [InlineData(PushBackSide.A)]
        [InlineData(PushBackSide.B)]
        public void T8_PushBackComposite_BothSidesFollowTheRule(PushBackSide side)
        {
            var catalog = Catalog;
            var state = new PushBackCompositeEditorState();
            state.SideA.LoadNew();
            state.SetSideBPresent(true);
            state.SideB.LoadNew();
            state.SetSlotCount(2);
            for (var slot = 0; slot < state.SlotCount; slot++)
            {
                state.SetSlotPresent(PushBackSide.B, slot, true);
            }

            foreach (var editing in new[] { PushBackSide.A, PushBackSide.B })
            {
                var matrix = state.Of(editing).Structure;
                for (var front = 0; front < matrix.Count; front++)
                {
                    state.Of(editing).AdjustLevels(front, 4 - matrix.Fronts[front].LoadLevels);
                    foreach (var cell in matrix.Fronts[front].Cells)
                    {
                        // El requisito impar es el que dispara H58: 33 + 6 + 6 = 45" sobre una reticula de 2".
                        cell.PalletHeight = 33.0;
                        cell.ClearHeight = 6.0;
                        cell.InOutBeamDepth = 6.0;
                    }
                }
            }

            var inputs = PushBackEditorInputs.NewDesign();
            inputs.Pallet = new PalletSpecification(42.0, 48.0, 33.0, 1000.0, "kg");

            var system = new PushBackCompositeEditorAssembler(catalog).Build(state, inputs, catalog).System;
            var local = side == PushBackSide.A ? system.Composite.SideA.Local : system.Composite.SideB.Local;
            var grid = RealGrid(catalog, local.Structure);

            var loaded = local.Structure.Fronts.Where(front => front.LoadBeamLevels.Count == 4).ToList();
            Assert.NotEmpty(loaded);
            foreach (var front in loaded)
            {
                AssertEveryIntervalIsItsFirstPunch(front, grid.Base, grid.Pitch);
                Assert.Equal(new[] { 46.0, 46.0, 46.0 }, Spacings(Exits(front.LoadBeamLevels)));
            }
        }

        /// <summary>
        /// Caracterizacion: la pendiente de Push Back no pertenece a esta iniciativa. El larguero posterior se sigue
        /// derivando del bajo, asi que subida y rotacion son las mismas en todos los niveles de la cama.
        /// </summary>
        [Theory]
        [InlineData(4)]
        [InlineData(6)]
        [InlineData(10)]
        public void T9_PushBackSlope_IsTheSameOnEveryLevel_AndStaysDerivedFromTheLowAnchor(int palletsDeep)
        {
            var catalog = Catalog;
            var design = H58PushBackDesign();
            design.Structure.PalletsDeep = palletsDeep;
            design.Structure.Fronts[0].PalletsDeep = palletsDeep;
            var system = new PushBackResolver(catalog).Resolve(design);
            var front = system.Structure.Fronts[0];

            var cells = PushBackElevations.Resolve(system, catalog, front).OrderBy(entry => entry.Key).Select(entry => entry.Value).ToList();

            Assert.Equal(4, cells.Count);
            var firstRise = cells[0].RearInsertion - cells[0].LowInsertion;
            Assert.All(cells, cell =>
            {
                Assert.Equal(firstRise, cell.RearInsertion - cell.LowInsertion, 6);
                Assert.Equal(cells[0].RotationRadians, cell.RotationRadians, 9);
            });
            Assert.Equal(Exits(front.LoadBeamLevels), cells.Select(cell => cell.LowInsertion).ToList());
        }

        // ---- T10 y T11: edicion, guardado y reapertura ----------------------------------------------------------------

        [Fact]
        public void T10_DynamicEditUpdate_IsIdempotent()
        {
            var catalog = Catalog;
            var resolver = new DynamicRackSystemResolver(catalog);
            var first = resolver.Resolve(H58Design()).System;

            var snapshot = resolver.Snapshot(first, 4, 6.0, 6.0, TestCatalogIds.Profiles.Posts.Standard);
            var second = resolver.Resolve(snapshot).System;

            Assert.Equal(new[] { 46.0, 46.0, 46.0 }, Spacings(Exits(second.LoadBeamLevels)));
            Assert.Equal(Exits(first.LoadBeamLevels), Exits(second.LoadBeamLevels));
            Assert.Equal(Entrances(first.LoadBeamLevels), Entrances(second.LoadBeamLevels));
        }

        [Fact]
        public void T10_PushBackEditUpdate_IsIdempotent()
        {
            var catalog = Catalog;
            var resolver = new PushBackResolver(catalog);
            var first = resolver.Resolve(H58PushBackDesign());

            var second = resolver.Resolve(resolver.Snapshot(first));

            Assert.Equal(new[] { 46.0, 46.0, 46.0 }, Spacings(Exits(second.Structure.Fronts[0].LoadBeamLevels)));
            Assert.Equal(Exits(first.Structure.Fronts[0].LoadBeamLevels), Exits(second.Structure.Fronts[0].LoadBeamLevels));
        }

        private static void AssertNoElevationIsPersisted(string json)
        {
            Assert.DoesNotContain("ExitElevation", json, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("EntranceElevation", json, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("LoadBeamLevels", json, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void T11_DynamicSaveReopen_RecomputesTheSameLevels_AndEditingKeepsTheRule()
        {
            var catalog = Catalog;
            var resolver = new DynamicRackSystemResolver(catalog);
            var design = H58Design();
            var before = Exits(resolver.Resolve(design).System.LoadBeamLevels);

            var json = JsonSerializer.Serialize(DynamicRackSystemDocument.From(design));
            var reopened = JsonSerializer.Deserialize<DynamicRackSystemDocument>(json).ToDesign();

            AssertNoElevationIsPersisted(json);
            Assert.Equal(new[] { 46.0, 46.0, 46.0 }, Spacings(before));
            Assert.Equal(before, Exits(resolver.Resolve(reopened).System.LoadBeamLevels));

            // Editar tras reabrir: 35" de tarima son 47" de requisito, y el primer troquel son 48".
            reopened.Pallet = new PalletSpecification(42.0, 48.0, 35.0, 1000.0, "kg");
            Assert.Equal(new[] { 48.0, 48.0, 48.0 }, Spacings(Exits(resolver.Resolve(reopened).System.LoadBeamLevels)));
        }

        [Fact]
        public void T11_PushBackSaveReopen_RecomputesTheSameLevels()
        {
            var catalog = Catalog;
            var resolver = new PushBackResolver(catalog);
            var design = H58PushBackDesign();
            var before = Exits(resolver.Resolve(design).Structure.Fronts[0].LoadBeamLevels);

            var json = JsonSerializer.Serialize(PushBackDesignDocument.FromDomain(design));
            var reopened = JsonSerializer.Deserialize<PushBackDesignDocument>(json).ToDomain();

            AssertNoElevationIsPersisted(json);
            Assert.Equal(new[] { 46.0, 46.0, 46.0 }, Spacings(before));
            Assert.Equal(before, Exits(resolver.Resolve(reopened).Structure.Fronts[0].LoadBeamLevels));
        }

        // ---- T12: documentos existentes ----------------------------------------------------------------------------

        /// <summary>La regla ANTERIOR, escrita aqui a proposito como oraculo de la cota inferior de D-LEG.</summary>
        private static List<double> ExitsUnderThePreviousRule(double first, IReadOnlyList<double> requirements, double gridBase, double pitch)
        {
            var result = new List<double>();
            var theoretical = first;
            result.Add(Nearest(theoretical, gridBase, pitch));
            foreach (var requirement in requirements.Take(requirements.Count - 1))
            {
                theoretical += requirement;
                result.Add(Nearest(theoretical, gridBase, pitch));
            }

            return result;
        }

        private static List<double> EntrancesUnderThePreviousRule(
            double first, double slope, IReadOnlyList<double> requirements, double gridBase, double pitch)
            => ExitsTheoretical(first, requirements).Select(value => Nearest(value + slope, gridBase, pitch)).ToList();

        private static List<double> ExitsTheoretical(double first, IReadOnlyList<double> requirements)
        {
            var result = new List<double> { first };
            foreach (var requirement in requirements.Take(requirements.Count - 1))
            {
                result.Add(result[result.Count - 1] + requirement);
            }

            return result;
        }

        [Theory]
        [MemberData(nameof(Matrix))]
        public void T12_NothingDropsBelowThePreviousRule_AndTheFirstLevelDoesNotMove(
            double gridBase, double pitch, double first, double[] requirements)
        {
            const double slope = 7.4375;
            var levels = Levels(Cells(requirements), first, slope, gridBase, pitch);
            var exits = Exits(levels);
            var entrances = Entrances(levels);
            var previousExits = ExitsUnderThePreviousRule(first, requirements, gridBase, pitch);
            var previousEntrances = EntrancesUnderThePreviousRule(first, slope, requirements, gridBase, pitch);

            Assert.Equal(previousExits[0], exits[0], 6);
            Assert.Equal(previousEntrances[0], entrances[0], 6);
            for (var index = 1; index < exits.Count; index++)
            {
                Assert.True(exits[index] >= previousExits[index] - 1e-6, $"salida {index + 1}: {exits[index]} < {previousExits[index]}");
                Assert.True(entrances[index] >= previousEntrances[index] - 1e-6, $"entrada {index + 1}: {entrances[index]} < {previousEntrances[index]}");
            }
        }

        [Theory]
        [InlineData(0.0)]
        [InlineData(3.0)]
        [InlineData(6.0)]
        [InlineData(7.5)]
        [InlineData(9.5)]
        public void T12_ALegacyDynamicDocument_KeepsItsAuthoredInputs_AndAdoptsTheRuleOnResolve(double stored)
        {
            var catalog = Catalog;
            var resolver = new DynamicRackSystemResolver(catalog);
            var legacy = H58Design(first: stored);
            Assert.Null(legacy.FirstLevelDatum);

            var json = JsonSerializer.Serialize(DynamicRackSystemDocument.From(legacy));
            var reopened = JsonSerializer.Deserialize<DynamicRackSystemDocument>(json).ToDesign();
            var system = resolver.Resolve(reopened).System;

            // Resolver no reescribe lo authored, y nada se migra: el documento vuelve a serializarse igual.
            Assert.Equal(json, JsonSerializer.Serialize(DynamicRackSystemDocument.From(reopened)));
            Assert.Null(reopened.FirstLevelDatum);

            var grid = RealGrid(catalog, system);
            Assert.Equal(Nearest(stored, grid.Base, grid.Pitch), system.LoadBeamLevels[0].ExitElevation, 6);
            AssertEveryIntervalIsItsFirstPunch(system.Fronts[0], grid.Base, grid.Pitch);
        }

        // ---- T15: altura ----------------------------------------------------------------------------------------------

        [Theory]
        [InlineData(33.0, 4)]
        [InlineData(33.0, 6)]
        [InlineData(36.5, 5)]
        public void T15_TheAutomaticHeaderContainsTheTopLevel(double palletHeight, int levels)
        {
            var system = new DynamicRackSystemResolver(Catalog).Resolve(H58Design(palletHeight, levels)).System;
            var front = system.Fronts[0];
            var top = front.LoadBeamLevels.OrderBy(level => level.LevelNumber).Last();
            var cell = front.Levels.OrderBy(level => level.LevelNumber).Last();

            var required = top.EntranceElevation
                           + cell.InOutBeamDepth
                           + (cell.Pallet.Height + cell.ClearHeight) * DynamicHeaderHeightCalculator.TopFinishFraction;

            Assert.Equal(DynamicHeaderHeightCalculator.RoundUpToCommercialFoot(required), front.Height, 6);
        }
    }
}
