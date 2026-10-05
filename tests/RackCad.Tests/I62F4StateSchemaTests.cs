#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using Xunit;
using static RackCad.Tests.StateV2Samples;

namespace RackCad.Tests
{
    /// <summary>
    /// I-62 F4 (slice F4-H), C-18: the materialized schema of <c>rackcad-automation-state/v2</c>
    /// (<c>agent-execution/schemas/automation-state.v2.schema.json</c>) is exactly the emission of the validator's declared tree
    /// (<see cref="StateV2Shape.ToJsonSchema"/>), so the contract and the validator cannot drift; every valid synthetic state of the F4 guards validates
    /// against it, and each shape mutation is rejected by both the schema and the shape check.
    /// </summary>
    public class I62F4StateSchemaTests
    {
        private const string SchemaPath = "docs/automation/agent-execution/schemas/automation-state.v2.schema.json";

        private static JsonNode Committed() => JsonNode.Parse(I62Repo.ReadText(SchemaPath))!;

        private static IEnumerable<StatePoint> ValidPoints() =>
            CustodyHistory().Concat(OrchestrationSamples.F8()).Concat(ReviewerSamples.Loop()).Append(WindowRebaseClose());

        [Fact]
        public void I62_C18_TheMaterializedSchemaIsTheEmissionOfTheValidatorsDeclaredTree()
        {
            var expected = StateV2Shape.ToJsonSchema();
            Assert.True(JsonNode.DeepEquals(expected, Committed()), "regenerate " + SchemaPath + " from StateV2Shape.ToJsonSchema()");
            Assert.Empty(MiniJsonSchema.Validate(Committed(), JsonNode.Parse("{}")!).Where(p => p.Contains("unsupported keyword", StringComparison.Ordinal)));
        }

        [Fact]
        public void I62_C18_EveryValidSyntheticStateValidatesAgainstTheSchema()
        {
            var schema = Committed();
            foreach (var p in ValidPoints())
            {
                Assert.Empty(StateV2Shape.Check(p.State));
                var problems = MiniJsonSchema.Validate(schema, YamlSubset.ToJson(p.State));
                Assert.True(problems.Count == 0, Y.N(p.State, "custody.record_version") + ": " + string.Join("; ", problems.Take(5)));
            }
        }

        [Theory]
        [InlineData("unknown key")]
        [InlineData("missing key")]
        [InlineData("enum")]
        [InlineData("sha")]
        [InlineData("null")]
        [InlineData("stateref")]
        [InlineData("integer")]
        public void I62_C18_AShapeMutationIsRejectedByTheSchemaAndByTheShapeCheck(string mutation)
        {
            var s = Clone(Point(6).State);
            switch (mutation)
            {
                case "unknown key":
                    ((YamlMap)s["custody"]!)["extra"] = "x";
                    break;
                case "missing key":
                    ((YamlMap)s["custody"]!).Remove("window");
                    break;
                case "enum":
                    ((YamlMap)s["custody"]!)["point"] = "Q9";
                    break;
                case "sha":
                    ((YamlMap)s["automation_state"]!)["last_evidence_commit"] = "ABC";
                    break;
                case "null":
                    ((YamlMap)s["protocol"]!)["effective_sha"] = null;
                    break;
                case "stateref":
                    ((YamlMap)Y.M(s, "custody.principal")!)["binding"] = M(("path", "x"));
                    break;
                case "integer":
                    ((YamlMap)s["custody"]!)["record_version"] = 0L;
                    break;
            }

            Assert.NotEmpty(StateV2Shape.Check(s));
            Assert.NotEmpty(MiniJsonSchema.Validate(Committed(), YamlSubset.ToJson(s)));
        }

        [Fact]
        public void I62_C18_TheDeclaredOpenPointsStayOpenAndNothingElse()
        {
            var s = Clone(Point(6).State);
            ((YamlMap)s["execution_context"]!)["anything"] = M(("nested", 1L));
            ((YamlMap)Y.M(s, "orchestration.next_action.budget_remaining")!)["review_rounds"] = 2L;
            Assert.Empty(MiniJsonSchema.Validate(Committed(), YamlSubset.ToJson(s)));
            s.Remove("execution_context");
            Assert.Empty(MiniJsonSchema.Validate(Committed(), YamlSubset.ToJson(s)));
            ((YamlMap)Y.M(s, "orchestration.next_action")!)["extra"] = "x";
            Assert.NotEmpty(MiniJsonSchema.Validate(Committed(), YamlSubset.ToJson(s)));
        }
    }
}
