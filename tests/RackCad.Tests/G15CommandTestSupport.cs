using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Drawing;
using RackCad.Application.Geometry;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Cantilever;
using RackCad.Application.Systems.Shared;
using RackCad.Application.Views.Placement;
using RackCad.Domain.Systems.Shared;

namespace RackCad.Tests
{
    /// <summary>
    /// Fakes for the ID19 command run of G15. The port records every observable event in order, so a test can prove
    /// what happened before a point, after it and inside the single write scope. Nothing here reimplements an
    /// authority: the plan is the real G14 pipeline and the write scope is the only faked side (AutoCAD).
    /// </summary>
    internal static class G15
    {
        internal static string KindOf(RackSystemKind kind)
        {
            switch (kind)
            {
                case RackSystemKind.SelectiveRack: return RackEmbedDocument.KindSelective;
                case RackSystemKind.PalletFlow: return RackEmbedDocument.KindDynamic;
                case RackSystemKind.PushBack: return RackEmbedDocument.KindPushBack;
                case RackSystemKind.Cantilever: return RackEmbedDocument.KindCantilever;
                case RackSystemKind.Selective: return RackEmbedDocument.KindCabecera;
                default: return RackEmbedDocument.KindCama;
            }
        }

        internal static RackEmbedDocument Envelope(string rackId, RackSystemKind kind, string view = RackEmbedDocument.ViewPlanta)
            => new RackEmbedDocument { Id = rackId, Kind = KindOf(kind), Name = "Rack", View = view, Section = -1, Design = "{}" };

        internal static RackProjectionPreparedView Prepared(
            string rackId, RackSystemKind kind, RackViewAddress target)
        {
            var envelope = Envelope(rackId, kind);
            return RackProjectionMaterializationFamilies.Of(kind) == RackProjectionMaterializationFamily.Cantilever
                ? new RackProjectionPreparedView(
                    rackId, kind, target, "Base " + rackId, envelope, null,
                    new CantileverViewPlan(CantileverViewKind.Planta, -1, Array.Empty<CantileverViewCurve>(),
                        Array.Empty<CantileverDiagnostic>()))
                : new RackProjectionPreparedView(
                    rackId, kind, target, "Base " + rackId, envelope,
                    new HeaderRunPlan(Array.Empty<HeaderGroup>(), Array.Empty<HeaderBlockInstance>()), null);
        }

        internal static RackViewAvailabilityFacts Facts(RackSystemKind kind)
        {
            switch (kind)
            {
                case RackSystemKind.SelectiveRack: return new SelectiveViewAvailabilityFacts(1, new[] { 0 });
                case RackSystemKind.PalletFlow: return new DynamicViewAvailabilityFacts(new[] { 0 });
                case RackSystemKind.PushBack: return new PushBackViewAvailabilityFacts(new[] { 0 }, false);
                case RackSystemKind.Cantilever: return new CantileverViewAvailabilityFacts(2);
                case RackSystemKind.Selective: return new CabeceraViewAvailabilityFacts();
                default: return new CamaViewAvailabilityFacts();
            }
        }

        /// <summary>Planta -> Planta projection (same class, Rigid) of two racks of one kind.</summary>
        internal static Scenario Rigid(
            RackSystemKind kind = RackSystemKind.SelectiveRack,
            params LibraryPieceAvailabilityFact[] pieces)
        {
            var scenario = new Scenario(kind, DimensionViewKind.Planta);
            scenario.AddRack(G14.RackA, "REF-1", 0, 0);
            scenario.AddRack(G14.RackB, "REF-2", 0, 100);
            if (pieces.Length > 0)
            {
                scenario.Pieces = pieces;
            }

            return scenario;
        }

        /// <summary>Planta -> Frontal projection (Orthographic) of two Selective racks.</summary>
        internal static Scenario Orthographic()
        {
            var scenario = new Scenario(RackSystemKind.SelectiveRack, DimensionViewKind.Frontal);
            scenario.AddRack(G14.RackA, "REF-1", 0, 0);
            scenario.AddRack(G14.RackB, "REF-2", 0, 100);
            return scenario;
        }

        internal sealed class Scenario
        {
            private readonly List<RackPhysicalReferenceSnapshot> references = new List<RackPhysicalReferenceSnapshot>();
            private readonly List<RackPhysicalDefinitionSnapshot> definitions = new List<RackPhysicalDefinitionSnapshot>();
            private readonly Dictionary<string, RackSourceTransformFactsResult> facts =
                new Dictionary<string, RackSourceTransformFactsResult>(StringComparer.Ordinal);
            private readonly List<string> rackIds = new List<string>();

            internal Scenario(RackSystemKind kind, DimensionViewKind targetKind)
            {
                Kind = kind;
                TargetKind = targetKind;
                Services = new G14.Services(kind, Facts(kind));
                Services.TargetAddresses.Add(G14.Planta);
                Services.TargetAddresses.Add(G14.Frontal0);
                Services.TargetAddresses.Add(G14.Lateral0);
                Pieces = new[] { G14.Piece("P-1", RequirementRole.Required, "POSTE") };
            }

            internal RackSystemKind Kind { get; }
            internal DimensionViewKind TargetKind { get; }
            internal G14.Services Services { get; }
            internal LibraryPieceAvailabilityFact[] Pieces { get; set; }
            internal RackViewAddress SourceAddress { get; set; } = G14.Planta;
            internal IReadOnlyList<RackPhysicalDefinitionSnapshot> Definitions => definitions;

            internal void AddRack(string rackId, string referenceKey, double x, double y, double rotation = 0.0, double originX = 0.0, string name = "Rack")
            {
                var definitionKey = "DEF-" + referenceKey;
                references.Add(G14.Reference(referenceKey, definitionKey));
                definitions.Add(G14.Definition(definitionKey, rackId, KindOf(Kind), name: name));
                facts[referenceKey] = G14.Facts(x, y, rotation, originX: originX);
                rackIds.Add(rackId);
                Services.WithName(rackId, name);
                Services.WithFrame(rackId, G14.Planta, G14.PlantaFrame());
                Services.WithFrame(rackId, G14.Frontal0, G14.FrontalFrame());
                Services.WithFrame(rackId, G14.Lateral0, G14.LateralFrame());
            }

            internal RackProjectionSnapshot Snapshot()
            {
                foreach (var rackId in rackIds)
                {
                    var targets = new[] { G14.Planta, G14.Frontal0, G14.Lateral0 };
                    foreach (var target in targets)
                    {
                        Services.WithPreparation(
                            rackId,
                            target,
                            new RackProjectionTargetPreparation(
                                true,
                                PieceRequirementExtractionOutcome.Extracted,
                                Pieces,
                                null,
                                RackProjectionMaterializationFamilies.Of(Kind) == RackProjectionMaterializationFamily.Unsupported
                                    ? null
                                    : Prepared(rackId, Kind, target)));
                    }
                }

                var selection = G14.Selection(references, definitions);
                return RackProjectionSnapshot.Available(
                    new RackProjectionRequest(selection, TargetKind, facts, Services.Build()),
                    Array.Empty<string>());
            }

            internal Port NewPort(params RackProjectionPick[] picks)
                => new Port(this, picks);
        }

        internal sealed class ScopeFactory : IRackProjectionWriteScopeFactory
        {
            private readonly List<string> events;

            internal ScopeFactory(List<string> events) => this.events = events;

            internal int Begun { get; private set; }
            internal int Commits { get; private set; }
            internal int Disposed { get; private set; }
            internal List<RackProjectionPreparedView> Created { get; } = new List<RackProjectionPreparedView>();
            internal List<RackProjectedPlacement> Placed { get; } = new List<RackProjectedPlacement>();
            internal Func<RackProjectionPreparedView, RackProjectionDefinitionResult> OnCreate { get; set; }
            internal Func<RackProjectedPlacement, RackProjectionReferenceResult> OnPlace { get; set; }
            internal Action OnCommit { get; set; }

            public IRackProjectionWriteScope Begin()
            {
                Begun++;
                events.Add("begin");
                return new Scope(this);
            }

            private sealed class Scope : IRackProjectionWriteScope
            {
                private readonly ScopeFactory owner;
                private bool committed;

                internal Scope(ScopeFactory owner) => this.owner = owner;

                public RackProjectionDefinitionResult CreateDefinition(RackProjectionPreparedView view)
                {
                    owner.events.Add("create:" + view.RackId);
                    owner.Created.Add(view);
                    return owner.OnCreate != null
                        ? owner.OnCreate(view)
                        : RackProjectionDefinitionResult.Created(view.BaseName, view.RackId);
                }

                public RackProjectionReferenceResult PlaceReference(
                    RackProjectionDefinitionResult definition, RackProjectedPlacement placement)
                {
                    owner.events.Add("place:" + placement.PhysicalKey);
                    owner.Placed.Add(placement);
                    return owner.OnPlace != null ? owner.OnPlace(placement) : RackProjectionReferenceResult.Placed();
                }

                public void Commit()
                {
                    owner.events.Add("commit");
                    owner.Commits++;
                    owner.OnCommit?.Invoke();
                    committed = true;
                }

                public void Dispose()
                {
                    owner.Disposed++;
                    owner.events.Add(committed ? "dispose:committed" : "dispose:rolledback");
                }
            }
        }

        internal sealed class Port : IRackProjectionCommandPort
        {
            private readonly Scenario scenario;
            private readonly Queue<RackProjectionPick> picks;

            internal Port(Scenario scenario, RackProjectionPick[] picks)
            {
                this.scenario = scenario;
                this.picks = new Queue<RackProjectionPick>(picks);
                Scopes = new ScopeFactory(Events);
            }

            internal List<string> Events { get; } = new List<string>();
            internal List<string> Printed { get; } = new List<string>();
            internal ScopeFactory Scopes { get; }
            internal RackProjectionSnapshot SnapshotToReturn { get; set; }
            internal Func<IReadOnlyList<LibraryBlockRequirement>, RackProjectionLibraryObservation> Observe { get; set; }
            internal IReadOnlyList<LibraryBlockRequirement> Imported { get; private set; }
            internal int CaptureCalls { get; private set; }

            public IRackProjectionWriteScopeFactory WriteScopes => Scopes;

            public RackProjectionSnapshot Capture()
            {
                CaptureCalls++;
                Events.Add("capture");
                return SnapshotToReturn ?? scenario.Snapshot();
            }

            public void Report(IReadOnlyList<string> lines)
            {
                Events.Add("report");
                Printed.AddRange(lines);
            }

            public void BeforeWrite() => Events.Add("before-write");

            public RackProjectionPick PickBasePoint()
            {
                Events.Add("pick-base");
                return picks.Count > 0 ? picks.Dequeue() : RackProjectionPick.Cancel();
            }

            public RackProjectionPick PickTargetPoint(Point3D basePoint)
            {
                Events.Add("pick-target");
                return picks.Count > 0 ? picks.Dequeue() : RackProjectionPick.Cancel();
            }

            public RackProjectionLibraryObservation ImportAndObserve(IReadOnlyList<LibraryBlockRequirement> requirements)
            {
                Events.Add("import");
                Imported = requirements;
                return Observe != null
                    ? Observe(requirements)
                    : new RackProjectionLibraryObservation(
                        requirements
                            .Select(r => new LibraryBlockAvailabilityFact(r, LibraryBlockAvailability.Found))
                            .ToList(),
                        false);
            }
        }

        internal static RackProjectionPick[] Points(double bx = 0, double by = 0, double tx = 500, double ty = 0)
            => new[]
            {
                RackProjectionPick.Ok(new Point3D(bx, by, 0)),
                RackProjectionPick.Ok(new Point3D(tx, ty, 0)),
            };
    }
}
