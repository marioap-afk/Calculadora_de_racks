using RackCad.Application.Systems.Shared;
using RackCad.Application.Views.Policy;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    public sealed class ProjectionAvailabilityPolicyTests
    {
        [Fact]
        public void CanonicalAvailableAndExposed_IsAccepted()
        {
            var result = Evaluate(
                "selective", "frontal", 0,
                new SelectiveViewAvailabilityFacts(2, new[] { 0, 2 }),
                RackViewProductOperation.CreateFirst);

            Assert.Equal(ProjectionPolicyDecisionKind.Accept, result.Decision);
            Assert.Equal(RackViewAddress.Fondo(0), result.CanonicalAddress);
        }

        [Fact]
        public void CanonicalizableAvailableAndExposed_IsAcceptedCanonicalized()
        {
            var result = Evaluate(
                "DYNAMIC", "LATERAL", 2,
                new DynamicViewAvailabilityFacts(new[] { 0, 2 }),
                RackViewProductOperation.InsertSibling);

            Assert.Equal(ProjectionPolicyDecisionKind.AcceptCanonicalized, result.Decision);
            Assert.Equal(RackViewAddress.Post(2), result.CanonicalAddress);
            Assert.Equal("dynamic", result.CanonicalSyntax.Value.Kind);
            Assert.Equal("lateral", result.CanonicalSyntax.Value.View);
        }

        [Fact]
        public void CoercedAndInvalidFailBeforeAvailability()
        {
            var coerced = Evaluate(
                "selective", "frontal", -1,
                new SelectiveViewAvailabilityFacts(1, new[] { 0 }),
                RackViewProductOperation.GroupProjection);
            var invalid = Evaluate(
                "pushback", "lateral", -1,
                new PushBackViewAvailabilityFacts(new[] { 0 }, isComposite: false),
                RackViewProductOperation.GroupProjection);

            Assert.Equal(ProjectionPolicyReasonCode.CoercedLegacyFormNotAllowed, coerced.ReasonCode);
            Assert.Equal(ProjectionPolicyReasonCode.InvalidSyntax, invalid.ReasonCode);
        }

        [Fact]
        public void CanonicalButPhysicallyAbsentVariant_IsRejected()
        {
            var result = Evaluate(
                "selective", "frontal", 99,
                new SelectiveViewAvailabilityFacts(2, new[] { 0 }),
                RackViewProductOperation.CreateFirst);

            Assert.Equal(ProjectionPolicyDecisionKind.Reject, result.Decision);
            Assert.Equal(ProjectionPolicyReasonCode.UnsupportedVariant, result.ReasonCode);
            Assert.Equal(ProjectionPolicyRemedyCategory.ChooseAnotherView, result.RemedyCategory);
        }

        [Fact]
        public void AvailableFlowBedSister_IsRejectedByExposureNotAvailability()
        {
            var result = Evaluate(
                "cama", null, -1,
                new CamaViewAvailabilityFacts(),
                RackViewProductOperation.InsertSibling);

            Assert.Equal(ProjectionPolicyDecisionKind.Reject, result.Decision);
            Assert.Equal(ProjectionPolicyReasonCode.NotExposedForOperation, result.ReasonCode);
        }

        [Fact]
        public void CantileverStationUsesTheTypedVariantAndPhysicalCount()
        {
            var available = Evaluate(
                "cantilever", "lateral", 1,
                new CantileverViewAvailabilityFacts(2),
                RackViewProductOperation.GroupProjection);
            var absent = Evaluate(
                "cantilever", "lateral", 2,
                new CantileverViewAvailabilityFacts(2),
                RackViewProductOperation.GroupProjection);

            Assert.Equal(ProjectionPolicyDecisionKind.Accept, available.Decision);
            Assert.Equal(RackViewVariantKind.Station, available.CanonicalAddress.Value.Variant.Kind);
            Assert.Equal(1, available.CanonicalAddress.Value.Variant.Index);
            Assert.Equal(ProjectionPolicyReasonCode.UnsupportedVariant, absent.ReasonCode);
        }

        [Fact]
        public void PushBackUsesPhysicalPostIdentityWithoutAnOrdinalRemap()
        {
            var facts = new PushBackViewAvailabilityFacts(new[] { 0, 3 }, isComposite: false);
            var physicalPost = Evaluate(
                "pushback", "lateral", 3, facts, RackViewProductOperation.Batch);
            var missingOrdinal = Evaluate(
                "pushback", "lateral", 1, facts, RackViewProductOperation.Batch);

            Assert.Equal(ProjectionPolicyDecisionKind.Accept, physicalPost.Decision);
            Assert.Equal(3, physicalPost.CanonicalAddress.Value.Variant.Index);
            Assert.Equal(ProjectionPolicyReasonCode.UnsupportedVariant, missingOrdinal.ReasonCode);
        }

        [Fact]
        public void WrongSystemFactsAreRejectedStructurally()
        {
            var result = Evaluate(
                "selective", "frontal", 0,
                new CamaViewAvailabilityFacts(),
                RackViewProductOperation.CreateFirst);

            Assert.Equal(ProjectionPolicyReasonCode.UnavailableForSystem, result.ReasonCode);
            Assert.Equal(ProjectionPolicyRemedyCategory.Unsupported, result.RemedyCategory);
        }

        [Fact]
        public void TypedNewViewIntentIsEncodedCanonicallyBeforeAvailabilityAndExposure()
        {
            var accepted = ProjectionAvailabilityPolicy.EvaluateNewView(
                RackSystemKind.Selective,
                RackViewAddress.Whole(DimensionViewKind.Planta),
                new CabeceraViewAvailabilityFacts(),
                RackViewProductOperation.CreateFirst);
            var unsupported = ProjectionAvailabilityPolicy.EvaluateNewView(
                RackSystemKind.Cantilever,
                RackViewAddress.Post(0),
                new CantileverViewAvailabilityFacts(1),
                RackViewProductOperation.CreateFirst);

            Assert.Equal(ProjectionPolicyDecisionKind.Accept, accepted.Decision);
            Assert.Equal("cabecera", accepted.CanonicalSyntax.Value.Kind);
            Assert.Equal("planta", accepted.CanonicalSyntax.Value.View);
            Assert.Equal(-1, accepted.CanonicalSyntax.Value.Section);
            Assert.Equal(ProjectionPolicyReasonCode.UnsupportedVariant, unsupported.ReasonCode);
        }

        private static ProjectionPolicyDecision Evaluate(
            string kind,
            string view,
            int section,
            RackViewAvailabilityFacts facts,
            RackViewProductOperation operation) =>
            ProjectionAvailabilityPolicy.Evaluate(RackViewCodec.Decode(kind, view, section), facts, operation);
    }
}
