using RackCad.Application.Systems.Shared;
using RackCad.Application.Views.Policy;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    public sealed class ProjectionCodecPolicyTests
    {
        [Fact]
        public void DefaultDecisionFailsClosed()
        {
            var result = default(ProjectionPolicyDecision);

            Assert.Equal(ProjectionPolicyDecisionKind.Reject, result.Decision);
            Assert.Null(result.CanonicalAddress);
            Assert.Null(result.CanonicalSyntax);
        }

        [Fact]
        public void Canonical_IsAcceptedWithoutChangingItsSemanticAddress()
        {
            var decoded = RackViewCodec.Decode("cantilever", "lateral", 2);

            var result = ProjectionCodecPolicy.Evaluate(decoded);

            Assert.Equal(ProjectionPolicyDecisionKind.Accept, result.Decision);
            Assert.Equal(RackViewAddress.Station(2), result.CanonicalAddress);
            Assert.Equal("cantilever/lateral/2", Syntax(result.CanonicalSyntax));
            Assert.Equal(ProjectionPolicyReasonCode.None, result.ReasonCode);
        }

        [Fact]
        public void Canonicalizable_IsAcceptedWithCanonicalEncoding()
        {
            var decoded = RackViewCodec.Decode("SELECTIVE", "FRONTAL", 1);

            var result = ProjectionCodecPolicy.Evaluate(decoded);

            Assert.Equal(ProjectionPolicyDecisionKind.AcceptCanonicalized, result.Decision);
            Assert.Equal(RackViewAddress.Fondo(1), result.CanonicalAddress);
            Assert.Equal("selective/frontal/1", Syntax(result.CanonicalSyntax));
        }

        [Fact]
        public void CoercedLegacyForm_IsRejectedWithoutChangingTheCodec()
        {
            var decoded = RackViewCodec.Decode("selective", "frontal", -1);

            var result = ProjectionCodecPolicy.Evaluate(decoded);

            Assert.Equal(ProjectionPolicyDecisionKind.Reject, result.Decision);
            Assert.Equal(ProjectionPolicyReasonCode.CoercedLegacyFormNotAllowed, result.ReasonCode);
            Assert.Equal(ProjectionPolicyRemedyCategory.UpdateRack, result.RemedyCategory);
        }

        [Fact]
        public void InvalidSyntaxAndUnknownKindHaveDifferentReasons()
        {
            var invalidSyntax = ProjectionCodecPolicy.Evaluate(
                RackViewCodec.Decode("pushback", "future-view", 0));
            var unknownKind = ProjectionCodecPolicy.Evaluate(
                RackViewCodec.Decode("future-kind", "frontal", 0));

            Assert.Equal(ProjectionPolicyReasonCode.InvalidSyntax, invalidSyntax.ReasonCode);
            Assert.Equal(ProjectionPolicyReasonCode.UnknownKind, unknownKind.ReasonCode);
        }

        [Fact]
        public void NewViewIntentAlwaysProducesCanonicalSyntax()
        {
            var result = ProjectionCodecPolicy.ForNewView(
                RackSystemKind.PalletFlow,
                RackViewAddress.FlowEnd(RackFlowEnd.Entrance));

            Assert.Equal(ProjectionPolicyDecisionKind.Accept, result.Decision);
            Assert.Equal("dynamic/frontal/1", Syntax(result.CanonicalSyntax));
            Assert.Equal(
                RackViewSyntaxDisposition.Canonical,
                RackViewCodec.Decode(
                    result.CanonicalSyntax.Value.Kind,
                    result.CanonicalSyntax.Value.View,
                    result.CanonicalSyntax.Value.Section).Disposition);
        }

        private static string Syntax(RackViewSyntax? syntax)
        {
            Assert.True(syntax.HasValue);
            return syntax.Value.Kind + "/" + (syntax.Value.View ?? "null") + "/" + syntax.Value.Section;
        }
    }
}
