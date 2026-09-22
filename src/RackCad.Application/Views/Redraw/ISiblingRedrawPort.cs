using System;
using System.Collections.Generic;

namespace RackCad.Application.Views.Redraw
{
    public enum RackSiblingLayerUse
    {
        DirectReference,
        ExistingDefinitionEntity
    }

    public sealed class RackSiblingLayerRequirement
    {
        public RackSiblingLayerRequirement(string layer, bool isLocked, RackSiblingLayerUse use = RackSiblingLayerUse.DirectReference)
        {
            Layer = layer;
            IsLocked = isLocked;
            Use = use;
        }

        public string Layer { get; }
        public bool IsLocked { get; }
        public RackSiblingLayerUse Use { get; }
    }

    public sealed class RackSiblingUnitPreparation<TUnit>
    {
        private RackSiblingUnitPreparation(bool success, TUnit unit, IReadOnlyList<RackSiblingLayerRequirement> layers, string diagnostic)
        {
            IsSuccess = success;
            Unit = unit;
            Layers = layers ?? Array.Empty<RackSiblingLayerRequirement>();
            Diagnostic = diagnostic;
        }

        public bool IsSuccess { get; }
        public TUnit Unit { get; }
        public IReadOnlyList<RackSiblingLayerRequirement> Layers { get; }
        public string Diagnostic { get; }

        public static RackSiblingUnitPreparation<TUnit> Prepared(TUnit unit, IReadOnlyList<RackSiblingLayerRequirement> layers = null)
            => new RackSiblingUnitPreparation<TUnit>(true, unit, layers, null);

        public static RackSiblingUnitPreparation<TUnit> Failed(string diagnostic)
            => new RackSiblingUnitPreparation<TUnit>(false, default, null, diagnostic);
    }

    public enum RackSiblingMutationKind { Committed, Discarded }

    public sealed class RackSiblingMutationResult
    {
        private RackSiblingMutationResult(RackSiblingMutationKind kind, string unit, string diagnostic)
        {
            Kind = kind;
            FailedUnit = unit;
            Diagnostic = diagnostic;
        }

        public RackSiblingMutationKind Kind { get; }
        public string FailedUnit { get; }
        public string Diagnostic { get; }
        public static RackSiblingMutationResult Committed() => new RackSiblingMutationResult(RackSiblingMutationKind.Committed, null, null);
        public static RackSiblingMutationResult Discarded(string unit, string diagnostic) => new RackSiblingMutationResult(RackSiblingMutationKind.Discarded, unit, diagnostic);
    }

    public interface ISiblingRedrawPort<TUnit>
    {
        RackSiblingUnitPreparation<TUnit> Prepare(RackSiblingMember member);
        RackSiblingMutationResult Mutate(IReadOnlyList<TUnit> units);
        void Post(RackSiblingRedrawPlan<TUnit> plan, RackSiblingMutationResult committed);
    }
}
