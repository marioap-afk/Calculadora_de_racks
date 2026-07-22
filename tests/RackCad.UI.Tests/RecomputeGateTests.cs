using RackCad.UI.Editor;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>The synchronous, scope-based recompute coalescer (the selective window's model, centralized in I-15).</summary>
    public sealed class RecomputeGateTests
    {
        [Fact]
        public void Request_OutsideAScope_RunsImmediately()
        {
            var runs = 0;
            var gate = new RecomputeGate(() => runs++);

            gate.Request();
            gate.Request();

            Assert.Equal(2, runs);
            Assert.False(gate.IsDeferred);
        }

        [Fact]
        public void Request_InsideAScope_CoalescesToOneRunOnClose()
        {
            var runs = 0;
            var gate = new RecomputeGate(() => runs++);

            using (gate.Defer())
            {
                Assert.True(gate.IsDeferred);
                gate.Request();
                gate.Request();
                gate.Request();
                Assert.Equal(0, runs);   // nothing ran yet — all latched
                Assert.True(gate.IsPending);
            }

            Assert.Equal(1, runs);        // exactly one pass on close
            Assert.False(gate.IsDeferred);
            Assert.False(gate.IsPending);
        }

        [Fact]
        public void Scope_WithNoRequest_RunsNothing()
        {
            var runs = 0;
            var gate = new RecomputeGate(() => runs++);

            using (gate.Defer())
            {
            }

            Assert.Equal(0, runs);
        }

        [Fact]
        public void NestedScopes_FlushOnlyOnOutermostClose()
        {
            var runs = 0;
            var gate = new RecomputeGate(() => runs++);

            using (gate.Defer())
            {
                using (gate.Defer())
                {
                    gate.Request();
                    gate.Request();
                }

                Assert.Equal(0, runs); // inner close is not the outermost — still deferred
                Assert.True(gate.IsDeferred);
            }

            Assert.Equal(1, runs);
        }

        [Fact]
        public void Scope_ToleratesDoubleDispose()
        {
            var runs = 0;
            var gate = new RecomputeGate(() => runs++);

            var scope = gate.Defer();
            gate.Request();
            scope.Dispose();
            scope.Dispose(); // must not run the recompute a second time nor underflow the depth

            Assert.Equal(1, runs);
            Assert.False(gate.IsDeferred);
        }
    }
}
