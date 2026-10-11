using System;
using System.IO;
using RackCad.Application.StructuralAnalysis;
using RackCad.Application.StructuralAnalysis.Providers.RamElements;

namespace RackCad.StructuralAnalysis.RamTool
{
    /// <summary>
    /// <c>dump-script</c> (D-11): renders the fixed dump script for a folder and publishes it into a NEW folder with the atomic neutral
    /// publisher. It never overwrites. One new folder per dump, so a dump is never reused.
    /// </summary>
    internal static class RamToolDumpScript
    {
        private static readonly string[] Values = { "--out", "--adapter-version", "--embed-folder" };

        public static int Run(string[] args, TextWriter output, TextWriter error)
        {
            var arguments = RamToolArguments.Parse(args, Values, new string[0]);

            if (arguments.Error != null)
            {
                error.WriteLine(arguments.Error);
                error.Write(RamToolCommands.Usage);
                return RamToolExitCodes.InvalidUsage;
            }

            var missing = arguments.Missing("--out", "--adapter-version");

            if (missing != null)
            {
                error.WriteLine("The option " + missing + " is required.");
                error.Write(RamToolCommands.Usage);
                return RamToolExitCodes.InvalidUsage;
            }

            var folder = Path.GetFullPath(arguments.Value("--out"));
            var embedded = arguments.Value("--embed-folder") ?? folder;
            var created = RamDumpScriptFactory.Create(arguments.Value("--adapter-version"), embedded);

            if (created.Outcome != StructuralExportOutcome.Valid)
            {
                error.WriteLine("The dump script cannot be created: " + RamToolFiles.Describe(created.Diagnostics));
                return RamToolExitCodes.InvalidUsage;
            }

            var written = new StructuralPackagePublisher().Write(folder, new[] { created.Script.ToPackageFile() });

            if (!written.Succeeded)
            {
                error.WriteLine("The dump script was not published: " + written.Code + " (" + written.Subject + ")");
                return RamToolExitCodes.OutputRefused;
            }

            output.WriteLine("Dump script published in '" + folder + "'; it embeds the folder '" + embedded + "'.");
            return RamToolExitCodes.Pass;
        }
    }
}
