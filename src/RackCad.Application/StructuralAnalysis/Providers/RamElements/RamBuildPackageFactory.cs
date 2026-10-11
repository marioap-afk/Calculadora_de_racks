using System;
using System.Collections.Generic;

namespace RackCad.Application.StructuralAnalysis.Providers.RamElements
{
    /// <summary>The stable diagnostic codes of the package build. RED skeleton: the values are not assigned yet.</summary>
    public static class RamPackageDiagnosticCodes
    {
        public const string AdapterVersionInvalid = "";
        public const string FolderInvalid = "";
        public const string LoadsMissing = "";
        public const string LoadsHashMismatch = "";
        public const string MappingStale = "";
        public const string NotRepresentable = "";
    }

    /// <summary>The inputs of CreateRamBuildPackage. RED skeleton.</summary>
    public sealed class RamBuildPackageOptions
    {
        public RamBuildPackageOptions(RamSectionMaterialTable table, string adapterVersion, string packageFolder, byte[] loadFileBytes) =>
            throw new NotImplementedException();

        public RamSectionMaterialTable Table => throw new NotImplementedException();

        public string AdapterVersion => throw new NotImplementedException();

        public string PackageFolder => throw new NotImplementedException();

        public byte[] GetLoadFileBytes() => throw new NotImplementedException();
    }

    /// <summary>The content of a package. RED skeleton.</summary>
    public sealed class RamBuildPackage
    {
        public const string ManifestFileName = "";
        public const string ScriptFileName = "";
        public const string LoadFileName = "";

        public IReadOnlyList<StructuralPackageFile> Files => throw new NotImplementedException();

        public string AnalysisInputFingerprint => throw new NotImplementedException();
    }

    /// <summary>The result of building the package. RED skeleton.</summary>
    public sealed class RamBuildPackageResult
    {
        public StructuralExportOutcome Outcome => throw new NotImplementedException();

        public IReadOnlyList<StructuralDiagnostic> Diagnostics => throw new NotImplementedException();

        public RamBuildPackage Package => throw new NotImplementedException();
    }

    /// <summary>CreateRamBuildPackage(model, options) (I-65 D-08, D-13). RED skeleton: it compiles and does nothing yet.</summary>
    public static class RamBuildPackageFactory
    {
        public static RamBuildPackageResult CreateRamBuildPackage(StructuralModelResult result, RamBuildPackageOptions options) =>
            throw new NotImplementedException();

        public static RamBuildPackageResult CreateRamBuildPackage(StructuralModel model, RamBuildPackageOptions options) =>
            throw new NotImplementedException();

        public static bool IsAbsoluteNormalisedFolder(string folder) => throw new NotImplementedException();
    }
}
