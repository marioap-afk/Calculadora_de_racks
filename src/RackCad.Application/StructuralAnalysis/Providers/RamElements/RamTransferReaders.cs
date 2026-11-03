using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
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
            StructuralTransferValues transferValues = null;

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
                    var valuesObject = scan.Object(values, "$.values");

                    // A65-F4-03: the values of an APPROVED read-back are read strictly, with the exact keys the build writes; the values a
                    // failed build leaves half written are not used by anyone, so there they only have to be an object.
                    if (valuesObject != null && passed)
                    {
                        transferValues = ReadValues(scan, valuesObject);
                    }
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

            return Result(scan, new StructuralTransferReadback(passed, fingerprint, adapterVersion, differences, transferValues));
        }

        // ---------------------------------------------------------------- the values (A65-F4-03)

        private static readonly string[] ValuesKeys = { "nodes", "members", "loads", "loadCase" };

        private static readonly string[] NodeValueKeys = { "id", "ramId", "coordinates", "restraints" };

        private static readonly string[] MemberValueKeys =
        {
            "id", "ramId", "section", "material", "rotation", "cardinalPoint", "offsetI", "offsetJ", "stiffnessFactors", "axialBehavior",
            "springI", "springJ", "releases", "l1", "l2", "l3", "description",
        };

        private static readonly string[] LoadValueKeys = { "id", "member", "index", "direction", "value", "percent", "isPercentage" };

        private static readonly string[] CaseValueKeys = { "id", "category", "selfWeight" };

        /// <summary>The values the build leaves in the read-back, read with their exact keys into the neutral type; <c>null</c> when there is a finding.</summary>
        private static StructuralTransferValues ReadValues(RamJsonScan scan, RamJsonObject values)
        {
            var before = scan.Diagnostics.Count;

            scan.Keys(values, "$.values", ValuesKeys);

            var nodes = new List<StructuralTransferNodeValue>();
            var members = new List<StructuralTransferMemberValue>();
            var loads = new List<StructuralTransferLoadValue>();
            StructuralTransferCaseValue loadCase = null;

            var nodeItems = scan.Array(values, "nodes", "$.values");

            for (var index = 0; nodeItems != null && index < nodeItems.Count; index++)
            {
                var path = "$.values.nodes[" + index.ToString(CultureInfo.InvariantCulture) + "]";
                var item = scan.Object(nodeItems[index], path);

                if (item == null)
                {
                    continue;
                }

                scan.Keys(item, path, NodeValueKeys);

                var id = scan.String(item, "id", path, nonBlank: true);
                var modelId = scan.Integer(item, "ramId", path, 0, int.MaxValue);
                var coordinates = scan.Numbers(item, "coordinates", path, 3);
                var restraints = scan.Bools(item, "restraints", path, StructuralSupport.FlagCount);

                if (id != null && modelId.HasValue && coordinates != null && restraints != null)
                {
                    nodes.Add(new StructuralTransferNodeValue(
                        id,
                        modelId.Value.ToString(CultureInfo.InvariantCulture),
                        RamDumpReader.ToPosition(coordinates),
                        RamDumpReader.ToFlags(restraints)));
                }
            }

            var memberItems = scan.Array(values, "members", "$.values");

            for (var index = 0; memberItems != null && index < memberItems.Count; index++)
            {
                var path = "$.values.members[" + index.ToString(CultureInfo.InvariantCulture) + "]";
                var item = scan.Object(memberItems[index], path);

                if (item == null)
                {
                    continue;
                }

                scan.Keys(item, path, MemberValueKeys);

                var id = scan.String(item, "id", path, nonBlank: true);
                var modelId = scan.Integer(item, "ramId", path, 0, int.MaxValue);
                var section = scan.String(item, "section", path, nonBlank: true);
                var material = scan.String(item, "material", path, nonBlank: true);

                // the rest of the keys is read only to be strict about its type: the comparison does not use it
                scan.Number(item, "rotation", path);
                scan.Integer(item, "cardinalPoint", path, 0, 9);
                scan.Numbers(item, "offsetI", path, 3);
                scan.Numbers(item, "offsetJ", path, 3);
                scan.Numbers(item, "stiffnessFactors", path, 4);
                scan.Integer(item, "axialBehavior", path, 0, 3);
                scan.Number(item, "springI", path);
                scan.Number(item, "springJ", path);
                scan.Bools(item, "releases", path, StructuralMemberReleases.FlagCount);
                scan.Numbers(item, "l1", path, 3);
                scan.Numbers(item, "l2", path, 3);
                scan.Numbers(item, "l3", path, 3);
                scan.String(item, "description", path, allowNull: true);

                if (id != null && modelId.HasValue && section != null && material != null)
                {
                    members.Add(new StructuralTransferMemberValue(id, modelId.Value.ToString(CultureInfo.InvariantCulture), section, material));
                }
            }

            var loadItems = scan.Array(values, "loads", "$.values");

            for (var index = 0; loadItems != null && index < loadItems.Count; index++)
            {
                var path = "$.values.loads[" + index.ToString(CultureInfo.InvariantCulture) + "]";
                var item = scan.Object(loadItems[index], path);

                if (item == null)
                {
                    continue;
                }

                scan.Keys(item, path, LoadValueKeys);

                scan.String(item, "id", path, nonBlank: true);
                var member = scan.String(item, "member", path, nonBlank: true);
                var loadIndex = scan.Integer(item, "index", path, 0, int.MaxValue);
                var direction = scan.String(item, "direction", path);
                var value = scan.Number(item, "value", path);
                var percent = scan.Number(item, "percent", path);
                var isPercentage = scan.Bool(item, "isPercentage", path);
                var axis = direction == null ? null : RamDumpReader.DirectionOf(direction);

                if (direction != null && axis == null)
                {
                    scan.Invalid(JsonCodes.Value, path + ".direction");
                }

                if (member != null && loadIndex.HasValue && axis.HasValue && value.HasValue && percent.HasValue && isPercentage.HasValue)
                {
                    loads.Add(new StructuralTransferLoadValue(
                        member,
                        loadIndex.Value,
                        axis.Value.Axis,
                        StructuralForce.FromPoundsForce(axis.Value.Flip ? -value.Value + 0.0 : value.Value + 0.0),
                        percent.Value + 0.0,
                        isPercentage.Value));
                }
            }

            if (values.TryGetValue("loadCase", out var caseValue) && !(caseValue is RamJsonNull))
            {
                var path = "$.values.loadCase";
                var item = scan.Object(caseValue, path);

                if (item != null)
                {
                    scan.Keys(item, path, CaseValueKeys);

                    var id = scan.String(item, "id", path, nonBlank: true);
                    var category = scan.String(item, "category", path, nonBlank: true);
                    var selfWeight = scan.Numbers(item, "selfWeight", path, 3);

                    if (id != null && category != null && selfWeight != null)
                    {
                        loadCase = new StructuralTransferCaseValue(id, category);
                    }
                }
            }

            if (scan.Diagnostics.Count != before)
            {
                return null;
            }

            try
            {
                return new StructuralTransferValues(nodes, members, loads, loadCase);
            }
            catch (ArgumentException)
            {
                scan.Invalid(JsonCodes.Value, "$.values");
                return null;
            }
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
