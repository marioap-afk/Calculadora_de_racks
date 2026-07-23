using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Headers;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-18a — Push Back geometry + SystemPlan composition (items 2-4). The lateral plan is composed from the dynamic
    /// lateral plan as a BLACK BOX: the common structure is kept, the dynamic-specific pieces are removed by Role/PieceId,
    /// and the Push Back pieces are added (low IN/OUT, high TROQUEL_REDONDO per cell, tangent intermediates, pushback bed,
    /// rear topes). Golden is the order-independent physical fingerprint; the dynamic plan is never altered.
    /// </summary>
    public class PushBackPlanTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private const string InOut = "LARGUERO_IN_OUT_C6";
        private const string Redondo = "LARGUERO_ESCALON_TROQUEL_REDONDO";
        private const string Infinito = "LARGUERO_ESCALON_INFINITO";
        private const string Tope = "LARGUERO_ESCALON_TOPE_DE_3";

        private static DynamicRackDesign Structure(int palletsDeep = 4, int loadLevels = 3)
            => new DynamicRackDesign
            {
                Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                PalletsDeep = palletsDeep,
                LoadLevels = loadLevels,
                FirstLevelHeight = 6.0,
                BeamDepth = 4.0
            };

        private static PushBackSystem System(RackCatalog catalog)
            => new PushBackResolver(catalog).Resolve(new PushBackDesign { Structure = Structure() });

        private static string Signature(DynamicSystemPlan plan)
            => string.Join("|", plan.Flatten().Instances
                .Select(i => FormattableString.Invariant($"{i.View}:{i.Role}:{i.PieceId}:{i.Insertion.X:0.###}:{i.Insertion.Y:0.###}"))
                .OrderBy(s => s, StringComparer.Ordinal));

        // ---- End beams: IN/OUT only low, TROQUEL_REDONDO only high, snapped ---------------------------------

        [Fact]
        public void EndBeams_InOutOnlyAtLowEnd_TroquelRedondoOnlyAtHighEnd()
        {
            var catalog = Catalog;
            var system = System(catalog);
            var instances = new PushBackSystemLateralBuilder().Build(system, catalog).Flatten().Instances;

            var inOut = instances.Where(i => i.PieceId == InOut).ToList();
            var redondo = instances.Where(i => i.PieceId == Redondo).ToList();

            Assert.NotEmpty(inOut);
            Assert.NotEmpty(redondo);
            // IN/OUT is only at the low end (X == StartX == 0); TROQUEL_REDONDO only at the high end (X == TotalLength).
            Assert.All(inOut, i => Assert.Equal(0.0, i.Insertion.X, 3));
            Assert.All(redondo, i => Assert.Equal(system.TotalLength, i.Insertion.X, 3));
            // one of each per load level
            Assert.Equal(system.Structure.LoadBeamLevels.Count, inOut.Count);
            Assert.Equal(system.Structure.LoadBeamLevels.Count, redondo.Count);
        }

        [Fact]
        public void EndBeams_ElevationsMatchTheResolverSnappedLevels()
        {
            var catalog = Catalog;
            var system = System(catalog);
            var instances = new PushBackSystemLateralBuilder().Build(system, catalog).Flatten().Instances;

            // The low/high beam elevations are exactly the resolver's snapped exit/entrance elevations (2" troquel grid).
            var exitYs = system.Structure.LoadBeamLevels.Select(l => Math.Round(l.ExitElevation, 3)).OrderBy(y => y).ToList();
            var entranceYs = system.Structure.LoadBeamLevels.Select(l => Math.Round(l.EntranceElevation, 3)).OrderBy(y => y).ToList();

            var lowYs = instances.Where(i => i.PieceId == InOut).Select(i => Math.Round(i.Insertion.Y, 3)).OrderBy(y => y).ToList();
            var highYs = instances.Where(i => i.PieceId == Redondo).Select(i => Math.Round(i.Insertion.Y, 3)).OrderBy(y => y).ToList();

            Assert.Equal(exitYs, lowYs);
            Assert.Equal(entranceYs, highYs);
        }

        [Fact]
        public void HighBeam_PeralteIsThePerCellResolvedValue()
        {
            var catalog = Catalog;
            var design = new PushBackDesign { Structure = Structure(loadLevels: 2) };
            design.Structure.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1, LoadLevels = 2, PalletsDeep = 4 });
            var f0 = new PushBackFrontConfig();
            f0.HighEndBeamPeraltes.Add(5.0);
            f0.HighEndBeamPeraltes.Add(4.0);
            design.Fronts.Add(f0);
            var system = new PushBackResolver(catalog).Resolve(design);

            var redondo = new PushBackSystemLateralBuilder().Build(system, catalog).Flatten().Instances
                .Where(i => i.PieceId == Redondo)
                .ToList();

            var peraltes = redondo.Select(i => i.DynamicParameters[SelectiveRackDefaults.PeralteParam]).OrderBy(p => p).ToList();
            Assert.Contains(5.0, peraltes);
            Assert.Contains(4.0, peraltes);
        }

        // ---- Bed axis: low = IN/OUT TROQUEL_CAMA, high = TROQUEL_REDONDO INICIO ------------------------------

        [Fact]
        public void BedAxis_RunsFromInOutTroquelCamaToRedondoInicio()
        {
            var catalog = Catalog;
            var system = System(catalog);

            var axes = PushBackFlowBedGeometry.Resolve(system, catalog);
            Assert.NotEmpty(axes);

            var lowCama = CatalogLookup.Local(catalog, InOut, "TROQUEL_CAMA", "LATERAL");
            var highInicio = CatalogLookup.Local(catalog, Redondo, "INICIO_DERECHO", "LATERAL");
            var level0 = system.Structure.LoadBeamLevels[0];
            var axis0 = axes.First(a => a.LevelNumber == 1);

            // Low mate = IN/OUT TROQUEL_CAMA at the exit (StartX=0, exit elevation).
            Assert.Equal(0.0 + lowCama.X, axis0.ExitMate.X, 3);
            Assert.Equal(level0.ExitElevation + lowCama.Y, axis0.ExitMate.Y, 3);
            // High mate = TROQUEL_REDONDO INICIO_DERECHO at the entrance (EndX, mirrored, entrance elevation).
            Assert.Equal(system.TotalLength - highInicio.X, axis0.HighMate.X, 3);
            Assert.Equal(level0.EntranceElevation + highInicio.Y, axis0.HighMate.Y, 3);
            Assert.True(axis0.HighMate.X > axis0.ExitMate.X); // bed runs low(left) -> high(right)
        }

        [Fact]
        public void Intermediates_AreInfinito_AndTangentToThePushBackAxis()
        {
            var catalog = Catalog;
            var system = System(catalog);
            var axes = PushBackFlowBedGeometry.Resolve(system, catalog);

            var intermediates = new PushBackSystemLateralBuilder().Build(system, catalog).Flatten().Instances
                .Where(i => i.PieceId == Infinito)
                .ToList();

            Assert.NotEmpty(intermediates);
            // Each intermediate's left/right contact lands exactly on the Push Back rail-origin line (tangency).
            var left = CatalogLookup.Local(catalog, Infinito, "INICIO_IZQUIERDO", "LATERAL");
            var right = CatalogLookup.Local(catalog, Infinito, "INICIO_DERECHO", "LATERAL");
            foreach (var beam in intermediates)
            {
                var axis = axes.First(a => a.LevelNumber == 1); // level-1 axis for this single-level check subset
                var mate = beam.MirroredX ? right : left;
                var contactX = beam.Insertion.X + (beam.MirroredX ? -mate.X : mate.X);
                var expectedY = axis.RailOriginYAt(contactX) - mate.Y;
                // The beam is tangent when its insertion Y sits on the axis line for SOME resolved axis (its own level).
                var tangentToSomeLevel = axes.Any(a =>
                    Math.Abs((a.RailOriginYAt(beam.Insertion.X + (beam.MirroredX ? -mate.X : mate.X)) - mate.Y) - beam.Insertion.Y) < 1e-6);
                Assert.True(tangentToSomeLevel, "intermediate not tangent to any Push Back axis line");
            }
        }

        // ---- Lateral plan composition -----------------------------------------------------------------------

        [Fact]
        public void LateralPlan_RemovesDynamicSpecifics_KeepsStructure_AddsPushBackPieces()
        {
            var catalog = Catalog;
            var system = System(catalog);
            var instances = new PushBackSystemLateralBuilder().Build(system, catalog).Flatten().Instances;

            // Structure kept.
            Assert.Contains(instances, i => i.Role == HeaderBlockRole.Post);
            Assert.Contains(instances, i => i.Role == HeaderBlockRole.Separator);
            // Push Back pieces added.
            Assert.Contains(instances, i => i.PieceId == InOut);           // low IN/OUT
            Assert.Contains(instances, i => i.PieceId == Redondo);          // high TROQUEL_REDONDO
            Assert.Contains(instances, i => i.PieceId == Infinito);         // intermediates
            Assert.Contains(instances, i => i.Role == HeaderBlockRole.Rail); // pushback bed
            Assert.Contains(instances, i => i.Role == HeaderBlockRole.Tope && i.PieceId == Tope); // rear tope
            // NO brakes, NO GUIA anywhere.
            Assert.DoesNotContain(instances, i => i.Role == HeaderBlockRole.Brake);
            Assert.DoesNotContain(instances, i => (i.PieceId ?? string.Empty).Contains("GUIA"));
        }

        [Fact]
        public void LateralPlan_IsDistinctFromTheDynamicPlan_AndDynamicPlanIsUnchanged()
        {
            var catalog = Catalog;
            var system = System(catalog);

            var dynamicBefore = Signature(new DynamicSystemLateralBuilder().Build(system.Structure, catalog));
            var pushBack = Signature(new PushBackSystemLateralBuilder().Build(system, catalog));
            var dynamicAfter = Signature(new DynamicSystemLateralBuilder().Build(system.Structure, catalog));

            Assert.NotEqual(dynamicBefore, pushBack);       // Push Back plan differs from the dynamic one
            Assert.Equal(dynamicBefore, dynamicAfter);       // the dynamic builder output is unchanged (not mutated)
        }

        [Fact]
        public void RearTope_ActiveByDefault_DeactivablePerCell()
        {
            var catalog = Catalog;
            var system = System(catalog);
            var activeCount = new PushBackSystemLateralBuilder().Build(system, catalog).Flatten().Instances
                .Count(i => i.Role == HeaderBlockRole.Tope);
            Assert.True(activeCount > 0);

            // Deactivate one cell and confirm the rear tope count drops by exactly one.
            system.RearTope.Disable(0, 0);
            var afterCount = new PushBackSystemLateralBuilder().Build(system, catalog).Flatten().Instances
                .Count(i => i.Role == HeaderBlockRole.Tope);
            Assert.Equal(activeCount - 1, afterCount);
        }

        [Fact]
        public void Bed_IsPushback_FullSpan_NoBrakes()
        {
            var catalog = Catalog;
            var system = System(catalog);
            var instances = new PushBackSystemLateralBuilder().Build(system, catalog).Flatten().Instances;

            Assert.Contains(instances, i => i.Role == HeaderBlockRole.Rail);
            Assert.Contains(instances, i => i.Role == HeaderBlockRole.Roller);
            Assert.Contains(instances, i => i.Role == HeaderBlockRole.Stop);
            Assert.DoesNotContain(instances, i => i.Role == HeaderBlockRole.Brake);
            var rail = instances.First(i => i.Role == HeaderBlockRole.Rail);
            Assert.Equal(system.TotalLength, rail.DynamicParameters[SelectiveRackDefaults.LengthParam], 3);
        }
    }
}
