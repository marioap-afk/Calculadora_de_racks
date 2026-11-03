using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace RackCad.Application.StructuralAnalysis.Providers.RamElements
{
    /// <summary>A file of the package other than the manifest, with the SHA-256 of its bytes.</summary>
    public sealed class RamManifestFile
    {
        public RamManifestFile(string name, string sha256)
        {
            Name = name;
            Sha256 = sha256;
        }

        public string Name { get; }

        public string Sha256 { get; }
    }

    /// <summary>
    /// The manifest of provenance of the package (I-65 D-12, CD-I65-F3-09): schema <c>rackcad-ram-build-manifest/v1</c>. Deterministic
    /// JSON, UTF-8 and LF, without a clock, a machine name or a user name; the only path it carries is the folder of the package.
    /// It says what the package was built from and what it leaves out, and it never claims that anything is validated.
    /// </summary>
    public static class RamBuildManifest
    {
        public const string Schema = "rackcad-ram-build-manifest/v1";
        public const string AdapterName = "rackcad-ram-elements-adapter";
        public const string ProductName = "RAM Elements";

        /// <summary>The version of the product that the measurements were taken on. It is DECLARED, not verified by the script.</summary>
        public const string DeclaredProductVersion = "26.00.01.015";

        public const string ProductVersionStatus = "declared, not verified";
        public const string PackageName = "ramelements";
        public const string ReadbackFileName = "ram-readback.json";

        /// <summary>The warning of CD-I65-F3-09 (Q-O3, ADR-0050 section 5), literal.</summary>
        public const string AssumptionsWarning =
            "These assumptions are hypotheses for transfer equivalence only; they are not a structural validation.";

        /// <summary>What V1 does not model (CD-I65-F3-09, section 1 of the F3 document), literal.</summary>
        public static IReadOnlyList<string> Exclusions { get; } = new ReadOnlyCollection<string>(new[]
        {
            "The base, the plates, the gussets, the spacers, the bracing and the round bars are not modelled as elements (SUP-02, IDL-06, CD-I65-F1-14).",
            "No self weight, no load combinations, no nodal loads, no distributed loads and no local loads.",
            "No springs, no rigid end offsets and no releases (D-02, D-09).",
        });

        public static string Create(
            StructuralModel model,
            RamBuildPlan plan,
            string adapterVersion,
            string packageFolder,
            string analysisInputFingerprint,
            string loadFileName,
            string loadFileSha256,
            IEnumerable<RamManifestFile> files)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            if (plan == null)
            {
                throw new ArgumentNullException(nameof(plan));
            }

            if (files == null)
            {
                throw new ArgumentNullException(nameof(files));
            }

            var metadata = model.Metadata;
            var writer = new RamJsonWriter(asciiOnly: false, indented: true);

            writer.BeginObject();
            writer.Property("schema", Schema);

            writer.Name("adapter").BeginObject();
            writer.Property("name", AdapterName);
            writer.Property("version", adapterVersion);
            writer.EndObject();

            writer.Name("target").BeginObject();
            writer.Property("product", ProductName);
            writer.Property("productVersion", DeclaredProductVersion);
            writer.Property("productVersionStatus", ProductVersionStatus);
            writer.Property("package", PackageName);
            writer.Property("packageVersion", RamBuildPayload.RequiredPackageVersion);
            writer.Property("units", plan.Units);
            writer.EndObject();

            writer.Name("source").BeginObject();
            writer.Property("rackId", metadata.SourceRef.RackId);
            writer.Property("kind", metadata.SourceRef.Kind);
            writer.Name("stationIndex").Integer(metadata.SourceRef.StationIndex);
            writer.Property("sourceFingerprint", metadata.SourceRef.SourceFingerprint);
            writer.Property("generatorVersion", metadata.GeneratorVersion);
            writer.EndObject();

            writer.Property("analysisInputFingerprint", analysisInputFingerprint);
            writer.Property("packageFolder", packageFolder);

            writer.Name("loadsFile").BeginObject();
            writer.Property("name", loadFileName);
            writer.Property("sha256", loadFileSha256);
            writer.EndObject();

            writer.Name("mapping").BeginObject();
            writer.Property("rowsSha256", metadata.MappingRowsSha256);
            writer.Name("rows").BeginArray();

            foreach (var row in plan.UsedRows)
            {
                WriteRow(writer, row);
            }

            writer.EndArray();
            writer.EndObject();

            writer.Name("assumptions").BeginObject();
            writer.Property("sha256", metadata.AssumptionsSha256);
            writer.Property("warning", AssumptionsWarning);
            writer.Name("items").BeginArray();

            foreach (var assumption in model.Assumptions.OrderBy(a => a.Id, StringComparer.Ordinal))
            {
                writer.BeginObject();
                writer.Property("id", assumption.Id);
                writer.Property("statement", assumption.Statement);
                writer.EndObject();
            }

            writer.EndArray();
            writer.EndObject();

            writer.Name("exclusions").StringArray(Exclusions);

            writer.Name("files").BeginArray();

            foreach (var file in files)
            {
                writer.BeginObject();
                writer.Property("name", file.Name);
                writer.Property("sha256", file.Sha256);
                writer.EndObject();
            }

            writer.EndArray();

            writer.Name("readback").BeginObject();
            writer.Property("name", ReadbackFileName);
            writer.Property("repeatsAnalysisInputFingerprint", true);
            writer.EndObject();

            writer.EndObject();

            return writer.ToString() + "\n";
        }

        private static void WriteRow(RamJsonWriter writer, RamSectionMapRow row)
        {
            writer.BeginObject();
            writer.Property("sectionId", row.SectionId.Value);

            writer.Name("ramSection").BeginObject();
            writer.Property("name", row.RamSectionName);
            writer.Property("table", row.RamSectionTable);
            writer.Property("country", row.RamSectionCountry);
            writer.EndObject();

            writer.Property("materialId", row.MaterialId.Value);
            writer.Property("ramMaterial", row.RamMaterial);

            writer.Name("expectedMaterial");

            if (row.ExpectedMaterial == null)
            {
                writer.Null();
            }
            else
            {
                writer.BeginObject();
                WriteQuantity(writer, "elasticModulus", row.ExpectedMaterial.ElasticModulus);

                writer.Name("poissonRatio");

                if (row.ExpectedMaterial.PoissonRatio.HasValue)
                {
                    writer.Number(row.ExpectedMaterial.PoissonRatio.Value);
                }
                else
                {
                    writer.Null();
                }

                WriteQuantity(writer, "shearModulus", row.ExpectedMaterial.ShearModulus);
                WriteQuantity(writer, "unitWeight", row.ExpectedMaterial.UnitWeight);
                writer.Property("source", row.ExpectedMaterial.Source);
                writer.EndObject();
            }

            writer.Property("status", row.IsVerified ? "verified" : "proposed");
            writer.Property("verifiedBy", row.VerifiedBy);
            writer.Property("verifiedOn", row.VerifiedOn);
            writer.Property("canonicalText", row.CanonicalText);
            writer.EndObject();
        }

        private static void WriteQuantity(RamJsonWriter writer, string name, RamQuantity quantity)
        {
            writer.Name(name);

            if (quantity == null)
            {
                writer.Null();
                return;
            }

            writer.BeginObject();
            writer.Property("value", quantity.Value);
            writer.Property("unit", quantity.Unit);
            writer.EndObject();
        }
    }
}
