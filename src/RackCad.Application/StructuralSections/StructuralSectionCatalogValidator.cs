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

                var edi = identity.NormalizedEdiDesignation;

                if (byEdi.ContainsKey(edi))
                {
                    Add(issues, CatalogValidationSeverity.Error, CatalogValidationCategory.DuplicateId,
                        CodeDuplicateEdi,
                        "dos secciones normalizan a la misma designacion EDI '" + edi + "' (colision de normalizacion).",
                        id);
                }
                else
                {
                    byEdi.Add(edi, section);
                }

                if (identity.ExpectedSectionId != section.SectionId)
                {
                    Add(issues, CatalogValidationSeverity.Error, CatalogValidationCategory.InvalidReference,
                        CodeIdMismatch,
                        "el id no corresponde a su familia y designacion EDI; se esperaba '" +
                        identity.ExpectedSectionId + "'.",
                        id);
                }

                if (string.IsNullOrWhiteSpace(identity.SourceRevision))
                {
                    Add(issues, CatalogValidationSeverity.Error, CatalogValidationCategory.InvalidReference,
                        CodeMissingRevision, "la seccion no declara la revision de su fuente.", id);
                }

                if (!catalog.TryGetSource(identity.SourceId, out _))
                {
                    Add(issues, CatalogValidationSeverity.Error, CatalogValidationCategory.InvalidReference,
                        CodeUnknownSource,
                        "la seccion referencia la fuente '" + identity.SourceId + "', que no esta declarada.",
                        id);
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

                Index(byDesignation, edi, section);
                Index(byDesignation, identity.NormalizedManualLabel, section);
            }

            foreach (var pair in byDesignation.Where(entry => entry.Value.Count > 1))
            {
                Add(issues, CatalogValidationSeverity.Error, CatalogValidationCategory.DuplicateId,
                    CodeAmbiguousDesignation,
                    "la designacion '" + pair.Key + "' resuelve a " + pair.Value.Count + " secciones: " +
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

            if (string.IsNullOrWhiteSpace(manifest.SourceSha256))
            {
                Add(issues, CatalogValidationSeverity.Error, CatalogValidationCategory.Manifest,
                    CodeManifestMetadata, "el manifiesto no declara el SHA-256 del libro fuente.",
                    StructuralSectionCsvSchema.ManifestFile);
            }

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

            if (hashOf == null)
            {
                return;
            }

            var declaredFiles = (manifest.Files ?? new StructuralSectionsManifest.ManifestFile[0])
                .ToDictionary(file => file.Name, file => file.Sha256, StringComparer.Ordinal);

            foreach (var fileName in StructuralSectionCsvSchema.AllFiles())
            {
                if (!declaredFiles.TryGetValue(fileName, out var declared))
                {
                    Add(issues, CatalogValidationSeverity.Error, CatalogValidationCategory.Manifest,
                        CodeManifestHash, "el manifiesto no declara el hash de este archivo.", fileName);
                    continue;
                }

                var computed = hashOf(fileName);

                if (!string.Equals(declared, computed, StringComparison.OrdinalIgnoreCase))
                {
                    Add(issues, CatalogValidationSeverity.Error, CatalogValidationCategory.Manifest,
                        CodeManifestHash,
                        "el SHA-256 del archivo distribuido no coincide con el declarado en el manifiesto.",
                        fileName);
                }
            }

            // The manifest never hashes itself: that would be circular, and a self-hash could never be written.
            if (declaredFiles.ContainsKey(StructuralSectionCsvSchema.ManifestFile))
            {
                Add(issues, CatalogValidationSeverity.Error, CatalogValidationCategory.Manifest,
                    CodeManifestMetadata, "el manifiesto se incluye a si mismo en sus hashes.",
                    StructuralSectionCsvSchema.ManifestFile);
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
