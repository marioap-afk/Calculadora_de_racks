using System;
using System.Collections.Generic;

namespace RackCad.Application.StructuralAnalysis
{
    public enum StructuralLoadSide
    {
        PositiveY = 1,
        NegativeY = 2,
    }

    /// <summary>One load of the file, with its magnitude already converted to pounds-force.</summary>
    public sealed class StructuralLoadEntry
    {
        public StructuralLoadEntry(string id, int level, StructuralLoadSide side, double positionFraction, StructuralForce magnitude)
        {
            throw new NotImplementedException();
        }

        public string Id => throw new NotImplementedException();

        public int Level => throw new NotImplementedException();

        public StructuralLoadSide Side => throw new NotImplementedException();

        /// <summary>The fraction, in [0, 1], of the physical arm measured from its start on the column side.</summary>
        public double PositionFraction => throw new NotImplementedException();

        public StructuralForce Magnitude => throw new NotImplementedException();
    }

    public sealed class StructuralLoadFile
    {
        public StructuralLoadFile(string sha256, IEnumerable<StructuralLoadEntry> loads)
        {
            throw new NotImplementedException();
        }

        /// <summary>SHA-256, in lowercase hexadecimal, of the exact bytes of the file.</summary>
        public string Sha256 => throw new NotImplementedException();

        public IReadOnlyList<StructuralLoadEntry> Loads => throw new NotImplementedException();
    }

    public sealed class StructuralLoadFileReadResult
    {
        private StructuralLoadFileReadResult()
        {
        }

        public StructuralExportOutcome Outcome => throw new NotImplementedException();

        public IReadOnlyList<StructuralDiagnostic> Diagnostics => throw new NotImplementedException();

        /// <summary>The parsed file when, and only when, the outcome is Valid.</summary>
        public StructuralLoadFile LoadFile => throw new NotImplementedException();
    }

    /// <summary>
    /// The strict reader of <c>rackcad-structural-loads/v1</c> (I-65 D-10b, CD-I65-F1-05). A number without a unit is never
    /// accepted.
    /// </summary>
    public static class StructuralLoadFileReader
    {
        public const string SchemaId = "rackcad-structural-loads/v1";

        public static StructuralLoadFileReadResult Read(byte[] bytes) => throw new NotImplementedException();
    }
}
