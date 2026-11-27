using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Application.Workspace;

namespace RackCad.Application.VentanaRackCad
{
    // T6 at (I-64, sexta tanda): «Actualizar todo». The decisions of the Owner of 2026-11-23 and the A-14, C-05 (D-09 and D-20): next to the
    // «Actualizar» of every rack, an «Actualizar todo» outside the racks writes, in ONE operation, every draft of the window with changes of
    // the active drawing; all or nothing (if one fails, none is written, and the window says which one and why); one step of Undo. It is a NEW
    // composition over the doors of every system: the «Actualizar» of a rack and the seam of one rack of I-55 (RackSiblingRedrawRun,
    // ISiblingRedrawPort, SiblingRedrawTransaction) do not change.

    /// <summary>The drafts «Actualizar todo» writes: the six editors of the window and the page «Variables» (decision Q-02 of the Owner).</summary>
    public enum UpdateAllSystem
    {
        Variables,
        Selective,
        PushBack,
        Dynamic,
        Cantilever,
        Cabecera,
        Cama,
    }

    /// <summary>
    /// One draft of «Actualizar todo»: exactly the write its own «Actualizar» would hand to its door (COPIES taken when the request was made),
    /// so the command writes what each «Actualizar» writes. Exactly one of the writes is set, the one of <see cref="System"/>.
    /// </summary>
    public sealed record UpdateAllItem(
        UpdateAllSystem System,
        string RackId,
        string Name,
        RackDoorWrite Dynamic = null,
        SelectiveDoorWrite Selective = null,
        PushBackDoorWrite PushBack = null,
        CantileverDoorWrite Cantilever = null,
        CamaDoorWrite Cama = null,
        CabeceraDoorWrite Cabecera = null,
        VariablesDoorWrite Variables = null)
    {
        /// <summary>The write of the item matches its system (and is the only one); the door refuses an item that does not.</summary>
        public bool IsWellFormed
        {
            get
            {
                var set = new object[] { Dynamic, Selective, PushBack, Cantilever, Cama, Cabecera, Variables }.Count(w => w != null);
                if (set != 1) return false;
                return System switch
                {
                    UpdateAllSystem.Dynamic => Dynamic != null && !Dynamic.NewRack && !Dynamic.Inserts,
                    UpdateAllSystem.Selective => Selective != null && !Selective.NewRack && !Selective.Inserts,
                    UpdateAllSystem.PushBack => PushBack != null && !PushBack.NewRack && !PushBack.Inserts,
                    UpdateAllSystem.Cantilever => Cantilever != null && !Cantilever.NewRack && (Cantilever.InsertViews == null || Cantilever.InsertViews.Count == 0),
                    UpdateAllSystem.Cama => Cama != null && !Cama.NewRack,
                    UpdateAllSystem.Cabecera => Cabecera != null,
                    UpdateAllSystem.Variables => Variables != null,
                    _ => false,
                };
            }
        }
    }

    /// <summary>«Actualizar todo»: its number, the drawing it is for and its drafts, in the order the command writes them.</summary>
    public sealed record UpdateAllWrite(long RequestId, SessionId Session, IReadOnlyList<UpdateAllItem> Items);

    /// <summary>How one draft of «Actualizar todo» ended.</summary>
    public enum UpdateAllOutcome
    {
        /// <summary>Written, inside the one operation that was confirmed.</summary>
        Applied,

        /// <summary>Written, with a notice (a view that was not redrawn, what happened after the confirmation).</summary>
        AppliedWithWarnings,

        /// <summary>This draft is why nothing was written: its cause says why.</summary>
        Failed,

        /// <summary>Not written because another draft of the set failed (all or nothing).</summary>
        NotAppliedOtherFailed,

        /// <summary>Not written: «Actualizar todo» was cancelled before the confirmation.</summary>
        Cancelled,

        /// <summary>The confirmation of AutoCAD failed: nobody knows whether it was written; the drawing must be read again.</summary>
        Unknown,
    }

    /// <summary>
    /// What happened to one draft: its outcome, the cause in the words of the window and the answer of its system, as its own «Actualizar»
    /// answers (<see cref="Written"/> for a rack, <see cref="VariablesWritten"/> for the page «Variables»), so the window leaves the editor as
    /// after its «Actualizar».
    /// </summary>
    public sealed record UpdateAllItemResult(
        UpdateAllSystem System,
        string RackId,
        string Name,
        UpdateAllOutcome Outcome,
        string Cause,
        RackDesignWritten Written = null,
        VariablesDoorWritten VariablesWritten = null)
    {
        public bool IsWritten => Outcome == UpdateAllOutcome.Applied || Outcome == UpdateAllOutcome.AppliedWithWarnings;
    }

    /// <summary>The answer of RACKACTUALIZARTODO: whether the one operation was confirmed, what to tell, and every draft with its outcome.</summary>
    public sealed record UpdateAllWritten(long RequestId, bool Committed, string Message, IReadOnlyList<UpdateAllItemResult> Items);

    /// <summary>
    /// The door of «Actualizar todo» (T6 at): the same rules as the door of a rack — a number per request, the drawing it was made for,
    /// «Dejar de esperar» (<see cref="IRackCadRackDoorPort.Abandon"/>) until the command takes it, and <see cref="IRackCadRackDoorPort.RequestStarted"/>
    /// when it took it. A window gets it only when its door to the racks also implements it.
    /// </summary>
    public interface IRackCadUpdateAllDoorPort : IRackCadRackDoorPort
    {
        /// <summary>Asks AutoCAD to write every draft of the request in ONE operation (RACKACTUALIZARTODO); the answer arrives with <see cref="UpdateAllWritten"/>.</summary>
        RackCadClassicResult RequestUpdateAll(UpdateAllWrite write);

        event Action<UpdateAllWritten> UpdateAllWritten;
    }

    // ------------------------------------------------------------------ elegibility (condition 4 of the A-14, adapted)

    /// <summary>
    /// One draft of the window as «Actualizar todo» sees it, from the editor or the page, in a plain value. <paramref name="BlockedReason"/>
    /// is why its own «Actualizar» cannot write now (an error, a value rejected and not corrected, text not applied); empty when it can.
    /// <paramref name="PossiblyStale"/> is the mark «posiblemente desactualizado» of the editor (part st of the sixth batch; the session
    /// connects it when the parts are joined).
    /// </summary>
    public sealed record UpdateAllCandidate(
        UpdateAllSystem System,
        string RackId,
        string Name,
        bool IsDirty,
        bool IsWriting,
        bool IsNewRack,
        string BlockedReason,
        bool PossiblyStale = false);

    /// <summary>A draft that prevents «Actualizar todo», with its cause.</summary>
    public sealed record UpdateAllBlocker(UpdateAllCandidate Candidate, string Cause);

    /// <summary>What «Actualizar todo» would write now and what prevents it.</summary>
    public sealed record UpdateAllEligibilityResult(IReadOnlyList<UpdateAllCandidate> Included, IReadOnlyList<UpdateAllBlocker> Blockers)
    {
        public int Count => Included.Count;

        /// <summary>There is something to write and nothing prevents it.</summary>
        public bool CanRun => Included.Count > 0 && Blockers.Count == 0;

        /// <summary>A draft marked «posiblemente desactualizado» is included: «Actualizar todo» asks first, as «Actualizar» does.</summary>
        public bool NeedsConfirmation => Included.Any(c => c.PossiblyStale);

        public IReadOnlyList<UpdateAllCandidate> PossiblyStale => Included.Where(c => c.PossiblyStale).ToList();

        /// <summary>Why «Actualizar todo» cannot run, in the words of the window; empty when it can.</summary>
        public string BlockedText
        {
            get
            {
                if (Blockers.Count > 0)
                {
                    return "«Actualizar todo» no puede escribir mientras "
                           + (Blockers.Count == 1 ? "este borrador no se corrija: " : "estos borradores no se corrijan: ")
                           + string.Join("; ", Blockers.Select(b => UpdateAllText.Quoted(b.Candidate.Name) + ": " + UpdateAllText.Sentence(b.Cause)))
                           + " No se escribiría nada.";
                }

                return Included.Count == 0 ? UpdateAllText.NothingToWrite : string.Empty;
            }
        }
    }

    /// <summary>The pure rules of «Actualizar todo» that need no AutoCAD: who is included, who prevents it, and in which order.</summary>
    public static class UpdateAllEligibility
    {
        /// <summary>
        /// Condition 4 of the A-14, adapted to the window: a draft «Modificado» of the active drawing is included; a clean one is not, nor
        /// a draft of «Nuevo» not inserted yet (it needs its point of insertion); a draft that is being written, or whose own «Actualizar»
        /// is blocked (an error, a value rejected and not corrected), PREVENTS «Actualizar todo»: zero writes, and the window names it with
        /// its cause. A draft marked «posiblemente desactualizado» is included, and «Actualizar todo» asks first. Variables go first
        /// (decision Q-02: the racks are not written over variables that are about to change), then the racks by system, in the order of
        /// the list of the window, then by name.
        /// </summary>
        public static UpdateAllEligibilityResult Decide(IEnumerable<UpdateAllCandidate> candidates)
        {
            var included = new List<UpdateAllCandidate>();
            var blockers = new List<UpdateAllBlocker>();
            foreach (var candidate in candidates ?? Array.Empty<UpdateAllCandidate>())
            {
                if (candidate == null || candidate.IsNewRack) continue;
                if (candidate.IsWriting)
                {
                    blockers.Add(new UpdateAllBlocker(candidate, "AutoCAD lo está escribiendo; espera a que termine."));
                    continue;
                }

                if (!candidate.IsDirty) continue;
                if (!string.IsNullOrWhiteSpace(candidate.BlockedReason))
                {
                    blockers.Add(new UpdateAllBlocker(candidate, candidate.BlockedReason.Trim()));
                    continue;
                }

                included.Add(candidate);
            }

            return new UpdateAllEligibilityResult(Ordered(included, c => c).ToList(), Ordered(blockers, b => b.Candidate).ToList());
        }

        private static IEnumerable<T> Ordered<T>(IEnumerable<T> values, Func<T, UpdateAllCandidate> candidate)
            => values
                .OrderBy(v => (int)candidate(v).System)
                .ThenBy(v => candidate(v).Name ?? string.Empty, StringComparer.CurrentCultureIgnoreCase)
                .ThenBy(v => candidate(v).RackId ?? string.Empty, StringComparer.OrdinalIgnoreCase);
    }

    // ------------------------------------------------------------------ the composition (conditions 1 and 4, «Dice cuál falló y por qué»)

    /// <summary>
    /// What the re-read of one draft found, before anything is written: whether it can be written now, why not, and (for the page
    /// «Variables») the racks its write would redraw. <paramref name="State"/> is what the host keeps for the write (the blocks, the plan).
    /// </summary>
    public sealed record UpdateAllCheck(bool CanWrite, string Reason, IReadOnlyList<string> RedrawnRackIds = null, object State = null)
    {
        public static UpdateAllCheck Ready(object state = null, IReadOnlyList<string> redrawnRackIds = null) => new UpdateAllCheck(true, null, redrawnRackIds, state);

        public static UpdateAllCheck Refused(string reason) => new UpdateAllCheck(false, reason);
    }

    /// <summary>
    /// What the write of one draft did, inside the one operation: whether it wrote (by the rule of its own «Actualizar»), why not, a notice,
    /// and the answer of its system.
    /// </summary>
    /// <remarks>
    /// <paramref name="PendingImports"/> (review of c4985a2e, I-2): the blocks of the library the draft needs and the drawing does not have
    /// yet. Nothing is imported inside the transaction: the composition discards the pass, the host imports them OUTSIDE, and the set is
    /// re-read and written again.
    /// </remarks>
    public sealed record UpdateAllApplied(bool Written, string Reason, string Warning = null, RackDesignWritten Answer = null, VariablesDoorWritten VariablesAnswer = null,
        IReadOnlyList<string> PendingImports = null)
    {
        public static UpdateAllApplied Failed(string reason) => new UpdateAllApplied(false, reason);

        public bool NeedsImports => PendingImports != null && PendingImports.Count > 0;
    }

    /// <summary>
    /// The pure rule of «escrito» for a rack inside «Actualizar todo» (review of c4985a2e, B-1). The drawing helpers swallow the failure of
    /// one view (the redraw of that view returns a failure and the rack goes on), so the rule of the «Actualizar» of a rack (at least one view
    /// redrawn) could leave a rack half written in an operation that is confirmed. Inside the set a rack counts as written only when every
    /// view it had was redrawn or erased and, read again inside the transaction, the views that remain are those and carry one design.
    /// </summary>
    public static class UpdateAllVerification
    {
        /// <summary>Why the rack is not wholly written; null when it is.</summary>
        public static string AfterRackWrite(int viewsBefore, int redrawn, int erased, int viewsAfter, bool viewsCoincide, string siblingsReason)
        {
            var missing = viewsBefore - redrawn - erased;
            if (missing > 0)
            {
                return "AutoCAD no redibujó " + UpdateAllText.Count(missing, "vista", "vistas") + " de las "
                       + viewsBefore.ToString(CultureInfo.InvariantCulture) + " del rack (la línea de órdenes dice cuál): el rack quedaría a medias.";
            }

            var expected = viewsBefore - erased;
            if (viewsAfter != expected)
            {
                return "Releído tras escribirlo, el rack tiene " + UpdateAllText.Count(viewsAfter, "vista", "vistas") + " y debían quedar "
                       + expected.ToString(CultureInfo.InvariantCulture) + ": el rack quedaría a medias.";
            }

            if (!viewsCoincide)
            {
                return "Releído tras escribirlo, sus vistas no llevan el mismo diseño: el rack quedaría a medias."
                       + (string.IsNullOrWhiteSpace(siblingsReason) ? "" : " " + UpdateAllText.Sentence(siblingsReason));
            }

            return null;
        }
    }

    /// <summary>
    /// The host of the composition (the AutoCAD command, or a double in the tests): ONE outer transaction, opened by <see cref="Begin"/> and
    /// closed by <see cref="Commit"/> or <see cref="Rollback"/>; the re-read of every draft inside it before any write; the writes; and the
    /// cancellation, which is only asked before the confirmation.
    /// </summary>
    public interface IUpdateAllUnitPort
    {
        /// <summary>Opens the one outer transaction of the operation.</summary>
        void Begin();

        /// <summary>Re-reads and compares one draft with the drawing, inside the transaction, writing nothing.</summary>
        UpdateAllCheck Preflight(UpdateAllItem item);

        /// <summary>The user asked to cancel (ESC in AutoCAD). Only asked before <see cref="Commit"/>.</summary>
        bool CancelRequested();

        /// <summary>Writes one draft inside the transaction, with what its re-read kept.</summary>
        UpdateAllApplied Apply(UpdateAllItem item, UpdateAllCheck check);

        /// <summary>Confirms the operation; it may throw.</summary>
        void Commit();

        /// <summary>Discards everything the operation wrote (also when nothing was written; idempotent).</summary>
        void Rollback();

        /// <summary>
        /// Imports the blocks of the library a pass needed (review of c4985a2e, I-2), with NO transaction open, as the «Actualizar» of every
        /// rack imports before its own transaction. Null when they were imported; otherwise why not.
        /// </summary>
        string ImportOutside(IReadOnlyCollection<string> blocks);

        /// <summary>The progress while the operation lasts: a step of a total, and its draft (null at the end).</summary>
        void Progress(string step, int done, int total, UpdateAllItem item);

        /// <summary>After the confirmation or the rollback (the one regeneration); a failure here is a notice, never a rollback.</summary>
        void Finish(bool committed);
    }

    /// <summary>
    /// The pure composition of «Actualizar todo» (the A-14, C-05): every draft re-read and compared inside ONE transaction before any write;
    /// all or nothing (a refusal, a failure, a cancellation or a confirmation that fails leaves zero writes, or says it cannot know); and the
    /// list of results, where every draft has its cause or «No aplicado: falló otro rack del conjunto».
    /// </summary>
    public static class UpdateAllRun
    {
        /// <summary>The result of a request the command could not run at all (another drawing, an exception before the composition).</summary>
        public static UpdateAllWritten Refused(UpdateAllWrite write, string reason)
        {
            var requestId = write?.RequestId ?? 0;
            var items = (write?.Items ?? Array.Empty<UpdateAllItem>()).Where(i => i != null).ToList();
            var cause = string.IsNullOrWhiteSpace(reason) ? "No se escribió nada." : reason.Trim();
            return new UpdateAllWritten(requestId, false, "No se escribió nada: " + UpdateAllText.Sentence(cause),
                items.Select(i => NotWritten(requestId, i, UpdateAllOutcome.NotAppliedOtherFailed, cause)).ToList());
        }

        /// <summary>
        /// Runs the composition over the host. The host's transaction is always closed: confirmed only when every draft was re-read and
        /// written, discarded otherwise. <see cref="IUpdateAllUnitPort.Finish"/> runs last, also after a refusal, and its failure is a notice.
        /// </summary>
        public static UpdateAllWritten Execute(UpdateAllWrite write, IUpdateAllUnitPort port)
        {
            if (port == null) throw new ArgumentNullException(nameof(port));
            if (write == null || write.Items == null || write.Items.Count == 0) return Refused(write, UpdateAllText.NothingToWrite);
            if (write.Items.Any(i => i == null || !i.IsWellFormed)) return Refused(write, "La petición de «Actualizar todo» no es válida.");

            var state = new RunState();
            UpdateAllWritten answer;
            try
            {
                answer = Compose(write, port, state);
            }
            finally
            {
                if (state.Begun && !state.Committed) SafeRollback(port);
            }

            if (!state.Begun) return answer;
            try
            {
                port.Finish(state.Committed);
            }
            catch (Exception ex)
            {
                // A failure after the confirmation never undoes it: it is said.
                answer = answer with { Message = answer.Message + " Después, AutoCAD avisó: " + UpdateAllText.Sentence(ex.Message) };
            }

            return answer;
        }

        private sealed class RunState
        {
            public bool Begun;

            public bool Committed;
        }

        private static UpdateAllWritten Compose(UpdateAllWrite write, IUpdateAllUnitPort port, RunState state)
        {
            var items = write.Items;
            var total = items.Count;
            UpdateAllApplied[] applied;

            // A pass: the re-read and the writes inside one transaction. A pass whose drafts need blocks of the library the drawing lacks is
            // discarded, they are imported OUTSIDE any transaction (review of c4985a2e, I-2), and a second pass re-reads and writes the set.
            for (var pass = 1; ; pass++)
            {
                try
                {
                    port.Begin();
                    state.Begun = true;
                }
                catch (Exception ex)
                {
                    return Refused(write, "AutoCAD no pudo empezar la operación: " + ex.Message);
                }

                // 1. The re-read of every draft, inside the transaction and before any write. Every refusal is said with its cause.
                var checks = new UpdateAllCheck[total];
                var refusals = new string[total];
                for (var index = 0; index < total; index++)
                {
                    port.Progress("Comprobando", index, total, items[index]);
                    UpdateAllCheck check;
                    try
                    {
                        check = port.Preflight(items[index]) ?? UpdateAllCheck.Refused("La comprobación no respondió.");
                    }
                    catch (Exception ex)
                    {
                        check = UpdateAllCheck.Refused("No se pudo comprobar: " + ex.Message);
                    }

                    checks[index] = check;
                    if (!check.CanWrite) refusals[index] = string.IsNullOrWhiteSpace(check.Reason) ? "No se puede escribir." : check.Reason.Trim();
                }

                // 2. The page «Variables» is written first: a rack it would redraw must not be written over it with a draft resolved
                // against the variables of before (decision Q-02). Both are named.
                for (var index = 0; index < total; index++)
                {
                    if (items[index].System != UpdateAllSystem.Variables || refusals[index] != null) continue;
                    var redrawn = new HashSet<string>(checks[index].RedrawnRackIds ?? Array.Empty<string>(), StringComparer.OrdinalIgnoreCase);
                    var overlap = Enumerable.Range(0, total)
                        .Where(other => items[other].System != UpdateAllSystem.Variables && items[other].RackId != null && redrawn.Contains(items[other].RackId))
                        .ToList();
                    if (overlap.Count == 0) continue;
                    refusals[index] = "redibujaría " + UpdateAllText.List(overlap.Select(o => UpdateAllText.Quoted(items[o].Name)))
                                      + ", que también " + (overlap.Count == 1 ? "tiene" : "tienen") + " un borrador sin escribir: escribe o descarta antes uno de los dos.";
                    foreach (var other in overlap)
                        refusals[other] ??= "«Variables» también lo redibuja: escribe o descarta antes uno de los dos.";
                }

                if (refusals.Any(r => r != null))
                {
                    port.Rollback();
                    return Answer(write, false, "No se escribió nada: " + FailureSummary(items, refusals),
                        Enumerable.Range(0, total).Select(i => refusals[i] != null
                            ? NotWritten(write.RequestId, items[i], UpdateAllOutcome.Failed, refusals[i])
                            : NotWritten(write.RequestId, items[i], UpdateAllOutcome.NotAppliedOtherFailed, UpdateAllText.OtherFailed)).ToList());
                }

                // 3. The writes, in order; a cancellation is only heard before the confirmation.
                applied = new UpdateAllApplied[total];
                for (var index = 0; index < total; index++)
                {
                    if (Cancelled(port))
                    {
                        port.Rollback();
                        return CancelledAnswer(write);
                    }

                    port.Progress("Escribiendo", index, total, items[index]);
                    UpdateAllApplied result;
                    try
                    {
                        result = port.Apply(items[index], checks[index]) ?? UpdateAllApplied.Failed("La escritura no respondió.");
                    }
                    catch (Exception ex)
                    {
                        result = UpdateAllApplied.Failed("No se pudo escribir: " + ex.Message);
                    }

                    if (!result.Written)
                    {
                        port.Rollback();
                        var cause = string.IsNullOrWhiteSpace(result.Reason) ? "No se pudo escribir." : result.Reason.Trim();
                        return Answer(write, false,
                            "No se escribió nada: falló " + UpdateAllText.Quoted(items[index].Name) + ". " + UpdateAllText.Sentence(cause),
                            Enumerable.Range(0, total).Select(i => i == index
                                ? NotWritten(write.RequestId, items[i], UpdateAllOutcome.Failed, cause)
                                : NotWritten(write.RequestId, items[i], UpdateAllOutcome.NotAppliedOtherFailed, UpdateAllText.OtherFailed)).ToList());
                    }

                    applied[index] = result;
                }

                // 3b. Blocks of the library the drawing lacks: nothing was imported inside the transaction. The pass is discarded, they are
                // imported outside it, and the set is read and written again; if they are still lacking, the drafts that need them fail.
                var pending = applied.Where(a => a.NeedsImports).SelectMany(a => a.PendingImports)
                    .Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(k => k, StringComparer.OrdinalIgnoreCase).ToList();
                if (pending.Count == 0) break;
                port.Rollback();
                var needing = applied;
                if (pass >= 2)
                {
                    return ImportFailed(write, needing, "Necesita bloques de la biblioteca que siguen sin estar en el dibujo tras importarlos ("
                                                        + string.Join(", ", pending) + ").");
                }

                port.Progress("Importando bloques de la biblioteca", 0, pending.Count, null);
                string importError;
                try
                {
                    importError = port.ImportOutside(pending);
                }
                catch (Exception ex)
                {
                    importError = ex.Message;
                }

                if (importError != null)
                {
                    return ImportFailed(write, needing, "No se pudieron importar los bloques de la biblioteca que necesita ("
                                                        + string.Join(", ", pending) + "): " + UpdateAllText.Sentence(importError));
                }
            }

            if (Cancelled(port))
            {
                port.Rollback();
                return CancelledAnswer(write);
            }

            // 4. The confirmation: if it throws, nobody knows what the drawing holds (the classification of D-13 for the set).
            port.Progress("Confirmando", total, total, null);
            try
            {
                port.Commit();
                state.Committed = true;
            }
            catch (Exception ex)
            {
                // The transaction is not confirmed for the composition (state.Committed stays false), so it is discarded too; what
                // AutoCAD kept is unknown, and every draft says to read the drawing again.
                const string Unknown = "No se sabe si AutoCAD lo escribió: falló la confirmación de la operación. Vuelve a leerlo del dibujo antes de seguir.";
                return Answer(write, false, "No se sabe qué escribió AutoCAD: falló la confirmación de «Actualizar todo» (" + ex.Message
                                            + "). Vuelve a leer del dibujo los racks del conjunto antes de seguir.",
                    items.Select(i => NotWritten(write.RequestId, i, UpdateAllOutcome.Unknown, Unknown)).ToList());
            }

            var results = Enumerable.Range(0, total).Select(i => Written(write.RequestId, items[i], applied[i])).ToList();
            return Answer(write, true, WrittenSummary(results), results);
        }

        private static bool Cancelled(IUpdateAllUnitPort port)
        {
            try
            {
                return port.CancelRequested();
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static void SafeRollback(IUpdateAllUnitPort port)
        {
            try
            {
                port.Rollback();
            }
            catch (Exception)
            {
                // The transaction is discarded when the host disposes it anyway.
            }
        }

        // The drafts that needed the blocks fail with the cause; the others are not applied.
        private static UpdateAllWritten ImportFailed(UpdateAllWrite write, UpdateAllApplied[] applied, string cause)
        {
            var items = write.Items;
            var failed = Enumerable.Range(0, items.Count).Where(i => applied[i] != null && applied[i].NeedsImports).ToList();
            return Answer(write, false,
                "No se escribió nada: " + (failed.Count == 1 ? "falló " : "fallaron ") + UpdateAllText.List(failed.Select(i => UpdateAllText.Quoted(items[i].Name)))
                + ". " + UpdateAllText.Sentence(cause),
                Enumerable.Range(0, items.Count).Select(i => failed.Contains(i)
                    ? NotWritten(write.RequestId, items[i], UpdateAllOutcome.Failed, cause)
                    : NotWritten(write.RequestId, items[i], UpdateAllOutcome.NotAppliedOtherFailed, UpdateAllText.OtherFailed)).ToList());
        }

        private static UpdateAllWritten CancelledAnswer(UpdateAllWrite write)
            => Answer(write, false, "No se escribió nada: se canceló «Actualizar todo» antes de confirmar.",
                write.Items.Select(i => NotWritten(write.RequestId, i, UpdateAllOutcome.Cancelled, "Cancelado antes de confirmar: no se escribió nada.")).ToList());

        private static UpdateAllWritten Answer(UpdateAllWrite write, bool committed, string message, IReadOnlyList<UpdateAllItemResult> items)
            => new UpdateAllWritten(write.RequestId, committed, message, items);

        // A draft not written: the answer its own «Actualizar» gives when it writes nothing, with the cause.
        private static UpdateAllItemResult NotWritten(long requestId, UpdateAllItem item, UpdateAllOutcome outcome, string cause)
        {
            var text = UpdateAllText.CauseText(outcome, cause);
            return item.System == UpdateAllSystem.Variables
                ? new UpdateAllItemResult(item.System, item.RackId, item.Name, outcome, cause, VariablesWritten: new VariablesDoorWritten(requestId, false, text))
                : new UpdateAllItemResult(item.System, item.RackId, item.Name, outcome, cause, new RackDesignWritten(requestId, item.RackId, false, text));
        }

        private static UpdateAllItemResult Written(long requestId, UpdateAllItem item, UpdateAllApplied applied)
        {
            var warned = !string.IsNullOrWhiteSpace(applied.Warning);
            var outcome = warned ? UpdateAllOutcome.AppliedWithWarnings : UpdateAllOutcome.Applied;
            var cause = warned ? applied.Warning.Trim() : null;
            if (item.System == UpdateAllSystem.Variables)
            {
                var variables = applied.VariablesAnswer ?? new VariablesDoorWritten(requestId, true, "AutoCAD escribió el borrador de «Variables».");
                return new UpdateAllItemResult(item.System, item.RackId, item.Name, outcome, cause, VariablesWritten: variables with { Written = true });
            }

            var answer = applied.Answer ?? new RackDesignWritten(requestId, item.RackId, true, "AutoCAD escribió el borrador.");
            var message = UpdateAllText.WithinTheSet(answer.Message) + (warned ? " " + UpdateAllText.Sentence(cause) : string.Empty);
            return new UpdateAllItemResult(item.System, item.RackId, item.Name, outcome, cause, answer with { Written = true, Message = message });
        }

        private static string FailureSummary(IReadOnlyList<UpdateAllItem> items, string[] refusals)
        {
            var failed = Enumerable.Range(0, items.Count).Where(i => refusals[i] != null).ToList();
            return (failed.Count == 1 ? "no se puede escribir " : "no se pueden escribir ")
                   + UpdateAllText.List(failed.Select(i => UpdateAllText.Quoted(items[i].Name))) + ". La lista dice por qué.";
        }

        private static string WrittenSummary(IReadOnlyList<UpdateAllItemResult> results)
        {
            var count = results.Count;
            var warned = results.Count(r => r.Outcome == UpdateAllOutcome.AppliedWithWarnings);
            return "AutoCAD escribió " + UpdateAllText.Count(count, "borrador", "borradores") + " en una sola operación"
                   + (warned == 0 ? string.Empty : " (" + UpdateAllText.Count(warned, "con avisos", "con avisos") + ")")
                   + ". Un «U» en AutoCAD lo deshace todo.";
        }
    }

    /// <summary>The words of «Actualizar todo» in the window, shared by the rules, the command and the window.</summary>
    public static class UpdateAllText
    {
        public const string NothingToWrite = "No hay borradores con cambios sin escribir en este dibujo.";

        public const string OtherFailed = "No aplicado: falló otro rack del conjunto.";

        /// <summary>«Actualizar todo (N)».</summary>
        public static string Label(int count) => "Actualizar todo (" + count.ToString(CultureInfo.InvariantCulture) + ")";

        public static string Quoted(string name) => "«" + (string.IsNullOrWhiteSpace(name) ? "sin nombre" : name.Trim()) + "»";

        public static string List(IEnumerable<string> names)
        {
            var all = names.ToList();
            if (all.Count <= 1) return all.FirstOrDefault() ?? string.Empty;
            return string.Join(", ", all.Take(all.Count - 1)) + " y " + all[all.Count - 1];
        }

        public static string Count(int count, string one, string many) =>
            count.ToString(CultureInfo.InvariantCulture) + " " + (count == 1 ? one : many);

        /// <summary>A cause as a sentence: trimmed and ending with a full stop.</summary>
        public static string Sentence(string text)
        {
            var trimmed = (text ?? string.Empty).Trim();
            if (trimmed.Length == 0) return trimmed;
            var ends = trimmed.EndsWith(".", StringComparison.Ordinal) || trimmed.EndsWith("!", StringComparison.Ordinal)
                       || trimmed.EndsWith("?", StringComparison.Ordinal) || trimmed.EndsWith(".»", StringComparison.Ordinal);
            return ends ? trimmed : trimmed + ".";
        }

        /// <summary>What the editor of a draft that was not written says, with its cause.</summary>
        public static string CauseText(UpdateAllOutcome outcome, string cause)
        {
            var sentence = Sentence(cause);
            return outcome switch
            {
                UpdateAllOutcome.Failed => "«Actualizar todo» no escribió nada: este borrador no se pudo escribir. " + sentence + " El borrador sigue sin escribir.",
                UpdateAllOutcome.NotAppliedOtherFailed => sentence + " El borrador sigue sin escribir.",
                UpdateAllOutcome.Cancelled => sentence + " El borrador sigue sin escribir.",
                UpdateAllOutcome.Unknown => sentence,
                _ => sentence,
            };
        }

        /// <summary>The answer of a rack written inside «Actualizar todo»: its own answer, and that the one «U» undoes the whole set.</summary>
        public static string WithinTheSet(string message)
            => "«Actualizar todo»: " + Sentence(string.IsNullOrWhiteSpace(message) ? "AutoCAD escribió el borrador" : message);
    }
}
