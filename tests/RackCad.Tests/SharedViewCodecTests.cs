using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using RackCad.Application.Systems.Shared;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    public sealed class SharedViewCodecTests
    {
        [Fact]
        public void CT04V2_DecodeAndCanonicalEncodeMatchEveryCharacterizedRow()
        {
            var matrix = LoadMatrix();

            foreach (var row in matrix.DecodeRows)
            {
                var decoded = RackViewCodec.Decode(row.Kind, row.ViewInput, row.Section);

                Assert.Equal(row.Disposition, decoded.Disposition.ToString());
                Assert.Equal(row.CoercionCode, decoded.CoercionCode);
                Assert.Equal(row.Kind, decoded.OriginalSyntax.Kind);
                Assert.Equal(row.ViewInput, decoded.OriginalSyntax.View);
                Assert.Equal(row.Section, decoded.OriginalSyntax.Section);

                if (row.SemanticAddress == null)
                {
                    Assert.False(decoded.HasAddress);
                    continue;
                }

                Assert.True(decoded.HasAddress);
                Assert.Equal(row.SemanticAddress, decoded.Address.ToString());
                Assert.Equal(row.CanonicalEncoding, RackViewCodec.Encode(decoded.SystemKind, decoded.Address).ToString());
            }
        }

        [Fact]
        public void CT04V2_KindTokensAreTotalAndCaseOnlyIsCanonicalizable()
        {
            foreach (var row in LoadMatrix().KindRows)
            {
                var decoded = RackViewCodec.DecodeKind(row.KindInput);
                Assert.Equal(row.Disposition, decoded.Disposition.ToString());
                Assert.Equal(row.KindInput, decoded.OriginalKind);
            }
        }

        [Fact]
        public void AvailabilityKeepsSyntaxAndPhysicalExistenceSeparate()
        {
            var selective = RackViewCodec.Decode("selective", "frontal", 999);
            Assert.Equal(RackViewSyntaxDisposition.Canonical, selective.Disposition);
            Assert.Equal(
                RackViewAvailabilityStatus.VariantNotPresent,
                RackViewAvailability.Evaluate(selective, new SelectiveViewAvailabilityFacts(2, new[] { 0, 1 })).Status);

            var dynamic = RackViewCodec.Decode("dynamic", "lateral", 999);
            Assert.Equal(
                RackViewAvailabilityStatus.VariantNotPresent,
                RackViewAvailability.Evaluate(dynamic, new DynamicViewAvailabilityFacts(new[] { 0, 1 })).Status);

            var cantilever = RackViewCodec.Decode("cantilever", "lateral", 999);
            Assert.Equal(
                RackViewAvailabilityStatus.VariantNotPresent,
                RackViewAvailability.Evaluate(cantilever, new CantileverViewAvailabilityFacts(3)).Status);

            var sideB = RackViewCodec.Decode("pushback", "frontal", 2);
            Assert.Equal(
                RackViewAvailabilityStatus.VariantNotPresent,
                RackViewAvailability.Evaluate(
                    sideB, new PushBackViewAvailabilityFacts(Array.Empty<int>(), isComposite: false)).Status);
            Assert.Equal(
                RackViewAvailabilityStatus.Available,
                RackViewAvailability.Evaluate(
                    sideB, new PushBackViewAvailabilityFacts(Array.Empty<int>(), isComposite: true)).Status);
        }

        [Fact]
        public void AvailabilityFailsClosedForWrongKindAndInvalidDecode()
        {
            var decoded = RackViewCodec.Decode("selective", "frontal", 0);
            var wrong = RackViewAvailability.Evaluate(
                decoded,
                new CamaViewAvailabilityFacts());
            Assert.Equal(RackViewAvailabilityStatus.SystemDoesNotSupportKind, wrong.Status);

            var invalid = RackViewCodec.Decode("pushback", "future-view", 0);
            var unavailable = RackViewAvailability.Evaluate(
                invalid,
                new PushBackViewAvailabilityFacts(Array.Empty<int>(), isComposite: false));
            Assert.Equal(RackViewAvailabilityStatus.Unavailable, unavailable.Status);
        }

        private static MatrixDocument LoadMatrix()
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory != null && !File.Exists(Path.Combine(directory.FullName, "RackCad.sln")))
            {
                directory = directory.Parent;
            }

            var path = Path.Combine(
                directory?.FullName ?? throw new InvalidOperationException("Repository root not found."),
                "tests", "RackCad.Tests", "Fixtures", "I57", "ct04-v2-coercion-matrix.json");
            return JsonSerializer.Deserialize<MatrixDocument>(File.ReadAllText(path),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        private sealed class MatrixDocument
        {
            public List<KindRow> KindRows { get; set; } = new List<KindRow>();
            public List<DecodeRow> DecodeRows { get; set; } = new List<DecodeRow>();
        }

        private sealed class KindRow
        {
            public string KindInput { get; set; }
            public string Disposition { get; set; }
        }

        private sealed class DecodeRow
        {
            public string Kind { get; set; }
            public string ViewInput { get; set; }
            public int Section { get; set; }
            public string SemanticAddress { get; set; }
            public string Disposition { get; set; }
            public string CoercionCode { get; set; }
            public string CanonicalEncoding { get; set; }
        }
    }
}
