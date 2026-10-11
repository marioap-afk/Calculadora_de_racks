using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RackCad.Application.StructuralAnalysis.Providers.RamElements
{
    /// <summary>The payload of the build script (I-65 D-09 rule 2). RED skeleton: it compiles and does nothing yet.</summary>
    public static class RamBuildPayload
    {
        public const string Schema = "";

        public const string RequiredPackageVersion = "";

        public const double RelativeTolerance = 0;

        public const double PercentTolerance = 0;

        public const double AxesTolerance = 0;

        public static IReadOnlyList<string> CoreFunctions { get; } = new ReadOnlyCollection<string>(new string[0]);

        public static string Create(RamBuildPlan plan, string adapterVersion, string packageFolder, string analysisInputFingerprint) =>
            throw new NotImplementedException();
    }
}
