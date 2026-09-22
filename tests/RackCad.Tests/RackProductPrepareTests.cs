using System;
using System.Linq;
using RackCad.Application.Systems.Shared;
using RackCad.Application.Views.Policy;
using RackCad.Application.Views.Preparation;
using RackCad.Domain.Systems.Shared;
using Xunit;
using static RackCad.Tests.RackProductPrepareTestSupport;

namespace RackCad.Tests
{
    public sealed class RackProductPrepareTests
    {
        [Fact]
        public void AcceptedIntentCallsResolveAndSharedPrepareExactlyOnceAndPreservesTypedOutput()
        {
            var resolve = SuccessResolve();
            var builds = 0;
            var result = Preparer(RackSystemKind.SelectiveRack, resolve, () => builds++).Prepare(Accepted(NewIntent(
                RackSystemKind.SelectiveRack, RackViewAddress.Fondo(0),
                new SelectiveViewAvailabilityFacts(1, new[] { 0 }), new CountingIdFactory())));

            Assert.True(result.IsSuccess);
            Assert.Equal(1, resolve.Calls);
            Assert.Equal(1, builds);
            Assert.IsType<Payload>(result.Product.Prepared.Payload);
            Assert.Equal("Base Frontal/Fondo(0)", result.Product.Prepared.BaseName);
            Assert.Equal("library-key", Assert.Single(result.Product.Prepared.BlockRequirements).Key);
            Assert.Equal(RackViewAddress.Fondo(0), result.Product.Prepared.Address);
            Assert.Equal(RackProductSourceKind.NewRack, result.Product.SourceKind);
        }

        [Fact]
        public void ResolveFailureSkipsPrepareAndPreservesCause()
        {
            var builds = 0;
            var resolve = new CountingResolve<Authored, Resolved>("selective",
                _ => RackResolveResult<Resolved>.Unsupported("selective", "unsupported authored"));

            var result = Preparer(RackSystemKind.SelectiveRack, resolve, () => builds++).Prepare(Accepted(NewIntent(
                RackSystemKind.SelectiveRack, RackViewAddress.Fondo(0),
                new SelectiveViewAvailabilityFacts(1, Array.Empty<int>()), new CountingIdFactory())));

            Assert.Equal(RackProductPrepareFailure.ResolveFailed, result.Failure);
            Assert.Equal("UNSUPPORTED_KIND", result.CauseCode);
            Assert.Equal("unsupported authored", result.Diagnostic);
            Assert.Equal(1, resolve.Calls);
            Assert.Equal(0, builds);
        }

        [Fact]
        public void PrepareFailureIsStructuredAndDoesNotEraseUnderlyingCause()
        {
            var resolve = SuccessResolve();

            var result = Preparer(RackSystemKind.SelectiveRack, resolve, failBuild: true).Prepare(Accepted(NewIntent(
                RackSystemKind.SelectiveRack, RackViewAddress.Fondo(0),
                new SelectiveViewAvailabilityFacts(1, Array.Empty<int>()), new CountingIdFactory())));

            Assert.Equal(RackProductPrepareFailure.PrepareFailed, result.Failure);
            Assert.Equal("BUILDER_FAILED", result.CauseCode);
            Assert.Contains("prepare boom", result.Diagnostic);
        }

        [Fact]
        public void CanonicalProductEnvelopeUsesPreparedAddressAndKeepsBaseNameSeparate()
        {
            var result = Preparer(RackSystemKind.SelectiveRack, SuccessResolve()).Prepare(Accepted(NewIntent(
                RackSystemKind.SelectiveRack, RackViewAddress.Fondo(0),
                new SelectiveViewAvailabilityFacts(1, Array.Empty<int>()), new CountingIdFactory())));

            Assert.Equal("selective", result.Product.Envelope.Kind);
            Assert.Equal("frontal", result.Product.Envelope.View);
            Assert.Equal(0, result.Product.Envelope.Section);
            Assert.Equal("Rack A", result.Product.Envelope.Name);
            Assert.NotEqual(result.Product.Prepared.BaseName, result.Product.Envelope.Name);
            Assert.Equal("{\"design\":1}", result.Product.Envelope.Design);
        }

        [Fact]
        public void InvalidOperationFailsBeforeResolve()
        {
            var resolve = SuccessResolve();
            var intent = NewIntent(
                RackSystemKind.SelectiveRack, RackViewAddress.Fondo(0),
                new SelectiveViewAvailabilityFacts(1, Array.Empty<int>()), new CountingIdFactory(),
                (RackViewProductOperation)999);

            var result = RackProductIntentAcceptance.Accept(intent);

            Assert.Equal(RackProductPrepareFailure.UnsupportedIntent, result.Failure);
            Assert.Equal(0, resolve.Calls);
        }

        private static CountingResolve<Authored, Resolved> SuccessResolve()
            => new CountingResolve<Authored, Resolved>("selective",
                _ => RackResolveResult<Resolved>.Success("selective", new Resolved()));
    }
}
