using System;
using System.IO;

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

    /// <summary>What marks a package as a fault injection. The product never writes it.</summary>
    internal static class RamToolInjection
    {
        public const string Marker = "rackcadFaultInjection";
        public const string UnknownSection = "unknown-section";
        public const string InvertedAxes = "inverted-axes";
        public const string WrongVersion = "wrong-version";
        public const string MissingCoreFunction = "missing-core-function";
    }

    /// <summary>The sub-commands of the tool: dump-script, compare and inject.</summary>
    internal static class RamToolCommands
    {
        public static int Run(string[] args, TextWriter output, TextWriter error)
        {
            throw new NotImplementedException();
        }
    }
}
