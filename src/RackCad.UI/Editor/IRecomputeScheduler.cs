using System;

namespace RackCad.UI.Editor
{
    /// <summary>
    /// Schedules a deferred flush for <see cref="RecomputeDebouncer"/> (initiative I-15). Production wraps
    /// <see cref="System.Windows.Threading.Dispatcher.BeginInvoke(System.Delegate,System.Windows.Threading.DispatcherPriority,object[])"/>
    /// at background priority (see <see cref="DispatcherRecomputeScheduler"/>); tests inject a manual scheduler that runs
    /// the flush on demand, so the coalescing logic is verifiable without a WPF message loop.
    /// </summary>
    public interface IRecomputeScheduler
    {
        /// <summary>Runs <paramref name="flush"/> later, off the current call (coalescing a burst into one redraw).</summary>
        void Schedule(Action flush);
    }
}
