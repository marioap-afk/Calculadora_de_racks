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

            var noun = Of(role);

            if (string.IsNullOrWhiteSpace(storedId))
            {
                var missing = CantileverDiagnostic.Blocking(
                    CantileverDiagnostics.SectionIdMissing,
                    "El diseno no declara la seccion " + noun + ".");
                diagnostics.Add(missing);
                return CantileverSectionResolution.Failed(default, missing);
            }

            if (!StructuralSectionId.TryParse(storedId, out var id))
            {
                var invalid = CantileverDiagnostic.Blocking(
                    CantileverDiagnostics.SectionIdInvalid,
                    "El id de seccion " + noun + " no es valido: '" + storedId + "'.");
                diagnostics.Add(invalid);
                return CantileverSectionResolution.Failed(default, invalid);
            }

            if (!catalog.TryGetById(id, out var section))
            {
                var unknown = CantileverDiagnostic.Blocking(
                    CantileverDiagnostics.SectionUnknown,
                    "El catalogo no tiene la seccion " + noun + " '" + id.Value + "'.");
                diagnostics.Add(unknown);
                return CantileverSectionResolution.Failed(id, unknown);
            }

            if (!section.IsEnabled)
            {
                diagnostics.Add(CantileverDiagnostic.Warning(
                    CantileverDiagnostics.SectionDisabled,
                    "La seccion " + noun + " '" + id.Value +
                    "' esta deshabilitada para disenos nuevos; este diseno la conserva."));
            }

            return CantileverSectionResolution.Ok(section);
        }

        /// <summary>
        /// The role's noun with its preposition attached: <c>de la columna</c>, <c>del brazo</c>.
        ///
        /// Spanish contracts <c>de</c> and <c>el</c> into <c>del</c>, so a message built as
        /// <c>"de " + noun</c> reads correctly for the two feminine nouns and produces "de el tensor" for the
        /// masculine ones. Contracting HERE keeps the four call sites identical and keeps every message the two
        /// earlier initiatives already emit byte-for-byte unchanged.
        /// </summary>
        private static string Of(CantileverMemberRole role)
        {
            var noun = Noun(role);

            return noun.StartsWith("el ", StringComparison.Ordinal)
                ? "del " + noun.Substring(3)
                : "de " + noun;
        }

        /// <summary>
        /// The noun a message uses for a role.
        ///
        /// An explicit <c>switch</c> with a throwing default. The earlier form was
        /// <c>role == Column ? "la columna" : "la base"</c>, which was true while there were exactly two roles
        /// and became a LIE the day the arm was added: a design with an unknown arm section reported that the
        /// catalogue had no section for "la base", pointing whoever read it at a field that was fine. A default
        /// that guesses a noun is worse than no message, because it is believed.
        /// </summary>
        private static string Noun(CantileverMemberRole role)
        {
            switch (role)
            {
                case CantileverMemberRole.Column:
                    return "la columna";
                case CantileverMemberRole.Base:
                    return "la base";
                case CantileverMemberRole.Arm:
                    return "el brazo";
                case CantileverMemberRole.Separator:
                    return "el separador";
                case CantileverMemberRole.Brace:
                    return "el tensor";
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(role), role, "El rol '" + role + "' no tiene sustantivo para mensajes.");
            }
        }
    }
}
