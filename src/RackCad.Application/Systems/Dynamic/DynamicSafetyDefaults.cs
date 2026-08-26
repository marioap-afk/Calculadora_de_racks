using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems.Selective;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.Selective;

namespace RackCad.Application.Systems.Dynamic
{
    /// <summary>Catalog-driven safety selections applied to a new dynamic rack.</summary>
    public static class DynamicSafetyDefaults
    {
        private static readonly string[] Families =
        {
            SelectiveSafetyDefaults.BotaType,
            SelectiveSafetyDefaults.LateralType,
            SelectiveSafetyDefaults.DesviadorType,
            SelectiveSafetyDefaults.DefensaType,
            SelectiveSafetyDefaults.GuiaType
        };

        public static IReadOnlyList<SelectiveSafetySelection> Build(RackCatalog catalog)
        {
            var result = new List<SelectiveSafetySelection>();
            foreach (var family in Families)
            {
                var element = catalog?.SafetyElements?.FirstOrDefault(entry => entry != null
                    && !string.IsNullOrWhiteSpace(entry.Id)
                    && SelectiveSafetyDefaults.IsType(entry.Type, family));
                if (element == null)
                {
                    continue;
                }

                result.Add(new SelectiveSafetySelection
                {
                    ElementId = element.Id,
                    Quantity = 1,
                    Side = SelectiveSafetyDefaults.IsType(family, SelectiveSafetyDefaults.BotaType)
                           || SelectiveSafetyDefaults.IsType(family, SelectiveSafetyDefaults.DesviadorType)
                        ? SafetySide.Both
                        : SafetySide.None
                });
            }

            return result;
        }
    }

    /// <summary>
    /// Dynamic LATERAL default: the first post faces the exit and the last faces the entrance. An empty override list
    /// remains adaptive when fronts are added or removed; any explicit per-post entry switches to the authored grid.
    /// </summary>
    public static class DynamicLateralGuardPlan
    {
        public static SafetySide SideAt(SelectiveSafetySelection selection, int postIndex, int postCount)
        {
            if (selection == null || postIndex < 0 || postIndex >= Math.Max(1, postCount))
            {
                return SafetySide.None;
            }

            if (selection.Side != SafetySide.None || selection.PostSides.Any(post => post != null))
            {
                return selection.SideForPost(postIndex);
            }

            // Regla ADAPTATIVA: protector en los DOS postes de los extremos y en ninguno interior.
            //
            // Owner-validation round 2 (I-32): aquí Left/Right es ORIENTACIÓN, no extremo longitudinal. La versión
            // anterior leía el Right del último poste como «extremo posterior» y, en un sistema de extremo bajo,
            // lo borraba — con lo que un Push Back nuevo perdía el protector del último poste. Un rack SIEMPRE
            // lleva los dos: el primero sin espejo y el último espejado, porque protegen caras opuestas del
            // pasillo. Dónde acaba cada copia lo decide <see cref="CopiesAt"/>, que es quien conoce los extremos.
            if (postCount <= 1)
            {
                return SafetySide.Both;   // un solo poste es el primero y el último a la vez
            }

            if (postIndex == 0)
            {
                return SafetySide.Left;
            }

            if (postIndex == postCount - 1)
            {
                return SafetySide.Right;
            }

            return SafetySide.None;
        }

        /// <summary>
        /// Las copias físicas del protector en un poste. Owner-validation round 1 (I-32): este es el camino REAL del
        /// protector lateral, y también aquí hay que separar los tres ejes.
        ///
        /// <para>Una elección EXPLÍCITA del usuario la resuelve <see cref="SelectiveSafetyEnds"/>, la misma autoridad
        /// que el resto de la seguridad: conserva la pertenencia, conserva la orientación y, en un sistema de extremo
        /// bajo, lleva la pieza delante. Antes el lado se traducía directamente a un extremo, y un <c>Right</c> en
        /// Push Back acababa dibujado ATRÁS, donde no hay pasillo que proteger.</para>
        ///
        /// <para>La regla ADAPTATIVA (sin elección) emite las copias DIRECTAMENTE, sin pasar por un
        /// <see cref="SafetySide"/> intermedio que volvería a mezclar los dos ejes: el primer poste lleva una copia
        /// sin espejo y el último una espejada, y en un sistema de extremo bajo las dos van DELANTE. El último
        /// protector no desaparece por ser de extremo bajo — cambia de orientación, no de extremo.</para>
        /// </summary>
        public static IReadOnlyList<SafetyEndCopy> CopiesAt(
            SelectiveSafetySelection selection, int postIndex, int postCount)
        {
            if (selection == null || postIndex < 0 || postIndex >= Math.Max(1, postCount))
            {
                return new SafetyEndCopy[0];
            }

            if (selection.Side != SafetySide.None || selection.PostSides.Any(post => post != null))
            {
                return SelectiveSafetyEnds.CopiesForPost(selection, postIndex);
            }

            // El extremo ALTO solo existe donde el sistema lo tiene. Con la marca de extremo bajo, la copia
            // espejada se queda delante en vez de desaparecer.
            var atFarEnd = !selection.LowEndOnly;

            // I-42 — un Push Back COMPUESTO tiene cara de carga en los DOS extremos: son dos pasillos, y cada uno
            // necesita su par de protectores. La PERTENENCIA no cambia —siguen siendo las dos lineas de orilla, y
            // ningun poste interior—: lo que se duplica es la CARA, no el numero de postes. Convertir «dos pasillos»
            // en «protector en cada poste» es exactamente el defecto que el dueño vio.
            // I-42: la segunda cara se pregunta POR LINEA. Un rack compuesto PARCIAL la tiene solo donde hay
            // lado B; las lineas de los frentes que siguen siendo de un solo sentido conservan su regla legacy.
            var bothFaces = selection.HasSecondLoadFaceAt(postIndex);

            if (postCount <= 1)
            {
                return bothFaces
                    ? new[]
                    {
                        new SafetyEndCopy(atHighEnd: false, mirrored: false),
                        new SafetyEndCopy(atHighEnd: false, mirrored: true),
                        new SafetyEndCopy(atHighEnd: true, mirrored: false),
                        new SafetyEndCopy(atHighEnd: true, mirrored: true),
                    }
                    : new[]
                    {
                        new SafetyEndCopy(atHighEnd: false, mirrored: false),
                        new SafetyEndCopy(atHighEnd: atFarEnd, mirrored: true),
                    };
            }

            // La copia de la cara LEJANA es la IMAGEN ESPEJO de la cercana: los dos pasillos miran en sentidos
            // opuestos, asi que la pieza que protege uno esta girada respecto de la que protege el otro. Antes las
            // dos salian con la misma mano y la del fondo quedaba del reves — la orientacion que el dueño rechazo.
            if (postIndex == 0)
            {
                return bothFaces
                    ? new[]
                    {
                        new SafetyEndCopy(atHighEnd: false, mirrored: false),
                        new SafetyEndCopy(atHighEnd: true, mirrored: true),
                    }
                    : new[] { new SafetyEndCopy(atHighEnd: false, mirrored: false) };
            }

            if (postIndex == postCount - 1)
            {
                return bothFaces
                    ? new[]
                    {
                        new SafetyEndCopy(atHighEnd: false, mirrored: true),
                        new SafetyEndCopy(atHighEnd: true, mirrored: false),
                    }
                    : new[] { new SafetyEndCopy(atHighEnd: atFarEnd, mirrored: true) };
            }

            return new SafetyEndCopy[0];
        }

        /// <summary>La copia que le toca a un extremo, o null si ese extremo no lleva protector en ese poste.</summary>
        public static SafetyEndCopy? CopyAtEnd(
            SelectiveSafetySelection selection, int postIndex, int postCount, DynamicRackEnd end)
        {
            var highEnd = end == DynamicRackEnd.Entrance;
            foreach (var copy in CopiesAt(selection, postIndex, postCount))
            {
                if (copy.AtHighEnd == highEnd)
                {
                    return copy;
                }
            }

            return null;
        }

        public static bool DrawsAtEnd(
            SelectiveSafetySelection selection,
            int postIndex,
            int postCount,
            DynamicRackEnd end)
            => CopyAtEnd(selection, postIndex, postCount, end).HasValue;
    }
}
