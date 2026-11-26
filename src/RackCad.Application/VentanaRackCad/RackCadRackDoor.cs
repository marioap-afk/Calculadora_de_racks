using System;
using System.Collections.Generic;
using RackCad.Application.Persistence;
using RackCad.Application.ProjectVariables;
using RackCad.Application.Systems.Selective;
using RackCad.Application.Systems.Shared;
using RackCad.Application.Workspace;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.Selective;

namespace RackCad.Application.VentanaRackCad
{
    /// <summary>The window asks AutoCAD to read the design of ONE rack of the drawing of the session; the number ties the answer to this request.</summary>
    public sealed record RackDoorRequest(long RequestId, SessionId Session, string RackId);

    /// <summary>
    /// The window asks AutoCAD to redraw ONE rack of the drawing of the session with a draft. The design and the system are COPIES taken when
    /// the request was made, so nothing the user does afterwards reaches what AutoCAD writes; the name is already trimmed, as the classic
    /// editor sends it. With <paramref name="InsertViews"/> («Insertar vista…», step 4 of Dinámico; decision 10 of the prototype) AutoCAD
    /// first writes the draft in the views that exist and then inserts those views as linked copies of the rack (the same identity);
    /// without them it only updates. With <paramref name="NewRack"/> (a new Dinámico made in the window, step 4) the rack is not in the
    /// drawing yet: AutoCAD inserts it with those views, with the identity the window gave it and the automatic name of I-60.
    /// </summary>
    public sealed record RackDoorWrite(long RequestId, SessionId Session, string RackId, string Name, DynamicRackDesign Design, DynamicRackSystem System,
        IReadOnlyList<RackViewAddress> InsertViews = null, bool NewRack = false)
    {
        /// <summary>The write also inserts views.</summary>
        public bool Inserts => InsertViews != null && InsertViews.Count > 0;
    }

    /// <summary>What AutoCAD read of ONE rack for the RackCad window, for the request of that number: its persisted design, or why there is none.</summary>
    public sealed record RackDesignRead(long RequestId, string RackId, bool Found, string Kind, string Name, string DesignJson, string Reason);

    /// <summary>
    /// What AutoCAD answered when it wrote the draft of a rack, for the request of that number: whether it wrote it and what it said; for a
    /// new rack, the name the drawing gave it (the automatic one of I-60 when it had none).
    /// </summary>
    public sealed record RackDesignWritten(long RequestId, string RackId, bool Written, string Message, string Name = null);

    /// <summary>
    /// The door of the RackCad window to ONE rack of the drawing (I-64 A-14, line D): reading its design to edit it in the window, and
    /// writing the draft back. Both run as AutoCAD commands in the document's own command context, and the palette never locks the
    /// document for them; the focus goes back to the drawing once the command is handed over. Each request carries a number and the session
    /// it was made for: an answer is for that request only, a command that runs in another drawing than the one that was active at the
    /// request does nothing, and a request the window stopped waiting for (<see cref="Abandon"/>) does nothing when its command runs. The answers arrive later (or, if AutoCAD runs
    /// the command at once, during the call), through the events, on the thread of AutoCAD.
    /// </summary>
    public interface IRackCadRackDoorPort
    {
        /// <summary>Asks AutoCAD to read the design of the rack; the answer arrives with <see cref="DesignRead"/>.</summary>
        RackCadClassicResult RequestDesign(RackDoorRequest request);

        /// <summary>
        /// Asks AutoCAD to write the draft: an update, an update that then inserts views («Insertar vista…»), or the insertion of a new
        /// rack (see <see cref="RackDoorWrite"/>); the answer arrives with <see cref="DesignWritten"/>.
        /// </summary>
        RackCadClassicResult RequestDynamicUpdate(RackDoorWrite write);

        /// <summary>The window stopped waiting for the request: if its command has not run yet, it will do nothing when it runs.</summary>
        void Abandon(long requestId);

        event System.Action<RackDesignRead> DesignRead;

        event System.Action<RackDesignWritten> DesignWritten;

        /// <summary>
        /// The command of a write took its request (I-02 of the fifth review): its answer will come, also when AutoCAD waits for the user
        /// first (placing views). From then on the window keeps waiting: abandoning it would no longer stop what the command writes.
        /// </summary>
        event System.Action<long> RequestStarted;
    }

    /// <summary>
    /// The window asks AutoCAD to redraw ONE Selectivo of the drawing of the session with its draft (I-64 A-14, line D, Selectivo E1-A): only
    /// an update, as «Actualizar» of RACKEDITAR. The design and the system are COPIES taken when the request was made and the name is
    /// already trimmed; <paramref name="LinkedPropertyFinalStates"/> is the final state of every linked property (a linked holgura as it was
    /// read), which AutoCAD reconciles onto the authored document of the drawing so a binding nobody touched is kept.
    /// </summary>
    public sealed record SelectiveDoorWrite(
        long RequestId,
        SessionId Session,
        string RackId,
        string Name,
        SelectivePalletDesign Design,
        SelectiveRackSystem System,
        IReadOnlyDictionary<PropertyId, LinkedPropertyEditState> LinkedPropertyFinalStates,
        IReadOnlyList<RackViewAddress> InsertViews = null,
        bool NewRack = false)
    {
        /// <summary>
        /// Selectivo E2, part A: the write also inserts views (RACKSELECTIVOINSERTAR, «Insertar vista…»: first the draft in the views that
        /// exist, then the new ones as linked copies), or, with <see cref="NewRack"/>, inserts a new Selectivo with them (RACKSELECTIVONUEVO:
        /// the rack is not in the drawing yet; it takes the identity the window gave it and, with a blank name, the automatic one of I-60).
        /// </summary>
        public bool Inserts => InsertViews != null && InsertViews.Count > 0;
    }

    /// <summary>
    /// What AutoCAD read of ONE Selectivo for the RackCad window, for the request of that number: what RACKEDITAR decides before its
    /// window exists. When it opens: the EFFECTIVE design (the value in force of every linked property) as the JSON of its document, the
    /// committed state of every linked property (its literal is the authored one, frozen behind a binding) and, for display only, the
    /// names of the variables that govern them. When it does not: why (a broken reference, a register that cannot be read, another kind),
    /// exactly as RACKEDITAR refuses to open.
    /// </summary>
    public sealed record SelectiveDesignRead(
        long RequestId,
        string RackId,
        bool Opens,
        string Kind,
        string Name,
        string EffectiveDesignJson,
        IReadOnlyDictionary<PropertyId, LinkedPropertyEditState> LinkedPropertyStates,
        IReadOnlyDictionary<PropertyId, string> LinkedVariableNames,
        string Reason)
    {
        /// <summary>A read that does not open the editor, with its reason.</summary>
        public static SelectiveDesignRead Refused(long requestId, string rackId, string kind, string name, string reason)
            => new SelectiveDesignRead(requestId, rackId, false, kind, name, null, null, null, reason);

        /// <summary>True when a project variable or a formula governs that property (read only in the window, decision 12).</summary>
        public bool IsLinked(PropertyId property)
            => LinkedPropertyStates != null && LinkedPropertyStates.TryGetValue(property, out var state) && state != null
               && state.Source.Kind != LinkedPropertySourceKind.Literal;

        /// <summary>The effective design to load in the draft (a fresh object); null when the read does not open or the JSON is not one.</summary>
        public SelectivePalletDesign EffectiveDesign()
        {
            if (!Opens || string.IsNullOrWhiteSpace(EffectiveDesignJson)) return null;
            try
            {
                return new SelectivePalletDesignStore().Deserialize(EffectiveDesignJson)?.ToDomain();
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }
    }

    /// <summary>What RACKSELECTIVOAPLICAR decided before drawing: the authored document of the drawing now, or why nothing is written.</summary>
    public sealed record SelectiveApplyPlan(bool CanWrite, SelectivePalletDesignDocument Saved, string Reason);

    /// <summary>
    /// The door of the RackCad window to ONE Selectivo of the drawing (I-64 A-14, line D, Selectivo E1-A), with the same numbering, the same
    /// other-drawing check, the same «Dejar de esperar» (<see cref="IRackCadRackDoorPort.Abandon"/>) and the same
    /// <see cref="IRackCadRackDoorPort.RequestStarted"/> as the door of Dinámico it extends. The write answers with
    /// <see cref="IRackCadRackDoorPort.DesignWritten"/>.
    /// </summary>
    public interface IRackCadSelectiveDoorPort : IRackCadRackDoorPort
    {
        /// <summary>Asks AutoCAD to read the Selectivo (RACKSELECTIVOABRIR); the answer arrives with <see cref="SelectiveRead"/>.</summary>
        RackCadClassicResult RequestSelectiveDesign(RackDoorRequest request);

        /// <summary>
        /// Asks AutoCAD to redraw the Selectivo with the draft (RACKSELECTIVOAPLICAR), to redraw it and then insert views
        /// (RACKSELECTIVOINSERTAR) or to insert a new Selectivo (RACKSELECTIVONUEVO), as <see cref="SelectiveDoorWrite"/> says; the answer
        /// arrives with <see cref="IRackCadRackDoorPort.DesignWritten"/>.
        /// </summary>
        RackCadClassicResult RequestSelectiveUpdate(SelectiveDoorWrite write);

        event System.Action<SelectiveDesignRead> SelectiveRead;
    }

    /// <summary>The rules of the door that need no AutoCAD, so they are tested where AutoCAD is not.</summary>
    public static class RackDoorRules
    {
        /// <summary>
        /// The answer of RACKDINAMICOAPLICAR from what the drawing half of RACKEDITAR did (I-08 of the third review): «written» only when
        /// at least one view was redrawn. Otherwise the draft stays not written, and the answer says whether views were erased. Declared
        /// (M-28 of the fourth review): a write where one view is redrawn and another fails counts as written, as it does for RACKEDITAR.
        /// </summary>
        public static RackDesignWritten AnswerForApply(long requestId, string rackId, int redrawn, int erased)
        {
            if (redrawn > 0)
            {
                var text = "AutoCAD redibujó " + Count(redrawn, "vista", "vistas") + " del rack con el borrador";
                if (erased > 0) text += " y borró " + Count(erased, "vista que ya no existe", "vistas que ya no existen");
                return new RackDesignWritten(requestId, rackId, true, text + "; la línea de órdenes da el recuento por tipo de vista.");
            }

            if (erased > 0)
            {
                return new RackDesignWritten(requestId, rackId, false,
                    "AutoCAD no redibujó ninguna vista del rack, pero borró " + Count(erased, "vista que ya no existe", "vistas que ya no existen")
                    + ": revisa el dibujo y la línea de órdenes. El borrador sigue sin escribir.");
            }

            return new RackDesignWritten(requestId, rackId, false,
                "AutoCAD no redibujó ninguna vista del rack, así que el borrador sigue sin escribir. Revisa la línea de órdenes de AutoCAD.");
        }

        /// <summary>
        /// The answer of RACKDINAMICOINSERTAR («Insertar vista…»): the draft is written when the views that exist were redrawn with it, as
        /// for RACKDINAMICOAPLICAR; then the answer says how many new views were inserted (none when the placement was cancelled). When no
        /// view was redrawn the draft stays not written, even if a view was inserted, so «Actualizar» can still write it in all of them.
        /// </summary>
        public static RackDesignWritten AnswerForInsert(long requestId, string rackId, int redrawn, int erased, int inserted)
        {
            var applied = AnswerForApply(requestId, rackId, redrawn, erased);
            if (!applied.Written)
            {
                return inserted > 0
                    ? new RackDesignWritten(requestId, rackId, false,
                        "AutoCAD insertó " + Count(inserted, "vista nueva", "vistas nuevas") + ", pero no redibujó las vistas que ya había, así que el "
                        + "borrador sigue sin escribir: usa «Actualizar» y revisa la línea de órdenes de AutoCAD.")
                    : applied;
            }

            var text = "AutoCAD redibujó " + Count(redrawn, "vista", "vistas") + " del rack con el borrador";
            if (erased > 0) text += ", borró " + Count(erased, "vista que ya no existe", "vistas que ya no existen");
            text += inserted > 0
                ? " e insertó " + Count(inserted, "vista nueva", "vistas nuevas") + "."
                : ", pero no insertó ninguna vista nueva: se canceló la colocación o no se pudo preparar. Revisa la línea de órdenes de AutoCAD.";
            return new RackDesignWritten(requestId, rackId, true, text);
        }

        /// <summary>
        /// The answer of RACKDINAMICONUEVO (a new Dinámico of the window): written when at least one view of the new rack was placed, with the
        /// name the drawing gave it; otherwise the draft stays in the window, not written.
        /// </summary>
        public static RackDesignWritten AnswerForNewRack(long requestId, string rackId, int placed, string name)
        {
            if (placed > 0)
            {
                var named = string.IsNullOrWhiteSpace(name) ? "el rack nuevo" : "el rack nuevo «" + name.Trim() + "»";
                return new RackDesignWritten(requestId, rackId, true,
                    "AutoCAD insertó " + named + " con " + Count(placed, "vista", "vistas") + ". Para seguir editándolo aquí, vuelve a leer la lista y ábrelo.",
                    string.IsNullOrWhiteSpace(name) ? null : name.Trim());
            }

            return new RackDesignWritten(requestId, rackId, false,
                "AutoCAD no insertó el rack nuevo: se canceló la colocación o no se pudo preparar. El borrador sigue en la ventana; revisa la línea de órdenes de AutoCAD.");
        }

        /// <summary>
        /// The answer of RACKSELECTIVOABRIR (Selectivo E1-A): what RACKEDITAR decides before its window exists, from the kind and the
        /// inner design of the rack and the register of project variables of the drawing. It opens exactly when RACKEDITAR opens: the
        /// document must be read, <see cref="SelectiveEditorOpen.Resolve"/> must open it (a broken reference, a kind from the future or a
        /// register that cannot be used block it, with no fallback to the frozen literal) and the options of the linked holgura must be
        /// usable. The name is the one of the inner document, as the classic editor shows it.
        /// </summary>
        public static SelectiveDesignRead AnswerForSelectiveOpen(long requestId, string rackId, string kind, string designJson, ProjectVariablesReadResult registry)
        {
            if (!string.Equals(kind, RackEmbedDocument.KindSelective, StringComparison.Ordinal))
                return SelectiveDesignRead.Refused(requestId, rackId, kind, null, "El rack no es un Selectivo: este editor no lo abre.");

            SelectivePalletDesignDocument saved;
            try
            {
                saved = new SelectivePalletDesignStore().Deserialize(designJson);
            }
            catch (Exception ex)
            {
                return SelectiveDesignRead.Refused(requestId, rackId, kind, null, "No se pudieron leer los datos del rack. " + ex.Message);
            }

            if (saved == null) return SelectiveDesignRead.Refused(requestId, rackId, kind, null, "No se pudieron leer los datos del rack.");

            var open = SelectiveEditorOpen.Resolve(saved, registry);
            if (!open.IsOpen) return SelectiveDesignRead.Refused(requestId, rackId, kind, saved.Name, open.Error);

            var options = LinkedPropertyOptions.ForProperty(ProjectPropertyIds.SelectiveVerticalClearance, registry);
            if (!options.IsUsable) return SelectiveDesignRead.Refused(requestId, rackId, kind, saved.Name, options.Error);

            // The names are for display only, read from the same accredited register that resolved the values.
            var names = new Dictionary<PropertyId, string>();
            var accreditation = UsableProjectVariablesRegistry.Accredit(registry);
            if (accreditation.IsUsable)
            {
                foreach (var pair in open.LinkedPropertyStates)
                {
                    if (pair.Value != null && pair.Value.IsReference && accreditation.Registry.TryGetTarget(pair.Value.Source.VariableId, out var target))
                        names[pair.Key] = target.Name;
                }
            }

            var json = new SelectivePalletDesignStore().Serialize(SelectivePalletDesignDocument.From(open.Design, saved.Id, saved.Name));
            return new SelectiveDesignRead(requestId, rackId, true, kind, saved.Name, json, open.LinkedPropertyStates, names, null);
        }

        /// <summary>
        /// What RACKSELECTIVOAPLICAR decides before the drawing half of RACKEDITAR runs (Selectivo E1-A), from the rack and the register
        /// read AGAIN at write time. The classic editor is modal, so what it opened is what it writes on; the window is not, so the drawing
        /// may have changed in between. Nothing is written when: the rack is not a readable Selectivo; it would not open now (the same
        /// rule as the read); the binding of a holgura is not the one the window opened with (written through the window, the opened state
        /// would silently undo it); the reconciliation of the classic path would fail; or the variable that governs a linked holgura has
        /// another value now (the draft was drawn with the old one, and the drawing would not match its own document).
        /// </summary>
        public static SelectiveApplyPlan PlanSelectiveApply(string kind, string designJson, ProjectVariablesReadResult registry, SelectiveDoorWrite write)
        {
            const string NotReadable = "El rack del dibujo no es un Selectivo legible: no se escribió nada.";
            if (write?.Design == null) return new SelectiveApplyPlan(false, null, "No hay un borrador válido que escribir.");
            if (!string.Equals(kind, RackEmbedDocument.KindSelective, StringComparison.Ordinal)) return new SelectiveApplyPlan(false, null, NotReadable);

            SelectivePalletDesignDocument saved;
            try
            {
                saved = new SelectivePalletDesignStore().Deserialize(designJson);
            }
            catch (Exception)
            {
                return new SelectiveApplyPlan(false, null, NotReadable);
            }

            if (saved == null) return new SelectiveApplyPlan(false, null, NotReadable);

            var open = SelectiveEditorOpen.Resolve(saved, registry);
            if (!open.IsOpen) return new SelectiveApplyPlan(false, null, open.Error + " No se escribió nada.");

            var finals = write.LinkedPropertyFinalStates ?? new Dictionary<PropertyId, LinkedPropertyEditState>();
            var properties = new HashSet<PropertyId>(open.LinkedPropertyStates.Keys);
            properties.UnionWith(finals.Keys);
            foreach (var property in properties)
            {
                open.LinkedPropertyStates.TryGetValue(property, out var now);
                finals.TryGetValue(property, out var final);
                var linked = (now != null && now.Source.Kind != LinkedPropertySourceKind.Literal)
                             || (final != null && final.Source.Kind != LinkedPropertySourceKind.Literal);
                if (linked && !Equals(now, final))
                {
                    return new SelectiveApplyPlan(false, null,
                        "El vínculo de " + LinkedLabel(property) + " cambió en el dibujo desde que se abrió el rack aquí: no se escribió nada. "
                        + "Vuelve a abrir el rack para escribir con el vínculo actual.");
                }
            }

            var reconciled = LinkedPropertyReconciler.Reconcile(saved, write.Design, finals, registry, write.RackId, write.Name);
            if (!reconciled.IsSuccess) return new SelectiveApplyPlan(false, null, reconciled.Error + " No se escribió nada.");

            foreach (var pair in finals)
            {
                if (pair.Value == null || pair.Value.Source.Kind == LinkedPropertySourceKind.Literal) continue;
                if (!SelectiveLinkedProperties.All.TryGetDescriptor(pair.Key, out var descriptor)) continue;
                if (descriptor.ReadEffective(reconciled.Effective) != descriptor.ReadEffective(write.Design))
                {
                    return new SelectiveApplyPlan(false, null,
                        "El valor de lo que gobierna " + LinkedLabel(pair.Key) + " cambió desde que se abrió el rack aquí: no se escribió nada. "
                        + "Vuelve a abrir el rack para escribir con su valor actual.");
                }
            }

            return new SelectiveApplyPlan(true, saved, null);
        }

        private static string LinkedLabel(PropertyId property)
            => property == ProjectPropertyIds.SelectiveVerticalClearance ? "la holgura vertical"
                : property == ProjectPropertyIds.SelectivePalletTolerance ? "la tolerancia horizontal"
                : "la propiedad «" + property + "»";

        private static string Count(int count, string one, string many) =>
            count.ToString(System.Globalization.CultureInfo.InvariantCulture) + " " + (count == 1 ? one : many);
    }
}
