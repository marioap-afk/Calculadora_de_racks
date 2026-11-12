#nullable enable
using System;

namespace RackCad.Application.Workspace
{
    /// <summary>
    /// Pure scheduler of the idle pass of the panel (I-64 F4-T2C, IC-04). The callback of the idle notification of the host only
    /// calls <see cref="OnIdleTick"/>, which schedules at most ONE continuation through the post that the host adapter assigned;
    /// the continuation runs later, when the stack of that callback has already returned, and is where the drain (and with it every
    /// read of the drawing) happens.
    ///
    /// Rules it enforces:
    /// - <see cref="OnIdleTick"/> never runs the continuation and never reads: it schedules only when nothing is scheduled yet, a post
    ///   exists and the eligibility check in memory says a drain could run now (so there is no loop of posts while it is not eligible);
    /// - the scheduled run un-marks itself, asks the eligibility check AGAIN and runs the continuation only if it still holds; it never
    ///   schedules itself;
    /// - every fault of the check, of the post, of the continuation and of the fault logger is contained and leaves nothing scheduled.
    /// It holds no host type, starts no work of its own and keeps nothing but in-memory counters for tests and diagnostics.
    /// </summary>
    public sealed class WorkspaceIdleScheduler
    {
        private readonly Func<bool> _canRunNow;
        private readonly Action _continuation;
        private readonly Action<string, Exception>? _logFault;
        private Action<Action>? _post;
        private bool _scheduled;
        private int _continuationsRun;
        private int _continuationsSkipped;

        public WorkspaceIdleScheduler(Func<bool> canRunNow, Action continuation, Action<string, Exception>? logFault = null)
        {
            _canRunNow = canRunNow ?? throw new ArgumentNullException(nameof(canRunNow));
            _continuation = continuation ?? throw new ArgumentNullException(nameof(continuation));
            _logFault = logFault;
        }

        /// <summary>Whether a continuation is scheduled and has not run yet.</summary>
        public bool IsScheduled => _scheduled;

        /// <summary>How many scheduled runs executed the continuation. A counter in memory; it is not logged.</summary>
        public int ContinuationsRun => _continuationsRun;

        /// <summary>How many scheduled runs found the check false (or failing) and did nothing. A counter in memory; it is not logged.</summary>
        public int ContinuationsSkipped => _continuationsSkipped;

        /// <summary>
        /// Assigns the post, the only way this scheduler hands a run to the host. By assignment, never by constructor; a second call
        /// replaces the post. Without one the scheduler never schedules and never runs anything.
        /// </summary>
        public void SetPost(Action<Action> post)
        {
            _post = post ?? throw new ArgumentNullException(nameof(post));
        }

        /// <summary>
        /// Called inside the idle callback. In this order it does nothing when a continuation is already scheduled, when there is no
        /// post, or when the check is false; otherwise it marks the continuation as scheduled and calls the post ONCE. It never runs the
        /// continuation by itself and nothing it does can reach the caller.
        /// </summary>
        public void OnIdleTick()
        {
            if (_scheduled) return;
            var post = _post;
            if (post == null) return;
            try
            {
                if (!_canRunNow()) return;
                _scheduled = true;
                post(Run);
            }
            catch (Exception ex)
            {
                _scheduled = false;
                Report("WorkspaceIdleScheduler.OnIdleTick", ex);
            }
        }

        // Handed to the post and only there. It un-marks, asks the check again and runs the continuation only if it still holds.
        private void Run()
        {
            _scheduled = false;

            var eligible = false;
            try
            {
                eligible = _canRunNow();
            }
            catch (Exception ex)
            {
                Report("WorkspaceIdleScheduler.Run.Check", ex);
            }

            if (!eligible)
            {
                _continuationsSkipped++;
                return;
            }

            try
            {
                _continuationsRun++;
                _continuation();
            }
            catch (Exception ex)
            {
                Report("WorkspaceIdleScheduler.Run", ex);
            }
        }

        // A failing logger is never a second fault.
        private void Report(string context, Exception exception)
        {
            try
            {
                _logFault?.Invoke(context, exception);
            }
            catch
            {
                // Reporting must never become a second fault.
            }
        }
    }
}
