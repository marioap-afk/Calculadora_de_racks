using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using RackCad.Application.Bom;
using RackCad.Application.Catalogs;
using RackCad.Application.Expressions;
using RackCad.Application.Persistence;
using RackCad.Application.ProjectVariables;
using RackCad.Application.Systems.Selective;
using RackCad.Domain.Systems.Selective;
using Xunit;
using Xunit.Abstractions;

namespace RackCad.Tests
{
    /// <summary>I-49 G11: candidate proof through the persisted, accredited, evaluated and downstream real chain.</summary>
    public sealed class G11RealChainCandidateTests
    {
        private const string RackId = "3f2b1c9e-6d4a-4f38-9b71-0c2a5e8d1f44";
        private const string X = "8A1D4E772C934B608F156E0B93A7C221"; // A2: exact N-layout text is authoritative.
        private const string Y = "{11111111-2222-3333-4444-555555555555}"; // A2: exact B-layout text.
        private const string PostId = "POSTE_OMEGA_ATORNILLABLE_CON_TROQUEL_GOTA_DE_AGUA";
        private const string BeamId = "LARGUERO_ESCALON_CAL14_3_REMACHES";

        [Fact]
        [Trait("Gate", "G11-RealChain")]
        public void PERSISTED_MIXED_SOURCES_REACH_REAL_GEOMETRY_AND_BOM_AND_PROPAGATE()
        {
            var registryRead = RoundTripRegistry(Registry(10.0));
            var authored = RoundTripDesign(Authored());

            Assert.Equal(SelectivePropertyValueDocument.ProjectVariableKind,
                authored.PropertyValues[ProjectPropertyIds.SelectiveVerticalClearanceToken].Kind);
            Assert.Equal(X, authored.PropertyValues[ProjectPropertyIds.SelectiveVerticalClearanceToken].VariableId);
            Assert.Equal(SelectivePropertyValueDocument.ExpressionKind,
                authored.PropertyValues[ProjectPropertyIds.SelectivePalletToleranceToken].Kind);
            Assert.Equal(Y, Assert.IsType<BoundReference>(
                Assert.IsType<BoundBinary>(authored.PropertyValues[ProjectPropertyIds.SelectivePalletToleranceToken]
                    .Expression).Left).Symbol.Key);

            var accreditation = UsableProjectVariablesRegistry.Accredit(registryRead);
            Assert.True(accreditation.IsUsable, accreditation.Error);
            var evaluation = RegistryEvaluation.Evaluate(ProjectVariablesExpressionAdapter.From(accreditation.Registry));
            Assert.True(evaluation.Result(SymbolId.ProjectVariable(Y)).Succeeded);
            Assert.Equal(12.0, evaluation.Result(SymbolId.ProjectVariable(Y)).Value);
            Assert.Contains(evaluation.Context.Symbols.Entries, entry => entry.Id.Key == X);
            Assert.Contains(evaluation.Context.Symbols.Entries, entry => entry.Id.Key == Y);

            var effective = Resolve(authored, registryRead);
            Assert.Equal(10.0, effective.VerticalClearance);
            Assert.Equal(8.0, effective.PalletTolerance);

            var literal = RoundTripDesign(AuthoredLiteral(10.0, 8.0));
            var linkedGeometry = ResolveGeometry(effective);
            var literalGeometry = ResolveGeometry(literal.ToDomain());
            Assert.Equal(literalGeometry.Height, linkedGeometry.Height);
            Assert.Equal(literalGeometry.Bays[0].BeamLength, linkedGeometry.Bays[0].BeamLength);
            var linkedSignature = BomSignature(effective);
            Assert.Equal(BomSignature(literal.ToDomain()), linkedSignature);

            var changed = Resolve(authored, RoundTripRegistry(Registry(14.0)));
            Assert.Equal(14.0, changed.VerticalClearance);
            Assert.Equal(12.0, changed.PalletTolerance);
            var changedGeometry = ResolveGeometry(changed);
            Assert.NotEqual(linkedGeometry.Bays[0].Levels[1].Y, changedGeometry.Bays[0].Levels[1].Y);
            Assert.NotEqual(linkedGeometry.Bays[0].BeamLength, changedGeometry.Bays[0].BeamLength);
            Assert.NotEqual(linkedSignature, BomSignature(changed));
        }

        [Fact]
        [Trait("Gate", "G11-RealChain")]
        public void BROKEN_AND_CYCLIC_CHAINS_FAIL_CLOSED_BEFORE_GEOMETRY_OR_BOM()
        {
            var authored = RoundTripDesign(Authored());
            var broken = Registry(10.0);
            broken.Variables.RemoveAt(0); // Y still names X; X is absent.

            AssertNoDownstream(authored, RoundTripRegistry(broken));

            var cyclic = Registry(10.0);
            cyclic.Variables[0].Definition = ExpressionDefinition(Reference(Y));
            AssertNoDownstream(authored, RoundTripRegistry(cyclic));

            Assert.Equal(6.0, authored.VerticalClearance); // frozen authored values were never fallback output.
            Assert.Equal(4.0, authored.PalletTolerance);
        }

        private static ProjectVariablesReadResult RoundTripRegistry(ProjectVariablesDocument document)
        {
            var store = new ProjectVariablesStore();
            var read = store.Deserialize(store.Serialize(document));
            Assert.Equal(ProjectVariablesReadOutcome.Readable, read.Outcome);
            var x = read.Document.Variables.FirstOrDefault(entry => entry.Name == "X");
            if (x != null)
            {
                Assert.Equal(X, x.VariableId);
            }
            Assert.Equal(Y, read.Document.Variables.First(entry => entry.Name == "Y").VariableId);
            return read;
        }

        private static SelectivePalletDesignDocument RoundTripDesign(SelectivePalletDesignDocument document)
        {
            var store = new SelectivePalletDesignStore();
            return store.Deserialize(store.Serialize(document));
        }

        private static SelectivePalletDesign Resolve(
            SelectivePalletDesignDocument authored,
            ProjectVariablesReadResult registry)
        {
            var result = new SelectiveEffectiveDesignResolver().ResolveAccredited(authored, registry);
            Assert.True(result.IsSuccess, result.Error);
            return result.Design;
        }

        private static void AssertNoDownstream(
            SelectivePalletDesignDocument authored,
            ProjectVariablesReadResult registry)
        {
            var result = new SelectiveEffectiveDesignResolver().ResolveAccredited(authored, registry);
            Assert.False(result.IsSuccess);
            Assert.Null(result.Design);
            Assert.Null(TryBom(result));
        }

        private static BillOfMaterials TryBom(SelectiveEffectiveResolution result)
            => result.IsSuccess ? BuildBom(result.Design) : null;

        private static string BomSignature(SelectivePalletDesign design)
            => string.Join("|", BuildBom(design).Lines.Select(line =>
                line.Category + ":" + line.ProfileId + ":" +
                line.Length.ToString("0.###", CultureInfo.InvariantCulture) + "x" + line.Quantity));

        private static BillOfMaterials BuildBom(SelectivePalletDesign design)
        {
            var catalog = JsonRackCatalogProvider.FromBaseDirectory().Load();
            var system = new SelectiveGeometryResolver().Resolve(design, catalog);
            return SelectiveBomBuilder.Build(system, catalog);
        }

        private static SelectiveRackSystem ResolveGeometry(SelectivePalletDesign design)
            => new SelectiveGeometryResolver().Resolve(
                design,
                JsonRackCatalogProvider.FromBaseDirectory().Load());

        private static ProjectVariablesDocument Registry(double x)
        {
            var document = ProjectVariablesDocument.CreateNew();
            document.Variables = new List<ProjectVariableDocument>
            {
                Variable(X, "X", new ProjectVariableDefinitionDocument { Kind = "literal", Value = x }),
                Variable(Y, "Y", ExpressionDefinition(Add(Reference(X), BoundExpression.Number(2.0)))),
            };
            return document;
        }

        private static ProjectVariableDocument Variable(
            string id,
            string name,
            ProjectVariableDefinitionDocument definition)
            => new ProjectVariableDocument
            {
                VariableId = id,
                Name = name,
                Type = VariableType.Length.ToString(),
                Definition = definition,
            };

        private static ProjectVariableDefinitionDocument ExpressionDefinition(BoundExpression expression)
            => ProjectVariableDefinitionDocument.From(G8ContractTestSupport.CreateExpressionDefinition(expression));

        private static BoundExpression Reference(string id)
            => BoundExpression.Reference(SymbolId.ProjectVariable(id));

        private static BoundExpression Add(BoundExpression left, BoundExpression right)
            => BoundExpression.Binary(BoundBinaryOperator.Add, left, right);

        private static BoundExpression Subtract(BoundExpression left, BoundExpression right)
            => BoundExpression.Binary(BoundBinaryOperator.Subtract, left, right);

        private static SelectivePalletDesignDocument Authored()
        {
            var document = SelectivePalletDesignDocument.From(Design(6.0, 4.0), RackId, "Rack G11");
            document.SchemaVersion = SelectivePalletDesignDocument.PromotedSchemaVersion;
            document.PropertyValues = new Dictionary<string, SelectivePropertyValueDocument>
            {
                [ProjectPropertyIds.SelectiveVerticalClearanceToken] =
                    SelectivePropertyValueDocument.ToProjectVariable(X),
                [ProjectPropertyIds.SelectivePalletToleranceToken] =
                    SelectivePropertyValueDocument.FromExpression(Subtract(Reference(Y), BoundExpression.Number(4.0))),
            };
            return document;
        }

        private static SelectivePalletDesignDocument AuthoredLiteral(double clearance, double tolerance)
            => SelectivePalletDesignDocument.From(Design(clearance, tolerance), RackId, "Rack G11 literal");

        private static SelectivePalletDesign Design(double clearance, double tolerance)
        {
            var design = new SelectivePalletDesign
            {
                PostId = PostId,
                PostPeralte = 3.0,
                VerticalClearance = clearance,
                PalletTolerance = tolerance,
                FloorBeamRise = 4.0,
                PalletDepth = 48.0,
                DepthCount = 1,
                DrawBasePlate = true,
            };
            var bay = new SelectiveBayDesign { FloorBeam = true };
            for (var level = 0; level < 2; level++)
            {
                bay.Levels.Add(new SelectiveCell
                {
                    Pallet = new Tarima { Frente = 42.0, Alto = 60.0 },
                    PalletCount = 2,
                    BeamId = BeamId,
                    BeamPeralte = 4.0,
                });
            }
            design.Bays.Add(bay);
            return design;
        }
    }

    /// <summary>Deterministic G11 scaling measurement. Timing is evidence, never a pass/fail SLA.</summary>
    public sealed class G11PerformanceMeasurementTests
    {
        private readonly ITestOutputHelper _output;

        public G11PerformanceMeasurementTests(ITestOutputHelper output) => _output = output;

        [Theory]
        [InlineData("SMALL", 32, 8, 3)]
        [InlineData("MEDIUM", 128, 32, 3)]
        [InlineData("STRESS", 384, 96, 3)]
        [Trait("Gate", "G11-Performance")]
        public void DETERMINISTIC_CHAIN_FANOUT_MEASUREMENT(
            string scale,
            int variableCount,
            int rackCount,
            int viewsPerRack)
        {
            var fixture = PerformanceFixture.Create(variableCount, rackCount, viewsPerRack);

            Measure(() => RegistryEvaluation.Evaluate(fixture.Context)); // warmup
            Measure(() => fixture.Evaluation.DependencyGraph.TransitiveDependents(fixture.Root));
            Measure(() => ProjectVariableConsumerDiscovery.DiscoverConsumers(fixture.Entries, fixture.Affected));
            Measure(() => ProjectVariableMutationPreflight.ChangeDefinition(
                fixture.Registry, fixture.RootVariable, VariableDefinition.Literal(10.5), fixture.Entries));

            var registryTimes = Timings(() => RegistryEvaluation.Evaluate(fixture.Context));
            var closureTimes = Timings(() => fixture.Evaluation.DependencyGraph.TransitiveDependents(fixture.Root));
            var discoveryTimes = Timings(() =>
                ProjectVariableConsumerDiscovery.DiscoverConsumers(fixture.Entries, fixture.Affected));
            VariableMutationPreflightResult last = null;
            var preflightTimes = Timings(() => last = ProjectVariableMutationPreflight.ChangeDefinition(
                fixture.Registry, fixture.RootVariable, VariableDefinition.Literal(10.5), fixture.Entries));

            Assert.All(fixture.Evaluation.Results.Values, result => Assert.True(result.Succeeded));
            Assert.Equal(variableCount, fixture.Affected.Count);
            var discovery = ProjectVariableConsumerDiscovery.DiscoverConsumers(fixture.Entries, fixture.Affected);
            Assert.True(discovery.IsSuccess, discovery.Error);
            Assert.Equal(rackCount, discovery.Consumers.Count);
            Assert.True(last.IsSuccess, last.Error);
            Assert.Equal(rackCount, last.Plan.RackMutations.Count);
            var readCount = last.Plan.PlanReadSet.SymbolResultObservations.Count +
                            last.Plan.PlanReadSet.RepairDecisionObservations.Count;
            Assert.True(readCount > 0);

            _output.WriteLine(
                $"G11 PERF {scale}: V={variableCount}; E={variableCount - 1}; racks={rackCount}; " +
                $"scanEntries={fixture.Entries.Count}; expressionConsumers={rackCount}; " +
                $"affectedClosure={fixture.Affected.Count}; PlanReadSet={readCount}");
            Write("RegistryEvaluation", registryTimes);
            Write("AffectedClosure", closureTimes);
            Write("ConsumerDiscovery", discoveryTimes);
            Write("ChangeDefinitionPreflight", preflightTimes);
        }

        private void Write(string operation, double[] samples)
        {
            Array.Sort(samples);
            _output.WriteLine(
                $"  {operation}: min={samples[0]:0.###} ms; median={samples[samples.Length / 2]:0.###} ms; " +
                $"max={samples[samples.Length - 1]:0.###} ms; iterations={samples.Length}");
        }

        private static double[] Timings(Action action)
            => Enumerable.Range(0, 7).Select(_ => Measure(action)).ToArray();

        private static double Measure(Action action)
        {
            var stopwatch = Stopwatch.StartNew();
            action();
            stopwatch.Stop();
            return stopwatch.Elapsed.TotalMilliseconds;
        }

        private sealed class PerformanceFixture
        {
            private PerformanceFixture(
                ProjectVariablesDocument registry,
                ExpressionContext context,
                RegistryEvaluation evaluation,
                VariableId rootVariable,
                IReadOnlyList<ProjectVariableScanEntry> entries,
                IReadOnlyList<VariableId> affected)
            {
                Registry = registry;
                Context = context;
                Evaluation = evaluation;
                RootVariable = rootVariable;
                Root = SymbolId.ProjectVariable(rootVariable.Value);
                Entries = entries;
                Affected = affected;
            }

            internal ProjectVariablesDocument Registry { get; }
            internal ExpressionContext Context { get; }
            internal RegistryEvaluation Evaluation { get; }
            internal VariableId RootVariable { get; }
            internal SymbolId Root { get; }
            internal IReadOnlyList<ProjectVariableScanEntry> Entries { get; }
            internal IReadOnlyList<VariableId> Affected { get; }

            internal static PerformanceFixture Create(int variableCount, int rackCount, int viewsPerRack)
            {
                var registry = ProjectVariablesDocument.CreateNew();
                registry.Variables = new List<ProjectVariableDocument>();
                for (var index = 0; index < variableCount; index++)
                {
                    var id = Id(index);
                    var definition = index == 0
                        ? new ProjectVariableDefinitionDocument { Kind = "literal", Value = 10.0 }
                        : ProjectVariableDefinitionDocument.From(G8ContractTestSupport.CreateExpressionDefinition(
                            BoundExpression.Binary(
                                BoundBinaryOperator.Add,
                                BoundExpression.Reference(SymbolId.ProjectVariable(Id(index - 1))),
                                BoundExpression.Number(0.01))));
                    registry.Variables.Add(new ProjectVariableDocument
                    {
                        VariableId = id,
                        Name = "V" + index.ToString(CultureInfo.InvariantCulture),
                        Type = VariableType.Length.ToString(),
                        Definition = definition,
                    });
                }

                var accreditation = UsableProjectVariablesRegistry.Accredit(ProjectVariablesReadResult.Readable(registry));
                Assert.True(accreditation.IsUsable, accreditation.Error);
                var context = ProjectVariablesExpressionAdapter.From(accreditation.Registry);
                var evaluation = RegistryEvaluation.Evaluate(context);
                var rootVariable = VariableId.Parse(Id(0));
                var root = SymbolId.ProjectVariable(rootVariable.Value);
                var affected = new[] { root }.Concat(evaluation.DependencyGraph.TransitiveDependents(root))
                    .Select(symbol => VariableId.Parse(symbol.Key)).ToArray();

                var entries = new List<ProjectVariableScanEntry>();
                for (var rack = 0; rack < rackCount; rack++)
                {
                    var rackId = RackId(rack);
                    var design = SelectivePalletDesignDocument.From(SimpleDesign(), rackId, "Perf " + rack);
                    design.SchemaVersion = SelectivePalletDesignDocument.PromotedSchemaVersion;
                    design.PropertyValues = new Dictionary<string, SelectivePropertyValueDocument>
                    {
                        [ProjectPropertyIds.SelectiveVerticalClearanceToken] =
                            SelectivePropertyValueDocument.FromExpression(
                                BoundExpression.Reference(SymbolId.ProjectVariable(Id(variableCount - 1)))),
                    };
                    for (var view = 0; view < viewsPerRack; view++)
                    {
                        entries.Add(ProjectVariableScanEntry.Selective(
                            "PERF-" + rack + "-" + view, rackId, design, directReferenceCount: 1));
                    }
                }

                return new PerformanceFixture(registry, context, evaluation, rootVariable, entries, affected);
            }

            private static string Id(int index) => $"00000000-0000-0000-0000-{index + 1:000000000000}";
            private static string RackId(int index) => $"10000000-0000-0000-0000-{index + 1:000000000000}";

            private static SelectivePalletDesign SimpleDesign()
            {
                var design = new SelectivePalletDesign
                {
                    PostId = "POSTE_A",
                    PostPeralte = 3,
                    VerticalClearance = 6,
                    PalletTolerance = 4,
                    PalletDepth = 48,
                };
                var bay = new SelectiveBayDesign();
                bay.Levels.Add(new SelectiveCell
                {
                    Pallet = new Tarima { Frente = 42, Alto = 50 },
                    PalletCount = 1,
                    BeamId = "BEAM_A",
                    BeamPeralte = 4,
                });
                design.Bays.Add(bay);
                return design;
            }
        }
    }
}
