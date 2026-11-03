using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using RackCad.Application.StructuralAnalysis;
using RackCad.Application.StructuralAnalysis.Providers.RamElements;

namespace RackCad.StructuralAnalysis.RamTool
{
    /// <summary>The kinds of fault the tool injects (OV-I65-05, 06 and 07) and what marks a package as an injection. The product never writes it.</summary>
    internal static class RamToolInjection
    {
        public const string Marker = "rackcadFaultInjection";
        public const string UnknownSection = "unknown-section";
        public const string InvertedAxes = "inverted-axes";
        public const string WrongVersion = "wrong-version";
        public const string MissingCoreFunction = "missing-core-function";

        public static IReadOnlyList<string> Kinds { get; } = new[] { UnknownSection, InvertedAxes, WrongVersion, MissingCoreFunction };

        /// <summary>A name of a function of the core that does not exist.</summary>
        public const string NonexistentFunction = "rackcad_nonexistent_function";

        /// <summary>A version of the package of the analysis program that is not the one required.</summary>
        public const string AlteredVersion = "999.0.0";

        /// <summary>The tail of a section name that does not exist, written after a space so that the format is kept.</summary>
        public const string NonexistentSectionTail = "INEXISTENTE-99";
    }

    /// <summary>
    /// <c>inject</c> (D-11, OV-I65-05..07): from a VALID package, writes into a NEW folder a package with exactly ONE semantic mutation of the
    /// payload of the build script, the script rendered again with the new embedded folder, the load file unchanged and a manifest marked as a
    /// fault injection (kind, mutation and the SHA-256 of the manifest of the source package). The source folder is not touched.
    /// </summary>
    internal static class RamToolInject
    {
        private static readonly string[] Values = { "--package", "--fault", "--out", "--embed-folder" };

        public static int Run(string[] args, TextWriter output, TextWriter error)
        {
            var arguments = RamToolArguments.Parse(args, Values, new string[0]);

            if (arguments.Error != null)
            {
                error.WriteLine(arguments.Error);
                error.Write(RamToolCommands.Usage);
                return RamToolExitCodes.InvalidUsage;
            }

            var missing = arguments.Missing("--package", "--fault", "--out");

            if (missing != null)
            {
                error.WriteLine("The option " + missing + " is required.");
                error.Write(RamToolCommands.Usage);
                return RamToolExitCodes.InvalidUsage;
            }

            var fault = arguments.Value("--fault");

            if (!RamToolInjection.Kinds.Contains(fault, StringComparer.Ordinal))
            {
                error.WriteLine("Unknown fault '" + fault + "'. Kinds: " + string.Join(", ", RamToolInjection.Kinds) + ".");
                return RamToolExitCodes.InvalidUsage;
            }

            var target = Path.GetFullPath(arguments.Value("--out"));
            var embedded = arguments.Value("--embed-folder") ?? target;

            if (!RamBuildPackageFactory.IsAbsoluteNormalisedFolder(embedded))
            {
                error.WriteLine("The embedded folder '" + embedded + "' is not an absolute normalised Windows folder.");
                return RamToolExitCodes.InvalidUsage;
            }

            var source = Path.GetFullPath(arguments.Value("--package"));
            var manifestBytes = RamToolFiles.ReadBytes(Path.Combine(source, RamBuildPackage.ManifestFileName), out var problem);
            var scriptBytes = manifestBytes == null ? null : RamToolFiles.ReadBytes(Path.Combine(source, RamBuildPackage.ScriptFileName), out problem);
            var loadBytes = scriptBytes == null ? null : RamToolFiles.ReadBytes(Path.Combine(source, RamBuildPackage.LoadFileName), out problem);

            if (loadBytes == null)
            {
                error.WriteLine(problem);
                return RamToolExitCodes.InvalidUsage;
            }

            string manifestText;
            string scriptText;

            try
            {
                manifestText = RamToolFiles.Utf8.GetString(manifestBytes);
                scriptText = RamToolFiles.Utf8.GetString(scriptBytes);
            }
            catch (DecoderFallbackException exception)
            {
                error.WriteLine("The package is not valid UTF-8: " + exception.Message);
                return RamToolExitCodes.InvalidUsage;
            }

            var manifest = RamBuildManifestReader.Read(manifestText);

            if (manifest.Outcome != StructuralExportOutcome.Valid)
            {
                error.WriteLine("The source package is not valid (its manifest): " + RamToolFiles.Describe(manifest.Diagnostics));
                return RamToolExitCodes.InvalidUsage;
            }

            JsonObject payload;
            JsonObject manifestNode;

            try
            {
                payload = JsonNode.Parse(ExtractPayload(scriptText)) as JsonObject;
                manifestNode = JsonNode.Parse(manifestText) as JsonObject;
            }
            catch (Exception exception) when (exception is JsonException || exception is FormatException)
            {
                error.WriteLine("The source package cannot be read: " + exception.Message);
                return RamToolExitCodes.InvalidUsage;
            }

            if (payload == null || manifestNode == null || !string.Equals((string)payload["schema"], RamBuildPayload.Schema, StringComparison.Ordinal))
            {
                error.WriteLine("The script of the source package does not carry a build payload.");
                return RamToolExitCodes.InvalidUsage;
            }

            string mutation;

            try
            {
                mutation = Mutate(payload, fault);
            }
            catch (Exception exception) when (exception is InvalidOperationException || exception is NullReferenceException || exception is ArgumentOutOfRangeException || exception is FormatException)
            {
                error.WriteLine("The payload of the source package has not the expected shape: " + exception.Message);
                return RamToolExitCodes.InvalidUsage;
            }

            payload["packageFolder"] = embedded;

            var newScriptBytes = new UTF8Encoding(false).GetBytes(RamBuildScriptTemplate.Render(Compact(payload)));
            MarkManifest(manifestNode, embedded, newScriptBytes, fault, mutation, Sha256(manifestBytes));

            var files = new[]
            {
                new StructuralPackageFile(RamBuildPackage.ManifestFileName, new UTF8Encoding(false).GetBytes(Indented(manifestNode))),
                new StructuralPackageFile(RamBuildPackage.ScriptFileName, newScriptBytes),
                new StructuralPackageFile(RamBuildPackage.LoadFileName, loadBytes),
            };

            var written = new StructuralPackagePublisher().Write(target, files);

            if (!written.Succeeded)
            {
                error.WriteLine("The injected package was not published: " + written.Code + " (" + written.Subject + ")");
                return RamToolExitCodes.OutputRefused;
            }

            output.WriteLine("Injected package published in '" + target + "' (fault: " + fault + "; mutation: " + mutation + "; embedded folder: " + embedded + ").");
            return RamToolExitCodes.Pass;
        }

        /// <summary>The payload of a rendered script: the string literal of its payload line, with the escape of the template undone.</summary>
        private static string ExtractPayload(string script)
        {
            var lines = script.Split('\n').Where(line => line.StartsWith(RamBuildScriptTemplate.PayloadLinePrefix, StringComparison.Ordinal)).ToList();

            if (lines.Count != 1 || !lines[0].EndsWith("\"", StringComparison.Ordinal))
            {
                throw new FormatException("The script has not exactly one payload line.");
            }

            var literal = lines[0].Substring(RamBuildScriptTemplate.PayloadLinePrefix.Length, lines[0].Length - RamBuildScriptTemplate.PayloadLinePrefix.Length - 1);
            var text = new StringBuilder(literal.Length);

            for (var index = 0; index < literal.Length; index++)
            {
                if (literal[index] == '\\')
                {
                    index++;

                    if (index >= literal.Length || (literal[index] != '\\' && literal[index] != '"'))
                    {
                        throw new FormatException("The payload literal has an unknown escape.");
                    }
                }
                else if (literal[index] == '"')
                {
                    throw new FormatException("The payload literal has an unescaped quote.");
                }

                text.Append(literal[index]);
            }

            return text.ToString();
        }

        /// <summary>Applies the ONE mutation and returns its description.</summary>
        private static string Mutate(JsonObject payload, string fault)
        {
            switch (fault)
            {
                case RamToolInjection.UnknownSection:
                {
                    var member = (JsonObject)((JsonArray)payload["members"])[0];
                    var before = (string)member["section"];
                    var space = before.LastIndexOf(' ');
                    var after = (space > 0 ? before.Substring(0, space) : before) + " " + RamToolInjection.NonexistentSectionTail;
                    member["section"] = after;
                    return "members[0].section: '" + before + "' -> '" + after + "'";
                }

                case RamToolInjection.InvertedAxes:
                {
                    var member = (JsonObject)((JsonArray)payload["members"])[0];
                    var axis = (JsonArray)member["l2"];
                    var values = axis.Select(node => node.GetValue<double>()).ToList();

                    for (var index = 0; index < values.Count; index++)
                    {
                        axis[index] = JsonValue.Create(0.0 - values[index]);
                    }

                    return "members[0].l2: the expected local axis 2 of the first member is inverted";
                }

                case RamToolInjection.WrongVersion:
                {
                    var capabilities = (JsonObject)payload["capabilities"];
                    var before = (string)capabilities["version"];
                    capabilities["version"] = RamToolInjection.AlteredVersion;
                    return "capabilities.version: '" + before + "' -> '" + RamToolInjection.AlteredVersion + "'";
                }

                default:
                {
                    var capabilities = (JsonObject)payload["capabilities"];
                    var functions = ((JsonArray)capabilities["coreFunctions"]).Select(node => (string)node).ToList();
                    functions.Add(RamToolInjection.NonexistentFunction);
                    functions.Sort(StringComparer.Ordinal);
                    capabilities["coreFunctions"] = new JsonArray(functions.Select(name => (JsonNode)JsonValue.Create(name)).ToArray());
                    return "capabilities.coreFunctions: the nonexistent function '" + RamToolInjection.NonexistentFunction + "' is required";
                }
            }
        }

        private static void MarkManifest(JsonObject manifest, string embedded, byte[] scriptBytes, string fault, string mutation, string sourceSha256)
        {
            manifest["packageFolder"] = embedded;

            foreach (var node in (JsonArray)manifest["files"])
            {
                var entry = (JsonObject)node;

                if (string.Equals((string)entry["name"], RamBuildPackage.ScriptFileName, StringComparison.Ordinal))
                {
                    entry["sha256"] = Sha256(scriptBytes);
                }
            }

            manifest[RamToolInjection.Marker] = new JsonObject
            {
                ["kind"] = fault,
                ["mutation"] = mutation,
                ["sourcePackageSha256"] = sourceSha256,
            };
        }

        /// <summary>Compact JSON in plain ASCII on one line, as the script payload must be.</summary>
        private static string Compact(JsonNode node)
        {
            var text = node.ToJsonString(new JsonSerializerOptions { WriteIndented = false, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
            var ascii = new StringBuilder(text.Length);

            foreach (var character in text)
            {
                if (character > 0x7E)
                {
                    ascii.Append("\\u").Append(((int)character).ToString("x4", System.Globalization.CultureInfo.InvariantCulture));
                }
                else
                {
                    ascii.Append(character);
                }
            }

            return ascii.ToString();
        }

        /// <summary>Indented JSON with LF line ends and a final line feed, like the manifest of the product.</summary>
        private static string Indented(JsonNode node)
        {
            var text = node.ToJsonString(new JsonSerializerOptions { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
            return text.Replace("\r\n", "\n") + "\n";
        }

        private static string Sha256(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    }
}
