using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Persistence;
using RackCad.Application.ProjectVariables;
using RackCad.Application.RackFrames;
using RackCad.Application.StructuralSections;
using RackCad.Application.StructuralSections.Geometry;
using RackCad.Application.Systems.Cantilever;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.PushBack;
using RackCad.Application.Systems.Selective;
using RackCad.Application.Systems.Shared;
using RackCad.Application.Views.Insertion;
using RackCad.Application.Views.Placement;
using RackCad.Application.Views.Policy;
using RackCad.Application.Views.Preparation;
using RackCad.Application.Views.Redraw;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems.Cantilever;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;
using RackCad.Domain.Systems.Shared;
using RackCad.Plugin.Drawing;

namespace RackCad.Plugin.Views
{
    /// <summary>
    /// The facts of ONE snapshot of the drawing. Once it exists the sessions never touch AutoCAD again: they decide from
    /// the scan, the catalog and the project-variable register that were read together, before any point.
    /// </summary>
    internal sealed class RackProjectionDrawingFacts
    {
        private readonly Func<StructuralSectionCatalog> sections;
        private StructuralSectionCatalog loaded;
        private bool hasLoaded;

        internal RackProjectionDrawingFacts(
            RackSiblingDrawingScan scan,
            RackCatalog catalog,
            ProjectVariablesReadResult variables,
            Func<StructuralSectionCatalog> sections)
        {
            Scan = scan;
            Catalog = catalog;
            Variables = variables;
            this.sections = sections;
        }

        internal RackSiblingDrawingScan Scan { get; }
        internal RackCatalog Catalog { get; }
        internal ProjectVariablesReadResult Variables { get; }

        /// <summary>The structural-section catalogue, loaded on first use, or null when it is not available.</summary>
        internal StructuralSectionCatalog Sections()
        {
            if (!hasLoaded)
            {
                loaded = sections();
                hasLoaded = true;
            }

            return loaded;
        }

        internal StructuralSectionGeometryFactory Geometry()
        {
            var catalogue = Sections();
            return catalogue == null ? null : new StructuralSectionGeometryFactory(catalogue);
        }
    }

    /// <summary>Raised inside the AUTH-09 delegate so the session can tell WHY a rack did not resolve.</summary>
    internal sealed class RackProjectionResolveException : Exception
    {
        internal RackProjectionResolveException(RackProjectionResolveFailure failure, string message)
            : base(message)
        {
            Failure = failure;
        }

        internal RackProjectionResolveFailure Failure { get; }
    }

    /// <summary>
    /// One RackId as the pure ID19 plan sees it: the Foundation authorities (AUTH-13 comparator, AUTH-09 resolve,
    /// AUTH-10 preparation, AUTH-05 frame, AUTH-12 V2 requirements and query) composed once per rack, each called at most
    /// once per input. It decides nothing: every verdict is the pure plan's.
    /// </summary>
    internal abstract class RackProjectionKindSession
    {
        private readonly RackProjectionDrawingFacts facts;
        private readonly string rackId;
        private readonly string kind;
        private readonly RackSystemKind systemKind;
        private RackSiblingMembershipSnapshot membership;

        protected RackProjectionKindSession(
            string rackId, string kind, RackSystemKind systemKind, RackProjectionDrawingFacts facts)
        {
            this.rackId = rackId;
            this.kind = kind;
            this.systemKind = systemKind;
            this.facts = facts;
        }

        protected string RackId => rackId;
        protected string Kind => kind;
        protected RackSystemKind SystemKind => systemKind;
        protected RackProjectionDrawingFacts Facts => facts;

        protected RackSiblingMembershipSnapshot Membership
            => membership ?? (membership = facts.Scan.MembershipFor(rackId));

        internal abstract RackProjectionAuthoredState Authored();
        internal abstract RackProjectionResolveOutcome Resolve();
        internal abstract RackViewFrameResult Frame(RackViewAddress address);
        internal abstract RackProjectionTargetPreparation Prepare(RackViewAddress target);

        /// <summary>Custom properties gate over exactly the mutable members already classified by membership.</summary>
        internal RackProjectionPropertiesState Properties()
        {
            var gate = RackSiblingCustomPropertiesGate.Evaluate(Membership, facts.Scan.Properties);
            if (gate.Accepted) return RackProjectionPropertiesState.Equivalent;
            return gate.Diagnostic != null
                && gate.Diagnostic.StartsWith("CUSTOM_PROPERTIES_DIVERGENT", StringComparison.Ordinal)
                    ? RackProjectionPropertiesState.Divergent
                    : RackProjectionPropertiesState.UnreadableAttributable;
        }

        /// <summary>
        /// What the RACKEDITAR preflight would reject in this rack's sweep, xref dependents included: the remedy Actualizar
        /// would fail on them. It only asks the existing authorities (I-11 inner sources, envelope kind and id, descriptor).
        /// </summary>
        internal RackProjectionEditPreflight EditPreflight()
        {
            var members = Membership.Members
                .Where(member => member.Kind == RackSiblingMembershipKind.Redraw
                    || member.Kind == RackSiblingMembershipKind.ReadOnly
                    || member.Kind == RackSiblingMembershipKind.Erase)
                .ToList();
            var blocks = new List<(Autodesk.AutoCAD.DatabaseServices.ObjectId BlockId, RackEmbedDocument Embed)>();

            foreach (var member in members)
            {
                var key = member.Fact.DefinitionKey;
                if (!facts.Scan.Envelopes.TryGetValue(key, out var envelope)
                    || !string.Equals(envelope.Kind, kind, StringComparison.OrdinalIgnoreCase)
                    || !string.Equals(envelope.Id, rackId, StringComparison.OrdinalIgnoreCase))
                {
                    return new RackProjectionEditPreflight(true, member.Fact.IsXrefDependent, "KIND_OR_IDENTITY_MISMATCH");
                }

                blocks.Add((facts.Scan.Definitions[key], envelope));
            }

            if (blocks.Count == 0)
            {
                return RackProjectionEditPreflight.Accepted;
            }

            RackProject initiating = null;
            try
            {
                initiating = new RackProjectStore().Deserialize(blocks[0].Embed.Design);
            }
            catch (Exception)
            {
                // A benign failure of the initiating design is exactly what PreflightInnerSources tolerates.
            }

            var inner = RackCommandSupport.PreflightInnerSources(blocks, systemKind, initiating);
            if (inner.Aborted)
            {
                return new RackProjectionEditPreflight(true, members.Any(m => m.Fact.IsXrefDependent), "INNER_SOURCE");
            }

            if (systemKind == RackSystemKind.PushBack || systemKind == RackSystemKind.Cantilever)
            {
                foreach (var member in members)
                {
                    var decoded = RackCommandSupport.DecodeView(facts.Scan.Envelopes[member.Fact.DefinitionKey]);
                    if (!decoded.HasAddress || decoded.SystemKind != systemKind)
                    {
                        return new RackProjectionEditPreflight(true, member.Fact.IsXrefDependent, "VIEW_DESCRIPTOR");
                    }
                }
            }

            return RackProjectionEditPreflight.Accepted;
        }

        /// <summary>The names the views of this rack carry, in stable order (the rack's own views, never another rack's).</summary>
        protected IEnumerable<string> SiblingNames()
            => Membership.MutableMembers
                .OrderBy(member => member.Fact.DefinitionKey, StringComparer.Ordinal)
                .Select(member => facts.Scan.Envelopes.TryGetValue(member.Fact.DefinitionKey, out var envelope) ? envelope.Name : null)
                .ToList();

        protected RackEmbedDocument Representative(IReadOnlyList<string> selectedDefinitionKeys)
        {
            foreach (var key in selectedDefinitionKeys.OrderBy(k => k, StringComparer.Ordinal))
            {
                if (facts.Scan.Envelopes.TryGetValue(key, out var envelope)) return envelope;
            }

            return null;
        }

        protected static RackProjectionAuthoredState Map(RackAuthoredComparisonOutcome outcome)
        {
            switch (outcome)
            {
                case RackAuthoredComparisonOutcome.Single: return RackProjectionAuthoredState.Single;
                case RackAuthoredComparisonOutcome.Divergent: return RackProjectionAuthoredState.Divergent;
                default: return RackProjectionAuthoredState.Unreadable;
            }
        }

        protected RackAuthoredInput SiblingInput()
        {
            var siblings = new List<RackAuthoredSibling>();
            var store = new RackEmbedStore();
            foreach (var member in Membership.AuthoredGateMembers)
            {
                if (!facts.Scan.Envelopes.TryGetValue(member.Fact.DefinitionKey, out var envelope)) continue;
                siblings.Add(new RackAuthoredSibling(
                    member.Fact.DefinitionKey, envelope.Kind, store.Serialize(envelope), envelope.Design));
            }

            var complete = Membership.Members.All(member => member.Kind != RackSiblingMembershipKind.BlockingUnreadable)
                && siblings.Count == Membership.AuthoredGateMembers.Count;
            return new RackAuthoredInput(rackId, siblings, complete, "RackSiblingScan/CaptureDrawing");
        }

        protected SelectiveAuthoredComparisonInput SelectiveInput()
        {
            var entries = Membership.AuthoredGateMembers.Select(member =>
                ProjectVariableScanProjection.Project(
                    member.Fact.DefinitionKey,
                    facts.Scan.Envelopes.TryGetValue(member.Fact.DefinitionKey, out var envelope) ? envelope : null,
                    member.Fact.LayoutReferenceCount)).ToList();
            return new SelectiveAuthoredComparisonInput(rackId, entries);
        }

        // ------------------------------------------------------------------ factories

        internal static RackProjectionKindSession Create(
            string kind, string rackId, IReadOnlyList<string> selectedDefinitionKeys, RackProjectionDrawingFacts facts)
        {
            if (string.Equals(kind, RackEmbedDocument.KindSelective, StringComparison.OrdinalIgnoreCase))
                return Selective(rackId, selectedDefinitionKeys, facts);
            if (string.Equals(kind, RackEmbedDocument.KindDynamic, StringComparison.OrdinalIgnoreCase))
                return Dynamic(rackId, selectedDefinitionKeys, facts);
            if (string.Equals(kind, RackEmbedDocument.KindPushBack, StringComparison.OrdinalIgnoreCase))
                return PushBack(rackId, selectedDefinitionKeys, facts);
            if (string.Equals(kind, RackEmbedDocument.KindCantilever, StringComparison.OrdinalIgnoreCase))
                return Cantilever(rackId, selectedDefinitionKeys, facts);
            if (string.Equals(kind, RackEmbedDocument.KindCabecera, StringComparison.OrdinalIgnoreCase))
                return Cabecera(rackId, selectedDefinitionKeys, facts);
            return new UnexposedKindSession(rackId, kind, facts);
        }

        private static RackProjectionKindSession Selective(
            string rackId, IReadOnlyList<string> selected, RackProjectionDrawingFacts facts)
        {
            var catalog = facts.Catalog;
            string rackName = null;
            return new RackProjectionKindSession<SelectiveAuthoredComparisonInput, SelectivePalletDesignDocument,
                SelectiveRackSystem, HeaderRunPlan>(
                rackId, RackEmbedDocument.KindSelective, RackSystemKind.SelectiveRack, selected, facts,
                session => session.SelectiveInput(),
                RackAuthoredComparatorPorts.Selective(),
                resolvePort: RackResolvePorts.Selective<SelectivePalletDesignDocument, SelectiveRackSystem>(authored =>
                {
                    var effective = new SelectiveEffectiveDesignResolver().ResolveAccredited(authored, facts.Variables);
                    if (!effective.IsSuccess)
                        throw new RackProjectionResolveException(
                            RackProjectionResolveFailure.BrokenReference, effective.Error);
                    return new SelectiveGeometryResolver().Resolve(effective.Design, catalog);
                }),
                outputBlocking: system => null,
                availability: system => new SelectiveViewAvailabilityFacts(
                    SelectiveDepthLayout.Count(system),
                    new SelectiveLateralBuilder().Cortes(system, catalog).Select(cut => cut.PostIndex)),
                candidates: (system, availability) => SelectiveCandidates(system, catalog, (SelectiveViewAvailabilityFacts)availability),
                frame: (system, address) => SelectiveViewFrameAdapter.Resolve(system, catalog, address),
                prepare: RackViewPreparationPorts.Selective<SelectiveRackSystem, HeaderRunPlan>(
                    (system, address) => RackViewBatchProducts.SelectivePlan(system, address, catalog, rackName),
                    address => true,
                    RackBlockRequirementExtractors.HeaderRun),
                baseName: (system, address, name) => RackViewBatchProducts.SelectiveName(
                    system, address, name, string.IsNullOrWhiteSpace(name) ? null : name.Trim()),
                extract: RackHeaderPieceRequirementExtractorV2.Extract,
                setRackName: name => rackName = name);
        }

        private static RackProjectionKindSession Dynamic(
            string rackId, IReadOnlyList<string> selected, RackProjectionDrawingFacts facts)
        {
            var catalog = facts.Catalog;
            return new RackProjectionKindSession<RackAuthoredInput, DynamicRackDesign, DynamicRackSystem, HeaderRunPlan>(
                rackId, RackEmbedDocument.KindDynamic, RackSystemKind.PalletFlow, selected, facts,
                session => session.SiblingInput(),
                RackAuthoredComparatorPorts.Dynamic(),
                resolvePort: RackResolvePorts.Dynamic<DynamicRackDesign, DynamicRackSystem>(
                    design => new DynamicRackSystemResolver(catalog).Resolve(design).System),
                outputBlocking: system => null,
                availability: system => new DynamicViewAvailabilityFacts(
                    new DynamicSystemLateralBuilder().Cortes(system, catalog).Select(cut => cut.PostIndex)),
                candidates: (system, availability) => DynamicCandidates(system, catalog),
                frame: (system, address) => DynamicViewFrameAdapter.Resolve(system, catalog, address),
                prepare: RackViewPreparationPorts.Dynamic<DynamicRackSystem, HeaderRunPlan>(
                    (system, address) => RackViewBatchProducts.DynamicPlan(system, address, catalog),
                    address => true,
                    RackBlockRequirementExtractors.HeaderRun),
                baseName: (system, address, name) => RackViewBatchProducts.DynamicName(
                    system, address, name, string.IsNullOrWhiteSpace(name) ? null : name.Trim()),
                extract: RackHeaderPieceRequirementExtractorV2.Extract,
                setRackName: null);
        }

        private static RackProjectionKindSession PushBack(
            string rackId, IReadOnlyList<string> selected, RackProjectionDrawingFacts facts)
        {
            var catalog = facts.Catalog;
            return new RackProjectionKindSession<RackAuthoredInput, PushBackDesign, PushBackSystem, HeaderRunPlan>(
                rackId, RackEmbedDocument.KindPushBack, RackSystemKind.PushBack, selected, facts,
                session => session.SiblingInput(),
                RackAuthoredComparatorPorts.PushBack(),
                resolvePort: RackResolvePorts.PushBack<PushBackDesign, PushBackSystem>(
                    design => new PushBackResolver(catalog).Resolve(design)),
                outputBlocking: system => RackCad.Application.Bom.RackBomOutputGate.For(system).Reason,
                availability: system => new PushBackViewAvailabilityFacts(
                    new PushBackSystemLateralBuilder().Cortes(system, catalog).Select(cut => cut.PostIndex),
                    system.IsComposite),
                candidates: (system, availability) => PushBackCandidates(system, catalog, availability),
                frame: (system, address) => PushBackViewFrameAdapter.Resolve(system, catalog, address),
                prepare: RackViewPreparationPorts.PushBack<PushBackSystem, HeaderRunPlan>(
                    (system, address) => RackViewBatchProducts.PushBackPlan(system, address, catalog),
                    address => true,
                    RackBlockRequirementExtractors.HeaderRun),
                baseName: (system, address, name) => RackViewBatchProducts.PushBackName(
                    system, address, name, string.IsNullOrWhiteSpace(name) ? null : name.Trim()),
                extract: RackHeaderPieceRequirementExtractorV2.Extract,
                setRackName: null);
        }

        private static RackProjectionKindSession Cabecera(
            string rackId, IReadOnlyList<string> selected, RackProjectionDrawingFacts facts)
        {
            var catalog = facts.Catalog;
            return new RackProjectionKindSession<RackAuthoredInput, RackFrameConfiguration, RackFrameConfiguration,
                HeaderRunPlan>(
                rackId, RackEmbedDocument.KindCabecera, RackSystemKind.Selective, selected, facts,
                session => session.SiblingInput(),
                RackAuthoredComparatorPorts.Cabecera(),
                resolvePort: RackResolvePorts.Cabecera<RackFrameConfiguration, RackFrameConfiguration>(design => design),
                outputBlocking: system => null,
                availability: system => new CabeceraViewAvailabilityFacts(),
                candidates: (system, availability) => new[]
                {
                    RackViewAddress.Whole(DimensionViewKind.Lateral),
                    RackViewAddress.Whole(DimensionViewKind.Planta),
                },
                frame: (system, address) => CabeceraViewFrameAdapter.Resolve(system, address),
                prepare: RackViewPreparationPorts.Cabecera<RackFrameConfiguration, HeaderRunPlan>(
                    (system, address) => RackViewBatchProducts.HeaderPlan(system, address, catalog),
                    address => true,
                    RackBlockRequirementExtractors.HeaderRun),
                baseName: (system, address, name) => RackViewBatchProducts.HeaderName(catalog, system, address, name),
                extract: RackHeaderPieceRequirementExtractorV2.Extract,
                setRackName: null);
        }

        private static RackProjectionKindSession Cantilever(
            string rackId, IReadOnlyList<string> selected, RackProjectionDrawingFacts facts)
        {
            CantileverLineDesign design = null;
            return new RackProjectionKindSession<RackAuthoredInput, CantileverLineDesign, CantileverLineAssembly,
                CantileverViewPlan>(
                rackId, RackEmbedDocument.KindCantilever, RackSystemKind.Cantilever, selected, facts,
                session => session.SiblingInput(),
                RackAuthoredComparatorPorts.Cantilever(),
                resolvePort: RackResolvePorts.Cantilever<CantileverLineDesign, CantileverLineAssembly>(authored =>
                {
                    design = authored;
                    var catalogue = facts.Sections();
                    if (catalogue == null)
                        throw new RackProjectionResolveException(
                            RackProjectionResolveFailure.DependencyUnavailable, "catalogo de secciones no disponible");
                    return new CantileverLineEditorAssembler(catalogue).Build(authored).Line;
                }),
                outputBlocking: line => line.IsBlocked ? "CANTILEVER_LINE_BLOCKED" : null,
                availability: line => new CantileverViewAvailabilityFacts(line.Stations.Count),
                candidates: (line, availability) => CantileverCandidates(line.Stations.Count),
                frame: (line, address) => CantileverViewFrameAdapter.Resolve(
                    line, RackViewBatchProducts.CantileverPlan(line, design, facts.Geometry(), address), address),
                prepare: RackViewPreparationPorts.Cantilever<CantileverLineAssembly, CantileverViewPlan>(
                    (line, address) => RackViewBatchProducts.CantileverPlan(line, design, facts.Geometry(), address),
                    address => true,
                    RackBlockRequirementExtractors.Cantilever),
                baseName: (line, address, name) => RackViewProductNames.Cantilever(address, name),
                extract: null,
                setRackName: null);
        }

        // ------------------------------------------------------------------ candidate addresses per family

        private static IEnumerable<RackViewAddress> SelectiveCandidates(
            SelectiveRackSystem system, RackCatalog catalog, SelectiveViewAvailabilityFacts availability)
        {
            yield return RackViewAddress.Whole(DimensionViewKind.Planta);
            for (var fondo = 0; fondo < availability.FondoCount; fondo++) yield return RackViewAddress.Fondo(fondo);
            foreach (var cut in new SelectiveLateralBuilder().Cortes(system, catalog))
                yield return RackViewAddress.Post(cut.PostIndex);
        }

        private static IEnumerable<RackViewAddress> DynamicCandidates(DynamicRackSystem system, RackCatalog catalog)
        {
            yield return RackViewAddress.Whole(DimensionViewKind.Planta);
            yield return RackViewAddress.FlowEnd(RackFlowEnd.Exit);
            yield return RackViewAddress.FlowEnd(RackFlowEnd.Entrance);
            foreach (var cut in new DynamicSystemLateralBuilder().Cortes(system, catalog))
                yield return RackViewAddress.Post(cut.PostIndex);
        }

        private static IEnumerable<RackViewAddress> PushBackCandidates(
            PushBackSystem system, RackCatalog catalog, RackViewAvailabilityFacts availability)
        {
            yield return RackViewAddress.Whole(DimensionViewKind.Planta);
            foreach (var end in new[] { RackPushBackEnd.EntradaSalida, RackPushBackEnd.Posterior })
                foreach (var side in new[] { RackPushBackSide.A, RackPushBackSide.B })
                    yield return RackViewAddress.PushBackCut(end, side);
            foreach (var cut in new PushBackSystemLateralBuilder().Cortes(system, catalog))
                yield return RackViewAddress.Post(cut.PostIndex);
        }

        private static IEnumerable<RackViewAddress> CantileverCandidates(int stations)
        {
            yield return RackViewAddress.Whole(DimensionViewKind.Planta);
            yield return RackViewAddress.Whole(DimensionViewKind.Frontal);
            for (var station = 0; station < stations; station++) yield return RackViewAddress.Station(station);
        }
    }

    /// <summary>A kind the registry knows but ID19 does not project (Flow Bed): it resolves to nothing and the plan rejects it.</summary>
    internal sealed class UnexposedKindSession : RackProjectionKindSession
    {
        internal UnexposedKindSession(string rackId, string kind, RackProjectionDrawingFacts facts)
            : base(rackId, kind, string.Equals(kind, RackEmbedDocument.KindCama, StringComparison.OrdinalIgnoreCase)
                ? RackSystemKind.Cama
                : RackSystemKind.SelectiveRack, facts)
        {
        }

        internal override RackProjectionAuthoredState Authored() => RackProjectionAuthoredState.Single;

        internal override RackProjectionResolveOutcome Resolve()
        {
            var known = string.Equals(Kind, RackEmbedDocument.KindCama, StringComparison.OrdinalIgnoreCase);
            return known
                ? new RackProjectionResolveOutcome(
                    RackProjectionResolveState.Resolved, new CamaViewAvailabilityFacts(), null)
                : new RackProjectionResolveOutcome(
                    RackProjectionResolveState.Failed,
                    new SelectiveViewAvailabilityFacts(0, null),
                    null,
                    "UNKNOWN_KIND",
                    RackProjectionResolveFailure.UnknownKind);
        }

        internal override RackViewFrameResult Frame(RackViewAddress address)
            => RackViewFrameResult.Unavailable(RackViewFrameFailure.UnsupportedAddress);

        internal override RackProjectionTargetPreparation Prepare(RackViewAddress target)
            => new RackProjectionTargetPreparation(false, PieceRequirementExtractionOutcome.Extracted, null, "NOT_EXPOSED");
    }

    internal sealed class RackProjectionKindSession<TInput, TAuthored, TResolved, TPayload> : RackProjectionKindSession
    {
        private readonly Func<RackProjectionKindSession, TInput> input;
        private readonly IRackAuthoredComparatorPort<TInput, TAuthored> comparator;
        private readonly IRackResolvePort<TAuthored, TResolved> resolvePort;
        private readonly Func<TResolved, string> outputBlocking;
        private readonly Func<TResolved, RackViewAvailabilityFacts> availability;
        private readonly Func<TResolved, RackViewAvailabilityFacts, IEnumerable<RackViewAddress>> candidates;
        private readonly Func<TResolved, RackViewAddress, RackViewFrameResult> frame;
        private readonly IRackViewPreparationPort<TResolved, TPayload> prepare;
        private readonly Func<TResolved, RackViewAddress, string, string> baseName;
        private readonly Func<TPayload, RackViewAddress, PieceRequirementExtractionResult> extract;
        private readonly Action<string> setRackName;
        private readonly RackEmbedDocument source;
        private readonly string rawSourceName;

        private TInput comparisonInput;
        private RackAuthoredComparisonResult<TAuthored> comparison;
        private bool compared;
        private RackResolveResult<TResolved> resolved;
        private bool hasResolved;
        private RackProjectionResolveFailure resolveFailure;
        private RackViewAvailabilityFacts availabilityFacts;

        internal RackProjectionKindSession(
            string rackId,
            string kind,
            RackSystemKind systemKind,
            IReadOnlyList<string> selectedDefinitionKeys,
            RackProjectionDrawingFacts facts,
            Func<RackProjectionKindSession, TInput> input,
            IRackAuthoredComparatorPort<TInput, TAuthored> comparator,
            IRackResolvePort<TAuthored, TResolved> resolvePort,
            Func<TResolved, string> outputBlocking,
            Func<TResolved, RackViewAvailabilityFacts> availability,
            Func<TResolved, RackViewAvailabilityFacts, IEnumerable<RackViewAddress>> candidates,
            Func<TResolved, RackViewAddress, RackViewFrameResult> frame,
            IRackViewPreparationPort<TResolved, TPayload> prepare,
            Func<TResolved, RackViewAddress, string, string> baseName,
            Func<TPayload, RackViewAddress, PieceRequirementExtractionResult> extract,
            Action<string> setRackName)
            : base(rackId, kind, systemKind, facts)
        {
            this.input = input;
            this.comparator = comparator;
            this.resolvePort = resolvePort;
            this.outputBlocking = outputBlocking;
            this.availability = availability;
            this.candidates = candidates;
            this.frame = frame;
            this.prepare = prepare;
            this.baseName = baseName;
            this.extract = extract;
            this.setRackName = setRackName;
            var representative = Representative(selectedDefinitionKeys);

            // The name the rack's views carry AS IS: BaseName and the plan grouping read it with their own fallback (AUTH-11).
            rawSourceName = representative?.Name;

            // The envelope is composed from a copy that carries a usable Name (AUTH-15 refuses an envelope without one); the source
            // envelope in the drawing is never touched, and a named rack keeps its name (G16 OV-ID19-01).
            source = representative == null
                ? null
                : RackProjectionEnvelopeName.WithLogicalName(representative, SiblingNames());
            if (representative != null) setRackName?.Invoke(rawSourceName);
        }

        private RackAuthoredComparisonResult<TAuthored> Comparison()
        {
            if (!compared)
            {
                comparisonInput = input(this);
                comparison = comparator.Compare(comparisonInput);
                compared = true;
            }

            return comparison;
        }

        internal override RackProjectionAuthoredState Authored() => Map(Comparison().Outcome);

        private RackResolveResult<TResolved> ResolveOnce()
        {
            if (!hasResolved)
            {
                var authored = Comparison();
                resolved = authored.Outcome == RackAuthoredComparisonOutcome.Single
                    ? ResolveThroughPort(authored.Authored)
                    : RackResolveResult<TResolved>.Unsupported(Kind, "authored gate not satisfied");
                hasResolved = true;
            }

            return resolved;
        }

        private RackResolveResult<TResolved> ResolveThroughPort(TAuthored authored)
        {
            try
            {
                var result = resolvePort.Resolve(authored);
                if (!result.IsSuccess)
                {
                    resolveFailure = RackProjectionResolveFailure.Unreadable;
                }

                return result;
            }
            catch (RackProjectionResolveException ex)
            {
                resolveFailure = ex.Failure;
                return RackResolveResult<TResolved>.Unsupported(Kind, ex.Message);
            }
        }

        internal override RackProjectionResolveOutcome Resolve()
        {
            var result = ResolveOnce();
            if (!result.IsSuccess)
            {
                var failure = result.Failure == RackResolveFailure.UnsupportedKind
                        && resolveFailure == RackProjectionResolveFailure.None
                    ? RackProjectionResolveFailure.UnknownKind
                    : resolveFailure == RackProjectionResolveFailure.None
                        ? RackProjectionResolveFailure.Unreadable
                        : resolveFailure;
                return new RackProjectionResolveOutcome(
                    RackProjectionResolveState.Failed, Empty(), null, result.Code ?? result.Diagnostic, failure);
            }

            var system = result.Resolved;
            availabilityFacts = availability(system);
            var reason = outputBlocking(system);
            var addresses = candidates(system, availabilityFacts)
                .Where(address => RackViewAvailability.Evaluate(
                    RackViewBatchProducts.Decoded(SystemKind, address), availabilityFacts).Status
                        == RackViewAvailabilityStatus.Available)
                .ToList();

            return new RackProjectionResolveOutcome(
                string.IsNullOrWhiteSpace(reason)
                    ? RackProjectionResolveState.Resolved
                    : RackProjectionResolveState.ResolvedWithOutputBlocking,
                availabilityFacts,
                addresses,
                reason);
        }

        private RackViewAvailabilityFacts Empty()
        {
            switch (SystemKind)
            {
                case RackSystemKind.PalletFlow: return new DynamicViewAvailabilityFacts(null);
                case RackSystemKind.PushBack: return new PushBackViewAvailabilityFacts(null, false);
                case RackSystemKind.Cantilever: return new CantileverViewAvailabilityFacts(0);
                case RackSystemKind.Selective: return new CabeceraViewAvailabilityFacts();
                default: return new SelectiveViewAvailabilityFacts(0, null);
            }
        }

        internal override RackViewFrameResult Frame(RackViewAddress address)
        {
            var result = ResolveOnce();
            if (!result.IsSuccess) return RackViewFrameResult.Unavailable(RackViewFrameFailure.MissingSource);
            try
            {
                return frame(result.Resolved, address);
            }
            catch (Exception)
            {
                return RackViewFrameResult.Unavailable(RackViewFrameFailure.VariantUnavailable);
            }
        }

        internal override RackProjectionTargetPreparation Prepare(RackViewAddress target)
        {
            var result = ResolveOnce();
            if (!result.IsSuccess || source == null || availabilityFacts == null)
            {
                return Unavailable("NOT_RESOLVED");
            }

            var request = new RackProductViewRequest(
                RackViewProductOperation.GroupProjection, SystemKind, target, availabilityFacts);
            var accepted = RackProductIntentAcceptance.Accept(
                new ExistingRackViewIntent<TInput>(new ExistingRackContext<TInput>(source, comparisonInput), request));
            if (!accepted.IsAccepted) return Unavailable(accepted.CauseCode);

            var preparer = new RackProductPreparer<TAuthored, TResolved, TPayload>(
                new ResolvedOnce<TAuthored, TResolved>(Kind, result),
                prepare,
                (system, address) => frame(system, address),
                (system, address) => baseName(system, address, rawSourceName));
            var product = preparer.PrepareExisting(accepted.Intent, new PrecomputedComparator(Kind, Comparison()));
            if (!product.IsSuccess) return Unavailable(product.CauseCode ?? product.Failure.ToString());

            var payload = product.Product.Prepared.Payload;
            var headerPlan = payload as HeaderRunPlan;
            var cantileverPlan = payload as CantileverViewPlan;
            if (cantileverPlan != null && cantileverPlan.IsEmpty)
            {
                // The same refusal the Cantilever editors make: a view with nothing to draw is not a view.
                return Unavailable("CANTILEVER_VIEW_EMPTY");
            }

            var view = new RackProjectionPreparedView(
                RackId,
                SystemKind,
                product.Product.Prepared.Address,
                product.Product.Prepared.BaseName,
                product.Product.Envelope,
                headerPlan,
                cantileverPlan);

            if (extract == null)
            {
                return new RackProjectionTargetPreparation(
                    true, PieceRequirementExtractionOutcome.Extracted, null, null, view);
            }

            var extraction = extract(payload, product.Product.Prepared.Address);
            if (!extraction.IsSuccess)
            {
                return new RackProjectionTargetPreparation(
                    true, PieceRequirementExtractionOutcome.UnknownSourceRole, null, null, view);
            }

            var observed = LibraryPieceAvailabilityFlowV2.Observe(
                extraction.Requirements, allowImport: false, new AutoCadExternalLibraryBlockQuery(), null);
            return new RackProjectionTargetPreparation(
                true, PieceRequirementExtractionOutcome.Extracted, observed.Facts, null, view);
        }

        private static RackProjectionTargetPreparation Unavailable(string code)
            => new RackProjectionTargetPreparation(false, PieceRequirementExtractionOutcome.Extracted, null, code);

        /// <summary>The AUTH-09 result already obtained for this rack: the preparer never resolves a second time.</summary>
        private sealed class ResolvedOnce<TAuth, TRes> : IRackResolvePort<TAuth, TRes>
        {
            private readonly RackResolveResult<TRes> result;

            internal ResolvedOnce(string kind, RackResolveResult<TRes> result)
            {
                Kind = kind;
                this.result = result;
            }

            public string Kind { get; }
            public RackResolveResult<TRes> Resolve(TAuth input) => result;
        }

        /// <summary>The AUTH-13 comparison already obtained for this rack: the preparer never compares a second time.</summary>
        private sealed class PrecomputedComparator : IRackAuthoredComparatorPort<TInput, TAuthored>
        {
            private readonly RackAuthoredComparisonResult<TAuthored> result;

            internal PrecomputedComparator(string kind, RackAuthoredComparisonResult<TAuthored> result)
            {
                Kind = kind;
                this.result = result;
            }

            public string Kind { get; }
            public RackAuthoredComparisonResult<TAuthored> Compare(TInput input) => result;
        }
    }
}
