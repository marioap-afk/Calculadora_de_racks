using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using RackCad.Application.StructuralAnalysis;
using RackCad.Application.StructuralAnalysis.Providers.RamElements;
using RackCad.Application.StructuralSections;
using Xunit;
using C = RackCad.Tests.StructuralAnalysisCantileverFixtures;

namespace RackCad.Tests
{
    /// <summary>
    /// The inputs of the I-65 F3-T1 tests. Every verified row built here is a FIXTURE declared as such (verifier
    /// <c>TEST-FIXTURE</c>, source <c>TEST-FIXTURE-SOURCE</c>): none is an engineering verification, and the values are not
    /// engineering values.
    /// </summary>
    internal static class StructuralAnalysisRamElementsFixtures
    {
        public const string DataFile = "assets/structural-analysis/ram-elements/section-material-map.json";
        public const string ColumnSection = C.ColumnSection;
        public const string ArmSection = C.ArmSection;
        public const string ColumnMaterial = "ASTM-A992-GR50";
        public const string ArmMaterial = "ASTM-A500-GRC";
        public const string ColumnRamSection = "W 12X26";
        public const string ArmRamSection = "HSS_SQR 4X4X1_4";
        public const string ColumnRamMaterial = "A992 Gr50";
        public const string ArmRamMaterial = "A500 GrC rectangular";
        public const string Verifier = "TEST-FIXTURE";
        public const string Source = "TEST-FIXTURE-SOURCE";
        public const string Schema = "rackcad-ram-elements-section-map/v1";

        public static string DataFileText() => File.ReadAllText(Path.Combine(C.RepoRoot(), DataFile.Replace('/', Path.DirectorySeparatorChar)));

        /// <summary>A VERIFIED fixture row; fixture numbers, not engineering values.</summary>
        public static JsonObject Row(string sectionId, string ramName, string materialId, string ramMaterial) =>
            (JsonObject)JsonNode.Parse(
                "{ \"sectionId\": \"" + sectionId + "\","
                + " \"ramSection\": { \"name\": \"" + ramName + "\", \"table\": \"TEST-FIXTURE-TABLE\", \"country\": \"TEST-FIXTURE-COUNTRY\" },"
                + " \"materialId\": \"" + materialId + "\","
                + " \"ramMaterial\": \"" + ramMaterial + "\","
                + " \"expectedMaterial\": { \"elasticModulus\": { \"value\": 111.5, \"unit\": \"ksi\" }, \"poissonRatio\": 0.25, \"shearModulus\": null,"
                + " \"unitWeight\": { \"value\": 222.5, \"unit\": \"pcf\" }, \"source\": \"" + Source + "\" },"
                + " \"status\": \"verified\", \"verifiedBy\": \"" + Verifier + "\", \"verifiedOn\": \"2026-10-11\" }");

        public static JsonObject ColumnRow() => Row(ColumnSection, ColumnRamSection, ColumnMaterial, ColumnRamMaterial);

        public static JsonObject ArmRow() => Row(ArmSection, ArmRamSection, ArmMaterial, ArmRamMaterial);

        public static string Document(params JsonObject[] rows)
        {
            var array = new JsonArray();

            foreach (var row in rows)
            {
                array.Add(row);
            }

            var root = new JsonObject { ["schema"] = Schema, ["rows"] = array };
            return root.ToJsonString();
        }

        /// <summary>The document of the two rows of REF-CANT-01, both verified as FIXTURES.</summary>
        public static string RefDocument() => Document(ColumnRow(), ArmRow());

        public static RamSectionMapReadResult Read(string json) => RamSectionMapReader.Read(json);

        public static RamSectionMaterialTable Table(string json)
        {
            var result = RamSectionMapReader.Read(json);
            Assert.True(
                result.Outcome == StructuralExportOutcome.Valid && result.Table != null,
                "Se esperaba una tabla Valid; resultado " + result.Outcome + ": " + Codes(result.Diagnostics));
            return result.Table;
        }

        public static RamSectionMaterialTable RefTable() => Table(RefDocument());

        public static string Codes(IEnumerable<StructuralDiagnostic> diagnostics) =>
            string.Join(", ", diagnostics.Select(d => d.Code + "(" + d.Subject + ")"));

        public static StructuralModelResult BuildRef(RamSectionMaterialTable table) =>
            C.Build(C.Design(), profile: table);

        public static StructuralModel ValidRefModel(RamSectionMaterialTable table)
        {
            var result = BuildRef(table);
            Assert.True(
                result.Outcome == StructuralExportOutcome.Valid && result.Model != null,
                "Se esperaba Valid; resultado " + result.Outcome + ": " + C.Describe(result));
            return result.Model;
        }

        // ---------------------------------------------------------------- a synthetic model (arbitrary geometry, loads and supports)

        public sealed class SyntheticMember
        {
            public string Id = "M1";
            public string NodeI = "N1";
            public string NodeJ = "N2";
            public string Section = ColumnSection;
            public string Material = ColumnMaterial;
            public StructuralDirection Peralte = new StructuralDirection(1, 0, 0);
            public bool[] Releases = new bool[StructuralMemberReleases.FlagCount];
        }

        /// <summary>A model assembled directly from its public types, so a test controls every value.</summary>
        public static StructuralModel Synthetic(
            IEnumerable<StructuralNode> nodes,
            IEnumerable<SyntheticMember> members,
            IEnumerable<StructuralSupport> supports,
            IEnumerable<StructuralMemberPointLoad> loads,
            IEnumerable<StructuralDiagnostic> diagnostics = null)
        {
            var list = members.ToList();
            var sections = list.Select(m => m.Section).Distinct(StringComparer.Ordinal)
                .Select(id => new StructuralSection(StructuralSectionId.Parse(id), null, null, null, null)).ToList();
            var materials = list.Select(m => m.Material).Distinct(StringComparer.Ordinal)
                .Select(id => new StructuralMaterial(StructuralMaterialId.Create(id))).ToList();

            return new StructuralModel(
                new StructuralModelMetadata(
                    new StructuralSourceRef("RACK", "Cantilever", 0, "source-fingerprint"), "test-generator", "load-sha", "mapping-sha", "assumptions-sha"),
                nodes,
                list.Select(m => new StructuralMember(
                    m.Id,
                    m.NodeI,
                    m.NodeJ,
                    StructuralSectionId.Parse(m.Section),
                    StructuralMaterialId.Create(m.Material),
                    m.Peralte,
                    new StructuralMemberReleases(m.Releases, "CON-01"),
                    StructuralMemberRole.Column,
                    "PIECE")),
                sections,
                materials,
                supports,
                loads,
                new[] { new AnalysisAssumption("TEST-01", "A test assumption.") },
                diagnostics ?? new StructuralDiagnostic[0]);
        }

        public static StructuralNode Node(string id, double x, double y, double z) =>
            new StructuralNode(id, new StructuralPosition(StructuralLength.FromInches(x), StructuralLength.FromInches(y), StructuralLength.FromInches(z)));

        public static StructuralSupport Support(string nodeId, params bool[] flags) =>
            new StructuralSupport(nodeId, flags, "SUP-TEST");

        public static StructuralMemberPointLoad Load(string id, string memberId, double poundsForce, double distanceInches) =>
            new StructuralMemberPointLoad(id, memberId, StructuralForce.FromPoundsForce(poundsForce), StructuralLength.FromInches(distanceInches));

        /// <summary>The smallest valid synthetic model: a vertical column, 100 in long, in the neutral frame (Z up).</summary>
        public static StructuralModel Column(IEnumerable<StructuralSupport> supports = null, IEnumerable<StructuralMemberPointLoad> loads = null) =>
            Synthetic(
                new[] { Node("N1", 0, 0, 0), Node("N2", 0, 0, 100) },
                new[] { new SyntheticMember() },
                supports ?? new[] { Support("N1", true, true, true, true, true, true), Support("N2", true, false, false, false, true, true) },
                loads ?? new StructuralMemberPointLoad[0]);

        public static RamSectionMaterialTable ColumnTable() => Table(Document(ColumnRow()));
    }
}
