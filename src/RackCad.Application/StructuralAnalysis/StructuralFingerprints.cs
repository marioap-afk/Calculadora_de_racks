using System;
using System.Collections.Generic;

namespace RackCad.Application.StructuralAnalysis
{
    /// <summary>The fingerprints of I-65 D-12 (CD-I65-F1-10). All are SHA-256 in lowercase hexadecimal.</summary>
    public static class StructuralFingerprints
    {
        public static string Sha256Hex(byte[] bytes) => throw new NotImplementedException();

        /// <summary>Design bytes + rack id + kind + station index, computed from primitive values.</summary>
        public static string Source(byte[] designBytes, string rackId, string kind, int stationIndex) =>
            throw new NotImplementedException();

        /// <summary>The hash of the verified mapping rows actually used, in ordinal order of section id.</summary>
        public static string MappingRows(IEnumerable<StructuralMaterialRow> rows) => throw new NotImplementedException();

        /// <summary>The hash of the assumption set, in ordinal order of assumption id.</summary>
        public static string Assumptions(IEnumerable<AnalysisAssumption> assumptions) => throw new NotImplementedException();

        /// <summary>Canonical model + load-file hash + mapping-rows hash + assumptions hash + adapter version.</summary>
        public static string AnalysisInput(StructuralModel model, string adapterVersion) => throw new NotImplementedException();
    }
}
