#nullable enable
using RackCad.Application.Catalogs;
using RackCad.Domain.Systems.PushBack;

namespace RackCad.Application.Systems.PushBack.Pilot
{
    /// <summary>
    /// I-64 F4-T4B (D-T4B-03, D-T4B-02) — the pure help that gives the system of the OUTGOING design of a plan. Skeleton of the RED step:
    /// the shape exists and every body is inert.
    /// </summary>
    public static class PushBackPilotSystemRebuild
    {
        public const string UnreadableDesignReason = "El diseño saliente del plan no se puede leer.";

        public const string NotPushBackReason = "El diseño saliente del plan no es de Push Back.";

        public static PushBackSystem Build(string designText, RackCatalog catalog) => new();

        public static bool IsComposite(PushBackSystem system) => false;
    }
}
