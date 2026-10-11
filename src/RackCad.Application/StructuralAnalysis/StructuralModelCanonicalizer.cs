using System;

namespace RackCad.Application.StructuralAnalysis
{
    /// <summary>
    /// The deterministic canonical form of a model (I-65 D-12, CD-I65-F1-10): ordinal order by id, invariant culture,
    /// round-trip doubles, no timestamps, UTF-8 without BOM and LF line endings.
    /// </summary>
    public static class StructuralModelCanonicalizer
    {
        public static string ToCanonicalJson(StructuralModel model) => throw new NotImplementedException();

        public static byte[] ToCanonicalBytes(StructuralModel model) => throw new NotImplementedException();
    }
}
