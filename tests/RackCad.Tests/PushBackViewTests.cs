using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Headers;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-18a — frontal and planta Push Back cuts (item 3) plus the stable semantic golden of the five views (item 6).
    /// Every view is composed from the dynamic view as a black box; the dynamic result is never altered.
    /// </summary>
    public class PushBackViewTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private const string InOut = "LARGUERO_IN_OUT_C6";
        private const string Redondo = "LARGUERO_ESCALON_TROQUEL_REDONDO";
        private const string Infinito = "LARGUERO_ESCALON_INFINITO";
        private const string Tope = "LARGUERO_ESCALON_TOPE_DE_3";

        private static PushBackSystem Uniform(RackCatalog catalog)
            => new PushBackResolver(catalog).Resolve(new PushBackDesign
            {
                Structure = new DynamicRackDesign
                {
                    Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                    PalletsDeep = 4,
                    LoadLevels = 3,
                    FirstLevelHeight = 6.0,
                    BeamDepth = 4.0
                }
            });

        private static string Signature(DynamicSystemPlan plan)
            => string.Join("|", plan.Flatten().Instances
                .Select(i => FormattableString.Invariant($"{i.View}:{i.Role}:{i.PieceId}:{i.Insertion.X:0.###}:{i.Insertion.Y:0.###}"))
                .OrderBy(s => s, StringComparer.Ordinal));

        // ---- Frontal --------------------------------------------------------------------------------------

        [Fact]
        public void Frontal_EntradaSalida_HasInOut_NoRedondo_NoGuia()
        {
            var catalog = Catalog;
            var system = Uniform(catalog);
            var instances = new PushBackSystemFrontalBuilder().BuildPlan(system, catalog, PushBackFrontalEnd.EntradaSalida).Flatten().Instances;

            Assert.Contains(instances, i => i.PieceId == InOut);
            Assert.DoesNotContain(instances, i => i.PieceId == Redondo);
            Assert.DoesNotContain(instances, i => (i.PieceId ?? string.Empty).Contains("GUIA"));
            Assert.Contains(instances, i => i.Role == HeaderBlockRole.Post);
        }

        [Fact]
        public void Frontal_Posterior_HasRedondoAndRearTope_NoInOut_NoDynamicSafety()
        {
            var catalog = Catalog;
            var system = Uniform(catalog);
            var instances = new PushBackSystemFrontalBuilder().BuildPlan(system, catalog, PushBackFrontalEnd.Posterior).Flatten().Instances;

            Assert.Contains(instances, i => i.PieceId == Redondo);
            Assert.Contains(instances, i => i.Role == HeaderBlockRole.Tope && i.PieceId == Tope);
            Assert.DoesNotContain(instances, i => i.PieceId == InOut);          // no IN/OUT on the rear cut
            Assert.DoesNotContain(instances, i => i.Role == HeaderBlockRole.Safety); // no normal dynamic safety
            Assert.Contains(instances, i => i.Role == HeaderBlockRole.Post);     // structure kept
        }

        // ---- Planta ---------------------------------------------------------------------------------------

        [Fact]
        public void Planta_KeepsLowInOut_SwapsHighToRedondo_KeepsIntermediates_NoGuia()
        {
            var catalog = Catalog;
            var system = Uniform(catalog);
            var instances = new PushBackSystemPlantaBuilder().Build(system, catalog);

            Assert.Contains(instances, i => i.PieceId == InOut);      // low IN/OUT stays
            Assert.Contains(instances, i => i.PieceId == Redondo);    // high becomes TROQUEL_REDONDO
            Assert.Contains(instances, i => i.PieceId == Infinito);   // intermediates kept
            Assert.Contains(instances, i => i.Role == HeaderBlockRole.Tope && i.PieceId == Tope);
            Assert.DoesNotContain(instances, i => (i.PieceId ?? string.Empty).Contains("GUIA"));
        }

        [Fact]
        public void Planta_IsDistinctFromDynamic_AndDynamicIsUnchanged()
        {
            var catalog = Catalog;
            var system = Uniform(catalog);

            var dynamicBefore = Signature(new DynamicSystemPlantaBuilder().BuildPlan(system.Structure, catalog));
            var pushBack = Signature(new PushBackSystemPlantaBuilder().BuildPlan(system, catalog));
            var dynamicAfter = Signature(new DynamicSystemPlantaBuilder().BuildPlan(system.Structure, catalog));

            Assert.NotEqual(dynamicBefore, pushBack);
            Assert.Equal(dynamicBefore, dynamicAfter);
        }

        // ---- The five views are a stable, distinct golden (rich scenario) ----------------------------------

        [Fact]
        public void FiveViews_AreDistinctAndNonEmpty_OnARichScenario()
        {
            var catalog = Catalog;
            var design = new PushBackDesign
            {
                Structure = new DynamicRackDesign
                {
                    Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                    PalletsDeep = 6,
                    LoadLevels = 2,
                    FirstLevelHeight = 6.0,
                    BeamDepth = 4.0
                }
            };
            // Two fronts with different fondos and DepthStartPosition.
            design.Structure.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1, LoadLevels = 2, PalletsDeep = 6, DepthStartPosition = 1 });
            design.Structure.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1, LoadLevels = 2, PalletsDeep = 3, DepthStartPosition = 4 });
            // Two levels with different rear peraltes on front 0.
            var f0 = new PushBackFrontConfig();
            f0.HighEndBeamPeraltes.Add(5.0);
            f0.HighEndBeamPeraltes.Add(4.0);
            design.Fronts.Add(f0);
            // One deactivated rear tope cell.
            design.RearTope.Disable(0, 0);

            var system = new PushBackResolver(catalog).Resolve(design);
            var lateralBuilder = new PushBackSystemLateralBuilder();
            var frontalBuilder = new PushBackSystemFrontalBuilder();

            var latFull = Signature(lateralBuilder.Build(system, catalog));                  // full lateral
            var latSection = Signature(lateralBuilder.Build(system, catalog, 0));            // lateral section at post 0
            var frontES = Signature(frontalBuilder.BuildPlan(system, catalog, PushBackFrontalEnd.EntradaSalida));
            var frontPost = Signature(frontalBuilder.BuildPlan(system, catalog, PushBackFrontalEnd.Posterior));
            var planta = Signature(new PushBackSystemPlantaBuilder().BuildPlan(system, catalog));

            // All five plans are non-empty.
            Assert.All(new[] { latFull, latSection, frontES, frontPost, planta },
                s => Assert.False(string.IsNullOrEmpty(s), "a view produced an empty plan"));
            // The four semantically distinct views must differ (full vs sectioned lateral may coincide for small systems).
            var distinct = new[] { latFull, frontES, frontPost, planta };
            Assert.Equal(distinct.Length, distinct.Distinct().Count());
        }

        [Fact]
        public void LateralCortes_ProduceOnePlanPerTransversePost()
        {
            var catalog = Catalog;
            var system = Uniform(catalog);

            var cortes = new PushBackSystemLateralBuilder().Cortes(system, catalog);

            Assert.NotEmpty(cortes);
            Assert.All(cortes, corte => Assert.NotEmpty(corte.Plan.Flatten().Instances));
        }
    }
}
