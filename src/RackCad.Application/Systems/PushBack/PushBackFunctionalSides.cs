using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;

namespace RackCad.Application.Systems.PushBack
{
    /// <summary>
    /// I-42 (ronda 8, V1) — QUE LADOS TIENEN ALMACENAMIENTO FUNCIONAL en un tramo del rack.
    ///
    /// <para>
    /// Son TRES cosas distintas y esta clase existe para no volver a confundirlas:
    /// <list type="bullet">
    /// <item><b>la ranura fisica existe</b> — la reticula transversal la conserva siempre, tambien en blanco
    /// (ronda 2), porque de ella cuelgan postes, placas y cabeceras;</item>
    /// <item><b>el lado esta declarado</b> — <see cref="PushBackSideSystem.IsPresent"/>, que es una propiedad del
    /// RACK entero: dice que ese lado existe en alguna parte;</item>
    /// <item><b>ese lado almacena algo AQUI</b> — lo unico que autoriza a rotularlo en una vista que muestra ese
    /// tramo.</item>
    /// </list>
    /// </para>
    ///
    /// <para>
    /// La regla de blanco NO se duplica: se pregunta por los niveles EFECTIVOS del frente de ese lado en esa ranura
    /// (<see cref="DynamicFrontActivation"/>), que es la misma autoridad que usan el resolver, el BOM y el resto de
    /// los constructores.
    /// </para>
    /// </summary>
    public static class PushBackFunctionalSides
    {
        /// <summary>Si <paramref name="side"/> almacena en la ranura <paramref name="slot"/>.</summary>
        public static bool HasStorageAt(PushBackSideSystem side, int slot)
        {
            if (side == null || !side.IsPresent)
            {
                return false;
            }

            var front = side.Front(slot);
            return front != null && DynamicFrontActivation.EffectiveLoadLevels(front) > 0;
        }

        /// <summary>Si <paramref name="side"/> almacena en ALGUNA de las ranuras que <paramref name="shows"/> admite.</summary>
        public static bool HasStorageIn(PushBackSystem system, PushBackSideSystem side, Func<int, bool> shows)
        {
            var slots = system?.Structure?.Fronts?.Count ?? 0;
            for (var slot = 0; slot < slots; slot++)
            {
                if ((shows == null || shows(slot)) && HasStorageAt(side, slot))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>Los lados con almacenamiento en las ranuras que <paramref name="shows"/> admite, en orden A, B.</summary>
        public static IReadOnlyList<PushBackSide> In(PushBackSystem system, Func<int, bool> shows)
        {
            var composite = system?.Composite;
            if (composite == null)
            {
                return Array.Empty<PushBackSide>();
            }

            return new[]
                {
                    (Side: PushBackSide.A, View: composite.SideA),
                    (Side: PushBackSide.B, View: composite.SideB),
                }
                .Where(candidate => HasStorageIn(system, candidate.View, shows))
                .Select(candidate => candidate.Side)
                .ToList();
        }
    }
}
