#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
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

        // RED skeleton (INV-F4T4C-20): the three texts of I-01 exist with an inert value.
        public const string UpdateNotRunReason = "";

        public const string NoActiveDocumentReason = "";

        public const string ModalOpenReason = "";

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

        /// <summary>
        /// The whole update under the slot. TOTAL: it never throws. The outcome is the one that the draft must be closed with.
        /// </summary>
        public static DraftApplyOutcome<PushBackPilotDraftState> Execute(
            DraftSession<PushBackPilotDraftState> draft,
            PushBackPilotUpdateRequest request,
            IPushBackPilotUpdatePort port)
        {
            try
            {
                if (draft == null || request == null || port == null)
                    return DraftApplyOutcome<PushBackPilotDraftState>.NotAppliedFailure(MissingArgumentsReason);

                return new Execution(draft, request, port).Run();
            }
            catch (Exception ex)
            {
                return DraftApplyOutcome<PushBackPilotDraftState>.NotAppliedFailure(RunFailurePrefix + ex.Message);
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
            if (!postRead.Read.IsAdmitted) return PostNotAdmittedPrefix + (postRead.Read.Admission.Diagnostic ?? "sin diagnóstico");

            var found = postRead.StaleBase.Entries.Values
                .Where(entry => entry.Membership == DraftMembership.Mutable)
                .ToDictionary(entry => entry.Key, entry => entry.EnvelopeText, StringComparer.Ordinal);
            var expected = plan.PostExpectation.Envelopes;
            if (found.Count != expected.Count) return PostKeysDifferReason;

            foreach (var item in expected)
            {
                if (!found.TryGetValue(item.Key, out var text)) return PostKeysDifferReason;
                if (!string.Equals(text, item.Text, StringComparison.Ordinal)) return PostEnvelopeDiffersPrefix + item.Key;
            }

            var scale = postRead.Read.InitialState!.Inputs.Annotations.AnnotationScale;
            return scale.Equals(plan.PostExpectation.AnnotationScale) ? null : PostScaleDiffersReason;
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

        private sealed class Execution
        {
            private readonly DraftSession<PushBackPilotDraftState> _draft;
            private readonly PushBackPilotUpdateRequest _request;
            private readonly IPushBackPilotUpdatePort _port;
            private readonly List<string> _warnings = new();

            private PushBackPilotPrepareInput? _input;
            private PushBackPilotUpdatePlan? _plan;
            private RackCatalog? _catalog;
            private bool _writeStarted;
            private bool _sealReturned;
            private Exception? _mutationDisposeFailure;

            public Execution(DraftSession<PushBackPilotDraftState> draft, PushBackPilotUpdateRequest request, IPushBackPilotUpdatePort port)
            {
                _draft = draft;
                _request = request;
                _port = port;
            }

            // The two scopes are released in finally blocks of their own, the one of MUTATE inside the one of the run, and what a release
            // throws never covers the outcome that was already decided (D-T4B-02 (6)).
            public DraftApplyOutcome<PushBackPilotDraftState> Run()
            {
                IPushBackPilotUpdateScope? scope = null;
                DraftApplyOutcome<PushBackPilotDraftState>? outcome;
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

            // ------------------------------------------------------------------ PREPARE

            // Returns the decision when the update cannot go on, and null when it can.
            private DraftApplyOutcome<PushBackPilotDraftState>? Prepare(IPushBackPilotUpdateScope scope)
            {
                _input = PushBackPilotPrepareInput.From(_draft);
                var values = scope.ReadForPrepare();
                if (values == null) return NotAppliedFailure(NoReadReason);
                if (!values.DocumentMatches) return NotAppliedPreflight(DocumentMismatchReason);

                _catalog = values.Catalog;
                var origin = ChooseOrigin(values, _input.InitialDraftState.Origin, out var originKey);
                if (origin != null) return origin;

                var pilot = PushBackPilotIntentReader.Read(ScanOf(values, originKey!), values.Catalog);
                var prepared = PushBackPilotUpdatePlanner.Prepare(_input, pilot, values.Catalog!);
                switch (prepared.Kind)
                {
                    case PushBackPilotPrepareKind.Ready:
                        _plan = prepared.Plan;
                        break;
                    case PushBackPilotPrepareKind.Conflict:
                        return DraftApplyOutcome<PushBackPilotDraftState>.NotAppliedConflict(prepared.Differences);
                    case PushBackPilotPrepareKind.NotAdmitted:
                        return DraftApplyOutcome<PushBackPilotDraftState>.NotAppliedAdmission(prepared.Reason ?? PushBackPilotIntentReader.MaterializeDiagnostic);
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
            private DraftApplyOutcome<PushBackPilotDraftState>? Mutate(IPushBackPilotUpdateScope scope)
            {
                var start = scope.BeginMutation();
                if (start == null) return NotAppliedFailure(NoMutationScopeReason);
                if (start.RejectionCode != null) return NotAppliedPreflight(start.RejectionCode);

                var mutation = start.Scope;
                if (mutation == null) return NotAppliedFailure(NoMutationScopeReason);

                DraftApplyOutcome<PushBackPilotDraftState>? decided;
                var sealing = false;
                try
                {
                    decided = Write(mutation, () => sealing = true);
                }
                catch (Exception ex)
                {
                    decided = sealing
                        ? DraftApplyOutcome<PushBackPilotDraftState>.CommitThrew(SealFailedPrefix + ex.Message)
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
            private DraftApplyOutcome<PushBackPilotDraftState>? Write(IPushBackPilotMutationScope mutation, Action markSealing)
            {
                var plan = _plan!;
                var values = mutation.Reread();
                if (values == null) return NotAppliedFailure(NoReadReason);
                if (!values.DocumentMatches) return NotAppliedPreflight(DocumentMismatchReason);

                // The origin is chosen again, in pure code, and it must be the key of the plan.
                var origin = ChooseOrigin(values, _input!.InitialDraftState.Origin, out var originKey);
                var sameOrigin = origin == null && string.Equals(originKey, plan.OriginKey, StringComparison.Ordinal);

                var reread = PushBackPilotIntentReader.BuildStaleBase(ScanOf(values, plan.OriginKey));
                var comparison = DraftStaleBase.Create(plan.ExpectedBase, plan.ExpectedVariableRegistryText).CompareWith(reread);
                if (!sameOrigin || !comparison.IsMatch)
                    return DraftApplyOutcome<PushBackPilotDraftState>.NotAppliedConflict(comparison.Differences);

                // The destination: the keys of the plan are the mutable ones that were re-read, and every prepared block is the re-read one.
                var mutable = reread.Entries.Values.Where(e => e.Membership == DraftMembership.Mutable).Select(e => e.Key).ToList();
                var planned = plan.Members.Select(m => m.Key).ToList();
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

                markSealing();
                mutation.Seal();
                _sealReturned = true;
                return null;
            }

            // What a failed release of the scope of MUTATE does to a decision that was taken without a seal (D-T4B-02 (4) and (6)).
            private DraftApplyOutcome<PushBackPilotDraftState> WithAbortFailure(DraftApplyOutcome<PushBackPilotDraftState> decided)
            {
                var failure = _mutationDisposeFailure;
                if (failure == null) return decided;

                var abort = AbortFailedPrefix + failure.Message;
                switch (decided.Kind)
                {
                    case DraftApplyOutcomeKind.NotAppliedFailure:
                        // A unit started and the abort failed too: nothing is known about what was written, and no rollback is claimed.
                        return _writeStarted
                            ? DraftApplyOutcome<PushBackPilotDraftState>.CommitThrew(abort + "; " + decided.Cause)
                            : NotAppliedFailure(decided.Cause + "; " + abort);
                    case DraftApplyOutcomeKind.CommitThrew:
                        return DraftApplyOutcome<PushBackPilotDraftState>.CommitThrew(decided.Cause + "; " + abort);
                    case DraftApplyOutcomeKind.NotAppliedPreflight:
                        return NotAppliedPreflight(decided.Cause + "; " + abort);
                    default:
                        // A conflict has no cause to add it to: the failure of the release is dropped, and the tests say so.
                        return decided;
                }
            }

            // ------------------------------------------------------------------ POST

            private DraftApplyOutcome<PushBackPilotDraftState> Post(IPushBackPilotUpdateScope scope)
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

                    // The origin of POST is the key of the plan: the definition does not change, even if its text is the new one now.
                    var read = PushBackPilotIntentReader.Read(ScanOf(values.Scan, plan.OriginKey), _catalog);
                    var cause = ComparePost(plan, read);
                    if (cause != null) return PostFailed(cause);

                    return _warnings.Count == 0
                        ? DraftApplyOutcome<PushBackPilotDraftState>.Applied(read.Read)
                        : DraftApplyOutcome<PushBackPilotDraftState>.AppliedWithWarnings(read.Read, _warnings);
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
            private DraftApplyOutcome<PushBackPilotDraftState>? ChooseOrigin(PushBackPilotScanValues values, string origin, out string? key)
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

            // C-F4-05 (at least one survivor) and the locked layers, before the import and again in MUTATE.
            private DraftApplyOutcome<PushBackPilotDraftState>? SurvivorsAndLayers(PushBackPilotScanValues values, PushBackPilotUpdatePlan plan)
            {
                var snapshot = RackSiblingMembership.Classify(values.Facts, _request.RackId, null, false);
                if (!snapshot.Members.Any(member => member.IsSurvivor)) return NotAppliedPreflight(NoSurvivorsReason);

                var locked = values.LockedLayerKeys
                    .Where(key => plan.Members.Any(member => string.Equals(member.Key, key, StringComparison.Ordinal)))
                    .OrderBy(key => key, StringComparer.Ordinal)
                    .ToList();
                return locked.Count == 0 ? null : NotAppliedPreflight(LockedLayerPrefix + string.Join(", ", locked));
            }

            // What a failed release of the scope of the run does: it never changes the classification that was already decided.
            private DraftApplyOutcome<PushBackPilotDraftState> WithScopeDisposeFailure(
                DraftApplyOutcome<PushBackPilotDraftState>? outcome, Exception? failure)
            {
                var decided = outcome ?? NotAppliedFailure(NoScopeReason);
                if (failure == null) return decided;

                var text = DisposeWarningPrefix + failure.Message;
                switch (decided.Kind)
                {
                    case DraftApplyOutcomeKind.NotAppliedAdmission:
                        return DraftApplyOutcome<PushBackPilotDraftState>.NotAppliedAdmission(decided.Cause + "; " + text);
                    case DraftApplyOutcomeKind.NotAppliedPreflight:
                        return NotAppliedPreflight(decided.Cause + "; " + text);
                    case DraftApplyOutcomeKind.NotAppliedFailure:
                        return NotAppliedFailure(decided.Cause + "; " + text);
                    case DraftApplyOutcomeKind.CommitThrew:
                        return DraftApplyOutcome<PushBackPilotDraftState>.CommitThrew(decided.Cause + "; " + text);
                    case DraftApplyOutcomeKind.Applied:
                    case DraftApplyOutcomeKind.AppliedWithWarnings:
                        // The only exception to "nothing changes": after POST an Applied becomes AppliedWithWarnings, and both end Clean.
                        return DraftApplyOutcome<PushBackPilotDraftState>.AppliedWithWarnings(decided.PostRead!, decided.Warnings.Append(text).ToArray());
                    case DraftApplyOutcomeKind.AppliedPostRereadFailed:
                        return DraftApplyOutcome<PushBackPilotDraftState>.AppliedPostRereadFailed(decided.Cause!, decided.Warnings.Append(text).ToArray());
                    default:
                        // A conflict carries no cause: the failure of the release is dropped (declared limitation).
                        return decided;
                }
            }

            private DraftApplyOutcome<PushBackPilotDraftState> PostFailed(string cause) =>
                DraftApplyOutcome<PushBackPilotDraftState>.AppliedPostRereadFailed(cause, _warnings.ToArray());

            private static DraftApplyOutcome<PushBackPilotDraftState> NotAppliedFailure(string cause) =>
                DraftApplyOutcome<PushBackPilotDraftState>.NotAppliedFailure(cause);

            private static DraftApplyOutcome<PushBackPilotDraftState> NotAppliedPreflight(string cause) =>
                DraftApplyOutcome<PushBackPilotDraftState>.NotAppliedPreflight(cause);
        }
    }
}
