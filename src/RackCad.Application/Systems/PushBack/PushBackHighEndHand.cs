using System;
using System.Collections.Generic;
using RackCad.Application.Drawing;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;

namespace RackCad.Application.Systems.PushBack
{
    /// <summary>
    /// I-42 (correccion aislada 5) — LA MANO del larguero ALTO y de su tope, decidida por el EXTREMO FISICO de la
    /// cabecera que recibe ese extremo alto.
    ///
    /// <para>
    /// Regla del dueño: si la cama termina en el ULTIMO poste de la cabecera, el larguero y el tope conservan su
    /// orientacion normal; si termina en el PRIMER poste o en un poste interior, los dos se invierten. Van SIEMPRE
    /// juntos: no son dos decisiones, es una.
    /// </para>
    /// <para>
    /// Lo que habia antes no era esta pregunta. La mano salia de <c>DynamicLoadBeamGeometry.Placements</c>, que en el
    /// marco de una cama fija «bajo sin espejo, alto con espejo», y el compuesto la volteaba con su reflexion
    /// rigida. El resultado era uniforme —el escalon apuntaba hacia afuera de su propia cama— pero no miraba el
    /// extremo de la cabecera: medido, el lado B de unas encontradas, las dos corridas completas y el rack de un
    /// solo sentido quedaban al reves.
    /// </para>
    /// <para>
    /// Se resuelve sobre la X de MUNDO y sobre las vistas que muestran la PROFUNDIDAD —el lateral y la planta—,
    /// porque es ahi donde esa mano se ve. El corte frontal mira la retícula transversal y su espejo significa otra
    /// cosa: no se recalcula ahi, se deja el que ya trae.
    /// </para>
    /// </summary>
    public static class PushBackHighEndHand
    {
        /// <summary>
        /// Cuanto puede separarse una pieza del poste al que pertenece y seguir siendo de ese extremo. El tope se
        /// coloca a menos de una pulgada del contacto de su larguero, y el poste mas proximo esta a media
        /// profundidad: dos pulgadas distinguen sin ambiguedad.
        /// </summary>
        private const double EndTolerance = 2.0;

        /// <summary>
        /// True cuando <paramref name="x"/> cae en el ULTIMO poste de la cabecera, que es el unico extremo con
        /// orientacion normal. El primer poste y cualquier poste interior invierten.
        /// </summary>
        public static bool AtLastPost(DynamicRackSystem structure, double x)
            => structure != null
               && structure.TotalLength > 0.0
               && Math.Abs(x - structure.TotalLength) <= EndTolerance;

        /// <summary>
        /// Fija la mano del larguero ALTO y de su tope en un plan YA en coordenadas de mundo. El larguero bajo, la
        /// estructura, la cama y el resto de la seguridad no se tocan.
        /// </summary>
        public static void Apply(
            IEnumerable<HeaderBlockInstance> instances, PushBackSystem system, DynamicRackSystem structure)
        {
            if (instances == null || structure == null)
            {
                return;
            }

            var highId = string.IsNullOrWhiteSpace(system?.HighEndBeamCatalogId)
                ? PushBackDefaults.HighEndBeamCatalogId
                : system.HighEndBeamCatalogId;

            foreach (var instance in instances)
            {
                if (instance == null)
                {
                    continue;
                }

                var isHighBeam = instance.Role == HeaderBlockRole.Beam
                                 && string.Equals(instance.PieceId, highId, StringComparison.OrdinalIgnoreCase);
                if (!isHighBeam && instance.Role != HeaderBlockRole.Tope)
                {
                    continue;
                }

                // El tope va montado con la mano CONTRARIA a la de su larguero — es la relacion que ya existia en
                // las tres vistas y que el dueño no objeta. Lo que cambia es CUANDO se invierte el par.
                var normal = AtLastPost(structure, instance.Insertion.X);
                instance.MirroredX = isHighBeam ? !normal : normal;
            }
        }
    }
}
