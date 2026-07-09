using System.Collections.Generic;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>The parametric connection-point resolution (X and Y slopes) behind the post/beam mates.</summary>
    public class SelectivePostGeometryTests
    {
        [Fact]
        public void Resolve_AppliesBothSlopes_WhenTheirParamsArePresent()
        {
            // Like the post's TROQUEL_LARGUERO: X slides in FRONTAL, Y slides in PLANTA — same mechanism.
            var entry = new ConnectionLayoutEntry
            {
                LocalX = -0.75,
                LocalXPorParam = 0.5,
                ParamX = "PERALTE",
                LocalY = 1.0,
                LocalYPorParam = 0.25,
                ParamY = "PERALTE"
            };

            var point = SelectivePostGeometry.Resolve(entry, new Dictionary<string, double> { ["PERALTE"] = 3.0 });

            Assert.Equal(-0.75 + 0.5 * 3.0, point.X, 4);
            Assert.Equal(1.0 + 0.25 * 3.0, point.Y, 4);
        }

        [Fact]
        public void Resolve_FixedPoint_IgnoresSlopesWithoutParamNames()
        {
            var entry = new ConnectionLayoutEntry { LocalX = 2.0, LocalY = 3.0, LocalXPorParam = 9.0, LocalYPorParam = 9.0 };

            var point = SelectivePostGeometry.Resolve(entry, new Dictionary<string, double> { ["PERALTE"] = 5.0 });

            Assert.Equal(2.0, point.X, 4);
            Assert.Equal(3.0, point.Y, 4);
        }

        [Fact]
        public void Resolve_NullEntry_IsOrigin()
        {
            var point = SelectivePostGeometry.Resolve(null, null);

            Assert.Equal(0.0, point.X, 4);
            Assert.Equal(0.0, point.Y, 4);
        }
    }
}
