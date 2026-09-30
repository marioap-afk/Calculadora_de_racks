using System.Collections.Generic;
using RackCad.Application.Systems.Shared;
using RackCad.Application.Views.Preparation;
using RackCad.Domain.Systems.Shared;
using Xunit;
using static RackCad.Tests.RackProductPrepareTestSupport;

namespace RackCad.Tests
{
    public sealed class RackViewPreparationSystemTests
    {
        public static IEnumerable<object[]> Systems()
        {
            yield return Row(RackSystemKind.SelectiveRack, RackViewAddress.Fondo(0),
                new SelectiveViewAvailabilityFacts(1, new[] { 0 }));
            yield return Row(RackSystemKind.PalletFlow, RackViewAddress.Post(0),
                new DynamicViewAvailabilityFacts(new[] { 0 }));
            yield return Row(RackSystemKind.PushBack, RackViewAddress.Post(0),
                new PushBackViewAvailabilityFacts(new[] { 0 }, false));
            yield return Row(RackSystemKind.Cantilever, RackViewAddress.Station(0),
                new CantileverViewAvailabilityFacts(1));
            yield return Row(RackSystemKind.Selective, RackViewAddress.Whole(DimensionViewKind.Lateral),
                new CabeceraViewAvailabilityFacts());
            yield return Row(RackSystemKind.Cama, RackViewAddress.Whole(DimensionViewKind.Lateral),
                new CamaViewAvailabilityFacts());
        }

        [Theory]
        [MemberData(nameof(Systems))]
        public void EveryProductSystemCanPrepareAnExposedNewView(
            RackSystemKind systemKind,
            RackViewAddress address,
            RackViewAvailabilityFacts facts)
        {
            var resolve = new CountingResolve<Authored, Resolved>(Kind(systemKind),
                _ => RackResolveResult<Resolved>.Success(Kind(systemKind), new Resolved()));

            var result = Preparer(systemKind, resolve).Prepare(Accepted(NewIntent(
                systemKind, address, facts, new CountingIdFactory())));

            Assert.True(result.IsSuccess);
            Assert.Equal(Kind(systemKind), result.Product.Prepared.Kind);
            Assert.Equal(address, result.Product.Prepared.Address);
        }

        private static object[] Row(
            RackSystemKind systemKind,
            RackViewAddress address,
            RackViewAvailabilityFacts facts) => new object[] { systemKind, address, facts };
    }
}
