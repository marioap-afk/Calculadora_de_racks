using System;
using System.Windows.Threading;

namespace RackCad.UI.Editor
{
    /// <summary>
    /// The production <see cref="IRecomputeScheduler"/>: defers the flush to the WPF <see cref="Dispatcher"/> at
    /// <see cref="DispatcherPriority.Background"/>, exactly as <see cref="RackFrameConfiguratorWindow"/> does today, so a
    /// burst of change notifications redraws once when the UI is idle (initiative I-15). WPF-only, so it lives outside
    /// the pure <see cref="RecomputeDebouncer"/> that tests exercise with a manual scheduler.
    /// </summary>
    public sealed class DispatcherRecomputeScheduler : IRecomputeScheduler
    {
        private readonly Dispatcher dispatcher;

        /// <summary>Uses <paramref name="dispatcher"/>, defaulting to the current thread's dispatcher (the UI thread).</summary>
        public DispatcherRecomputeScheduler(Dispatcher dispatcher = null)
        {
            this.dispatcher = dispatcher ?? Dispatcher.CurrentDispatcher;
        }

        public void Schedule(Action flush)
        {
            if (flush == null)
            {
                throw new ArgumentNullException(nameof(flush));
            }

            dispatcher.BeginInvoke(flush, DispatcherPriority.Background);
        }
    }
}
