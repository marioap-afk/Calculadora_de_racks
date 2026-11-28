#nullable enable
using System;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Domain.Systems.PushBack;

namespace RackCad.Application.Systems.PushBack.Pilot
{
    public enum PushBackPilotMaterializationKind
    {
        /// <summary>The envelope is a single-sided Push Back the pilot can edit: <see cref="PushBackPilotMaterialization.State"/> is set.</summary>
        Admitted,

        /// <summary>A readable Push Back the pilot does not serve (an A/B rack, or one that parks a dormant tail): it is read-only here.</summary>
        OutOfPilot,

        /// <summary>The text could not be read or resolved: <see cref="PushBackPilotMaterialization.Reason"/> says why.</summary>
        Failed,
    }

    /// <summary>
    /// The typed result of <see cref="PushBackPilotMaterializer.Materialize"/>: Admitted(state), OutOfPilot(reason) or
    /// Failed(reason). Nothing is ever thrown to the caller.
    /// </summary>
    public sealed class PushBackPilotMaterialization
    {
        private PushBackPilotMaterialization(PushBackPilotMaterializationKind kind, PushBackPilotDraftState? state, string? reason)
        {
            Kind = kind;
            State = state;
            Reason = reason;
        }

        public PushBackPilotMaterializationKind Kind { get; }

        public bool IsAdmitted => Kind == PushBackPilotMaterializationKind.Admitted;

        /// <summary>The materialized state; null unless the result is Admitted.</summary>
        public PushBackPilotDraftState? State { get; }

        /// <summary>Why the result is not Admitted; null for Admitted.</summary>
        public string? Reason { get; }

        public static PushBackPilotMaterialization Admitted(PushBackPilotDraftState state)
            => new PushBackPilotMaterialization(PushBackPilotMaterializationKind.Admitted, state ?? throw new ArgumentNullException(nameof(state)), null);

        public static PushBackPilotMaterialization OutOfPilot(string reason)
            => new PushBackPilotMaterialization(PushBackPilotMaterializationKind.OutOfPilot, null, reason);

        public static PushBackPilotMaterialization Failed(string reason)
            => new PushBackPilotMaterialization(PushBackPilotMaterializationKind.Failed, null, reason);
    }

    /// <summary>
    /// I-64 F4-T2A (D-T2-03, D-T2-04) — materializes the pilot state from the authored envelope text; since I-64 F4B PB-T4d it no
    /// longer composes a state to write (that went with the run of the pilot). It owns NO parser: it reads through the existing
    /// RACKEDITAR path (<see cref="RackEmbedStore"/>, the
    /// kind check and <see cref="RackProjectStore"/>) and reproduces the Application part of what the classic window does
    /// to load a design, in the same order. It does not reproduce what only the controls do: LoadFromModel and the first
    /// recompute.
    /// </summary>
    public static class PushBackPilotMaterializer
    {
        /// <summary>The single diagnostic of INV-17: an unreadable envelope, or one written by a future version.</summary>
        public const string UnreadableOrFutureDiagnostic = "Ilegible o de versión futura.";

        public const string KindDivergentDiagnostic = "El sobre no es un sistema Push Back.";

        public const string NoDesignDiagnostic = "El sobre no trae un diseño Push Back legible.";

        public const string CompositeOutOfPilotReason = "Rack de dos lados (A/B): fuera del piloto.";

        public const string DormantTailOutOfPilotReason = "El diseño aparca una cola compuesta: fuera del piloto.";

        public const string CatalogRequiredDiagnostic = "Se necesita el catálogo para materializar el diseño.";

        public const string NonPilotContentReason = "El borrador cambia contenido que el piloto no edita: no se compone.";

        public const string InvalidScaleReason = "La escala de anotación no es un número finito mayor que cero: no se compone.";

        public const string BaseChangedReason = "La base ya no coincide al materializarla de nuevo: no se compone.";

        public const string MaterializeFailurePrefix = "No se pudo materializar el diseño: ";

        public const string ComposeFailurePrefix = "No se pudo componer el diseño: ";

        /// <summary>
        /// Materializes the state of the pilot from the raw authored envelope text. It never throws: the outcome is a typed
        /// result, and a failure of the existing reader or of the resolver becomes Failed with its message.
        /// </summary>
        public static PushBackPilotMaterialization Materialize(string originText, RackCatalog catalog)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(originText)) return PushBackPilotMaterialization.Failed(UnreadableOrFutureDiagnostic);
                if (catalog == null) return PushBackPilotMaterialization.Failed(CatalogRequiredDiagnostic);

                // 1. The existing reading path of RACKEDITAR, without a parser of its own.
                var embed = new RackEmbedStore().Deserialize(originText);
                if (embed == null) return PushBackPilotMaterialization.Failed(UnreadableOrFutureDiagnostic);
                if (!string.Equals(embed.Kind, RackEmbedDocument.KindPushBack, StringComparison.Ordinal))
                    return PushBackPilotMaterialization.Failed(KindDivergentDiagnostic);

                // INV-17: a major version that is too new AND an inner design that cannot be read are ONE diagnostic. The
                // "no design" diagnostic is only for a readable project that carries no Push Back design.
                if (!new RackProjectStore().TryDeserialize(embed.Design, out var project, out _))
                    return PushBackPilotMaterialization.Failed(UnreadableOrFutureDiagnostic);

                var design = project?.PushBackDesign;
                if (design == null) return PushBackPilotMaterialization.Failed(NoDesignDiagnostic);

                // A single-sided design is `!IsComposite` (the composite assembler always writes a Composite block).
                if (design.IsComposite) return PushBackPilotMaterialization.OutOfPilot(CompositeOutOfPilotReason);

                // 2. The Application part of RackPushBackSystemWindow.LoadExisting, in the same order.
                var resolver = new PushBackResolver(catalog);
                var editor = new PushBackCompositeEditorState();
                editor.SetActiveSide(PushBackSide.A);
                editor.SetSideBPresent(false);
                var inputs = editor.SideA.LoadFromDesign(design, resolver);

                editor.SetSideBPresent(false);
                editor.LoadComposite(design.Composite);

                editor.ClearDormantTail();
                var structure = design.Structure;
                if (structure != null)
                {
                    editor.ParkDormantTail(
                        structure.Modules.Where(module => module != null && PushBackCompositeStructure.IsCompositeTailId(module.ModuleId)),
                        structure.HeaderLineOverrides.Where(line => line != null && PushBackCompositeStructure.IsCompositeTailId(line.ModuleId)));
                }

                if (editor.DormantCompositeTail.Count > 0 || editor.DormantTailLineOverrides.Count > 0)
                    return PushBackPilotMaterialization.OutOfPilot(DormantTailOutOfPilotReason);

                editor.AlignSlotGrid();

                // 3. LoadFromModel and the first Recompute are NOT reproduced: they belong to the controls.
                return PushBackPilotMaterialization.Admitted(new PushBackPilotDraftState(editor, inputs, originText));
            }
            catch (Exception ex)
            {
                return PushBackPilotMaterialization.Failed(MaterializeFailurePrefix + ex.Message);
            }
        }

    }
}
