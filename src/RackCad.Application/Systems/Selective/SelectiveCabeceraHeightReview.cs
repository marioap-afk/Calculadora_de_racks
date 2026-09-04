using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Domain.Systems.Selective;

namespace RackCad.Application.Systems.Selective
{
    /// <summary>Qué tan grave es lo que la altura de una cabecera provoca en un fondo concreto.</summary>
    public enum SelectiveCabeceraHeightIssue
    {
        /// <summary>La cabecera queda por DEBAJO del nivel de carga superior: el larguero o la tarima sobresaldrían.</summary>
        Severe,

        /// <summary>Difiere del alto resuelto de ese poste: el frontal y el corte lateral pueden dejar de coincidir.</summary>
        Informative
    }

    /// <summary>Un hallazgo concreto: qué le pasa a esta altura en un fondo determinado.</summary>
    public sealed class SelectiveCabeceraHeightFinding
    {
        internal SelectiveCabeceraHeightFinding(int fondoIndex, SelectiveCabeceraHeightIssue issue, double reference)
        {
            FondoIndex = fondoIndex;
            Issue = issue;
            Reference = reference;
        }

        public int FondoIndex { get; }

        public SelectiveCabeceraHeightIssue Issue { get; }

        /// <summary>El valor contra el que se comparó: el nivel superior (severa) o el alto resuelto (informativa).</summary>
        public double Reference { get; }
    }

    /// <summary>
    /// Revisa la altura de una cabecera personalizada contra TODOS los fondos destino (I-43, gate 8.6E).
    /// <para>
    /// Una misma receta puede ser válida en un fondo, discrepante en otro y peligrosa en un tercero: cada fondo tiene
    /// su propia topología y sus propias alturas. Validar solo el fondo VISIBLE deja pasar en silencio justo el caso
    /// que el usuario no puede ver.
    /// </para>
    /// <para>
    /// Es una lectura PURA sobre el sistema ya resuelto: no decide cabeceras, no muta nada y no crea una autoridad
    /// nueva — consulta la que ya existe (<see cref="SelectiveDepthLayout.BaysOfFondo"/> y
    /// <see cref="SelectivePostGeometry"/>). Un destino que no tiene ese poste se OMITE: ni se crea, ni se recorta, ni
    /// bloquea a los demás.
    /// </para>
    /// </summary>
    public sealed class SelectiveCabeceraHeightReview
    {
        /// <summary>Pulgadas por debajo de las cuales dos alturas se consideran la misma. Criterio preexistente.</summary>
        private const double Tolerance = 0.5;

        private SelectiveCabeceraHeightReview(
            IReadOnlyList<SelectiveCabeceraHeightFinding> findings,
            IReadOnlyList<int> skippedFondos,
            int postIndex,
            double height)
        {
            Findings = findings;
            SkippedFondos = skippedFondos;
            PostIndex = postIndex;
            Height = height;
        }

        public IReadOnlyList<SelectiveCabeceraHeightFinding> Findings { get; }

        /// <summary>Fondos destino que NO tienen ese poste, por lo que no se revisaron.</summary>
        public IReadOnlyList<int> SkippedFondos { get; }

        public int PostIndex { get; }

        public double Height { get; }

        /// <summary>Hay al menos un fondo donde la cabecera queda por debajo del nivel de carga superior.</summary>
        public bool HasSevere => Findings.Any(f => f.Issue == SelectiveCabeceraHeightIssue.Severe);

        public bool HasInformative => Findings.Any(f => f.Issue == SelectiveCabeceraHeightIssue.Informative);

        public bool IsClean => Findings.Count == 0;

        /// <summary>
        /// Revisa <paramref name="height"/> en el poste <paramref name="postIndex"/> de cada fondo de
        /// <paramref name="targetFondos"/>. Los criterios numéricos son los que ya usaba el editor, aplicados ahora a
        /// cada destino en vez de solo al visible.
        /// </summary>
        public static SelectiveCabeceraHeightReview Of(
            SelectiveRackSystem system,
            IEnumerable<int> targetFondos,
            int postIndex,
            double height)
        {
            var findings = new List<SelectiveCabeceraHeightFinding>();
            var skipped = new List<int>();
            if (system == null || targetFondos == null)
            {
                return new SelectiveCabeceraHeightReview(findings, skipped, postIndex, height);
            }

            foreach (var k in targetFondos.Distinct().OrderBy(k => k))
            {
                var bays = SelectiveDepthLayout.BaysOfFondo(system, k);

                // Un fondo de C frentes tiene postes 0..C. Fuera de ese rango el poste no existe ALLI: se omite, que
                // es distinto de estar bien — el llamador lo reporta aparte.
                if (bays == null || bays.Count == 0 || postIndex < 0 || postIndex > bays.Count)
                {
                    skipped.Add(k);
                    continue;
                }

                var top = SelectivePostGeometry.TopLevelYAtPost(bays, postIndex);
                if (top > 0.0 && height < top - Tolerance)
                {
                    findings.Add(new SelectiveCabeceraHeightFinding(k, SelectiveCabeceraHeightIssue.Severe, top));
                    continue; // una severa absorbe a la informativa del mismo fondo: es el mismo problema, peor
                }

                var resolved = SelectivePostGeometry.PostHeight(bays, postIndex, SelectivePostGeometry.FallbackHeight(bays, system.Height));
                if (resolved > 0.0 && Math.Abs(height - resolved) > Tolerance)
                {
                    findings.Add(new SelectiveCabeceraHeightFinding(k, SelectiveCabeceraHeightIssue.Informative, resolved));
                }
            }

            return new SelectiveCabeceraHeightReview(findings, skipped, postIndex, height);
        }

        /// <summary>
        /// UN solo mensaje para todos los fondos implicados: qué altura se pidió, dónde queda por debajo del nivel de
        /// carga y dónde solo difiere del alto resuelto. Abrir un diálogo por fondo convertiría una revisión en una
        /// ristra de avisos que nadie lee.
        /// </summary>
        public string Describe()
        {
            if (IsClean) return string.Empty;

            var text = "Altura de la cabecera del poste " + One(PostIndex) + ": "
                + Inches(Height) + ".";

            var severe = Findings.Where(f => f.Issue == SelectiveCabeceraHeightIssue.Severe).ToList();
            if (severe.Count > 0)
            {
                text += "\n\nPor debajo del nivel de carga superior en: " + string.Join(", ", severe.Select(Where)) + "."
                    + "\nEl larguero o la tarima superiores sobresaldrían por encima del poste.";
            }

            var info = Findings.Where(f => f.Issue == SelectiveCabeceraHeightIssue.Informative).ToList();
            if (info.Count > 0)
            {
                text += "\n\nDifiere del alto resuelto en: " + string.Join(", ", info.Select(Where)) + "."
                    + "\nEl frontal coloca los largueros para el alto resuelto, así que el corte lateral y el frontal "
                    + "pueden dejar de coincidir.";
            }

            if (SkippedFondos.Count > 0)
            {
                text += "\n\nSin ese poste (se omiten): " + string.Join(", ", SkippedFondos.Select(One)) + ".";
            }

            return text;
        }

        private static string Where(SelectiveCabeceraHeightFinding f) => One(f.FondoIndex) + " (" + Inches(f.Reference) + ")";

        /// <summary>Los fondos se numeran desde 1 de cara al usuario, como en el resto del editor.</summary>
        private static string One(int index) => "F" + (index + 1).ToString(CultureInfo.InvariantCulture);

        private static string Inches(double value) => value.ToString("0.##", CultureInfo.InvariantCulture) + " in";
    }
}
