using System;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Headers;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using Xunit;

namespace RackCad.Tests
{
    public class FlowBedLateralBuilderTests
    {
        private const string Roller19 = TestCatalogIds.FlowBed.Roller1Point9;
        private const string Roller25 = TestCatalogIds.FlowBed.Roller2Point5;

        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static FlowBedConfiguration Config(FlowBedType type, string rollerId, double laneDepth = 100.0, double palletDepth = 40.0)
            => new FlowBedConfiguration { BedType = type, LaneDepth = laneDepth, PalletDepth = palletDepth, RollerId = rollerId };

        [Fact]
        public void Build_TinyPitchOverride_IsClampedToTheTroquelGrid()
        {
            // Regression: a typo like "0.01" used to drive the placement loop directly -> hundreds of
            // thousands of rollers, freezing the editor. Rollers hook ON troqueles, so the pitch can
            // never go below the grid.
            var config = Config(FlowBedType.Pushback, Roller19);
            config.RollerPitchOverride = 0.01;

            var rollers = new FlowBedLateralBuilder().Build(config, Catalog)
                .Where(i => i.Role == HeaderBlockRole.Roller)
                .OrderBy(i => i.Insertion.X)
                .ToList();

            // 100" lane at >= FlowBedDefaults.TroquelPitch (1") pitch: bounded, never hundreds of thousands.
            Assert.InRange(rollers.Count, 1, 120);
            for (var k = 1; k < rollers.Count; k++)
            {
                Assert.True(rollers[k].Insertion.X - rollers[k - 1].Insertion.X >= FlowBedDefaults.TroquelPitch - 1e-9,
                    "roller pitch fell below the troquel grid");
            }
        }

        [Fact]
        public void Build_PlacesRailWithLongitudAndStopAtTroquelTope()
        {
            var instances = new FlowBedLateralBuilder().Build(Config(FlowBedType.Pushback, Roller19), Catalog);

            var rail = instances.Single(i => i.Role == HeaderBlockRole.Rail);
            Assert.Equal(100.0, rail.DynamicParameters["LONGITUD"], 4);

            var stop = instances.Single(i => i.Role == HeaderBlockRole.Stop);
            Assert.Equal(0.5, stop.Insertion.X, 4);   // TROQUEL_TOPE on the rail
            Assert.Equal(2.75, stop.Insertion.Y, 4);
        }

        [Fact]
        public void Build_FirstRollerSevenInchesFromTroquelTope_OnTheTroquelLine()
        {
            var rollers = new FlowBedLateralBuilder().Build(Config(FlowBedType.Pushback, Roller19), Catalog)
                .Where(i => i.Role == HeaderBlockRole.Roller)
                .OrderBy(i => i.Insertion.X)
                .ToList();

            Assert.Equal(7.5, rollers[0].Insertion.X, 4);   // 0.5 (tope) + 7"
            Assert.All(rollers, r => Assert.Equal(2.75, r.Insertion.Y, 4));
        }

        [Theory]
        [InlineData(Roller19, 2.0)]
        [InlineData(Roller25, 3.0)]
        public void Build_RollerPitch_IsTheMinimumForTheDiameter(string rollerId, double expectedPitch)
        {
            var rollers = new FlowBedLateralBuilder().Build(Config(FlowBedType.Pushback, rollerId), Catalog)
                .Where(i => i.Role == HeaderBlockRole.Roller)
                .OrderBy(i => i.Insertion.X)
                .ToList();

            Assert.Equal(expectedPitch, rollers[1].Insertion.X - rollers[0].Insertion.X, 4);
        }

        [Fact]
        public void Build_Dynamic_WhenBrakeDoesNotFit_FallsBackToPlainRollers()
        {
            // With laneDepth 49 (40" pallet, 1.9" rollers): the first brake is due at offset 48, but its trailing
            // roller would land at 50 > maxOffset 48.5. Instead of stopping the bed at the last pre-brake roller
            // (offset 43, leaving ~5.5" of rail unsupported), the builder falls back to plain rollers to the end.
            var instances = new FlowBedLateralBuilder().Build(Config(FlowBedType.Dynamic, Roller19, laneDepth: 49.0), Catalog);

            Assert.DoesNotContain(instances, i => i.Role == HeaderBlockRole.Brake);

            var maxRoller = instances.Where(i => i.Role == HeaderBlockRole.Roller).Max(i => i.Insertion.X);
            Assert.Equal(47.5, maxRoller, 4); // rollers keep filling the rail past the unfittable brake
        }

        [Fact]
        public void Build_Pushback_HasNoBrakes()
        {
            var instances = new FlowBedLateralBuilder().Build(Config(FlowBedType.Pushback, Roller19), Catalog);

            Assert.DoesNotContain(instances, i => i.Role == HeaderBlockRole.Brake);
        }

        [Fact]
        public void Build_Dynamic_PlacesBrakes_WithTrailingRollerTwoInchesPast()
        {
            var instances = new FlowBedLateralBuilder().Build(Config(FlowBedType.Dynamic, Roller19), Catalog);
            var brakes = instances.Where(i => i.Role == HeaderBlockRole.Brake).OrderBy(i => i.Insertion.X).ToList();

            Assert.NotEmpty(brakes);

            // 40" pallet, 1.9" rollers (2" pitch): first brake clears end of tope + 41" -> offset 48 -> worldX 48.5.
            Assert.Equal(48.5, brakes[0].Insertion.X, 4);

            // A roller resumes 2" past each brake.
            var rollerXs = instances.Where(i => i.Role == HeaderBlockRole.Roller)
                .Select(i => Math.Round(i.Insertion.X, 3)).ToHashSet();
            Assert.Contains(Math.Round(brakes[0].Insertion.X + 2.0, 3), rollerXs);
        }

        [Fact]
        public void Build_BrakeSpacing_IsAboutPalletDepthPlusOne()
        {
            var instances = new FlowBedLateralBuilder().Build(Config(FlowBedType.Dynamic, Roller19, laneDepth: 140.0), Catalog);
            var brakeXs = instances.Where(i => i.Role == HeaderBlockRole.Brake)
                .Select(i => i.Insertion.X).OrderBy(x => x).ToList();

            Assert.True(brakeXs.Count >= 2);
            Assert.Equal(41.0, brakeXs[1] - brakeXs[0], 4); // brake-to-brake ~ pallet depth + 1"
        }
    }
}
