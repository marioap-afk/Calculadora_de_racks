using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Shared;
using RackCad.Domain.RackFrames;

namespace RackCad.Application.Systems.Selective
{
    /// <summary>
    /// I-53 (ID6 REUSE + ID7 BATCH DISTRIBUTION) — PREPARE of the Selectivo: the one phase of a cabecera batch that can fail,
    /// and the one that decides everything (contract §3.3-§3.9, §6.2-§6.7; ADR-0037).
    /// <para>
    /// It checks that the resolution it reads is current (RR-01), resolves the source address against it and captures the
    /// source's CURRENT value, crosses <see cref="SelectiveEditorState.TargetFondos"/> with the requested posts, omits what the
    /// topology does not have, validates the applicable destinations, and materializes and normalizes one copy per
    /// destination: the depth of ITS fondo through <see cref="SelectiveCabeceraAuthority.ImposeFondoDepth"/>, the peralte of
    /// ITS post through <see cref="SelectivePostGeometry"/>, and the height of the recipe, reviewed at every destination by
    /// <see cref="SelectiveCabeceraHeightReview"/>. Rejections follow the precedence of §3.7.
    /// </para>
    /// <para>
    /// It is PURE with respect to everything observable: the editor state, the stored cabeceras, the resolved system and the
    /// recompute. It only reads them, and the only objects it modifies are the private copies it materialized. The source is
    /// captured, never assigned, and the snapshot dies here: what survives is the prepared plan and its copies.
    /// </para>
    /// <para>
    /// There is no generic executor behind it: the shared core only states the result, and this is the Selectivo's own
    /// implementation of the protocol over its own state.
    /// </para>
    /// </summary>
    public static class SelectiveHeaderBatchPlanner
    {
        /// <summary>Inches under which an edited peralte equals the run peralte (the editor's historical tolerance).</summary>
        private const double PeralteTolerance = 1e-6;

        public static SelectiveHeaderBatchPreparation Prepare(
            SelectiveEditorState state, SelectiveHeaderResolution resolution, SelectiveHeaderBatchRequest request)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var operation = request.Operation;

            // RR-01: the gesture ends BEFORE PREPARE on a resolution that is not the current one — no plan, no outcome.
            if (resolution?.System == null)
            {
                return SelectiveHeaderBatchPreparation.Ended(state, operation, SelectiveHeaderPreconditionFailure.NoResolvedSystem);
            }

            if (!resolution.IsCurrent)
            {
                return SelectiveHeaderBatchPreparation.Ended(state, operation, SelectiveHeaderPreconditionFailure.ResolutionNotCurrent);
            }

            var system = resolution.System;
            var topology = SelectiveTopology.From(state);

            // 1-2. The source: the CURRENT value its address designates, or the configurator result of an EDIT.
            SelectiveHeaderAddress? source = null;
            RackFrameConfiguration sourceConfiguration;
            if (operation == SelectiveHeaderBatchOperation.Distribute)
            {
                var address = request.SourceAddress.Value;
                if (!state.PostExistsIn(address.FondoIndex, address.PostIndex))
                {
                    return Reject(state, operation, HeaderRejectionCode.SourceNotFound);
                }

                sourceConfiguration = SelectiveCabeceraAuthority.UsableCustomAt(system, address.FondoIndex, address.PostIndex);
                if (sourceConfiguration == null)
                {
                    return Reject(state, operation, HeaderRejectionCode.SourceUnusable);
                }

                source = address;
            }
            else
            {
                sourceConfiguration = request.EditResult;
            }

            if (!(HeaderConfigurationSnapshot.TryCapture(sourceConfiguration) is HeaderConfigurationCapture.Captured captured))
            {
                return Reject(state, operation, HeaderRejectionCode.SourceUnusable);
            }

            // 3-4. Targets = TargetFondos (I-43, as is) x posts, in (fondo, post) order; omissions keep their position.
            var fondos = state.TargetFondos.Fondos;
            var posts = request.TargetPosts.Distinct().OrderBy(post => post).ToList();
            if (fondos.Count == 0 || posts.Count == 0)
            {
                return Reject(state, operation, HeaderRejectionCode.NoTargets);
            }

            if (posts[0] < 0)
            {
                return Reject(state, operation, HeaderRejectionCode.MalformedTarget);
            }

            var resolved = new List<SelectiveHeaderAddress>();
            var targets = new List<SelectiveHeaderAddress>();
            var omitted = new List<HeaderOmission<SelectiveHeaderAddress>>();
            foreach (var fondo in fondos)
            {
                foreach (var post in posts)
                {
                    var address = new SelectiveHeaderAddress(fondo, post);
                    resolved.Add(address);
                    if (!topology.HasFondo(fondo) || !state.PostExistsIn(fondo, post))
                    {
                        // Omitted, never created and never clamped onto a neighbour.
                        omitted.Add(new HeaderOmission<SelectiveHeaderAddress>(address, HeaderOmissionReason.AbsentInScope));
                    }
                    else if (source.HasValue && source.Value == address)
                    {
                        omitted.Add(new HeaderOmission<SelectiveHeaderAddress>(address, HeaderOmissionReason.IsSource));
                    }
                    else
                    {
                        targets.Add(address);
                    }
                }
            }

            if (targets.Count == 0)
            {
                return Reject(state, operation, HeaderRejectionCode.NoApplicableTargets);
            }

            // 5. Validate every applicable destination BEFORE copying: a single invalid one rejects the whole batch.
            // ImposeFondoDepth silently ignores a depth <= 0, which is exactly why the check happens here first.
            var depths = new double[targets.Count];
            for (var i = 0; i < targets.Count; i++)
            {
                depths[i] = state.CabeceraDepthOfFondo(targets[i].FondoIndex);
                if (!(double.IsFinite(depths[i]) && depths[i] > 0.0))
                {
                    return Reject(state, operation, HeaderRejectionCode.DestinationInvalid);
                }
            }

            // 6. One NEW materialization per destination: never the source, never another destination's copy.
            var copies = new List<RackFrameConfiguration>(targets.Count);
            foreach (var _ in targets)
            {
                copies.Add(captured.Snapshot.Materialize());
            }

            // 7. Normalize each copy. The height travels with the recipe (ADR-0032 D9/D10); the depth is its fondo's and the
            // peralte its post's. An EDIT precomputes the per-post peralte it will write, with the editor's rule: the edited
            // value, or 0 (inherit) when it is not positive or equals the run peralte.
            var editPost = -1;
            var editPostPeralte = 0.0;
            if (operation == SelectiveHeaderBatchOperation.Edit)
            {
                editPost = posts[0];
                var edited = copies[0].PostPeralte;
                editPostPeralte = edited > 0.0 && Math.Abs(edited - system.PostPeralte) > PeralteTolerance ? edited : 0.0;
            }

            for (var i = 0; i < copies.Count; i++)
            {
                SelectiveCabeceraAuthority.ImposeFondoDepth(copies[i], depths[i]);
                copies[i].PostPeralte = operation == SelectiveHeaderBatchOperation.Distribute
                    ? SelectivePostGeometry.PostPeralteAt(system, targets[i].PostIndex)
                    : SelectivePostGeometry.PostPeralteFor(system, editPostPeralte);
            }

            // 8. The normalized copies must still be usable cabeceras.
            foreach (var copy in copies)
            {
                if (!RackDesignValidation.IsUsableHeader(copy))
                {
                    return Reject(state, operation, HeaderRejectionCode.DestinationInvalid);
                }
            }

            // 9. The height of the recipe, reviewed at every destination by the existing review.
            var warnings = SelectiveCabeceraHeightReview.OfDestinations(system, targets, copies[0].Height)
                .Select(finding => new HeaderBatchWarning<SelectiveHeaderAddress>(
                    finding.Address,
                    finding.Issue == SelectiveCabeceraHeightIssue.Severe ? HeaderWarningSeverity.Severe : HeaderWarningSeverity.Informative,
                    finding.Describe()))
                .ToList();

            // 10. The plan, with the signature of what this PREPARE read.
            var plan = new HeaderBatchPlan<SelectiveHeaderAddress>.Prepared(targets, omitted, warnings, Signature(state, resolution, resolved));
            return SelectiveHeaderBatchPreparation.Prepared(
                state, operation, plan, new ReadOnlyCollection<RackFrameConfiguration>(copies), editPost, editPostPeralte);
        }

        /// <summary>
        /// The signature of a batch (contract §3.9, RR-01 rule 5): everything whose change would make the prepared copies or
        /// their destinations wrong — the generation of the resolved system, the fondos and frentes of the topology, the
        /// master grid, the run peralte, the cabecera depth of every fondo involved and the effective peralte of every post
        /// involved. Only values, never live objects. MUTATE recomputes it over the same addresses before the first write.
        /// </summary>
        internal static HeaderBatchSignature Signature(
            SelectiveEditorState state, SelectiveHeaderResolution resolution, IEnumerable<SelectiveHeaderAddress> addresses)
        {
            var system = resolution.System;
            var topology = SelectiveTopology.From(state);
            var involved = addresses.ToList();

            var components = new List<string>
            {
                "generacion=" + resolution.Generation.ToString(CultureInfo.InvariantCulture),
                "fondos=" + topology.FondoCount.ToString(CultureInfo.InvariantCulture),
            };

            for (var fondo = 0; fondo < topology.FondoCount; fondo++)
            {
                components.Add("frentes[" + Text(fondo) + "]=" + Text(topology.FrontCount(fondo)));
            }

            components.Add("postes=" + Text(state.MaxFrenteCount()));
            components.Add("peralte-tramo=" + Text(system.PostPeralte));

            foreach (var fondo in involved.Select(address => address.FondoIndex).Distinct().OrderBy(index => index))
            {
                components.Add("profundidad[" + Text(fondo) + "]=" + Text(state.CabeceraDepthOfFondo(fondo)));
            }

            foreach (var post in involved.Select(address => address.PostIndex).Distinct().OrderBy(index => index))
            {
                components.Add("peralte[" + Text(post) + "]=" + Text(SelectivePostGeometry.PostPeralteAt(system, post)));
            }

            return new HeaderBatchSignature(components);
        }

        private static SelectiveHeaderBatchPreparation Reject(
            SelectiveEditorState state, SelectiveHeaderBatchOperation operation, HeaderRejectionCode code)
            => SelectiveHeaderBatchPreparation.Rejected(state, operation, code);

        private static string Text(int value) => value.ToString(CultureInfo.InvariantCulture);

        private static string Text(double value) => value.ToString("R", CultureInfo.InvariantCulture);
    }
}
