using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RackCad.Application.Systems.Selective
{
    /// <summary>
    /// What applying (or resetting) one cabecera over the target fondos actually reached (I-43): the post it was aimed
    /// at, the fondos that took it, and the fondos that could not because that post does not exist there.
    /// <para>
    /// The omissions are reported rather than swallowed. A fondo with fewer frentes simply has fewer posts, so aiming
    /// at post 4 of a rack where one fondo stops at post 2 is a legitimate operation that lands on some fondos and not
    /// others; the user has to be told which, or they would believe a cabecera exists where none was created.
    /// </para>
    /// </summary>
    public sealed class SelectiveCabeceraApplyResult
    {
        public SelectiveCabeceraApplyResult(int postIndex, IReadOnlyList<int> appliedFondos, IReadOnlyList<int> omittedFondos)
        {
            PostIndex = postIndex;
            AppliedFondos = appliedFondos;
            OmittedFondos = omittedFondos;
        }

        /// <summary>The post the operation was aimed at.</summary>
        public int PostIndex { get; }

        /// <summary>The fondos that received the configuration (each an independent copy), ascending.</summary>
        public IReadOnlyList<int> AppliedFondos { get; }

        /// <summary>The target fondos that do not have this post, ascending. Nothing was created for them.</summary>
        public IReadOnlyList<int> OmittedFondos { get; }

        public bool Applied => AppliedFondos.Count > 0;

        /// <summary>One Spanish sentence for a status line, with fondos numbered the way the editor shows them
        /// (one-based) and the post numbered as its "Poste N" label.</summary>
        public string Describe(bool reset)
        {
            var verb = reset ? "Restablecida" : "Aplicada";
            if (!Applied)
            {
                return "El poste " + (PostIndex + 1).ToString(CultureInfo.InvariantCulture)
                    + " no existe en ningún fondo destino: no se cambió nada.";
            }

            var text = string.Format(
                CultureInfo.InvariantCulture,
                AppliedFondos.Count == 1
                    ? "{0} la cabecera del poste {1} en el fondo {2}."
                    : "{0} la cabecera del poste {1} en los fondos {2}.",
                verb,
                PostIndex + 1,
                string.Join(", ", AppliedFondos.Select(k => (k + 1).ToString(CultureInfo.InvariantCulture))));

            if (OmittedFondos.Count > 0)
            {
                text += string.Format(
                    CultureInfo.InvariantCulture,
                    OmittedFondos.Count == 1
                        ? " Se omitió el fondo {0}: no llega a ese poste."
                        : " Se omitieron los fondos {0}: no llegan a ese poste.",
                    string.Join(", ", OmittedFondos.Select(k => (k + 1).ToString(CultureInfo.InvariantCulture))));
            }

            return text;
        }
    }
}
