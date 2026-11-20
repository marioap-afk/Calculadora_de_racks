#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.PushBack.Draft;
using RackCad.Application.Systems.Shared;
using RackCad.Application.Views.Redraw;
using RackCad.Application.Workspace.Drafts;
using RackCad.Domain.Systems.Shared;

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

        /// <summary>True when the update can go on to take the slot.</summary>
        public bool Proceeds { get; }

        /// <summary>The kind that stopped the update; only meaningful when <see cref="Proceeds"/> is false.</summary>
        public PushBackPilotPrepareKind Kind { get; }

        /// <summary>The constant of the run that says why the update stopped; null when it proceeds.</summary>
        public string? Reason { get; }

        internal static PushBackPilotPrecheck Go() => new(true, PushBackPilotPrepareKind.Invalid, null);

        internal static PushBackPilotPrecheck Stop(PushBackPilotPrepareKind kind, string reason) => new(false, kind, reason);
    }

    /// <summary>
    /// I-64 F4-T4B (D-T4B-02) — the pure and TOTAL run of the update of the bounded Push Back pilot: it decides, classifies and
    /// compares, and it never touches the drawing by itself: everything that does goes through <see cref="IPushBackPilotUpdatePort"/>.
    /// It never throws: every failure is one of the outcomes of D-13, by the frontier where it happened.
    ///
    /// The order is the one of the package: the check before the slot, then, under the slot, the read of PREPARE (the origin is chosen
    /// here, in pure code, by the exact text of the origin of the draft), the survivors and the locked layers, the import, the start of
    /// MUTATE, the re-read that is the authority of MUTATE, one write per member, the seal and POST. POST is only reached once the seal
    /// returned, and its interpretation (the names that the views must carry, the warnings and the comparison) is made here.
    ///
    /// I-64 F4B PB-T3b (D-PBT3-10) — the same run serves the two models of the draft: the one of the pilot and the full one. The views that the
    /// update leaves obsolete are taken away between the start of MUTATE and its seal, by the capacity that the scope of MUTATE may implement.
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

        /// <summary>I-01: the three texts of the gate of the host, as constants of the run. Their wording does not change.</summary>
        public const string UpdateNotRunReason = "La actualización no llegó a ejecutarse.";

        public const string NoActiveDocumentReason = "No hay un documento activo para actualizar.";

        public const string ModalOpenReason = "Hay una ventana de RackCad abierta: ciérrala antes de actualizar.";

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

        public const string EraseUnsupportedReason = "El dibujo no admite borrar vistas en esta actualización.";

        public const string PostErasedStillPresentPrefix = "POST_ERASED_STILL_PRESENT: ";

        private const string CompletenessEvidence = "RackSiblingScan/CaptureDrawing";

        /// <summary>
        /// The check before the slot (step 1): the pure PREPARE without a re-read and without a catalog. It only stops for the three kinds
        /// that need neither; anything else goes on.
        /// </summary>
        public static PushBackPilotPrecheck Precheck(DraftSession<PushBackPilotDraftState> draft)
        {
            if (draft == null) throw new ArgumentNullException(nameof(draft));

            var result = PushBackPilotUpdatePlanner.Prepare(PushBackPilotPrepareInput.From(draft), null!, null!);
            switch (result.Kind)
            {
                case PushBackPilotPrepareKind.NotApplicable: return PushBackPilotPrecheck.Stop(result.Kind, NotApplicableReason);
                case PushBackPilotPrepareKind.PendingText: return PushBackPilotPrecheck.Stop(result.Kind, PendingTextReason);
                case PushBackPilotPrepareKind.NothingToApply: return PushBackPilotPrecheck.Stop(result.Kind, NothingToApplyReason);
                default: return PushBackPilotPrecheck.Go();
            }
        }

        /// <summary>The same check for a draft of the FULL model (D-PBT3-10): the same three kinds, over its own snapshot.</summary>
        public static PushBackPilotPrecheck Precheck(DraftSession<PushBackFullDraftState> draft)
        {
            if (draft == null) throw new ArgumentNullException(nameof(draft));

            var result = PushBackPilotUpdatePlanner.PrepareFull(PushBackFullPrepareInput.From(draft), null!, null!);
            switch (result.Kind)
            {
                case PushBackPilotPrepareKind.NotApplicable: return PushBackPilotPrecheck.Stop(result.Kind, NotApplicableReason);
                case PushBackPilotPrepareKind.PendingText: return PushBackPilotPrecheck.Stop(result.Kind, PendingTextReason);
                case PushBackPilotPrepareKind.NothingToApply: return PushBackPilotPrecheck.Stop(result.Kind, NothingToApplyReason);
                default: return PushBackPilotPrecheck.Go();
            }
        }

        /// <summary>
        /// The whole update under the slot. TOTAL: it never throws. The outcome is the one that the draft must be closed with.
        /// </summary>
        public static DraftApplyOutcome<PushBackPilotDraftState> Execute(
            DraftSession<PushBackPilotDraftState> draft,
            PushBackPilotUpdateRequest request,
            IPushBackPilotUpdatePort port)
        {
            if (draft == null || request == null || port == null)
                return DraftApplyOutcome<PushBackPilotDraftState>.NotAppliedFailure(MissingArgumentsReason);

            // M-01: the state of the run lives outside the try, so what escapes is classified with what the run already did.
            PushBackPilotPrepareInput? input = null;
            var execution = new Execution<PushBackPilotDraftState>(
                draft,
                request,
                port,
                snapshotted =>
                {
                    input = PushBackPilotPrepareInput.From(snapshotted);
                    return input.InitialDraftState.Origin;
                },
                (_, scan, catalog) => PushBackPilotUpdatePlanner.Prepare(input!, PushBackPilotIntentReader.Read(scan, catalog), catalog),
                (scan, catalog, plan) =>
                {
                    var read = PushBackPilotIntentReader.Read(scan, catalog);
                    return (read.Read, ComparePost(plan, read));
                });
            try
            {
                return execution.Run();
            }
            catch (Exception ex)
            {
                return execution.OutcomeOfAnEscape(ex);
            }
        }

        /// <summary>
        /// The whole update under the slot for a draft of the FULL model (D-PBT3-10). TOTAL: it never throws. It is the same run as the one of the
        /// pilot, with the plan, the re-read and the comparison of the full model.
        /// </summary>
        public static DraftApplyOutcome<PushBackFullDraftState> ExecuteFull(
            DraftSession<PushBackFullDraftState> draft,
            PushBackPilotUpdateRequest request,
            IPushBackPilotUpdatePort port)
        {
            if (draft == null || request == null || port == null)
                return DraftApplyOutcome<PushBackFullDraftState>.NotAppliedFailure(MissingArgumentsReason);

            // M-01: the state of the run lives outside the try, so what escapes is classified with what the run already did.
            PushBackFullPrepareInput? input = null;
            var execution = new Execution<PushBackFullDraftState>(
                draft,
                request,
                port,
                snapshotted =>
                {
                    input = PushBackFullPrepareInput.From(snapshotted);
                    return input.InitialDraftState.Origin;
                },
                (_, scan, catalog) => PushBackPilotUpdatePlanner.PrepareFull(input!, PushBackFullDraftReader.Read(scan, catalog), catalog),
                (scan, catalog, plan) =>
                {
                    var read = PushBackFullDraftReader.Read(scan, catalog);
                    return (read.Read, ComparePostFull(plan, read));
                });
            try
            {
                return execution.Run();
            }
            catch (Exception ex)
            {
                return execution.OutcomeOfAnEscape(ex);
            }
        }

        /// <summary>
        /// The comparison of POST, without normalizing anything (D-T4B-05): the same keys as the plan, the envelope of each key equal
        /// ordinally, and the same scale. Null when everything matches; otherwise the cause.
        /// </summary>
        public static string? ComparePost(PushBackPilotUpdatePlan plan, PushBackPilotIntentResult? postRead)
        {
            if (plan == null) throw new ArgumentNullException(nameof(plan));
            if (postRead == null) return PostRereadMissingReason;

            return ComparePostCore(plan, postRead.Read.Admission, postRead.StaleBase, postRead.Read.InitialState?.Inputs.Annotations.AnnotationScale);
        }

        /// <summary>The comparison of POST for the FULL model (D-PBT3-10): the erased views are gone, and the rest as for the pilot.</summary>
        public static string? ComparePostFull(PushBackPilotUpdatePlan plan, PushBackFullIntentResult? postRead)
        {
            if (plan == null) throw new ArgumentNullException(nameof(plan));
            if (postRead == null) return PostRereadMissingReason;

            return ComparePostCore(plan, postRead.Read.Admission, postRead.StaleBase, postRead.Read.InitialState?.Editable.AnnotationScale);
        }

        // The comparison of the two models: the read that is admitted, the views that were erased and are gone, the same keys, the same envelopes
        // and the same scale (the one of the state that the read gave).
        private static string? ComparePostCore(PushBackPilotUpdatePlan plan, DraftAdmission admission, DraftStaleBase? staleBase, double? scale)
        {
            if (!admission.IsAdmitted) return PostNotAdmittedPrefix + (admission.Diagnostic ?? "sin diagnóstico");

            var found = staleBase!.Entries.Values
                .Where(entry => entry.Membership == DraftMembership.Mutable)
                .ToDictionary(entry => entry.Key, entry => entry.EnvelopeText, StringComparer.Ordinal);
            foreach (var erased in plan.PostExpectation.ErasedKeys)
            {
                if (found.ContainsKey(erased)) return PostErasedStillPresentPrefix + erased;
            }

            var expected = plan.PostExpectation.Envelopes;
            if (found.Count != expected.Count) return PostKeysDifferReason;

            foreach (var item in expected)
            {
                if (!found.TryGetValue(item.Key, out var text)) return PostKeysDifferReason;
                if (!string.Equals(text, item.Text, StringComparison.Ordinal)) return PostEnvelopeDiffersPrefix + item.Key;
            }

            return scale.HasValue && scale.Value.Equals(plan.PostExpectation.AnnotationScale) ? null : PostScaleDiffersReason;
        }

        /// <summary>
        /// The name that each view of the plan must carry, by key, with the rule of the classic editor: the base is the trimmed name of
        /// the outgoing envelope, and a view without a base has no desired name. A plan view uses the plan view name, a lateral one the
        /// name of its post, and a frontal one the name of its cut (with the side when the rack is composite).
        /// </summary>
        public static IReadOnlyDictionary<string, string> DesiredNamesOf(PushBackPilotUpdatePlan plan, RackCatalog catalog)
        {
            if (plan == null) throw new ArgumentNullException(nameof(plan));

            var names = new Dictionary<string, string>(StringComparer.Ordinal);
            var store = new RackEmbedStore();
            bool? composite = null;
            foreach (var member in plan.Members)
            {
                var embed = store.Deserialize(member.EnvelopeText);
                if (embed == null || string.IsNullOrWhiteSpace(embed.Name)) continue;

                var baseName = embed.Name.Trim();
                var address = RackViewCodec.Decode(embed.Kind, embed.View, embed.Section).Address;
                string? desired;
                switch (address.Kind)
                {
                    case DimensionViewKind.Planta:
                        desired = RackViewBaseName.LinkedPlanta(baseName);
                        break;
                    case DimensionViewKind.Lateral:
                        desired = RackViewBaseName.LinkedLateral(baseName, address.Variant.Index);
                        break;
                    default:
                        composite ??= PushBackPilotSystemRebuild.IsComposite(PushBackPilotSystemRebuild.Build(member.DesignText, catalog));
                        desired = RackViewBaseName.LinkedPushBackCut(baseName, address.Variant.PushBackEnd, address.Variant.PushBackSide, composite.Value);
                        break;
                }

                if (desired != null) names[member.Key] = desired;
            }

            return names;
        }

        // ------------------------------------------------------------------ the state of ONE run

        private sealed class Execution<TState>
        {
            private readonly DraftSession<TState> _draft;
            private readonly PushBackPilotUpdateRequest _request;
            private readonly IPushBackPilotUpdatePort _port;
            private readonly Func<DraftSession<TState>, string> _originOf;
            private readonly Func<DraftSession<TState>, PushBackPilotIntentScan, RackCatalog, PushBackPilotPrepareResult> _prepare;
            private readonly Func<PushBackPilotIntentScan, RackCatalog?, PushBackPilotUpdatePlan, (DraftIntentRead<TState> Read, string? Cause)> _postRead;
            private readonly List<string> _warnings = new();

            private string? _origin;
            private PushBackPilotUpdatePlan? _plan;
            private RackCatalog? _catalog;
            private bool _writeStarted;
            private bool _sealReturned;
            private Exception? _mutationDisposeFailure;

            public Execution(
                DraftSession<TState> draft,
                PushBackPilotUpdateRequest request,
                IPushBackPilotUpdatePort port,
                Func<DraftSession<TState>, string> originOf,
                Func<DraftSession<TState>, PushBackPilotIntentScan, RackCatalog, PushBackPilotPrepareResult> prepare,
                Func<PushBackPilotIntentScan, RackCatalog?, PushBackPilotUpdatePlan, (DraftIntentRead<TState> Read, string? Cause)> postRead)
            {
                _draft = draft;
                _request = request;
                _port = port;
                _originOf = originOf;
                _prepare = prepare;
                _postRead = postRead;
            }

            // The two scopes are released in finally blocks of their own, the one of MUTATE inside the one of the run, and what a release
            // throws never covers the outcome that was already decided (D-T4B-02 (6)).
            public DraftApplyOutcome<TState> Run()
            {
                IPushBackPilotUpdateScope? scope = null;
                DraftApplyOutcome<TState>? outcome;
                Exception? scopeDisposeFailure = null;
                try
                {
                    scope = _port.Begin(_request);
                    if (scope == null)
                    {
                        outcome = NotAppliedFailure(NoScopeReason);
                    }
                    else
                    {
                        outcome = Prepare(scope) ?? Mutate(scope) ?? Post(scope);
                    }
                }
                catch (Exception ex)
                {
                    outcome = _sealReturned
                        ? PostFailed(PostFailurePrefix + ex.Message)
                        : NotAppliedFailure(RunFailurePrefix + ex.Message);
                }
                finally
                {
                    if (scope != null)
                    {
                        try
                        {
                            scope.Dispose();
                        }
                        catch (Exception ex)
                        {
                            scopeDisposeFailure = ex;
                        }
                    }
                }

                return WithScopeDisposeFailure(outcome, scopeDisposeFailure);
            }

            // What an exception that escaped from the run means, by what the run had already done: after the seal the write is done, after
            // a write began its persistence is not known, and only before both it is a plain failure (M-01).
            public DraftApplyOutcome<TState> OutcomeOfAnEscape(Exception ex)
            {
                if (_sealReturned) return PostFailed(PostFailurePrefix + ex.Message);
                if (_writeStarted) return DraftApplyOutcome<TState>.CommitThrew(WriteFailurePrefix + ex.Message);
                return NotAppliedFailure(RunFailurePrefix + ex.Message);
            }

            // ------------------------------------------------------------------ PREPARE

            // Returns the decision when the update cannot go on, and null when it can.
            private DraftApplyOutcome<TState>? Prepare(IPushBackPilotUpdateScope scope)
            {
                _origin = _originOf(_draft);
                var values = scope.ReadForPrepare();
                if (values == null) return NotAppliedFailure(NoReadReason);
                if (!values.DocumentMatches) return NotAppliedPreflight(DocumentMismatchReason);

                _catalog = values.Catalog;
                var origin = ChooseOrigin(values, _origin, out var originKey);
                if (origin != null) return origin;

                var prepared = _prepare(_draft, ScanOf(values, originKey!), values.Catalog!);
                switch (prepared.Kind)
                {
                    case PushBackPilotPrepareKind.Ready:
                        _plan = prepared.Plan;
                        break;
                    case PushBackPilotPrepareKind.Conflict:
                        return DraftApplyOutcome<TState>.NotAppliedConflict(prepared.Differences);
                    case PushBackPilotPrepareKind.NotAdmitted:
                        return DraftApplyOutcome<TState>.NotAppliedAdmission(prepared.Reason ?? PushBackPilotIntentReader.MaterializeDiagnostic);
                    case PushBackPilotPrepareKind.NothingToApply:
                        return NotAppliedPreflight(NothingToApplyReason);
                    case PushBackPilotPrepareKind.PendingText:
                        return NotAppliedPreflight(PendingTextReason);
                    case PushBackPilotPrepareKind.NotApplicable:
                        return NotAppliedPreflight(NotApplicableReason);
                    default:
                        return NotAppliedPreflight(prepared.Reason ?? PushBackPilotUpdatePlanner.MissingInputReason);
                }

                var plan = _plan!;
                var refusal = SurvivorsAndLayers(values, plan);
                if (refusal != null) return refusal;

                var import = scope.PrepareUnits(plan);
                if (import == null || !import.Ok) return NotAppliedPreflight(import?.Cause ?? ImportFailedReason);
                return null;
            }

            // ------------------------------------------------------------------ MUTATE

            // Returns the decision of MUTATE, or null when the seal returned and POST must follow.
            private DraftApplyOutcome<TState>? Mutate(IPushBackPilotUpdateScope scope)
            {
                var start = scope.BeginMutation();
                if (start == null) return NotAppliedFailure(NoMutationScopeReason);
                if (start.RejectionCode != null) return NotAppliedPreflight(start.RejectionCode);

                var mutation = start.Scope;
                if (mutation == null) return NotAppliedFailure(NoMutationScopeReason);

                DraftApplyOutcome<TState>? decided;
                var sealing = false;
                try
                {
                    decided = Write(mutation, () => sealing = true);
                }
                catch (Exception ex)
                {
                    decided = sealing
                        ? DraftApplyOutcome<TState>.CommitThrew(SealFailedPrefix + ex.Message)
                        : NotAppliedFailure(WriteFailurePrefix + ex.Message);
                }
                finally
                {
                    try
                    {
                        mutation.Dispose();
                    }
                    catch (Exception ex)
                    {
                        _mutationDisposeFailure = ex;
                    }
                }

                if (_sealReturned)
                {
                    // The seal returned: the write is done, so a release that fails is only a warning and POST goes on (D-13).
                    if (_mutationDisposeFailure != null) _warnings.Add(DisposeWarningPrefix + _mutationDisposeFailure.Message);
                    return null;
                }

                return WithAbortFailure(decided ?? NotAppliedFailure(NoMutationScopeReason));
            }

            // The re-read is the authority of MUTATE: nothing that PREPARE found authorizes a write until it agrees.
            private DraftApplyOutcome<TState>? Write(IPushBackPilotMutationScope mutation, Action markSealing)
            {
                var plan = _plan!;

                // The capacity first: a plan that erases views cannot be written by a scope that cannot erase them, whatever the drawing says.
                var eraser = mutation as IPushBackPilotViewEraser;
                if (plan.Erasures.Count > 0 && eraser == null) return NotAppliedPreflight(EraseUnsupportedReason);

                var values = mutation.Reread();
                if (values == null) return NotAppliedFailure(NoReadReason);
                if (!values.DocumentMatches) return NotAppliedPreflight(DocumentMismatchReason);

                // The origin is chosen again, in pure code, and it must be the key of the plan.
                var origin = ChooseOrigin(values, _origin!, out var originKey);
                var sameOrigin = origin == null && string.Equals(originKey, plan.OriginKey, StringComparison.Ordinal);

                var reread = PushBackPilotIntentReader.BuildStaleBase(ScanOf(values, plan.OriginKey));
                var comparison = DraftStaleBase.Create(plan.ExpectedBase, plan.ExpectedVariableRegistryText).CompareWith(reread);
                if (!sameOrigin || !comparison.IsMatch)
                    return DraftApplyOutcome<TState>.NotAppliedConflict(comparison.Differences);

                // The destination: the keys of the plan, the redrawn and the erased, are the mutable ones that were re-read, and every prepared block is the re-read one.
                var mutable = reread.Entries.Values.Where(e => e.Membership == DraftMembership.Mutable).Select(e => e.Key).ToList();
                var planned = plan.Members.Select(m => m.Key).Concat(plan.Erasures).ToList();
                var matches = values.PreparedBlockMatches;
                if (mutable.Count != planned.Count
                    || mutable.Any(key => !planned.Contains(key, StringComparer.Ordinal))
                    || matches == null
                    || planned.Any(key => !matches.TryGetValue(key, out var same) || !same))
                    return NotAppliedPreflight(DestinationChangedReason);

                var refusal = SurvivorsAndLayers(values, plan);
                if (refusal != null) return refusal;

                foreach (var member in plan.Members)
                {
                    _writeStarted = true;
                    var unit = mutation.WriteUnit(member.Key, plan.Envelope(member.Key)!);
                    if (unit == null) continue;
                    foreach (var piece in unit.MissingPieces) _warnings.Add(MissingPiecePrefix + member.Key + ": " + piece);
                }

                // The views that the edition leaves obsolete are taken away after the redrawing, in ordinal order, inside the same MUTATE.
                foreach (var key in plan.Erasures)
                {
                    _writeStarted = true;
                    eraser!.EraseUnit(key);
                }

                markSealing();
                mutation.Seal();
                _sealReturned = true;
                return null;
            }

            // What a failed release of the scope of MUTATE does to a decision that was taken without a seal (D-T4B-02 (4) and (6)).
            private DraftApplyOutcome<TState> WithAbortFailure(DraftApplyOutcome<TState> decided)
            {
                var failure = _mutationDisposeFailure;
                if (failure == null) return decided;

                var abort = AbortFailedPrefix + failure.Message;
                switch (decided.Kind)
                {
                    case DraftApplyOutcomeKind.NotAppliedFailure:
                        // A unit started and the abort failed too: nothing is known about what was written, and no rollback is claimed.
                        return _writeStarted
                            ? DraftApplyOutcome<TState>.CommitThrew(abort + "; " + decided.Cause)
                            : NotAppliedFailure(decided.Cause + "; " + abort);
                    case DraftApplyOutcomeKind.CommitThrew:
                        return DraftApplyOutcome<TState>.CommitThrew(decided.Cause + "; " + abort);
                    case DraftApplyOutcomeKind.NotAppliedPreflight:
                        return NotAppliedPreflight(decided.Cause + "; " + abort);
                    default:
                        // A conflict has no cause to add it to: the failure of the release is dropped, and the tests say so.
                        return decided;
                }
            }

            // ------------------------------------------------------------------ POST

            private DraftApplyOutcome<TState> Post(IPushBackPilotUpdateScope scope)
            {
                try
                {
                    var plan = _plan!;
                    var desired = DesiredNamesOrNone(plan);
                    var values = scope.Post(new PushBackPilotPostRequest(desired));
                    if (values == null) return PostFailed(PostMissingReason);
                    if (values.ForeignScopeOpenAtPost) return PostFailed(ForeignScopeOpenCause);

                    foreach (var pair in desired)
                    {
                        var expected = BlockNaming.SanitizeBlockName(pair.Value);
                        if (!values.NamesByKey.TryGetValue(pair.Key, out var actual) || !string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase))
                            _warnings.Add(NameDiffersPrefix + pair.Key + " no es «" + expected + "».");
                    }

                    if (values.RegenFailure != null) _warnings.Add(RegenFailedPrefix + values.RegenFailure);
                    if (values.Scan == null) return PostFailed(PostRereadMissingReason);

                    // The origin of POST is the key of the plan that is read again: the origin of the draft, or, when the update erased it, the first
                    // view that was redrawn. The definition does not change, even if its text is the new one now.
                    var (read, cause) = _postRead(ScanOf(values.Scan, plan.PostOriginKey), _catalog, plan);
                    if (cause != null) return PostFailed(cause);

                    return _warnings.Count == 0
                        ? DraftApplyOutcome<TState>.Applied(read)
                        : DraftApplyOutcome<TState>.AppliedWithWarnings(read, _warnings);
                }
                catch (Exception ex)
                {
                    return PostFailed(PostFailurePrefix + ex.Message);
                }
            }

            // A failure of the rule of the names must not stop POST: the tidy-up and the re-read still run, and the failure is a warning.
            private IReadOnlyDictionary<string, string> DesiredNamesOrNone(PushBackPilotUpdatePlan plan)
            {
                try
                {
                    return DesiredNamesOf(plan, _catalog!);
                }
                catch (Exception ex)
                {
                    _warnings.Add(DesiredNamesFailedPrefix + ex.Message);
                    return new Dictionary<string, string>(StringComparer.Ordinal);
                }
            }

            // ------------------------------------------------------------------ pure helpers

            private PushBackPilotIntentScan ScanOf(PushBackPilotScanValues values, string originKey) =>
                new(
                    _request.RackId,
                    originKey,
                    values.Facts,
                    values.RawEnvelopes.ToDictionary(pair => pair.Key, pair => (string?)pair.Value, StringComparer.Ordinal),
                    CompletenessEvidence);

            // The origin is the ONLY mutable member whose raw text is, byte for byte, the origin of the draft. Zero or several is a
            // preflight refusal with its own reason, and never the misleading "origin missing" of the reader.
            private DraftApplyOutcome<TState>? ChooseOrigin(PushBackPilotScanValues values, string origin, out string? key)
            {
                key = null;
                var staleBase = PushBackPilotIntentReader.BuildStaleBase(ScanOf(values, string.Empty));
                var matches = staleBase.Entries.Values
                    .Where(entry => entry.Membership == DraftMembership.Mutable && string.Equals(entry.EnvelopeText, origin, StringComparison.Ordinal))
                    .Select(entry => entry.Key)
                    .ToList();
                if (matches.Count == 0) return NotAppliedPreflight(PushBackPilotUpdatePlanner.NoOriginReason);
                if (matches.Count > 1) return NotAppliedPreflight(PushBackPilotUpdatePlanner.SeveralOriginsReason);

                key = matches[0];
                return null;
            }

            // C-F4-05 (at least one survivor, among the views that stay) and the locked layers, those of the views that are redrawn and those that
            // are erased, before the import and again in MUTATE.
            private DraftApplyOutcome<TState>? SurvivorsAndLayers(PushBackPilotScanValues values, PushBackPilotUpdatePlan plan)
            {
                var snapshot = RackSiblingMembership.Classify(values.Facts, _request.RackId, null, false);
                if (!snapshot.Members.Any(member => member.IsSurvivor && !plan.Erasures.Contains(member.Fact.DefinitionKey, StringComparer.Ordinal)))
                    return NotAppliedPreflight(NoSurvivorsReason);

                var locked = values.LockedLayerKeys
                    .Where(key => plan.Members.Any(member => string.Equals(member.Key, key, StringComparison.Ordinal))
                                  || plan.Erasures.Contains(key, StringComparer.Ordinal))
                    .OrderBy(key => key, StringComparer.Ordinal)
                    .ToList();
                return locked.Count == 0 ? null : NotAppliedPreflight(LockedLayerPrefix + string.Join(", ", locked));
            }

            // What a failed release of the scope of the run does: it never changes the classification that was already decided.
            private DraftApplyOutcome<TState> WithScopeDisposeFailure(
                DraftApplyOutcome<TState>? outcome, Exception? failure)
            {
                var decided = outcome ?? NotAppliedFailure(NoScopeReason);
                if (failure == null) return decided;

                var text = DisposeWarningPrefix + failure.Message;
                switch (decided.Kind)
                {
                    case DraftApplyOutcomeKind.NotAppliedAdmission:
                        return DraftApplyOutcome<TState>.NotAppliedAdmission(decided.Cause + "; " + text);
                    case DraftApplyOutcomeKind.NotAppliedPreflight:
                        return NotAppliedPreflight(decided.Cause + "; " + text);
                    case DraftApplyOutcomeKind.NotAppliedFailure:
                        return NotAppliedFailure(decided.Cause + "; " + text);
                    case DraftApplyOutcomeKind.CommitThrew:
                        return DraftApplyOutcome<TState>.CommitThrew(decided.Cause + "; " + text);
                    case DraftApplyOutcomeKind.Applied:
                    case DraftApplyOutcomeKind.AppliedWithWarnings:
                        // The only exception to "nothing changes": after POST an Applied becomes AppliedWithWarnings, and both end Clean.
                        return DraftApplyOutcome<TState>.AppliedWithWarnings(decided.PostRead!, decided.Warnings.Append(text).ToArray());
                    case DraftApplyOutcomeKind.AppliedPostRereadFailed:
                        return DraftApplyOutcome<TState>.AppliedPostRereadFailed(decided.Cause!, decided.Warnings.Append(text).ToArray());
                    default:
                        // A conflict carries no cause: the failure of the release is dropped (declared limitation).
                        return decided;
                }
            }

            private DraftApplyOutcome<TState> PostFailed(string cause) =>
                DraftApplyOutcome<TState>.AppliedPostRereadFailed(cause, _warnings.ToArray());

            private static DraftApplyOutcome<TState> NotAppliedFailure(string cause) =>
                DraftApplyOutcome<TState>.NotAppliedFailure(cause);

            private static DraftApplyOutcome<TState> NotAppliedPreflight(string cause) =>
                DraftApplyOutcome<TState>.NotAppliedPreflight(cause);
        }
    }
}
