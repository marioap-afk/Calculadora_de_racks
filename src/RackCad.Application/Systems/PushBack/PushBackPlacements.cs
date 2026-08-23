using System.Collections.Generic;
using RackCad.Application.Systems.Dynamic;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;

namespace RackCad.Application.Systems.PushBack
{
    /// <summary>
    /// I-41 (PB-015) — LAS colocaciones de largueros de extremo de Push Back. Compone
    /// <see cref="DynamicLoadBeamGeometry.Placements(DynamicRackSystem, DynamicRackFront)"/> como CAJA NEGRA y le
    /// cambia una sola cosa: la X del larguero de ENTRADA (el posterior), que deja de ser el <c>EndX</c> del frente y
    /// pasa a ser la de la celda, resuelta por <see cref="PushBackCellDepth.RearX"/>.
    ///
    /// <para>
    /// Existe para que ese cambio ocurra en UN sitio. Antes de I-41 los tres consumidores de Push Back —las
    /// elevaciones, los dos largueros de extremo y el tope posterior— llamaban cada uno a la geometria dinamica; si
    /// cada uno aplicara el fondo por celda por su cuenta, bastaria con que uno se olvidara para que el tope quedase
    /// colgado de un larguero que ya no esta ahi.
    /// </para>
    /// <para>
    /// El extremo BAJO no se toca: los dos extremos de una calle Push Back comparten el pasillo, asi que el larguero
    /// de entrada/salida sigue en el arranque del frente independientemente de lo profundo que sea el nivel.
    /// </para>
    /// El Dinamico no pasa por aqui y su geometria no se modifica.
    /// </summary>
    public static class PushBackPlacements
    {
        /// <summary>
        /// Las colocaciones del frente con la X del posterior ya resuelta por celda. Sin frente (lateral no
        /// seccionado) devuelve las dinamicas tal cual: no hay celda a la que preguntar, y la referencia es el rack
        /// entero, exactamente como antes de I-41.
        /// </summary>
        public static IReadOnlyList<DynamicLoadBeamPlacement> Resolve(PushBackSystem system, DynamicRackFront front)
        {
            var structure = system?.Structure;
            var placements = DynamicLoadBeamGeometry.Placements(structure, front);
            if (structure == null || front == null)
            {
                return placements;
            }

            var result = new List<DynamicLoadBeamPlacement>(placements.Count);
            foreach (var placement in placements)
            {
                if (!placement.IsEntrance)
                {
                    result.Add(placement);
                    continue;
                }

                var rearX = PushBackCellDepth.RearX(system, front, placement.LevelNumber);
                result.Add(new DynamicLoadBeamPlacement(
                    placement.LevelNumber,
                    true,
                    rearX,
                    placement.Y,
                    placement.MirroredX,
                    placement.BeamCatalogId,
                    placement.BeamDepth,
                    placement.BeamLength));
            }

            return result;
        }
    }
}
