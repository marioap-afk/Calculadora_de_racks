#nullable enable
using System;

namespace RackCad.Application.Systems.PushBack.Pilot
{
    /// <summary>
    /// I-64 F4-T4C1-HOST (D-T4C-04) — the executor of the update of the pilot: the one thing the host core asks of the application that
    /// hosts it. It has no state. It asks the host to run the synchronous steps of <paramref name="run"/> inside a command context, for
    /// the document whose native identity is <paramref name="documentNativeId"/>, and answers at once whether the request could be made.
    /// A false answer carries its cause in <paramref name="failure"/> and means that no step of the run will ever be called.
    /// </summary>
    public interface IPushBackPilotUpdateHost
    {
        bool TryRequest(long documentNativeId, IPushBackPilotHostRun run, out string failure);
    }

    /// <summary>Why a run of the host did not reach the update of the pilot: the request was refused, or the host failed.</summary>
    public enum PushBackPilotHostFailure
    {
        Rejected,
        Failed,
    }

    /// <summary>
    /// The synchronous delegate of one run of the update, one instance per request. The host core implements it. The five operations are
    /// TOTAL: none of them ever throws.
    /// </summary>
    public interface IPushBackPilotHostRun
    {
        /// <summary>Checks, in memory, that the run may go on and marks it as started. False carries the reason.</summary>
        bool Admit(out string reason);

        /// <summary>Opens the scope of the own mutation of the run. It is called under the lock, before the update.</summary>
        void Begin();

        /// <summary>Runs the update once, through the port that is handed in.</summary>
        void Execute(IPushBackPilotUpdatePort port);

        /// <summary>The run ended without reaching the update. It is called at most once, and never after <see cref="Execute"/>.</summary>
        void NotRun(PushBackPilotHostFailure kind, string cause);

        /// <summary>The command context reported that it completed.</summary>
        void Completed();
    }

    /// <summary>
    /// The opaque target of one run: the document that was admitted, seen from the application only through three operations. The host
    /// application implements it, and no type of the host crosses it.
    /// </summary>
    public interface IPushBackPilotHostTarget
    {
        /// <summary>Locks the document. The lock is only released by disposing what is returned.</summary>
        IDisposable Lock();

        /// <summary>Creates the writer of this run, for this document only.</summary>
        IPushBackPilotUpdatePort CreateWriter();

        /// <summary>Retires the writer: it cannot be used again.</summary>
        void Retire(IPushBackPilotUpdatePort writer);
    }

    /// <summary>The two primitives of the host that the runner needs, and nothing else.</summary>
    public interface IPushBackPilotHostPrimitives
    {
        /// <summary>
        /// Asks the host to call <paramref name="callback"/> inside a command context, and <paramref name="completed"/> once the context
        /// reports that it completed. The completion carries no failure.
        /// </summary>
        void EnterCommandContext(Action callback, Action completed);

        /// <summary>Resolves the document by its native identity: it must be open and still the active one.</summary>
        bool TryOpenTarget(long documentNativeId, out IPushBackPilotHostTarget? target, out string reason);
    }

    /// <summary>The texts that the host adds to the update of the pilot. A prefix ends with a colon and a space.</summary>
    public static class PushBackPilotHostTexts
    {
        public const string UpdateInFlightReason = "Hay una actualización en curso: espera a que termine.";

        public const string HostNotRunReason = "La actualización no llegó a ejecutarse; no se escribió nada en el dibujo.";

        public const string HostRequestFailedPrefix = "No se pudo pedir el contexto de comando: ";

        public const string LockFailedPrefix = "No se pudo bloquear el documento: ";

        public const string HostUnavailableReason = "La actualización no está disponible en esta sesión.";

        public const string UpdateNotClosedReason = "El borrador no aceptó el cierre de la actualización.";
    }

    /// <summary>
    /// The runner of ONE request (D-T4C-04): it exists for as long as the request does, it is created only by <see cref="Request"/> and
    /// no long-lived type keeps it. It fixes the order of the steps of a run and the terminal latch: from a true <c>Admit</c> on, exactly
    /// one terminal call is made (<c>Execute</c>, or <c>NotRun</c> when <c>Execute</c> was never reached), and the completion is passed on
    /// once. Its two entry points are total: no exception leaves them towards the host.
    /// </summary>
    public sealed class PushBackPilotHostRunner
    {
        private enum Phase
        {
            Pending,
            Started,
            Ended,
        }

        private readonly long _documentNativeId;
        private readonly IPushBackPilotHostRun _run;
        private readonly IPushBackPilotHostPrimitives _primitives;
        private readonly Action<string, Exception>? _log;
        private Phase _phase = Phase.Pending;
        private bool _completionPassed;

        private PushBackPilotHostRunner(
            long documentNativeId, IPushBackPilotHostRun run, IPushBackPilotHostPrimitives primitives, Action<string, Exception>? log)
        {
            _documentNativeId = documentNativeId;
            _run = run;
            _primitives = primitives;
            _log = log;
        }

        /// <summary>
        /// Asks the host for the command context, once. When the request itself fails before the callback began, the runner is ended, no
        /// terminal call is made (the failure travels only through <paramref name="failure"/>) and the answer is false. When it fails after
        /// a synchronous callback began, the failure is only logged and the answer is true.
        /// </summary>
        public static bool Request(
            long documentNativeId,
            IPushBackPilotHostRun run,
            IPushBackPilotHostPrimitives primitives,
            Action<string, Exception>? log,
            out string failure)
        {
            if (run == null) throw new ArgumentNullException(nameof(run));
            if (primitives == null) throw new ArgumentNullException(nameof(primitives));

            failure = string.Empty;
            var runner = new PushBackPilotHostRunner(documentNativeId, run, primitives, log);
            try
            {
                primitives.EnterCommandContext(runner.OnCallback, runner.OnCompleted);
            }
            catch (Exception ex)
            {
                runner.Log("PushBackPilotHostRunner.Request", ex);
                if (runner._phase == Phase.Pending)
                {
                    runner._phase = Phase.Ended;
                    failure = PushBackPilotHostTexts.HostRequestFailedPrefix + ex.Message;
                    return false;
                }
            }

            return true;
        }

        private void OnCallback()
        {
            try
            {
                if (_phase != Phase.Pending)
                {
                    Log("PushBackPilotHostRunner.OnCallback", new InvalidOperationException("The callback ran again; it does nothing."));
                    return;
                }

                _phase = Phase.Started;
                RunSteps();
            }
            catch (Exception ex)
            {
                Log("PushBackPilotHostRunner.OnCallback", ex);
            }
        }

        private void OnCompleted()
        {
            try
            {
                if (_completionPassed)
                {
                    Log("PushBackPilotHostRunner.OnCompleted", new InvalidOperationException("The completion arrived again; it does nothing."));
                    return;
                }

                _completionPassed = true;
                if (_phase == Phase.Pending) _phase = Phase.Ended;
                _run.Completed();
            }
            catch (Exception ex)
            {
                Log("PushBackPilotHostRunner.OnCompleted", ex);
            }
        }

        // The steps of D-T4C-04 (a), in their exact order. 'terminal' is the latch: once Execute or NotRun was reached no other
        // terminal call is made, whatever happens afterwards.
        private void RunSteps()
        {
            var terminal = false;
            try
            {
                // 1. The document, by its admitted identity.
                IPushBackPilotHostTarget? target;
                if (!_primitives.TryOpenTarget(_documentNativeId, out target, out _) || target == null)
                {
                    terminal = true;
                    _run.NotRun(PushBackPilotHostFailure.Rejected, PushBackPilotUpdateRun.DocumentMismatchReason);
                    return;
                }

                // 2. The session, in memory.
                if (!_run.Admit(out var reason))
                {
                    terminal = true;
                    _run.NotRun(PushBackPilotHostFailure.Rejected, reason);
                    return;
                }

                // 3 to 7, with the lock of that document.
                RunLocked(target, ref terminal);
            }
            catch (Exception ex)
            {
                Log("PushBackPilotHostRunner.RunSteps", ex);
            }
            finally
            {
                // 8. Exactly one terminal call from here on, also when something above failed in a way that none of the steps covered.
                if (!terminal) TryNotRun(PushBackPilotHostFailure.Failed, PushBackPilotHostTexts.HostNotRunReason);
            }
        }

        private void RunLocked(IPushBackPilotHostTarget target, ref bool terminal)
        {
            IDisposable? held = null;
            IPushBackPilotUpdatePort? writer = null;
            var locking = true;
            try
            {
                held = target.Lock();

                // A host that answers no lock does not honor the contract: the run does not go on without one (M-06).
                if (held == null) throw new InvalidOperationException("The target did not answer a lock.");
                locking = false;
                _run.Begin();
                writer = target.CreateWriter();

                // Execute is total and it is the terminal call: the latch is set before it, so nothing after it can become a second one.
                terminal = true;
                _run.Execute(writer);
            }
            catch (Exception ex)
            {
                Log("PushBackPilotHostRunner.RunLocked", ex);
                if (!terminal)
                {
                    terminal = true;
                    // The sentence of the host is the whole text, never a prefix (M-04): the exception went to the log above.
                    var cause = locking
                        ? PushBackPilotHostTexts.LockFailedPrefix + ex.Message
                        : PushBackPilotHostTexts.HostNotRunReason;
                    TryNotRun(PushBackPilotHostFailure.Failed, cause);
                }
            }
            finally
            {
                // 7. The writer is retired only when it was created, and the lock is released, each in a try of its own.
                if (writer != null)
                {
                    try
                    {
                        target.Retire(writer);
                    }
                    catch (Exception ex)
                    {
                        Log("PushBackPilotHostRunner.Retire", ex);
                    }
                }

                if (held != null)
                {
                    try
                    {
                        held.Dispose();
                    }
                    catch (Exception ex)
                    {
                        Log("PushBackPilotHostRunner.Unlock", ex);
                    }
                }
            }
        }

        private void TryNotRun(PushBackPilotHostFailure kind, string cause)
        {
            try
            {
                _run.NotRun(kind, cause);
            }
            catch (Exception ex)
            {
                Log("PushBackPilotHostRunner.NotRun", ex);
            }
        }

        private void Log(string context, Exception exception)
        {
            try
            {
                _log?.Invoke(context, exception);
            }
            catch
            {
                // Logging must never become a second failure.
            }
        }
    }
}
