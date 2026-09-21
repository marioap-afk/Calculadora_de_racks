using System.Collections.Generic;
using RackCad.Application.Systems.Shared;
using RackCad.Application.Views.Policy;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    public sealed class RackViewExposureTests
    {
        public static IEnumerable<object[]> ProductSystems()
        {
            yield return new object[] { RackSystemKind.SelectiveRack, RackViewAddress.Fondo(0) };
            yield return new object[] { RackSystemKind.PalletFlow, RackViewAddress.FlowEnd(RackFlowEnd.Exit) };
            yield return new object[] { RackSystemKind.PushBack, RackViewAddress.PushBackCut(RackPushBackEnd.EntradaSalida, RackPushBackSide.A) };
            yield return new object[] { RackSystemKind.Cantilever, RackViewAddress.Station(0) };
            yield return new object[] { RackSystemKind.Selective, RackViewAddress.Whole(DimensionViewKind.Lateral) };
        }

        [Theory]
        [MemberData(nameof(ProductSystems))]
        public void SupportedViewsAreExposedInEveryProductOperation(
            RackSystemKind systemKind,
            RackViewAddress address)
        {
            foreach (var operation in new[]
            {
                RackViewProductOperation.CreateFirst,
                RackViewProductOperation.InsertSibling,
                RackViewProductOperation.Batch,
                RackViewProductOperation.GroupProjection
            })
            {
                Assert.True(RackViewExposure.IsExposed(systemKind, address, operation));
            }
        }

        [Fact]
        public void FlowBedIsExplicitlyExposedOnlyForItsSingleFirstView()
        {
            var address = RackViewAddress.Whole(DimensionViewKind.Lateral);

            Assert.True(RackViewExposure.IsExposed(
                RackSystemKind.Cama, address, RackViewProductOperation.CreateFirst));
            Assert.False(RackViewExposure.IsExposed(
                RackSystemKind.Cama, address, RackViewProductOperation.InsertSibling));
            Assert.False(RackViewExposure.IsExposed(
                RackSystemKind.Cama, address, RackViewProductOperation.Batch));
            Assert.False(RackViewExposure.IsExposed(
                RackSystemKind.Cama, address, RackViewProductOperation.GroupProjection));
        }

        [Fact]
        public void AGloballyValidAddressIsNotAutomaticallyExposedByAnotherSystem()
        {
            Assert.False(RackViewExposure.IsExposed(
                RackSystemKind.Cantilever,
                RackViewAddress.Post(0),
                RackViewProductOperation.CreateFirst));
        }
    }
}
