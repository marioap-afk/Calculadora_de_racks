using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Geometry;
using RackCad.Application.Systems.Shared;
using RackCad.Application.Views.Placement;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// G14 (AF, AG): ID19 creates linked views, not copies. The logical rack count never changes and no placement
    /// carries a new identity.
    /// </summary>
    public class RackViewCountInvariantTests
    {
        [Fact]
        public void G14_AF_EVERY_PLACEMENT_KEEPS_THE_SOURCE_RACKID()
        {
            var placement = Placements();

            Assert.All(placement, item => Assert.Contains(item.RackId, new[] { G14.RackA, G14.RackB }));
            Assert.Equal(2, placement.Count);
        }

        [Fact]
        public void G14_AG_THE_NUMBER_OF_LOGICAL_RACKS_IS_THE_NUMBER_OF_SOURCE_GROUPS()
        {
            var placement = Placements();

            Assert.Equal(2, placement.Select(item => item.RackId).Distinct(StringComparer.OrdinalIgnoreCase).Count());
        }

        [Fact]
        public void G14_THE_PLACEMENT_CONTRACT_HAS_NO_COPY_OR_RESTAMP_SURFACE()
        {
            var names = typeof(RackProjectedPlacement).Assembly
                .GetTypes()
                .Where(type => type.Namespace == typeof(RackProjectedPlacement).Namespace)
                .SelectMany(type => type.GetMembers())
                .Select(member => member.Name)
                .ToArray();

            Assert.DoesNotContain(names, name => name.Contains("NewRackId", StringComparison.Ordinal));
            Assert.DoesNotContain(names, name => name.Contains("Restamp", StringComparison.Ordinal));
            Assert.DoesNotContain(names, name => name.Contains("CopyName", StringComparison.Ordinal));
            Assert.DoesNotContain(names, name => name.Contains("Clone", StringComparison.Ordinal));
        }

        internal static IReadOnlyList<RackProjectedPlacement> Placements()
        {
            var plan = BuildPlan();
            var placement = plan.Place(new Point3D(0, 0, 0), new Point3D(500, 0, 0));
            Assert.True(placement.IsAvailable);
            return placement.Placements;
        }

        internal static RackGroupPlacementPlan BuildPlan()
        {
            var services = new G14.Services(RackSystemKind.SelectiveRack, G14.SelectiveFacts(fondos: 1, posts: 0));
            services.TargetAddresses.Add(G14.Frontal0);
            foreach (var rackId in new[] { G14.RackA, G14.RackB })
            {
                services.WithFrame(rackId, G14.Planta, G14.PlantaFrame());
                services.WithFrame(rackId, G14.Frontal0, G14.FrontalFrame());
            }

            var selection = G14.Selection(
                new[] { G14.Reference("REF-1", "DEF-1"), G14.Reference("REF-2", "DEF-2") },
                new[] { G14.Definition("DEF-1", G14.RackA), G14.Definition("DEF-2", G14.RackB) });

            var facts = new Dictionary<string, RackSourceTransformFactsResult>
            {
                ["REF-1"] = G14.Facts(x: 0, y: 0),
                ["REF-2"] = G14.Facts(x: 0, y: 100),
            };

            var result = RackGroupPlacementPlan.Create(
                new RackProjectionRequest(selection, DimensionViewKind.Frontal, facts, services.Build()));
            Assert.True(result.IsAvailable);
            return result.Plan;
        }
    }

    /// <summary>
    /// G14: the plan keeps one authored authority per rack and never carries authored payloads of its own.
    /// </summary>
    public class RackViewAuthoredEquivalenceTests
    {
        [Fact]
        public void G14_THE_PLAN_NEVER_EXPOSES_AUTHORED_PAYLOADS()
        {
            var plan = RackViewCountInvariantTests.BuildPlan();

            var members = typeof(RackProjectionGroup).GetProperties().Select(property => property.Name).ToArray();
            Assert.DoesNotContain(members, name => name.Contains("Authored", StringComparison.Ordinal));
            Assert.DoesNotContain(members, name => name.Contains("Design", StringComparison.Ordinal));
            Assert.All(plan.Groups, group => Assert.False(string.IsNullOrWhiteSpace(group.RackId)));
        }

        [Fact]
        public void G14_AUTHORED_EQUIVALENCE_IS_DECIDED_BEFORE_RESOLVE()
        {
            var services = new G14.Services(RackSystemKind.SelectiveRack, G14.SelectiveFacts(fondos: 1, posts: 0));
            services.TargetAddresses.Add(G14.Frontal0);
            services.WithFrame(G14.RackA, G14.Planta, G14.PlantaFrame());
            services.WithFrame(G14.RackA, G14.Frontal0, G14.FrontalFrame());
            services.Authored = _ => RackProjectionAuthoredState.Divergent;

            var selection = G14.Selection(
                new[] { G14.Reference("REF-1", "DEF-1") },
                new[] { G14.Definition("DEF-1", G14.RackA) });
            var facts = new Dictionary<string, RackSourceTransformFactsResult>
            {
                ["REF-1"] = G14.Facts(x: 0, y: 0),
            };

            var result = RackGroupPlacementPlan.Create(
                new RackProjectionRequest(selection, DimensionViewKind.Frontal, facts, services.Build()));

            Assert.False(result.IsAvailable);
            Assert.Empty(services.ResolveCalls);
        }
    }

    /// <summary>
    /// G14: a rack projected from one of its views never gains or loses sibling views in the plan.
    /// </summary>
    public class RackViewPartialRackContractTests
    {
        [Fact]
        public void G14_ONE_SELECTED_VIEW_PRODUCES_EXACTLY_ONE_TARGET_VIEW_PER_RACK()
        {
            var plan = RackViewCountInvariantTests.BuildPlan();

            Assert.All(plan.Groups, group => Assert.Single(group.MemberKeys));
            Assert.Equal(plan.Groups.Count, plan.Views.Count);
        }

        [Fact]
        public void G14_THE_PLAN_ONLY_TARGETS_THE_REQUESTED_CLASS()
        {
            var plan = RackViewCountInvariantTests.BuildPlan();

            Assert.All(plan.Groups, group => Assert.Equal(DimensionViewKind.Frontal, group.TargetAddress.Kind));
            Assert.Equal(DimensionViewKind.Frontal, plan.TargetKind);
        }
    }
}
