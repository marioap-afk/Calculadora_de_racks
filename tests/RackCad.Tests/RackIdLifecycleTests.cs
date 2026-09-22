using RackCad.Application.Systems.Shared;
using RackCad.Application.Views.Policy;
using RackCad.Application.Views.Preparation;
using RackCad.Domain.Systems.Shared;
using Xunit;
using static RackCad.Tests.RackProductPrepareTestSupport;

namespace RackCad.Tests
{
    public sealed class RackIdLifecycleTests
    {
        [Fact]
        public void AcceptedNewRackMintsExactlyOneIdentity()
        {
            var ids = new CountingIdFactory();
            var resolve = SuccessResolve(RackSystemKind.SelectiveRack);

            var accepted = RackProductIntentAcceptance.Accept(NewIntent(
                RackSystemKind.SelectiveRack, RackViewAddress.Fondo(0),
                new SelectiveViewAvailabilityFacts(1, new[] { 0 }), ids));
            var result = Preparer(RackSystemKind.SelectiveRack, resolve).Prepare(accepted.Intent);

            Assert.True(accepted.IsAccepted);
            Assert.True(result.IsSuccess);
            Assert.Equal("rack-new", result.Product.RackId);
            Assert.Equal(1, ids.Calls);
        }

        [Fact]
        public void RejectedNewRackMintsNoIdentityAndDoesNotResolve()
        {
            var ids = new CountingIdFactory();
            var resolve = SuccessResolve(RackSystemKind.Cama);

            var accepted = RackProductIntentAcceptance.Accept(NewIntent(
                RackSystemKind.Cama, RackViewAddress.Whole(DimensionViewKind.Lateral),
                new CamaViewAvailabilityFacts(), ids, RackViewProductOperation.InsertSibling));

            Assert.False(accepted.IsAccepted);
            Assert.Equal(RackProductPrepareFailure.PolicyRejected, accepted.Failure);
            Assert.Equal(0, ids.Calls);
            Assert.Equal(0, resolve.Calls);
        }

        [Fact]
        public void MultipleViewsOfOneAcceptedCreationContextShareIdentity()
        {
            var ids = new CountingIdFactory();
            var context = new NewRackCreationContext<Authored>(
                new Authored(), "Rack A", "{}", ids);
            var resolve = SuccessResolve(RackSystemKind.SelectiveRack);
            var preparer = Preparer(RackSystemKind.SelectiveRack, resolve);

            var firstAccepted = RackProductIntentAcceptance.Accept(NewIntent(
                RackSystemKind.SelectiveRack, RackViewAddress.Fondo(0),
                new SelectiveViewAvailabilityFacts(2, new[] { 0 }), ids,
                RackViewProductOperation.CreateFirst, context));
            var secondAccepted = RackProductIntentAcceptance.Accept(NewIntent(
                RackSystemKind.SelectiveRack, RackViewAddress.Fondo(1),
                new SelectiveViewAvailabilityFacts(2, new[] { 0 }), ids,
                RackViewProductOperation.Batch, context));
            var first = preparer.Prepare(firstAccepted.Intent);
            var second = preparer.Prepare(secondAccepted.Intent);

            Assert.True(first.IsSuccess);
            Assert.True(second.IsSuccess);
            Assert.Equal(first.Product.RackId, second.Product.RackId);
            Assert.Equal(1, ids.Calls);
        }

        [Fact]
        public void RetryingTheSamePureIntentIsSemanticallyIdempotent()
        {
            var ids = new CountingIdFactory();
            var context = new NewRackCreationContext<Authored>(new Authored(), "Rack A", "{}", ids);
            var intent = NewIntent(
                RackSystemKind.SelectiveRack, RackViewAddress.Fondo(0),
                new SelectiveViewAvailabilityFacts(1, new[] { 0 }), ids,
                RackViewProductOperation.CreateFirst, context);
            var preparer = Preparer(RackSystemKind.SelectiveRack, SuccessResolve(RackSystemKind.SelectiveRack));

            var accepted = RackProductIntentAcceptance.Accept(intent);
            var first = preparer.Prepare(accepted.Intent);
            var retry = preparer.Prepare(accepted.Intent);

            Assert.True(first.IsSuccess);
            Assert.True(retry.IsSuccess);
            Assert.Equal(first.Product.RackId, retry.Product.RackId);
            Assert.Equal(first.Product.Prepared.Address, retry.Product.Prepared.Address);
            Assert.Equal(first.Product.Prepared.BaseName, retry.Product.Prepared.BaseName);
            Assert.Equal(first.Product.Envelope.Kind, retry.Product.Envelope.Kind);
            Assert.Equal(first.Product.Envelope.View, retry.Product.Envelope.View);
            Assert.Equal(first.Product.Envelope.Section, retry.Product.Envelope.Section);
            Assert.Equal(1, ids.Calls);
        }

        private static CountingResolve<Authored, Resolved> SuccessResolve(RackSystemKind systemKind)
            => new CountingResolve<Authored, Resolved>(Kind(systemKind),
                _ => RackResolveResult<Resolved>.Success(Kind(systemKind), new Resolved()));
    }
}
