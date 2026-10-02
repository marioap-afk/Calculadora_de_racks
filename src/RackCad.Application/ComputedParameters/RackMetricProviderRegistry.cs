using System;
using System.Collections.Generic;
using RackCad.Application.Persistence;

namespace RackCad.Application.ComputedParameters
{
    /// <summary>
    /// Registro CERRADO de providers (I-63 D-08): una tabla explicita por los seis tokens
    /// <c>RackEmbedDocument.Kind*</c>, construida con <see cref="KindDispatch{T}"/>. Sin reflexion, configuracion ni
    /// descubrimiento: un token duplicado o en blanco falla al construir.
    /// </summary>
    public sealed class RackMetricProviderRegistry
    {
        private RackMetricProviderRegistry(KindDispatch<IRackMetricProvider> dispatch)
        {
            Dispatch = dispatch;
        }

        /// <summary>El registro productivo: exactamente los seis kinds.</summary>
        public static RackMetricProviderRegistry Default { get; } = new RackMetricProviderRegistry(
            new KindDispatch<IRackMetricProvider>(
                // ESQUELETO RED: ningun provider registrado todavia.
                new IRackMetricProvider[0],
                provider => provider.KindToken));

        public KindDispatch<IRackMetricProvider> Dispatch { get; }

        /// <summary>Busca el provider con comparador <c>Ordinal</c>, el mismo del despacho de handlers.</summary>
        public bool TryGet(string kindToken, out IRackMetricProvider provider)
            => Dispatch.TryGet(kindToken, out provider);
    }
}
