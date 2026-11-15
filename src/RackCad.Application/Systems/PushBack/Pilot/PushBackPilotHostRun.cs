#nullable enable
using System;

namespace RackCad.Application.Systems.PushBack.Pilot
{
    // RED skeleton (INV-F4T4C-20): the contracts of the runner exist and every body is inert.
    public interface IPushBackPilotUpdateHost
    {
        bool TryRequest(long documentNativeId, IPushBackPilotHostRun run, out string failure);
    }

    public enum PushBackPilotHostFailure
    {
        Rejected,
        Failed,
    }

    public interface IPushBackPilotHostRun
    {
        bool Admit(out string reason);

        void Begin();

        void Execute(IPushBackPilotUpdatePort port);

        void NotRun(PushBackPilotHostFailure kind, string cause);

        void Completed();
    }

    public interface IPushBackPilotHostTarget
    {
        IDisposable Lock();

        IPushBackPilotUpdatePort CreateWriter();

        void Retire(IPushBackPilotUpdatePort writer);
    }

    public interface IPushBackPilotHostPrimitives
    {
        void EnterCommandContext(Action callback, Action completed);

        bool TryOpenTarget(long documentNativeId, out IPushBackPilotHostTarget? target, out string reason);
    }

    public static class PushBackPilotHostTexts
    {
        public const string UpdateInFlightReason = "";

        public const string HostNotRunReason = "";

        public const string HostRequestFailedPrefix = "";

        public const string LockFailedPrefix = "";

        public const string HostUnavailableReason = "";

        public const string UpdateNotClosedReason = "";
    }

    public sealed class PushBackPilotHostRunner
    {
        private PushBackPilotHostRunner()
        {
        }

        public static bool Request(
            long documentNativeId,
            IPushBackPilotHostRun run,
            IPushBackPilotHostPrimitives primitives,
            Action<string, Exception>? log,
            out string failure)
        {
            failure = string.Empty;
            return false;
        }
    }
}
