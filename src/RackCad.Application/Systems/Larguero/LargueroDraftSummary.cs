using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.StructuralSections;

namespace RackCad.Application.Systems.Larguero
{
    /// <summary>One line of the summary of a larguero: what it is and its value.</summary>
    public sealed record LargueroSummaryLine(string Label, string Value);

    /// <summary>One line of the bill of materials of the summary: the component, then its pieces (the recipe of <see cref="LargueroBomBuilder"/>).</summary>
    public sealed record LargueroBomPreviewLine(string Component, string Description, string LengthText, string QuantityText, bool IsPiece);

    /// <summary>
    /// The summary of a larguero in the making on the page «Nuevo › Larguero» (I-64 A-14, line D, quinta tanda, part «lg», a draft in
    /// construction: LG-03 of the Owner, «Déjalo WIP»): the section and its catalog, the peralte, the length to cut, the ménsula, the weight of
    /// an AISC section, the name it would take (LG-05 of the prototype, accepted: «Larguero 96 in · P3»), the bill of materials with the one
    /// recipe of the larguero (<see cref="LargueroBomBuilder.Component"/>: a profile and 2 ménsulas), and what is still missing. Pure: it saves
    /// nothing and draws nothing.
    /// </summary>
    public sealed class LargueroDraftSummary
    {
        public const string NoSectionText = "Elige una sección de la lista.";
        public const string NoPeralteText = "Elige el peralte del larguero.";
        public const string NoLengthText = "Escribe una longitud a corte mayor que 0 (en pulgadas).";
        public const string NoMensulaText = "Elige la ménsula.";
        public const string AiscMensulaText = "Elige la ménsula: una sección AISC no trae ménsula en el catálogo.";
        public const string UnknownMensulaText = "La ménsula elegida no está en el catálogo (mensulas.csv).";

        private LargueroDraftSummary(string proposedName, IReadOnlyList<LargueroSummaryLine> lines, IReadOnlyList<LargueroBomPreviewLine> bom,
            IReadOnlyList<string> missing)
        {
            ProposedName = proposedName;
            Lines = lines;
            Bom = bom;
            Missing = missing;
        }

        /// <summary>The name the larguero would take: «Larguero 96 in · P3», or «Larguero 96 in · C10X15.3» with an AISC section; empty while the
        /// section, its peralte or the length are missing.</summary>
        public string ProposedName { get; }

        public IReadOnlyList<LargueroSummaryLine> Lines { get; }

        public IReadOnlyList<LargueroBomPreviewLine> Bom { get; }

        /// <summary>What is still missing, in the order of the page; empty when the larguero is complete.</summary>
        public IReadOnlyList<string> Missing { get; }

        public bool IsComplete => Missing.Count == 0;

        /// <param name="section">The section chosen; null when none.</param>
        /// <param name="peralte">The peralte chosen for a larguero of RackCad (one of its peraltes); ignored for an AISC section, whose depth it is.</param>
        /// <param name="length">The length to cut, in inches; null when what was written is not a number greater than 0.</param>
        /// <param name="mensulaId">The ménsula chosen (an id of mensulas.csv); null or blank when none.</param>
        /// <param name="catalog">The catalog of RackCad, for the ménsulas and the recipe of the larguero.</param>
        public static LargueroDraftSummary Of(LargueroSectionOption section, double? peralte, double? length, string mensulaId, RackCatalog catalog)
        {
            var lines = new List<LargueroSummaryLine>();
            var missing = new List<string>();
            var mensula = string.IsNullOrWhiteSpace(mensulaId)
                ? null
                : catalog?.Mensulas.FirstOrDefault(m => string.Equals(m?.Id, mensulaId.Trim(), StringComparison.OrdinalIgnoreCase));

            if (section == null)
            {
                missing.Add(NoSectionText);
                if (length == null) missing.Add(NoLengthText);
                if (mensula == null) missing.Add(string.IsNullOrWhiteSpace(mensulaId) ? NoMensulaText : UnknownMensulaText);
                return new LargueroDraftSummary(string.Empty, lines, Array.Empty<LargueroBomPreviewLine>(), missing);
            }

            lines.Add(new LargueroSummaryLine("Sección", section.Name + " · " + section.TypeLabel));
            lines.Add(new LargueroSummaryLine("Catálogo", section.CatalogLabel + ": «" + section.SourceLabel + "» (" + section.Id + ")"));

            double? effectivePeralte;
            if (section.HasPeralteChoice)
            {
                effectivePeralte = peralte.HasValue && section.Peraltes.Contains(peralte.Value) ? peralte : null;
                if (effectivePeralte == null) missing.Add(NoPeralteText);
                lines.Add(new LargueroSummaryLine("Peralte", effectivePeralte == null ? "falta" : LargueroSectionNaming.Number(effectivePeralte.Value) + " in"));
            }
            else
            {
                effectivePeralte = section.Depth;
                lines.Add(new LargueroSummaryLine("Peralte", section.Depth.HasValue
                    ? LargueroSectionNaming.Number(section.Depth.Value) + " in, el de la sección (del catálogo)"
                    : "el catálogo no da el peralte de esta sección"));
            }

            if (length == null) missing.Add(NoLengthText);
            lines.Add(new LargueroSummaryLine("Longitud a corte", length == null ? "falta" : LargueroSectionNaming.Number(length.Value) + " in"));

            if (mensula == null)
            {
                missing.Add(!string.IsNullOrWhiteSpace(mensulaId) ? UnknownMensulaText
                    : section.Source == LargueroSectionSource.Aisc ? AiscMensulaText : NoMensulaText);
                lines.Add(new LargueroSummaryLine("Ménsula", "falta"));
            }
            else
            {
                lines.Add(new LargueroSummaryLine("Ménsula", MensulaText(mensula)
                    + (string.Equals(mensula.Id, section.DefaultMensulaId, StringComparison.OrdinalIgnoreCase) ? " · la de su larguero en el catálogo" : string.Empty)));
            }

            if (section.Aisc != null)
            {
                var weight = section.WeightText;
                if (length != null)
                {
                    var pounds = StructuralSectionUnits.WeightInPounds(section.Aisc, length.Value);
                    var kilograms = StructuralSectionUnits.WeightInKilograms(section.Aisc, length.Value);
                    weight += " · " + pounds.ToString("0.#", CultureInfo.InvariantCulture) + " lb (" + kilograms.ToString("0.#", CultureInfo.InvariantCulture)
                              + " kg) a " + LargueroSectionNaming.Number(length.Value) + " in";
                }

                lines.Add(new LargueroSummaryLine("Peso del perfil", weight));
            }
            else if (section.Gauge != null)
            {
                lines.Add(new LargueroSummaryLine("Calibre", section.Gauge));
            }

            var name = effectivePeralte == null || length == null
                ? string.Empty
                : "Larguero " + LargueroSectionNaming.Number(length.Value) + " in · "
                  + (section.Source == LargueroSectionSource.Aisc ? section.Name : "P" + LargueroSectionNaming.Number(effectivePeralte.Value));

            return new LargueroDraftSummary(name, lines, BomOf(section, effectivePeralte, length, mensula, catalog), missing);
        }

        /// <summary>«Ménsula 3 remaches de cinta calibre 10 · 6 in de alto · calibre 10».</summary>
        public static string MensulaText(MensulaCatalogEntry mensula)
        {
            if (mensula == null) return string.Empty;
            var parts = new List<string> { mensula.Label };
            if (mensula.Height > 0.0) parts.Add(LargueroSectionNaming.Number(mensula.Height) + " in de alto");
            if (!string.IsNullOrWhiteSpace(mensula.Gauge)) parts.Add("calibre " + mensula.Gauge.Trim());
            return string.Join(" · ", parts);
        }

        // The bill of materials, with THE recipe of a larguero (LargueroBomBuilder.Component, the one Selectivo uses too): the quantities, the
        // categories and the lengths come from it; the descriptions are the names of this page (the section by its type, the ménsula). The
        // larguero is described as the classic BOM describes it (LargueroBomBuilder.DescribeBeam): its section and « · P<peralte>».
        private static IReadOnlyList<LargueroBomPreviewLine> BomOf(LargueroSectionOption section, double? peralte, double? length,
            MensulaCatalogEntry mensula, RackCatalog catalog)
        {
            var component = LargueroBomBuilder.Component(catalog, section.Id, length ?? 0.0, peralte ?? 0.0, mensula?.Id, quantity: 1);
            var lengthText = length == null ? "—" : LargueroSectionNaming.Number(component.Length);
            var described = section.Source == LargueroSectionSource.RackCad && peralte.HasValue
                ? section.Name + " · P" + LargueroSectionNaming.Number(peralte.Value)
                : section.Name;
            var lines = new List<LargueroBomPreviewLine>
            {
                new LargueroBomPreviewLine(component.Category, described, lengthText, component.Quantity.ToString(CultureInfo.InvariantCulture), IsPiece: false)
            };

            foreach (var piece in component.Pieces)
            {
                var isMensula = piece.Category == LargueroBomBuilder.Mensula;
                // With no ménsula chosen the recipe would fall back to the one of the row of a larguero of RackCad: the page lists none.
                if (isMensula && mensula == null) continue;
                lines.Add(new LargueroBomPreviewLine(piece.Category,
                    isMensula ? mensula?.Label ?? piece.Description : section.Name,
                    isMensula ? "—" : lengthText,
                    piece.Quantity.ToString(CultureInfo.InvariantCulture) + " por larguero", IsPiece: true));
            }

            return lines;
        }
    }
}
