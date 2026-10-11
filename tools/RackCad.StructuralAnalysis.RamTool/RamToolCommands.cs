using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using RackCad.Application.StructuralAnalysis;

namespace RackCad.StructuralAnalysis.RamTool
{
    /// <summary>The exit codes of the tool. Each one means one thing and is never reused.</summary>
    internal static class RamToolExitCodes
    {
        /// <summary>The command did what it was asked; for a comparison, the verdict is PASS.</summary>
        public const int Pass = 0;

        /// <summary>The comparison ran and its verdict is FAIL.</summary>
        public const int Fail = 1;

        /// <summary>The comparison was rejected (a failed or stale read-back, a failed dump): no row is approved.</summary>
        public const int Rejected = 2;

        /// <summary>Wrong usage or an invalid input.</summary>
        public const int InvalidUsage = 3;

        /// <summary>The destination exists or could not be written: nothing is ever overwritten.</summary>
        public const int OutputRefused = 4;
    }

    /// <summary>The sub-commands of the tool: dump-script, compare and inject. Each one is a class that the tests call with their own writers.</summary>
    internal static class RamToolCommands
    {
        public const string Usage =
            "RackCad structural analysis console tool (outside the product; it runs nothing in the analysis program).\n"
            + "\n"
            + "Usage:\n"
            + "  dump-script --out <new folder> --adapter-version <version> [--embed-folder <absolute normalised Windows folder>]\n"
            + "      Publishes the dump script into a NEW folder (never overwrites). The folder it embeds is --embed-folder, by default --out.\n"
            + "  compare --package <folder> --generated <dump.json> --manual <dump.json> [--report <new file>]\n"
            + "          [--generated-combinations-unread] [--manual-combinations-unread]\n"
            + "      Reads manifest.json and ram-readback.json of the package and the two dumps, prints the comparison report and, if asked,\n"
            + "      writes it to a NEW file (never overwrites).\n"
            + "  inject --package <valid package folder> --fault <kind> --out <new folder> [--embed-folder <folder>]\n"
            + "      Writes into a NEW folder a copy of the package with ONE semantic mutation, marked as a fault injection in its manifest.\n"
            + "      Kinds: unknown-section, inverted-axes, wrong-version, missing-core-function.\n"
            + "\n"
            + "Exit codes: 0 PASS or done; 1 FAIL; 2 REJECTED or STALE; 3 invalid usage or input; 4 output refused (it exists or cannot be written).\n";

        public static int Run(string[] args, TextWriter output, TextWriter error)
        {
            if (args == null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }

            if (error == null)
            {
                throw new ArgumentNullException(nameof(error));
            }

            if (args.Length == 0)
            {
                error.Write(Usage);
                return RamToolExitCodes.InvalidUsage;
            }

            var rest = args.Skip(1).ToArray();

            switch (args[0])
            {
                case "dump-script":
                    return RamToolDumpScript.Run(rest, output, error);
                case "compare":
                    return RamToolCompare.Run(rest, output, error);
                case "inject":
                    return RamToolInject.Run(rest, output, error);
                case "help":
                case "--help":
                    output.Write(Usage);
                    return RamToolExitCodes.Pass;
                default:
                    error.WriteLine("Unknown command '" + args[0] + "'.");
                    error.Write(Usage);
                    return RamToolExitCodes.InvalidUsage;
            }
        }
    }

    /// <summary>The explicit arguments of a sub-command: <c>--name value</c> pairs and <c>--flag</c> switches. Anything else is an error.</summary>
    internal sealed class RamToolArguments
    {
        private readonly Dictionary<string, string> _values = new Dictionary<string, string>(StringComparer.Ordinal);
        private readonly HashSet<string> _flags = new HashSet<string>(StringComparer.Ordinal);

        private RamToolArguments()
        {
        }

        public string Error { get; private set; }

        public static RamToolArguments Parse(string[] args, string[] valueOptions, string[] flagOptions)
        {
            var result = new RamToolArguments();

            for (var index = 0; index < args.Length; index++)
            {
                var name = args[index];

                if (flagOptions.Contains(name, StringComparer.Ordinal))
                {
                    result._flags.Add(name);
                }
                else if (valueOptions.Contains(name, StringComparer.Ordinal))
                {
                    if (index + 1 >= args.Length)
                    {
                        result.Error = "The option " + name + " needs a value.";
                        return result;
                    }

                    if (result._values.ContainsKey(name))
                    {
                        result.Error = "The option " + name + " is given twice.";
                        return result;
                    }

                    result._values.Add(name, args[++index]);
                }
                else
                {
                    result.Error = "Unknown argument '" + name + "'.";
                    return result;
                }
            }

            return result;
        }

        public string Value(string name) => _values.TryGetValue(name, out var value) ? value : null;

        public bool Flag(string name) => _flags.Contains(name);

        /// <summary>The first required option that is missing or blank, or <c>null</c>.</summary>
        public string Missing(params string[] required) => required.FirstOrDefault(name => string.IsNullOrWhiteSpace(Value(name)));
    }

    /// <summary>Reading and writing the files of the tool: strict UTF-8, and new files only.</summary>
    internal static class RamToolFiles
    {
        public static readonly UTF8Encoding Utf8 = new UTF8Encoding(false, true);

        /// <summary>Reads a UTF-8 text file; <c>null</c> (with the reason) when it is missing or is not valid UTF-8.</summary>
        public static string ReadText(string path, out string problem)
        {
            problem = null;

            try
            {
                if (!File.Exists(path))
                {
                    problem = "The file '" + path + "' does not exist.";
                    return null;
                }

                return Utf8.GetString(File.ReadAllBytes(path));
            }
            catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException || exception is DecoderFallbackException)
            {
                problem = "The file '" + path + "' cannot be read: " + exception.Message;
                return null;
            }
        }

        /// <summary>Reads the bytes of a file; <c>null</c> (with the reason) when it is missing or cannot be read.</summary>
        public static byte[] ReadBytes(string path, out string problem)
        {
            problem = null;

            try
            {
                if (!File.Exists(path))
                {
                    problem = "The file '" + path + "' does not exist.";
                    return null;
                }

                return File.ReadAllBytes(path);
            }
            catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException)
            {
                problem = "The file '" + path + "' cannot be read: " + exception.Message;
                return null;
            }
        }

        /// <summary>Writes a NEW file; false when it exists or cannot be created. It never overwrites.</summary>
        public static bool TryCreate(string path, string text, out string problem)
        {
            problem = null;

            try
            {
                using (var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    var bytes = new UTF8Encoding(false).GetBytes(text);
                    stream.Write(bytes, 0, bytes.Length);
                }

                return true;
            }
            catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException)
            {
                problem = "The file '" + path + "' cannot be created: " + exception.Message;
                return false;
            }
        }

        public static string Describe(IEnumerable<StructuralDiagnostic> diagnostics) =>
            string.Join(", ", diagnostics.Select(d => d.Code + "(" + d.Subject + ")"));
    }
}
