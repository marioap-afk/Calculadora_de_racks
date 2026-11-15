#nullable enable
using System;
using RackCad.Application.Systems.PushBack.Pilot;

namespace RackCad.Plugin.Views
{
    // RED skeleton (INV-F4T4C-20): the executor, its primitives and its target exist and every body is inert.
    internal sealed class RackPilotUpdateCommandContext : IPushBackPilotUpdateHost
    {
        public bool TryRequest(long documentNativeId, IPushBackPilotHostRun run, out string failure)
        {
            failure = string.Empty;
            return false;
        }

        private sealed class Primitives : IPushBackPilotHostPrimitives
        {
            public void EnterCommandContext(Action callback, Action completed)
            {
            }

            public bool TryOpenTarget(long documentNativeId, out IPushBackPilotHostTarget? target, out string reason)
            {
                target = null;
                reason = string.Empty;
                return false;
            }
        }

        private sealed class Target : IPushBackPilotHostTarget
        {
            public IDisposable Lock() => null!;

            public IPushBackPilotUpdatePort CreateWriter() => null!;

            public void Retire(IPushBackPilotUpdatePort writer)
            {
            }
        }
    }
}
