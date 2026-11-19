using System.Collections.Generic;
using RackCad.Domain.Systems.PushBack;

namespace RackCad.Application.Systems.PushBack
{
    /// <summary>
    /// The criterion that blocks the OUTPUT of the Push Back editor. SKELETON (I-64 F4B PB-T1b, the RED): no diagnostics,
    /// never blocked, and the status is always the fallback.
    /// </summary>
    public static class PushBackEditorOutputGate
    {
        public static IReadOnlyList<PushBackCompositeDiagnostic> Diagnostics(
            PushBackCompositeEditorState composite, PushBackSystem system)
            => new List<PushBackCompositeDiagnostic>();

        public static bool IsBlocked(PushBackCompositeEditorState composite, PushBackSystem system) => false;

        public static string StatusOr(PushBackCompositeEditorState composite, PushBackSystem system, string fallback) => fallback;
    }
}
