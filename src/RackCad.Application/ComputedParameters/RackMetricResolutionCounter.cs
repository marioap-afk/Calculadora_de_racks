using System;
using System.Threading;

namespace RackCad.Application.ComputedParameters
{
    /// <summary>
    /// El contador de resoluciones Phi2+Phi3 de metricas (I-63 INV-30, D-24): lo incrementa el costado de resolucion REAL,
    /// <see cref="RackMetricResolutionSide"/>, y ningun otro camino. Sigue el patron de
    /// <see cref="RackPopulationEvaluationCounter"/>: se observa abriendo un ambito con <see cref="Begin"/>, que fluye con
    /// el contexto de ejecucion, asi que las pruebas en paralelo no se contaminan y no hay estatico compartido sin alcance.
    /// </summary>
    public sealed class RackMetricResolutionCounter : IDisposable
    {
        private static readonly AsyncLocal<RackMetricResolutionCounter> Current
            = new AsyncLocal<RackMetricResolutionCounter>();

        private readonly RackMetricResolutionCounter _parent;
        private int _count;

        private RackMetricResolutionCounter(RackMetricResolutionCounter parent)
        {
            _parent = parent;
        }

        /// <summary>Las resoluciones observadas desde que se abrio el ambito.</summary>
        public int Count => Volatile.Read(ref _count);

        /// <summary>Abre un ambito de observacion. Cerrarlo (<see cref="Dispose"/>) restaura el ambito anterior.</summary>
        public static RackMetricResolutionCounter Begin()
        {
            var counter = new RackMetricResolutionCounter(Current.Value);
            Current.Value = counter;
            return counter;
        }

        /// <summary>Anota UNA resolucion en el ambito activo y en los que lo contienen.</summary>
        internal static void Record()
        {
            for (var counter = Current.Value; counter != null; counter = counter._parent)
            {
                Interlocked.Increment(ref counter._count);
            }
        }

        public void Dispose()
        {
            if (ReferenceEquals(Current.Value, this))
            {
                Current.Value = _parent;
            }
        }
    }
}
