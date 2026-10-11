using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

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

    /// <summary>The manifest of provenance of the package (I-65 D-12, CD-I65-F3-09). RED skeleton: it compiles and does nothing yet.</summary>
    public static class RamBuildManifest
    {
        public const string Schema = "";
        public const string AdapterName = "";
        public const string ProductName = "";
        public const string DeclaredProductVersion = "";
        public const string ProductVersionStatus = "";
        public const string PackageName = "";
        public const string ReadbackFileName = "";
        public const string AssumptionsWarning = "";

        public static IReadOnlyList<string> Exclusions { get; } = new ReadOnlyCollection<string>(new string[0]);

        public static string Create(
            StructuralModel model,
            RamBuildPlan plan,
            string adapterVersion,
            string packageFolder,
            string analysisInputFingerprint,
            string loadFileName,
            string loadFileSha256,
            IEnumerable<RamManifestFile> files) =>
            throw new NotImplementedException();
    }
}
