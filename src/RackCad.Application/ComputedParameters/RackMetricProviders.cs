using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Selective;
using RackCad.Domain.Systems.Selective;

namespace RackCad.Application.ComputedParameters
{
    /// <summary>
    /// Provider de metricas por kind (I-63 D-08): una funcion PURA, sin estado ni cache, que no lee AutoCAD, el
    /// registro ni catalogos, no llama a resolvers ni a otro provider. Solo recibe lo que el orquestador ya resolvio.
    /// Emite un <see cref="MetricValue"/> por CADA metrica <c>(Rack, *)</c>.
    /// </summary>
    public interface IRackMetricProvider
    {
        /// <summary>El token <c>RackEmbedDocument.Kind*</c> que atiende.</summary>
        string KindToken { get; }

        /// <summary>Soporte (D-06) y fase minima de una metrica <c>(Rack, *)</c> para este kind.</summary>
        RackMetricDeclaration Declare(MetricId metric);

        RackMetricResults Compute(RackMetricInput input);
    }

    /// <summary>Base de los providers: aplica D-06 con una tabla fija y delega solo lo <c>Supported</c>.</summary>
    internal abstract class RackMetricProviderBase : IRackMetricProvider
    {
        public abstract string KindToken { get; }

        public abstract RackMetricDeclaration Declare(MetricId metric);

        public RackMetricResults Compute(RackMetricInput input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            var values = new Dictionary<MetricId, MetricValue>();
            foreach (var metric in RackMetricIds.RackMetrics)
            {
                var declaration = Declare(metric);
                switch (declaration.Support)
                {
                    case RackMetricSupport.NotSupported:
                        values[metric] = MetricValue.NotSupported();
                        break;

                    case RackMetricSupport.NotApplicable:
                        values[metric] = MetricValue.NotApplicable();
                        break;

                    default:
                        values[metric] = ComputeSupported(metric, input);
                        break;
                }
            }

            return RackMetricResults.Create(input.RackId, values);
        }

        /// <summary>Valor de una metrica <c>Supported</c>. Por defecto, ninguna lo es.</summary>
        protected virtual MetricValue ComputeSupported(MetricId metric, RackMetricInput input)
            => throw new InvalidOperationException("El kind " + KindToken + " no declara soporte para " + metric + ".");
    }

    /// <summary>Selectivo: Supported en frentes y vacios, sobre el sistema resuelto del fondo 0 (D-07).</summary>
    internal sealed class SelectiveRackMetricProvider : RackMetricProviderBase
    {
        public override string KindToken => RackEmbedDocument.KindSelective;

        public override RackMetricDeclaration Declare(MetricId metric)
            => RackMetricDeclaration.Supported(RackMetricPhase.Resolved);

        protected override MetricValue ComputeSupported(MetricId metric, RackMetricInput input)
        {
            var prerequisite = input.Prerequisite
                ?? throw new InvalidOperationException("Una metrica Supported exige el prerrequisito del orquestador.");

            if (!prerequisite.IsResolved)
            {
                return MetricValue.Unavailable(prerequisite.Failure);
            }

            var bays = SelectiveDepthLayout.BaysOfFondo(prerequisite.ResolvedSystem, 0)
                ?? new List<SelectiveBay>();

            if (metric == RackMetricIds.Frentes)
            {
                return MetricValue.Available(bays.Count);
            }

            return MetricValue.Available(bays.Count(bay => bay.Levels.Count == 0 && bay.FloorPalletCount <= 0));
        }
    }

    internal sealed class DynamicRackMetricProvider : RackMetricProviderBase
    {
        public override string KindToken => RackEmbedDocument.KindDynamic;

        public override RackMetricDeclaration Declare(MetricId metric) => RackMetricDeclaration.NotSupported();
    }

    internal sealed class PushBackRackMetricProvider : RackMetricProviderBase
    {
        public override string KindToken => RackEmbedDocument.KindPushBack;

        public override RackMetricDeclaration Declare(MetricId metric) => RackMetricDeclaration.NotSupported();
    }

    internal sealed class CantileverRackMetricProvider : RackMetricProviderBase
    {
        public override string KindToken => RackEmbedDocument.KindCantilever;

        public override RackMetricDeclaration Declare(MetricId metric) => RackMetricDeclaration.NotSupported();
    }

    internal sealed class CabeceraRackMetricProvider : RackMetricProviderBase
    {
        public override string KindToken => RackEmbedDocument.KindCabecera;

        public override RackMetricDeclaration Declare(MetricId metric) => RackMetricDeclaration.NotApplicable();
    }

    internal sealed class CamaRackMetricProvider : RackMetricProviderBase
    {
        public override string KindToken => RackEmbedDocument.KindCama;

        public override RackMetricDeclaration Declare(MetricId metric) => RackMetricDeclaration.NotApplicable();
    }
}
