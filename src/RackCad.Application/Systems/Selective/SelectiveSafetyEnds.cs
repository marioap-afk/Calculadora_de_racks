using System.Collections.Generic;
using RackCad.Domain.Systems.Selective;

namespace RackCad.Application.Systems.Selective
{
    /// <summary>Una copia física de una pieza de seguridad: en qué extremo va y con qué orientación.</summary>
    public readonly struct SafetyEndCopy
    {
        public SafetyEndCopy(bool atHighEnd, bool mirrored)
        {
            AtHighEnd = atHighEnd;
            Mirrored = mirrored;
        }

        /// <summary>True si la copia va en el extremo ALTO; false en el BAJO.</summary>
        public bool AtHighEnd { get; }

        /// <summary>Orientación de la copia en su propio sitio.</summary>
        public bool Mirrored { get; }
    }

    /// <summary>
    /// Owner-validation round 1 (I-32) — separa los TRES ejes que <see cref="SafetySide"/> mezcla en una sola
    /// enumeración, y que confundirlos costó la validación manual:
    ///
    /// <list type="number">
    /// <item><b>Pertenencia</b> — qué postes llevan la pieza. Vive en
    /// <see cref="SelectiveSafetySelection.PostSides"/>: una entrada con <see cref="SafetySide.None"/> excluye ese
    /// poste, y un poste sin entrada hereda el <see cref="SelectiveSafetySelection.Side"/> general.</item>
    /// <item><b>Orientación</b> — el espejo de la pieza en su propio sitio.</item>
    /// <item><b>Extremo longitudinal</b> — en qué punta del rack se dibuja.</item>
    /// </list>
    ///
    /// Push Back solo necesita restringir el TERCERO: entrada y salida comparten el extremo bajo y el alto no lleva
    /// seguridad ordinaria. La versión anterior lo conseguía BORRANDO la matriz por poste, con lo que destruía el
    /// primero. La versión intermedia colapsaba <c>Right</c> a <c>Left</c>, con lo que destruía el SEGUNDO: perdía la
    /// orientación que el usuario había elegido.
    ///
    /// La regla vigente conserva los tres: una elección <c>Right</c> en Push Back se dibuja <b>en el extremo bajo,
    /// orientada a la derecha en su propio sitio, nunca atrás</b>.
    /// </summary>
    public static class SelectiveSafetyEnds
    {
        private static readonly IReadOnlyList<SafetyEndCopy> None = new SafetyEndCopy[0];

        /// <summary>
        /// Las copias físicas de la pieza en el poste <paramref name="postIndex"/>.
        ///
        /// Vacío significa que ese poste no la lleva — la PERTENENCIA manda y nunca se reinterpreta. Sin la marca
        /// <see cref="SelectiveSafetySelection.LowEndOnly"/> (Selectivo, Dinámico) el lado se lee literal, como
        /// siempre: Left una copia baja sin espejo, Right una alta espejada, Both las dos. Con la marca, TODAS las
        /// copias caen en el extremo bajo y cada una conserva su propia orientación.
        /// </summary>
        public static IReadOnlyList<SafetyEndCopy> CopiesForPost(SelectiveSafetySelection selection, int postIndex)
        {
            var side = selection?.SideForPost(postIndex) ?? SafetySide.None;
            if (side == SafetySide.None)
            {
                return None;
            }

            var lowEndOnly = selection.LowEndOnly;

            // I-42: un sistema con cara de carga en los DOS extremos (un Push Back compuesto) materializa cada
            // orientacion elegida en LOS DOS: son dos pasillos, y lo que protege a uno no protege al otro. La
            // PERTENENCIA —que postes llevan la pieza— no se toca: sigue siendo la del usuario o la adaptativa.
            if (selection.HasSecondLoadFaceAt(postIndex))
            {
                // La copia de la cara LEJANA es la IMAGEN ESPEJO de la cercana. Los dos pasillos de un rack
                // compuesto miran en sentidos opuestos: la bota que protege uno esta girada respecto de la que
                // protege el otro. Repetir la mano dejaba la del fondo del reves, que es la orientacion que el
                // dueño rechazo. Con un solo pasillo esto no se ejecuta y nada cambia.
                switch (side)
                {
                    case SafetySide.Left:
                        return new[]
                        {
                            new SafetyEndCopy(atHighEnd: false, mirrored: false),
                            new SafetyEndCopy(atHighEnd: true, mirrored: true),
                        };

                    case SafetySide.Right:
                        return new[]
                        {
                            new SafetyEndCopy(atHighEnd: false, mirrored: true),
                            new SafetyEndCopy(atHighEnd: true, mirrored: false),
                        };

                    default:   // Both: las dos caras en los dos extremos, y el espejo ya esta en el par
                        return new[]
                        {
                            new SafetyEndCopy(atHighEnd: false, mirrored: false),
                            new SafetyEndCopy(atHighEnd: false, mirrored: true),
                            new SafetyEndCopy(atHighEnd: true, mirrored: false),
                            new SafetyEndCopy(atHighEnd: true, mirrored: true),
                        };
                }
            }

            switch (side)
            {
                case SafetySide.Left:
                    return new[] { new SafetyEndCopy(atHighEnd: false, mirrored: false) };

                case SafetySide.Right:
                    // Push Back: el extremo alto no existe para la seguridad, pero la ORIENTACIÓN elegida sí se respeta.
                    return new[] { new SafetyEndCopy(atHighEnd: !lowEndOnly, mirrored: true) };

                default:   // Both
                    return lowEndOnly
                        ? new[] { new SafetyEndCopy(atHighEnd: false, mirrored: false) }
                        : new[]
                        {
                            new SafetyEndCopy(atHighEnd: false, mirrored: false),
                            new SafetyEndCopy(atHighEnd: true, mirrored: true),
                        };
            }
        }

        /// <summary>True cuando la pieza de ese poste se dibuja en el extremo pedido.</summary>
        public static bool DrawsAt(SelectiveSafetySelection selection, int postIndex, bool highEnd)
        {
            foreach (var copy in CopiesForPost(selection, postIndex))
            {
                if (copy.AtHighEnd == highEnd)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// El lado EFECTIVO de un poste para los consumidores que aún razonan con <see cref="SafetySide"/>: conserva
        /// la pertenencia y, en un sistema de extremo bajo, mantiene la orientación pero nunca el extremo alto.
        /// </summary>
        public static SafetySide EndsForPost(SelectiveSafetySelection selection, int postIndex)
        {
            var copies = CopiesForPost(selection, postIndex);
            if (copies.Count == 0)
            {
                return SafetySide.None;
            }

            var low = false;
            var high = false;
            foreach (var copy in copies)
            {
                if (copy.AtHighEnd) high = true; else low = true;
            }

            if (low && high) return SafetySide.Both;
            return high ? SafetySide.Right : SafetySide.Left;
        }
    }
}
