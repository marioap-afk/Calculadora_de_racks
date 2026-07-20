using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RackCad.Application.Catalogs;
using RackCad.Application.RackFrames;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using Xunit;

namespace RackCad.Tests
{
    public sealed class CatalogCanonicalIdsTests
    {
        [Fact]
        public void ShippedCatalogs_MatchCanonicalTestExpectations()
        {
            var catalog = JsonRackCatalogProvider.FromBaseDirectory().Load();
            var templates = RackFrameTemplateProvider.FromBaseDirectory().Load();

            var problems = Validate(catalog, templates);

            Assert.True(problems.Count == 0, FailureMessage(problems));
        }

        internal static IReadOnlyList<string> Validate(
            RackCatalog catalog,
            IReadOnlyList<RackFrameTemplate> templates)
        {
            catalog ??= new RackCatalog();
            templates ??= Array.Empty<RackFrameTemplate>();
            var problems = new List<string>();

            foreach (var expectation in TestCatalogIds.AllCatalogEntries)
            {
                if (!CatalogEntryExists(catalog, templates, expectation))
                {
                    problems.Add($"[ID ausente][{expectation.Collection}] {expectation.Id}");
                }
            }

            foreach (var expectation in TestCatalogIds.EssentialBlocks)
            {
                if (catalog.Blocks.FindBlock(expectation.PieceId, expectation.View) == null)
                {
                    problems.Add(
                        $"[bloque/vista ausente][Blocks] {expectation.PieceId} @ {expectation.View}");
                }
            }

            foreach (var expectation in TestCatalogIds.EssentialConnections)
            {
                if (catalog.ConnectionLayout.FindConnectionLayout(
                        expectation.PieceId,
                        expectation.ConnectionPointId,
                        expectation.View) == null)
                {
                    problems.Add(
                        "[conexión ausente][ConnectionLayout] "
                        + $"{expectation.PieceId} / {expectation.ConnectionPointId} / {expectation.View}");
                }
            }

            ValidateBeamMensulas(catalog, problems);
            ValidateDefaults(catalog, problems);
            ValidateTemplateForeignKeys(catalog, templates, problems);
            ValidateProductionConstants(problems);

            return problems
                .Distinct(StringComparer.Ordinal)
                .OrderBy(problem => problem, StringComparer.Ordinal)
                .ToArray();
        }

        private static bool CatalogEntryExists(
            RackCatalog catalog,
            IReadOnlyList<RackFrameTemplate> templates,
            TestCatalogIds.CatalogExpectation expectation)
        {
            return expectation.Collection switch
            {
                TestCatalogIds.CatalogCollection.PostProfiles => ContainsId(catalog.PostProfiles, expectation.Id),
                TestCatalogIds.CatalogCollection.TrussProfiles => ContainsId(catalog.TrussProfiles, expectation.Id),
                TestCatalogIds.CatalogCollection.BeamProfiles => ContainsId(catalog.BeamProfiles, expectation.Id),
                TestCatalogIds.CatalogCollection.SpacerProfiles => ContainsId(catalog.SpacerProfiles, expectation.Id),
                TestCatalogIds.CatalogCollection.BasePlates => ContainsId(catalog.BasePlates, expectation.Id),
                TestCatalogIds.CatalogCollection.Mensulas => ContainsId(catalog.Mensulas, expectation.Id),
                TestCatalogIds.CatalogCollection.FlowBedProfiles => ContainsId(catalog.FlowBedProfiles, expectation.Id),
                TestCatalogIds.CatalogCollection.SafetyElements => ContainsId(catalog.SafetyElements, expectation.Id),
                TestCatalogIds.CatalogCollection.ConnectionPoints => ContainsId(catalog.ConnectionPoints, expectation.Id),
                TestCatalogIds.CatalogCollection.Views => ContainsId(catalog.Views, expectation.Id),
                TestCatalogIds.CatalogCollection.Templates => templates.Any(template =>
                    Matches(template?.Id, expectation.Id)),
                _ => false
            };
        }

        private static bool ContainsId<TEntry>(IEnumerable<TEntry> entries, string id)
            where TEntry : CatalogEntryBase
        {
            return entries?.Any(entry => Matches(entry?.Id, id)) == true;
        }

        private static void ValidateBeamMensulas(RackCatalog catalog, ICollection<string> problems)
        {
            foreach (var expectation in TestCatalogIds.EssentialBeamMensulas)
            {
                var beam = catalog.BeamProfiles.FirstOrDefault(entry => Matches(entry?.Id, expectation.BeamId));
                if (beam != null && !Matches(beam.Mensula, expectation.MensulaId))
                {
                    problems.Add(
                        "[FK esencial ausente][BeamProfiles.Mensula] "
                        + $"{expectation.BeamId} -> {expectation.MensulaId}; actual={Display(beam.Mensula)}");
                }
            }
        }

        private static void ValidateDefaults(RackCatalog catalog, ICollection<string> problems)
        {
            var defaults = catalog.Defaults ?? new RackDefaults();
            ExpectValue(problems, "Defaults.Post", TestCatalogIds.Profiles.Posts.Standard, defaults.Post);
            ExpectValue(problems, "Defaults.BasePlate", TestCatalogIds.BasePlates.Standard, defaults.BasePlate);
            ExpectValue(
                problems,
                "Defaults.DiagonalProfile",
                TestCatalogIds.Profiles.Truss.Standard,
                defaults.DiagonalProfile);
            ExpectValue(
                problems,
                "Defaults.HorizontalProfile",
                TestCatalogIds.Profiles.Truss.Standard,
                defaults.HorizontalProfile);
            ExpectValue(
                problems,
                "Defaults.BraceStartConnectionPoint",
                TestCatalogIds.ConnectionPoints.TrussPunch,
                defaults.BraceStartConnectionPoint);
            ExpectValue(
                problems,
                "Defaults.BraceEndConnectionPoint",
                TestCatalogIds.ConnectionPoints.TrussEnd,
                defaults.BraceEndConnectionPoint);
            ExpectValue(
                problems,
                "Defaults.BasePlateConnectionPoint",
                TestCatalogIds.ConnectionPoints.PostMount,
                defaults.BasePlateConnectionPoint);
        }

        private static void ValidateTemplateForeignKeys(
            RackCatalog catalog,
            IReadOnlyList<RackFrameTemplate> templates,
            ICollection<string> problems)
        {
            var expectedIds = new HashSet<string>(
                TestCatalogIds.Templates.Entries.Select(entry => entry.Id),
                StringComparer.OrdinalIgnoreCase);

            foreach (var template in templates.Where(template => template != null && expectedIds.Contains(template.Id)))
            {
                var post = ValueOrDefault(template.Post, catalog.Defaults?.Post);
                RequireForeignKey(problems, template.Id, "Post", post, catalog.PostProfiles);

                var basePlate = ValueOrDefault(template.BasePlate, catalog.Defaults?.BasePlate);
                RequireForeignKey(problems, template.Id, "BasePlate", basePlate, catalog.BasePlates);

                var diagonal = ValueOrDefault(template.DiagonalProfile, catalog.Defaults?.DiagonalProfile);
                RequireForeignKey(problems, template.Id, "DiagonalProfile", diagonal, catalog.TrussProfiles);

                var braceStart = ValueOrDefault(
                    template.BraceStartConnectionPoint,
                    catalog.Defaults?.BraceStartConnectionPoint);
                RequireForeignKey(problems, template.Id, "BraceStartConnectionPoint", braceStart, catalog.ConnectionPoints);

                var braceEnd = ValueOrDefault(
                    template.BraceEndConnectionPoint,
                    catalog.Defaults?.BraceEndConnectionPoint);
                RequireForeignKey(problems, template.Id, "BraceEndConnectionPoint", braceEnd, catalog.ConnectionPoints);

                var horizontals = template.Horizontals ?? Array.Empty<TemplateHorizontal>();
                for (var index = 0; index < horizontals.Count; index++)
                {
                    var profile = ValueOrDefault(horizontals[index]?.Profile, catalog.Defaults?.HorizontalProfile);
                    RequireForeignKey(
                        problems,
                        template.Id,
                        $"Horizontals[{index}].Profile",
                        profile,
                        catalog.TrussProfiles);
                }
            }
        }

        private static void RequireForeignKey<TEntry>(
            ICollection<string> problems,
            string owner,
            string field,
            string expectedId,
            IEnumerable<TEntry> entries)
            where TEntry : CatalogEntryBase
        {
            if (!ContainsId(entries, expectedId))
            {
                problems.Add(
                    $"[FK esencial ausente][Templates.{field}] {owner} -> {Display(expectedId)}");
            }
        }

        private static void ValidateProductionConstants(ICollection<string> problems)
        {
            ExpectProductionConstant(problems, "CatalogIds.StandardPost", TestCatalogIds.Profiles.Posts.Standard, CatalogIds.StandardPost);
            ExpectProductionConstant(problems, "CatalogIds.BasePlate", TestCatalogIds.BasePlates.Standard, CatalogIds.BasePlate);
            ExpectProductionConstant(problems, "CatalogIds.TrussProfile", TestCatalogIds.Profiles.Truss.Standard, CatalogIds.TrussProfile);
            ExpectProductionConstant(problems, "CatalogIds.LowerHorizontal", TestCatalogIds.Profiles.Truss.Standard, CatalogIds.LowerHorizontal);
            ExpectProductionConstant(problems, "CatalogIds.IntermediateHorizontal", TestCatalogIds.Profiles.Truss.Standard, CatalogIds.IntermediateHorizontal);
            ExpectProductionConstant(problems, "CatalogIds.UpperHorizontal", TestCatalogIds.Profiles.Truss.Standard, CatalogIds.UpperHorizontal);
            ExpectProductionConstant(problems, "CatalogIds.Diagonal", TestCatalogIds.Profiles.Truss.Standard, CatalogIds.Diagonal);
            ExpectProductionConstant(problems, "CatalogIds.BraceStartConnectionPoint", TestCatalogIds.ConnectionPoints.TrussPunch, CatalogIds.BraceStartConnectionPoint);
            ExpectProductionConstant(problems, "CatalogIds.BraceEndConnectionPoint", TestCatalogIds.ConnectionPoints.TrussEnd, CatalogIds.BraceEndConnectionPoint);
            ExpectProductionConstant(problems, "CatalogIds.BasePlateConnectionPoint", TestCatalogIds.ConnectionPoints.PostMount, CatalogIds.BasePlateConnectionPoint);

            ExpectProductionConstant(problems, "DynamicRackDefaults.InOutBeamCatalogId", TestCatalogIds.Profiles.Beams.DynamicInOut, DynamicRackDefaults.InOutBeamCatalogId);
            ExpectProductionConstant(problems, "DynamicRackDefaults.InOutBeamView", TestCatalogIds.Views.Lateral, DynamicRackDefaults.InOutBeamView);
            ExpectProductionConstant(problems, "DynamicRackDefaults.InOutBeamBedMatePoint", TestCatalogIds.ConnectionPoints.BedMate, DynamicRackDefaults.InOutBeamBedMatePoint);
            ExpectProductionConstant(problems, "DynamicRackDefaults.IntermediateBeamCatalogId", TestCatalogIds.Profiles.Beams.DynamicIntermediate, DynamicRackDefaults.IntermediateBeamCatalogId);
            ExpectProductionConstant(problems, "DynamicRackDefaults.IntermediateBeamView", TestCatalogIds.Views.Lateral, DynamicRackDefaults.IntermediateBeamView);
            ExpectProductionConstant(problems, "DynamicRackDefaults.IntermediateBeamLeftBedMatePoint", TestCatalogIds.ConnectionPoints.LeftStart, DynamicRackDefaults.IntermediateBeamLeftBedMatePoint);
            ExpectProductionConstant(problems, "DynamicRackDefaults.IntermediateBeamRightBedMatePoint", TestCatalogIds.ConnectionPoints.RightStart, DynamicRackDefaults.IntermediateBeamRightBedMatePoint);
            ExpectProductionConstant(problems, "DynamicRackDefaults.SeparatorCatalogId", TestCatalogIds.Profiles.Spacers.Header, DynamicRackDefaults.SeparatorCatalogId);
            ExpectProductionConstant(problems, "DynamicRackDefaults.SeparatorView", TestCatalogIds.Views.Front, DynamicRackDefaults.SeparatorView);
            ExpectProductionConstant(problems, "DynamicRackDefaults.SeparatorPostPoint", TestCatalogIds.ConnectionPoints.SpacerPunch, DynamicRackDefaults.SeparatorPostPoint);
            ExpectProductionConstant(problems, "DynamicRackDefaults.SeparatorMatePoint", TestCatalogIds.ConnectionPoints.HeaderPunch, DynamicRackDefaults.SeparatorMatePoint);

            ExpectProductionConstant(problems, "FlowBedDefaults.RailId", TestCatalogIds.FlowBed.Rail, FlowBedDefaults.RailId);
            ExpectProductionConstant(problems, "FlowBedDefaults.StopId", TestCatalogIds.FlowBed.Stop, FlowBedDefaults.StopId);
            ExpectProductionConstant(problems, "FlowBedDefaults.BrakeId", TestCatalogIds.FlowBed.Brake, FlowBedDefaults.BrakeId);
            ExpectProductionConstant(problems, "FlowBedDefaults.RollerId", TestCatalogIds.FlowBed.Roller1Point9, FlowBedDefaults.RollerId);
            ExpectProductionConstant(problems, "FlowBedDefaults.View", TestCatalogIds.Views.Lateral, FlowBedDefaults.View);
            ExpectProductionConstant(problems, "FlowBedDefaults.RailTopePoint", TestCatalogIds.ConnectionPoints.StopPunch, FlowBedDefaults.RailTopePoint);
            ExpectProductionConstant(problems, "FlowBedDefaults.RailInOutMatePoint", TestCatalogIds.ConnectionPoints.RailInOut, FlowBedDefaults.RailInOutMatePoint);

            ExpectProductionConstant(problems, "SelectiveRackDefaults.View", TestCatalogIds.Views.Front, SelectiveRackDefaults.View);
            ExpectProductionConstant(problems, "SelectiveRackDefaults.PostBeamPoint", TestCatalogIds.ConnectionPoints.BeamPunch, SelectiveRackDefaults.PostBeamPoint);
            ExpectProductionConstant(problems, "SelectiveRackDefaults.BeamProfileStartPoint", TestCatalogIds.ConnectionPoints.ProfileStart, SelectiveRackDefaults.BeamProfileStartPoint);
            ExpectProductionConstant(problems, "SelectiveRackDefaults.PlateMatePoint", TestCatalogIds.ConnectionPoints.PostMount, SelectiveRackDefaults.PlateMatePoint);
            ExpectProductionConstant(problems, "SelectiveRackDefaults.PalletPieceId", TestCatalogIds.BlockOnlyPieces.Pallet, SelectiveRackDefaults.PalletPieceId);

            ExpectProductionConstant(problems, "RackFrameTemplateCatalog.StandardTemplateId", TestCatalogIds.Templates.Standard, RackFrameTemplateCatalog.StandardTemplateId);
            ExpectProductionConstant(problems, "DynamicForkliftDefensePlan.PostOriginPoint", TestCatalogIds.ConnectionPoints.PostOrigin, DynamicForkliftDefensePlan.PostOriginPoint);
        }

        private static void ExpectValue(
            ICollection<string> problems,
            string owner,
            string expected,
            string actual)
        {
            if (!Matches(expected, actual))
            {
                problems.Add(
                    $"[FK esencial ausente][{owner}] esperado={expected}; actual={Display(actual)}");
            }
        }

        private static void ExpectProductionConstant(
            ICollection<string> problems,
            string name,
            string expected,
            string actual)
        {
            if (!string.Equals(expected, actual, StringComparison.Ordinal))
            {
                problems.Add(
                    "[constante de producción divergente] "
                    + $"{name}: test={expected}; producto={Display(actual)}");
            }
        }

        private static string FailureMessage(IReadOnlyList<string> problems)
        {
            if (problems == null || problems.Count == 0)
            {
                return string.Empty;
            }

            var message = new StringBuilder();
            message.AppendLine($"Los catálogos distribuidos incumplen {problems.Count} expectativa(s) canónica(s):");
            foreach (var problem in problems)
            {
                message.Append("- ").AppendLine(problem);
            }

            return message.ToString();
        }

        private static bool Matches(string left, string right)
        {
            return string.Equals(left, right, StringComparison.OrdinalIgnoreCase);
        }

        private static string ValueOrDefault(string value, string fallback)
        {
            return string.IsNullOrWhiteSpace(value) ? fallback : value;
        }

        private static string Display(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "<vacío>" : value;
        }
    }
}
