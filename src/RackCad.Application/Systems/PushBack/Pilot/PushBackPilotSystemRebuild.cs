#nullable enable
using System;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Domain.Systems.PushBack;

namespace RackCad.Application.Systems.PushBack.Pilot
{
    /// <summary>
    /// I-64 F4-T4B (D-T4B-03, D-T4B-02) — the pure help that gives the system of the OUTGOING design of a plan. The plan keeps only text,
    /// so the writer of the host rebuilds the system from the serialized design of a member, with the same resolver that PREPARE used. It
    /// reads nothing of the drawing and writes nothing.
    /// </summary>
    public static class PushBackPilotSystemRebuild
    {
        public const string UnreadableDesignReason = "El diseño saliente del plan no se puede leer.";

        public const string NotPushBackReason = "El diseño saliente del plan no es de Push Back.";

        /// <summary>
        /// The system of the serialized design of a member of the plan. A design that cannot be read, or that is not of Push Back, is an
        /// <see cref="InvalidOperationException"/> with its reason: the caller decides what that means.
        /// </summary>
        public static PushBackSystem Build(string designText, RackCatalog catalog)
        {
            if (designText == null) throw new ArgumentNullException(nameof(designText));
            if (catalog == null) throw new ArgumentNullException(nameof(catalog));

            if (!new RackProjectStore().TryDeserialize(designText, out var project, out _) || project == null)
                throw new InvalidOperationException(UnreadableDesignReason);

            var design = project.PushBackDesign;
            if (design == null) throw new InvalidOperationException(NotPushBackReason);

            return new PushBackResolver(catalog).Resolve(design);
        }

        /// <summary>True when the system is a composite rack (two sides): its frontal cuts then carry the side in their name.</summary>
        public static bool IsComposite(PushBackSystem system)
        {
            if (system == null) throw new ArgumentNullException(nameof(system));
            return system.IsComposite;
        }
    }
}
