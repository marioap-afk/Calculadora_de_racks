using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs.Validation;

namespace RackCad.Application.StructuralSections
{
    /// <summary>
    /// Checks the neutral catalog and reports; it never fixes anything, exactly like the I-19 validator.
    ///
    /// It REUSES I-19's <see cref="CatalogValidationSeverity"/> — a shared vocabulary of "how bad is this" is
    /// worth sharing — and nothing else. <c>CatalogValidator</c> is not touched: growing it into a switch over
    /// two unrelated catalogs is precisely the shape I-19 exists to avoid, and the two have different data,
    /// different keys and different failure modes.
    ///
    /// What it deliberately does NOT check: strength, capacity, buckling, deflection or any design quantity.
    /// That is deferred by ADR-0017 and is not what a catalog validator is for.
    /// </summary>
    public sealed class StructuralSectionCatalogValidator
    {
        public const string CodeDuplicateId = "SS_DUPLICATE_ID";
        public const string CodeDuplicateEdi = "SS_DUPLICATE_EDI";
        public const string CodeAmbiguousDesignation = "SS_AMBIGUOUS_DESIGNATION";
        public const string CodeIdMismatch = "SS_ID_DOES_NOT_MATCH_DESIGNATION";
        public const string CodeUnknownSource = "SS_UNKNOWN_SOURCE";
        public const string CodeMissingRevision = "SS_MISSING_REVISION";
        public const string CodeFamilyMismatch = "SS_FAMILY_MISMATCH";
        public const string CodeMissingRequiredField = "SS_MISSING_REQUIRED_FIELD";
        public const string CodeNonFiniteNumber = "SS_NON_FINITE_NUMBER";
        public const string CodeNonPositiveDimension = "SS_NON_POSITIVE_DIMENSION";
        public const string CodeNonPositiveWeight = "SS_NON_POSITIVE_WEIGHT";
        public const string CodeNonPositiveArea = "SS_NON_POSITIVE_AREA";
        public const string CodeFamilyInvariant = "SS_FAMILY_INVARIANT";
        public const string CodeThicknessNotDistinguished = "SS_HSS_THICKNESS_NOT_DISTINGUISHED";
        public const string CodeZeroInsteadOfNull = "SS_ZERO_INSTEAD_OF_NULL";
        public const string CodeManifestCount = "SS_MANIFEST_COUNT";
        public const string CodeManifestHash = "SS_MANIFEST_HASH";
        public const string CodeManifestRejectedRows = "SS_MANIFEST_REJECTED_ROWS";
        public const string CodeManifestMetadata = "SS_MANIFEST_METADATA";
        public const string CodeMaterialGradeInferred = "SS_MATERIAL_GRADE_INFERRED";
        public const string CodeInvalidIdNamespace = "SS_INVALID_ID_NAMESPACE";
        public const string CodeManifestFileSet = "SS_MANIFEST_FILE_SET";
        public const string CodeManifestDuplicateFile = "SS_MANIFEST_DUPLICATE_FILE";
        public const string CodeManifestMalformedHash = "SS_MANIFEST_MALFORMED_HASH";
        public const string CodeManifestSourceMismatch = "SS_MANIFEST_SOURCE_MISMATCH";

        /// <summary>Validates the catalog on its own, without the distributed files.</summary>
        public CatalogValidationReport Validate(StructuralSectionCatalog catalog)
        {
            if (catalog == null)
            {
                throw new ArgumentNullException(nameof(catalog));
            }

            var issues = new List<CatalogValidationIssue>();

            ValidateIdentities(catalog, issues);
            ValidateNumbers(catalog, issues);
            ValidateFamilyInvariants(catalog, issues);

            var report = new CatalogValidationReport();
            report.AddRange(issues);
            return report;
        }

        /// <summary>
        /// Validates the catalog AND the manifest's claims about the files on disk: counts, per-file SHA-256
        /// and metadata. A hash is recomputed by <paramref name="hashOf"/>, so the caller decides what "the
        /// file" is (a real path in production, a synthetic string in a test).
        /// </summary>
        public CatalogValidationReport Validate(
            StructuralSectionCatalog catalog,
            StructuralSectionsManifest manifest,
            Func<string, string> hashOf)
        {
            var report = Validate(catalog);
            var issues = new List<CatalogValidationIssue>();

            ValidateManifest(catalog, manifest, hashOf, issues);
            report.AddRange(issues);

            return report;
        }

        private static void ValidateIdentities(
            StructuralSectionCatalog catalog,
            ICollection<CatalogValidationIssue> issues)
        {
            var byId = new Dictionary<string, StructuralSectionDefinition>(StringComparer.Ordinal);
            var byEdi = new Dictionary<string, StructuralSectionDefinition>(StringComparer.Ordinal);
            var byDesignation = new Dictionary<string, List<StructuralSectionDefinition>>(StringComparer.Ordinal);

            foreach (var section in catalog.All)
            {
                var identity = section.Identity;
                var id = section.SectionId.Value;

                if (string.IsNullOrEmpty(id))
                {
                    Add(issues, CatalogValidationSeverity.Error, CatalogValidationCategory.DuplicateId,
                        CodeMissingRequiredField, "la seccion no tiene id.", identity?.ManualLabel);
                    continue;
                }

                if (byId.ContainsKey(id))
                {
                    Add(issues, CatalogValidationSeverity.Error, CatalogValidationCategory.DuplicateId,
                        CodeDuplicateId, "el id se repite en el catalogo.", id);
                }
                else
                {
                    byId.Add(id, section);
                }

                if (string.IsNullOrWhiteSpace(identity.EdiDesignation) ||
                    string.IsNullOrWhiteSpace(identity.ManualLabel))
                {
                    Add(issues, CatalogValidationSeverity.Error, CatalogValidationCategory.InvalidReference,
                        CodeMissingRequiredField, "falta la designacion EDI o la etiqueta del manual.", id);
                    continue;
                }

                // Per SOURCE, not global: two publishers may name a profile the same way, which is exactly
                // why the id carries their authority.
                var edi = identity.NormalizedEdiDesignation;
                var scopedEdi = identity.SourceId + "|" + edi;

                if (byEdi.ContainsKey(scopedEdi))
                {
                    Add(issues, CatalogValidationSeverity.Error, CatalogValidationCategory.DuplicateId,
                        CodeDuplicateEdi,
                        "dos secciones de la fuente '" + identity.SourceId +
                        "' normalizan a la misma designacion EDI '" + edi + "' (colision de normalizacion).",
                        id);
                }
                else
                {
                    byEdi.Add(scopedEdi, section);
                }

                if (string.IsNullOrWhiteSpace(identity.SourceRevision))
                {
                    Add(issues, CatalogValidationSeverity.Error, CatalogValidationCategory.InvalidReference,
                        CodeMissingRevision, "la seccion no declara la revision de su fuente.", id);
                }

                // The id is rebuilt through the authority the SOURCE declares, never through a default. A
                // section whose source is unknown cannot be checked at all, and saying so is the honest
                // outcome — silently assuming "AISC" is what ADR-0021 forbids.
                if (!catalog.TryGetSource(identity.SourceId, out var source))
                {
                    Add(issues, CatalogValidationSeverity.Error, CatalogValidationCategory.InvalidReference,
                        CodeUnknownSource,
                        "la seccion referencia la fuente '" + identity.SourceId + "', que no esta declarada; " +
                        "sin ella no se puede reconstruir su id.",
                        id);
                }
                else if (!StructuralSectionId.IsValidNamespace(source.IdNamespace))
                {
                    Add(issues, CatalogValidationSeverity.Error, CatalogValidationCategory.InvalidReference,
                        CodeInvalidIdNamespace,
                        "la fuente '" + source.SourceId + "' declara un namespace de id invalido: '" +
                        (source.IdNamespace ?? "<null>") + "'.",
                        id);
                }
                else
                {
                    var expected = identity.ExpectedSectionId(source.IdNamespace);

                    if (expected != section.SectionId)
                    {
                        Add(issues, CatalogValidationSeverity.Error, CatalogValidationCategory.InvalidReference,
                            CodeIdMismatch,
                            "el id no corresponde al namespace '" + source.IdNamespace +
                            "' de su fuente, su familia y su designacion EDI; se esperaba '" + expected + "'.",
                            id);
                    }

                    if (!string.Equals(
                            identity.SourceRevision, source.Revision, StringComparison.Ordinal))
                    {
                        Add(issues, CatalogValidationSeverity.Error, CatalogValidationCategory.InvalidReference,
                            CodeMissingRevision,
                            "la seccion declara la revision '" + identity.SourceRevision +
                            "' y su fuente declara '" + source.Revision + "'.",
                            id);
                    }
                }

                if (section.Dimensions == null || section.Dimensions.Family != identity.Family)
                {
                    Add(issues, CatalogValidationSeverity.Error, CatalogValidationCategory.InvalidReference,
                        CodeFamilyMismatch,
                        "las dimensiones no corresponden a la familia declarada.", id);
                }

                if (!string.IsNullOrEmpty(section.MaterialGrade) &&
                    string.Equals(identity.SourceId, StructuralSectionSource.AiscShapesId, StringComparison.Ordinal))
                {
                    Add(issues, CatalogValidationSeverity.Warning, CatalogValidationCategory.InvalidReference,
                        CodeMaterialGradeInferred,
                        "la seccion trae grado de material y la base AISC no lo publica: no debe inferirse.", id);
                }

                Index(byDesignation, scopedEdi, section);
                Index(byDesignation, identity.SourceId + "|" + identity.NormalizedManualLabel, section);
            }

            foreach (var pair in byDesignation.Where(entry => entry.Value.Count > 1))
            {
                Add(issues, CatalogValidationSeverity.Error, CatalogValidationCategory.DuplicateId,
                    CodeAmbiguousDesignation,
                    "dentro de una misma fuente, la designacion '" + pair.Key + "' resuelve a " +
                    pair.Value.Count + " secciones: " +
                    string.Join(", ", pair.Value.Select(section => section.SectionId.Value)) + ".",
                    pair.Key);
            }
        }

        private static void ValidateNumbers(
            StructuralSectionCatalog catalog,
            ICollection<CatalogValidationIssue> issues)
        {
            foreach (var section in catalog.All)
            {
                var id = section.SectionId.Value;

                if (double.IsNaN(section.WeightPerLength) || double.IsInfinity(section.WeightPerLength))
                {
                    Add(issues, CatalogValidationSeverity.Error, CatalogValidationCategory.InvalidReference,
                        CodeNonFiniteNumber, "el peso lineal no es finito.", id);
                }
                else if (section.WeightPerLength <= 0)
                {
                    Add(issues, CatalogValidationSeverity.Error, CatalogValidationCategory.InvalidReference,
                        CodeNonPositiveWeight, "el peso lineal debe ser positivo.", id);
                }

                var properties = section.Properties ?? StructuralSectionProperties.Empty;

                if (!properties.Area.HasValue)
                {
                    Add(issues, CatalogValidationSeverity.Error, CatalogValidationCategory.InvalidReference,
                        CodeMissingRequiredField, "la seccion no declara area.", id);
                }
                else if (properties.Area.Value <= 0)
                {
                    Add(issues, CatalogValidationSeverity.Error, CatalogValidationCategory.InvalidReference,
                        CodeNonPositiveArea, "el area debe ser positiva.", id);
                }

                foreach (var pair in NumericValues(section))
                {
                    if (!pair.Value.HasValue)
                    {
                        continue;
                    }

                    var value = pair.Value.Value;

                    if (double.IsNaN(value) || double.IsInfinity(value))
                    {
                        Add(issues, CatalogValidationSeverity.Error, CatalogValidationCategory.InvalidReference,
                            CodeNonFiniteNumber, "'" + pair.Key + "' no es un numero finito.", id);
                        continue;
                    }

                    if (!MustBeStrictlyPositive(pair.Key))
                    {
                        continue;
                    }

                    if (value == 0)
                    {
                        // For a magnitude, zero is never a published value: AISC prints an en dash for "not
                        // applicable" and the importer turns that into null. A zero therefore means a value was
                        // LOST in parsing — the exact failure a tolerant reader hides.
                        Add(issues, CatalogValidationSeverity.Error, CatalogValidationCategory.InvalidReference,
                            CodeZeroInsteadOfNull,
                            "'" + pair.Key + "' vale cero; una magnitud ausente debe ser nula, no cero.", id);
                    }
                    else if (value < 0)
                    {
                        Add(issues, CatalogValidationSeverity.Error, CatalogValidationCategory.InvalidReference,
                            CodeNonPositiveDimension,
                            "'" + pair.Key + "' debe ser positivo y vale " + pair.Value.Value + ".", id);
                    }
                }
            }
        }

        /// <summary>
        /// Which published quantities cannot physically be zero or negative.
        ///
        /// The exceptions are POSITIONS and an angle, not magnitudes: <c>x</c>, <c>y</c>, <c>eo</c>, <c>xp</c>,
        /// <c>yp</c> and the single-angle points <c>zA/zB/zC</c>, <c>wA/wB/wC</c> are measured from a datum, so
        /// zero is a legitimate value — and it really occurs: <c>zB</c> is exactly 0 for every equal-leg angle,
        /// because point B sits on the z axis. Flagging those would turn 61 correct rows into false errors.
        /// </summary>
        private static bool MustBeStrictlyPositive(string variable)
        {
            switch (variable)
            {
                case "x":
                case "y":
                case "eo":
                case "xp":
                case "yp":
                case "zA":
                case "zB":
                case "zC":
                case "wA":
                case "wB":
                case "wC":
                case "tanAlpha":
                    return false;
                default:
                    return true;
            }
        }

        private static void ValidateFamilyInvariants(
            StructuralSectionCatalog catalog,
            ICollection<CatalogValidationIssue> issues)
        {
            foreach (var section in catalog.All)
            {
                var id = section.SectionId.Value;

                switch (section.Dimensions)
                {
                    case WSectionDimensions w:
                        RequireAll(issues, id, "W",
                            ("d", w.Depth), ("bf", w.FlangeWidth), ("tw", w.WebThickness), ("tf", w.FlangeThickness));
                        break;

                    case HssRectangularSectionDimensions hss:
                        RequireAll(issues, id, "HSS-RECT",
                            ("Ht", hss.OverallDepth), ("B", hss.OverallWidth),
                            ("tnom", hss.NominalThickness), ("tdes", hss.DesignThickness));

                        if (hss.NominalThickness.HasValue && hss.DesignThickness.HasValue &&
                            hss.DesignThickness.Value > hss.NominalThickness.Value)
                        {
                            Add(issues, CatalogValidationSeverity.Error, CatalogValidationCategory.InvalidReference,
                                CodeThicknessNotDistinguished,
                                "el espesor de diseno supera al nominal, lo que invierte su relacion fisica.", id);
                        }

                        if (hss.FlatDepth.HasValue && hss.OverallDepth.HasValue &&
                            hss.FlatDepth.Value >= hss.OverallDepth.Value)
                        {
                            Add(issues, CatalogValidationSeverity.Error, CatalogValidationCategory.InvalidReference,
                                CodeFamilyInvariant,
                                "la pared plana 'h' no puede igualar ni superar el peralte total 'Ht'.", id);
                        }

                        if (hss.FlatWidth.HasValue && hss.OverallWidth.HasValue &&
                            hss.FlatWidth.Value >= hss.OverallWidth.Value)
                        {
                            Add(issues, CatalogValidationSeverity.Error, CatalogValidationCategory.InvalidReference,
                                CodeFamilyInvariant,
                                "la pared plana 'b' no puede igualar ni superar el ancho total 'B'.", id);
                        }

                        break;

                    case ChannelSectionDimensions channel:
                        RequireAll(issues, id, "C",
                            ("d", channel.Depth), ("bf", channel.FlangeWidth),
                            ("tw", channel.WebThickness), ("tf", channel.FlangeThickness));
                        break;

                    case AngleSectionDimensions angle:
                        RequireAll(issues, id, "L",
                            ("d", angle.ShortLeg), ("b", angle.LongLeg), ("t", angle.Thickness));

                        if (angle.ShortLeg.HasValue && angle.LongLeg.HasValue &&
                            angle.ShortLeg.Value > angle.LongLeg.Value)
                        {
                            Add(issues, CatalogValidationSeverity.Error, CatalogValidationCategory.InvalidReference,
                                CodeFamilyInvariant,
                                "el ala corta 'd' supera al ala larga 'b': las columnas estan intercambiadas.", id);
                        }

                        break;
                }
            }
        }

        private static void ValidateManifest(
            StructuralSectionCatalog catalog,
            StructuralSectionsManifest manifest,
            Func<string, string> hashOf,
            ICollection<CatalogValidationIssue> issues)
        {
            if (manifest == null)
            {
                Add(issues, CatalogValidationSeverity.Error, CatalogValidationCategory.Manifest,
                    CodeManifestMetadata, "no hay manifiesto.", StructuralSectionCsvSchema.ManifestFile);
                return;
            }

            if (!string.Equals(manifest.SchemaVersion, StructuralSectionsManifest.CurrentSchemaVersion, StringComparison.Ordinal))
            {
                Add(issues, CatalogValidationSeverity.Error, CatalogValidationCategory.Manifest,
                    CodeManifestMetadata,
                    "la version de esquema del manifiesto ('" + manifest.SchemaVersion + "') no es la vigente ('" +
                    StructuralSectionsManifest.CurrentSchemaVersion + "').",
                    StructuralSectionCsvSchema.ManifestFile);
            }

            ValidateManifestMetadata(catalog, manifest, issues);

            if (manifest.RejectedSelectedRows != 0)
            {
                Add(issues, CatalogValidationSeverity.Error, CatalogValidationCategory.Manifest,
                    CodeManifestRejectedRows,
                    "la importacion rechazo " + manifest.RejectedSelectedRows +
                    " filas de una familia seleccionada; deben ser cero.",
                    StructuralSectionCsvSchema.ManifestFile);
            }

            var actual = catalog.CountsByFamily();

            foreach (var family in StructuralSectionFamilies.All)
            {
                var token = StructuralSectionFamilies.ToToken(family);
                var declared = manifest.CountsByFamily != null && manifest.CountsByFamily.TryGetValue(token, out var value)
                    ? value
                    : -1;

                if (declared != actual[family])
                {
                    Add(issues, CatalogValidationSeverity.Error, CatalogValidationCategory.Manifest,
                        CodeManifestCount,
                        "el manifiesto declara " + declared + " secciones de la familia " + token +
                        " y el catalogo distribuido trae " + actual[family] + ".",
                        token);
                }
            }

            if (manifest.TotalCount != catalog.Count)
            {
                Add(issues, CatalogValidationSeverity.Error, CatalogValidationCategory.Manifest,
                    CodeManifestCount,
                    "el manifiesto declara " + manifest.TotalCount + " secciones en total y el catalogo trae " +
                    catalog.Count + ".",
                    StructuralSectionCsvSchema.ManifestFile);
            }

            ValidateManifestFileSet(manifest, hashOf, issues);
        }

        /// <summary>
        /// Validates the status overlay ON ITS OWN TERMS: it is a local decision, not source data, so it never
        /// touches a hash. What it must satisfy is that every id it names exists and that it names none twice —
        /// an overlay pointing at a section that no longer exists would look like it worked and disable
        /// nothing.
        /// </summary>
        public CatalogValidationReport ValidateOverlay(
            StructuralSectionCatalog catalog,
            IReadOnlyList<StructuralSectionStatusOverride> overrides)
        {
            if (catalog == null)
            {
                throw new ArgumentNullException(nameof(catalog));
            }

            var issues = new List<CatalogValidationIssue>();
            var seen = new HashSet<string>(StringComparer.Ordinal);

            foreach (var entry in overrides ?? new StructuralSectionStatusOverride[0])
            {
                var id = entry.SectionId.Value;

                if (!seen.Add(id))
                {
                    Add(issues, CatalogValidationSeverity.Error, CatalogValidationCategory.DuplicateId,
                        CodeDuplicateId, "el overlay de estado declara el id mas de una vez.", id);
                    continue;
                }

                if (!catalog.TryGetById(entry.SectionId, out _))
                {
                    Add(issues, CatalogValidationSeverity.Error, CatalogValidationCategory.InvalidReference,
                        CodeUnknownSource,
                        "el overlay de estado referencia un id que no existe en el catalogo.", id);
                }
            }

            var report = new CatalogValidationReport();
            report.AddRange(issues);
            return report;
        }

        /// <summary>
        /// Every metadata field a consumer would otherwise take on faith: the catalog it claims to be, the
        /// source and revision it claims to come from, the id authority it was built with, the worksheet it
        /// was read from, the mapping version and a workbook hash that is actually a SHA-256.
        ///
        /// The point is not paranoia about the file RackCad ships — it is that a catalog folder can be
        /// replaced in a deployed installation, and a loader that only checks row counts would accept a
        /// different source's data as if it were this one.
        /// </summary>
        private static void ValidateManifestMetadata(
            StructuralSectionCatalog catalog,
            StructuralSectionsManifest manifest,
            ICollection<CatalogValidationIssue> issues)
        {
            var at = StructuralSectionCsvSchema.ManifestFile;

            RequireText(issues, at, "catalogId", manifest.CatalogId);
            RequireText(issues, at, "sourceId", manifest.SourceId);
            RequireText(issues, at, "sourceRevision", manifest.SourceRevision);
            RequireText(issues, at, "sourceWorksheet", manifest.SourceWorksheet);
            RequireText(issues, at, "mapperVersion", manifest.MapperVersion);
            RequireText(issues, at, "sourceFileName", manifest.SourceFileName);
            RequireText(issues, at, "idNamespace", manifest.IdNamespace);

            if (manifest.CatalogId != null &&
                !string.Equals(
                    manifest.CatalogId,
                    StructuralSectionsManifest.StructuralSectionsCatalogId,
                    StringComparison.Ordinal))
            {
                Add(issues, CatalogValidationSeverity.Error, CatalogValidationCategory.Manifest,
                    CodeManifestMetadata,
                    "el manifiesto dice ser del catalogo '" + manifest.CatalogId + "' y no de '" +
                    StructuralSectionsManifest.StructuralSectionsCatalogId + "'.",
                    at);
            }

            if (!IsSha256(manifest.SourceSha256))
            {
                Add(issues, CatalogValidationSeverity.Error, CatalogValidationCategory.Manifest,
                    CodeManifestMalformedHash,
                    "el SHA-256 del libro fuente no son exactamente 64 caracteres hexadecimales.",
                    at);
            }

            if (manifest.IdNamespace != null && !StructuralSectionId.IsValidNamespace(manifest.IdNamespace))
            {
                Add(issues, CatalogValidationSeverity.Error, CatalogValidationCategory.Manifest,
                    CodeInvalidIdNamespace,
                    "el manifiesto declara el namespace de id invalido '" + manifest.IdNamespace + "'.",
                    at);
            }

            // The mapper version is checked by VALUE, not merely for presence: a different mapping produces
            // different columns, so a version this build does not know means a file whose meaning is not
            // guaranteed.
            if (manifest.MapperVersion != null &&
                !string.Equals(
                    manifest.MapperVersion,
                    StructuralSectionsManifest.SupportedMapperVersion,
                    StringComparison.Ordinal))
            {
                Add(issues, CatalogValidationSeverity.Error, CatalogValidationCategory.Manifest,
                    CodeManifestMetadata,
                    "el manifiesto declara la version de mapeo '" + manifest.MapperVersion +
                    "' y este build solo soporta '" + StructuralSectionsManifest.SupportedMapperVersion + "'.",
                    at);
            }

            // The worksheet has to be the one this source and revision imply. A non-empty but wrong name
            // describes a workbook that is not the one the rest of the metadata claims.
            if (manifest.SourceWorksheet != null &&
                StructuralSectionSource.TryExpectedWorksheet(
                    manifest.SourceId, manifest.SourceRevision, out var expectedWorksheet) &&
                !string.Equals(manifest.SourceWorksheet, expectedWorksheet, StringComparison.Ordinal))
            {
                Add(issues, CatalogValidationSeverity.Error, CatalogValidationCategory.Manifest,
                    CodeManifestMetadata,
                    "el manifiesto dice haber leido la hoja '" + manifest.SourceWorksheet + "', y la fuente '" +
                    manifest.SourceId + "' en la revision '" + manifest.SourceRevision +
                    "' solo puede venir de '" + expectedWorksheet + "'.",
                    at);
            }

            ValidateSingleSource(catalog, manifest, issues);
        }

        /// <summary>
        /// Schema 1.0 of the manifest describes EXACTLY ONE source, and this checks that the catalog it
        /// accompanies is that catalog: one source, the one the manifest names, with its revision and its id
        /// authority, and every section belonging to it.
        ///
        /// The MODEL is deliberately multi-source —that is what the id namespace exists for— but the
        /// distribution format v1.0 is not. A second source, even one no section uses, produces a catalog
        /// this manifest cannot describe, and accepting it would mean shipping a file whose declared counts
        /// and hashes say nothing about half of what is there.
        /// </summary>
        private static void ValidateSingleSource(
            StructuralSectionCatalog catalog,
            StructuralSectionsManifest manifest,
            ICollection<CatalogValidationIssue> issues)
        {
            var at = StructuralSectionCsvSchema.ManifestFile;

            if (catalog.Sources.Count != 1)
            {
                Add(issues, CatalogValidationSeverity.Error, CatalogValidationCategory.Manifest,
                    CodeManifestSourceMismatch,
                    "el esquema " + StructuralSectionsManifest.CurrentSchemaVersion +
                    " del manifiesto describe EXACTAMENTE UNA fuente y el catalogo distribuido declara " +
                    catalog.Sources.Count + ": " +
                    string.Join(", ", catalog.Sources.Select(source => source.SourceId)) + ".",
                    at);
                return;
            }

            var only = catalog.Sources[0];

            if (!string.Equals(only.SourceId, manifest.SourceId, StringComparison.Ordinal))
            {
                Add(issues, CatalogValidationSeverity.Error, CatalogValidationCategory.Manifest,
                    CodeManifestSourceMismatch,
                    "el manifiesto declara la fuente '" + manifest.SourceId +
                    "' y el catalogo distribuido declara '" + only.SourceId + "'.",
                    at);
            }

            if (!string.Equals(only.Revision, manifest.SourceRevision, StringComparison.Ordinal))
            {
                Add(issues, CatalogValidationSeverity.Error, CatalogValidationCategory.Manifest,
                    CodeManifestSourceMismatch,
                    "el manifiesto declara la revision '" + manifest.SourceRevision +
                    "' y la fuente distribuida declara '" + only.Revision + "'.",
                    at);
            }

            if (!string.Equals(only.IdNamespace, manifest.IdNamespace, StringComparison.Ordinal))
            {
                Add(issues, CatalogValidationSeverity.Error, CatalogValidationCategory.Manifest,
                    CodeManifestSourceMismatch,
                    "el manifiesto declara el namespace de id '" + manifest.IdNamespace +
                    "' y la fuente distribuida declara '" + only.IdNamespace + "'.",
                    at);
            }

            foreach (var section in catalog.All)
            {
                if (!string.Equals(section.Identity.SourceId, only.SourceId, StringComparison.Ordinal))
                {
                    Add(issues, CatalogValidationSeverity.Error, CatalogValidationCategory.Manifest,
                        CodeManifestSourceMismatch,
                        "la seccion declara la fuente '" + section.Identity.SourceId +
                        "' y el catalogo distribuido solo declara '" + only.SourceId + "'.",
                        section.SectionId.Value);
                    break;
                }
            }
        }

        /// <summary>
        /// The declared file set has to be EXACTLY the immutable one: no missing file, no unexpected file, no
        /// repeated name, every hash well formed and every hash right. The overlay is deliberately absent, and
        /// the manifest never hashes itself.
        /// </summary>
        private static void ValidateManifestFileSet(
            StructuralSectionsManifest manifest,
            Func<string, string> hashOf,
            ICollection<CatalogValidationIssue> issues)
        {
            var declared = manifest.Files ?? new StructuralSectionsManifest.ManifestFile[0];
            var expected = new HashSet<string>(StructuralSectionCsvSchema.ImmutableFiles(), StringComparer.Ordinal);
            var seen = new Dictionary<string, string>(StringComparer.Ordinal);

            foreach (var file in declared)
            {
                if (string.IsNullOrEmpty(file.Name))
                {
                    Add(issues, CatalogValidationSeverity.Error, CatalogValidationCategory.Manifest,
                        CodeManifestFileSet, "el manifiesto declara un archivo sin nombre.",
                        StructuralSectionCsvSchema.ManifestFile);
                    continue;
                }

                if (seen.ContainsKey(file.Name))
                {
                    Add(issues, CatalogValidationSeverity.Error, CatalogValidationCategory.Manifest,
                        CodeManifestDuplicateFile,
                        "el manifiesto declara el archivo dos veces.", file.Name);
                    continue;
                }

                seen.Add(file.Name, file.Sha256);

                if (string.Equals(file.Name, StructuralSectionCsvSchema.ManifestFile, StringComparison.Ordinal))
                {
                    Add(issues, CatalogValidationSeverity.Error, CatalogValidationCategory.Manifest,
                        CodeManifestMetadata, "el manifiesto se incluye a si mismo en sus hashes.", file.Name);
                    continue;
                }

                if (string.Equals(file.Name, StructuralSectionCsvSchema.StatusFile, StringComparison.Ordinal))
                {
                    Add(issues, CatalogValidationSeverity.Error, CatalogValidationCategory.Manifest,
                        CodeManifestFileSet,
                        "el manifiesto hashea el overlay de estado, que es EDITABLE: una edicion legitima " +
                        "invalidaria los datos importados.",
                        file.Name);
                    continue;
                }

                if (!expected.Contains(file.Name))
                {
                    Add(issues, CatalogValidationSeverity.Error, CatalogValidationCategory.Manifest,
                        CodeManifestFileSet,
                        "el manifiesto declara un archivo que no forma parte del catalogo inmutable.",
                        file.Name);
                    continue;
                }

                if (!IsSha256(file.Sha256))
                {
                    Add(issues, CatalogValidationSeverity.Error, CatalogValidationCategory.Manifest,
                        CodeManifestMalformedHash,
                        "el hash declarado no son exactamente 64 caracteres hexadecimales.", file.Name);
                }
            }

            foreach (var fileName in StructuralSectionCsvSchema.ImmutableFiles())
            {
                if (!seen.TryGetValue(fileName, out var declaredHash))
                {
                    Add(issues, CatalogValidationSeverity.Error, CatalogValidationCategory.Manifest,
                        CodeManifestFileSet, "el manifiesto no declara este archivo del catalogo inmutable.",
                        fileName);
                    continue;
                }

                if (hashOf == null || !IsSha256(declaredHash))
                {
                    continue;
                }

                if (!string.Equals(declaredHash, hashOf(fileName), StringComparison.OrdinalIgnoreCase))
                {
                    Add(issues, CatalogValidationSeverity.Error, CatalogValidationCategory.Manifest,
                        CodeManifestHash,
                        "el SHA-256 del archivo distribuido no coincide con el declarado en el manifiesto.",
                        fileName);
                }
            }
        }

        private static bool IsSha256(string value)
        {
            if (value == null || value.Length != 64)
            {
                return false;
            }

            foreach (var ch in value)
            {
                var isHex = (ch >= '0' && ch <= '9') || (ch >= 'a' && ch <= 'f') || (ch >= 'A' && ch <= 'F');

                if (!isHex)
                {
                    return false;
                }
            }

            return true;
        }

        private static void RequireText(
            ICollection<CatalogValidationIssue> issues,
            string at,
            string field,
            string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                Add(issues, CatalogValidationSeverity.Error, CatalogValidationCategory.Manifest,
                    CodeManifestMetadata, "el manifiesto no declara '" + field + "'.", at);
            }
        }

        private static void RequireAll(
            ICollection<CatalogValidationIssue> issues,
            string id,
            string family,
            params (string Name, double? Value)[] required)
        {
            foreach (var entry in required)
            {
                if (!entry.Value.HasValue)
                {
                    Add(issues, CatalogValidationSeverity.Error, CatalogValidationCategory.InvalidReference,
                        CodeFamilyInvariant,
                        "la familia " + family + " exige la dimension '" + entry.Name + "' y la seccion no la trae.",
                        id);
                }
            }
        }

        /// <summary>Every optional numeric value of a section, keyed by its published variable name.</summary>
        private static IEnumerable<KeyValuePair<string, double?>> NumericValues(StructuralSectionDefinition section)
        {
            foreach (var pair in Dimensions(section))
            {
                yield return pair;
            }

            var p = section.Properties ?? StructuralSectionProperties.Empty;

            yield return new KeyValuePair<string, double?>("A", p.Area);
            yield return new KeyValuePair<string, double?>("Ix", p.Ix);
            yield return new KeyValuePair<string, double?>("Zx", p.Zx);
            yield return new KeyValuePair<string, double?>("Sx", p.Sx);
            yield return new KeyValuePair<string, double?>("rx", p.Rx);
            yield return new KeyValuePair<string, double?>("Iy", p.Iy);
            yield return new KeyValuePair<string, double?>("Zy", p.Zy);
            yield return new KeyValuePair<string, double?>("Sy", p.Sy);
            yield return new KeyValuePair<string, double?>("ry", p.Ry);
            yield return new KeyValuePair<string, double?>("J", p.J);
            yield return new KeyValuePair<string, double?>("Iz", p.Iz);
            yield return new KeyValuePair<string, double?>("rz", p.Rz);
            yield return new KeyValuePair<string, double?>("Sz", p.Sz);
            yield return new KeyValuePair<string, double?>("Iw", p.Iw);
            yield return new KeyValuePair<string, double?>("Cw", p.Cw);
            yield return new KeyValuePair<string, double?>("C", p.HssTorsionalConstant);
            yield return new KeyValuePair<string, double?>("ro", p.Ro);
            yield return new KeyValuePair<string, double?>("rts", p.Rts);
            yield return new KeyValuePair<string, double?>("ho", p.Ho);
            yield return new KeyValuePair<string, double?>("PA", p.PA);
            yield return new KeyValuePair<string, double?>("PA2", p.PA2);
            yield return new KeyValuePair<string, double?>("PB", p.PB);
            yield return new KeyValuePair<string, double?>("PC", p.PC);
            yield return new KeyValuePair<string, double?>("PD", p.PD);
            yield return new KeyValuePair<string, double?>("tanAlpha", p.TanAlpha);
            yield return new KeyValuePair<string, double?>("H", p.FlexuralConstantH);
            yield return new KeyValuePair<string, double?>("Wno", p.Wno);
            yield return new KeyValuePair<string, double?>("Sw1", p.Sw1);
            yield return new KeyValuePair<string, double?>("Sw2", p.Sw2);
            yield return new KeyValuePair<string, double?>("Sw3", p.Sw3);
            yield return new KeyValuePair<string, double?>("Qf", p.Qf);
            yield return new KeyValuePair<string, double?>("Qw", p.Qw);
            yield return new KeyValuePair<string, double?>("zA", p.ZA);
            yield return new KeyValuePair<string, double?>("zB", p.ZB);
            yield return new KeyValuePair<string, double?>("zC", p.ZC);
            yield return new KeyValuePair<string, double?>("wA", p.WA);
            yield return new KeyValuePair<string, double?>("wB", p.WB);
            yield return new KeyValuePair<string, double?>("wC", p.WC);
            yield return new KeyValuePair<string, double?>("SwA", p.SwA);
            yield return new KeyValuePair<string, double?>("SwB", p.SwB);
            yield return new KeyValuePair<string, double?>("SwC", p.SwC);
            yield return new KeyValuePair<string, double?>("SzA", p.SzA);
            yield return new KeyValuePair<string, double?>("SzB", p.SzB);
            yield return new KeyValuePair<string, double?>("SzC", p.SzC);
        }

        /// <summary>The geometric measurements of a section, keyed by their published variable name.</summary>
        private static IEnumerable<KeyValuePair<string, double?>> Dimensions(StructuralSectionDefinition section)
        {
            switch (section.Dimensions)
            {
                case WSectionDimensions w:
                    yield return new KeyValuePair<string, double?>("d", w.Depth);
                    yield return new KeyValuePair<string, double?>("ddet", w.DetailingDepth);
                    yield return new KeyValuePair<string, double?>("bf", w.FlangeWidth);
                    yield return new KeyValuePair<string, double?>("bfdet", w.DetailingFlangeWidth);
                    yield return new KeyValuePair<string, double?>("tw", w.WebThickness);
                    yield return new KeyValuePair<string, double?>("twdet", w.DetailingWebThickness);
                    yield return new KeyValuePair<string, double?>("twdet_2", w.HalfDetailingWebThickness);
                    yield return new KeyValuePair<string, double?>("tf", w.FlangeThickness);
                    yield return new KeyValuePair<string, double?>("tfdet", w.DetailingFlangeThickness);
                    yield return new KeyValuePair<string, double?>("kdes", w.KDesign);
                    yield return new KeyValuePair<string, double?>("kdet", w.KDetailing);
                    yield return new KeyValuePair<string, double?>("k1", w.K1);
                    yield return new KeyValuePair<string, double?>("T", w.DistanceBetweenFilletToes);
                    yield return new KeyValuePair<string, double?>("WGi", w.WorkableGageInner);
                    yield return new KeyValuePair<string, double?>("WGo", w.WorkableGageOuter);
                    break;

                case HssRectangularSectionDimensions hss:
                    yield return new KeyValuePair<string, double?>("Ht", hss.OverallDepth);
                    yield return new KeyValuePair<string, double?>("B", hss.OverallWidth);
                    yield return new KeyValuePair<string, double?>("h", hss.FlatDepth);
                    yield return new KeyValuePair<string, double?>("b", hss.FlatWidth);
                    yield return new KeyValuePair<string, double?>("tnom", hss.NominalThickness);
                    yield return new KeyValuePair<string, double?>("tdes", hss.DesignThickness);
                    break;

                case ChannelSectionDimensions channel:
                    yield return new KeyValuePair<string, double?>("d", channel.Depth);
                    yield return new KeyValuePair<string, double?>("ddet", channel.DetailingDepth);
                    yield return new KeyValuePair<string, double?>("bf", channel.FlangeWidth);
                    yield return new KeyValuePair<string, double?>("bfdet", channel.DetailingFlangeWidth);
                    yield return new KeyValuePair<string, double?>("tw", channel.WebThickness);
                    yield return new KeyValuePair<string, double?>("twdet", channel.DetailingWebThickness);
                    yield return new KeyValuePair<string, double?>("twdet_2", channel.HalfDetailingWebThickness);
                    yield return new KeyValuePair<string, double?>("tf", channel.FlangeThickness);
                    yield return new KeyValuePair<string, double?>("tfdet", channel.DetailingFlangeThickness);
                    yield return new KeyValuePair<string, double?>("kdes", channel.KDesign);
                    yield return new KeyValuePair<string, double?>("kdet", channel.KDetailing);
                    yield return new KeyValuePair<string, double?>("T", channel.DistanceBetweenFilletToes);
                    yield return new KeyValuePair<string, double?>("WGi", channel.WorkableGageInner);

                    // POSITIONS relative to a designated edge: checked for finiteness, not for sign
                    // (see MustBeStrictlyPositive).
                    yield return new KeyValuePair<string, double?>("x", channel.CentroidX);
                    yield return new KeyValuePair<string, double?>("eo", channel.ShearCenterX);
                    yield return new KeyValuePair<string, double?>("xp", channel.PlasticNeutralAxisX);
                    break;

                case AngleSectionDimensions angle:
                    yield return new KeyValuePair<string, double?>("d", angle.ShortLeg);
                    yield return new KeyValuePair<string, double?>("b", angle.LongLeg);
                    yield return new KeyValuePair<string, double?>("t", angle.Thickness);
                    yield return new KeyValuePair<string, double?>("kdes", angle.KDesign);
                    yield return new KeyValuePair<string, double?>("kdet", angle.KDetailing);
                    yield return new KeyValuePair<string, double?>("x", angle.CentroidX);
                    yield return new KeyValuePair<string, double?>("y", angle.CentroidY);
                    yield return new KeyValuePair<string, double?>("xp", angle.PlasticNeutralAxisX);
                    yield return new KeyValuePair<string, double?>("yp", angle.PlasticNeutralAxisY);
                    break;
            }
        }

        private static void Index(
            IDictionary<string, List<StructuralSectionDefinition>> index,
            string key,
            StructuralSectionDefinition section)
        {
            if (!index.TryGetValue(key, out var bucket))
            {
                bucket = new List<StructuralSectionDefinition>(1);
                index.Add(key, bucket);
            }

            if (!bucket.Contains(section))
            {
                bucket.Add(section);
            }
        }

        private static void Add(
            ICollection<CatalogValidationIssue> issues,
            CatalogValidationSeverity severity,
            CatalogValidationCategory category,
            string code,
            string message,
            string location)
        {
            issues.Add(new CatalogValidationIssue(severity, category, code, message, location));
        }
    }
}
