using System;

namespace RackCad.UI.Editor
{
    /// <summary>
    /// Synchronous, scope-based coalescing of an editor's recompute, centralizing the pattern
    /// <see cref="RackSelectiveWindow"/> inlines today (its <c>DeferRecompute</c>/<c>recomputeDeferDepth</c>/
    /// <c>recomputePending</c>, initiative I-15). Every <see cref="Request"/> issued while a <see cref="Defer"/> scope is
    /// open collapses into AT MOST ONE run when the OUTERMOST scope closes — still synchronous, inside the same gesture,
    /// so the model/status is fresh for any follow-up reader. Outside any scope, <see cref="Request"/> runs immediately.
    /// This collapses the double pipeline of composite gestures (e.g. a matrix click whose focus move first commits a
    /// pending value via LostFocus). Pure: no WPF, no timers — testable synchronously.
    /// </summary>
    public sealed class RecomputeGate
    {
        private readonly Action recompute;
        private int deferDepth;
        private bool pending;

        public RecomputeGate(Action recompute)
        {
            this.recompute = recompute ?? throw new ArgumentNullException(nameof(recompute));
        }

        /// <summary>True while at least one <see cref="Defer"/> scope is open (a <see cref="Request"/> only latches).</summary>
        public bool IsDeferred => deferDepth > 0;

        /// <summary>True when a recompute was requested inside a scope and has not run yet.</summary>
        public bool IsPending => pending;

        /// <summary>
        /// Requests a recompute: while a <see cref="Defer"/> scope is open it only latches (the enclosing scope runs one
        /// pass on close); otherwise it runs the recompute now.
        /// </summary>
        public void Request()
        {
            if (deferDepth > 0)
            {
                pending = true; // latched: the enclosing Defer scope runs one pass on close
                return;
            }

            recompute();
        }

        /// <summary>
        /// Opens a coalescing scope. ALWAYS dispose via <c>using</c>: a leaked depth would freeze the recompute forever.
        /// Nested scopes are counted; only the outermost close flushes a pending request (once). Double-dispose is
        /// tolerated, matching the selective window's <c>RecomputeDeferral</c>.
        /// </summary>
        public IDisposable Defer() => new Scope(this);

        private sealed class Scope : IDisposable
        {
            private RecomputeGate owner;

            public Scope(RecomputeGate owner)
            {
                this.owner = owner;
                owner.deferDepth++;
            }

            public void Dispose()
            {
                var gate = owner;
                if (gate == null) return; // tolerate double-dispose
                owner = null;
                gate.deferDepth--;
                if (gate.deferDepth > 0 || !gate.pending) return;
                gate.pending = false;
                gate.recompute();
            }
        }
    }
}
