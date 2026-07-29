using System;
using System.Collections.Generic;
using RackCad.Application.StructuralSections;

namespace RackCad.Application.Systems.Cantilever
{
    /// <summary>
    /// What resolving ONE section id produced: the id, the catalogue row, and why it failed if it did.
    /// </summary>
    public sealed class CantileverSectionResolution
    {
        private CantileverSectionResolution(
            StructuralSectionId sectionId,
            StructuralSectionDefinition section,
            CantileverDiagnostic failure)
        {
            SectionId = sectionId;
            Section = section;
            Failure = failure;
        }

        public StructuralSectionId SectionId { get; }

        /// <summary>The catalogue row, or null when the id did not resolve.</summary>
        public StructuralSectionDefinition Section { get; }

        /// <summary>The blocking diagnostic, or null on success.</summary>
        public CantileverDiagnostic Failure { get; }

        public bool IsResolved => Section != null;

        internal static CantileverSectionResolution Ok(StructuralSectionDefinition section) =>
            new CantileverSectionResolution(section.SectionId, section, null);

        internal static CantileverSectionResolution Failed(StructuralSectionId id, CantileverDiagnostic failure) =>
            new CantileverSectionResolution(id, null, failure);
    }

    /// <summary>
    /// THE single boundary where a Cantilever design's stored TEXT becomes a <c>StructuralSectionId</c> and
    /// a catalogue row.
    ///
    /// There is exactly one on purpose (ADR-0024, D1). The design keeps text because Domain cannot see the
    /// id type; if every consumer parsed it, "is this a valid design?" would have as many answers as call
    /// sites, and a malformed id would surface as an exception from wherever it happened to be read first.
    ///
    /// Lookup uses <c>TryGetById</c>, which resolves DISABLED sections on purpose (owner decision 15 of
    /// I-36A): a design saved months ago must keep opening. Being disabled is reported as a WARNING and
    /// never silently substituted.
    /// </summary>
    public static class CantileverSectionResolver
    {
        /// <summary>
        /// Parses and looks up one id. <paramref name="role"/> only shapes the message, never the outcome:
        /// the catalogue does not know about roles and this method does not teach it.
        /// </summary>
        public static CantileverSectionResolution Resolve(
            StructuralSectionCatalog catalog,
            string storedId,
            CantileverMemberRole role,
            ICollection<CantileverDiagnostic> diagnostics)
        {
            if (catalog == null)
            {
                throw new ArgumentNullException(nameof(catalog));
            }

            if (diagnostics == null)
            {
                throw new ArgumentNullException(nameof(diagnostics));
            }

            var noun = Noun(role);

            if (string.IsNullOrWhiteSpace(storedId))
            {
                var missing = CantileverDiagnostic.Blocking(
                    CantileverDiagnostics.SectionIdMissing,
                    "El diseno no declara la seccion de " + noun + ".");
                diagnostics.Add(missing);
                return CantileverSectionResolution.Failed(default, missing);
            }

            if (!StructuralSectionId.TryParse(storedId, out var id))
            {
                var invalid = CantileverDiagnostic.Blocking(
                    CantileverDiagnostics.SectionIdInvalid,
                    "El id de seccion de " + noun + " no es valido: '" + storedId + "'.");
                diagnostics.Add(invalid);
                return CantileverSectionResolution.Failed(default, invalid);
            }

            if (!catalog.TryGetById(id, out var section))
            {
                var unknown = CantileverDiagnostic.Blocking(
                    CantileverDiagnostics.SectionUnknown,
                    "El catalogo no tiene la seccion de " + noun + " '" + id.Value + "'.");
                diagnostics.Add(unknown);
                return CantileverSectionResolution.Failed(id, unknown);
            }

            if (!section.IsEnabled)
            {
                diagnostics.Add(CantileverDiagnostic.Warning(
                    CantileverDiagnostics.SectionDisabled,
                    "La seccion de " + noun + " '" + id.Value +
                    "' esta deshabilitada para disenos nuevos; este diseno la conserva."));
            }

            return CantileverSectionResolution.Ok(section);
        }

        private static string Noun(CantileverMemberRole role) =>
            role == CantileverMemberRole.Column ? "la columna" : "la base";
    }
}
