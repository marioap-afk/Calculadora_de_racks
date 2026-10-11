using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace RackCad.Application.StructuralAnalysis
{
    /// <summary>
    /// The fingerprints of I-65 D-12 (CD-I65-F1-10). All are SHA-256 in lowercase hexadecimal.
    ///
    /// Every fingerprint hashes a LENGTH-PREFIXED composition of its inputs, so that moving a byte from one field to the
    /// next can never produce the same hash. Nothing in them depends on the culture, the clock or the machine.
    /// </summary>
    public static class StructuralFingerprints
    {
        private const string SourceHeader = "rackcad-source-fingerprint/v1";
        private const string MappingHeader = "rackcad-mapping-rows/v1";
        private const string AssumptionsHeader = "rackcad-assumptions/v1";
        private const string InputHeader = "rackcad-analysis-input-fingerprint/v1";

        public static string Sha256Hex(byte[] bytes)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException(nameof(bytes));
            }

            return Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
        }

        /// <summary>Design bytes + rack id + kind + station index, computed from primitive values.</summary>
        public static string Source(byte[] designBytes, string rackId, string kind, int stationIndex)
        {
            if (designBytes == null)
            {
                throw new ArgumentNullException(nameof(designBytes));
            }

            if (rackId == null)
            {
                throw new ArgumentNullException(nameof(rackId));
            }

            if (kind == null)
            {
                throw new ArgumentNullException(nameof(kind));
            }

            return Compose(writer =>
            {
                WriteText(writer, SourceHeader);
                WriteBytes(writer, designBytes);
                WriteText(writer, rackId);
                WriteText(writer, kind);
                writer.Write(stationIndex);
            });
        }

        /// <summary>The hash of the verified mapping rows actually used, in ordinal order of section id.</summary>
        public static string MappingRows(IEnumerable<StructuralMaterialRow> rows)
        {
            var ordered = (rows ?? Enumerable.Empty<StructuralMaterialRow>())
                .OrderBy(row => row.SectionId.Value, StringComparer.Ordinal)
                .ToList();

            return Compose(writer =>
            {
                WriteText(writer, MappingHeader);
                writer.Write(ordered.Count);

                foreach (var row in ordered)
                {
                    WriteText(writer, row.CanonicalText);
                }
            });
        }

        /// <summary>The hash of the assumption set, in ordinal order of assumption id.</summary>
        public static string Assumptions(IEnumerable<AnalysisAssumption> assumptions)
        {
            var ordered = (assumptions ?? Enumerable.Empty<AnalysisAssumption>())
                .OrderBy(assumption => assumption.Id, StringComparer.Ordinal)
                .ToList();

            return Compose(writer =>
            {
                WriteText(writer, AssumptionsHeader);
                writer.Write(ordered.Count);

                foreach (var assumption in ordered)
                {
                    WriteText(writer, assumption.Id);
                    WriteText(writer, assumption.Statement);
                }
            });
        }

        /// <summary>Canonical model + load-file hash + mapping-rows hash + assumptions hash + adapter version.</summary>
        public static string AnalysisInput(StructuralModel model, string adapterVersion)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            StructuralRequire.Text(adapterVersion, nameof(adapterVersion));

            var canonical = StructuralModelCanonicalizer.ToCanonicalBytes(model);

            return Compose(writer =>
            {
                WriteText(writer, InputHeader);
                WriteBytes(writer, canonical);
                WriteText(writer, model.Metadata.LoadFileSha256);
                WriteText(writer, model.Metadata.MappingRowsSha256);
                WriteText(writer, model.Metadata.AssumptionsSha256);
                WriteText(writer, adapterVersion);
            });
        }

        private static string Compose(Action<BinaryWriter> write)
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                write(writer);
                writer.Flush();
                return Sha256Hex(stream.ToArray());
            }
        }

        private static void WriteText(BinaryWriter writer, string text) =>
            WriteBytes(writer, new UTF8Encoding(false).GetBytes(text ?? string.Empty));

        private static void WriteBytes(BinaryWriter writer, byte[] bytes)
        {
            writer.Write(bytes.Length);
            writer.Write(bytes);
        }
    }
}
