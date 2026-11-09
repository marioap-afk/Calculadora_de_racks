using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.RackFrames;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.PushBack;
using RackCad.Application.Systems.Selective;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-67 — reproduccion de H58 contra el resolver real y el catalogo real. Cada intervalo entre niveles debe
    /// quedar en el primer troquel real que satisface su separacion requerida, medido desde la elevacion RESUELTA
    /// del nivel anterior. Con la tarima de 33" y el larguero C6 de 6" el requisito es 45": sobre una reticula de 2"
    /// el resultado correcto es 46" en TODOS los intervalos.
    /// </summary>
    public class LevelSpacingH58ReproductionTests
    {
        private const double RequiredInterval = 45.0;

        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static DynamicRackDesign Structure(int levels, double firstLevelHeight)
            => new DynamicRackDesign
            {
                // 33" de tarima + 6" de larguero C6 + 6" de holgura = 45" de separacion requerida.
                Pallet = new PalletSpecification(42.0, 48.0, 33.0, 1000.0, "kg"),
                PalletsDeep = 4,
                LoadLevels = levels,
                FirstLevelHeight = firstLevelHeight,
                BeamDepth = 6.0,
                HeaderPostCatalogId = TestCatalogIds.Profiles.Posts.Standard
            };

        private static IReadOnlyList<double> Spacings(IEnumerable<DynamicLoadBeamLevel> levels)
        {
            var exits = levels.OrderBy(level => level.LevelNumber).Select(level => level.ExitElevation).ToList();
            return exits.Zip(exits.Skip(1), (lower, upper) => Math.Round(upper - lower, 6)).ToList();
        }

        [Fact]
        public void Dynamic_UniformRequirement_IsFortySixOnEveryInterval()
        {
            var system = new DynamicRackSystemResolver(Catalog).Resolve(Structure(4, 6.0)).System;

            Assert.Equal(new[] { 46.0, 46.0, 46.0 }, Spacings(system.LoadBeamLevels));
        }

        [Fact]
        public void PushBack_UniformRequirement_IsFortySixOnEveryInterval()
        {
            var design = new PushBackDesign { Structure = Structure(4, 6.0) };
            design.Structure.Fronts.Add(new DynamicRackFrontDesign
            {
                PalletCount = 1, LoadLevels = 4, PalletsDeep = 4, DepthStartPosition = 1
            });
            design.Fronts.Add(new PushBackFrontConfig());

            var system = new PushBackResolver(Catalog).Resolve(design);

            Assert.Equal(new[] { 46.0, 46.0, 46.0 }, Spacings(system.Structure.Fronts[0].LoadBeamLevels));
        }

        [Fact]
        public void ResolveLevels_NeverLeavesAnIntervalBelowItsRequirement()
        {
            foreach (var first in new[] { 0.0, 2.5, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0, 9.25, 12.0 })
            {
                var spacings = Spacings(new DynamicRackSystemResolver(Catalog).Resolve(Structure(6, first)).System.LoadBeamLevels);

                Assert.All(spacings, spacing => Assert.True(
                    spacing >= RequiredInterval - 1e-6,
                    $"primer nivel {first}: separaciones [{string.Join(", ", spacings)}], requerido {RequiredInterval}"));
            }
        }
    }
}
