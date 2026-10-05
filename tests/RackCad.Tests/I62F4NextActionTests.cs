#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using static RackCad.Tests.OrchestrationSamples;
using static RackCad.Tests.StateV2Samples;

namespace RackCad.Tests
{
    /// <summary>
    /// I-62 F4 (slice F4-G), C-38: <c>NextAction</c> derivable de forma única (Proposal V14 §20.4, B.8.8 I-S18, P-17) through
    /// <see cref="NextActionDerivation"/>. The positives are every point of the F.8 loop and of the REVIEWER loop; each negative contradicts one
    /// frozen determination, and switching off its clause silences it (mutation). States the Freeze leaves free (for instance PUBLISHED) stay free.
    /// </summary>
    public class I62F4NextActionTests
    {
        private static List<string> Ids(IEnumerable<StateViolation> v) => v.Select(x => x.Invariant + (x.Clause.Length > 0 ? "/" + x.Clause : string.Empty)).ToList();

        private static List<string> File(StatePoint p, params string[] disabled) => Ids(new StateV2Validator(disabled).ValidateFile(p));

        [Fact]
        public void I62_C38_EveryPointOfTheArchitectAndReviewerLoopsDerivesItsStoredNextAction()
        {
            foreach (var p in F8().Concat(ReviewerSamples.Loop()))
            {
                var (ambiguities, mismatches) = NextActionDerivation.Check(p);
                Assert.True(ambiguities.Count == 0 && mismatches.Count == 0, Rv(p) + ": " + string.Join("; ", ambiguities.Concat(mismatches)));
            }

            Assert.Equal(new[] { "pending review" }, NextActionDerivation.Derive(F8Step(2)).Select(d => d.Rule));
            Assert.Equal(new[] { "CORRECTING" }, NextActionDerivation.Derive(F8Step(7)).Select(d => d.Rule));
            Assert.Empty(NextActionDerivation.Derive(F8Step(11)));
            Assert.Equal("REVIEWER", NextActionDerivation.Derive(ReviewerSamples.Loop()[1]).Single().Role);
        }

        [Fact]
        public void I62_C38_CorrectingIsThePrincipalsCorrectAndRereviewOnTheLoopObject()
        {
            var s7 = F8Step(7);
            Orch(s7)["next_action"] = NextAction("PRINCIPAL_COORDINATOR", "VERIFY_CI", Obj(C1, X, B1));
            Assert.Contains("I-S18/NextAction", File(s7));
            Assert.DoesNotContain("I-S18/NextAction", File(s7, "NextAction"));

            var wrongTarget = F8Step(7);
            Orch(wrongTarget)["next_action"] = NextAction("PRINCIPAL_COORDINATOR", "CORRECT_AND_REREVIEW", Obj(C2, X, B2));
            Assert.Contains("I-S18/NextAction", File(wrongTarget));

            var architect = F8Step(7);
            Orch(architect)["next_action"] = NextAction("ARCHITECT", "REVIEW", Obj(C1, X, B1));
            Assert.Contains("I-S18/NextAction", File(architect));
        }

        [Fact]
        public void I62_C38_APendingReviewInvokesTheLoopsReviewerWithTheCurrentAuthorizationAndItsOutputContract()
        {
            var s2 = F8Step(2);
            Orch(s2)["next_action"] = NextAction("ARCHITECT", "REVIEW", Obj(C1, X, B1));
            Assert.Contains("I-S18/NextAction", File(s2));

            var target = F8Step(2);
            ((YamlMap)Orch(target)["next_action"]!)["target"] = Obj(C2, X, B2);
            Assert.Contains("I-S18/NextAction", File(target));

            var output = F8Step(2);
            ((YamlMap)Orch(output)["next_action"]!)["expected_output"] = "rackcad-reviewer-result/v1";
            Assert.Contains("I-S18/NextAction", File(output));

            var principal = F8Step(2);
            ((YamlMap)Orch(principal)["next_action"]!)["role"] = "PRINCIPAL_COORDINATOR";
            Assert.Contains("I-S18/NextAction", File(principal));
            Assert.DoesNotContain("I-S18/NextAction", File(principal, "NextAction"));

            var reviewer = ReviewerSamples.Loop()[1];
            ((YamlMap)Orch(reviewer)["next_action"]!)["expected_output"] = "rackcad-architect-review-result/v1";
            Assert.Contains("I-S18/NextAction", File(reviewer));
        }

        [Fact]
        public void I62_C38_TwoDeterminationsWithDifferentRolesAreAMaterialAmbiguity()
        {
            var s7 = F8Step(7);
            var escalation = (YamlMap)Orch(s7)["escalation"]!;
            escalation["state"] = "OWNER";
            escalation["reason"] = "decisión de alcance";
            escalation["required_decision"] = "aceptar o rechazar el alcance";
            Orch(s7)["next_action"] = NextAction("OWNER", "DECIDE", null);
            var ids = File(s7);
            Assert.Contains("I-S18/P-17", ids);
            Assert.DoesNotContain("I-S18/P-17", File(s7, "P-17"));
            Assert.Single(NextActionDerivation.Check(s7).Ambiguities);
        }

        [Fact]
        public void I62_C38_AStateTheFreezeLeavesFreeKeepsItsStoredNextAction()
        {
            var s11 = F8Step(11);
            Orch(s11)["next_action"] = NextAction("PRINCIPAL_COORDINATOR", "WAIT_CI", Obj(C2, X, B2));
            Assert.DoesNotContain("I-S18/NextAction", File(s11));
            Assert.DoesNotContain("I-S18/P-17", File(s11));
        }
    }
}
