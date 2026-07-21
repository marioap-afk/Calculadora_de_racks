using System.Collections.Generic;
using Autodesk.AutoCAD.EditorInput;
using RackCad.Application.Persistence;

namespace RackCad.Plugin.KindHandlers
{
    /// <summary>
    /// The single seam that resolves an envelope <see cref="RackEmbedDocument.Kind"/> to its handler, or reports the
    /// historic visible error. Every consumer (RACKEDITAR, RACKBOMTOTAL, and the RACKDUPLICAR/RACKLAYOUT copy gate)
    /// goes through here so a kind with NO registered handler always surfaces
    /// "RackCad: tipo de rack no reconocido (&lt;kind&gt;)." and no operation continues silently with a partial
    /// result or an inconsistent identity. The four embedded kinds are always registered, so real data never hits
    /// the error path.
    /// </summary>
    internal static class KindHandlerDispatch
    {
        /// <summary>Resolve the handler for <paramref name="kind"/> (case-sensitive, mirroring the historic
        /// RACKEDITAR / RACKBOMTOTAL <c>switch</c>), or write the historic visible error and return false. Callers
        /// must not continue on false.</summary>
        public static bool TryResolve(Editor editor, string kind, out IRackKindHandler handler)
        {
            if (KindHandlerRegistry.Default.TryGet(kind, out handler))
            {
                return true;
            }

            editor.WriteMessage("\n" + KindDispatchMessages.NotRecognized(kind));
            return false;
        }

        /// <summary>Resolve the handler case-INSENSITIVELY (matching the copy restamp's OrdinalIgnoreCase), or write
        /// the historic visible error and return false. Used to gate an INDEPENDENT copy before it is placed, so an
        /// unrecognized kind never produces a copy with a possibly-inconsistent inner identity.</summary>
        public static bool TryResolveIgnoreCase(Editor editor, string kind, out IRackKindHandler handler)
        {
            if (KindHandlerRegistry.Default.TryGetIgnoreCase(kind, out handler))
            {
                return true;
            }

            editor.WriteMessage("\n" + KindDispatchMessages.NotRecognized(kind));
            return false;
        }

        /// <summary>Resolve handlers for EVERY kind up front (case-sensitive), returning them aligned to
        /// <paramref name="kinds"/>. On the FIRST unresolved kind it writes the historic visible error and returns
        /// false, so a command can PREFLIGHT its whole batch and abort BEFORE doing any work — never showing a
        /// partial result. Used by RACKBOMTOTAL over every placed rack.</summary>
        public static bool TryResolveAll(Editor editor, IReadOnlyList<string> kinds, out IReadOnlyList<IRackKindHandler> handlers)
        {
            if (KindHandlerRegistry.Default.TryResolveAll(kinds, out handlers, out var firstUnresolved))
            {
                return true;
            }

            editor.WriteMessage("\n" + KindDispatchMessages.NotRecognized(firstUnresolved));
            return false;
        }
    }
}
