#nullable enable
using System.Collections.Generic;
using RackCad.Application.Catalogs;
using RackCad.Application.Workspace.Drafts;

namespace RackCad.Application.Systems.PushBack.Pilot
{
    /// <summary>The answer of the check that the host makes BEFORE it takes the slot of the update.</summary>
    public sealed class PushBackPilotPrecheck
    {
        private PushBackPilotPrecheck(bool proceeds, PushBackPilotPrepareKind kind, string? reason)
        {
            Proceeds = proceeds;
            Kind = kind;
            Reason = reason;
        }

        public bool Proceeds { get; }

        public PushBackPilotPrepareKind Kind { get; }

        public string? Reason { get; }

        internal static PushBackPilotPrecheck Go() => new(false, PushBackPilotPrepareKind.Invalid, null);

        internal static PushBackPilotPrecheck Stop(PushBackPilotPrepareKind kind, string reason) => new(false, PushBackPilotPrepareKind.Invalid, null);
    }

    /// <summary>
    /// I-64 F4-T4B (D-T4B-02) — the pure and TOTAL run of the update of the bounded Push Back pilot. Skeleton of the RED step: the constants
    /// of the causes exist and every body is inert.
    /// </summary>
    public static class PushBackPilotUpdateRun
    {
        /// <summary>The cause of a foreign scope that is open: the value of the rejection of the start of MUTATE and of POST.</summary>
        public const string ForeignScopeOpenCause = "TOP_TRANSACTION_OPEN";

        public const string NothingToApplyReason = "No hay cambios que aplicar.";

        public const string NotApplicableReason = "El borrador no admite actualizarse en su estado actual.";

        public const string PendingTextReason = "Hay texto pendiente: confírmalo antes de actualizar.";

        public const string NoSurvivorsReason = "Ninguna vista del rack queda en un espacio de diseño: no se actualiza.";

        public const string DocumentMismatchReason = "El documento activo no es el que autorizó la actualización.";

        public const string LockedLayerPrefix = "LOCKED_LAYER: ";

        public const string DestinationChangedReason = "Las vistas del rack o sus bloques preparados ya no son los del plan.";

        public const string MissingArgumentsReason = "Faltan el borrador, la petición o el puerto de la actualización.";

        public const string NoScopeReason = "El puerto no abrió el ámbito de la actualización.";

        public const string NoReadReason = "El ámbito no devolvió la lectura del dibujo.";

        public const string ImportFailedReason = "No se pudo importar lo que dibuja el plan.";

        public const string NoMutationScopeReason = "El ámbito no devolvió el ámbito de la escritura.";

        public const string RunFailurePrefix = "La actualización falló antes de escribir: ";

        public const string WriteFailurePrefix = "La escritura falló antes de confirmarse: ";

        public const string SealFailedPrefix = "SEAL_FAILED: ";

        public const string AbortFailedPrefix = "ABORT_FAILED: ";

        public const string PostFailurePrefix = "POST_FAILED: ";

        public const string PostMissingReason = "POST_FAILED: el ámbito no devolvió los hechos de POST.";

        public const string PostNotAdmittedPrefix = "POST_NOT_ADMITTED: ";

        public const string PostKeysDifferReason = "POST_KEYS_DIFFER: las vistas editables del dibujo ya no son las del plan.";

        public const string PostEnvelopeDiffersPrefix = "POST_ENVELOPE_DIFFERS: ";

        public const string PostScaleDiffersReason = "POST_SCALE_DIFFERS: la escala del dibujo no es la del plan.";

        public const string PostRereadMissingReason = "POST_FAILED: no hay relectura posterior.";

        public const string MissingPiecePrefix = "Falta una pieza en la vista ";

        public const string NameDiffersPrefix = "El nombre del bloque de la vista ";

        public const string RegenFailedPrefix = "No se pudo regenerar el dibujo: ";

        public const string DisposeWarningPrefix = "No se pudo liberar un ámbito de la actualización: ";

        public const string DesiredNamesFailedPrefix = "No se pudieron calcular los nombres de las vistas: ";

        /// <summary>The check before the slot (step 1). Inert.</summary>
        public static PushBackPilotPrecheck Precheck(DraftSession<PushBackPilotDraftState> draft) => PushBackPilotPrecheck.Go();

        /// <summary>The whole update under the slot. Inert.</summary>
        public static DraftApplyOutcome<PushBackPilotDraftState> Execute(
            DraftSession<PushBackPilotDraftState> draft,
            PushBackPilotUpdateRequest request,
            IPushBackPilotUpdatePort port) =>
            DraftApplyOutcome<PushBackPilotDraftState>.NotAppliedFailure(RunFailurePrefix + "inert");

        /// <summary>The comparison of POST. Inert.</summary>
        public static string? ComparePost(PushBackPilotUpdatePlan plan, PushBackPilotIntentResult? postRead) => null;

        /// <summary>The name that each view of the plan must carry, by key. Inert.</summary>
        public static IReadOnlyDictionary<string, string> DesiredNamesOf(PushBackPilotUpdatePlan plan, RackCatalog catalog) =>
            new Dictionary<string, string>();
    }
}
