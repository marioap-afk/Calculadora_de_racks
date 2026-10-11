using System;
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

        /// <summary>The COMPLETE row as length-prefixed text: it is what travels in the neutral row and in the fingerprint.</summary>
        public string CanonicalText => throw new NotImplementedException();

        /// <summary>The neutral row this provider row projects to.</summary>
        public StructuralMaterialRow ToNeutralRow() => throw new NotImplementedException();
    }
}
