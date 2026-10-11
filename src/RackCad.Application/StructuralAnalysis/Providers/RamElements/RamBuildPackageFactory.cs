using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace RackCad.Application.StructuralAnalysis.Providers.RamElements
{
    /// <summary>The stable diagnostic codes of the package build. A code is never renamed or reused.</summary>
    public static class RamPackageDiagnosticCodes
    {
        public const string AdapterVersionInvalid = "RAM-PACKAGE-ADAPTER-VERSION-INVALID";
        public const string FolderInvalid = "RAM-PACKAGE-FOLDER-INVALID";
        public const string LoadsMissing = "RAM-PACKAGE-LOADS-MISSING";
        public const string LoadsHashMismatch = "RAM-PACKAGE-LOADS-HASH-MISMATCH";
        public const string MappingStale = "RAM-PACKAGE-MAPPING-STALE";
        public const string NotRepresentable = "RAM-PACKAGE-NOT-REPRESENTABLE";
    }

    /// <summary>The inputs of <see cref="RamBuildPackageFactory.CreateRamBuildPackage(StructuralModel, RamBuildPackageOptions)"/>.</summary>
    public sealed class RamBuildPackageOptions
    {
        private readonly byte[] _loadFileBytes;

        /// <param name="table">The provider table the model was built with.</param>
        /// <param name="adapterVersion">The version of the adapter; it is part of the fingerprint of the input.</param>
        /// <param name="packageFolder">The ABSOLUTE WINDOWS path of the folder the package will be written to (the script runs there).</param>
        /// <param name="loadFileBytes">The bytes of the load file the model was built from; they travel unchanged in the package.</param>
        public RamBuildPackageOptions(RamSectionMaterialTable table, string adapterVersion, string packageFolder, byte[] loadFileBytes)
        {
            Table = table ?? throw new ArgumentNullException(nameof(table));
            AdapterVersion = adapterVersion;
            PackageFolder = packageFolder;
            _loadFileBytes = loadFileBytes == null ? null : (byte[])loadFileBytes.Clone();
        }

        public RamSectionMaterialTable Table { get; }

        public string AdapterVersion { get; }

        public string PackageFolder { get; }

        /// <summary>A copy of the bytes, or <c>null</c> when none were given.</summary>
        public byte[] GetLoadFileBytes() => _loadFileBytes == null ? null : (byte[])_loadFileBytes.Clone();
    }

    /// <summary>The content of a package: its three files, in the order manifest, script, load file.</summary>
    public sealed class RamBuildPackage
    {
        public const string ManifestFileName = "manifest.json";
        public const string ScriptFileName = "rackcad-ram-build.py";
        public const string LoadFileName = "structural-loads.json";

        internal RamBuildPackage(IReadOnlyList<StructuralPackageFile> files, string analysisInputFingerprint)
        {
            Files = files;
            AnalysisInputFingerprint = analysisInputFingerprint;
        }

        public IReadOnlyList<StructuralPackageFile> Files { get; }

        public string AnalysisInputFingerprint { get; }
    }

    /// <summary>The result of building the package: the content when, and only when, the outcome is Valid. Never a partial package.</summary>
    public sealed class RamBuildPackageResult
    {
        internal RamBuildPackageResult(StructuralExportOutcome outcome, IReadOnlyList<StructuralDiagnostic> diagnostics, RamBuildPackage package)
        {
            Outcome = outcome;
            Diagnostics = diagnostics;
            Package = package;
        }

        public StructuralExportOutcome Outcome { get; }

        public IReadOnlyList<StructuralDiagnostic> Diagnostics { get; }

        public RamBuildPackage Package { get; }
    }

    /// <summary>
    /// <c>CreateRamBuildPackage(model, options)</c> (I-65 D-08, D-13): the strict validation first, then the content of the package.
    /// It returns bytes and never touches the disk (D-01): writing is the job of the caller. Only a Valid model, a table with a
    /// verified row for every section the model uses, a translation without blocking diagnostics, an absolute and normalised
    /// folder, the bytes of the load file whose SHA-256 is the one of the model and a non-blank version of the adapter give content.
    /// Anything else is a blocking diagnostic under the precedence of CD-I65-F1-04 and there is NO content.
    /// </summary>
    public static class RamBuildPackageFactory
    {
        public static RamBuildPackageResult CreateRamBuildPackage(StructuralModelResult result, RamBuildPackageOptions options)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (result.Outcome == StructuralExportOutcome.Valid && result.Model != null)
            {
                return CreateRamBuildPackage(result.Model, options);
            }

            var diagnostics = new List<StructuralDiagnostic>();
            CheckOptions(options, diagnostics);
            diagnostics.AddRange(RamElementsTranslator.Translate(result, options.Table).Diagnostics);

            return Refuse(diagnostics);
        }

        public static RamBuildPackageResult CreateRamBuildPackage(StructuralModel model, RamBuildPackageOptions options)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            var diagnostics = new List<StructuralDiagnostic>();
            CheckOptions(options, diagnostics);

            var loadBytes = options.GetLoadFileBytes();

            if (loadBytes != null && loadBytes.Length > 0
                && !string.Equals(StructuralFingerprints.Sha256Hex(loadBytes), model.Metadata.LoadFileSha256, StringComparison.Ordinal))
            {
                diagnostics.Add(StructuralDiagnostic.Blocking(RamPackageDiagnosticCodes.LoadsHashMismatch, StructuralExportOutcome.Invalid, "loads"));
            }

            var translation = RamElementsTranslator.Translate(model, options.Table);
            diagnostics.AddRange(translation.Diagnostics);

            var plan = translation.Plan;

            if (plan != null)
            {
                CheckPlan(model, plan, diagnostics);
            }

            if (StructuralModelResult.Aggregate(diagnostics) != StructuralExportOutcome.Valid)
            {
                return Refuse(diagnostics);
            }

            return Build(model, plan, options, loadBytes, diagnostics);
        }

        /// <summary>
        /// True when <paramref name="folder"/> is an absolute and normalised WINDOWS path (the script runs inside the analysis
        /// program, on Windows): a drive letter and a backslash, or the UNC prefix of two backslashes with a server and a share; then
        /// at least one name. No forward slash, no <c>.</c> or <c>..</c> segment, no empty segment, no trailing separator, and no
        /// control or reserved character in a name. The rule is explicit: it does not depend on the system the code runs on.
        /// </summary>
        public static bool IsAbsoluteNormalisedFolder(string folder)
        {
            if (string.IsNullOrEmpty(folder))
            {
                return false;
            }

            string rest;
            var minimum = 1;

            if (folder.StartsWith("\\\\", StringComparison.Ordinal))
            {
                rest = folder.Substring(2);
                minimum = 3;
            }
            else if (folder.Length >= 3 && IsDriveLetter(folder[0]) && folder[1] == ':' && folder[2] == '\\')
            {
                rest = folder.Substring(3);
            }
            else
            {
                return false;
            }

            var segments = rest.Split('\\');

            if (segments.Length < minimum)
            {
                return false;
            }

            return segments.All(IsNormalisedSegment);
        }

        // ---------------------------------------------------------------- validation

        private static void CheckOptions(RamBuildPackageOptions options, List<StructuralDiagnostic> diagnostics)
        {
            if (string.IsNullOrWhiteSpace(options.AdapterVersion))
            {
                diagnostics.Add(StructuralDiagnostic.Blocking(RamPackageDiagnosticCodes.AdapterVersionInvalid, StructuralExportOutcome.Invalid, "adapterVersion"));
            }

            if (!IsAbsoluteNormalisedFolder(options.PackageFolder))
            {
                diagnostics.Add(StructuralDiagnostic.Blocking(RamPackageDiagnosticCodes.FolderInvalid, StructuralExportOutcome.Invalid, "packageFolder"));
            }

            var bytes = options.GetLoadFileBytes();

            if (bytes == null || bytes.Length == 0)
            {
                diagnostics.Add(StructuralDiagnostic.Blocking(RamPackageDiagnosticCodes.LoadsMissing, StructuralExportOutcome.Invalid, "loads"));
            }
        }

        /// <summary>The checks that need the plan: the rows must be the ones the model was built with, and every name must be usable.</summary>
        private static void CheckPlan(StructuralModel model, RamBuildPlan plan, List<StructuralDiagnostic> diagnostics)
        {
            var mapping = StructuralFingerprints.MappingRows(plan.UsedRows.Select(row => row.ToNeutralRow()));

            if (!string.Equals(mapping, model.Metadata.MappingRowsSha256, StringComparison.Ordinal))
            {
                diagnostics.Add(StructuralDiagnostic.Blocking(RamPackageDiagnosticCodes.MappingStale, StructuralExportOutcome.Invalid, "mapping"));
            }

            foreach (var member in plan.Members)
            {
                if (string.IsNullOrWhiteSpace(member.SectionName) || string.IsNullOrWhiteSpace(member.MaterialName))
                {
                    diagnostics.Add(StructuralDiagnostic.Blocking(RamPackageDiagnosticCodes.NotRepresentable, StructuralExportOutcome.Invalid, member.Id));
                }
            }
        }

        private static RamBuildPackageResult Refuse(List<StructuralDiagnostic> diagnostics)
        {
            return new RamBuildPackageResult(
                StructuralModelResult.Aggregate(diagnostics),
                new ReadOnlyCollection<StructuralDiagnostic>(diagnostics),
                null);
        }

        // ---------------------------------------------------------------- the content

        private static RamBuildPackageResult Build(
            StructuralModel model,
            RamBuildPlan plan,
            RamBuildPackageOptions options,
            byte[] loadBytes,
            List<StructuralDiagnostic> diagnostics)
        {
            var fingerprint = StructuralFingerprints.AnalysisInput(model, options.AdapterVersion);
            var encoding = new UTF8Encoding(false);

            var payload = RamBuildPayload.Create(plan, options.AdapterVersion, options.PackageFolder, fingerprint);
            var scriptBytes = encoding.GetBytes(RamBuildScriptTemplate.Render(payload));

            var manifest = RamBuildManifest.Create(
                model,
                plan,
                options.AdapterVersion,
                options.PackageFolder,
                fingerprint,
                RamBuildPackage.LoadFileName,
                StructuralFingerprints.Sha256Hex(loadBytes),
                new[]
                {
                    new RamManifestFile(RamBuildPackage.ScriptFileName, StructuralFingerprints.Sha256Hex(scriptBytes)),
                    new RamManifestFile(RamBuildPackage.LoadFileName, StructuralFingerprints.Sha256Hex(loadBytes)),
                });

            var files = new ReadOnlyCollection<StructuralPackageFile>(new List<StructuralPackageFile>
            {
                new StructuralPackageFile(RamBuildPackage.ManifestFileName, encoding.GetBytes(manifest)),
                new StructuralPackageFile(RamBuildPackage.ScriptFileName, scriptBytes),
                new StructuralPackageFile(RamBuildPackage.LoadFileName, loadBytes),
            });

            return new RamBuildPackageResult(
                StructuralExportOutcome.Valid,
                new ReadOnlyCollection<StructuralDiagnostic>(diagnostics),
                new RamBuildPackage(files, fingerprint));
        }

        // ---------------------------------------------------------------- the folder rule

        private static bool IsDriveLetter(char character) =>
            (character >= 'A' && character <= 'Z') || (character >= 'a' && character <= 'z');

        private static bool IsNormalisedSegment(string segment)
        {
            if (segment.Length == 0 || segment == "." || segment == "..")
            {
                return false;
            }

            return segment.All(character => character >= 0x20 && "/:<>\"|?*".IndexOf(character) < 0);
        }
    }
}
