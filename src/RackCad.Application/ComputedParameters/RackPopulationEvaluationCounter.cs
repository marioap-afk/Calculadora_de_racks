using System;
using System.Threading;

namespace RackCad.Application.ComputedParameters
{
    /// <summary>
    /// El contador UNICO de evaluaciones de poblacion (I-63 INV-32, A-2.1). Lo incrementa el orquestador real,
    /// <see cref="ProjectPopulation.Evaluate"/>, y ningun otro camino. Se observa abriendo un ambito con
    /// <see cref="Begin"/>: el ambito fluye con el contexto de ejecucion, asi que las pruebas que corren en paralelo
    /// no se contaminan entre si y el contador no es un estatico compartido sin alcance.
    /// </summary>
    public sealed class RackPopulationEvaluationCounter : IDisposable
    {
        private static readonly AsyncLocal<RackPopulationEvaluationCounter> Current
            = new AsyncLocal<RackPopulationEvaluationCounter>();

        private readonly RackPopulationEvaluationCounter _parent;
        private int _count;

        private RackPopulationEvaluationCounter(RackPopulationEvaluationCounter parent)
        {
            _parent = parent;
        }

        /// <summary>Las evaluaciones de poblacion observadas desde que se abrio el ambito.</summary>
        public int Count => Volatile.Read(ref _count);

        /// <summary>Abre un ambito de observacion. Cerrarlo (<see cref="Dispose"/>) restaura el ambito anterior.</summary>
        public static RackPopulationEvaluationCounter Begin()
        {
            var counter = new RackPopulationEvaluationCounter(Current.Value);
            Current.Value = counter;
            return counter;
        }

        /// <summary>Anota UNA evaluacion de poblacion en el ambito activo y en los que lo contienen.</summary>
        internal static void Record()
        {
            // Esqueleto del RED: aun no anota evaluaciones.
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
