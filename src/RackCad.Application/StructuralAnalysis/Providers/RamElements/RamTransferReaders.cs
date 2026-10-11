using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace RackCad.Application.StructuralAnalysis.Providers.RamElements
{
    /// <summary>The result of reading the read-back of a build: the transfer when, and only when, the outcome is Valid.</summary>
    public sealed class RamReadbackReadResult
    {
        internal RamReadbackReadResult(StructuralExportOutcome outcome, IReadOnlyList<StructuralDiagnostic> diagnostics, StructuralTransferReadback transfer)
        {
            Outcome = outcome;
            Diagnostics = diagnostics;
            Transfer = transfer;
        }

        public StructuralExportOutcome Outcome { get; }

        public IReadOnlyList<StructuralDiagnostic> Diagnostics { get; }

        /// <summary>The transfer as read when, and only when, <see cref="Outcome"/> is Valid; otherwise <c>null</c>.</summary>
        public StructuralTransferReadback Transfer { get; }
    }

    /// <summary>
    /// The strict reader of <c>rackcad-ram-readback/v1</c> (I-65 D-11). It receives TEXT: it does no I/O.
    ///
    /// <para>
    /// There are two exact shapes: the full one (schema, status, adapter version, fingerprint, differences, values) and the minimal record that
    /// the build writes when it cannot serialise the full one (schema, status, fingerprint, error). The transfer is APPROVED only when the status
    /// is exactly <c>READBACK OK</c> and the full shape carries a version, a fingerprint and no difference. A failure status, whatever the gate
    /// that caused it, is read as a transfer that is not approved, and it keeps its differences. Any other status is Invalid.
    /// </para>
    /// </summary>
    public static class RamReadbackReader
    {
        public const string Schema = "rackcad-ram-readback/v1";

        public const string StatusPassed = "READBACK OK";

        /// <summary>The failure status of the build script, with the dash written as a character (the script writes it as an escape).</summary>
        public static readonly string StatusFailed = "FAILED " + (char)0x2014 + " DISCARD MODEL";

        private static readonly RamJsonCodes JsonCodes = new RamJsonCodes(
            RamDumpDiagnosticCodes.ReadbackJsonMalformed,
            RamDumpDiagnosticCodes.ReadbackSchemaMismatch,
            RamDumpDiagnosticCodes.ReadbackDuplicateProperty,
            RamDumpDiagnosticCodes.ReadbackUnknownProperty,
            RamDumpDiagnosticCodes.ReadbackMissingProperty,
            RamDumpDiagnosticCodes.ReadbackWrongType,
            RamDumpDiagnosticCodes.ReadbackValueInvalid);

        private static readonly string[] FullKeys = { "schema", "status", "adapterVersion", "analysisInputFingerprint", "differences", "values" };

        private static readonly string[] BackupKeys = { "schema", "status", "analysisInputFingerprint", "error" };

        public static RamReadbackReadResult Read(string json)
        {
            var scan = new RamJsonScan(JsonCodes);
            var parsed = scan.Parse(json);

            if (parsed == null)
            {
                return Result(scan, null);
            }

            var root = scan.Object(parsed, "$");

            if (root == null)
            {
                return Result(scan, null);
            }

            var schema = scan.String(root, "schema", "$");

            if (schema != null && !string.Equals(schema, Schema, StringComparison.Ordinal))
            {
                scan.Invalid(JsonCodes.Schema, "$.schema");
            }

            var status = scan.String(root, "status", "$");

            if (status != null && !string.Equals(status, StatusPassed, StringComparison.Ordinal) && !string.Equals(status, StatusFailed, StringComparison.Ordinal))
            {
                scan.Invalid(RamDumpDiagnosticCodes.ReadbackStatusUnknown, "$.status");
                return Result(scan, null);
            }

            var passed = string.Equals(status, StatusPassed, StringComparison.Ordinal);
            var backup = !passed && root.ContainsKey("error");

            scan.Keys(root, "$", backup ? BackupKeys : FullKeys);

            string adapterVersion = null;
            List<string> differences = null;

            if (backup)
            {
                var error = scan.String(root, "error", "$");
                differences = error == null ? null : new List<string> { error };
            }
            else
            {
                adapterVersion = scan.String(root, "adapterVersion", "$", nonBlank: true, allowNull: true);
                differences = scan.Strings(root, "differences", "$");

                if (root.TryGetValue("values", out var values))
                {
                    scan.Object(values, "$.values");
                }
            }

            var fingerprint = scan.String(root, "analysisInputFingerprint", "$", allowNull: true);

            if (fingerprint != null && !RamJsonScan.IsSha256(fingerprint))
            {
                scan.Invalid(JsonCodes.Value, "$.analysisInputFingerprint");
            }

            if (passed)
            {
                if (root.ContainsKey("adapterVersion") && adapterVersion == null && !scan.HasFindings)
                {
                    scan.Invalid(JsonCodes.Value, "$.adapterVersion");
                }

                if (root.ContainsKey("analysisInputFingerprint") && fingerprint == null && !scan.HasFindings)
                {
                    scan.Invalid(JsonCodes.Value, "$.analysisInputFingerprint");
                }

                if (differences != null && differences.Count > 0)
                {
                    scan.Invalid(RamDumpDiagnosticCodes.ReadbackInconsistent, "$.differences");
                }
            }

            if (scan.HasFindings || differences == null || status == null)
            {
                return Result(scan, null);
            }

            return Result(scan, new StructuralTransferReadback(passed, fingerprint, adapterVersion, differences));
        }

        private static RamReadbackReadResult Result(RamJsonScan scan, StructuralTransferReadback transfer)
        {
            var outcome = StructuralModelResult.Aggregate(scan.Diagnostics);

            return new RamReadbackReadResult(
                outcome,
                new ReadOnlyCollection<StructuralDiagnostic>(scan.Diagnostics),
                outcome == StructuralExportOutcome.Valid ? transfer : null);
        }
    }

    /// <summary>The result of reading a manifest: the provenance when, and only when, the outcome is Valid.</summary>
    public sealed class RamManifestReadResult
    {
        internal RamManifestReadResult(StructuralExportOutcome outcome, IReadOnlyList<StructuralDiagnostic> diagnostics, StructuralPackageProvenance provenance)
        {
            Outcome = outcome;
            Diagnostics = diagnostics;
            Provenance = provenance;
        }

        public StructuralExportOutcome Outcome { get; }

        public IReadOnlyList<StructuralDiagnostic> Diagnostics { get; }

        /// <summary>The provenance when, and only when, <see cref="Outcome"/> is Valid; otherwise <c>null</c>.</summary>
        public StructuralPackageProvenance Provenance { get; }
    }

    /// <summary>
    /// The strict reader of <c>rackcad-ram-build-manifest/v1</c> (I-65 D-12). It receives TEXT: it does no I/O. The shape is exactly the one
    /// the writer writes; it gives the fingerprint of the input and the version of the adapter that the package records.
    /// </summary>
    public static class RamBuildManifestReader
    {
        private static readonly RamJsonCodes JsonCodes = new RamJsonCodes(
            RamDumpDiagnosticCodes.ManifestJsonMalformed,
            RamDumpDiagnosticCodes.ManifestSchemaMismatch,
            RamDumpDiagnosticCodes.ManifestDuplicateProperty,
            RamDumpDiagnosticCodes.ManifestUnknownProperty,
            RamDumpDiagnosticCodes.ManifestMissingProperty,
            RamDumpDiagnosticCodes.ManifestWrongType,
            RamDumpDiagnosticCodes.ManifestValueInvalid);

        private static readonly string[] RootKeys =
        {
            "schema", "adapter", "target", "source", "analysisInputFingerprint", "packageFolder", "loadsFile", "mapping", "assumptions",
            "exclusions", "files", "readback",
        };

        private static readonly string[] AdapterKeys = { "name", "version" };

        private static readonly string[] TargetKeys = { "product", "productVersion", "productVersionStatus", "package", "packageVersion", "units" };

        private static readonly string[] SourceKeys = { "rackId", "kind", "stationIndex", "sourceFingerprint", "generatorVersion" };

        private static readonly string[] FileKeys = { "name", "sha256" };

        private static readonly string[] MappingKeys = { "rowsSha256", "rows" };

        private static readonly string[] RowKeys =
        {
            "sectionId", "ramSection", "materialId", "ramMaterial", "expectedMaterial", "status", "verifiedBy", "verifiedOn", "canonicalText",
        };

        private static readonly string[] SectionKeys = { "name", "table", "country" };

        private static readonly string[] ExpectedKeys = { "elasticModulus", "poissonRatio", "shearModulus", "unitWeight", "source" };

        private static readonly string[] QuantityKeys = { "value", "unit" };

        private static readonly string[] AssumptionsKeys = { "sha256", "warning", "items" };

        private static readonly string[] AssumptionKeys = { "id", "statement" };

        private static readonly string[] ReadbackKeys = { "name", "repeatsAnalysisInputFingerprint" };

        public static RamManifestReadResult Read(string json)
        {
            var scan = new RamJsonScan(JsonCodes);
            var parsed = scan.Parse(json);

            if (parsed == null)
            {
                return Result(scan, null);
            }

            var root = scan.Object(parsed, "$");

            if (root == null)
            {
                return Result(scan, null);
            }

            scan.Keys(root, "$", RootKeys);

            var schema = scan.String(root, "schema", "$");

            if (schema != null && !string.Equals(schema, RamBuildManifest.Schema, StringComparison.Ordinal))
            {
                scan.Invalid(JsonCodes.Schema, "$.schema");
            }

            string version = null;
            var adapter = scan.Child(root, "adapter", "$");

            if (adapter != null)
            {
                scan.Keys(adapter, "$.adapter", AdapterKeys);
                scan.String(adapter, "name", "$.adapter", nonBlank: true);
                version = scan.String(adapter, "version", "$.adapter", nonBlank: true);
            }

            var fingerprint = scan.String(root, "analysisInputFingerprint", "$");

            if (fingerprint != null && !RamJsonScan.IsSha256(fingerprint))
            {
                scan.Invalid(JsonCodes.Value, "$.analysisInputFingerprint");
            }

            scan.String(root, "packageFolder", "$", nonBlank: true);

            Block(scan, root, "target", TargetKeys, o =>
            {
                foreach (var key in TargetKeys)
                {
                    scan.String(o, key, "$.target");
                }
            });

            Block(scan, root, "source", SourceKeys, o =>
            {
                scan.String(o, "rackId", "$.source");
                scan.String(o, "kind", "$.source");
                scan.Integer(o, "stationIndex", "$.source", 0, int.MaxValue);
                scan.String(o, "sourceFingerprint", "$.source");
                scan.String(o, "generatorVersion", "$.source");
            });

            Block(scan, root, "loadsFile", FileKeys, o => ReadFile(scan, o, "$.loadsFile"));
            Block(scan, root, "readback", ReadbackKeys, o =>
            {
                scan.String(o, "name", "$.readback", nonBlank: true);
                scan.Bool(o, "repeatsAnalysisInputFingerprint", "$.readback");
            });

            Block(scan, root, "mapping", MappingKeys, o =>
            {
                scan.String(o, "rowsSha256", "$.mapping");
                var rows = scan.Array(o, "rows", "$.mapping");

                for (var index = 0; rows != null && index < rows.Count; index++)
                {
                    ReadRow(scan, rows[index], "$.mapping.rows[" + index + "]");
                }
            });

            Block(scan, root, "assumptions", AssumptionsKeys, o =>
            {
                scan.String(o, "sha256", "$.assumptions");
                scan.String(o, "warning", "$.assumptions");
                var items = scan.Array(o, "items", "$.assumptions");

                for (var index = 0; items != null && index < items.Count; index++)
                {
                    var path = "$.assumptions.items[" + index + "]";
                    var item = scan.Object(items[index], path);

                    if (item != null)
                    {
                        scan.Keys(item, path, AssumptionKeys);
                        scan.String(item, "id", path);
                        scan.String(item, "statement", path);
                    }
                }
            });

            scan.Strings(root, "exclusions", "$");

            var files = scan.Array(root, "files", "$");

            for (var index = 0; files != null && index < files.Count; index++)
            {
                var path = "$.files[" + index + "]";
                var file = scan.Object(files[index], path);

                if (file != null)
                {
                    scan.Keys(file, path, FileKeys);
                    ReadFile(scan, file, path);
                }
            }

            if (scan.HasFindings || fingerprint == null || version == null)
            {
                return Result(scan, null);
            }

            return Result(scan, new StructuralPackageProvenance(fingerprint, version));
        }

        private static void Block(RamJsonScan scan, RamJsonObject root, string key, string[] keys, Action<RamJsonObject> read)
        {
            var child = scan.Child(root, key, "$");

            if (child == null)
            {
                return;
            }

            scan.Keys(child, "$." + key, keys);
            read(child);
        }

        private static void ReadFile(RamJsonScan scan, RamJsonObject file, string path)
        {
            scan.String(file, "name", path, nonBlank: true);

            var hash = scan.String(file, "sha256", path);

            if (hash != null && !RamJsonScan.IsSha256(hash))
            {
                scan.Invalid(JsonCodes.Value, path + ".sha256");
            }
        }

        private static void ReadRow(RamJsonScan scan, object value, string path)
        {
            var row = scan.Object(value, path);

            if (row == null)
            {
                return;
            }

            scan.Keys(row, path, RowKeys);

            foreach (var key in new[] { "sectionId", "materialId", "ramMaterial", "status", "canonicalText" })
            {
                scan.String(row, key, path);
            }

            scan.String(row, "verifiedBy", path, allowNull: true);
            scan.String(row, "verifiedOn", path, allowNull: true);

            var section = scan.Child(row, "ramSection", path);

            if (section != null)
            {
                scan.Keys(section, path + ".ramSection", SectionKeys);

                foreach (var key in SectionKeys)
                {
                    scan.String(section, key, path + ".ramSection", allowNull: true);
                }
            }

            if (!row.TryGetValue("expectedMaterial", out var expectedValue) || expectedValue is RamJsonNull)
            {
                return;
            }

            var expected = scan.Object(expectedValue, path + ".expectedMaterial");

            if (expected == null)
            {
                return;
            }

            var expectedPath = path + ".expectedMaterial";
            scan.Keys(expected, expectedPath, ExpectedKeys);
            scan.String(expected, "source", expectedPath, allowNull: true);

            if (expected.TryGetValue("poissonRatio", out var poisson) && !(poisson is RamJsonNull))
            {
                scan.Number(expected, "poissonRatio", expectedPath);
            }

            foreach (var key in new[] { "elasticModulus", "shearModulus", "unitWeight" })
            {
                if (!expected.TryGetValue(key, out var quantityValue) || quantityValue is RamJsonNull)
                {
                    continue;
                }

                var quantity = scan.Object(quantityValue, expectedPath + "." + key);

                if (quantity != null)
                {
                    scan.Keys(quantity, expectedPath + "." + key, QuantityKeys);
                    scan.Number(quantity, "value", expectedPath + "." + key);
                    scan.String(quantity, "unit", expectedPath + "." + key);
                }
            }
        }

        private static RamManifestReadResult Result(RamJsonScan scan, StructuralPackageProvenance provenance)
        {
            var outcome = StructuralModelResult.Aggregate(scan.Diagnostics);

            return new RamManifestReadResult(
                outcome,
                new ReadOnlyCollection<StructuralDiagnostic>(scan.Diagnostics),
                outcome == StructuralExportOutcome.Valid ? provenance : null);
        }
    }
}
