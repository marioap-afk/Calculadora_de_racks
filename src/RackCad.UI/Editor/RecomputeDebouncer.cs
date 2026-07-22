using System;

namespace RackCad.UI.Editor
{
    /// <summary>
    /// Asynchronous coalescing of an editor's preview redraw, centralizing the pattern
    /// <see cref="RackFrameConfiguratorWindow"/> inlines today (its <c>SchedulePreviewRedraw</c>/
    /// <c>previewRedrawQueued</c>, initiative I-15). A burst of change notifications collapses into ONE redraw when idle:
    /// the first <see cref="Schedule"/> queues a flush via the injected <see cref="IRecomputeScheduler"/> and marks the
    /// debouncer queued; further <see cref="Schedule"/> calls are no-ops until the flush runs, clears the flag and runs
    /// the redraw once. Pure coalescing logic: the scheduler owns the "later" (Dispatcher in production, manual in tests).
    /// </summary>
    public sealed class RecomputeDebouncer
    {
        private readonly Action redraw;
        private readonly IRecomputeScheduler scheduler;
        private bool queued;

        public RecomputeDebouncer(Action redraw, IRecomputeScheduler scheduler)
        {
            this.redraw = redraw ?? throw new ArgumentNullException(nameof(redraw));
            this.scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
        }

        /// <summary>True between a first <see cref="Schedule"/> and the flush that clears it.</summary>
        public bool IsQueued => queued;

        /// <summary>Queues a single redraw; a burst of calls before the flush collapses into one.</summary>
        public void Schedule()
        {
            if (queued)
            {
                return;
            }

            queued = true;
            scheduler.Schedule(Flush);
        }

        private void Flush()
        {
            queued = false;
            redraw();
        }
    }
}
