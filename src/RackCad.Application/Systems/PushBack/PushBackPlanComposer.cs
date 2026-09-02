using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Drawing;
using RackCad.Domain.Systems.Dynamic;

using System;

using System.Globalization;

namespace RackCad.Application.Systems.PushBack
{
    /// <summary>
    /// Pure helpers to transform a BLACK-BOX dynamic <see cref="HeaderRunPlan"/> into a Push Back plan: they identify
    /// the dynamic-specific pieces to remove by <see cref="HeaderBlockInstance.Role"/> and <see cref="HeaderBlockInstance.PieceId"/>
    /// (never by generated group name), while preserving the common structure (cabeceras, separators, derived posts,
    /// plates, annotations, dimensions). The dynamic plan itself is never mutated — callers build NEW lists.
    /// </summary>
    public static class PushBackPlanComposer
    {
        private static readonly System.StringComparison Ic = System.StringComparison.OrdinalIgnoreCase;

        /// <summary>The dynamic complete IN/OUT beam (both dynamic ends use it) — removed; Push Back re-adds its own low IN/OUT.</summary>
        public static bool IsDynamicEndBeam(HeaderBlockInstance instance)
            => instance != null && string.Equals(instance.PieceId, DynamicRackDefaults.InOutBeamCatalogId, Ic);

        /// <summary>The dynamic intermediate beam — removed; Push Back re-adds intermediates tangent to its own axis.</summary>
        public static bool IsDynamicIntermediate(HeaderBlockInstance instance)
            => instance != null && string.Equals(instance.PieceId, DynamicRackDefaults.IntermediateBeamCatalogId, Ic);

        /// <summary>Any roller-bed piece (rail/roller/brake/stop) — removed; Push Back re-adds its own bed.</summary>
        public static bool IsBedPiece(HeaderBlockInstance instance)
            => instance != null && (instance.Role == HeaderBlockRole.Rail
                || instance.Role == HeaderBlockRole.Roller
                || instance.Role == HeaderBlockRole.Brake
                || instance.Role == HeaderBlockRole.Stop);

        /// <summary>Any dynamic safety OR tope. Used to strip BOTH from the REAR frontal cut (which carries neither).</summary>
        public static bool IsSafetyPiece(HeaderBlockInstance instance)
            => instance != null && (instance.Role == HeaderBlockRole.Safety || instance.Role == HeaderBlockRole.Tope);

        /// <summary>A dynamic tope (rear pallet stop) — removed everywhere; Push Back adds its own rear topes.</summary>
        public static bool IsDynamicTope(HeaderBlockInstance instance)
            => instance != null && instance.Role == HeaderBlockRole.Tope;

        /// <summary>
        /// Dynamic-specific pieces removed when composing a Push Back plan: both dynamic end beams, the roller bed, the
        /// dynamic intermediate beams and any dynamic TOPE. The normal SAFETY (Role.Safety) is KEPT — the resolver already
        /// restricted it to the low end (GUIA-free), so it is the authoritative low-end safety projection.
        /// </summary>
        public static bool IsDynamicSpecific(HeaderBlockInstance instance)
            => IsBedPiece(instance) || IsDynamicTope(instance) || IsDynamicEndBeam(instance) || IsDynamicIntermediate(instance);

        /// <summary>A loose instance to KEEP: common structure (separators, derived posts, plates) + annotations/dimensions.</summary>
        public static bool KeepLoose(HeaderBlockInstance instance)
            => instance != null && !IsDynamicSpecific(instance);

        /// <summary>A <see cref="HeaderGroup"/> to KEEP: a structural cabecera group — none of its instances is dynamic-specific.</summary>
        public static bool KeepHeaderGroup(HeaderGroup group)
            => group?.Instances != null && !group.Instances.Any(IsDynamicSpecific);

        /// <summary>The structural cabecera groups of <paramref name="plan"/> (bed and intermediate groups removed).</summary>
        public static List<HeaderGroup> StructuralHeaderGroups(HeaderRunPlan plan)
            => (plan?.Headers ?? new List<HeaderGroup>()).Where(KeepHeaderGroup).ToList();

        /// <summary>The structural loose instances of <paramref name="plan"/> (dynamic ends/bed/safety/intermediates removed).</summary>
        /// <summary>
        /// La identidad FISICA de una pieza ya colocada: que es, donde esta, con que mano y con que rotacion.
        ///
        /// <para>
        /// Es la clave con la que una PROYECCION —un corte lateral, una planta— decide si dos instancias son la
        /// misma pieza. Vive aqui, y no en cada builder, porque el compositor del rack compuesto y el camino de un
        /// solo sentido tienen que usar exactamente la misma: si difirieran, una vista deduplicaria lo que la otra
        /// dibuja dos veces.
        /// </para>
        /// </summary>
        public static string PhysicalKey(HeaderBlockInstance instance)
            => instance == null
                ? string.Empty
                : string.Join(
                    "|",
                    instance.PieceId,
                    instance.Insertion.X.ToString("0.####", CultureInfo.InvariantCulture),
                    instance.Insertion.Y.ToString("0.####", CultureInfo.InvariantCulture),
                    instance.MirroredX,
                    instance.MirroredY,
                    instance.RotationRadians.ToString("0.######", CultureInfo.InvariantCulture));

        /// <summary>La identidad FISICA de un GRUPO: su definicion anidada y donde se coloca.</summary>
        public static string PhysicalKey(HeaderGroup group)
            => group == null
                ? string.Empty
                : "GRUPO|" + string.Join(
                    ";",
                    group.Instances.Select(PhysicalKey).Concat(
                        group.Placements.Select(placement => string.Join(
                            "|",
                            placement.InsertionX.ToString("0.####", CultureInfo.InvariantCulture),
                            placement.InsertionY.ToString("0.####", CultureInfo.InvariantCulture),
                            placement.Mirrored))));

        /// <summary>
        /// Un corte es una PROYECCION y dibuja cada cosa UNA vez.
        ///
        /// <para>
        /// Un corte muestra todos los frentes que su linea sostiene, y dos frentes contiguos proyectan sus piezas
        /// comunes EXACTAMENTE una encima de otra — el larguero de entrada de dos frentes que arrancan en la misma
        /// posicion, por ejemplo. Dibujar las dos deja bloques superpuestos en el DWG, que es lo que el dueño ve
        /// como «doble larguero». Las ANOTACIONES y las COTAS no se tocan: dos etiquetas iguales en el mismo sitio
        /// son un problema distinto y las emite otro pipeline.
        /// </para>
        /// <para>
        /// No afecta a ningun conteo: el BOM cuenta CAMAS y piezas del modelo, nunca instancias de una vista.
        /// </para>
        /// </summary>
        public static HeaderRunPlan DeduplicateProjection(
            IEnumerable<HeaderGroup> headers, IEnumerable<HeaderBlockInstance> loose)
        {
            var seen = new HashSet<string>(StringComparer.Ordinal);
            var groups = new List<HeaderGroup>();
            foreach (var group in headers ?? Enumerable.Empty<HeaderGroup>())
            {
                if (group != null && seen.Add(PhysicalKey(group)))
                {
                    groups.Add(group);
                }
            }

            var instances = new List<HeaderBlockInstance>();
            foreach (var instance in loose ?? Enumerable.Empty<HeaderBlockInstance>())
            {
                if (instance == null)
                {
                    continue;
                }

                var decoration = instance.Role == HeaderBlockRole.Annotation
                    || instance.Role == HeaderBlockRole.Dimension;
                if (decoration || seen.Add(PhysicalKey(instance)))
                {
                    instances.Add(instance);
                }
            }

            return new HeaderRunPlan(groups, instances);
        }

        public static List<HeaderBlockInstance> StructuralLoose(HeaderRunPlan plan)
            => (plan?.LooseInstances ?? new List<HeaderBlockInstance>()).Where(KeepLoose).ToList();
    }
}
