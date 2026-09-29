using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using RackCad.Application.Persistence;
using RackCad.Application.RackFrames;
using RackCad.Application.Systems.Shared;
using RackCad.Application.Views.Batch;
using RackCad.Application.Views.Preparation;
using RackCad.Application.Views.Redraw;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems.Cantilever;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Shared;
using Xunit;
using static RackCad.Tests.DimensionViewScenarios;

namespace RackCad.Tests
{
    /// <summary>
    /// G16 corrective round (Owner Validation rejected beb9597b). Two defects, each pinned by the evidence that reproduced it:
    ///
    /// <para>
    /// C16-02 (root cause): the inner Design that every first-view path persists is CORRECT for its kind (proved below with the
    /// production readers). What failed was <c>RackUnsupportedSiblingInsert.TryAuthorize</c> handing RACKEDITAR the definitions of
    /// EVERY rack scanned in the drawing instead of the ones of the rack being edited, so the existing, authoritative
    /// wrong-kind preflight found «another kind» in an unrelated rack. The preflight is untouched.
    /// </para>
    /// <para>
    /// C16-01 (root cause): the request the editor menu hands the Plugin lost its ordered views (a Selective Lateral with no
    /// section decodes to nothing, and the Selective and Dynamic modules rebuilt the request from the legacy single view and
    /// dropped a batch), so the new-rack batch saw an empty request and reported ONE_RACK_REQUIRED (0/0). The
    /// one-logical-rack rule is untouched and still applies.
    /// </para>
    /// </summary>
    public class G16FirstViewCorrectiveTests
    {
        private const string RackA = "aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa";
        private const string RackB = "bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb";
        private const string RackC = "cccccccc-cccc-4ccc-8ccc-cccccccccccc";

        // ================================================================ C16-02: what is persisted is right

        public static IEnumerable<object[]> FirstViews()
        {
            foreach (var view in new[]
            {
                (RackSystemKind.PalletFlow, RackViewAddress.FlowEnd(RackFlowEnd.Entrance)),
                (RackSystemKind.PalletFlow, RackViewAddress.FlowEnd(RackFlowEnd.Exit)),
                (RackSystemKind.PalletFlow, RackViewAddress.Post(0)),
                (RackSystemKind.PalletFlow, RackViewAddress.Whole(DimensionViewKind.Planta)),
                (RackSystemKind.Selective, RackViewAddress.Whole(DimensionViewKind.Lateral)),
                (RackSystemKind.Selective, RackViewAddress.Whole(DimensionViewKind.Planta)),
                (RackSystemKind.PushBack, RackViewAddress.Whole(DimensionViewKind.Planta)),
                (RackSystemKind.PushBack, RackViewAddress.Post(0)),
                (RackSystemKind.PushBack, RackViewAddress.PushBackCut(RackPushBackEnd.EntradaSalida, RackPushBackSide.A)),
                (RackSystemKind.PushBack, RackViewAddress.PushBackCut(RackPushBackEnd.Posterior, RackPushBackSide.A)),
                (RackSystemKind.Cantilever, RackViewAddress.Whole(DimensionViewKind.Planta)),
                (RackSystemKind.Cantilever, RackViewAddress.Whole(DimensionViewKind.Frontal)),
                (RackSystemKind.Cantilever, RackViewAddress.Station(0)),
            })
            {
                yield return new object[] { view.Item1, view.Item2 };
            }
        }

        [Theory]
        [MemberData(nameof(FirstViews))]
        public void G16_C02_EveryFirstViewPersistsAnInnerDesignOfTheExpectedKind(RackSystemKind kind, RackViewAddress address)
        {
            var envelope = FirstViewEnvelope(kind, address, RackA);

            // The persisted envelope, read back like the drawing hands it to RACKEDITAR.
            var persisted = new RackEmbedStore().Deserialize(new RackEmbedStore().Serialize(envelope));
            Assert.Equal(RackViewCodec.Encode(kind, address).Kind, persisted.Kind);
            Assert.Equal(RackA, persisted.Id);

            // View is presentation: the authored source kind is the SAME for every view of the system.
            var resolution = new RackProjectStore().ResolveInnerSource(persisted.Design, kind, null);
            Assert.Equal(InnerSourceOutcome.Success, resolution.Outcome);
            Assert.Equal(kind, resolution.Project.Kind);
        }

        [Fact]
        public void G16_C02_TheAuthoredKindAndSchemaDoNotDependOnTheViewThatWasPlacedFirst()
        {
            foreach (var kind in new[] { RackSystemKind.PalletFlow, RackSystemKind.Selective, RackSystemKind.PushBack, RackSystemKind.Cantilever })
            {
                var projects = FirstViews().Where(row => (RackSystemKind)row[0] == kind)
                    .Select(row => new RackProjectStore().Deserialize(
                        FirstViewEnvelope(kind, (RackViewAddress)row[1], RackA).Design))
                    .ToList();

                Assert.NotEmpty(projects);
                Assert.All(projects, project => Assert.Equal(kind, project.Kind));
                Assert.Single(projects.Select(project => project.SourceDocument.SchemaVersion).Distinct());
            }
        }

        [Fact]
        public void G16_C02_TheWrongKindPreflightStaysFailClosed()
        {
            var dynamic = FirstViewEnvelope(RackSystemKind.PalletFlow, RackViewAddress.Post(0), RackA).Design;
            var pushBack = FirstViewEnvelope(RackSystemKind.PushBack, RackViewAddress.Whole(DimensionViewKind.Planta), RackB).Design;

            var mixed = new RackProjectStore().PreflightInnerSources(
                new[] { dynamic, pushBack }, RackSystemKind.PalletFlow, null);

            Assert.True(mixed.Aborted);
            Assert.Equal(InnerSourceOutcome.WrongKind, mixed.BlockingOutcome);
        }

        // ================================================================ C16-02 root cause: which definitions are handed to RACKEDITAR

        [Fact]
        public void G16_C02_ARacksMutableMembersAreOnlyItsOwnDefinitions()
        {
            var scan = TwoRacksDrawing();

            var membership = RackSiblingMembership.Classify(scan.Facts, RackA, null, false);

            Assert.Equal(new[] { "A1", "A2" }, membership.MutableMembers.Select(m => m.Fact.DefinitionKey).OrderBy(k => k).ToArray());
            // The complete member list also carries every OTHER definition scanned, as NotMember: it is not a list of blocks to edit.
            Assert.Contains(membership.Members, m => m.Kind == RackSiblingMembershipKind.NotMember);
            Assert.True(membership.Members.Count > membership.MutableMembers.Count);
        }

        [Fact]
        public void G16_C02_HandingRackeditarEveryScannedDefinitionReproducesTheOwnerFailure()
        {
            var scan = TwoRacksDrawing();
            var membership = RackSiblingMembership.Classify(scan.Facts, RackA, null, false);
            var store = new RackProjectStore();

            // What beb9597b did: every member of the scan (other racks included).
            var everything = membership.Members.Select(m => scan.Designs[m.Fact.DefinitionKey]).ToList();
            var faulty = store.PreflightInnerSources(everything, RackSystemKind.PalletFlow, null);
            Assert.True(faulty.Aborted);
            Assert.Equal(InnerSourceOutcome.WrongKind, faulty.BlockingOutcome);

            // What RACKEDITAR must be given: the rack's own definitions. Same authoritative preflight, no loosening.
            var own = membership.MutableMembers.Select(m => scan.Designs[m.Fact.DefinitionKey]).ToList();
            var fixedPreflight = store.PreflightInnerSources(own, RackSystemKind.PalletFlow, null);
            Assert.False(fixedPreflight.Aborted);
        }

        [Fact]
        public void G16_C02_AnotherRackOfTheSameKindIsNeverRedrawnWithThisRacksDesign()
        {
            var scan = TwoRacksDrawing();
            var membership = RackSiblingMembership.Classify(scan.Facts, RackA, null, false);

            Assert.DoesNotContain(membership.MutableMembers, m => m.Fact.DefinitionKey == "C1");
        }

        [Fact]
        public void G16_C02_TheBlockListOfEveryExistingRackInsertIsTheMutableMembersOnly()
        {
            var source = Code("src/RackCad.Plugin/Views/RackUnsupportedSiblingInsert.cs");

            Assert.Contains("snapshot.Membership.MutableMembers", source);
            Assert.Equal(1, Regex.Matches(source, Regex.Escape("snapshot.Membership.Members")).Count);
            Assert.Matches(new Regex(@"MutableMembers\)\s*\r?\n\s*if \(snapshot\.Definitions\.TryGetValue"), source);
        }

        [Fact]
        public void G16_C02_ThePreflightOfInnerSourcesIsUntouched()
        {
            var preflight = Code("src/RackCad.Application/Persistence/RackProjectStore.cs");

            Assert.Contains("return source.Kind == expectedKind", preflight);
            Assert.Contains("InnerSourceResolution.WrongKind()", preflight);
            Assert.Contains("BenignFallback(initiating)", preflight);
        }

        // ================================================================ cross-system round trip: first view -> reopen -> another sister

        public static IEnumerable<object[]> FirstViewAndSister()
        {
            var rows = FirstViews().Select(row => ((RackSystemKind)row[0], (RackViewAddress)row[1])).ToList();
            foreach (var first in rows)
                foreach (var sister in rows.Where(r => r.Item1 == first.Item1 && !r.Item2.Equals(first.Item2)))
                    yield return new object[] { first.Item1, first.Item2, sister.Item2 };
        }

        [Theory]
        [MemberData(nameof(FirstViewAndSister))]
        public void G16_RoundTrip_EveryFirstViewReopensAndAcceptsEveryOtherSupportedSister(
            RackSystemKind kind, RackViewAddress first, RackViewAddress sister)
        {
            var firstEnvelope = FirstViewEnvelope(kind, first, RackA);
            var sisterEnvelope = FirstViewEnvelope(kind, sister, RackA);
            var otherKinds = new[] { RackSystemKind.PalletFlow, RackSystemKind.Selective, RackSystemKind.PushBack, RackSystemKind.Cantilever }
                .Where(other => other != kind).ToList();

            // The drawing: the rack being edited (two views) plus one unrelated rack of every other kind.
            var facts = new List<RackSiblingScanFact>
            {
                new RackSiblingScanFact("FIRST", firstEnvelope.View, true, false, true, RackA, RackA, 1, 0, false),
                new RackSiblingScanFact("SISTER", sisterEnvelope.View, false, false, true, RackA, RackA, 1, 0, false),
            };
            var designs = new Dictionary<string, string> { ["FIRST"] = firstEnvelope.Design, ["SISTER"] = sisterEnvelope.Design };
            foreach (var other in otherKinds)
            {
                var key = "OTHER-" + other;
                var id = Guid.NewGuid().ToString();
                facts.Add(new RackSiblingScanFact(key, "planta", false, false, true, id, id, 1, 0, false));
                designs[key] = FirstViewEnvelope(other, RackViewAddress.Whole(DimensionViewKind.Planta), id).Design;
            }

            var membership = RackSiblingMembership.Classify(facts, RackA, null, false);
            var store = new RackProjectStore();
            var initiating = store.Deserialize(firstEnvelope.Design);

            // What RACKEDITAR is handed: this rack's blocks. The unchanged wrong-kind preflight recognises the SAME kind.
            var blocks = membership.MutableMembers.Select(m => designs[m.Fact.DefinitionKey]).ToList();
            Assert.Equal(2, blocks.Count);
            var preflight = store.PreflightInnerSources(blocks, kind, initiating);
            Assert.False(preflight.Aborted);
            Assert.All(preflight.ResolvedSources, project => Assert.Equal(kind, project.Kind));

            // And the beb9597b input (every scanned definition) is exactly what tripped the preflight: it stays fail-closed.
            var everything = membership.Members.Select(m => designs[m.Fact.DefinitionKey]).ToList();
            Assert.True(store.PreflightInnerSources(everything, kind, initiating).Aborted);
        }

        [Fact]
        public void G16_RoundTrip_SelectiveEveryFirstViewPersistsAReadableAuthoredDesign()
        {
            var design = SelectiveDesign(DimensionDetail.Standard, twoFondos: true);
            var json = new SelectivePalletDesignStore().Serialize(SelectivePalletDesignDocument.From(design, RackA, "Rack"));

            foreach (var address in new[]
            {
                RackViewAddress.Fondo(0), RackViewAddress.Fondo(1), RackViewAddress.Post(0), RackViewAddress.Whole(DimensionViewKind.Planta),
            })
            {
                var syntax = RackViewCodec.Encode(RackSystemKind.SelectiveRack, address);
                var envelope = RackEmbedComposer.Compose(null, syntax.Kind, RackA, "Rack", syntax.View, syntax.Section, json);
                var persisted = new RackEmbedStore().Deserialize(new RackEmbedStore().Serialize(envelope));

                Assert.Equal(RackA, persisted.Id);
                Assert.Equal(RackA, new SelectivePalletDesignStore().Deserialize(persisted.Design).Id);
                Assert.True(RackViewCodec.Decode(persisted.Kind, persisted.View, persisted.Section).HasAddress);
            }
        }

        // ================================================================ C16-01: the request keeps its views, and one rack is still required

        [Fact]
        public void G16_C01_TheMenuModulesReturnTheWindowsRequestInsteadOfRebuildingIt()
        {
            var modules = Code("src/RackCad.UI/Editor/EditorModules.cs");

            Assert.DoesNotContain("new SelectiveInsertionRequest(", modules);
            Assert.DoesNotContain("new DynamicInsertionRequest(", modules);
            Assert.True(Regex.Matches(modules, Regex.Escape("window.InsertionRequest")).Count >= 3);
        }

        [Fact]
        public void G16_C01_ANewRackWithOneLateralViewNeedsNoExistingRack()
        {
            var port = new RecordingPort();
            var request = new RackViewBatchRequest(
                RackProductSourceKind.NewRack,
                RackSystemKind.SelectiveRack,
                new[] { new RackViewBatchItem(RackA, RackViewAddress.Post(0)) });

            var plan = RackViewBatchPlan<string>.Execute(request, port);

            Assert.Equal(RackViewBatchOutcome.COMPLETED, plan.Outcome);
            Assert.DoesNotContain("gate", port.Calls);
            Assert.DoesNotContain("redraw", port.Calls);
            Assert.Equal(new[] { RackViewAddress.Post(0) }, plan.Report.PlacedOrder);
        }

        [Theory]
        [InlineData(RackProductSourceKind.NewRack)]
        [InlineData(RackProductSourceKind.ExistingRack)]
        public void G16_C01_AnEmptyRequestStillFailsWithOneRackRequiredInBothFlows(RackProductSourceKind source)
        {
            var port = new RecordingPort();
            var plan = RackViewBatchPlan<string>.Execute(
                new RackViewBatchRequest(source, RackSystemKind.SelectiveRack, Array.Empty<RackViewBatchItem>()), port);

            Assert.Equal(RackViewBatchOutcome.PREFLIGHT_FAILED, plan.Outcome);
            Assert.Equal(RackViewBatchStopReason.MULTIPLE_RACK_IDENTITIES, plan.Report.StopReason);
            Assert.Equal("ONE_RACK_REQUIRED", plan.Report.Diagnostic);
            Assert.Empty(port.Calls);
        }

        [Theory]
        [InlineData(RackProductSourceKind.NewRack)]
        [InlineData(RackProductSourceKind.ExistingRack)]
        public void G16_C01_TwoRacksStillFailWithOneRackRequiredInBothFlows(RackProductSourceKind source)
        {
            var port = new RecordingPort();
            var plan = RackViewBatchPlan<string>.Execute(
                new RackViewBatchRequest(source, RackSystemKind.SelectiveRack, new[]
                {
                    new RackViewBatchItem(RackA, RackViewAddress.Post(0)),
                    new RackViewBatchItem(RackB, RackViewAddress.Whole(DimensionViewKind.Planta)),
                }), port);

            Assert.Equal("ONE_RACK_REQUIRED", plan.Report.Diagnostic);
            Assert.Empty(port.Calls);
        }

        [Fact]
        public void G16_C01_TheExistingRackFlowStillRunsItsSiblingGate()
        {
            var port = new RecordingPort();
            var plan = RackViewBatchPlan<string>.Execute(
                new RackViewBatchRequest(
                    RackProductSourceKind.ExistingRack,
                    RackSystemKind.SelectiveRack,
                    new[] { new RackViewBatchItem(RackA, RackViewAddress.Post(0)) }),
                port);

            Assert.Equal(RackViewBatchOutcome.COMPLETED, plan.Outcome);
            Assert.Contains("gate", port.Calls);
        }

        [Fact]
        public void G16_C01_TheOneRackRuleIsStillInThePlanAndNotRemoved()
        {
            var plan = Code("src/RackCad.Application/Views/Batch/RackViewBatchPlan.cs");

            Assert.Equal(2, Regex.Matches(plan, Regex.Escape("\"ONE_RACK_REQUIRED\"")).Count);
            Assert.Contains("original.Count == 0 || !OneIdentity(original, out var rackId)", plan);
        }

        // ================================================================ helpers

        private static RackEmbedDocument FirstViewEnvelope(RackSystemKind kind, RackViewAddress address, string rackId)
        {
            var syntax = RackViewCodec.Encode(kind, address);
            string design;
            switch (kind)
            {
                case RackSystemKind.PalletFlow:
                    design = new RackProjectStore().Serialize(RackProject.ForDynamic(DynamicPersistedDesign(DimensionDetail.Standard, Catalog)));
                    break;
                case RackSystemKind.Selective:
                    design = new RackProjectStore().Serialize(RackProject.ForSelective(new HardcodedStandardRackFrameService().CreateDefault()));
                    break;
                case RackSystemKind.PushBack:
                    design = new RackProjectStore().Serialize(RackProject.ForPushBack(PushBackSingleSidedDesign(DimensionDetail.Standard)));
                    break;
                default:
                    design = new RackProjectStore().Serialize(RackProject.ForCantilever(CantileverDesign()));
                    break;
            }

            return RackEmbedComposer.Compose(null, syntax.Kind, rackId, "Rack", syntax.View, syntax.Section, design);
        }

        private static CantileverLineDesign CantileverDesign()
        {
            var topology = new CantileverLineStationTopologyDesign
            {
                FaceMode = CantileverStationFaceMode.Single,
                SingleSide = CantileverArmSide.PositiveY,
                LevelCount = 2,
                RequestedClearHeight = 24.0,
                ColumnBaseTemplate = new CantileverStationColumnBaseTemplateDesign
                {
                    ColumnSectionId = "AISC-W-W10X33",
                    Base = new CantileverBaseDesign { SectionId = "AISC-W-W12X26", Length = 48.0 }
                }
            };
            topology.ColumnBaseTemplate.Connection.Punches.ColumnBottomPlateEndOffset = 1.5;
            topology.ColumnBaseTemplate.Connection.Punches.ColumnTopPunchOffset = 4.0;

            return new CantileverLineDesign
            {
                Name = "Linea A",
                StationCount = 3,
                ColumnCentreSpacing = 96.0,
                StationTopology = topology,
                DefaultArmTemplate = new CantileverArmTemplateDesign
                {
                    Body = new CantileverArmBodyDesign
                    {
                        Arrangement = CantileverArmBodyArrangement.Single,
                        SectionId = "AISC-HSS-RECT-HSS4X4X_250",
                        CutLength = 36.0
                    },
                    MountingPlate = new CantileverArmMountingPlateTemplateDesign
                    {
                        VerticalPunchCount = 2,
                        VerticalEndOffset = 1.5
                    }
                },
                Bracing = new CantileverBracingDesign()
            };
        }

        private sealed class Drawing
        {
            internal IReadOnlyList<RackSiblingScanFact> Facts { get; set; }
            internal IReadOnlyDictionary<string, string> Designs { get; set; }
        }

        /// <summary>Rack A (Dynamic, two views), rack B (Push Back) and rack C (another Dynamic) in one drawing.</summary>
        private static Drawing TwoRacksDrawing()
        {
            var designs = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["A1"] = FirstViewEnvelope(RackSystemKind.PalletFlow, RackViewAddress.Post(0), RackA).Design,
                ["A2"] = FirstViewEnvelope(RackSystemKind.PalletFlow, RackViewAddress.Whole(DimensionViewKind.Planta), RackA).Design,
                ["B1"] = FirstViewEnvelope(RackSystemKind.PushBack, RackViewAddress.Whole(DimensionViewKind.Planta), RackB).Design,
                ["C1"] = FirstViewEnvelope(RackSystemKind.PalletFlow, RackViewAddress.Post(0), RackC).Design,
            };
            RackSiblingScanFact Fact(string key, string rack, bool selected)
                => new RackSiblingScanFact(key, "lateral", selected, false, true, rack, rack, 1, 0, false);
            return new Drawing
            {
                Designs = designs,
                Facts = new[]
                {
                    Fact("A1", RackA, true), Fact("A2", RackA, false), Fact("B1", RackB, false), Fact("C1", RackC, false),
                },
            };
        }

        private static string Code(string relative)
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "RackCad.sln"))) dir = dir.Parent;
            Assert.NotNull(dir);
            var text = File.ReadAllText(Path.Combine(dir.FullName, relative.Replace('/', Path.DirectorySeparatorChar)));
            return string.Join("\n", text.Split('\n').Select(line =>
            {
                var comment = line.IndexOf("//", StringComparison.Ordinal);
                return comment >= 0 ? line.Substring(0, comment) : line;
            }));
        }

        private sealed class RecordingPort : IRackViewBatchPort<string>
        {
            internal List<string> Calls { get; } = new List<string>();

            public bool TryAcceptVariant(RackViewBatchItem requested, out RackViewBatchItem accepted, out string diagnostic)
            {
                Calls.Add("variant");
                accepted = requested;
                diagnostic = null;
                return true;
            }

            public RackViewBatchGateResult CheckSiblingGate(RackViewBatchRequest request)
            {
                Calls.Add("gate");
                return RackViewBatchGateResult.Accept();
            }

            public RackViewBatchPreparation<string> Prepare(RackViewBatchItem item)
            {
                Calls.Add("prepare");
                return RackViewBatchPreparation<string>.Success(item.Address.ToString());
            }

            public RackViewBatchRedrawResult RedrawExisting(IReadOnlyList<string> prepared)
            {
                Calls.Add("redraw");
                return RackViewBatchRedrawResult.Applied();
            }

            public RackViewBatchPlacementResult Place(RackViewBatchItem item, string prepared)
            {
                Calls.Add("place");
                return RackViewBatchPlacementResult.Placed(null);
            }
        }
    }
}
