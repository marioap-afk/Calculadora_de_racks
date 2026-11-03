using System;
using System.Collections.Generic;
using System.Globalization;
using RackCad.Application.StructuralSections;

namespace RackCad.Application.StructuralAnalysis.Providers.RamElements
{
    public enum RamSectionMapStatus
    {
        /// <summary>Not verified by an engineer: assigns nothing.</summary>
        Proposed = 1,

        /// <summary>Verified by an engineer, with verifier and date.</summary>
        Verified = 2,
    }

    /// <summary>A value with the unit it was written in. The unit is one of a closed table and is never converted here.</summary>
    public sealed class RamQuantity
    {
        public RamQuantity(double value, string unit)
        {
            Value = value;
            Unit = unit;
        }

        public double Value { get; }

        public string Unit { get; }
    }

    /// <summary>The oracle of the owner validation: the engineering values the material is expected to have, with their source.</summary>
    public sealed class RamExpectedMaterial
    {
        public RamExpectedMaterial(
            RamQuantity elasticModulus, double? poissonRatio, RamQuantity shearModulus, RamQuantity unitWeight, string source)
        {
            ElasticModulus = elasticModulus;
            PoissonRatio = poissonRatio;
            ShearModulus = shearModulus;
            UnitWeight = unitWeight;
            Source = source;
        }

        public RamQuantity ElasticModulus { get; }

        public double? PoissonRatio { get; }

        public RamQuantity ShearModulus { get; }

        public RamQuantity UnitWeight { get; }

        public string Source { get; }
    }

    /// <summary>One row of the provider table: the section and material names of the analysis program for one neutral section.</summary>
    public sealed class RamSectionMapRow
    {
        public RamSectionMapRow(
            StructuralSectionId sectionId,
            string ramSectionName,
            string ramSectionTable,
            string ramSectionCountry,
            StructuralMaterialId materialId,
            string ramMaterial,
            RamExpectedMaterial expectedMaterial,
            RamSectionMapStatus status,
            string verifiedBy,
            string verifiedOn)
        {
            SectionId = sectionId;
            RamSectionName = ramSectionName;
            RamSectionTable = ramSectionTable;
            RamSectionCountry = ramSectionCountry;
            MaterialId = materialId;
            RamMaterial = ramMaterial;
            ExpectedMaterial = expectedMaterial;
            Status = status;
            VerifiedBy = verifiedBy;
            VerifiedOn = verifiedOn;
        }

        public StructuralSectionId SectionId { get; }

        public string RamSectionName { get; }

        public string RamSectionTable { get; }

        public string RamSectionCountry { get; }

        public StructuralMaterialId MaterialId { get; }

        public string RamMaterial { get; }

        public RamExpectedMaterial ExpectedMaterial { get; }

        public RamSectionMapStatus Status { get; }

        public string VerifiedBy { get; }

        /// <summary>The verification date as written in the table (<c>yyyy-MM-dd</c>), or <c>null</c>.</summary>
        public string VerifiedOn { get; }

        public bool IsVerified => Status == RamSectionMapStatus.Verified;

        /// <summary>
        /// The COMPLETE row as length-prefixed text (<c>&lt;length&gt;:&lt;value&gt;</c> per field, <c>-</c> for an absent one):
        /// it is what travels in the neutral row and, through it, in the fingerprint. Numbers are written with the round-trip
        /// format of the invariant culture.
        /// </summary>
        public string CanonicalText
        {
            get
            {
                var fields = new List<string>
                {
                    Field(CanonicalHeader),
                    Field(SectionId.Value),
                    Field(RamSectionName),
                    Field(RamSectionTable),
                    Field(RamSectionCountry),
                    Field(MaterialId.Value),
                    Field(RamMaterial),
                };

                fields.AddRange(Quantity(ExpectedMaterial?.ElasticModulus));
                fields.Add(ExpectedMaterial?.PoissonRatio.HasValue == true
                    ? Field(ExpectedMaterial.PoissonRatio.Value.ToString("R", CultureInfo.InvariantCulture))
                    : Absent);
                fields.AddRange(Quantity(ExpectedMaterial?.ShearModulus));
                fields.AddRange(Quantity(ExpectedMaterial?.UnitWeight));
                fields.Add(Field(ExpectedMaterial?.Source));
                fields.Add(Field(Status == RamSectionMapStatus.Verified ? "verified" : "proposed"));
                fields.Add(Field(VerifiedBy));
                fields.Add(Field(VerifiedOn));

                return string.Join("|", fields);
            }
        }

        /// <summary>The neutral row this provider row projects to: verifier and date only when the row is verified.</summary>
        public StructuralMaterialRow ToNeutralRow()
        {
            var date = IsVerified && VerifiedOn != null
                ? DateTime.ParseExact(VerifiedOn, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None)
                : (DateTime?)null;

            return new StructuralMaterialRow(SectionId, MaterialId, IsVerified ? VerifiedBy : null, date, CanonicalText);
        }

        private const string CanonicalHeader = "rackcad-section-map-row/v1";
        private const string Absent = "-";

        private static string Field(string value) =>
            value == null ? Absent : value.Length.ToString(CultureInfo.InvariantCulture) + ":" + value;

        private static IEnumerable<string> Quantity(RamQuantity quantity)
        {
            if (quantity == null)
            {
                return new[] { Absent, Absent };
            }

            return new[] { Field(quantity.Value.ToString("R", CultureInfo.InvariantCulture)), Field(quantity.Unit) };
        }
    }
}
